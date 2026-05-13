namespace AegisLoop.Domain.Entities;

/// <summary>
/// Entité nommée extraite (Location, Organization, Person).
/// V1 : pas de type Event ni Keyword. EntityLink repoussé V2.
/// Invariant : NormalizedName toujours en minuscules, sans accents.
/// </summary>
public class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public EntityType Type { get; set; } = EntityType.Location;
    public string? AttributesJson { get; set; } // Dictionnaire extensible sérialisé
}