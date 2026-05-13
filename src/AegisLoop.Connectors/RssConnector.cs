using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Domain.Interfaces;

namespace AegisLoop.Connectors;

/// <summary>
/// Connecteur RSS/Atom V1 — collecte HTTP simple et lawful, sans scraping agressif.
/// </summary>
public class RssConnector : ISourceConnector
{
    private static readonly XName RssItem = "item";
    private static readonly XName AtomEntry = XName.Get("entry", "http://www.w3.org/2005/Atom");
    private readonly HttpClient _httpClient;

    public RssConnector(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ConnectorType => "Rss";

    public Task<ValidationResult> ValidateConfigAsync(string configJson)
    {
        var config = ParseConfig(configJson);
        if (config.ErrorMessage is not null)
        {
            return Task.FromResult(new ValidationResult(false, config.ErrorMessage));
        }

        if (config.FeedUri is null)
        {
            return Task.FromResult(new ValidationResult(false, "Le champ feedUrl est obligatoire."));
        }

        if (config.FeedUri.Scheme != Uri.UriSchemeHttp && config.FeedUri.Scheme != Uri.UriSchemeHttps)
        {
            return Task.FromResult(new ValidationResult(false, "feedUrl doit utiliser HTTP ou HTTPS."));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public async Task<IngestionResult> CollectAsync(string configJson, DateTime? since)
    {
        var validation = await ValidateConfigAsync(configJson);
        if (!validation.IsValid)
        {
            return new IngestionResult(false, 0, [], [validation.ErrorMessage ?? "Configuration RSS invalide."]);
        }

        var config = ParseConfig(configJson);
        var errors = new List<string>();

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.TimeoutSeconds));
            using var request = new HttpRequestMessage(HttpMethod.Get, config.FeedUri);
            request.Headers.UserAgent.ParseAdd("AegisLoop/1.0 (+local analyst workbench)");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return new IngestionResult(false, 0, [], [$"HTTP {(int)response.StatusCode} {response.ReasonPhrase}".Trim()]);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            var document = await XDocument.LoadAsync(stream, LoadOptions.None, timeoutCts.Token);
            var items = ParseFeed(document, config, since).ToList();

            return new IngestionResult(true, items.Count, items, errors);
        }
        catch (OperationCanceledException)
        {
            return new IngestionResult(false, 0, [], [$"Timeout RSS après {config.TimeoutSeconds} secondes."]);
        }
        catch (HttpRequestException ex)
        {
            return new IngestionResult(false, 0, [], [$"Erreur HTTP RSS: {ex.Message}"]);
        }
        catch (Exception ex) when (ex is System.Xml.XmlException or InvalidOperationException)
        {
            return new IngestionResult(false, 0, [], [$"Flux RSS/Atom invalide: {ex.Message}"]);
        }
    }

    public async Task<HealthCheckResult> HealthCheckAsync(string configJson)
    {
        var result = await CollectAsync(configJson, null);
        return result.Success
            ? new HealthCheckResult(true)
            : new HealthCheckResult(false, string.Join("; ", result.Errors));
    }

    public static string ComputeSourceHash(string sourceUrl, string title, DateTime? publishedAt)
    {
        var normalized = string.Join("|", Normalize(sourceUrl), Normalize(title), publishedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static IEnumerable<RawItem> ParseFeed(XDocument document, RssConfig config, DateTime? since)
    {
        var rootName = document.Root?.Name.LocalName.ToLowerInvariant();
        return rootName switch
        {
            "rss" or "rdf" => ParseRss(document, config, since),
            "feed" => ParseAtom(document, config, since),
            _ => throw new InvalidOperationException("Format RSS/Atom non reconnu.")
        };
    }

    private static IEnumerable<RawItem> ParseRss(XDocument document, RssConfig config, DateTime? since)
    {
        var channel = document.Descendants("channel").FirstOrDefault();
        var feedTitle = CleanText(channel?.Element("title")?.Value);
        var language = CleanText(channel?.Element("language")?.Value);

        return document.Descendants(RssItem)
            .Select(item => ToRawItem(
                title: CleanText(item.Element("title")?.Value),
                link: CleanText(item.Element("link")?.Value) ?? CleanText(item.Elements().FirstOrDefault(e => e.Name.LocalName == "guid")?.Value),
                publishedAt: ParseDate(CleanText(item.Element("pubDate")?.Value) ?? CleanText(item.Elements().FirstOrDefault(e => e.Name.LocalName is "date" or "published" or "updated")?.Value)),
                summary: CleanText(item.Element("description")?.Value),
                content: CleanText(item.Elements().FirstOrDefault(e => e.Name.LocalName == "encoded")?.Value),
                feedTitle,
                language))
            .Where(raw => raw is not null)
            .Select(raw => raw!)
            .Where(raw => since is null || raw.PublishedAt is null || raw.PublishedAt > since.Value.ToUniversalTime())
            .Take(config.MaxItemsPerPoll);
    }

    private static IEnumerable<RawItem> ParseAtom(XDocument document, RssConfig config, DateTime? since)
    {
        XNamespace atom = "http://www.w3.org/2005/Atom";
        var feedTitle = CleanText(document.Root?.Element(atom + "title")?.Value);
        var language = document.Root?.Attribute(XNamespace.Xml + "lang")?.Value;

        return document.Descendants(AtomEntry)
            .Select(entry => ToRawItem(
                title: CleanText(entry.Element(atom + "title")?.Value),
                link: CleanText(entry.Elements(atom + "link").FirstOrDefault(e => e.Attribute("rel")?.Value is null or "alternate")?.Attribute("href")?.Value),
                publishedAt: ParseDate(CleanText(entry.Element(atom + "published")?.Value) ?? CleanText(entry.Element(atom + "updated")?.Value)),
                summary: CleanText(entry.Element(atom + "summary")?.Value),
                content: CleanText(entry.Element(atom + "content")?.Value),
                feedTitle,
                language))
            .Where(raw => raw is not null)
            .Select(raw => raw!)
            .Where(raw => since is null || raw.PublishedAt is null || raw.PublishedAt > since.Value.ToUniversalTime())
            .Take(config.MaxItemsPerPoll);
    }

    private static RawItem? ToRawItem(string? title, string? link, DateTime? publishedAt, string? summary, string? content, string? feedTitle, string? language)
    {
        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(summary) && string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var sourceUrl = string.IsNullOrWhiteSpace(link) ? string.Empty : link.Trim();
        var normalizedTitle = title ?? "Item RSS sans titre";
        var payload = new
        {
            title = normalizedTitle,
            link = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl,
            publishedAt = publishedAt?.ToString("O", CultureInfo.InvariantCulture),
            summary,
            content,
            feedTitle,
            language
        };

        return new RawItem
        {
            RawContent = JsonSerializer.Serialize(payload),
            ContentType = RawContentType.Json,
            SourceHash = ComputeSourceHash(sourceUrl, normalizedTitle, publishedAt),
            CollectedAt = DateTime.UtcNow,
            PublishedAt = publishedAt,
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl
        };
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
        {
            return dto.UtcDateTime;
        }

        return null;
    }

    private static string? CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var withoutTags = Regex.Replace(value, "<.*?>", " ", RegexOptions.Singleline, TimeSpan.FromMilliseconds(250));
        var decoded = System.Net.WebUtility.HtmlDecode(withoutTags);
        var normalized = Regex.Replace(decoded, "\\s+", " ", RegexOptions.None, TimeSpan.FromMilliseconds(250)).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static RssConfig ParseConfig(string configJson)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson);
            var root = document.RootElement;
            var feedUrl = root.TryGetProperty("feedUrl", out var feedUrlValue) ? feedUrlValue.GetString() : null;
            var maxItems = root.TryGetProperty("maxItemsPerPoll", out var maxItemsValue) && maxItemsValue.TryGetInt32(out var parsedMaxItems)
                ? parsedMaxItems
                : 25;
            var timeoutSeconds = root.TryGetProperty("timeoutSeconds", out var timeoutValue) && timeoutValue.TryGetInt32(out var parsedTimeout)
                ? parsedTimeout
                : 10;

            return new RssConfig(
                Uri.TryCreate(feedUrl, UriKind.Absolute, out var uri) ? uri : null,
                Math.Clamp(maxItems, 1, 500),
                Math.Clamp(timeoutSeconds, 1, 30),
                null);
        }
        catch (JsonException ex)
        {
            return new RssConfig(null, 25, 10, $"Configuration JSON invalide: {ex.Message}");
        }
    }

    private sealed record RssConfig(Uri? FeedUri, int MaxItemsPerPoll, int TimeoutSeconds, string? ErrorMessage);
}