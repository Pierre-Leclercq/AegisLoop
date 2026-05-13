# Aegis Loop — Changelog du recentrage V1

> **Version :** 2.0  
> **Date :** 2026-04-23  
> **Objet :** Modifications apportées au corpus documentaire après audit critique

---

## Format

Chaque entrée suit le format : `Document → Élément → Action (Conservé/Simplifié/Déplacé V2/Supprimé/Ajouté) → Justification courte`

---

## README.md

| Élément | Action | Justification |
|---|---|---|
| Arborescence de repo | **Modifié** — Réduit de 14 à 6 projets C# | Sur-découpage modulaire |
| Périmètre MVP | **Modifié** — RSS + GDELT uniquement, plus YouTube/STAC | Connecteurs V2 |
| Stack cible | **Conservé** | Pas de changement technologique |
| Statut du projet | **Modifié** — "Spec-first, recentré après audit" | Réflecte le recentrage |
| Stratégie de développement | **Modifié** — V1 recentrée à 12 semaines | Planning réaliste solo |

## 00-expression-besoins.md

| Élément | Action | Justification |
|---|---|---|
| Sources MVP | **Modifié** — RSS + GDELT uniquement, plus YouTube/STAC/Import | Connecteurs V2 |
| Scénario 2 géospatial | **Simplifié** — Plus de STAC, concentré sur GDELT | STAC en V2 |
| Scénario 3 case file | **Reclassé SHOULD** — Était MUST | Pas critique pour la démo |
| Objectif O1 | **Simplifié** — "Au moins 2 types de sources" au lieu de 3 | RSS + GDELT |
| Non-objectifs | **Ajouté** — Timeline avancée, recherche avancée, watchlists en V2 | Réduction périmètre |
| Hypothèse H2 | **Modifiée** — RSS/GDELT suffisent (pas YouTube/STAC) | Connecteurs V2 |
| Priorités MVP | **Modifié** — Backlog réduit à 17 items | Recentrage |
| Feuille de route | **Modifié** — V1 à 12 semaines, contenu recentré | Planning réaliste |

## 01-specs-fonctionnelles.md

| Élément | Action | Justification |
|---|---|---|
| CU-01 Configurer connecteur | **Simplifié** — RSS et GDELT uniquement | Connecteurs V2 |
| CU-06 Contradictions | **Simplifié** — Détection + affichage, résolution manuelle | Pas de workflow complexe |
| CU-08 Export | **Simplifié** — Markdown + JSON uniquement, PDF en V2 | Complexité |
| CU-09 Recherche | **Simplifié** — Filtres simples, pas de recherche avancée | V2 |
| CU-10 Watchlists | **Supprimé V1** → V2 | Complexe |
| CU-11 Import manuel | **Supprimé V1** → V2 | Pas critique démo |
| F-01 Connecteurs | **Modifié** — RSS + GDELT uniquement, rate limiting concret | Connecteurs V2 |
| F-07 Scoring | **Modifié** — 3 composantes au lieu de 5 | Recentrage |
| F-10 Recherche | **Simplifié** — Filtres simples, pas de full-text avancé | V2 |
| F-13 Watchlists | **Supprimé V1** → V2 | Complexe |
| F-14 Export | **Simplifié** — Markdown + JSON, PDF en V2 | Complexité |
| Backlog MVP | **Réduit** — 32 → 17 items (12 Must + 5 Should) | Recentrage |
| Invariants métier | **Ajouté** — 10 invariants explicites | Lacune audit |
| Rate limiting connecteurs | **Ajouté** — Limites concrètes par connecteur | Lacune audit |
| Retry strategy | **Ajouté** — Exponentiel + circuit breaker | Lacune audit |
| Seed data | **Ajouté** — 2 scénarios concrets (Soudan, Golfe d'Aden) | Lacune audit |

## 02-architecture-technique.md

| Élément | Action | Justification |
|---|---|---|
| Structure projets | **Modifié** — 14 → 6 projets C# | Sur-découpage |
| Domain modèle | **Modifié** — 17 → 11 types | Sur-spécification |
| Scoring | **Modifié** — 5 → 3 composantes | Sur-complexité |
| Connecteurs | **Modifié** — 1 projet Connectors au lieu de 4 | 2 connecteurs V1 |
| Contracts | **Supprimé** — DTOs dans Application | Réduction |
| Fusion/Scoring/CaseManagement | **Fusionnés** dans Application | Pas de module séparé en V1 |
| Endpoints API | **Réduit** — 30+ → ~20 | API bureaucratique |
| Cycle de vie Electron/.NET | **Ajouté** — Démarrage, arrêt, crash, port, health check | Lacune audit |
| Stratégie configuration | **Ajouté** — Hiérarchie appsettings/user/env | Lacune audit |
| Chiffrement clés API | **Simplifié** — AES en V1, OS-native en V2 | Simplification |
| Invariants métier | **Ajouté** — 10 invariants explicites | Lacune audit |

## 03-specs-ui-ux.md

| Élément | Action | Justification |
|---|---|---|
| Vues UI | **Réduit** — 9 → 5 vues | Sur-ingénierie UI |
| Vue Contradictions | **Fusionné** dans EventCase (onglet) | Pas une vue séparée |
| Vue Export | **Fusionné** dans EventCase (bouton) | Pas une vue séparée |
| Vue Recherche avancée | **Fusionné** dans Observations (filtres) | Pas une vue séparée |
| Vue Connecteurs & Jobs | **Fusionné** dans Dashboard/Paramètres | Pas une vue séparée |
| Composants UI | **Réduit** — 14 → 10 (7 Must + 3 Should) | Recentrage |
| Raccourcis clavier | **Réduit** — 18 → 9 | Trop pour MVP |
| Scénarios GUI | **Réduit** — 3 → 2 MUST + 1 SHOULD | Recentrage |
| Seed data | **Ajouté** — 2 scénarios concrets définis | Lacune audit |
| Source Explorer | **Renommé** → Observations | Clarification |

## 04-manuel-utilisateur.md

| Élément | Action | Justification |
|---|---|---|
| Sections YouTube/STAC | **Supprimé V1** → V2 | Connecteurs V2 |
| Section Watchlists | **Supprimé V1** → V2 | V2 |
| Section Import manuel | **Supprimé V1** → V2 | V2 |
| Section Recherche avancée | **Simplifié** — Filtres simples | V2 |
| Section Export PDF | **Supprimé V1** → V2 | V2 |
| Section Timeline avancée | **Supprimé V1** → V2 | V2 |
| Navigation | **Modifié** — 5 vues au lieu de 9 | Recentrage |

## 05-plan-de-tests.md

| Élément | Action | Justification |
|---|---|---|
| Catégories de tests | **Réduit** — 11 → 5 | Cérémonial disproportionné |
| Tests performance | **Supprimé V1** → V2 | Pas de volume |
| Tests sécurité dédiés | **Supprimé V1** → Intégration | Vérifications basiques |
| Tests non-régression GUI | **Supprimé V1** → V2 | UI changeante |
| Matrice de tests | **Simplifiée** → Checklist de 10 tests critiques | Maintenance solo |
| Volume de tests | **Réduit** — 200+ → ~80-100 | Réaliste |
| Smoke tests démo | **Ajouté** — 8 scénarios de démo 5 min | Démontrabilité |
| Jeux de données de test | **Ajouté** — 4 niveaux concrets définis | Lacune audit |

## 06-dossier-presentation-produit.md

| Élément | Action | Justification |
|---|---|---|
| Pitch | **Modifié** — Aligné sur V1 recentrée | Cohérence |
| Fonctionnalités annoncées | **Réduit** — V1 uniquement | Ne pas annoncer ce qui n'existe pas |
| Connecteurs | **Modifié** — RSS + GDELT uniquement | V2 |
| Scénarios | **Modifié** — 2 MUST + 1 SHOULD | Recentrage |
| Timeline | **Modifié** — 12 semaines au lieu de 16 | Planning réaliste |

## 07-glossaire-et-decisions.md

| Élément | Action | Justification |
|---|---|---|
| Types de domaine | **Modifié** — 17 → 11 | Sur-spécification |
| Termes V2 ajoutés | **Ajouté** — Section V2 pour types repoussés | Traçabilité |
| Décisions post-audit | **Ajouté** — 14 arbitrages structurants | Référence |
| Scoring | **Modifié** — 5 → 3 composantes | Recentrage |

## ADR

| ADR | Action | Justification |
|---|---|---|
| 0001 Frontend Shell | **Conservé** — Aucun changement majeur | Electron reste pertinent |
| 0002 Backend modulaire | **Modifié** — 6 projets au lieu de 14 | Sur-découpage |
| 0003 Modèle de domaine | **Modifié** — 11 types au lieu de 17, 3 composantes scoring | Sur-spécification |
| 0004 Connecteurs OSINT | **Modifié** — RSS + GDELT uniquement en V1, V2 documenté | Connecteurs V2 |
| 0005 Persistance | **Conservé** — SQLite reste pertinent | Pas de changement |
| 0006 Stratégie tests | **Modifié** — 5 catégories au lieu de 11, ~100 tests | Cérémonial |
| 0007 Sécurité | **Modifié** — AES simplifié en V1, cycle de vie ajouté | Simplification |

## Fichiers .progress.md

| Fichier | Action | Justification |
|---|---|---|
| Tous | **Mis à jour** — Arbitrages intégrés, points ouverts, statuts | Réflecte le recentrage |

## Nouveaux fichiers

| Fichier | Action | Justification |
|---|---|---|
| 09-plan-de-recentrage-applique.md | **Créé** | Traçabilité des arbitrages |
| 10-mvp-solo-v1-officiel.md | **Créé** | Référence V1 officielle |
| 11-changelog-recentrage.md | **Créé** | Ce document |

---

## Résumé des actions par type

| Type d'action | Nombre |
|---|---|
| Modifié | 28 |
| Simplifié | 12 |
| Supprimé (V1) / Déplacé (V2) | 15 |
| Ajouté | 12 |
| Conservé | 4 |
| **Total** | **71** |