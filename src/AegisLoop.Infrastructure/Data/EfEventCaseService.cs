using AegisLoop.Application.Dtos;
using AegisLoop.Application.Interfaces;
using AegisLoop.Application.Services;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AegisLoop.Infrastructure.Data;

public sealed class EfEventCaseService : IEventCaseService
{
    private readonly AegisLoopDbContext _dbContext;
    private readonly IScoringService _scoringService;

    public EfEventCaseService(AegisLoopDbContext dbContext, IScoringService scoringService)
    {
        _dbContext = dbContext;
        _scoringService = scoringService;
    }

    public async Task<EventRebuildResultDto> RebuildAsync(CancellationToken cancellationToken = default)
    {
        var observations = await _dbContext.Observations
            .OrderBy(o => o.ObservedAt)
            .ThenBy(o => o.Title)
            .ToListAsync(cancellationToken);

        _dbContext.ConfidenceScores.RemoveRange(_dbContext.ConfidenceScores);
        _dbContext.EventCases.RemoveRange(_dbContext.EventCases);
        foreach (var observation in observations)
        {
            observation.EventCaseId = null;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var clusters = BuildClusters(observations);
        var now = DateTime.UtcNow;
        var linksCreated = 0;

        foreach (var cluster in clusters)
        {
            var eventCase = new EventCase
            {
                Title = EventCaseHeuristics.BuildEventTitle(cluster),
                Summary = BuildSummary(cluster),
                Category = EventCaseHeuristics.InferCategory(cluster),
                Status = AegisLoop.Domain.EventStatus.Detected,
                StartedAt = cluster.Min(o => o.ObservedAt),
                EndedAt = cluster.Count > 1 ? cluster.Max(o => o.ObservedAt) : null,
                LocationId = cluster
                    .Where(o => o.GeoLocationId is not null)
                    .GroupBy(o => o.GeoLocationId!.Value)
                    .OrderByDescending(g => g.Count())
                    .Select(g => (Guid?)g.Key)
                    .FirstOrDefault(),
                CorroborationCount = cluster.Select(o => o.SourceConnectorId).Distinct().Count(),
                CreatedAt = now,
                UpdatedAt = now
            };

            _dbContext.EventCases.Add(eventCase);
            foreach (var observation in cluster)
            {
                observation.EventCaseId = eventCase.Id;
                linksCreated++;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        foreach (var observation in observations)
        {
            await _scoringService.ScoreObservationAsync(observation.Id, cancellationToken);
        }

        foreach (var eventCaseId in await _dbContext.EventCases.Select(e => e.Id).ToListAsync(cancellationToken))
        {
            await _scoringService.ScoreEventAsync(eventCaseId, cancellationToken);
        }

        _dbContext.AuditEntries.Add(new AuditEntry
        {
            Category = AuditCategory.Correlation,
            Action = "EventCasesRebuilt",
            Actor = "system",
            TargetType = nameof(EventCase),
            Details = JsonSerializer.Serialize(new { observationsProcessed = observations.Count, eventCasesCreated = clusters.Count, linksCreated })
        });
        await _dbContext.SaveChangesAsync(cancellationToken);

        var events = await new EfAegisLoopStore(_dbContext).ListEventCasesAsync(500, cancellationToken);
        return new EventRebuildResultDto(observations.Count, clusters.Count, linksCreated, events);
    }

    private static List<List<Observation>> BuildClusters(IReadOnlyList<Observation> observations)
    {
        var clusters = new List<List<Observation>>();

        foreach (var observation in observations)
        {
            var matchingCluster = clusters.FirstOrDefault(cluster => cluster.Any(existing => EventCaseHeuristics.AreClose(existing, observation)));
            if (matchingCluster is null)
            {
                clusters.Add(new List<Observation> { observation });
            }
            else
            {
                matchingCluster.Add(observation);
            }
        }

        return clusters;
    }

    private static string BuildSummary(IReadOnlyList<Observation> observations)
    {
        var sources = observations.Select(o => o.SourceConnectorId).Distinct().Count();
        var started = observations.Min(o => o.ObservedAt).ToString("u");
        var ended = observations.Max(o => o.ObservedAt).ToString("u");
        return $"EventCase V1 généré par regroupement heuristique de {observations.Count} observation(s), {sources} source(s), fenêtre {started} → {ended}.";
    }
}