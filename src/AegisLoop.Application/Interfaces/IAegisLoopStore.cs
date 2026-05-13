using AegisLoop.Application.Dtos;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

public interface IAegisLoopStore
{
    Task<SourceConnector> EnsureDemoRssConnectorAsync(string feedUrl, CancellationToken cancellationToken = default);
    Task<SourceConnector> EnsureDemoGdeltConnectorAsync(string query, CancellationToken cancellationToken = default);
    Task<SourceConnector?> GetConnectorAsync(Guid connectorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SourceConnectorDto>> ListConnectorsAsync(CancellationToken cancellationToken = default);

    Task<IngestionJob> CreateIngestionJobAsync(Guid connectorId, CancellationToken cancellationToken = default);
    Task CompleteIngestionJobAsync(Guid jobId, JobStatus status, int itemsCollected, int itemsNormalized, string? errorMessage, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IngestionJobDto>> ListIngestionJobsAsync(CancellationToken cancellationToken = default);
    Task<IngestionJobDto?> GetIngestionJobAsync(Guid jobId, CancellationToken cancellationToken = default);

    Task<bool> SourceHashExistsAsync(string sourceHash, CancellationToken cancellationToken = default);
    Task AddRawItemAsync(RawItem rawItem, CancellationToken cancellationToken = default);
    Task AddObservationAsync(Observation observation, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ObservationDto>> ListObservationsAsync(int take = 100, CancellationToken cancellationToken = default);
    Task<ObservationDto?> GetObservationAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<EventCaseSummaryDto>> ListEventCasesAsync(int take = 100, CancellationToken cancellationToken = default);
    Task<EventCaseDetailDto?> GetEventCaseAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAuditEntryAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
