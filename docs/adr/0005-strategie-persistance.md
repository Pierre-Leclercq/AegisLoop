# ADR 0005 — Stratégie de persistance : SQLite

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop est un desktop app mono-utilisateur qui doit persister localement des observations, événements, scores, feedbacks et audit trails. La persistance doit être simple, fiable, sans serveur et compatible avec EF Core.

## Décision

Utiliser **SQLite** via **Entity Framework Core** en mode Code-First avec migrations.

Justification :
- **Zero installation** — Pas de serveur à configurer, un seul fichier .db
- **EF Core natif** — Provider SQLite mature et bien supporté
- **Performance suffisante** — 100k+ observations en local sans problème
- **Mono-utilisateur** — Pas de concurrence d'écriture, adapté au desktop
- **Portabilité** — Un fichier .db copiable, sauvegardable, versionnable
- **Transactions** — Support ACID complet

Stratégie complémentaire :
- EF Core Migrations pour les changements de schéma
- Sauvegarde automatique avant chaque migration
- Purge configurable des RawItems anciens (> 90 jours par défaut)
- VACUUM mensuel automatique
- Index sur les colonnes fréquemment requêtées

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **PostgreSQL** | Performance, PostGIS, concurrence | Serveur à installer et maintenir, sur-ingénierie pour desktop |
| **Fichiers JSON** | Simple, lisible | Pas de requêtes, pas de relations, pas de transactions |
| **LevelDB** | Performance clé-valeur | Pas de requêtes relationnelles, pas de EF Core |
| **LiteDB** | .NET natif, simple | Écosystème moins large, pas d'ORM mature |
| **RocksDB** | Haute performance | Complexe, pas de requêtes, bas niveau |

## Conséquences

- Pas de types géographiques natifs (lat/lon en décimal)
- Pas de concurrence d'écriture (mono-utilisateur uniquement)
- Taille de base < 500 MB pour 100k observations
- Migration vers PostgreSQL possible en V3 si besoin

## Références

- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 10
- [07-glossaire-et-decisions.md](../specs/07-glossaire-et-decisions.md) arbitrage A7