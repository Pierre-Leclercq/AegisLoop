# Analyse MVP et périmètre

> **Date :** 2026-04-23  
> **Axe d'audit :** B — Périmètre MVP  
> **Gravité des enjeux :** Critique

---

## Verdict : MVP trop large — recentrage nécessaire

Le backlog MVP contient **32 items** (20 Must + 12 Should). Pour un porteur solo en 3-4 mois, c'est **1.5 à 2 fois trop**. Un MVP crédible doit tenir en 15-18 items maximum, et chaque item doit servir directement la démonstration.

---

## 1. Ce qui est bien cadré

- **2 scénarios MUST** (crise géopolitique, suivi maritime) sont pertinents et démontrables
- **RSS + GDELT** comme connecteurs principaux est réaliste
- **Scoring heuristique explicable** est le bon choix pour V1
- **Mode démo** intégré est un atout stratégique
- **Provenance et audit trail** sont des différenciateurs réels

## 2. Ce qui glisse du Should vers le Must

Les items suivants sont classés Should mais risquent de glisser :

| Item | Risque de glissement | Recommandation |
|---|---|---|
| Connecteur YouTube | Quota API, complexité | **Repousser en V2** |
| Connecteur STAC | Peu de valeur démo immédiate | **Repousser en V2** |
| Import manuel de fichiers | Utile mais pas critique pour la démo | **Repousser en V2** |
| Watchlists | Complexe à implémenter correctement | **Simplifier en V1** |
| Export PDF | Complexité de génération | **Repousser en V2** |
| Timeline avancée | Nécessite bibliothèque spécialisée | **Simplifier en V1** |

## 3. Ce qui doit être coupé du MVP

| Fonctionnalité | Raison de la coupe | Destion |
|---|---|---|
| Connecteur YouTube | Quota, CGU, complexité | V2 |
| Connecteur STAC | Pas de valeur démo immédiate | V2 |
| Import manuel CSV/JSON/GeoJSON/KML | Utile mais pas critique | V2 |
| Export PDF | Complexité, Markdown suffit | V2 |
| Watchlists avancées | Complexité de matching | V2 (tags simples en V1) |
| Recherche avancée plein texte | SQLite FTS5 suffira, pas de UI complexe | V2 |
| Timeline avancée | Composant lourd | V2 (timeline simple en V1) |
| Gestion des contradictions avancée | Résolution analyste suffit | V2 (détection + affichage en V1) |
| Active learning | Prématuré | V2 |
| Rapports personnalisables | Sur-ingénierie | V2 |

## 4. Le "vrai MVP solo"

### Backlog MVP recentré (17 items)

**Must (12)** :
1. Connecteur RSS/Atom — collecte et polling
2. Connecteur GDELT — collecte et filtrage
3. Pipeline d'ingestion — collecte → normalisation → enrichissement
4. Normalisation vers Observation — modèle unifié
5. Détection d'entités basique — dictionnaire + regex
6. Fusion/correlation d'observations en événements — clustering temporel + géographique
7. Scoring heuristique — 3 composantes (FiabilitéSource, Corroboration, FeedbackAnalyste)
8. Dashboard — KPIs, événements prioritaires, contradictions
9. Vue EventCase — détail, observations, provenance, score décomposé
10. Vue Carte + Timeline simple — observations et événements
11. Feedback analyste — valider/invalider/corriger une observation
12. Mode démo avec seed data

**Should (5)** :
1. Export Markdown/JSON
2. Audit trail
3. Tags/notes simples
4. Contradictions — détection et affichage
5. Configuration des connecteurs

### V2 (items repoussés)
- YouTube, STAC, Import manuel
- Watchlists
- Export PDF
- Timeline avancée
- Active learning
- Recherche avancée
- NLP/entity linking

---

## 5. Slicing vertical proposé

Le corpus spécifie par couche horizontale. Pour un porteur solo, il faut un **chemin critique vertical** :

### Phase 1 — Démo minimale (semaines 1-4)
- Domain model (10 types essentiels)
- Connecteur RSS basique
- Pipeline ingestion → normalisation
- API minimale (5 endpoints)
- UI : Dashboard + EventCase basique

### Phase 2 — Valeur métier (semaines 5-8)
- Connecteur GDELT
- Scoring heuristique
- Feedback analyste
- Carte + Timeline simple
- Provenance visible

### Phase 3 — Démonstrabilité (semaines 9-12)
- Mode démo + seed data
- Contradictions
- Export Markdown
- Audit trail
- Polish UI

---

## 6. Critères de "démo prête"

Le MVP est prêt à être démontré quand :
1. ✅ Le dashboard affiche des événements avec scores décomposés
2. ✅ Un EventCase montre la provenance complète d'une observation
3. ✅ L'analyste peut valider/invalider une observation et voir l'impact sur le score
4. ✅ La carte montre les observations géolocalisées
5. ✅ Le mode démo fonctionne sans réseau
6. ✅ Les contradictions sont visibles dans l'interface

**Pas besoin de** : YouTube, STAC, export PDF, watchlists, recherche avancée, timeline avancée.

---

## 7. Risque de dérive périmètre

| Signal | Risque | Mitigation |
|---|---|---|
| "On peut ajouter YouTube rapidement" | Glissement vers 4 connecteurs | Verrouiller : RSS + GDELT uniquement en V1 |
| "Le scoring à 5 composantes n'est pas si compliqué" | Complexité de calcul et d'UI | Verrouiller : 3 composantes en V1 |
| "Les contradictions sont centrales" | Sur-ingénierie du modèle | Détection simple + affichage, résolution analyste manuelle |
| "Il faut des watchlists" | Feature creep | Tags simples en V1, watchlists en V2 |
| "L'export PDF est attendu" | Complexité de génération | Markdown + JSON suffisent pour démo |