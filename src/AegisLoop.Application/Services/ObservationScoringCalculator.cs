using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Services;

public static class ObservationScoringCalculator
{
    public const double SourceReliabilityWeight = 0.35;
    public const double CorroborationWeight = 0.35;
    public const double AnalystFeedbackWeight = 0.30;
    public const string AlgorithmVersion = "V1-heuristic-3c-2026-04";

    public static ConfidenceScore ScoreObservation(Observation observation, IEnumerable<Observation> allObservations, IEnumerable<AnalystFeedback> feedbacks)
    {
        var sourceReliability = Clamp01(observation.SourceReliability);
        var corroboration = ComputeObservationCorroboration(observation, allObservations);
        var analystFeedback = ComputeAnalystFeedback(observation.Id, nameof(Observation), feedbacks);
        var value = Weighted(sourceReliability, corroboration, analystFeedback);

        return new ConfidenceScore
        {
            TargetId = observation.Id,
            TargetType = ScoreTargetType.Observation,
            SourceReliability = sourceReliability,
            Corroboration = corroboration,
            AnalystFeedback = analystFeedback,
            Value = value,
            AlgorithmVersion = AlgorithmVersion,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public static ConfidenceScore ScoreEvent(EventCase eventCase, IReadOnlyList<Observation> observations, IReadOnlyList<ConfidenceScore> observationScores, IEnumerable<AnalystFeedback> feedbacks)
    {
        var latestByObservation = observationScores
            .Where(s => s.TargetType == ScoreTargetType.Observation)
            .GroupBy(s => s.TargetId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CalculatedAt).First());

        var sourceReliability = observations.Count == 0 ? 0.0 : observations.Average(o => Clamp01(o.SourceReliability));
        var distinctSources = observations.Select(o => o.SourceConnectorId).Distinct().Count();
        var corroboration = Clamp01((double)distinctSources / 3.0);
        var analystFeedback = ComputeAnalystFeedback(eventCase.Id, nameof(EventCase), feedbacks);

        var observationAverage = observations.Count == 0
            ? 0.0
            : observations.Average(o => latestByObservation.TryGetValue(o.Id, out var score) ? Clamp01(score.Value) : Clamp01(o.SourceReliability));

        var value = Weighted(observationAverage, corroboration, analystFeedback);

        return new ConfidenceScore
        {
            TargetId = eventCase.Id,
            TargetType = ScoreTargetType.EventCase,
            SourceReliability = sourceReliability,
            Corroboration = corroboration,
            AnalystFeedback = analystFeedback,
            Value = value,
            AlgorithmVersion = AlgorithmVersion,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public static double Weighted(double sourceReliability, double corroboration, double analystFeedback)
    {
        return Clamp01(
            SourceReliabilityWeight * Clamp01(sourceReliability)
            + CorroborationWeight * Clamp01(corroboration)
            + AnalystFeedbackWeight * Clamp01(analystFeedback));
    }

    public static double ComputeObservationCorroboration(Observation observation, IEnumerable<Observation> allObservations)
    {
        var independentSources = allObservations
            .Where(other => other.Id == observation.Id || EventCaseHeuristics.AreClose(observation, other))
            .Select(other => other.SourceConnectorId)
            .Distinct()
            .Count();

        return Clamp01((double)independentSources / 3.0);
    }

    public static double ComputeAnalystFeedback(Guid targetId, string targetType, IEnumerable<AnalystFeedback> feedbacks)
    {
        var latest = feedbacks
            .Where(f => f.TargetId == targetId && string.Equals(f.TargetType, targetType, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(f => f.CreatedAt)
            .FirstOrDefault();

        return latest?.Action switch
        {
            FeedbackAction.Confirm => 1.0,
            FeedbackAction.Correct => 0.7,
            FeedbackAction.Invalidate => 0.0,
            FeedbackAction.Note => 0.5,
            _ => 0.5
        };
    }

    public static double Clamp01(double value)
    {
        return Math.Clamp(Math.Round(value, 4, MidpointRounding.AwayFromZero), 0.0, 1.0);
    }
}