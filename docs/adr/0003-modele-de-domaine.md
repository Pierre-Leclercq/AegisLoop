# ADR 0003 — Modèle de domaine unifié

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop doit fusionner des données hétérogènes (RSS, GDELT, YouTube, STAC, imports manuels) en un modèle cohérent. Le modèle de domaine est le fondement de toute l'architecture.

## Décision

Adopter un **modèle de domaine unifié** centré sur l'**Observation** comme unité fondamentale, avec les entités clés : SourceConnector, RawItem, Observation, Entity, EntityLink, EventCase, Evidence, Location, Contradiction, ConfidenceScore, AnalystFeedback, Watchlist, AuditEntry.

Principes :
- **Domain-First** — Le modèle est défini dans le projet Domain, zéro dépendance infrastructure
- **Idempotence** — Normalisation et scoring produisent les mêmes résultats pour les mêmes entrées
- **Provenance intégrée** — Chaque Observation porte sa chaîne de provenance
- **Extensibilité** — Dictionnaire Metadata pour les champs non mappables
- **Value objects** — ConnectorConfiguration, ConfidenceScore comme value objects

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **Modèle par source** | Spécifique, pas de mapping | Pas de fusion possible, duplication |
| **Document store (schemaless)** | Flexible, pas de schéma fixe | Pas de validation, pas de relations, difficile à corréler |
| **Event sourcing** | Audit natif, replay | Complexité élevée, sur-ingénierie pour MVP |
| **Graph database model** | Relations naturelles | Trop complexe, persistance difficile en local |

## Justification

1. **Fusion possible** — Un modèle unifié est la condition sine qua non de la corrélation
2. **Domain pur** — Pas de dépendance EF Core ni SQLite dans le domaine
3. **Testabilité** — Le domaine est testable sans aucune infrastructure
4. **Provenance native** — Chaque élément trace son origine par construction
5. **Extensibilité** — Le dictionnaire Metadata permet d'ajouter des champs sans migration
6. **SOLID** — Interfaces du domaine, injection de dépendances, séparation des responsabilités

## Conséquences

- Mapping nécessaire entre chaque format source et le modèle unifié
- Certains champs source seront dans Metadata (perte de typage fort)
- Le modèle doit être stable — les changements impactent toute l'architecture

## Références

- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 4
- [01-specs-fonctionnelles.md](../specs/01-specs-fonctionnelles.md) section 3