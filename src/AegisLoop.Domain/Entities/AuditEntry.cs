namespace AegisLoop.Domain.Entities;

/// <summary>
/// Entrée du journal d'audit V1.
/// Invariant : append-only, jamais modifié ni supprimé.
/// Catégories : Ingestion, Normalization, Correlation, Scoring, Analyst, Configuration.
/// </summary>
public class AuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditCategory Category { get; set; } = AuditCategory.Ingestion;
    public string Action { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? Details { get; set; }
}