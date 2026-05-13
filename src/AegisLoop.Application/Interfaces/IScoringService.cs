using AegisLoop.Domain;
using AegisLoop.Application.Dtos;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Service de scoring heuristique V1 — 3 composantes :
/// FiabilitéSource (W1=0.35) + Corroboration (W2=0.35) + FeedbackAnalyste (W3=0.30).
/// </summary>
public interface IScoringService
{
    Task<ConfidenceScore> ScoreObservationAsync(Guid observationId, CancellationToken cancellationToken = default);
    Task<ConfidenceScore> ScoreEventAsync(Guid eventCaseId, CancellationToken cancellationToken = default);
    Task<ScoreBreakdownDto?> GetBreakdownAsync(Guid targetId, CancellationToken cancellationToken = default);
    Task RecalculateAfterFeedbackAsync(Guid targetId, ScoreTargetType targetType);
}