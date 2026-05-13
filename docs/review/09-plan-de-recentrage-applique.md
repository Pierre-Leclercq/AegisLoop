# Aegis Loop — Plan de recentrage appliqué

> **Version :** 2.0  
> **Statut :** Document de référence post-audit  
> **Date :** 2026-04-23  
> **Origine :** Application des recommandations de l'audit critique (score global 6.8/10, verdict GO avec coupes)

---

## 1. Résumé exécutif du recentrage

L'audit critique du corpus Aegis Loop (score 6.8/10) a identifié trois défauts systémiques : sur-découpage modulaire (14 projets), périmètre MVP trop large (32 items), modèle de domaine sur-spécifié (17 types). Le verdict est **GO avec coupes significatives**.

Ce document trace quelles recommandations ont été appliquées, comment elles ont été traduites dans le corpus, quelles recommandations restent ouvertes, et quels arbitrages ont été faits.

**Résultat du recentrage :**

| Avant audit | Après recentrage | Réduction |
|---|---|---|
| 14 projets C# | 6 projets C# | -57% |
| 32 items backlog | 17 items backlog | -47% |
| 17 types domaine | 11 types domaine | -35% |
| 5 composantes scoring | 3 composantes scoring | -40% |
| 30+ endpoints API | ~20 endpoints | -33% |
| 9 vues UI | 5 vues UI | -44% |
| 14 composants UI | 10 composants UI | -29% |
| 18 raccourcis clavier | 9 raccourcis clavier | -50% |
| 11 catégories tests | 5 catégories tests | -55% |
| 4 connecteurs V1 | 2 connecteurs V1 | -50% |

---

## 2. Arbitrages structurants appliqués

### A1 — Architecture : 14 → 6 projets C#

**Décision :** Fusionner tous les connecteurs en un seul projet `AegisLoop.Connectors`, intégrer Fusion + Scoring + CaseManagement dans `AegisLoop.Application`, supprimer `AegisLoop.Contracts` (DTOs dans Application), supprimer `Connectors.Abstractions` (interface dans Domain).

**Justification :** 14 projets pour un porteur solo = bureaucratie d'architecture disproportionnée. Chaque projet implique .csproj, références, namespace, tests séparés.

**Quand restaurer le découpage :** Quand un connecteur devient complexe (YouTube V2) → projet séparé. Quand Fusion dépasse 500 lignes de logique → module séparé. Quand l'équipe dépasse 2 développeurs.

### A2 — Backlog MVP : 32 → 17 items

**Décision :** 12 Must + 5 Should. Chaque Must sert directement la démo. YouTube, STAC, Import manuel, Watchlists, Export PDF, Timeline avancée, Recherche avancée, Active learning repoussés en V2/V3.

**Justification :** 32 items = 1.5 à 2 fois trop pour un porteur solo en 12 semaines.

### A3 — Domaine : 17 → 11 types

**Décision :** Garder SourceConnector (avec Config fusionnée), RawItem, Observation (avec ClaimText), Entity, EventCase, Location, Contradiction (simplifié), ConfidenceScore (3 composantes), AnalystFeedback, AuditEntry, IngestionJob. Repousser Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist en V2. ReportExport → DTO Application.

**Justification :** Claim et Evidence sont redondants avec Observation en V1. MediaAsset n'a pas de connecteur média en V1. EntityLink et SearchQuery sont des enrichissements V2.

### A4 — Scoring : 5 → 3 composantes

**Décision :** FiabilitéSource (0.35) + Corroboration (0.35) + FeedbackAnalyste (0.30). Fraîcheur et Spécificité repoussées en V2.

**Justification :** Fraîcheur est un tri, pas un score de confiance. Spécificité est trop subjective à calibrer pour V1. 3 composantes suffisent pour démontrer la décomposition et l'explicabilité.

### A5 — Connecteurs V1 : RSS + GDELT uniquement

**Décision :** Verrouiller les 2 connecteurs publics sans clé API. YouTube, STAC, Import manuel repoussés en V2. L'interface `ISourceConnector` est conservée pour permettre l'extension future.

**Justification :** RSS + GDELT suffisent pour la fusion multi-source, le scoring différencié et les contradictions. YouTube ajoute quota + CGU + complexité. STAC a une valeur démo limitée sans images.

### A6 — GUI : 9 → 5 vues

**Décision :** Dashboard, Carte+Timeline, EventCase, Observations, Paramètres. Contradictions → onglet dans EventCase. Export → bouton dans EventCase. Recherche → filtres dans Observations. Connecteurs/Jobs → panel dans Dashboard ou Paramètres.

**Justification :** Chaque vue = routing + layout + états + tests E2E. 9 vues pour un MVP solo = sur-ingénierie UI.

### A7 — Tests : 11 → 5 catégories, ~100 tests

**Décision :** Unitaires, Intégration, Contrat connecteurs, E2E (smoke), Robustesse. Tests performance, sécurité dédiés, non-régression GUI repoussés en V2.

**Justification :** Le cérémonial QA est disproportionné pour un prototype solo. La pyramide 55/25/10/10 reste valide.

### A8 — Endpoints API : 30+ → ~20

**Décision :** Éliminer les CRUD secondaires, regrouper les actions, simplifier les cas d'usage. Les endpoints pour Entity, Location, AuditEntry, Watchlist, SearchQuery sont exclus.

**Justification :** L'API locale ne doit pas devenir une bureaucratie REST inutile. 20 endpoints suffisent pour couvrir le chemin critique de la démo.

### A9 — Cycle de vie Electron ↔ .NET

**Décision :** Spécifier explicitement démarrage (spawn + health check), arrêt (SIGTERM + grâce 10s), crash (écran d'erreur + redémarrer), port conflict (5100-5110), mode démo (sans réseau), health check (30s).

**Justification :** Ce point était manquant dans le corpus initial. Il est critique pour la fiabilité du desktop app.

### A10 — Seed data concrètes

**Décision :** 2 scénarios : Crise au Soudan (50 obs, 5 événements, 2 contradictions) et Incident maritime Golfe d'Aden (40 obs, 3 événements, 1 contradiction). Chaque observation avec titre, contenu, source, date, localisation, score.

**Justification :** Les seed data étaient le cœur de la démontrabilité mais n'étaient pas définies concrètement.

---

## 3. Recommandations d'audit — Suivi d'application

### Recommandations appliquées ✅

| # | Recommandation | Comment appliquée | Localisation corpus |
|---|---|---|---|
| 1 | Réduire à 6-7 projets C# | Fusion connecteurs, intégration Fusion/Scoring/CaseManagement dans Application | 02-architecture 3, 10-mvp 7 |
| 2 | Couper backlog à 17 items | 12 Must + 5 Should, exclus documentés | 01-specs 4, 10-mvp 4 |
| 3 | Réduire à 5 vues UI | Fusion Contradictions/Export/Recherche | 03-specs-ui-ux 2, 10-mvp 6 |
| 4 | Simplifier domaine à 11 types | Claim→Observation, Evidence→V2, etc. | 02-architecture 4, 10-mvp 5 |
| 5 | Scoring à 3 composantes | FiabilitéSource + Corroboration + FeedbackAnalyste | 01-specs 3.7, 10-mvp 5 |
| 6 | Ajouter slicing vertical | Phase 1-2-3, chemin critique démo | 01-specs 6, 10-mvp 12 |
| 7 | Définir seed data | 2 scénarios, 90 observations | 01-specs 3.16, 10-mvp 11 |
| 8 | Spécifier cycle de vie Electron/.NET | Démarrage, arrêt, crash, port, health check | 02-architecture 13, 10-mvp 7 |
| 9 | Réduire endpoints à ~20 | Éliminer CRUD secondaires | 02-architecture 13.4, 10-mvp 7 |
| 10 | Réduire tests à 5 catégories | Unitaires, Intégration, Contrat, E2E, Robustesse | 05-plan-de-tests 3, 10-mvp 8 |
| 11 | Rate limiting concret | RSS 15min, GDELT 60req/min (marge), Nominatim 1.1s | 02-architecture connecteurs |
| 12 | Stratégie retry détaillée | Exponentiel 1-2-4-8-16s, circuit breaker 3 échecs/5min | 02-architecture connecteurs |
| 13 | Invariants métier | 10 invariants explicites | 02-architecture 4, 10-mvp 5 |
| 14 | Raccourcis clavier réduits | 9 raccourcis V1 | 03-specs-ui-ux 5, 10-mvp 6 |

### Recommandations partiellement appliquées 🟡

| # | Recommandation | Ce qui est fait | Ce qui reste ouvert |
|---|---|---|---|
| 15 | Schéma JSON d'export | Format défini dans 01-specs | Schéma JSON Schema formel à produire pendant implémentation |
| 16 | Stratégie de configuration | Hiérarchie définie | Fichier user config exact à définir pendant implémentation |
| 17 | Chiffrement clés API simplifié | AES en V1 décidé | Implémentation concrète pendant développement |

### Recommandations repoussées en V2 ⏳

| # | Recommandation | Raison du report |
|---|---|---|
| 18 | Tests de performance | Pas de volume en V1, seuils dans specs suffisent |
| 19 | Tests de sécurité dédiés | Vérifications basiques dans tests d'intégration |
| 20 | Tests non-régression GUI | L'UI va changer, trop tôt |
| 21 | Connecteurs YouTube/STAC/Import | V2 |
| 22 | Timeline avancée | Composant lourd, V2 |
| 23 | Active learning | V3 |
| 24 | NLP/entity linking | V3 |

---

## 4. Arbitrages complémentaires

### Arbitrage A11 — Contradictions simplifiées

**Décision :** Détection automatique simple + affichage + résolution manuelle par l'analyste. Pas de workflow de résolution complexe, pas de types multiples en V1, pas d'historique de résolution.

**Justification :** Les contradictions sont un différenciateur produit important, mais leur modélisation V1 était trop riche. Détecter, afficher, laisser l'analyste résoudre suffit pour la démo.

### Arbitrage A12 — ConnectorConfiguration fusionné dans SourceConnector

**Décision :** En V1, la configuration est un dictionnaire JSON dans SourceConnector. Pas de type séparé.

**Justification :** Un type séparé est correct architecturalement mais prématuré pour 2 connecteurs.

### Arbitrage A13 — Claim fusionné dans Observation

**Décision :** Ajouter un champ `ClaimText` et `ClaimConfidence` dans Observation. Pas de type Claim séparé.

**Justification :** En V1, Observation porte déjà la revendication. La distinction Claim/Observation n'apporte pas de valeur démonstrative.

### Arbitrage A14 — Evidence fusionné dans Observation

**Décision :** En V1, les observations SONT les preuves. EventCase référence directement des Observations.

**Justification :** Créer un type Evidence séparé ajoute de la complexité sans valeur métier en V1.

---

## 5. Points encore ouverts

| Point | Statut | Action requise |
|---|---|---|
| Schéma JSON Schema formel pour l'export | À définir | Pendant implémentation |
| Format exact du fichier user config | À définir | Pendant implémentation |
| Composant timeline exact (lib vs custom) | À décider | Pendant Phase 2 |
| Stratégie de géocodage offline | À affiner | Nominatim + cache, GeoNames locale en V2 |
| Contenu exact des 90 observations seed | À rédiger | Pendant Phase 3 |
| Politique de purge AuditEntry | À confirmer | > 90 jours par défaut, configurable |

---

## 6. Vérification de cohérence

| Contrainte audit | État dans corpus recentré | Conforme ? |
|---|---|---|
| 6-7 projets max | 6 projets + desktop-electron | ✅ |
| 17 items backlog max | 12 Must + 5 Should = 17 | ✅ |
| 11 types domaine max | 11 types | ✅ |
| 3 composantes scoring max | 3 composantes | ✅ |
| 5 vues UI max | 5 vues | ✅ |
| ~20 endpoints API | 21 endpoints | ✅ (marge acceptable) |
| 2 connecteurs V1 | RSS + GDELT | ✅ |
| 5 catégories tests | 5 catégories | ✅ |
| ~80-100 tests | 80-100 estimés | ✅ |
| 9-10 raccourcis | 9 raccourcis | ✅ |
| 10 composants UI max | 7 Must + 3 Should = 10 | ✅ |
| Seed data définies | 2 scénarios, 90 obs | ✅ |
| Cycle de vie Electron/.NET | Spécifié | ✅ |
| Invariants métier | 10 invariants | ✅ |

---

## Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](10-mvp-solo-v1-officiel.md)
- Changelog du recentrage : [11-changelog-recentrage.md](11-changelog-recentrage.md)
- Rapport d'audit global : [00-rapport-audit-global.md](00-rapport-audit-global.md)
- Checklist avant implémentation : [08-checklist-avant-implementation.md](08-checklist-avant-implementation.md)