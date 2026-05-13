namespace AegisLoop.Domain.Entities;

/// <summary>
/// Connecteur OSINT configuré et actif (inclut Config).
/// V1 : RSS + GDELT uniquement. ConnectorConfiguration fusionné (JSON).
/// </summary>
public class SourceConnector
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ConnectorType ConnectorType { get; set; } = ConnectorType.Rss;
    public string Name { get; set; } = string.Empty;
    public string Config { get; set; } = "{}"; // JSON polymorphique par type
    public ConnectorStatus Status { get; set; } = ConnectorStatus.Inactive;
    public DateTime? LastRunAt { get; set; }
    public int ErrorCount { get; set; } = 0;
}