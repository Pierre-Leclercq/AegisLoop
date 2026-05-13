namespace AegisLoop.Application.Dtos;

public sealed record ObservationDto(
    Guid Id,
    Guid? RawItemId,
    string Title,
    string Content,
    string? ClaimText,
    string Status,
    DateTime ObservedAt,
    Guid SourceConnectorId,
    string SourceType,
    string SourceName,
    string? SourceUrl,
    double SourceReliability,
    string? Language);
