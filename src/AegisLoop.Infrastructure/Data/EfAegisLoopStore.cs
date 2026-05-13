using System.Text.Json;
using AegisLoop.Application.Dtos;
using AegisLoop.Application.Interfaces;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AegisLoop.Infrastructure.Data;

public sealed class EfAegisLoopStore : IAegisLoopStore
{
    private readonly AegisLoopDbContext _dbContext;

    public EfAegisLoopStore(AegisLoopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<SourceConnector> EnsureDemoRssConnectorAsync(string feedUrl, CancellationToken cancellationToken = default)
    {
        var connector = await _dbContext.SourceConnectors
            .FirstOrDefaultAsync(c => c.ConnectorType == ConnectorType.Rss && c.Name == "RSS Démo", cancellationToken);

        var config = JsonSerializer.Serialize(new { feedUrl, pollingIntervalMinutes = 15, maxItemsPerPoll = 25, timeoutSeconds = 10 });
        if (connector is null)
        {
            connector = new SourceConnector
            {
                ConnectorType = ConnectorType.Rss,
                Name = "RSS Démo",
                Config = config,
                Status = ConnectorStatus.Active
            };
            _dbContext.SourceConnectors.Add(connector);
        }
        else
        {
            connector.Config = config;
            connector.Status = ConnectorStatus.Active;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return connector;
    }

    public async Task<SourceConnector> EnsureDemoGdeltConnectorAsync(string query, CancellationToken cancellationToken = default)
    {
        var connector = await _dbContext.SourceConnectors
            .FirstOrDefaultAsync(c => c.ConnectorType == ConnectorType.Gdelt && c.Name == "GDELT Démo", cancellationToken);

        var config = JsonSerializer.Serialize(new { query, maxItemsPerPoll = 25, timeoutSeconds = 10 });
        if (connector is null)
        {
            connector = new SourceConnector
            {
                ConnectorType = ConnectorType.Gdelt,
                Name = "GDELT Démo",
                Config = config,
                Status = ConnectorStatus.Active
            };
            _dbContext.SourceConnectors.Add(connector);
        }
        else
        {
            connector.Config = config;
            connector.Status = ConnectorStatus.Active;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return connector;
    }

    public Task<SourceConnector?> GetConnectorAsync(Guid connectorId, CancellationToken cancellationToken = default)
    {
        return _dbContext.SourceConnectors.FirstOrDefaultAsync(c => c.Id == connectorId, cancellationToken);
    }

    public async Task<IReadOnlyList<SourceConnectorDto>> ListConnectorsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SourceConnectors
            .OrderBy(c => c.Name)
            .Select(c => new SourceConnectorDto(c.Id, c.ConnectorType.ToString(), c.Name, c.Status.ToString(), c.LastRunAt, c.ErrorCount))
            .ToListAsync(cancellationToken);
    }

    public async Task<IngestionJob> CreateIngestionJobAsync(Guid connectorId, CancellationToken cancellationToken = default)
    {
        var job = new IngestionJob
        {
            ConnectorId = connectorId,
            StartedAt = DateTime.UtcNow,
            Status = JobStatus.Running
        };
        _dbContext.IngestionJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return job;
    }

    public async Task CompleteIngestionJobAsync(Guid jobId, JobStatus status, int itemsCollected, int itemsNormalized, string? errorMessage, CancellationToken cancellationToken = default)
    {
        var job = await _dbContext.IngestionJobs.FirstAsync(j => j.Id == jobId, cancellationToken);
        job.Status = status;
        job.CompletedAt = DateTime.UtcNow;
        job.ItemsCollected = itemsCollected;
        job.ItemsNormalized = itemsNormalized;
        job.ErrorMessage = errorMessage;

        var connector = await _dbContext.SourceConnectors.FirstAsync(c => c.Id == job.ConnectorId, cancellationToken);
        connector.LastRunAt = job.CompletedAt;
        connector.Status = status == JobStatus.Failed ? ConnectorStatus.Error : ConnectorStatus.Active;
        connector.ErrorCount = status == JobStatus.Failed ? connector.ErrorCount + 1 : 0;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<IngestionJobDto>> ListIngestionJobsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.IngestionJobs
            .OrderByDescending(j => j.StartedAt)
            .Take(50)
            .Select(j => new IngestionJobDto(j.Id, j.ConnectorId, j.StartedAt, j.CompletedAt, j.Status.ToString(), j.ItemsCollected, j.ItemsNormalized, j.ErrorMessage))
            .ToListAsync(cancellationToken);
    }

    public Task<IngestionJobDto?> GetIngestionJobAsync(Guid jobId, CancellationToken cancellationToken = default)
    {
        return _dbContext.IngestionJobs
            .Where(j => j.Id == jobId)
            .Select(j => new IngestionJobDto(j.Id, j.ConnectorId, j.StartedAt, j.CompletedAt, j.Status.ToString(), j.ItemsCollected, j.ItemsNormalized, j.ErrorMessage))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<bool> SourceHashExistsAsync(string sourceHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RawItems.AnyAsync(r => r.SourceHash == sourceHash, cancellationToken);
    }

    public async Task AddRawItemAsync(RawItem rawItem, CancellationToken cancellationToken = default)
    {
        _dbContext.RawItems.Add(rawItem);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddObservationAsync(Observation observation, CancellationToken cancellationToken = default)
    {
        _dbContext.Observations.Add(observation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ObservationDto>> ListObservationsAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        return await OrderedObservationQuery()
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);
    }

    public Task<ObservationDto?> GetObservationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return OrderedObservationQuery().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<EventCaseSummaryDto>> ListEventCasesAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        var events = await _dbContext.EventCases.AsNoTracking()
            .OrderByDescending(e => e.UpdatedAt)
            .Take(Math.Clamp(take, 1, 500))
            .ToListAsync(cancellationToken);

        return await BuildEventSummariesAsync(events, cancellationToken);
    }

    public async Task<EventCaseDetailDto?> GetEventCaseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventCase = await _dbContext.EventCases.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (eventCase is null)
        {
            return null;
        }

        var summary = (await BuildEventSummariesAsync(new[] { eventCase }, cancellationToken)).Single();
        var observations = await EventObservationQuery(id)
            .ToListAsync(cancellationToken);

        return new EventCaseDetailDto(
            summary.Id,
            summary.Title,
            summary.Summary,
            summary.Category,
            summary.Status,
            summary.StartedAt,
            summary.CreatedAt,
            summary.UpdatedAt,
            summary.ObservationCount,
            summary.CorroborationCount,
            summary.Score,
            summary.Sources,
            summary.ScoreBreakdown,
            observations);
    }

    public async Task AddAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        _dbContext.AuditEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private IQueryable<ObservationDto> OrderedObservationQuery()
    {
        return from observation in _dbContext.Observations.AsNoTracking()
               join connector in _dbContext.SourceConnectors.AsNoTracking()
                    on observation.SourceConnectorId equals connector.Id into connectors
               from connector in connectors.DefaultIfEmpty()
               orderby observation.ObservedAt descending
               select new ObservationDto(
                   observation.Id,
                   observation.RawItemId,
                   observation.Title,
                   observation.Content,
                   observation.ClaimText,
                   observation.Status.ToString(),
                   observation.ObservedAt,
                   observation.SourceConnectorId,
                    connector == null ? "Unknown" : connector.ConnectorType.ToString(),
                   connector == null ? "Source inconnue" : connector.Name,
                   observation.SourceUrl,
                   observation.SourceReliability,
                   observation.Language);
    }

    private IQueryable<ObservationDto> EventObservationQuery(Guid eventCaseId)
    {
        return from observation in _dbContext.Observations.AsNoTracking()
               join connector in _dbContext.SourceConnectors.AsNoTracking()
                    on observation.SourceConnectorId equals connector.Id into connectors
               from connector in connectors.DefaultIfEmpty()
               where observation.EventCaseId == eventCaseId
               orderby observation.ObservedAt descending
               select new ObservationDto(
                   observation.Id,
                   observation.RawItemId,
                   observation.Title,
                   observation.Content,
                   observation.ClaimText,
                   observation.Status.ToString(),
                   observation.ObservedAt,
                   observation.SourceConnectorId,
                   connector == null ? "Unknown" : connector.ConnectorType.ToString(),
                   connector == null ? "Source inconnue" : connector.Name,
                   observation.SourceUrl,
                   observation.SourceReliability,
                   observation.Language);
    }

    private async Task<IReadOnlyList<EventCaseSummaryDto>> BuildEventSummariesAsync(IReadOnlyList<EventCase> events, CancellationToken cancellationToken)
    {
        if (events.Count == 0)
        {
            return Array.Empty<EventCaseSummaryDto>();
        }

        var eventIds = events.Select(e => e.Id).ToHashSet();
        var observations = await (from observation in _dbContext.Observations.AsNoTracking()
                                  join connector in _dbContext.SourceConnectors.AsNoTracking()
                                      on observation.SourceConnectorId equals connector.Id into connectors
                                  from connector in connectors.DefaultIfEmpty()
                                  where observation.EventCaseId != null && eventIds.Contains(observation.EventCaseId.Value)
                                  select new
                                  {
                                      observation.EventCaseId,
                                      observation.SourceConnectorId,
                                      SourceName = connector == null ? "Source inconnue" : connector.Name
                                  }).ToListAsync(cancellationToken);

        var eventScores = await _dbContext.ConfidenceScores.AsNoTracking()
            .Where(s => eventIds.Contains(s.TargetId) && s.TargetType == ScoreTargetType.EventCase)
            .ToListAsync(cancellationToken);

        var latestScores = eventScores
            .GroupBy(s => s.TargetId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CalculatedAt).First());

        return events.Select(eventCase =>
        {
            var eventObservations = observations.Where(o => o.EventCaseId == eventCase.Id).ToList();
            var score = latestScores.TryGetValue(eventCase.Id, out var latestScore) ? latestScore : null;
            return new EventCaseSummaryDto(
                eventCase.Id,
                eventCase.Title,
                eventCase.Summary,
                eventCase.Category.ToString(),
                eventCase.Status.ToString(),
                eventCase.StartedAt,
                eventCase.CreatedAt,
                eventCase.UpdatedAt,
                eventObservations.Count,
                eventCase.CorroborationCount,
                score?.Value ?? 0.0,
                eventObservations.Select(o => o.SourceName).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(s => s).ToList(),
                score is null ? null : EfScoringService.ToBreakdown(score));
        }).ToList();
    }
}