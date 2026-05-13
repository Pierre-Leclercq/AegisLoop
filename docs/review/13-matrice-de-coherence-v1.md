# Aegis Loop — Matrice de cohérence V1

> **Version :** 1.0  
> **Date :** 2026-04-23  
> **Source de vérité :** [10-mvp-solo-v1-officiel.md](10-mvp-solo-v1-officiel.md)

---

## Matrice croisée

| Axe V1 | 01-specs | 02-archi | 03-ui-ux | 04-manuel | 05-tests | 06-pitch | 07-glossaire | ADR 0007 |
|---|---|---|---|---|---|---|---|---|
| **Backlog (17 items)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Domaine (11 types)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Architecture (6 projets)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné |
| **GUI (5 vues, 10 composants, 9 raccourcis)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Connecteurs (RSS + GDELT)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné |
| **Scoring (3 composantes)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **API (~20 endpoints)** | ✅ Aligné | ✅ Aligné | N/A | N/A | ✅ Aligné | N/A | ✅ Aligné | N/A |
| **Tests (5 catégories, ~80-100)** | ✅ Aligné | N/A | N/A | N/A | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Seed data (2 scénarios, 90 obs)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Démo prête (12 conditions)** | ✅ Aligné | N/A | N/A | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A | N/A |
| **Contradictions (simplifié)** | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A |
| **Claim/Evidence/Config fusionnés** | ✅ Aligné | ✅ Aligné | N/A | N/A | N/A | ✅ Aligné | ✅ Aligné | ✅ Aligné |
| **Sécurité locale** | ✅ Aligné | ✅ Aligné | N/A | ✅ Aligné | ✅ Aligné | ✅ Aligné | N/A | ✅ Aligné |

---

## Légende

| Statut | Signification |
|---|---|
| ✅ Aligné | Document strictement cohérent avec `10-mvp-solo-v1-officiel.md` |
| 🟡 Partiellement aligné | Cohérent mais avec réserves mineures |
| ❌ Non aligné | Contradiction avec la V1 officielle |
| N/A | Axe non applicable à ce document |

---

## Résultat global

| Axe V1 | Statut global |
|---|---|
| Backlog (17 items) | ✅ Aligné |
| Domaine (11 types) | ✅ Aligné |
| Architecture (6 projets) | ✅ Aligné |
| GUI (5 vues, 10 composants, 9 raccourcis) | ✅ Aligné |
| Connecteurs (RSS + GDELT) | ✅ Aligné |
| Scoring (3 composantes) | ✅ Aligné |
| API (~20 endpoints) | ✅ Aligné |
| Tests (5 catégories, ~80-100) | ✅ Aligné |
| Seed data (2 scénarios, 90 obs) | ✅ Aligné |
| Démo prête (12 conditions) | ✅ Aligné |
| Contradictions (simplifié) | ✅ Aligné |
| Claim/Evidence/Config fusionnés | ✅ Aligné |
| Sécurité locale | ✅ Aligné |

**Tous les axes V1 sont alignés. Aucun axe partiellement aligné ou non aligné.**

---

## Détail par document

### 01-specs-fonctionnelles.md — ✅ Aligné
- Backlog exactement 17 items (12 Must + 5 Should)
- Domaine exactement 11 types
- Scoring exactement 3 composantes
- Connecteurs RSS + GDELT uniquement
- Seed data 2 scénarios, 90 obs
- "Démo prête" 12 conditions reprises
- Types exclus documentés

### 02-architecture-technique.md — ✅ Aligné
- 6 projets C# + desktop-electron
- 11 types avec définitions complètes
- ~20 endpoints listés
- Cycle de vie Electron ↔ .NET spécifié
- Connecteurs RSS + GDELT, rate limiting, retry
- Types exclus documentés
- 10 invariants métier

### 03-specs-ui-ux.md — ✅ Aligné
- 5 vues listées et détaillées
- 10 composants (7 Must + 3 Should)
- 9 raccourcis listés
- Vues fusionnées documentées
- Scénarios GUI alignés sur seed data

### 04-manuel-utilisateur.md — ✅ Aligné
- Parcours V1 uniquement
- Aucune section YouTube/STAC/Watchlists/PDF
- Mode démo documenté
- Raccourcis cohérents
- Dépannage aligné

### 05-plan-de-tests.md — ✅ Aligné
- 5 catégories de tests
- ~80-100 tests estimés
- 10 tests critiques listés
- Smoke tests démo (8 checks)
- Tests exclus documentés (performance, sécurité, non-régression)

### 06-dossier-presentation-produit.md — ✅ Aligné
- Pitch ne promet que ce que V1 démontre
- Section "Ce que V1 ne fait PAS" explicite
- 12 conditions "Démo prête" reprises
- Scénarios alignés sur seed data

### 07-glossaire-et-decisions.md — ✅ Aligné
- Glossaire V1 nettoyé (pas de termes fantômes)
- Termes exclus documentés avec destination
- 14 arbitrages structurants (A1-A14) cohérents
- Points ouverts listés

### ADR 0007 — ✅ Aligné
- AES en V1 (pas DPAPI/Keychain)
- ReportExport supprimé (DTO Application)
- Sécurité locale cohérente avec 02-architecture

---

## Conclusion

**Matrice de cohérence : 13/13 axes alignés.**

Le corpus documentaire Aegis Loop V1 est **synchronisé et prêt** pour servir de base officielle avant implémentation.