using AegisLoop.Domain.Entities;

namespace AegisLoop.Application.Interfaces;

/// <summary>
/// Service de géocodage V1 — Nominatim (OSM) avec cache local.
/// </summary>
public interface IGeocodingService
{
    Task<Location?> GeocodeAsync(string placeName);
    Task<string?> ReverseGeocodeAsync(double latitude, double longitude);
}