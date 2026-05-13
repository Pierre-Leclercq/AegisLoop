namespace AegisLoop.Domain;

/// <summary>
/// Constantes métier V1 — pas de magic strings dans le code.
/// </summary>
public static class Constants
{
    public const string SystemActor = "system";
    public const string AnalystLocalActor = "analyst-local";
    public const string DemoSystemActor = "system-demo";
    public const string DemoSeedVersion = "v1-seed-2026-04";
    public const string DemoSeedPrefix = "aegisloop-demo-v1:";
    public const string DefaultRssFeedUrl = "https://www.nasa.gov/news-release/feed/";
    public const string DefaultGdeltQuery = "Sudan conflict";
    public const string RssDemoConnectorName = "RSS Démo";
    public const string GdeltDemoConnectorName = "GDELT Démo";
    public const string UnknownSourceName = "Source inconnue";
    public const string UnknownSourceType = "Unknown";
    public const string AlgorithmVersion = "V1-heuristic-3c-2026-04";
}