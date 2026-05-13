using AegisLoop.Domain.Entities;
using Xunit;

namespace AegisLoop.Domain.Tests.Entities;

public class ObservationTests
{
    [Fact]
    public void Observation_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var observation = new Observation();

        // Assert
        Assert.NotEqual(Guid.Empty, observation.Id);
        Assert.Equal(string.Empty, observation.Title);
        Assert.Equal(string.Empty, observation.Content);
        Assert.Equal(ObservationStatus.New, observation.Status);
        Assert.Equal(ObservationType.Article, observation.Type);
        Assert.Equal(0.3, observation.SourceReliability);
        Assert.False(observation.IsManual);
        Assert.True(observation.ObservedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Observation_CanSetProperties()
    {
        // Arrange
        var sourceConnectorId = Guid.NewGuid();
        var observation = new Observation
        {
            Title = "Crise au Soudan — combats à Khartoum",
            Content = "Des affrontements ont éclaté...",
            ClaimText = "Des combats ont lieu à Khartoum",
            Type = ObservationType.Article,
            SourceConnectorId = sourceConnectorId,
            SourceReliability = 0.75,
            Language = "fr",
        };

        // Assert
        Assert.Equal("Crise au Soudan — combats à Khartoum", observation.Title);
        Assert.Equal(ObservationType.Article, observation.Type);
        Assert.Equal(0.75, observation.SourceReliability);
        Assert.Equal("fr", observation.Language);
    }
}