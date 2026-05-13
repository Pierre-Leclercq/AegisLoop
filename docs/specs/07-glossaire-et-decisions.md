# Aegis Loop — Glossaire et décisions

> **Version :** 2.0  
> **Statut :** Aligné V1 officielle  
> **Dernière mise à jour :** 2026-04-23  
> **Document amont :** [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)

---

## 1. Glossaire V1

### Termes métier

| Terme | Définition V1 |
|---|---|
| **Analyste** | Utilisateur principal du workbench desktop. Configure, explore, valide, corrige, exporte. |
| **Observation** | Unité normalisée d'information, centre de gravité du domaine. Issue d'un RawItem après normalisation. Porte le ClaimText et la preuve en V1. |
| **EventCase** | Événement / dossier regroupant des observations corrélées. Doit avoir au moins 1 Observation. |
| **RawItem** | Donnée brute collectée avant normalisation. A toujours un SourceHash non vide. |
| **SourceConnector** | Connecteur configuré et actif (RSS ou GDELT en V1). Inclut sa configuration (JSON). |
| **ConfidenceScore** | Score explicable à 3 composantes : FiabilitéSource, Corroboration, FeedbackAnalyste. Value ∈ [0.0, 1.0]. |
| **Contradiction** | Conflit détecté entre deux observations du même EventCase. Types V1 : Temporelle, Factuelle, Géographique. |
| **AnalystFeedback** | Action de l'analyste (Confirmer, Invalider, Corriger). Immutable après 5 minutes. |
| **AuditEntry** | Entrée du journal d'audit. Append-only, jamais modifié ni supprimé. |
| **IngestionJob** | Trace d'une exécution de connecteur. Statut : Planned, Running, Completed, Failed, Cancelled. |
| **Entity** | Entité nommée extraite (Location, Organization, Person). Pas de EntityLink en V1. |
| **Location** | Coordonnées géographiques. Latitude ∈ [-90, 90], Longitude ∈ [-180, 180]. |
| **Provenance** | Chaîne complète traçant l'origine d'une observation : source → collecte → normalisation → enrichissement → score. |
| **Clustering** | Algorithme heuristique de fusion d'observations en EventCase. Critères : proximité temporelle, géographique, similarité textuelle, entités communes. |
| **Seed data** | Données embarquées pour la démo. 2 scénarios, 90 observations, 8 événements, 3 contradictions. |
| **Mode démo** | Mode de fonctionnement sans réseau, utilisant les seed data. |

### Termes techniques

| Terme | Définition V1 |
|---|---|
| **Workbench** | Application desktop Electron + React + TypeScript hébergeant un backend .NET local. |
| **ISourceConnector** | Interface dans Domain implémentée par les connecteurs RSS et GDELT. |
| **Pipeline ingestion** | Flux : Collecte → RawItem → Normalisation → Observation → Clustering → EventCase → Scoring → Contradiction. |
| **Scoring heuristique** | Calcul du ConfidenceScore : Score = 0.35×FiabilitéSource + 0.35×Corroboration + 0.30×FeedbackAnalyste. |
| **FacteurCorroboration** | min(1.0, SourcesIndépendantes / 3). 3 sources = facteur 1.0. |
| **Rate limiting** | RSS : 1 req/15min par flux. GDELT : 60 req/min max. |
| **Circuit breaker** | 3 échecs consécutifs sur 5 min → pause du connecteur. |
| **SSE** | Server-Sent Events sur /api/events/stream pour les notifications temps réel. |

### Termes exclus V1 (repoussés V2/V3)

| Terme | Destination | Raison |
|---|---|---|
| **Claim** | Fusionné dans Observation (champ ClaimText) | Redondant en V1 |
| **Evidence** | Fusionné dans Observation | Observation = preuve en V1 |
| **MediaAsset** | V2 | Pas de connecteur média en V1 |
| **SearchQuery** | V2 | Recherche avancée repoussée |
| **EntityLink** | V2 | Enrichissement V2 |
| **Watchlist** | V2 | Complexe à implémenter |
| **ReportExport** | DTO Application | Pas de type domaine séparé |
| **ConnectorConfiguration** | Propriétés de SourceConnector | 2 connecteurs = pas besoin de type séparé |
| **Active Learning** | V3 | Prématuré |
| **NLP avancé** | V3 | Prématuré |

---

## 2. Décisions structurantes V1

### A1 — Architecture : 14 → 6 projets C#

**Décision :** Fusionner tous les connecteurs en `AegisLoop.Connectors`, intégrer Fusion + Scoring + CaseManagement dans `AegisLoop.Application`, supprimer `AegisLoop.Contracts` (DTOs dans Application), supprimer `Connectors.Abstractions` (interface dans Domain).

**Justification :** 14 projets pour un porteur solo = bureaucratie d'architecture disproportionnée.

**Quand restaurer :** Quand un connecteur devient complexe (YouTube V2) → projet séparé. Quand Fusion dépasse 500 lignes → module séparé.

### A2 — Backlog MVP : 32 → 17 items

**Décision :** 12 Must + 5 Should. Chaque Must sert directement la démo. YouTube, STAC, Import manuel, Watchlists, Export PDF, Timeline avancée, Recherche avancée, Active learning repoussés en V2/V3.

**Justification :** 32 items = 1.5 à 2 fois trop pour un porteur solo en 12 semaines.

### A3 — Domaine : 17 → 11 types

**Décision :** Garder SourceConnector (avec Config fusionné), RawItem, Observation (avec ClaimText), Entity, EventCase, Location, Contradiction (simplifié), ConfidenceScore (3 composantes), AnalystFeedback, AuditEntry, IngestionJob. Repousser Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist en V2. ReportExport → DTO Application.

**Justification :** Claim et Evidence sont redondants avec Observation en V1. MediaAsset n'a pas de connecteur média en V1.

### A4 — Scoring : 5 → 3 composantes

**Décision :** FiabilitéSource (0.35) + Corroboration (0.35) + FeedbackAnalyste (0.30). Fraîcheur et Complétude/Spécificité repoussées.

**Justification :** Fraîcheur est un tri, pas un score de confiance. Complétude/Spécificité trop subjective à calibrer pour V1.

### A5 — Connecteurs V1 : RSS + GDELT uniquement

**Décision :** Verrouiller les 2 connecteurs publics sans clé API. YouTube, STAC, Import manuel repoussés en V2.

**Justification :** RSS + GDELT suffisent pour démontrer la fusion multi-source, le scoring différencié, la provenance distincte, et les contradictions entre sources.

### A6 — GUI : 9 → 5 vues

**Décision :** Dashboard, Carte+Timeline, EventCase, Observations, Paramètres. Contradictions → onglet dans EventCase. Export → bouton dans EventCase. Recherche → filtres dans Observations. Connecteurs/Jobs → panel dans Dashboard ou Paramètres.

**Justification :** Chaque vue = routing + layout + états + tests E2E. 9 vues pour un MVP solo = sur-ingénierie UI.

### A7 — Tests : 11 → 5 catégories, ~100 tests

**Décision :** Unitaires, Intégration, Contrat connecteurs, E2E (smoke), Robustesse. Tests performance, sécurité dédiés, non-régression GUI repoussés en V2.

**Justification :** Le cérémonial QA est disproportionné pour un prototype solo.

### A8 — Endpoints API : 30+ → ~20

**Décision :** Éliminer les CRUD secondaires, regrouper les actions, simplifier les cas d'usage. Les endpoints pour Entity, Location, AuditEntry, Watchlist, SearchQuery, CaseFile sont exclus.

**Justification :** L'API locale ne doit pas devenir une bureaucratie REST inutile.

### A9 — Cycle de vie Electron ↔ .NET

**Décision :** Spécifier explicitement démarrage (spawn + health check), arrêt (SIGTERM + grâce 10s), crash (écran d'erreur + redémarrer), port conflict (5100-5110), mode démo (sans réseau), health check (30s).

**Justification :** Ce point était manquant dans le corpus initial. Il est critique pour la fiabilité du desktop app.

### A10 — Seed data concrètes

**Décision :** 2 scénarios : Crise au Soudan (50 obs, 5 événements, 2 contradictions) et Incident maritime Golfe d'Aden (40 obs, 3 événements, 1 contradiction). Chaque observation avec titre, contenu, source, date, localisation, score.

**Justification :** Les seed data étaient le cœur de la démontrabilité mais n'étaient pas définies concrètement.

### A11 — Contradictions simplifiées

**Décision :** Détection automatique simple + affichage + résolution manuelle par l'analyste. Pas de workflow de résolution complexe, pas de types multiples en V1, pas d'historique de résolution.

**Justification :** Les contradictions sont un différenciateur produit important, mais leur modélisation V1 était trop riche.

### A12 — ConnectorConfiguration fusionné dans SourceConnector

**Décision :** En V1, la configuration est un dictionnaire JSON dans SourceConnector. Pas de type séparé.

**Justification :** Un type séparé est correct architecturalement mais prématuré pour 2 connecteurs.

### A13 — Claim fusionné dans Observation

**Décision :** Ajouter un champ `ClaimText` et `ClaimConfidence` dans Observation. Pas de type Claim séparé.

**Justification :** En V1, Observation porte déjà la revendication. La distinction Claim/Observation n'apporte pas de valeur démonstrative.

### A14 — Evidence fusionné dans Observation

**Décision :** En V1, les observations SONT les preuves. EventCase référence directement des Observations.

**Justification :** Créer un type Evidence séparé ajoute de la complexité sans valeur métier en V1.

---

## 3. Points encore ouverts

| Point | Statut | Action requise |
|---|---|---|
| Schéma JSON Schema formel pour l'export | À définir | Pendant implémentation |
| Format exact du fichier user config | À définir | Pendant implémentation |
| Composant timeline exact (lib vs custom) | À décider | Pendant Phase 2 |
| Stratégie de géocodage offline | À affiner | Nominatim + cache, GeoNames locale en V2 |
| Contenu exact des 90 observations seed | À rédiger | Pendant Phase 3 |
| Politique de purge AuditEntry | À confirmer | > 90 jours par défaut, configurable |

---

## Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)
- Plan de recentrage appliqué : [09-plan-de-recentrage-applique.md](../review/09-plan-de-recentrage-applique.md)
- Changelog du recentrage : [11-changelog-recentrage.md](../review/11-changelog-recentrage.md)