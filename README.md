# AegisLoop V1

**Workbench analyste desktop — fusion, qualification, traçage et explicabilité d'observations OSINT** — MVP Solo V1

## Architecture

```
AegisLoop.sln
├── src/
│   ├── AegisLoop.Domain/         # 11 types, invariants, interfaces (ISourceConnector, etc.)
│   ├── AegisLoop.Application/    # Use cases, pipeline ingestion, fusion, scoring, feedback
│   ├── AegisLoop.Infrastructure/ # EF Core + SQLite, configuration, logging, géocodage
│   ├── AegisLoop.Connectors/    # RSS + GDELT (un seul projet pour 2 connecteurs)
│   ├── AegisLoop.Api/            # REST Minimal APIs localhost (~20 endpoints)
│   ├── AegisLoop.Worker/         # Service d'ingestion, host, planification
│   └── desktop-electron/        # Shell Electron + React + TypeScript (orchestrateur local)
├── tests/
│   ├── AegisLoop.Domain.Tests/
│   ├── AegisLoop.Application.Tests/
│   ├── AegisLoop.Infrastructure.Tests/
│   ├── AegisLoop.Connectors.Tests/
│   ├── AegisLoop.Api.Tests/
│   └── AegisLoop.E2E.Tests/
└── docs/                         # Spécifications et ADR
```

## Stack technique

| Composant | Version |
|---|---|
| .NET SDK | 10.0.201+ |
| EF Core | 10.0.7 stable |
| SQLite | via EF Core |
| Electron | 41.3.0 |
| React | 19.x |
| TypeScript | 6.0.x |
| Vite | 8.x |
| Vitest | 4.x |

## Domaine V1 — 11 types

| Type | Responsabilité |
|---|---|
| SourceConnector | Connecteur configuré et actif (RSS, GDELT) |
| RawItem | Donnée brute avant normalisation |
| Observation | Unité normalisée — centre de gravité du domaine |
| Entity | Entité nommée extraite (Location, Org, Person) |
| EventCase | Événement / dossier regroupant des observations |
| Location | Coordonnées géographiques |
| Contradiction | Conflit entre observations |
| ConfidenceScore | Score explicable à 3 composantes |
| AnalystFeedback | Action de l'analyste (append-only après fenêtre) |
| AuditEntry | Entrée du journal d'audit |
| IngestionJob | Trace d'une exécution de connecteur |

## Vues Desktop V1 — 5 vues

| Vue | Raccourci | Description |
|-----|-----------|-------------|
| Dashboard | Ctrl+1 | KPIs, événements prioritaires, contradictions, activité connecteurs |
| Carte + Timeline | Ctrl+2 | EventCases géolocalisés, fond Natural Earth offline, timeline simple, filtres V1 |
| EventCase | Ctrl+3 | Détail événement, observations, provenance, score, contradictions |
| Observation | Ctrl+4 | Liste, filtres simples, détail observation, provenance, feedback |
| Paramètres | Ctrl+5 | Configuration connecteurs, scoring, mode démo, thème |

## Prérequis

- **.NET 10 SDK** (10.0.201+)
- **Node.js 24+** + npm 11+
- **Electron** (installé via npm)

## Lancement rapide

### Desktop complet (recommandé — Electron orchestre tout)

```bash
scripts\run-desktop.bat
# ou
cd src\desktop-electron
npm run electron:dev
```

Electron lance automatiquement :
1. L'API .NET sur `http://localhost:5100`
2. Le Worker d'ingestion
3. L'interface React

### API seule (pour debug)

```bash
run api
# ou
dotnet run --project src/AegisLoop.Api/AegisLoop.Api.csproj
# → http://localhost:5100
# → Health check : http://localhost:5100/health
```

### Worker seul (pour debug)

```bash
run worker
# ou
dotnet run --project src/AegisLoop.Worker/AegisLoop.Worker.csproj
```

### Build complet

```bash
scripts\build.bat
# ou backend seul
dotnet restore AegisLoop.sln
dotnet build AegisLoop.sln --configuration Release
# frontend seul
cd src\desktop-electron
npm install
npm run build
```

### Tests

```bash
scripts\test.bat
# ou
dotnet test AegisLoop.sln --configuration Release --settings .runsettings
cd src\desktop-electron
npm test
npm audit --audit-level=low
```

Baseline officielle de validation backend locale/CI :

```bash
dotnet test AegisLoop.sln --configuration Release --settings .runsettings
```

La commande Debug implicite `dotnet test AegisLoop.sln` n'est pas la baseline officielle dans cet environnement Windows local : Windows Code Integrity / Smart App Control peut bloquer le chargement d'assemblies de tests Debug générées localement avec `0x800711C7` (`Une stratégie de contrôle d’application a bloqué ce fichier`). Ce blocage est externe au code applicatif ; il ne doit pas être masqué en désactivant des tests. La CI et `scripts\test.bat` utilisent explicitement `Release`.

## Orchestration locale Electron

En mode développement, Electron est le point d'entrée principal du desktop :

- **Démarrage** : Electron spawn les processus `dotnet run` pour l'API et le Worker
- **Readiness** : Electron attend que `http://localhost:5100/health` réponde 200 avant d'ouvrir la fenêtre
- **Arrêt propre** : À la fermeture de l'appli, Electron envoie SIGTERM aux processus enfants, puis SIGKILL après 5s si nécessaire
- **Modularité conservée** : API et Worker restent exécutables indépendamment pour le debug

## Connecteurs V1

| Connecteur | Statut | Source |
|------------|--------|--------|
| RSS/Atom   | Phase 1 | Flux RSS configurables |
| GDELT      | Phase 1 | GDELT API v2 |

## Base de données

SQLite — fichier `aegisloop.db` créé automatiquement.
EF Core 10.0.7 stable est utilisé, conservé comme ORM pour AegisLoop.

## Démo locale V1 — seed/replay/export

Le scénario de démonstration principal fonctionne sans réseau via `examples/demo-data/v1-seed.json`.

Script détaillé de présentation : [`docs/demo/demo-script-v1.md`](docs/demo/demo-script-v1.md).

- Dataset `v1-seed-2026-04` : 2 scénarios (`sahel-civic-security`, `aden-maritime-incident`), 5 connecteurs simulés RSS/GDELT, 90 observations, feedbacks analyste seedés, EventCases reconstruits par heuristique V1.
- Les 8 groupes seed incluent des lieux et coordonnées déterministes non sensibles pour alimenter **Carte + Timeline** sans géocodage réseau.
- La vue **Carte + Timeline** utilise un fond cartographique public offline Natural Earth multi-échelle : `world` 1:110m (`natural-earth-110m/`), `regional` 1:50m (`natural-earth-50m/`) et `local` extrait 1:10m Sahel/Aden (`natural-earth-10m/`). Le rendu est viewport-driven : sélection de couches selon `viewBox`/zoom, chargement local progressif via `public/map-data/manifest.json`, filtrage des features visibles et fallback automatique local, sans tuiles externes, sans géocodeur et sans service cartographique réseau.
- Attribution affichée dans la carte : `Map data: Natural Earth — public domain`, avec l’échelle active. Natural Earth est public domain ; le fond est indicatif et ne constitue pas un SIG de précision.
- Endpoints utiles : `GET /api/demo/status`, `POST /api/demo/load`, `POST /api/demo/reset`, `POST /api/demo/rebuild`, `POST /api/demo/recalculate`, `GET /api/map-timeline`.
- Export dossier : `GET /api/export/{eventCaseId}?format=json` ou `format=markdown`.
- Depuis le desktop, les boutons **Exporter JSON** et **Exporter Markdown** déclenchent un téléchargement local nommé `aegisloop-eventcase-<id>.json` ou `aegisloop-eventcase-<id>.md`.

Parcours de démonstration :

1. Lancer l’app desktop : `scripts\run-desktop.bat`.
2. Ouvrir **Paramètres** (`Ctrl+5`) puis **Charger seed** dans “Mode démo seed/replay V1”.
3. Consulter le **Dashboard** (`Ctrl+1`) : compteurs, EventCases prioritaires, répartition sources/catégories.
4. Ouvrir **Carte + Timeline** (`Ctrl+2`) : vérifier les marqueurs, la timeline, les compteurs, les filtres source/score/scénario et la sélection synchronisée.
5. Ouvrir **EventCase** (`Ctrl+3`) et sélectionner un dossier.
6. Consulter la provenance agrégée : sources, RawItems, hashes, observations liées.
7. Soumettre un feedback EventCase ou Observation (Confirm/Invalidate/Correct/Note).
8. Observer le recalcul du score et le breakdown explicable.
9. Exporter le dossier en **JSON** puis en **Markdown** depuis la vue EventCase.
10. Revenir dans **Paramètres** pour consulter l’audit (`DemoSeedLoaded`, `DemoEventCasesRebuilt`, `DemoScoresRecalculated`, `EventCaseExported`, feedbacks).

Pour rejouer proprement : **Reset démo**, puis **Charger seed**. Le seed est déterministe et versionné ; les données sont simulées et non sensibles.

## État dépendances Phase 0C

- Cible officielle V1 : `.NET 10` / `net10.0` centralisé dans `Directory.Build.props`.
- Packages preview .NET supprimés : EF Core, Microsoft.Extensions.Hosting et Microsoft.AspNetCore.Mvc.Testing sont alignés sur `10.0.7` stable.
- Le masquage `NU1903` a été supprimé : les vulnérabilités NuGet High ne sont plus ignorées.
- Frontend aligné sur Electron `41.3.0`, Vite `8.0.10`, Vitest `4.1.5`, TypeScript `6.0.3`.
- Validations sécurité attendues : `dotnet list AegisLoop.sln package --vulnerable --include-transitive` et `npm audit --audit-level=low`.

## CI

GitHub Actions — build backend (.NET 10) + frontend sur `main` et `develop`.

---

*AegisLoop V1 — MVP Solo — Workbench analyste OSINT*