namespace AegisLoop.Application.Dtos;

public sealed record FeedbackDto(
    Guid Id,
    Guid TargetId,
    string TargetType,
    string Action,
    string? Details,
    DateTime CreatedAt);

public sealed record SubmitFeedbackRequest(
    Guid TargetId,
    string TargetType,
    string Action,
    string? Details);

public sealed record SourceConnectorProvenanceDto(
    Guid Id,
    string Name,
    string ConnectorType,
    string Status,
    DateTime? LastRunAt);

public sealed record RawItemProvenanceDto(
    Guid Id,
    string SourceHash,
    string ContentType,
    DateTime CollectedAt,
    DateTime? PublishedAt,
    string? SourceUrl,
    IReadOnlyDictionary<string, string?> Metadata);

public sealed record IngestionJobProvenanceDto(
    Guid Id,
    DateTime StartedAt,
    DateTime? CompletedAt,
    string Status,
    int ItemsCollected,
    int ItemsNormalized,
    string? ErrorMessage);

public sealed record ObservationProvenanceDto(
    Guid ObservationId,
    string Title,
    Guid? EventCaseId,
    DateTime ObservedAt,
    string? SourceUrl,
    SourceConnectorProvenanceDto? SourceConnector,
    RawItemProvenanceDto? RawItem,
    IngestionJobProvenanceDto? IngestionJob,
    ScoreBreakdownDto? Score,
    IReadOnlyList<FeedbackDto> Feedbacks,
    IReadOnlyDictionary<string, string?> Metadata);

public sealed record EventCaseProvenanceDto(
    Guid EventCaseId,
    string Title,
    IReadOnlyList<ObservationProvenanceDto> Observations,
    IReadOnlyList<SourceConnectorProvenanceDto> Sources,
    IReadOnlyList<RawItemProvenanceDto> RawItems,
    IReadOnlyList<string> Hashes,
    IReadOnlyList<FeedbackDto> Feedbacks,
    ScoreBreakdownDto? Score);

public sealed record AuditEntryDto(
    Guid Id,
    DateTime Date,
    string Category,
    string Action,
    string? TargetType,
    Guid? TargetId,
    string Message,
    string Level,
    string Actor);