# Journal de Livraison — Phase 0 Bis : Réalignement V1

**Date :** 2026-04-25 (initial) · 2026-04-26 (nettoyage .slnx + Class1.cs)
**Statut :** ✅ Phase 0 BIS TERMINÉE — Squelette réaligné sur le corpus V1 officiel

## 0. Contexte

La Phase 0 technique initiale avait introduit un vocabulaire et un modèle de domaine non conformes au corpus V1 officiel (threat intelligence générique au lieu du workbench analyste OSINT). Cette Phase 0 Bis corrige le squelette pour servir de base correcte à la Phase 1.

## 1. Éléments non conformes identifiés

### Domaine (types à supprimer/remplacer)
| Ancien type | Problème | Action |
|---|---|---|
| ThreatIndicator | Hors corpus V1 (cyber threat) | Supprimé → Observation |
| Rule | Hors corpus V1 (moteur de règles) | Supprimé |

### Interfaces applicatives (à supprimer/remplacer)
| Ancienne interface | Problème | Action |
|---|---|---|
| IThreatService | Hors corpus V1 | Supprimé |
| IRuleService | Hors corpus V1 | Supprimé |

### Vues UI (à renommer/restructurer)
| Ancienne vue | Problème | Vue V1 officielle |
|---|---|---|
| ThreatsMap | Hors corpus V1 | MapTimeline (Carte + Timeline) |
| RulesEngine | Hors corpus V1 | EventCase |
| AuditLog | Vue séparée non V1 | Fusionné dans EventCase + Dashboard |

### Worker
| Ancien nom | Action |
|---|---|
| CollectionWorker | Renommé en IngestionWorker |

### Description produit
| Ancien | Nouveau |
|---|---|
| "Système de surveillance et de réponse automatisée aux menaces cyber" | "Workbench analyste desktop — fusion, qualification, traçage et explicabilité d'observations OSINT" |

### KPIs Dashboard
| Ancien | Nouveau |
|---|---|
| "Menaces actives" | "Observations" |
| "Règles actives" | "Événements actifs" |
| "Événements audit" | "Contradictions" |

## 2. Corrections appliquées

### A. Domaine — 11 types V1 officiels créés

| Type | Fichier |
|---|---|
| SourceConnector | `src/AegisLoop.Domain/Entities/SourceConnector.cs` |
| RawItem | `src/AegisLoop.Domain/Entities/RawItem.cs` |
| Observation | `src/AegisLoop.Domain/Entities/Observation.cs` |
| Entity | `src/AegisLoop.Domain/Entities/Entity.cs` |
| EventCase | `src/AegisLoop.Domain/Entities/EventCase.cs` |
| Location | `src/AegisLoop.Domain/Entities/Location.cs` |
| Contradiction | `src/AegisLoop.Domain/Entities/Contradiction.cs` |
| ConfidenceScore | `src/AegisLoop.Domain/Entities/ConfidenceScore.cs` |
| AnalystFeedback | `src/AegisLoop.Domain/Entities/AnalystFeedback.cs` |
| AuditEntry | `src/AegisLoop.Domain/Entities/AuditEntry.cs` (mis à jour) |
| IngestionJob | `src/AegisLoop.Domain/Entities/IngestionJob.cs` |

Enums V1 : `src/AegisLoop.Domain/Enums.cs` (14 enums)

### B. Interfaces V1 créées

**Domain :**
- `ISourceConnector` + `ValidationResult`, `IngestionResult`, `HealthCheckResult`

**Application :**
- `INormalizationService` + `NormalizationResult`
- `IFusionEngine` + `FusionResult`
- `IScoringService`
- `IAnalystFeedbackService`
- `IAuditService` + `AuditQuery` (mis à jour)
- `IGeocodingService`

### C. Suppressions
- `src/AegisLoop.Domain/Entities/ThreatIndicator.cs` — supprimé
- `src/AegisLoop.Domain/Entities/Rule.cs` — supprimé
- `src/AegisLoop.Application/Interfaces/IThreatService.cs` — supprimé
- `src/AegisLoop.Application/Interfaces/IRuleService.cs` — supprimé
- `src/desktop-electron/src/views/ThreatsMap.tsx` — supprimé
- `src/desktop-electron/src/views/RulesEngine.tsx` — supprimé
- `src/desktop-electron/src/views/AuditLog.tsx` — supprimé
- `tests/AegisLoop.Domain.Tests/Entities/ThreatIndicatorTests.cs` — supprimé

### D. Nouveau projet Connectors
- `src/AegisLoop.Connectors/AegisLoop.Connectors.csproj` — créé
- `src/AegisLoop.Connectors/RssConnector.cs` — placeholder V1
- `src/AegisLoop.Connectors/GdeltConnector.cs` — placeholder V1
- `tests/AegisLoop.Connectors.Tests/AegisLoop.Connectors.Tests.csproj` — créé
- `tests/AegisLoop.Connectors.Tests/SmokeTests.cs` — smoke test

### E. Infrastructure mise à jour
- `AegisLoopDbContext` — 11 DbSets V1, mapping complet
- `Dependencies.cs` — inchangé (infrastructure générique)

### F. API mise à jour
- `Program.cs` — ~20 endpoints placeholder V1 conformes au corpus
- Endpoints : `/api/events`, `/api/observations`, `/api/feedback`, `/api/connectors`, `/api/ingestion/*`, `/api/scoring/*`, `/api/dashboard`, `/api/export/*`, `/api/demo/*`, `/api/events/stream`

### G. Worker mis à jour
- `CollectionWorker` → `IngestionWorker`
- `Program.cs` — référence IngestionWorker

### H. UI Frontend — 5 vues V1
- `Dashboard.tsx` — KPIs : Observations, Événements actifs, Contradictions, Score moyen
- `MapTimeline.tsx` — Carte + Timeline
- `EventCase.tsx` — Détail événement
- `Observations.tsx` — Liste, filtres, feedback
- `Settings.tsx` — Configuration connecteurs, scoring, démo

### I. Tests backend
- `ObservationTests.cs` — remplace ThreatIndicatorTests
- Smoke tests inchangés (Application, Infrastructure, Api, E2E)
- Nouveau `Connectors.Tests/SmokeTests.cs`

### J. Solution .sln
- Projets `AegisLoop.Connectors` et `AegisLoop.Connectors.Tests` ajoutés
- Total : 6 src + 6 tests = 12 projets C#

### K. Nettoyage (2026-04-26)
- `AegisLoop.slnx` supprimé — fichier vide (`<Solution></Solution>`), redondant avec `AegisLoop.sln` qui contient tous les projets
- `src/AegisLoop.Domain/Class1.cs` supprimé — fichier fantôme vide sans usage, les 11 types V1 sont dans `Entities/`
- Seul `AegisLoop.sln` est conservé comme fichier de solution

## 3. Structure du repo mise à jour

```
/
├── src/
│   ├── AegisLoop.Domain/
│   │   ├── Enums.cs                    # 14 enums V1
│   │   ├── Entities/                   # 11 types V1
│   │   │   ├── SourceConnector.cs
│   │   │   ├── RawItem.cs
│   │   │   ├── Observation.cs
│   │   │   ├── Entity.cs
│   │   │   ├── EventCase.cs
│   │   │   ├── Location.cs
│   │   │   ├── Contradiction.cs
│   │   │   ├── ConfidenceScore.cs
│   │   │   ├── AnalystFeedback.cs
│   │   │   ├── AuditEntry.cs
│   │   │   └── IngestionJob.cs
│   │   └── Interfaces/
│   │       └── ISourceConnector.cs
│   ├── AegisLoop.Application/
│   │   └── Interfaces/                 # 6 interfaces V1
│   │       ├── INormalizationService.cs
│   │       ├── IFusionEngine.cs
│   │       ├── IScoringService.cs
│   │       ├── IAnalystFeedbackService.cs
│   │       ├── IAuditService.cs
│   │       └── IGeocodingService.cs
│   ├── AegisLoop.Infrastructure/
│   │   ├── Data/AegisLoopDbContext.cs   # 11 DbSets V1
│   │   └── Dependencies.cs
│   ├── AegisLoop.Connectors/
│   │   ├── AegisLoop.Connectors.csproj
│   │   ├── RssConnector.cs
│   │   └── GdeltConnector.cs
│   ├── AegisLoop.Api/
│   │   └── Program.cs                  # ~20 endpoints V1
│   ├── AegisLoop.Worker/
│   │   ├── Program.cs
│   │   └── Services/IngestionWorker.cs
│   └── desktop-electron/
│       └── src/
│           ├── App.tsx                  # 5 vues V1
│           └── views/
│               ├── Dashboard.tsx
│               ├── MapTimeline.tsx
│               ├── EventCase.tsx
│               ├── Observations.tsx
│               └── Settings.tsx
├── tests/
│   ├── AegisLoop.Domain.Tests/
│   │   └── Entities/ObservationTests.cs
│   ├── AegisLoop.Application.Tests/
│   ├── AegisLoop.Infrastructure.Tests/
│   ├── AegisLoop.Connectors.Tests/
│   ├── AegisLoop.Api.Tests/
│   └── AegisLoop.E2E.Tests/
├── docs/
└── README.md
```

## 4. Références entre Projets

```
Domain ← (aucune dépendance)
Application ← Domain
Infrastructure ← Domain
Connectors ← Domain
Api ← Application, Infrastructure
Worker ← Application, Infrastructure
```

## 5. Conclusion

**Phase 0 Bis : ✅ TERMINÉE INTÉGRALEMENT**

Le squelette est maintenant strictement aligné sur le corpus V1 officiel :
- ✅ 11 types du domaine V1 officiels
- ✅ Interfaces V1 (ISourceConnector, INormalizationService, IFusionEngine, IScoringService, IAnalystFeedbackService, IAuditService, IGeocodingService)
- ✅ 5 vues UI V1 (Dashboard, Carte+Timeline, EventCase, Observations, Paramètres)
- ✅ ~20 endpoints API V1 conformes
- ✅ Worker IngestionWorker V1
- ✅ Projet Connectors (RSS + GDELT) ajouté
- ✅ Aucun vocabulaire hors périmètre (ThreatIndicator, Rule, ThreatsMap, RulesEngine supprimés)
- ✅ Solution compile
- ✅ Tests smoke passent

Le terrain est prêt pour la Phase 1 avec le bon produit.