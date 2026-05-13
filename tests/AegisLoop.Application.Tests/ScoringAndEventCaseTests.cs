using AegisLoop.Application.Services;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using AegisLoop.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AegisLoop.Application.Tests;

public sealed class ScoringAndEventCaseTests
{
    [Fact]
    public void ScoreObservation_Is_Deterministic_Bounded_And_Has_Three_Components()
    {
        var sourceA = Guid.NewGuid();
        var sourceB = Guid.NewGuid();
        var observedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc);
        var observation = Observation("Khartoum conflict incident", "Khartoum conflict incident reported downtown", sourceA, observedAt, 0.8);
        var corroborating = Observation("Khartoum conflict incident update", "Khartoum conflict incident confirmed by another source", sourceB, observedAt.AddHours(1), 0.6);
        var all = new[] { observation, corroborating };

        var first = ObservationScoringCalculator.ScoreObservation(observation, all, Array.Empty<AnalystFeedback>());
        var second = ObservationScoringCalculator.ScoreObservation(observation, all, Array.Empty<AnalystFeedback>());

        Assert.Equal(first.Value, second.Value);
        Assert.InRange(first.Value, 0.0, 1.0);
        Assert.InRange(first.SourceReliability, 0.0, 1.0);
        Assert.InRange(first.Corroboration, 0.0, 1.0);
        Assert.InRange(first.AnalystFeedback, 0.0, 1.0);
        Assert.Equal(0.8, first.SourceReliability);
        Assert.True(first.Corroboration > 0.3);
    }

    [Fact]
    public void ScoreBreakdown_Exposes_Three_Readable_Components()
    {
        var score = new ConfidenceScore
        {
            TargetId = Guid.NewGuid(),
            TargetType = ScoreTargetType.Observation,
            SourceReliability = 0.8,
            Corroboration = 0.6,
            AnalystFeedback = 0.5,
            Value = ObservationScoringCalculator.Weighted(0.8, 0.6, 0.5),
            AlgorithmVersion = ObservationScoringCalculator.AlgorithmVersion
        };

        var breakdown = EfScoringService.ToBreakdown(score);

        Assert.Equal(3, breakdown.Components.Count);
        Assert.Contains(breakdown.Components, c => c.Name == "SourceReliability" && c.Explanation.Length > 10);
        Assert.Contains(breakdown.Components, c => c.Name == "Corroboration");
        Assert.Contains(breakdown.Components, c => c.Name == "AnalystFeedback");
        Assert.All(breakdown.Components, component => Assert.InRange(component.Value, 0.0, 1.0));
    }

    [Fact]
    public void EventCaseHeuristic_Groups_Close_Observations_And_Rejects_Different_Ones()
    {
        var now = DateTime.UtcNow;
        var closeA = Observation("Aden maritime attack vessel", "Commercial vessel reports maritime attack near Aden", Guid.NewGuid(), now, 0.7);
        var closeB = Observation("Aden maritime attack update", "Second source confirms maritime attack on vessel near Aden", Guid.NewGuid(), now.AddHours(2), 0.6);
        var different = Observation("European wheat market prices", "Market prices rise after harvest data", Guid.NewGuid(), now.AddHours(2), 0.6);

        Assert.True(EventCaseHeuristics.AreClose(closeA, closeB));
        Assert.False(EventCaseHeuristics.AreClose(closeA, different));
    }

    [Fact]
    public void ScoreEvent_Is_Aggregated_From_Observation_Scores_And_Corroboration()
    {
        var eventCase = new EventCase { Id = Guid.NewGuid(), Title = "Aden maritime attack" };
        var obsA = Observation("Aden maritime attack vessel", "Commercial vessel reports maritime attack near Aden", Guid.NewGuid(), DateTime.UtcNow, 0.7);
        var obsB = Observation("Aden maritime attack update", "Second source confirms maritime attack on vessel near Aden", Guid.NewGuid(), DateTime.UtcNow, 0.6);
        obsA.EventCaseId = eventCase.Id;
        obsB.EventCaseId = eventCase.Id;
        var scores = new[]
        {
            new ConfidenceScore { TargetId = obsA.Id, TargetType = ScoreTargetType.Observation, Value = 0.6, CalculatedAt = DateTime.UtcNow },
            new ConfidenceScore { TargetId = obsB.Id, TargetType = ScoreTargetType.Observation, Value = 0.9, CalculatedAt = DateTime.UtcNow }
        };

        var eventScore = ObservationScoringCalculator.ScoreEvent(eventCase, new[] { obsA, obsB }, scores, Array.Empty<AnalystFeedback>());

        Assert.InRange(eventScore.Value, 0.0, 1.0);
        Assert.Equal(ObservationScoringCalculator.Weighted(0.75, 2.0 / 3.0, 0.5), eventScore.Value);
    }

    [Fact]
    public void AnalystFeedback_Component_Uses_Latest_Feedback_Deterministically()
    {
        var targetId = Guid.NewGuid();
        var feedbacks = new[]
        {
            new AnalystFeedback { TargetId = targetId, TargetType = nameof(Observation), Action = FeedbackAction.Confirm, CreatedAt = new DateTime(2026, 04, 28, 10, 00, 00, DateTimeKind.Utc) },
            new AnalystFeedback { TargetId = targetId, TargetType = nameof(Observation), Action = FeedbackAction.Invalidate, CreatedAt = new DateTime(2026, 04, 28, 11, 00, 00, DateTimeKind.Utc) }
        };

        var component = ObservationScoringCalculator.ComputeAnalystFeedback(targetId, nameof(Observation), feedbacks);

        Assert.Equal(0.0, component);
    }

    [Fact]
    public async Task EventCaseService_Rebuild_Persists_EventCases_Links_And_Scores()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();
        var sourceA = new SourceConnector { Name = "RSS A", ConnectorType = ConnectorType.Rss, Config = "{}" };
        var sourceB = new SourceConnector { Name = "GDELT B", ConnectorType = ConnectorType.Gdelt, Config = "{}" };
        dbContext.SourceConnectors.AddRange(sourceA, sourceB);
        dbContext.Observations.AddRange(
            Observation("Khartoum conflict incident", "Khartoum conflict incident reported downtown", sourceA.Id, DateTime.UtcNow, 0.8),
            Observation("Khartoum conflict incident update", "Another source confirms Khartoum conflict incident", sourceB.Id, DateTime.UtcNow.AddHours(1), 0.6),
            Observation("European wheat market prices", "Market prices rise after harvest data", sourceA.Id, DateTime.UtcNow.AddHours(1), 0.5));
        await dbContext.SaveChangesAsync();

        var service = new EfEventCaseService(dbContext, new EfScoringService(dbContext));
        var result = await service.RebuildAsync();

        Assert.Equal(3, result.ObservationsProcessed);
        Assert.Equal(2, result.EventCasesCreated);
        Assert.Equal(3, result.LinksCreated);
        Assert.Equal(2, await dbContext.EventCases.CountAsync());
        Assert.Equal(3, await dbContext.Observations.CountAsync(o => o.EventCaseId != null));
        Assert.True(await dbContext.ConfidenceScores.AnyAsync(s => s.TargetType == ScoreTargetType.EventCase));
    }

    private static Observation Observation(string title, string content, Guid sourceId, DateTime observedAt, double reliability)
    {
        return new Observation
        {
            Title = title,
            Content = content,
            ClaimText = content,
            SourceConnectorId = sourceId,
            ObservedAt = observedAt,
            SourceReliability = reliability,
            Language = "en"
        };
    }

    private static AegisLoopDbContext CreateDbContext()
    {
        return new AegisLoopDbContext(new DbContextOptionsBuilder<AegisLoopDbContext>()
            .UseSqlite($"Data Source={Path.Combine(Path.GetTempPath(), $"aegisloop-phase3-{Guid.NewGuid():N}.db")}")
            .Options);
    }
}