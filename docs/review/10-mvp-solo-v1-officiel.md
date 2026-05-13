# Aegis Loop — MVP Solo V1 Officiel

> **Version :** 2.0  
> **Statut :** Référence officielle V1 post-audit  
> **Date :** 2026-04-23  
> **Origine :** Recentrage après audit critique (voir `09-plan-de-recentrage-applique.md`)

---

## 1. Périmètre V1 officiel

Aegis Loop V1 est un **workbench analyste desktop** qui fusionne, qualifie, trace et rend explicables des observations hétérogènes issues de sources OSINT publiques.

**V1 est :**
- Une couche logicielle de fusion + confiance + feedback analyste
- Un démonstrateur solo crédible et déployable en ~12 semaines
- Lawful, traçable, éthique et défensif par construction

**V1 n'est PAS :**
- Une plateforme ISR complète
- Un système multi-utilisateur
- Un produit nécessitant une infrastructure distribuée
- Un moteur de scraping agressif
- Un système à composantes IA opaques

---

## 2. Sources V1

| Source | Type | Accès | V1 |
|---|---|---|---|
| **RSS/Atom** | Flux de médias et sites institutionnels | Public, pas de clé API | ✅ Must |
| **GDELT API v2** | Événements globaux géolocalisés | Public, pas de clé API | ✅ Must |
| **Seed/Replay data** | Données embarquées pour la démo | Local | ✅ Must |

**Connecteurs exclus V1 (repoussés V2+) :** YouTube Data API, STAC/Copernicus, Import manuel CSV/JSON/GeoJSON/KML.

**Justification :** RSS + GDELT suffisent pour démontrer la fusion multi-source (RSS = texte riche, GDELT = données structurées + géolocalisation), le scoring différencié, la provenance distincte, et les contradictions entre sources. Les autres connecteurs ajoutent de la complexité sans valeur démonstrative proportionnelle.

---

## 3. Scénarios V1

### Scénario MUST 1 — Surveillance d'une crise émergente

**Titre :** « Suivi d'une crise géopolitique via RSS + GDELT »

**Flux :**
1. L'analyste active les connecteurs RSS (médias internationaux) et GDELT (filtre pays/thème)
2. Le système collecte, normalise et corrèle les observations
3. Des événements sont détectés avec scores de confiance et provenance
4. L'analyste explore un événement, consulte la provenance, identifie une contradiction
5. L'analyste valide ou corrige, le score est recalculé
6. L'analyste exporte le dossier en Markdown/JSON

**Valeur démontrée :** Fusion multi-source, scoring explicable, provenance, contradictions, feedback analyste, export.

### Scénario MUST 2 — Suivi d'un événement maritime

**Titre :** « Incident maritime dans le Golfe d'Aden »

**Flux :**
1. L'analyste configure GDELT (filtre maritime) et RSS (sources maritimes)
2. Le système détecte des observations géolocalisées
3. La carte affiche les observations et clusters
4. L'analyste recoupe les sources, vérifie la provenance géographique
5. Une contradiction sur la localisation est détectée et résolue

**Valeur démontrée :** Corrélation géospatiale, carte interactive, contradictions géographiques.

### Scénario SHOULD (optionnel) — Construction incrémentale d'un dossier

**Titre :** « Enrichissement itératif d'un case file avec feedback »

**Flux :** L'analyste utilise Aegis Loop sur plusieurs sessions, ajoute des notes, valide des observations, suit l'évolution du scoring, exporte le dossier final.

**Valeur démontrée :** Case management, feedback itératif, enrichissement, export structuré.

---

## 4. Fonctionnalités V1 — Backlog officiel (17 items)

### Must (12 items)

| ID | Fonctionnalité | Description |
|---|---|---|
| BL-01 | Modèle de domaine | 11 types, invariants explicites, interfaces clés |
| BL-02 | Connecteur RSS/Atom | Collecte, polling, rate limiting, retry |
| BL-03 | Connecteur GDELT | Collecte, filtrage pays/thème, rate limiting, retry |
| BL-04 | Pipeline ingestion | Collecte → RawItem → Normalisation → Observation |
| BL-05 | Normalisation | Mapping vers Observation unifiée, Metadata extensible |
| BL-06 | Détection d'entités | Dictionnaire + regex, Entity basique |
| BL-07 | Fusion/correlation | Clustering temporel + géographique + similarité textuelle |
| BL-08 | Scoring heuristique | 3 composantes : FiabilitéSource, Corroboration, FeedbackAnalyste |
| BL-09 | Dashboard | KPIs, événements prioritaires, contradictions |
| BL-10 | Vue EventCase | Détail événement, observations, provenance, score, contradictions |
| BL-11 | Feedback analyste | Confirmer/invalider/corriger, recalcul du score |
| BL-12 | Mode démo + seed data | 2 scénarios embarqués, sans réseau |

### Should (5 items)

| ID | Fonctionnalité | Description |
|---|---|---|
| BL-13 | Export Markdown/JSON | Export de dossier événement |
| BL-14 | Audit trail | Journal append-only des actions |
| BL-15 | Tags/notes simples | Annotation basique sur observations et événements |
| BL-16 | Contradictions — détection et affichage | Détection automatique + onglet dans EventCase |
| BL-17 | Configuration connecteurs | Formulaire de configuration dans Paramètres |

### Exclus V1 (repoussés V2/V3)

| Fonctionnalité | Destination | Raison |
|---|---|---|
| Connecteur YouTube | V2 | Quota API, CGU, complexité |
| Connecteur STAC | V2 | Valeur démo limitée sans images |
| Import manuel multi-format | V2 | Pas critique pour la démo |
| Watchlists | V2 | Complexe à implémenter correctement |
| Export PDF | V2 | Complexité de génération |
| Timeline avancée | V2 | Composant lourd |
| Recherche avancée | V2 | Filtres simples suffisent en V1 |
| Active learning | V3 | Prématuré |
| NLP/entity linking | V3 | Prématuré |
| Plugins connecteurs | V3 | Pas de besoin en V1 |

---

## 5. Domaine V1 — 11 types

| Type | Responsabilité | V1 |
|---|---|---|
| **SourceConnector** | Connecteur configuré et actif (inclut Config) | ✅ Must |
| **RawItem** | Donnée brute avant normalisation | ✅ Must |
| **Observation** | Unité normalisée — centre de gravité du domaine | ✅ Must |
| **Entity** | Entité nommée extraite (Location, Org, Person) | ✅ Must |
| **EventCase** | Événement / dossier regroupant des observations | ✅ Must |
| **Location** | Coordonnées géographiques | ✅ Must |
| **Contradiction** | Conflit entre observations (simplifié) | ✅ Must |
| **ConfidenceScore** | Score explicable à 3 composantes | ✅ Must |
| **AnalystFeedback** | Action de l'analyste (append-only après fenêtre) | ✅ Must |
| **AuditEntry** | Entrée du journal d'audit | ✅ Must |
| **IngestionJob** | Trace d'une exécution de connecteur | ✅ Must |

**Types exclus V1 :** Claim (→ champ `ClaimText` dans Observation), Evidence (→ Observation = preuve en V1), MediaAsset (V2 avec YouTube), SearchQuery (V2), EntityLink (V2), Watchlist (V2), ReportExport (→ DTO Application), ConnectorConfiguration (→ propriétés de SourceConnector).

### Scoring V1 — 3 composantes

```
Score = W1 × FiabilitéSource + W2 × Corroboration + W3 × FeedbackAnalyste
```

| Composante | Plage | Poids défaut | Description |
|---|---|---|---|
| FiabilitéSource | 0.0–1.0 | 0.35 | Score de la source (configurable par type) |
| Corroboration | 0.0–1.0 | 0.35 | Nombre de sources indépendantes confirmant |
| FeedbackAnalyste | 0.0–1.0 | 0.30 | Validation/correction par l'analyste |

**Exclus V1 :** Fraîcheur (→ tri, pas un score de confiance), Spécificité (trop subjectif à calibrer).

### Invariants métier V1

1. Un EventCase doit avoir au moins 1 Observation
2. ConfidenceScore.Value ∈ [0.0, 1.0]
3. Un RawItem a toujours un SourceHash non vide
4. Toute Observation est liée à un RawItem ou marquée manuelle
5. Un AnalystFeedback est immutable après la fenêtre d'annulation (5 min)
6. AuditEntry est append-only, jamais modifié ni supprimé
7. Contradiction : Observation1Id ≠ Observation2Id
8. Location : Latitude ∈ [-90, 90], Longitude ∈ [-180, 180]
9. Un SourceConnector actif a toujours une configuration valide
10. Le score d'un EventCase est la moyenne pondérée des scores de ses Observations × FacteurCorroboration

---

## 6. GUI V1 — 5 vues

| Vue | Raccourci | Contenu |
|---|---|---|
| **Dashboard** | `Ctrl+1` | KPIs, événements prioritaires, contradictions, activité connecteurs |
| **Carte + Timeline** | `Ctrl+2` | Observations géolocalisées, timeline simple, clusters |
| **EventCase** | `Ctrl+3` | Détail événement, observations, provenance, score décomposé, contradictions (onglet), export (bouton), notes/tags |
| **Observations** | `Ctrl+4` | Liste, filtres simples, détail observation, provenance, feedback |
| **Paramètres** | `Ctrl+5` | Configuration connecteurs, scoring, mode démo, thème |

**Fusionnés :** Contradictions → onglet dans EventCase, Export → bouton dans EventCase, Recherche → filtres dans Observations, Connecteurs/Jobs → panel dans Dashboard ou Paramètres.

### Composants UI V1 (10)

**Must (7) :** ObservationCard, EventCaseHeader, ConfidenceScoreBadge, ProvenanceChain, TimelineSimple, MapContainer, FeedbackPanel

**Should (3) :** ContradictionAlert, SourceBadge, FilterBar

### Raccourcis clavier V1 (9)

| Raccourci | Action | Priorité |
|---|---|---|
| `Ctrl+1-5` | Navigation entre les 5 vues | Must |
| `Ctrl+Enter` | Valider une observation | Must |
| `Ctrl+Shift+X` | Invalider une observation | Must |
| `Ctrl+E` | Exporter le dossier courant | Must |
| `Ctrl+K` | Recherche rapide | Must |
| `Escape` | Fermer panel/modal | Must |
| `Ctrl+F` | Filtrer dans la vue courante | Should |
| `Ctrl+D` | Aller au Dashboard | Should |
| `?` | Aide des raccourcis | Should |

---

## 7. Architecture V1 — 6 projets

| Projet | Contenu | Justification |
|---|---|---|
| **AegisLoop.Domain** | 11 types, invariants, interfaces (ISourceConnector, IScoringService, etc.) | Séparation indispensable |
| **AegisLoop.Application** | Use cases, pipeline ingestion, fusion, scoring, feedback | Séparation indispensable |
| **AegisLoop.Infrastructure** | EF Core + SQLite, configuration, logging, géocodage | Séparation indispensable |
| **AegisLoop.Connectors** | RSS + GDELT (un seul projet pour 2 connecteurs) | 2 connecteurs = 1 projet suffit |
| **AegisLoop.Api** | REST Minimal APIs localhost (~20 endpoints) | Couche API |
| **AegisLoop.Worker** | Service d'ingestion, host, planification | Processus hôte |
| **desktop-electron** | Shell Electron + React + TypeScript | Frontend |

**Total backend : 6 projets C#** (au lieu de 14). Les modules Fusion, Scoring, CaseManagement sont intégrés dans Application.

### Cycle de vie Electron ↔ .NET

1. **Démarrage :** Electron spawn le processus .NET (`AegisLoop.Api`), attend le health check sur `/health` (timeout 30s), ouvre le BrowserWindow
2. **Arrêt :** Electron envoie SIGTERM, attend la grâce (10s max), kill si nécessaire
3. **Crash backend :** Electron affiche un écran d'erreur avec bouton "Redémarrer le backend"
4. **Port conflict :** Port par défaut 5100, si occupé → essai 5101-5110, si échec → erreur
5. **Mode démo :** Le backend peut démarrer sans réseau, les seed data sont embarquées
6. **Health check :** GET `/health` toutes les 30s, timeout 5s

### API V1 — ~20 endpoints

| Endpoint | Méthode | Description |
|---|---|---|
| `/health` | GET | Health check |
| `/api/events` | GET | Liste événements (paginée, filtrable) |
| `/api/events/{id}` | GET | Détail événement |
| `/api/events` | POST | Créer événement |
| `/api/events/{id}` | PATCH | Mettre à jour événement |
| `/api/observations` | GET | Liste observations |
| `/api/observations/{id}` | GET | Détail observation |
| `/api/observations/{id}/provenance` | GET | Provenance complète |
| `/api/feedback` | POST | Soumettre feedback (valider/invalider/corriger) |
| `/api/connectors` | GET | Liste connecteurs |
| `/api/connectors/{id}` | GET | Statut connecteur |
| `/api/connectors` | POST | Configurer connecteur |
| `/api/ingestion/run` | POST | Lancer collecte manuelle |
| `/api/ingestion/jobs` | GET | Historique jobs |
| `/api/ingestion/jobs/{id}` | GET | Détail job |
| `/api/scoring/{id}/breakdown` | GET | Décomposition du score |
| `/api/dashboard` | GET | KPIs et données dashboard |
| `/api/export/{id}` | GET | Export Markdown ou JSON |
| `/api/demo/load` | POST | Charger seed data |
| `/api/demo/reset` | POST | Réinitialiser démo |
| `/api/events/stream` | GET | SSE — notifications temps réel |

---

## 8. Tests V1 — 5 catégories, ~80-100 tests

| Catégorie | Outil | Priorité | Volume estimé |
|---|---|---|---|
| **Unitaires** | xUnit + FluentAssertions | Must | ~50 tests |
| **Intégration** | xUnit + WebApplicationFactory | Must | ~20 tests |
| **Contrat connecteurs** | xUnit | Must | ~5 tests |
| **E2E (smoke)** | Playwright | Must | ~5 tests |
| **Robustesse** | xUnit | Should | ~10 tests |

### 10 tests critiques

1. Normalisation RSS → Observation
2. Normalisation GDELT → Observation
3. Scoring avec 3 composantes → score ∈ [0, 1]
4. Fusion d'observations en événement
5. Détection de contradiction
6. Feedback analyste modifie le score
7. Pipeline ingestion bout en bout
8. API CRUD observations
9. Contrat ISourceConnector respecté
10. Smoke test démo 5 min

### Smoke tests de démo

- Application lançable localement
- Dashboard affiche des événements avec scores
- EventCase montre la provenance complète
- L'analyste peut valider/invalider et voir l'impact
- La carte affiche les observations géolocalisées
- Le mode démo fonctionne sans réseau
- Les contradictions sont visibles dans l'interface
- L'export Markdown fonctionne

---

## 9. Exclus V1 — Résumé

| Catégorie | Exclus V1 | Destination |
|---|---|---|
| Connecteurs | YouTube, STAC, Import manuel | V2 |
| Domaine | Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist, ReportExport, ConnectorConfiguration | V2 |
| Scoring | Fraîcheur, Spécificité | V2 |
| GUI | Vues Contradictions, Export, Recherche avancée | V2 (fusionnées) |
| UI | Composants SearchAdvanced, WatchlistManager, ExportWizard, TimelineAvancée | V2 |
| Raccourcis | 9 raccourcis supprimés | V2 |
| Tests | Performance, sécurité dédiés, non-régression GUI | V2 |
| Architecture | Projets séparés Fusion, Scoring, CaseManagement, Connectors.* | V3 |

---

## 10. Définition de "Démo prête"

Le MVP V1 est prêt à être démontré quand les conditions suivantes sont **toutes** remplies :

1. ✅ Application desktop lançable localement (clone → build → run < 30 min)
2. ✅ Configuration minimale fonctionnelle (pas de clé API requise)
3. ✅ 2 connecteurs V1 fonctionnels (RSS + GDELT)
4. ✅ Ingestion batch/polling opérationnelle
5. ✅ Normalisation des observations (RSS → Observation, GDELT → Observation)
6. ✅ Création ou consolidation d'événements (clustering automatique)
7. ✅ Scoring explicable (3 composantes, décomposable, cliquable)
8. ✅ Consultation de provenance (chaîne complète visible)
9. ✅ Au moins 1 scénario analyste complet (crise géopolitique)
10. ✅ Seed/replay data stable (2 scénarios, 90 observations, 8 événements, 3 contradictions)
11. ✅ Export simple (Markdown + JSON)
12. ✅ Smoke tests verts (5 min de démo sans crash)

**Pas besoin de :** YouTube, STAC, export PDF, watchlists, recherche avancée, timeline avancée, NLP, active learning.

---

## 11. Seed data — Définition concrète

### Scénario 1 — Crise au Soudan

- **50 observations** de 3 sources (RSS LeMonde, RSS BBC, GDELT)
- **5 événements** : Combats Khartoum, Déplacement populations, Coupure humanitaire, Déclarations diplomatiques, Incertitude sur le bilan
- **2 contradictions** : Bilan humain divergent, Localisation des combats
- Chaque observation : titre, contenu, source, date, localisation, score de fiabilité

### Scénario 2 — Incident maritime Golfe d'Aden

- **40 observations** de 2 sources (RSS maritime, GDELT maritime)
- **3 événements** : Attaque navire commerce, Réponse internationale, Trafic maritime dévié
- **1 contradiction** : Nature de l'attaque (missile vs drone)
- Données géolocalisées avec coordonnées

**Total seed :** 90 observations, 8 événements, 3 contradictions.

---

## 12. Planning V1 — 12 semaines

| Phase | Semaines | Contenu |
|---|---|---|
| **Phase 1** | 1-4 | Domain model (11 types) + Connecteur RSS + Pipeline + API minimale (5 endpoints) + Dashboard basique |
| **Phase 2** | 5-8 | Connecteur GDELT + Scoring 3 composantes + Feedback analyste + Carte/Timeline simple + Provenance visible |
| **Phase 3** | 9-12 | Mode démo + Seed data + Contradictions + Export Markdown + Audit trail + Polish UI + Smoke tests |

---

## Références

- Plan de recentrage appliqué : [09-plan-de-recentrage-applique.md](09-plan-de-recentrage-applique.md)
- Changelog du recentrage : [11-changelog-recentrage.md](11-changelog-recentrage.md)
- Rapport d'audit global : [00-rapport-audit-global.md](../review/00-rapport-audit-global.md)
- Checklist avant implémentation : [08-checklist-avant-implementation.md](../review/08-checklist-avant-implementation.md)