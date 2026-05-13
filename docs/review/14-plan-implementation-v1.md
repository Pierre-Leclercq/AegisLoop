# Aegis Loop — Plan d'implémentation V1

> **Version :** 1.0  
> **Date :** 2026-04-25  
> **Source de vérité :** [10-mvp-solo-v1-officiel.md](10-mvp-solo-v1-officiel.md)  
> **Statut :** En attente de validation

---

## 1. Cartographie des 17 items de backlog vers les phases

| ID | Backlog Item | Phase Impl. | Justification |
|---|---|---|---|
| BL-01 | Modèle de domaine (11 types) | Phase 1 | Fondation — tout dépend du domaine |
| BL-02 | Connecteur RSS/Atom | Phase 3 | Nécessite Domain + Infrastructure |
| BL-03 | Connecteur GDELT | Phase 3 | Même couche que RSS |
| BL-04 | Pipeline ingestion | Phase 2→3 | Use case dans Application, connecteurs en Phase 3 |
| BL-05 | Normalisation | Phase 2 | Service Application, dépend du Domain |
| BL-06 | Détection d'entités | Phase 2 | Service Application, partie de la normalisation |
| BL-07 | Fusion/correlation | Phase 2 | Service Application, cœur du pipeline |
| BL-08 | Scoring heuristique | Phase 2 | Service Application, dépend du Domain |
| BL-09 | Dashboard | Phase 5 | Vue GUI |
| BL-10 | Vue EventCase | Phase 5 | Vue GUI |
| BL-11 | Feedback analyste | Phase 2 | Service Application, dépend du scoring |
| BL-12 | Mode démo + seed data | Phase 6 | Intégration finale |
| BL-13 | Export Markdown/JSON | Phase 4→6 | Endpoint API en Phase 4, polish en Phase 6 |
| BL-14 | Audit trail | Phase 2 | Service Application, transversal |
| BL-15 | Tags/notes simples | Phase 4 | API + persistance, GUI en Phase 5 |
| BL-16 | Contradictions | Phase 2 | Détection dans Application, GUI en Phase 5 |
| BL-17 | Configuration connecteurs | Phase 4 | Endpoint API, GUI en Phase 5 |

---

## 2. Plan d'implémentation par phase

### PHASE 0 — Initialisation technique

**Objectif :** Solution compilable, repo structuré, CI verte, conventions établies.

**Fichiers à créer :**

```
/src/AegisLoop.Domain/AegisLoop.Domain.csproj
/src/AegisLoop.Application/AegisLoop.Application.csproj
/src/AegisLoop.Infrastructure/AegisLoop.Infrastructure.csproj
/src/AegisLoop.Connectors/AegisLoop.Connectors.csproj
/src/AegisLoop.Api/AegisLoop.Api.csproj
/src/AegisLoop.Worker/AegisLoop.Worker.csproj
/src/AegisLoop.sln

/tests/AegisLoop.Domain.Tests/AegisLoop.Domain.Tests.csproj
/tests/AegisLoop.Application.Tests/AegisLoop.Application.Tests.csproj
/tests/AegisLoop.Infrastructure.Tests/AegisLoop.Infrastructure.Tests.csproj
/tests/AegisLoop.Api.Tests/AegisLoop.Api.Tests.csproj
/tests/AegisLoop.E2E.Tests/AegisLoop.E2E.Tests.csproj

/desktop-electron/package.json
/desktop-electron/tsconfig.json
/desktop-electron/vite.config.ts
/desktop-electron/src/main.ts
/desktop-electron/src/preload.ts
/desktop-electron/src/App.tsx

/.editorconfig
/.gitignore (mis à jour)
/.github/workflows/build.yml
```

**Conventions de code :**
- C# : nullable reference types activé, warnings as errors sur NRT, StyleCop analyzers minimaux
- TS : strict mode, ESLint + Prettier
- Nommage : PascalCase C#, camelCase TS
- GUIDs comme identifiants principaux
- DateTime UTC partout, stocké en ISO 8601

**Livrables :**
- `dotnet build` réussit sur les 6 projets + 5 projets de tests
- `npm install` réussit dans desktop-electron
- CI GitHub Actions verte (build only)
- README technique de démarrage

---

### PHASE 1 — Noyau domaine

**Objectif :** Les 11 types du domaine avec leurs invariants, énumérations, interfaces clés, value objects. Tout testé unitairement.

**Fichiers à créer (AegisLoop.Domain) :**

```
/Enums/
  ConnectorType.cs       (Rss, Gdelt)
  ConnectorStatus.cs     (Active, Inactive, Error, Paused)
  ObservationStatus.cs   (New, Confirmed, Invalidated, Contradicted)
  ObservationType.cs     (Article, Report, GeospatialMetadata)
  EntityType.cs          (Location, Organization, Person)
  EventCategory.cs       (Conflict, Disaster, Political, Economic, Social, Environmental, Other)
  EventStatus.cs         (Detected, Confirmed, InProgress, Closed, Archived, Invalidated)
  ContradictionType.cs   (Temporal, Factual, Geographic)
  ContradictionStatus.cs (Open, Resolved, Dismissed)
  FeedbackAction.cs      (Confirm, Invalidate, Correct)
  AuditCategory.cs       (Ingestion, Normalization, Correlation, Scoring, Analyst, Configuration)
  JobStatus.cs           (Planned, Running, Completed, Failed, Cancelled)
  ScoreTargetType.cs     (Observation, EventCase)

/Entities/
  SourceConnector.cs
  RawItem.cs
  Observation.cs
  Entity.cs
  EventCase.cs
  Location.cs
  Contradiction.cs
  ConfidenceScore.cs
  AnalystFeedback.cs
  AuditEntry.cs
  IngestionJob.cs

/ValueObjects/
  ScoringWeights.cs      (W1=0.35, W2=0.35, W3=0.30)
  GeoLocation.cs         (Latitude, Longitude — value object)

/Interfaces/
  ISourceConnector.cs
  INormalizationService.cs
  IFusionEngine.cs
  IScoringService.cs
  IAnalystFeedbackService.cs
  IAuditService.cs
  IGeocodingService.cs
  ISourceReliabilityRegistry.cs
  IRepository.cs         (genérique minimal)
  IObservationRepository.cs
  IEventCaseRepository.cs

/Common/
  ValidationResult.cs
  IngestionResult.cs
  NormalizationResult.cs
  FusionResult.cs
  HealthCheckResult.cs
  AuditQuery.cs
  ApiEnvelope.cs
```

**Invariants implémentés (constructeurs avec validation) :**
1. EventCase : au moins 1 Observation à la création
2. ConfidenceScore.Value ∈ [0.0, 1.0] — clamp
3. RawItem : SourceHash non vide
4. Observation : RawItemId non vide ou IsManual=true
5. AnalystFeedback : immutable après 5 min (méthode Cancel vérifie la fenêtre)
6. AuditEntry : pas de setter après construction
7. Contradiction : Observation1Id ≠ Observation2Id
8. Location : Latitude ∈ [-90, 90], Longitude ∈ [-180, 180]
9. SourceConnector actif : Config non null/empty
10. EventCase.Score = moyenne pondérée observations × FacteurCorroboration

**Fichiers de tests (AegisLoop.Domain.Tests) :**
- Les 15 tests domaine listés dans 05-plan-de-tests.md
- Chaque invariant = au moins 1 test

**Livrables :**
- `dotnet build` réussit
- `dotnet test` sur Domain.Tests réussit (15 tests verts)
- Aucune dépendance extérieure dans Domain (0 package NuGet hormis xUnit pour les tests)

---

### PHASE 2 — Application / Use Cases V1

**Objectif :** Services applicatifs, pipeline ingestion, normalisation, fusion, scoring, feedback, contradictions, audit.

**Fichiers à créer (AegisLoop.Application) :**

```
/Scoring/
  ConfidenceScoringService.cs     (IScoringService)
  SourceReliabilityRegistry.cs    (ISourceReliabilityRegistry)
  ScoringWeights.cs               (value object, déjà dans Domain → config ici)

/Normalization/
  RssNormalizationService.cs     (INormalizationService spécialisé RSS)
  GdeltNormalizationService.cs   (INormalizationService spécialisé GDELT)
  NormalizationPipeline.cs       (orchestre parsing → mapping → enrichissement)

/Enrichment/
  EntityDetectionService.cs      (dictionnaire + regex)
  GeocodingEnrichmentService.cs  (appelle IGeocodingService)

/Fusion/
  FusionEngine.cs                (IFusionEngine — clustering heuristique)
  ContradictionDetector.cs       (détection automatique)

/Feedback/
  AnalystFeedbackService.cs      (IAnalystFeedbackService)

/Audit/
  AuditService.cs                (IAuditService)

/Pipeline/
  IngestionPipeline.cs           (orchestration collecte → normalisation → fusion → scoring)

/DTOs/
  EventDto.cs
  ObservationDto.cs
  ConnectorDto.cs
  DashboardDto.cs
  ProvenanceDto.cs
  ScoreBreakdownDto.cs
  FeedbackRequestDto.cs
  ExportDto.cs
  IngestionJobDto.cs
```

**Fichiers de tests (AegisLoop.Application.Tests) :**
- 10 tests scoring
- 10 tests normalisation (avec mocks des connecteurs)
- 10 tests fusion/correlation
- 5 tests pipeline ingestion
- Tests feedback, contradiction, audit

**Livrables :**
- `dotnet build` réussit
- `dotnet test` Domain + Application réussit (~50 tests)
- Scoring vérifié avec 3 composantes
- Normalisation RSS + GDELT vérifiée (avec stubs)
- Fusion vérifiée avec données de test minimales
- Feedback modifie le score

---

### PHASE 3 — Infrastructure V1

**Objectif :** Persistance SQLite/EF Core, repositories, connecteurs RSS et GDELT réels, géocodage, configuration, logging, seed/replay provider.

**Fichiers à créer (AegisLoop.Infrastructure) :**

```
/Persistence/
  AegisLoopDbContext.cs
  DbSeeds.cs
  Migrations/ (EF Core migrations)

/Repositories/
  ObservationRepository.cs
  EventCaseRepository.cs
  SourceConnectorRepository.cs
  RawItemRepository.cs
  AuditEntryRepository.cs

/Configuration/
  AppSettings.cs
  ConfigurationProvider.cs

/Geocoding/
  NominatimGeocodingService.cs  (IGeocodingService)
  GeocodeCache.cs

/Logging/
  SerilogConfiguration.cs
```

**Fichiers à créer (AegisLoop.Connectors) :**

```
/Rss/
  RssSourceConnector.cs          (ISourceConnector)
  RssConfig.cs
  RssParser.cs

/Gdelt/
  GdeltSourceConnector.cs       (ISourceConnector)
  GdeltConfig.cs
  GdeltParser.cs

/Common/
  RateLimiter.cs
  CircuitBreaker.cs
  RetryPolicy.cs

/Seed/
  SeedDataProvider.cs            (2 scénarios, 90 observations)
  SudanCrisisSeed.cs
  GulfOfAdenSeed.cs
```

**Fichiers de tests :**
- AegisLoop.Infrastructure.Tests : tests EF Core, migrations, repositories, géocodage
- Tests contrat connecteurs (5 tests) — dans un dossier ou projet dédié

**Livrables :**
- SQLite DB créable via migrations
- RSS connector parse un flux réel (ou fixture)
- GDELT connector interroge l'API (ou mock)
- Seed data provider retourne 90 observations structurées
- `dotnet test` Domain + Application + Infrastructure + Connectors réussit

---

### PHASE 4 — API locale + Worker

**Objectif :** ~20 endpoints REST, cycle de vie backend, orchestration ingestion, worker d'ingestion.

**Fichiers à créer (AegisLoop.Api) :**

```
/Program.cs                      (host, DI, middleware, CORS)
/Endpoints/
  HealthEndpoints.cs             (GET /health)
  EventEndpoints.cs              (GET/POST/PATCH /api/events)
  ObservationEndpoints.cs        (GET /api/observations, GET provenance)
  FeedbackEndpoints.cs          (POST /api/feedback)
  ConnectorEndpoints.cs         (GET/POST /api/connectors)
  IngestionEndpoints.cs         (POST /api/ingestion/run, GET jobs)
  ScoringEndpoints.cs           (GET /api/scoring/{id}/breakdown)
  DashboardEndpoints.cs         (GET /api/dashboard)
  ExportEndpoints.cs            (GET /api/export/{id})
  DemoEndpoints.cs              (POST /api/demo/load, POST /api/demo/reset)
  SSEndpoints.cs                (GET /api/events/stream)

/Middleware/
  ExceptionHandlingMiddleware.cs
  CorrelationIdMiddleware.cs

/Configuration/
  ApiConfiguration.cs
```

**Fichiers à créer (AegisLoop.Worker) :**

```
/Program.cs                      (host worker)
/Services/
  IngestionHostedService.cs      (BackgroundService, polling planifié)
  ConnectorScheduler.cs         (planification des collectes)
```

**Fichiers de tests (AegisLoop.Api.Tests) :**
- Tests d'intégration avec WebApplicationFactory (~20 tests)

**Livrables :**
- `dotnet run` sur Api démarre sur localhost:5100
- GET /health → 200
- Tous les endpoints CRUD fonctionnels
- Worker démarre et planifie les collectes
- SSE envoie des notifications basiques
- `dotnet test` Domain + Application + Infrastructure + Api réussit

---

### PHASE 5 — GUI V1

**Objectif :** Shell Electron, 5 vues, 10 composants, navigation, raccourcis, intégration backend.

**Fichiers à créer (desktop-electron) :**

```
/src/main.ts                     (spawn .NET, health check, lifecycle)
/src/preload.ts                  (IPC bridge)
/src/renderer/
  App.tsx                        (routing, layout)
  main.tsx                       (entry point React)

  /api/
    apiClient.ts                 (HTTP client vers localhost:5100)
    types.ts                      (DTOs TypeScript)

  /hooks/
    useEvents.ts
    useObservations.ts
    useConnectors.ts
    useDashboard.ts
    useKeyboardShortcuts.ts

  /views/
    DashboardView.tsx
    MapTimelineView.tsx
    EventCaseView.tsx
    ObservationsView.tsx
    SettingsView.tsx

  /components/
    ObservationCard.tsx           (Must)
    EventCaseHeader.tsx           (Must)
    ConfidenceScoreBadge.tsx      (Must)
    ProvenanceChain.tsx           (Must)
    TimelineSimple.tsx            (Must)
    MapContainer.tsx              (Must)
    FeedbackPanel.tsx             (Must)
    ContradictionAlert.tsx        (Should)
    SourceBadge.tsx               (Should)
    FilterBar.tsx                 (Should)

  /layout/
    NavBar.tsx                    (navigation verticale)
    StatusBar.tsx                 (barre de statut basse)
    TitleBar.tsx                  (recherche rapide Ctrl+K)

  /lib/
    keyboardShortcuts.ts          (9 raccourcis)
```

**Livrables :**
- Electron lance et spawn le backend .NET
- Les 5 vues sont navigables (Ctrl+1 à Ctrl+5)
- Dashboard affiche KPIs et événements depuis l'API
- EventCase affiche observations, provenance, score décomposé
- Observations affiche liste filtrée avec feedback
- Paramètres affiche configuration connecteurs + mode démo
- 9 raccourcis clavier fonctionnels
- Mode sombre par défaut

---

### PHASE 6 — Démo prête

**Objectif :** Seed data complète, scénario de démo, export fonctionnel, polish, smoke tests, corrections.

**Fichiers à créer/modifier :**

```
/examples/seed-data/
  sudan-crisis.json              (50 observations, 5 événements, 2 contradictions)
  gulf-of-aden.json              (40 observations, 3 événements, 1 contradiction)

/tests/AegisLoop.E2E.Tests/
  SmokeTests.cs                  (5 smoke tests Playwright)
```

**Actions :**
- Compléter le contenu des 90 observations seed (texte réaliste français/anglais)
- Vérifier le parcours démo complet : load → dashboard → EventCase → feedback → export
- Export Markdown et JSON fonctionnels et vérifiés
- Polish UI : états vides, loading, erreurs
- Corriger les bugs bloquants
- Smoke test 5 min sans crash
- Mise à jour README technique

**Livrables :**
- Mode démo charge en < 5s
- Scénario Soudan complet et crédible
- Scénario Golfe d'Aden complet et crédible
- Export Markdown/JSON depuis la GUI
- Smoke tests verts
- README de démarrage complet
- Les 12 conditions "Démo prête" sont toutes remplies

---

## 3. 10 tests critiques — Planification

| # | Test Critique | Phase | Projet de test |
|---|---|---|---|
| 1 | Normalisation RSS → Observation | Phase 2 | Application.Tests |
| 2 | Normalisation GDELT → Observation | Phase 2 | Application.Tests |
| 3 | Scoring avec 3 composantes → score ∈ [0, 1] | Phase 2 | Application.Tests |
| 4 | Fusion d'observations en événement | Phase 2 | Application.Tests |
| 5 | Détection de contradiction | Phase 2 | Application.Tests |
| 6 | Feedback analyste modifie le score | Phase 2 | Application.Tests |
| 7 | Pipeline ingestion bout en bout | Phase 2→3 | Application.Tests |
| 8 | API CRUD observations | Phase 4 | Api.Tests |
| 9 | Contrat ISourceConnector respecté | Phase 3 | Connectors (tests contrat) |
| 10 | Smoke test démo 5 min | Phase 6 | E2E.Tests |

**Priorité d'implémentation :** Les tests 1-6 sont implémentés dès la Phase 2. Les tests 7-9 en Phase 3-4. Le test 10 en Phase 6.

---

## 4. Arbitrages sur les 6 points ouverts

### PO-1 — Schéma JSON Schema pour l'export

**Décision :** Pas de JSON Schema formel en V1. L'export JSON suit la structure naturelle des DTOs existants (EventDto, ObservationDto, ProvenanceDto). Le schéma est implicite dans les types TypeScript/C#. Si un schéma formel est requis, il sera ajouté en V2.

**Justification :** Le format est défini par les DTO. Un schéma formel n'apporte rien à la démo. L'option la plus simple.

---

### PO-2 — Format exact du fichier user config

**Décision :** `appsettings.json` pour la base + `appsettings.local.json` pour les overrides (ignoré par Git). Configuration des connecteurs en JSON dans la DB SQLite (SourceConnector.Config). Pas de fichier config séparé.

**Format appsettings.json :**
```json
{
  "AegisLoop": {
    "Api": { "Port": 5100 },
    "Scoring": {
      "Weights": { "SourceReliability": 0.35, "Corroboration": 0.35, "AnalystFeedback": 0.30 }
    },
    "Geocoding": { "CacheTtlDays": 30 },
    "Audit": { "PurgeAfterDays": 90 },
    "Demo": { "AutoLoad": false }
  }
}
```

**Justification :** Standard .NET, pas de format custom, hiérarchie claire. L'option la plus simple compatible avec la configuration .NET.

---

### PO-3 — Composant timeline exact (lib vs custom)

**Décision :** Composant custom simple (TimelineSimple). Pas de lib externe. Un composant React léger : barre horizontale avec marqueurs temporels, sélection de plage par drag, tooltip au survol.

**Justification :** Les libs de timeline (vis-timeline, react-calendar-timeline) sont surdimensionnées pour V1. Un composant custom de ~150 lignes TSX suffit. L'option la plus simple, sans dépendance exotique.

---

### PO-4 — Stratégie de géocodage offline

**Décision :** Nominatim HTTP + cache SQLite (TTL 30 jours). En mode démo, le cache est pré-rempli avec les lieux des seed data. Si Nominatim est indisponible, les lieux restent sans coordonnées (pas de crash, pas de blocage). Pas de GeoNames locale en V1.

**Justification :** Nominatim + cache est la stratégie définie dans les specs V1. Le pré-remplissage du cache suffit pour la démo. GeoNames locale = V2. L'option la plus simple compatible avec le mode offline de la démo.

---

### PO-5 — Contenu exact des 90 observations seed

**Décision :** Les observations seed seront rédigées en Phase 6 avec les caractéristiques suivantes :

**Scénario 1 — Crise au Soudan (50 observations) :**
- Sources : RSS LeMonde (15), RSS BBC (15), GDELT (20)
- 5 événements : Combats Khartoum (15 obs), Déplacement populations (10 obs), Coupure humanitaire (8 obs), Déclarations diplomatiques (10 obs), Incertitude sur le bilan (7 obs)
- 2 contradictions : Bilan humain divergent (LeMonde: 500 morts vs BBC: 200 morts), Localisation combats (Nord Khartoum vs Centre Khartoum)
- Dates : échantillonnées sur 2 semaines (avril 2026)
- Lieux : Khartoum, Omdurman, Darfour, camps de réfugiés
- Langues : français (LeMonde), anglais (BBC, GDELT)

**Scénario 2 — Golfe d'Aden (40 observations) :**
- Sources : RSS maritime Lloyd's List (20), GDELT maritime (20)
- 3 événements : Attaque navire (15 obs), Réponse internationale (12 obs), Trafic dévié (13 obs)
- 1 contradiction : Nature de l'attaque (missile vs drone)
- Coordonnées : Golfe d'Aden (12°N, 45°E)
- Langues : anglais

**Justification :** La structure est définie par les specs. Le contenu exact sera réaliste mais synthétique (pas de vraies données sensibles). Assez détaillé pour la démo, pas trop pour la rédaction.

---

### PO-6 — Politique de purge AuditEntry

**Décision :** Purge automatique des AuditEntries de plus de 90 jours, configurable via `appsettings.json` (`Audit:PurgeAfterDays`). La purge s'exécute au démarrage du Worker et une fois par jour. Aucune purge manuelle en V1 (sauf réinitialisation démo qui vide tout).

**Justification :** 90 jours par défaut est raisonnable pour un desktop app. Configurable pour le cas d'usage long-terme. Simple à implémenter (une requête DELETE avec WHERE Timestamp < @cutoff). L'option la plus simple.

---

## 5. Risques et mitigations

| Risque | Probabilité | Impact | Mitigation |
|---|---|---|---|
| GDELT API instable ou changée | Moyen | Moyen | Circuit breaker, mode démo sans réseau, tests avec fixtures |
| MapLibre GL JS + Electron compatibilité | Faible | Moyen | Tester tôt en Phase 5, fallback liste si carte ne marche pas |
| Performance SQLite avec 90+ observations | Très faible | Faible | SQLite gère 100k+ lignes facilement, index sur les colonnes clés |
| Electron spawn .NET cross-platform | Moyen | Moyen | Tester sur Windows en priorité, macOS/Linux en CI |
| Surcharge de travail GUI | Moyen | Élevé | Prioriser les Must, les Should sont optionnels pour la démo |

---

## 6. Dépendances entre phases

```
Phase 0 ──→ Phase 1 ──→ Phase 2 ──→ Phase 3 ──→ Phase 4 ──→ Phase 5 ──→ Phase 6
  (scaffolding)  (domaine)  (services)  (infra+     (API+      (GUI)     (démo)
                                              connecteurs)  worker)
```

Chaque phase produit un état compilable et testable. On ne passe à la phase suivante que quand la phase courante a tous ses tests verts.

---

## 7. Critères de fin de phase

Chaque phase est terminée quand :
1. ✅ Tous les fichiers prévus sont créés et compilent
2. ✅ Tous les tests de la phase sont verts
3. ✅ `dotnet build` réussit sur l'ensemble de la solution
4. ✅ Aucune régression introduite
5. ✅ Le rapport de phase est rédigé
6. ✅ Les points ouverts rencontrés sont documentés

---

## 8. Packages NuGet prévus

| Projet | Packages |
|---|---|
| Domain | (aucun — pur C#) |
| Application | (aucun — dépend de Domain seulement) |
| Infrastructure | Microsoft.EntityFrameworkCore.Sqlite, Serilog, Serilog.Sinks.File, Serilog.Sinks.Console |
| Connectors | System.ServiceModel.Syndication (RSS), Microsoft.Extensions.Http, Polly (retry/circuit breaker) |
| Api | Microsoft.AspNetCore.App (framework), Serilog.AspNetCore |
| Worker | Microsoft.Extensions.Hosting |
| Domain.Tests | xUnit, FluentAssertions |
| Application.Tests | xUnit, FluentAssertions, Moq |
| Infrastructure.Tests | xUnit, FluentAssertions, Microsoft.EntityFrameworkCore.InMemory |
| Api.Tests | xUnit, FluentAssertions, Microsoft.AspNetCore.Mvc.Testing |

**Packages frontend (desktop-electron) :**
- electron, react, react-dom, typescript
- tailwindcss, @tailwindcss/typography
- maplibre-gl
- lucide-react (icônes)
- vite, @vitejs/plugin-react

---

## 9. Ordre de priorité en cas de tension

Si le temps ou la complexité imposent des choix, l'ordre de priorité est :

1. **Domain + Scoring + Normalisation** — C'est le cœur métier, non négociable
2. **Connecteurs RSS + Seed data** — Minimum pour la démo
3. **API + Dashboard** — Interface minimale fonctionnelle
4. **EventCase + Feedback** — Parcours analyste complet
5. **GDELT + Carte** — Deuxième connecteur et visualisation géo
6. **Export + Audit** — Should items
7. **Tags/Notes + Contradictions UI** — Should items de polish

Les Must (BL-01 à BL-12) sont non négociables. Les Should (BL-13 à BL-17) peuvent être simplifiés si nécessaire.

---

## Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](10-mvp-solo-v1-officiel.md)
- Architecture technique : [02-architecture-technique.md](../specs/02-architecture-technique.md)
- Spécifications fonctionnelles : [01-specs-fonctionnelles.md](../specs/01-specs-fonctionnelles.md)
- Spécifications UI/UX : [03-specs-ui-ux.md](../specs/03-specs-ui-ux.md)
- Plan de tests : [05-plan-de-tests.md](../specs/05-plan-de-tests.md)
- Rapport de synchronisation : [12-rapport-de-synchronisation-finale.md](12-rapport-de-synchronisation-finale.md)
</task_progress>
</read_file>