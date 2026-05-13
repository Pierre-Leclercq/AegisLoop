namespace AegisLoop.Domain.Entities;

/// <summary>
/// Unité normalisée — centre de gravité du domaine V1.
/// Claim fusionné (champ ClaimText). Evidence = Observation en V1.
/// Invariant : liée à un RawItem ou marquée manuelle.
/// </summary>
public class Observation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? RawItemId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ClaimText { get; set; }
    public double ClaimConfidence { get; set; } = 0.0;
    public ObservationType Type { get; set; } = ObservationType.Article;
    public ObservationStatus Status { get; set; } = ObservationStatus.New;
    public DateTime ObservedAt { get; set; } = DateTime.UtcNow;
    public Guid SourceConnectorId { get; set; }
    public Guid? EventCaseId { get; set; }
    public string? SourceUrl { get; set; }
    public double SourceReliability { get; set; } = 0.3;
    public string? MetadataJson { get; set; } // Dictionnaire extensible sérialisé
    public string? Language { get; set; }
    public Guid? GeoLocationId { get; set; }
    public bool IsManual { get; set; } = false;
}