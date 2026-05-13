# Risques et recommandations

> **Date :** 2026-04-23  
> **Axe d'audit :** Transversal — Risques et recommandations  
> **Gravité :** De critique à mineure

---

## 1. Risques critiques

### R1 — Paralysie par l'architecture
**Gravité :** Critique | **Impact :** Planning | **Probabilité :** Élevée

14 projets C#, 30+ endpoints, 17 types de domaine, 9 vues UI — le porteur solo risque de passer plus de temps à maintenir l'architecture qu'à livrer de la valeur.

**Mitigation :** Réduire à 6-7 projets, 15-20 endpoints, 11 types, 5 vues. Commencer par le chemin critique vertical.

### R2 — Dérive de périmètre
**Gravité :** Critique | **Impact :** Produit | **Probabilité :** Élevée

32 items de backlog MVP, c'est 1.5 à 2 fois trop pour un porteur solo en 3-4 mois. Le risque est de commencer partout et de ne rien finir.

**Mitigation :** Verrouiller le backlog à 17 items. Chaque item Must doit servir directement la démo.

### R3 — Absence de slicing vertical
**Gravité :** Critique | **Impact :** Planning | **Probabilité :** Élevée

Le corpus spécifie par couche horizontale, pas par feature. Le porteur solo risque d'implémenter Domain entièrement, puis Application entièrement, etc., sans jamais avoir de démo fonctionnelle.

**Mitigation :** Ajouter un guide de slicing vertical. Implémenter par chemin critique : un connecteur → pipeline → scoring → UI → démo.

---

## 2. Risques importants

### R4 — Sur-complexité du modèle de domaine
**Gravité :** Importante | **Impact :** Architecture | **Probabilité :** Moyenne

Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist — 6 types qui ne servent pas la démo V1.

**Mitigation :** Repousser ces 6 types en V2. Utiliser des champs simples dans les types existants.

### R5 — GUI trop grande
**Gravité :** Importante | **Impact :** UX/Planning | **Probabilité :** Moyenne

9 vues × (routing + layout + états + tests) = beaucoup de travail UI pour peu de valeur démonstrative.

**Mitigation :** 5 vues en V1. Contradictions et Export dans EventCase. Connecteurs dans Paramètres.

### R6 — Seed data non définies
**Gravité :** Importante | **Impact :** Démontrabilité | **Probabilité :** Élevée

Le mode démo est le cœur de la démontrabilité, mais les seed data ne sont pas définies concrètement.

**Mitigation :** Définir 2 scénarios de seed data avec 90 observations, 8 événements, 3 contradictions. Chaque observation avec titre, contenu, source, date, localisation, score.

### R7 — Cycle de vie Electron ↔ .NET non spécifié
**Gravité :** Importante | **Impact :** Architecture | **Probabilité :** Faible

Le démarrage, l'arrêt, les crashes et les conflits de port ne sont pas spécifiés.

**Mitigation :** Ajouter un document "Cycle de vie Electron ↔ .NET" avec démarrage, arrêt, crash recovery, port conflict.

---

## 3. Risques modérés

### R8 — Scoring à 5 composantes trop complexe pour V1
**Gravité :** Modérée | **Impact :** Domaine | **Probabilité :** Faible

5 composantes de scoring = 5 pondérations à calibrer, 5 composantes à afficher, 5 à expliquer.

**Mitigation :** 3 composantes en V1 : FiabilitéSource, Corroboration, FeedbackAnalyste.

### R9 — Plan de tests disproportionné
**Gravité :** Modérée | **Impact :** QA | **Probabilité :** Faible

11 catégories de tests pour un prototype solo, c'est du cérémonial QA.

**Mitigation :** 5 catégories en V1, 80-100 tests au lieu de 200+.

### R10 — Connecteurs Should glissent vers Must
**Gravité :** Modérée | **Impact :** Planning | **Probabilité :** Moyenne

YouTube, STAC et Import manuel sont classés Should mais pourraient glisser vers Must.

**Mitigation :** Verrouiller : RSS + GDELT uniquement en V1. Les autres connecteurs sont pré-architectés mais pas implémentés.

### R11 — Configuration non spécifiée
**Gravité :** Modérée | **Impact :** Architecture | **Probabilité :** Faible

Où sont stockées les clés API ? Quelle est la hiérarchie de configuration ?

**Mitigation :** Ajouter une section stratégie de configuration dans l'architecture.

### R12 — Audit trail comme surcoût invisible
**Gravité :** Modérée | **Impact :** Performance | **Probabilité :** Faible

Chaque action génère une AuditEntry. Sur 10 000 observations et 100 feedbacks, c'est 10 100+ entrées d'audit.

**Mitigation :** Implémenter l'audit trail mais avec un purge configurable (> 90 jours). Ne pas indexer chaque champ d'AuditEntry.

---

## 4. Risques mineurs

### R13 — Documentation trop volumineuse pour un porteur solo
**Gravité :** Mineure | **Impact :** Maintenance | **Probabilité :** Faible

24 fichiers de documentation, c'est beaucoup à maintenir.

**Mitigation :** Les fichiers .progress.md aident. Accepter que la documentation évolue avec le code.

### R14 — ADR figés trop tôt
**Gravité :** Mineure | **Impact :** Architecture | **Probabilité :** Faible

7 ADR figés avant la première ligne de code. Certains choix pourraient être remis en question.

**Mitigation :** Les ADR sont des décisions enregistrées, pas des contrats gravés dans le marbre. Ils peuvent être amendés.

---

## 5. Recommandations prioritaires

### Priorité 1 — Avant implémentation (bloquantes)

| # | Recommandation | Impact | Effort |
|---|---|---|---|
| 1 | Réduire à 6-7 projets C# | Architecture/Planning | Faible |
| 2 | Couper le backlog MVP à 17 items | Produit/Planning | Faible |
| 3 | Réduire à 5 vues UI | UX/Planning | Faible |
| 4 | Simplifier le domaine à 11 types | Architecture | Faible |
| 5 | Ajouter le slicing vertical | Planning | Moyen |
| 6 | Définir les seed data concrètes | Démontrabilité | Moyen |
| 7 | Spécifier le cycle de vie Electron ↔ .NET | Architecture | Moyen |

### Priorité 2 — Pendant l'implémentation (importantes)

| # | Recommandation | Impact | Effort |
|---|---|---|---|
| 8 | Scoring à 3 composantes | Domaine | Faible |
| 9 | 15-20 endpoints API | Architecture | Faible |
| 10 | 5 catégories de tests | QA | Faible |
| 11 | 8-10 raccourcis clavier | UX | Faible |
| 12 | Stratégie de configuration | Architecture | Faible |

### Priorité 3 — Plus tard (améliorations)

| # | Recommandation | Impact | Effort |
|---|---|---|---|
| 13 | Ajouter les invariants métier manquants | Domaine | Faible |
| 14 | Détailler la stratégie de retry | Architecture | Faible |
| 15 | Ajouter les rate limiting concrets | Connecteurs | Faible |
| 16 | Schéma JSON d'export | Produit | Moyen |
| 17 | Tests de performance en V2 | QA | Moyen |

---

## 6. Carte des dépendances entre risques

```
R1 (Paralysie archi) ──→ R4 (Sur-domaine) ──→ R8 (Scoring 5 composantes)
       │                                          │
       └──→ R2 (Dérive périmètre) ──→ R5 (GUI trop grande)
                    │
                    └──→ R3 (Pas de slicing vertical)
                              │
                              └──→ R6 (Seed data absentes)

R7 (Cycle de vie) ──→ Indépendant
R9 (Tests) ──→ Indépendant  
R10 (Connecteurs) ──→ R2 (Dérive périmètre)
R11 (Configuration) ──→ Indépendant
R12 (Audit trail) ──→ Indépendant
```

Les risques R1, R2, R3 sont les plus critiques et interconnectés. Les traiter en premier débloque tout le reste.

---

## 7. Proposition de "MVP solo réaliste"

### Ce qui reste

| Composant | V1 Réduite |
|---|---|
| Connecteurs | RSS + GDELT |
| Domaine | 11 types |
| Scoring | 3 composantes |
| Vues UI | 5 |
| Backlog | 17 items |
| Projets C# | 6-7 |
| Endpoints API | ~20 |
| Tests | ~100 |
| Catégories de tests | 5 |

### Ce qui est repoussé

| Composant | V2+ |
|---|---|
| Connecteurs YouTube, STAC, Import | V2 |
| Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist | V2 |
| Scoring Fraîcheur, Spécificité | V2 |
| Vues Contradictions, Export, Recherche avancée | V2 |
| Tests performance, sécurité, non-régression GUI | V2 |
| 7 projets C# supplémentaires (Fusion, Scoring, CaseManagement séparés) | V3 |

### Planning estimé

| Phase | Semaines | Contenu |
|---|---|---|
| Phase 1 | 1-4 | Domain model + RSS connector + Pipeline + API minimale + Dashboard basique |
| Phase 2 | 5-8 | GDELT + Scoring + Feedback + Carte/Timeline + Provenance |
| Phase 3 | 9-12 | Mode démo + Seed data + Contradictions + Export Markdown + Polish |

**Total : 12 semaines (3 mois)** au lieu de 16 semaines (4 mois) grâce aux coupes.