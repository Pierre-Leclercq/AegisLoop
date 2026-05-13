using System.Text.Json;
using AegisLoop.Application.Interfaces;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Services;

public sealed class RawItemNormalizationService : INormalizationService
{
    public Task<NormalizationResult> NormalizeAsync(RawItem rawItem)
    {
        if (rawItem.ContentType != RawContentType.Json)
        {
            return Task.FromResult(new NormalizationResult(false, null, "Seuls les RawItem JSON RSS/GDELT sont normalisés en V1."));
        }

        try
        {
            using var document = JsonDocument.Parse(rawItem.RawContent);
            var root = document.RootElement;

            var source = GetString(root, "source") ?? "rss";
            var title = GetString(root, "title");
            var summary = GetString(root, "summary") ?? GetString(root, "content") ?? string.Empty;
            var link = GetString(root, "link") ?? GetString(root, "url") ?? rawItem.SourceUrl;
            var language = GetString(root, "language");
            var feedTitle = GetString(root, "feedTitle");
            var domain = GetString(root, "domain");
            var sourceCountry = GetString(root, "sourceCountry");
            var query = GetString(root, "query");

            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(summary))
            {
                return Task.FromResult(new NormalizationResult(false, null, "RawItem sans titre ni contenu exploitable."));
            }

            var observedAt = rawItem.PublishedAt ?? rawItem.CollectedAt;
            var normalizedTitle = string.IsNullOrWhiteSpace(title) ? $"Observation {source.ToUpperInvariant()} sans titre" : title.Trim();
            var normalizedContent = string.IsNullOrWhiteSpace(summary) ? normalizedTitle : summary.Trim();
            var isGdelt = string.Equals(source, "gdelt", StringComparison.OrdinalIgnoreCase);

            var observation = new Observation
            {
                RawItemId = rawItem.Id,
                Title = normalizedTitle,
                Content = normalizedContent,
                ClaimText = normalizedContent,
                ClaimConfidence = isGdelt ? 0.6 : 0.5,
                Type = isGdelt ? ObservationType.GeospatialMetadata : ObservationType.Article,
                Status = ObservationStatus.New,
                ObservedAt = observedAt,
                SourceConnectorId = rawItem.ConnectorId,
                SourceUrl = link,
                SourceReliability = isGdelt ? 0.45 : 0.35,
                Language = language,
                MetadataJson = JsonSerializer.Serialize(new Dictionary<string, string?>
                {
                    ["source"] = source.ToLowerInvariant(),
                    ["feedTitle"] = feedTitle,
                    ["domain"] = domain,
                    ["sourceCountry"] = sourceCountry,
                    ["query"] = query,
                    ["sourceHash"] = rawItem.SourceHash
                })
            };

            return Task.FromResult(new NormalizationResult(true, observation, null));
        }
        catch (JsonException ex)
        {
            return Task.FromResult(new NormalizationResult(false, null, $"RawContent JSON invalide: {ex.Message}"));
        }
    }

    private static string? GetString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }
}
