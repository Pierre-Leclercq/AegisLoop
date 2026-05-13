namespace AegisLoop.Application.Dtos;

public sealed record ScoreComponentDto(
    string Name,
    double Value,
    double Weight,
    double Contribution,
    string Explanation);

public sealed record ScoreBreakdownDto(
    Guid TargetId,
    string TargetType,
    double Value,
    DateTime CalculatedAt,
    string AlgorithmVersion,
    IReadOnlyList<ScoreComponentDto> Components);

public sealed record EventCaseSummaryDto(
    Guid Id,
    string Title,
    string? Summary,
    string Category,
    string Status,
    DateTime StartedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ObservationCount,
    int CorroborationCount,
    double Score,
    IReadOnlyList<string> Sources,
    ScoreBreakdownDto? ScoreBreakdown);

public sealed record EventCaseDetailDto(
    Guid Id,
    string Title,
    string? Summary,
    string Category,
    string Status,
    DateTime StartedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int ObservationCount,
    int CorroborationCount,
    double Score,
    IReadOnlyList<string> Sources,
    ScoreBreakdownDto? ScoreBreakdown,
    IReadOnlyList<ObservationDto> Observations);

public sealed record EventRebuildResultDto(
    int ObservationsProcessed,
    int EventCasesCreated,
    int LinksCreated,
    IReadOnlyList<EventCaseSummaryDto> Events);