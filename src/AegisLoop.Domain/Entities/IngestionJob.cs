namespace AegisLoop.Domain.Entities;

/// <summary>
/// Trace d'une exécution de connecteur.
/// Statuts : Planned, Running, Completed, Failed, Cancelled.
/// </summary>
public class IngestionJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Planned;
    public int ItemsCollected { get; set; } = 0;
    public int ItemsNormalized { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}