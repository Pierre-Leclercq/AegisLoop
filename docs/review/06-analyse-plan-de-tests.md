# Analyse du plan de tests

> **Date :** 2026-04-23  
> **Axe d'audit :** G — Plan de tests  
> **Gravité :** Modérée

---

## Verdict : Plan complet mais disproportionné pour un MVP solo

Le plan de tests est structuré, réaliste dans ses principes et bien hiérarchisé. Cependant, **11 catégories de tests pour un porteur solo en phase prototype**, c'est du cérémonial QA disproportionné. La pyramide 60/20/20 est correcte, mais le niveau de détail est celui d'un produit mature, pas d'un démonstrateur.

Score : **6.0/10** — Disproportionné.

---

## 1. Ce qui est bien

- **Pyramide 60/20/20** — Bon ratio pour un projet solo
- **xUnit + FluentAssertions** — Bon choix, standard .NET
- **Playwright pour E2E** — Supporte Electron, moderne
- **SQLite en mémoire pour les tests d'intégration** — Simple, efficace
- **Tests de contrat pour connecteurs** — Essentiel pour l'interface `ISourceConnector`
- **Smoke test de démo en 5 min** — Excellent pour la crédibilité
- **Scénarios E2E détaillés** — Les 4 scénarios sont pertinents

## 2. Ce qui est disproportionné

### 2.1 Trop de catégories

11 catégories de tests (unitaires, intégration, contrat, fusion, API, GUI, E2E, non-régression, robustesse, performance, sécurité) c'est trop pour V1.

**Recommandation V1 : 5 catégories**

| Catégorie V1 | Outil | Priorité |
|---|---|---|
| **Tests unitaires** | xUnit + FluentAssertions | Must — Domain, Application, Scoring |
| **Tests d'intégration** | xUnit + WebApplicationFactory | Must — API, Pipeline, Persistance |
| **Tests de contrat connecteurs** | xUnit | Must — ISourceConnector |
| **Tests E2E (smoke)** | Playwright | Must — Scénarios de démo |
| **Tests de robustesse** | xUnit | Should — Erreurs, retries, edge cases |

Les tests de performance, de sécurité, de non-régression GUI et de fusion/scoring avancés sont repoussés en V2.

### 2.2 Tests de performance prématurés

Pour un prototype desktop mono-utilisateur avec SQLite et < 100k observations, les tests de performance sont prématurés.

**Recommandation :** Pas de tests de performance en V1. Simplement des seuils dans les specs :
- Dashboard < 2s
- Recherche < 500ms
- Chargement seed data < 5s

### 2.3 Tests de sécurité locaux — Trop de cérémonial

L'audit de sécurité V1 devrait se limiter à :
- API localhost uniquement
- CORS restrictif
- Validation des entrées
- Pas de SQL brut

Pas besoin d'une catégorie de tests séparée. Ces vérifications font partie des tests d'intégration.

### 2.4 Matrices de tests trop denses

La matrice CU × types de tests est exhaustive mais trop détaillée pour V1. Un porteur solo ne maintiendra pas cette matrice à jour.

**Recommandation :** Remplacer par une checklist de tests critiques :
- [ ] Observation est normalisée correctement (RSS)
- [ ] Observation est normalisée correctement (GDELT)
- [ ] Scoring produit un score entre 0 et 1
- [ ] Feedback analyste modifie le score
- [ ] Contradiction est détectée
- [ ] Provenance est traçable
- [ ] Dashboard affiche les événements
- [ ] Mode démo fonctionne sans réseau

---

## 3. Tests indispensables avant démo

| # | Test | Type | Priorité |
|---|---|---|---|
| 1 | Normalisation RSS → Observation | Unitaire | Must |
| 2 | Normalisation GDELT → Observation | Unitaire | Must |
| 3 | Scoring avec 3 composantes | Unitaire | Must |
| 4 | Fusion d'observations en événement | Unitaire | Must |
| 5 | Détection de contradiction | Unitaire | Must |
| 6 | Feedback analyste modifie le score | Unitaire | Must |
| 7 | Pipeline ingestion bout en bout | Intégration | Must |
| 8 | API CRUD observations | Intégration | Must |
| 9 | Contrat ISourceConnector | Contrat | Must |
| 10 | Smoke test démo 5 min | E2E | Must |

**10 tests critiques** suffisent pour valider la démo. Les autres sont importants mais pas bloquants.

---

## 4. Tests qui peuvent attendre V2

| Test | Raison |
|---|---|
| Tests de performance | Pas de volume en V1 |
| Tests de sécurité dédiés | Vérifications basiques dans les tests d'intégration |
| Tests de non-régression GUI | Trop tôt, l'UI va changer |
| Tests de robustesse avancés | Importants mais pas bloquants pour la démo |
| Matrices de tests exhaustives | Trop de maintenance pour un porteur solo |

---

## 5. Jeux de données de test — Insuffisamment spécifiés

Le corpus mentionne 4 niveaux de données de test mais ne définit pas leur contenu concret.

**Recommandation :** Définir explicitement :

| Niveau | Contenu | Taille |
|---|---|---|
| Factory | 1-5 observations par test | Minimal |
| Integration | 50 observations, 10 événements | Petit |
| E2E | 90 observations, 8 événements, 3 contradictions | Démo |
| Performance | 10 000 observations | V2 |

Les données E2E doivent correspondre exactement aux seed data du mode démo.

---

## 6. CI GitHub Actions — Réaliste

Le workflow CI proposé (build → test → lint → package) est adapté. Pas de sur-ingénierie ici.

**Recommandation :** Garder le workflow tel quel, mais ajouter un step "smoke test démo" qui vérifie que le mode démo fonctionne.

---

## 7. Résumé

| Action | Gravité | Urgence |
|---|---|---|
| Réduire à 5 catégories de tests en V1 | Importante | Avant implémentation |
| Identifier 10 tests critiques bloquants | Importante | Avant implémentation |
| Supprimer les tests de performance de V1 | Modérée | Pendant implémentation |
| Supprimer les tests de sécurité dédiés de V1 | Modérée | Pendant implémentation |
| Définir les jeux de données de test concrets | Importante | Avant implémentation |
| Simplifier la matrice en checklist | Modérée | Pendant implémentation |
| Ajouter un step smoke test démo dans CI | Mineure | Pendant implémentation |

---

## 8. Pyramide de tests réaliste pour un porteur solo

```
        ┌─────────────┐
        │  E2E (10%)  │  Smoke tests démo
        ├─────────────┤
        │ Intégration │  API, Pipeline, Persistance
        │    (25%)    │
        ├─────────────┤
        │  Contrat    │  ISourceConnector
        │    (10%)    │
        ├─────────────┤
        │  Unitaires  │  Domain, Application, Scoring
        │    (55%)    │
        └─────────────┘
```

**Total V1 : ~80-100 tests** (au lieu de 200+).