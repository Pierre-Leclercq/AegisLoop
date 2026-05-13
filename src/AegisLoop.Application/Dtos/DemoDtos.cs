namespace AegisLoop.Application.Dtos;

/// <summary>DTO status du mode démo V1.</summary>
public sealed record DemoStatusDto(
    string DatasetVersion,
    string SeedPath,
    bool Loaded,
    int Connectors,
    int RawItems,
    int Observations,
    int EventCases,
    int Scores,
    int Feedbacks,
    int AuditEntries,
    IReadOnlyList<string> Scenarios);

/// <summary>Fichier seed démo V1 complet.</summary>
public sealed record DemoSeedFile(
    string DatasetVersion,
    DateTime GeneratedAt,
    string Description,
    IReadOnlyList<DemoConnectorSeed> Connectors,
    IReadOnlyList<DemoGroupSeed> Groups,
    IReadOnlyList<DemoFeedbackSeed> Feedbacks);

public sealed record DemoConnectorSeed(string Key, string Type, string Name, double Reliability, System.Text.Json.JsonElement Config);
public sealed record DemoGroupSeed(string Scenario, string ScenarioLabel, string Keyword, int Count, string CategoryHint, string TitleBase, string ContentBase, IReadOnlyList<string> ConnectorKeys, DemoLocationSeed? Location = null);
public sealed record DemoLocationSeed(string Name, string Region, string Country, double Latitude, double Longitude);
public sealed record DemoFeedbackSeed(int ObservationIndex, string Action, string Details);