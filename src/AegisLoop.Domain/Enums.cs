namespace AegisLoop.Domain;

/// <summary>
/// Types de connecteur OSINT V1.
/// </summary>
public enum ConnectorType
{
    Rss,
    Gdelt
}

/// <summary>
/// Statut d'un SourceConnector V1.
/// </summary>
public enum ConnectorStatus
{
    Active,
    Inactive,
    Error,
    Paused
}

/// <summary>
/// Format de contenu d'un RawItem V1.
/// </summary>
public enum RawContentType
{
    Xml,
    Json,
    Html,
    Csv
}

/// <summary>
/// Statut d'une Observation V1.
/// </summary>
public enum ObservationStatus
{
    New,
    Confirmed,
    Invalidated,
    Contradicted
}

/// <summary>
/// Type d'Observation V1.
/// </summary>
public enum ObservationType
{
    Article,
    Report,
    GeospatialMetadata
}

/// <summary>
/// Type d'entité nommée V1.
/// </summary>
public enum EntityType
{
    Location,
    Organization,
    Person
}

/// <summary>
/// Catégorie d'un EventCase V1.
/// </summary>
public enum EventCategory
{
    Conflict,
    Disaster,
    Political,
    Economic,
    Social,
    Environmental,
    Other
}

/// <summary>
/// Statut d'un EventCase V1.
/// </summary>
public enum EventStatus
{
    Detected,
    Confirmed,
    InProgress,
    Closed,
    Archived,
    Invalidated
}

/// <summary>
/// Type de contradiction V1.
/// </summary>
public enum ContradictionType
{
    Temporal,
    Factual,
    Geographic
}

/// <summary>
/// Statut d'une contradiction V1.
/// </summary>
public enum ContradictionStatus
{
    Open,
    Resolved,
    Dismissed
}

/// <summary>
/// Action de feedback analyste V1.
/// </summary>
public enum FeedbackAction
{
    Confirm,
    Invalidate,
    Correct,
    Note
}

/// <summary>
/// Cible du score de confiance V1.
/// </summary>
public enum ScoreTargetType
{
    Observation,
    EventCase
}

/// <summary>
/// Statut d'un IngestionJob V1.
/// </summary>
public enum JobStatus
{
    Planned,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Catégorie d'audit V1.
/// </summary>
public enum AuditCategory
{
    Ingestion,
    Normalization,
    Correlation,
    Scoring,
    Analyst,
    Configuration
}