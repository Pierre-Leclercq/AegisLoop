namespace AegisLoop.Domain.Entities;

/// <summary>
/// Coordonnées géographiques V1.
/// Invariant : Latitude ∈ [-90, 90], Longitude ∈ [-180, 180].
/// </summary>
public class Location
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? GeoJson { get; set; }
    public string SourceType { get; set; } = "Geocoded"; // Geocoded, Manual, Native
}