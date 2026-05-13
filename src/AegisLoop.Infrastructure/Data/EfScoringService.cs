using AegisLoop.Application.Dtos;
using AegisLoop.Application.Interfaces;
using AegisLoop.Application.Services;
using AegisLoop.Domain;
using AegisLoop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AegisLoop.Infrastructure.Data;

public sealed class EfScoringService : IScoringService
{
    private readonly AegisLoopDbContext _dbContext;

    public EfScoringService(AegisLoopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ConfidenceScore> ScoreObservationAsync(Guid observationId, CancellationToken cancellationToken = default)
    {
        var observation = await _dbContext.Observations.FirstAsync(o => o.Id == observationId, cancellationToken);
        var allObservations = await _dbContext.Observations.AsNoTracking().ToListAsync(cancellationToken);
        var feedbacks = await _dbContext.AnalystFeedbacks.AsNoTracking().ToListAsync(cancellationToken);
        var score = ObservationScoringCalculator.ScoreObservation(observation, allObservations, feedbacks);

        _dbContext.ConfidenceScores.Add(score);
        AddScoringAudit("ObservationScoreCalculated", score);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return score;
    }

    public async Task<ConfidenceScore> ScoreEventAsync(Guid eventCaseId, CancellationToken cancellationToken = default)
    {
        var eventCase = await _dbContext.EventCases.FirstAsync(e => e.Id == eventCaseId, cancellationToken);
        var observations = await _dbContext.Observations.AsNoTracking()
            .Where(o => o.EventCaseId == eventCaseId)
            .ToListAsync(cancellationToken);
        var observationIds = observations.Select(o => o.Id).ToHashSet();
        var observationScores = await _dbContext.ConfidenceScores.AsNoTracking()
            .Where(s => s.TargetType == ScoreTargetType.Observation && observationIds.Contains(s.TargetId))
            .ToListAsync(cancellationToken);
        var feedbacks = await _dbContext.AnalystFeedbacks.AsNoTracking().ToListAsync(cancellationToken);
        var score = ObservationScoringCalculator.ScoreEvent(eventCase, observations, observationScores, feedbacks);

        _dbContext.ConfidenceScores.Add(score);
        AddScoringAudit("EventCaseScoreCalculated", score);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return score;
    }

    public async Task<ScoreBreakdownDto?> GetBreakdownAsync(Guid targetId, CancellationToken cancellationToken = default)
    {
        var score = await _dbContext.ConfidenceScores.AsNoTracking()
            .Where(s => s.TargetId == targetId)
            .OrderByDescending(s => s.CalculatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return score is null ? null : ToBreakdown(score);
    }

    public async Task RecalculateAfterFeedbackAsync(Guid targetId, ScoreTargetType targetType)
    {
        if (targetType == ScoreTargetType.Observation)
        {
            await ScoreObservationAsync(targetId);
            var eventCaseId = await _dbContext.Observations
                .Where(o => o.Id == targetId)
                .Select(o => o.EventCaseId)
                .FirstOrDefaultAsync();
            if (eventCaseId is not null)
            {
                await ScoreEventAsync(eventCaseId.Value);
            }
        }
        else
        {
            await ScoreEventAsync(targetId);
        }
    }

    public static ScoreBreakdownDto ToBreakdown(ConfidenceScore score)
    {
        return new ScoreBreakdownDto(
            score.TargetId,
            score.TargetType.ToString(),
            Math.Round(score.Value, 4, MidpointRounding.AwayFromZero),
            score.CalculatedAt,
            score.AlgorithmVersion,
            new[]
            {
                Component("SourceReliability", score.SourceReliability, ObservationScoringCalculator.SourceReliabilityWeight, "Fiabilité déclarée/calculée de la source normalisée."),
                Component("Corroboration", score.Corroboration, ObservationScoringCalculator.CorroborationWeight, "Part des sources indépendantes ou observations proches confirmant le fait."),
                Component("AnalystFeedback", score.AnalystFeedback, ObservationScoringCalculator.AnalystFeedbackWeight, "Dernier feedback analyste : défaut/neutre/note=0.5, confirmé=1, corrigé=0.7, invalidé=0.")
            });
    }

    private void AddScoringAudit(string action, ConfidenceScore score)
    {
        _dbContext.AuditEntries.Add(new AuditEntry
        {
            Category = AuditCategory.Scoring,
            Action = action,
            Actor = "system",
            TargetType = score.TargetType.ToString(),
            TargetId = score.TargetId,
            Details = JsonSerializer.Serialize(new
            {
                score.Id,
                score.Value,
                score.SourceReliability,
                score.Corroboration,
                score.AnalystFeedback,
                score.AlgorithmVersion
            })
        });
    }

    private static ScoreComponentDto Component(string name, double value, double weight, string explanation)
    {
        var boundedValue = ObservationScoringCalculator.Clamp01(value);
        return new ScoreComponentDto(
            name,
            boundedValue,
            weight,
            Math.Round(boundedValue * weight, 4, MidpointRounding.AwayFromZero),
            explanation);
    }
}