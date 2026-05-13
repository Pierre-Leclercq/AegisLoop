# Checklist avant implémentation

> **Date :** 2026-04-23  
> **Objectif :** Vérifier que le corpus est prêt à guider l'implémentation  
> **Prérequis :** Les actions correctives de l'audit doivent être traitées

---

## Instructions

Chaque item doit être vérifié avant le début de l'implémentation. Les items marqués 🔴 sont bloquants. Les items marqués 🟡 sont importants. Les items marqués 🟢 sont recommandés.

---

## 1. Positionnement produit

- [🟢] La proposition de valeur "fusion + confiance + feedback analyste" est claire et tenue
- [🟢] Le discours produit est crédible pour un porteur solo
- [🟢] Le positionnement évite la concurrence frontale avec les plateformes ISR

## 2. Périmètre MVP

- [🔴] Le backlog MVP est réduit à **17 items maximum** (12 Must + 5 Should)
- [🔴] Chaque item Must sert directement la démonstration
- [🔴] Les connecteurs YouTube, STAC, Import manuel sont repoussés en V2
- [🔴] Les fonctionnalités Watchlists, Export PDF, Timeline avancée, Recherche avancée sont repoussées en V2
- [🟡] Les scénarios de démo sont définis (géopolitique + maritime)
- [🟡] Les critères de "démo prête" sont définis (6 critères)

## 3. Modèle de domaine

- [🔴] Le domaine est réduit à **11 types maximum** en V1
- [🔴] Claim, Evidence, MediaAsset, SearchQuery, EntityLink, Watchlist sont repoussés en V2
- [🔴] Le scoring est réduit à **3 composantes** : FiabilitéSource, Corroboration, FeedbackAnalyste
- [🟡] Les invariants métier sont explicités (au moins 8-10)
- [🟡] Le calcul du score d'un EventCase est clarifié (moyenne ? min ? max ?)
- [🟢] Observation reste le centre de gravité du domaine

## 4. Architecture technique

- [🔴] Le découpage est réduit à **6-7 projets C#** maximum en V1
- [🔴] Les modules Fusion, Scoring, CaseManagement sont intégrés dans Application en V1
- [🔴] Les connecteurs RSS + GDELT sont dans un seul projet `AegisLoop.Connectors`
- [🟡] Le cycle de vie Electron ↔ .NET est spécifié (démarrage, arrêt, crash, port)
- [🟡] La stratégie de configuration est spécifiée (fichiers, hiérarchie, secrets)
- [🟡] Les endpoints API sont réduits à **15-20 maximum** en V1
- [🟢] Le chiffrement des clés API est simplifié (AES en V1, OS-native en V2)

## 5. GUI / UX

- [🔴] Les vues UI sont réduites à **5 maximum** en V1
- [🔴] Contradictions est un onglet dans EventCase, pas une vue séparée
- [🔴] Export est un bouton dans EventCase, pas une vue séparée
- [🟡] Les composants UI prioritaires sont identifiés (7 Must + 3 Should)
- [🟡] Les raccourcis clavier sont réduits à **8-10 maximum** en V1
- [🟢] La provenance et le scoring décomposé sont visibles en UI

## 6. Connecteurs OSINT

- [🔴] RSS/Atom + GDELT sont les **seuls** connecteurs V1
- [🟡] Les limites de rate limiting sont spécifiées concrètement
- [🟡] La stratégie de retry est détaillée (exponentiel, circuit breaker)
- [🟢] Les risques juridiques par connecteur sont documentés

## 7. Plan de tests

- [🔴] Les catégories de tests sont réduites à **5 maximum** en V1
- [🔴] Les tests de performance et de sécurité dédiés sont repoussés en V2
- [🟡] Les 10 tests critiques bloquants sont identifiés
- [🟡] Les jeux de données de test sont définis concrètement
- [🟢] Le smoke test de démo (5 min) est spécifié

## 8. Slicing vertical

- [🔴] Un guide de slicing vertical est ajouté au corpus
- [🔴] Le chemin critique pour la première démo est défini
- [🟡] L'ordre d'implémentation par phase est spécifié (3 phases × 4 semaines)
- [🟢] Les dépendances entre features sont explicites

## 9. Seed data / Mode démo

- [🔴] Les seed data sont définies concrètement (2 scénarios, 90 observations)
- [🟡] Chaque observation seed a : titre, contenu, source, date, localisation, score
- [🟢] Les seed data incluent des contradictions pour la démonstration
- [🟢] Le mode démo fonctionne sans connexion réseau

## 10. Cohérence documentaire

- [🟡] Les ADR sont mis à jour pour refléter les coupes V1
- [🟡] Les fichiers .progress.md sont mis à jour après les coupes
- [🟢] Les références croisées restent valides après les coupes
- [🟢] Le README reflète le MVP recentré

---

## Résumé de la checklist

| Catégorie | 🔴 Bloquants | 🟡 Importants | 🟢 Recommandés | Total |
|---|---|---|---|---|
| Positionnement produit | 0 | 0 | 3 | 3 |
| Périmètre MVP | 4 | 2 | 0 | 6 |
| Modèle de domaine | 3 | 2 | 1 | 6 |
| Architecture technique | 3 | 3 | 1 | 7 |
| GUI / UX | 3 | 2 | 1 | 6 |
| Connecteurs OSINT | 1 | 2 | 1 | 4 |
| Plan de tests | 2 | 2 | 1 | 5 |
| Slicing vertical | 2 | 1 | 1 | 4 |
| Seed data / Mode démo | 1 | 1 | 2 | 4 |
| Cohérence documentaire | 0 | 2 | 2 | 4 |
| **Total** | **19** | **17** | **13** | **49** |

---

## Critère de lancement

L'implémentation peut commencer quand :

1. ✅ Les **19 items 🔴 bloquants** sont tous cochés
2. ✅ Au moins **10 items 🟡 importants** sont cochés
3. ✅ Le guide de slicing vertical existe
4. ✅ Les seed data sont définies concrètement
5. ✅ Le cycle de vie Electron ↔ .NET est spécifié

**Si ces conditions ne sont pas remplies, le risque de paralysie architecturale ou de dérive de périmètre est élevé.**

---

## Actions correctives prioritaires (rappel)

| # | Action | Référence audit |
|---|---|---|
| 1 | Réduire à 6-7 projets C# | 03-analyse-architecture 1 |
| 2 | Couper le backlog à 17 items | 01-analyse-mvp 4 |
| 3 | Réduire à 5 vues UI | 04-analyse-ui-ux 2 |
| 4 | Simplifier le domaine à 11 types | 02-analyse-domaine 1-2 |
| 5 | Réduire le scoring à 3 composantes | 02-analyse-domaine 7 |
| 6 | Ajouter le slicing vertical | 01-analyse-mvp 5 |
| 7 | Définir les seed data | 04-analyse-ui-ux 7 |
| 8 | Spécifier le cycle de vie Electron ↔ .NET | 03-analyse-architecture 2 |
| 9 | Réduire les endpoints API à 15-20 | 03-analyse-architecture 4 |
| 10 | Réduire à 5 catégories de tests | 06-analyse-tests 2.1 |

---

## Références croisées

- Rapport global : [00-rapport-audit-global.md](00-rapport-audit-global.md)
- Analyse MVP : [01-analyse-mvp-et-perimetre.md](01-analyse-mvp-et-perimetre.md)
- Analyse domaine : [02-analyse-modele-de-domaine.md](02-analyse-modele-de-domaine.md)
- Analyse architecture : [03-analyse-architecture-technique.md](03-analyse-architecture-technique.md)
- Analyse UI/UX : [04-analyse-ui-ux.md](04-analyse-ui-ux.md)
- Analyse connecteurs : [05-analyse-connecteurs-osint.md](05-analyse-connecteurs-osint.md)
- Analyse tests : [06-analyse-plan-de-tests.md](06-analyse-plan-de-tests.md)
- Risques et recommandations : [07-risques-et-recommandations.md](07-risques-et-recommandations.md)