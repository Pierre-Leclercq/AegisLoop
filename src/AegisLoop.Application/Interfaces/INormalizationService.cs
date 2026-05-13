using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Service de normalisation V1 — transforme un RawItem en Observation.
/// Pipeline : Parsing → Mapping → Enrichissement → Déduplication → Persistance.
/// </summary>
public interface INormalizationService
{
    Task<NormalizationResult> NormalizeAsync(RawItem rawItem);
}

/// <summary>
/// Résultat de la normalisation d'un RawItem.
/// </summary>
public record NormalizationResult(
    bool Success,
    Observation? Observation,
    string? ErrorMessage
);