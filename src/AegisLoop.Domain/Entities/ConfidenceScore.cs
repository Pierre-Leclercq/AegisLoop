namespace AegisLoop.Domain.Entities;

/// <summary>
/// Score explicable à 3 composantes V1.
/// Score = W1 × FiabilitéSource + W2 × Corroboration + W3 × FeedbackAnalyste
/// Invariant : Value ∈ [0.0, 1.0].
/// V1 : Fraîcheur et Complétude exclues.
/// </summary>
public class ConfidenceScore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TargetId { get; set; }
    public ScoreTargetType TargetType { get; set; } = ScoreTargetType.Observation;
    public double Value { get; set; } = 0.0;
    public double SourceReliability { get; set; } = 0.0;
    public double Corroboration { get; set; } = 0.0;
    public double AnalystFeedback { get; set; } = 0.0;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public string AlgorithmVersion { get; set; } = "1.0";
}