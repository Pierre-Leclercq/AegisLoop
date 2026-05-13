using AegisLoop.Domain.Entities;

namespace AegisLoop.Domain.Interfaces;

/// <summary>
/// Interface commune pour tous les connecteurs OSINT V1.
/// Chaque connecteur DOIT implémenter cette interface.
/// V1 : RSS + GDELT uniquement.
/// </summary>
public interface ISourceConnector
{
    string ConnectorType { get; }
    Task<ValidationResult> ValidateConfigAsync(string configJson);
    Task<IngestionResult> CollectAsync(string configJson, DateTime? since);
    Task<HealthCheckResult> HealthCheckAsync(string configJson);
}

/// <summary>
/// Résultat de la validation de configuration d'un connecteur.
/// </summary>
public record ValidationResult(bool IsValid, string? ErrorMessage = null);

/// <summary>
/// Résultat d'une collecte par un connecteur.
/// </summary>
public record IngestionResult(
    bool Success,
    int ItemsCollected,
    IReadOnlyList<RawItem> Items,
    IReadOnlyList<string> Errors
);

/// <summary>
/// Résultat du health check d'un connecteur.
/// </summary>
public record HealthCheckResult(bool IsHealthy, string? ErrorMessage = null);