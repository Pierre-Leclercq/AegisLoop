namespace AegisLoop.Domain.Entities;

/// <summary>
/// Conflit entre observations (simplifié V1).
/// Invariant : Observation1Id ≠ Observation2Id.
/// V1 : pas de workflow de résolution complexe, pas d'historique de résolution.
/// </summary>
public class Contradiction
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ContradictionType Type { get; set; } = ContradictionType.Factual;
    public Guid Observation1Id { get; set; }
    public Guid Observation2Id { get; set; }
    public Guid EventCaseId { get; set; }
    public string? Description { get; set; }
    public ContradictionStatus Status { get; set; } = ContradictionStatus.Open;
}