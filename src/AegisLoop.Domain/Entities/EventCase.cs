namespace AegisLoop.Domain.Entities;

/// <summary>
/// Événement / dossier regroupant des observations.
/// Invariant : au moins 1 Observation.
/// </summary>
public class EventCase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public EventCategory Category { get; set; } = EventCategory.Other;
    public EventStatus Status { get; set; } = EventStatus.Detected;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public Guid? LocationId { get; set; }
    public int CorroborationCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}