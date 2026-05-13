# Analyse du modèle de domaine

> **Date :** 2026-04-23  
> **Axe d'audit :** D — Modèle de domaine  
> **Gravité :** Importante

---

## Verdict : Domaine bien conçu mais sur-spécifié pour V1

Le modèle de domaine est conceptuellement juste. Les concepts clés (Observation, EventCase, ConfidenceScore, AnalystFeedback) sont pertinents et bien différenciés. Cependant, **17 types est trop pour un MVP** — certains concepts sont prématurés et ajoutent de la complexité sans valeur démonstrative immédiate.

Score : **7.0/10** — Bon mais à simplifier.

---

## 1. Concepts essentiels à garder en V1

| Type | Raison | Statut |
|---|---|---|
| **SourceConnector** | Fondation du pipeline d'ingestion | ✅ V1 Must |
| **ConnectorConfiguration** | Configuration nécessaire | ✅ V1 Must (simplifier en propriétés de SourceConnector) |
| **RawItem** | Stockage des données brutes avant normalisation | ✅ V1 Must |
| **Observation** | Centre de gravité du domaine — concept clé | ✅ V1 Must |
| **Entity** | Détection d'entités nécessaire pour la fusion | ✅ V1 Must |
| **EventCase** | Concept central du produit — dossier d'événements | ✅ V1 Must |
| **Location** | Géolocalisation essentielle pour la carte | ✅ V1 Must |
| **Contradiction** | Différenciateur produit | ✅ V1 Must (simplifier) |
| **ConfidenceScore** | Différenciateur produit — scoring explicable | ✅ V1 Must (3 composantes) |
| **AnalystFeedback** | Boucle de feedback — cœur du produit | ✅ V1 Must |
| **AuditEntry** | Traçabilité — exigence non négociable | ✅ V1 Must (simplifier) |

**Total V1 : 11 types** (au lieu de 17)

## 2. Concepts à repousser en V2

| Type | Raison du report | Recommandation |
|---|---|---|
| **Claim** | Concept utile mais superflu en V1 — l'Observation porte déjà la revendication | Repousser en V2. En V1, utiliser un champ `ClaimText` dans Observation |
| **Evidence** | Redondant avec Observation dans le contexte MVP | Repousser en V2. En V1, les observations SONT les preuves |
| **MediaAsset** | Aucun connecteur ne ramène de média en V1 | Repousser en V2 quand YouTube/STAC seront implémentés |
| **SearchQuery** | La recherche avancée est repoussée | Repousser en V2 |
| **EntityLink** | Les relations entre entités sont un enrichissement V2 | Repousser en V2 |
| **ReportExport** | L'export est un Should — le type peut être un simple DTO | Simplifier en DTO dans Application |
| **Watchlist** | Repoussé en V2 | Repousser en V2 |

## 3. Observation comme centre de gravité — bon choix

Le choix d'**Observation** comme unité fondamentale est correct. C'est le bon niveau d'abstraction :
- Pas trop abstrait (comme "Signal" ou "DataPoint")
- Pas trop concret (comme "RssItem" ou "GdeltEvent")
- Porte naturellement la provenance
- Se prête à la fusion et au scoring

**Validation :** Observation est le bon centre de gravité. Ne pas changer.

## 4. Problèmes de différenciation conceptuelle

### 4.1 Claim vs Observation — Ambiguïté

Le concept de Claim (revendication/fait rapporté) est trop proche d'Observation dans le contexte V1. Une Observation EST déjà un fait rapporté par une source. Ajouter Claim crée une ambiguïté :

- **Observation** = fait collecté depuis une source
- **Claim** = assertion extraite de cette observation

En V1, cette distinction n'apporte pas de valeur démonstrative. **Recommandation :** fusionner Claim dans Observation via un champ `ClaimText` et `ClaimConfidence`.

### 4.2 Evidence vs Observation — Redondance

Evidence est défini comme "un fait utilisé pour étayer un événement". Mais en V1, les observations SONT les preuves. Créer un type séparé ajoute de la complexité sans valeur métier.

**Recommandation :** repousser Evidence en V2. En V1, EventCase référence directement des Observations.

### 4.3 ConnectorConfiguration — Peut être simplifié

ConnectorConfiguration comme type séparé est correct architecturalement, mais pour V1, il peut être un simple dictionnaire dans SourceConnector.

**Recommandation :** fusionner ConnectorConfiguration dans SourceConnector en V1.

## 5. Provenance — Citoyen de première classe ✅

La provenance est bien modélisée :
- Chaque Observation porte `SourceConnectorId`, `CollectedAt`, `RawItemId`
- La chaîne source → collecte → normalisation → enrichissement est traçable
- L'UI affiche la provenance décomposée

C'est un **point fort** du modèle. Ne pas réduire.

## 6. Contradictions — Bien mais à simplifier

Le concept de Contradiction est un différenciateur produit important. Cependant, la modélisation V1 est trop riche :

- **V1 spécifie :** type de contradiction, sévérité, résolution, historique
- **V1 recommandée :** détection automatique simple + affichage + résolution manuelle par l'analyste

**Recommandation :** Simplifier Contradiction en V1 — détecter, afficher, laisser l'analyste résoudre. Pas de workflow de résolution complexe.

## 7. ConfidenceScore — Réduire à 3 composantes

Le scoring à 5 composantes (FiabilitéSource, Fraîcheur, Corroboration, Spécificité, FeedbackAnalyste) est conceptuellement juste mais trop pour V1 :

| Composante | V1 ? | Raison |
|---|---|---|
| FiabilitéSource | ✅ | Essentiel — différencie RSS fiable de GDELT brut |
| Fraîcheur | ❌ | Repousser — la fraîcheur est un tri, pas un score de confiance |
| Corroboration | ✅ | Essentiel — cœur de la fusion |
| Spécificité | ❌ | Repousser — trop subjectif à calibrer |
| FeedbackAnalyste | ✅ | Essentiel — boucle de feedback |

**Recommandation :** 3 composantes en V1. Ajouter Fraîcheur et Spécificité en V2.

## 8. Invariants métier manquants ou ambigus

| Invariant | Statut | Action |
|---|---|---|
| Un EventCase doit avoir au moins 1 Observation | À spécifier | Ajouter dans les invariants |
| Un ConfidenceScore doit être entre 0 et 1 | À spécifier | Ajouter dans les invariants |
| Une Observation ne peut pas avoir un ConfidenceScore négatif | À spécifier | Ajouter dans les invariants |
| Un AnalystFeedback ne peut pas être modifié après création | À vérifier | Clarifier : append-only ou mutable ? |
| Deux Contradictions ne peuvent pas se chevaucher sur les mêmes Observations | À spécifier | Ajouter une règle de déduplication |
| Le score d'un EventCase est la moyenne pondérée des Observations | À clarifier | Est-ce une moyenne ? Un min ? Un max ? Préciser |

**Recommandation :** Ajouter une section "Invariants métier" au modèle de domaine avec au moins 8-10 invariants explicites.

## 9. Résumé des recommandations domaine V1

| Action | Gravité | Impact |
|---|---|---|
| Garder 11 types, repousser 6 en V2 | Importante | Architecture |
| Simplifier ConfidenceScore à 3 composantes | Importante | Domaine |
| Fusionner Claim dans Observation | Modérée | Domaine |
| Fusionner Evidence dans Observation | Modérée | Domaine |
| Fusionner ConnectorConfiguration dans SourceConnector | Mineure | Domaine |
| Ajouter les invariants métier manquants | Importante | Domaine |
| Simplifier Contradiction en V1 | Modérée | Domaine |