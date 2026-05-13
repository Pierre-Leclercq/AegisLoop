using Xunit;
using AegisLoop.Application.Services;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Tests;

public class SmokeTests
{
    [Fact]
    public async Task Application_Project_Loads_Successfully()
    {
        var service = new RawItemNormalizationService();
        var result = await service.NormalizeAsync(new RawItem
        {
            ContentType = RawContentType.Json,
            SourceHash = new string('a', 64),
            RawContent = "{\"title\":\"Smoke\",\"summary\":\"Application normalization\"}"
        });

        Assert.True(result.Success);
        Assert.Equal("Smoke", result.Observation?.Title);
    }
}