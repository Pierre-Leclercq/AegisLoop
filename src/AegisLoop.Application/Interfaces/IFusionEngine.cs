using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Moteur de fusion/corrélation V1 — clustering d'observations en événements.
/// Méthode : similarité textuelle + proximité temporelle + géographique + entités communes.
/// </summary>
public interface IFusionEngine
{
    Task<FusionResult> CorrelateAsync(IEnumerable<Observation> observations, CancellationToken cancellationToken = default);
}

/// <summary>
/// Résultat de la corrélation/fusion.
/// </summary>
public record FusionResult(
    IReadOnlyList<EventCase> NewEvents,
    IReadOnlyList<EventCase> UpdatedEvents
);