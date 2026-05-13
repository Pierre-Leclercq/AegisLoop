namespace AegisLoop.Application.Dtos;

public sealed record IngestionRequest(
    string? ConnectorType,
    Guid? ConnectorId,
    string? FeedUrl,
    string? Query,
    int? MaxItems);

public sealed record IngestionResponse(
    Guid JobId,
    Guid ConnectorId,
    string ConnectorType,
    string Status,
    int ItemsCollected,
    int ItemsCreated,
    int ItemsSkipped,
    int ItemsNormalized,
    IReadOnlyList<string> Errors);

public sealed record IngestionJobDto(
    Guid Id,
    Guid ConnectorId,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status,
    int ItemsCollected,
    int ItemsNormalized,
    string? ErrorMessage);

public sealed record SourceConnectorDto(
    Guid Id,
    string ConnectorType,
    string Name,
    string Status,
    DateTime? LastRunAt,
    int ErrorCount);
