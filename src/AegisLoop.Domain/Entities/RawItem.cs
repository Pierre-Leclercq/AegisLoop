namespace AegisLoop.Domain.Entities;

/// <summary>
/// Donnée brute avant normalisation, collectée par un connecteur OSINT.
/// Invariant : SourceHash non vide (SHA-256 pour déduplication).
/// </summary>
public class RawItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public string RawContent { get; set; } = string.Empty;
    public RawContentType ContentType { get; set; } = RawContentType.Xml;
    public string SourceHash { get; set; } = string.Empty; // SHA-256
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAt { get; set; }
    public string? SourceUrl { get; set; }
}