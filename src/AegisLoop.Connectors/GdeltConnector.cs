using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Domain.Interfaces;

namespace AegisLoop.Connectors;

/// <summary>
/// Connecteur GDELT DOC API v2 V1 — collecte HTTP simple sur endpoint public sans clé API.
/// </summary>
public class GdeltConnector : ISourceConnector
{
    private const string DefaultApiUrl = "https://api.gdeltproject.org/api/v2/doc/doc";
    private readonly HttpClient _httpClient;

    public GdeltConnector(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ConnectorType => "Gdelt";

    public Task<ValidationResult> ValidateConfigAsync(string configJson)
    {
        var config = ParseConfig(configJson);
        if (config.ErrorMessage is not null)
        {
            return Task.FromResult(new ValidationResult(false, config.ErrorMessage));
        }

        if (string.IsNullOrWhiteSpace(config.Query))
        {
            return Task.FromResult(new ValidationResult(false, "Le champ query est obligatoire pour GDELT."));
        }

        if (config.ApiUri.Scheme != Uri.UriSchemeHttp && config.ApiUri.Scheme != Uri.UriSchemeHttps)
        {
            return Task.FromResult(new ValidationResult(false, "apiUrl GDELT doit utiliser HTTP ou HTTPS."));
        }

        return Task.FromResult(new ValidationResult(true));
    }

    public async Task<IngestionResult> CollectAsync(string configJson, DateTime? since)
    {
        var validation = await ValidateConfigAsync(configJson);
        if (!validation.IsValid)
        {
            return new IngestionResult(false, 0, [], [validation.ErrorMessage ?? "Configuration GDELT invalide."]);
        }

        var config = ParseConfig(configJson);

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(config.TimeoutSeconds));
            using var request = new HttpRequestMessage(HttpMethod.Get, BuildRequestUri(config));
            request.Headers.UserAgent.ParseAdd("AegisLoop/1.0 (+local analyst workbench)");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, timeoutCts.Token);
            if (!response.IsSuccessStatusCode)
            {
                return new IngestionResult(false, 0, [], [$"HTTP {(int)response.StatusCode} {response.ReasonPhrase}".Trim()]);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(timeoutCts.Token);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: timeoutCts.Token);
            var items = ParseArticles(document.RootElement, config, since).ToList();

            return new IngestionResult(true, items.Count, items, []);
        }
        catch (OperationCanceledException)
        {
            return new IngestionResult(false, 0, [], [$"Timeout GDELT après {config.TimeoutSeconds} secondes."]);
        }
        catch (HttpRequestException ex)
        {
            return new IngestionResult(false, 0, [], [$"Erreur HTTP GDELT: {ex.Message}"]);
        }
        catch (JsonException ex)
        {
            return new IngestionResult(false, 0, [], [$"Réponse JSON GDELT invalide: {ex.Message}"]);
        }
        catch (InvalidOperationException ex)
        {
            return new IngestionResult(false, 0, [], [$"Réponse GDELT invalide: {ex.Message}"]);
        }
    }

    public async Task<HealthCheckResult> HealthCheckAsync(string configJson)
    {
        var result = await CollectAsync(configJson, null);
        return result.Success
            ? new HealthCheckResult(true)
            : new HealthCheckResult(false, string.Join("; ", result.Errors));
    }

    public static string ComputeSourceHash(string url, string title, DateTime? seenAt)
    {
        var normalized = string.Join("|", Normalize(url), Normalize(title), seenAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture) ?? string.Empty);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static Uri BuildRequestUri(GdeltConfig config)
    {
        var builder = new UriBuilder(config.ApiUri);
        var query = new Dictionary<string, string>
        {
            ["query"] = config.Query,
            ["mode"] = "artlist",
            ["format"] = "json",
            ["sort"] = "datedesc",
            ["maxrecords"] = config.MaxItemsPerPoll.ToString(CultureInfo.InvariantCulture)
        };

        builder.Query = string.Join("&", query.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
        return builder.Uri;
    }

    private static IEnumerable<RawItem> ParseArticles(JsonElement root, GdeltConfig config, DateTime? since)
    {
        if (!root.TryGetProperty("articles", out var articles) || articles.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException("Le tableau 'articles' est absent.");
        }

        return articles.EnumerateArray()
            .Select(article => ToRawItem(article, config.Query))
            .Where(raw => raw is not null)
            .Select(raw => raw!)
            .Where(raw => since is null || raw.PublishedAt is null || raw.PublishedAt > since.Value.ToUniversalTime())
            .Take(config.MaxItemsPerPoll);
    }

    private static RawItem? ToRawItem(JsonElement article, string query)
    {
        var title = CleanText(GetString(article, "title"));
        var url = CleanText(GetString(article, "url"));
        var seenAt = ParseGdeltDate(GetString(article, "seendate"));
        var domain = CleanText(GetString(article, "domain"));
        var language = CleanText(GetString(article, "language"));
        var sourceCountry = CleanText(GetString(article, "sourcecountry"));

        if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var normalizedTitle = string.IsNullOrWhiteSpace(title) ? "Article GDELT sans titre" : title;
        var sourceUrl = string.IsNullOrWhiteSpace(url) ? string.Empty : url;
        var payload = new
        {
            source = "gdelt",
            title = normalizedTitle,
            url = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl,
            seendate = seenAt?.ToString("O", CultureInfo.InvariantCulture),
            domain,
            language,
            sourceCountry,
            query
        };

        return new RawItem
        {
            RawContent = JsonSerializer.Serialize(payload),
            ContentType = RawContentType.Json,
            SourceHash = ComputeSourceHash(sourceUrl, normalizedTitle, seenAt),
            CollectedAt = DateTime.UtcNow,
            PublishedAt = seenAt,
            SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl
        };
    }

    private static DateTime? ParseGdeltDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var formats = new[] { "yyyyMMdd'T'HHmmss'Z'", "yyyyMMddHHmmss", "yyyy-MM-dd'T'HH:mm:ss'Z'" };
        if (DateTime.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var exact))
        {
            return exact;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
        {
            return dto.UtcDateTime;
        }

        return null;
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var decoded = System.Net.WebUtility.HtmlDecode(value);
        var normalized = Regex.Replace(decoded, "\\s+", " ", RegexOptions.None, TimeSpan.FromMilliseconds(250)).Trim();
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    private static string Normalize(string value) => value.Trim().ToLowerInvariant();

    private static GdeltConfig ParseConfig(string configJson)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(configJson) ? "{}" : configJson);
            var root = document.RootElement;
            var query = root.TryGetProperty("query", out var queryValue) ? queryValue.GetString() : null;
            var apiUrl = root.TryGetProperty("apiUrl", out var apiUrlValue) ? apiUrlValue.GetString() : DefaultApiUrl;
            var maxItems = root.TryGetProperty("maxItemsPerPoll", out var maxItemsValue) && maxItemsValue.TryGetInt32(out var parsedMaxItems)
                ? parsedMaxItems
                : 25;
            var timeoutSeconds = root.TryGetProperty("timeoutSeconds", out var timeoutValue) && timeoutValue.TryGetInt32(out var parsedTimeout)
                ? parsedTimeout
                : 10;

            return new GdeltConfig(
                string.IsNullOrWhiteSpace(query) ? string.Empty : query.Trim(),
                Uri.TryCreate(apiUrl, UriKind.Absolute, out var uri) ? uri : new Uri(DefaultApiUrl),
                Math.Clamp(maxItems, 1, 250),
                Math.Clamp(timeoutSeconds, 1, 30),
                null);
        }
        catch (JsonException ex)
        {
            return new GdeltConfig(string.Empty, new Uri(DefaultApiUrl), 25, 10, $"Configuration JSON invalide: {ex.Message}");
        }
    }

    private sealed record GdeltConfig(string Query, Uri ApiUri, int MaxItemsPerPoll, int TimeoutSeconds, string? ErrorMessage);
}