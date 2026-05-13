namespace AegisLoop.Domain.Entities;

/// <summary>
/// Action de l'analyste (append-only après fenêtre d'annulation de 5 min).
/// Invariant : immutable après la fenêtre d'annulation.
/// V1 : actions Confirm, Invalidate, Correct uniquement (pas Merge, Split, Enrich).
/// </summary>
public class AnalystFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TargetId { get; set; }
    public string TargetType { get; set; } = string.Empty; // "Observation" ou "EventCase"
    public FeedbackAction Action { get; set; } = FeedbackAction.Confirm;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsCancelable { get; set; } = true; // true pendant 5 min
}