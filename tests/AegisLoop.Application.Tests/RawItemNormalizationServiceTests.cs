using System.Text.Json;
using AegisLoop.Application.Services;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using Xunit;

namespace AegisLoop.Application.Tests;

public sealed class RawItemNormalizationServiceTests
{
    [Fact]
    public async Task NormalizeAsync_Maps_Rss_RawItem_To_Observation()
    {
        var rawItem = new RawItem
        {
            ConnectorId = Guid.NewGuid(),
            ContentType = RawContentType.Json,
            SourceHash = new string('a', 64),
            SourceUrl = "https://example.test/alpha",
            PublishedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc),
            RawContent = JsonSerializer.Serialize(new
            {
                title = "Article Alpha",
                summary = "Résumé Alpha",
                link = "https://example.test/alpha",
                language = "fr",
                feedTitle = "Flux Test"
            })
        };

        var result = await new RawItemNormalizationService().NormalizeAsync(rawItem);

        Assert.True(result.Success);
        Assert.NotNull(result.Observation);
        Assert.Equal(rawItem.Id, result.Observation!.RawItemId);
        Assert.Equal("Article Alpha", result.Observation.Title);
        Assert.Equal("Résumé Alpha", result.Observation.Content);
        Assert.Equal(rawItem.ConnectorId, result.Observation.SourceConnectorId);
        Assert.Equal("New", result.Observation.Status.ToString());
    }

    [Fact]
    public async Task NormalizeAsync_Rejects_Empty_Rss_RawItem()
    {
        var rawItem = new RawItem
        {
            ContentType = RawContentType.Json,
            SourceHash = new string('b', 64),
            RawContent = "{}"
        };

        var result = await new RawItemNormalizationService().NormalizeAsync(rawItem);

        Assert.False(result.Success);
        Assert.Null(result.Observation);
        Assert.Contains("sans titre", result.ErrorMessage);
    }

    [Fact]
    public async Task NormalizeAsync_Maps_Gdelt_RawItem_To_Geospatial_Observation()
    {
        var rawItem = new RawItem
        {
            ConnectorId = Guid.NewGuid(),
            ContentType = RawContentType.Json,
            SourceHash = new string('d', 64),
            SourceUrl = "https://example.test/gdelt-alpha",
            PublishedAt = new DateTime(2026, 04, 28, 10, 15, 00, DateTimeKind.Utc),
            RawContent = JsonSerializer.Serialize(new
            {
                source = "gdelt",
                title = "GDELT Alpha Incident",
                url = "https://example.test/gdelt-alpha",
                domain = "example.test",
                language = "English",
                sourceCountry = "United States",
                query = "Sudan conflict"
            })
        };

        var result = await new RawItemNormalizationService().NormalizeAsync(rawItem);

        Assert.True(result.Success);
        Assert.NotNull(result.Observation);
        Assert.Equal(ObservationType.GeospatialMetadata, result.Observation!.Type);
        Assert.Equal("GDELT Alpha Incident", result.Observation.Title);
        Assert.Equal(0.45, result.Observation.SourceReliability);
        Assert.Contains("gdelt", result.Observation.MetadataJson);
    }
}
