using AegisLoop.Domain;
using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Service d'audit V1 — journal append-only, jamais modifié ni supprimé.
/// Catégories : Ingestion, Normalization, Correlation, Scoring, Analyst, Configuration.
/// </summary>
public interface IAuditService
{
    Task LogAsync(AuditCategory category, string action, Guid? targetId, string? targetType, object? details);
    Task<IReadOnlyList<AuditEntry>> QueryAsync(AuditQuery query);
    Task ExportAsync(string filePath);
}

/// <summary>
/// Paramètres de requête d'audit V1.
/// </summary>
public record AuditQuery(
    AuditCategory? Category = null,
    DateTime? Since = null,
    DateTime? Until = null,
    int Skip = 0,
    int Take = 50
);