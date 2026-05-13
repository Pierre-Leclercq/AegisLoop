# Rapport d'audit critique — Corpus documentaire Aegis Loop

> **Date :** 2026-04-23  
> **Auditeur :** Reviewer senior indépendant  
> **Objet :** Évaluation du corpus spec-first avant implémentation  
> **Version du corpus auditée :** 1.0

---

## 1. Résumé exécutif

Le corpus documentaire d'Aegis Loop est **volumineux, structuré et globalement cohérent**. Il démontre une intention architecturale sérieuse et un positionnement produit clair. Cependant, il souffre de **trois défauts systémiques** qui menacent la faisabilité du MVP pour un porteur solo :

1. **Sur-découpage modulaire** — 14 projets C# pour un MVP solo crée une bureaucratie d'architecture disproportionnée à la valeur délivrée.
2. **Périmètre MVP trop large** — Les 32 items du backlog MVP, les 9 vues UI et les 17 fonctionnalités détaillées correspondent davantage à une V1 produit qu'à un démonstrateur solo.
3. **Modèle de domaine sur-spécifié** — 17 types de domaine avec invariants complexes pour un MVP qui doit d'abord prouver la valeur de la fusion + provenance + feedback.

Le positionnement "fusion + confiance + feedback analyste" est **réellement porteur** et bien articulé. La spécification de la provenance et du scoring heuristique est un point fort. Mais le corpus ne respecte pas son propre principe "éviter le piège boil the ocean" — il a lui-même bouilli l'océan en spéculaire.

**Verdict : GO avec coupes significatives.** Le corpus est une base solide, mais doit être recentré avant implémentation.

---

## 2. Verdict global

**GO avec coupes** — Le corpus peut servir de base à l'implémentation, à condition de :
- réduire le découpage modulaire de 14 à 6-7 projets maximum en V1,
- couper 40% du backlog MVP (repousser en V2),
- fusionner des vues UI (de 9 à 5-6 vues en V1),
- simplifier le modèle de domaine (repousser Claim, Evidence, MediaAsset, SearchQuery en V2),
- réduire le plan de tests à l'essentiel pour un prototype.

---

## 3. Score global : 6.8 / 10

Le corpus est au-dessus de la moyenne mais en dessous du niveau exigé pour un lancement d'implémentation sans risque de dérive.

---

## 4. Tableau de score par axe

| Axe | Score /10 | Verdict | Commentaire |
|---|---|---|---|
| Positionnement produit | **8.5** | Solide | Edge clair, discours crédible, métaphore porteuse |
| Cohérence MVP | **5.5** | Insuffisant | Trop large, 32 items c'est une V1 pas un MVP |
| Réalisme porteur solo | **5.0** | Insuffisant | 14 projets C#, 9 vues, 30+ endpoints = sur-ingénierie |
| Modèle de domaine | **7.0** | Bon mais sur-spécifié | Concepts justes, mais trop pour V1 |
| Architecture technique | **6.5** | Correct mais lourd | SOLID bien compris, découpage excessif |
| Stratégie connecteurs OSINT | **7.5** | Bon | Sources bien choisies, risques juridiques documentés |
| GUI / UX | **6.0** | Ambitieux | 9 vues et 14 composants pour un MVP solo |
| Qualité documentaire | **7.5** | Bon | Structuré, croisé, progress files utiles |
| Testabilité | **6.0** | disproportionné | Pyramide correcte mais trop de catégories pour V1 |
| Démontrabilité | **7.0** | Bon | Mode démo intégré, scénarios de démo clairs |

---

## 5. Principaux points forts

1. **Positionnement "fusion + confiance + feedback"** — C'est réellement un edge défendable et différenciant. Le corpus le tient de bout en bout.
2. **Provenance comme citoyen de première classe** — La chaîne de provenance est spécifiée à tous les niveaux (domaine, UI, export, audit). C'est rare et précieux.
3. **Scoring heuristique explicable** — Le modèle multicritères avec poids configurables et décomposition visible est un excellent choix pour V1.
4. **Réalisme des sources OSINT** — RSS et GDELT sont des choix solides. Les risques juridiques sont documentés par connecteur.
5. **Cohérence documentaire** — Les références croisées, les fichiers progress, les ADR forment un réseau documentaire utilisable.
6. **Mode démo intégré** — La spécification du mode démo avec datasets seed est un atout majeur pour la démontrabilité.
7. **Principes SOLID bien compris** — L'architecture montre une vraie maîtrise de la séparation des préoccupations.

---

## 6. Principales faiblesses

1. **Sur-découpage modulaire** — 14 projets C# pour un MVP solo est un signal fort de sur-ingénierie. Chaque projet implique un fichier .csproj, des références, un namespace, des tests séparés. Le coût de maintenance est disproportionné.
2. **Backlog MVP trop large** — 32 items "Must" + "Should" mélange ce qui est critique pour la démonstration avec ce qui est "nice to have". Un vrai MVP solo devrait tenir en 15-18 items.
3. **9 vues UI** — Pour un prototype desktop, c'est excessif. Certaines vues (Contradictions, Export, Paramètres) peuvent être des panels ou onglets plutôt que des vues complètes.
4. **Claim et Evidence dans le domaine** — Ce sont des concepts utiles mais prématurés pour V1. Ils ajoutent de la complexité sans valeur démonstrative immédiate.
5. **30+ endpoints API** — Beaucoup sont des CRUD classiques qui pourraient être regroupés ou simplifiés en V1.
6. **Absence de slicing vertical** — Le corpus spécifie par couche horizontale (domaine → application → infra), pas par feature. Un porteur solo bénéficierait d'un guidage vertical ("pour la démo, implémente ce chemin de bout en bout").

---

## 7. Principaux risques

| Risque | Gravité | Impact | Probabilité |
|---|---|---|---|
| Paralysie par l'architecture — Trop de projets avant la première feature | Critique | Planning | Élevée |
| Dérive périmètre — 32 items ne seront pas finis en 3-4 mois solo | Critique | Produit | Élevée |
| dette de connecteurs — Les 4 connecteurs Should glissent dans le Must | Importante | Planning | Moyenne |
| GUI trop grande — 9 vues = 9 composants de routing + layout | Importante | UX/Planning | Moyenne |
| Sur-spécification du domaine — Les développeurs speculent sur des concepts non essentiels | Modérée | Architecture | Moyenne |
| Audit trail comme surcoût invisible — ~10% sur toutes les features | Modérée | Performance | Faible |

---

## 8. Signaux de sur-ingénierie identifiés

| Signal | Localisation | Sévérité | Recommandation |
|---|---|---|---|
| 14 projets C# | 02-architecture 3.1 | Critique | Réduire à 6-7 en V1 |
| 30+ endpoints API | 02-architecture 13.4 | Importante | Réduire à 15-20 en V1 |
| 9 vues UI distinctes | 03-specs-ui-ux 2.2 | Importante | Réduire à 5-6 en V1 |
| 17 types de domaine | 02-architecture 4.2 | Importante | Réduire à 10-11 en V1 |
| 11 catégories de tests | 05-plan-de-tests 3 | Modérée | Réduire à 5-6 en V1 |
| 7 ADR | /docs/adr/ | Mineure | C'est approprié pour le spec-first |
| Scoring à 5 composantes | 01-specs-fonctionnelles 3.7 | Modérée | 3 composantes suffisent en V1 |
| Dictionnaire Metadata extensible | 02-architecture Observation | Modérée | Simple Dictionary<string,string> suffit, pas de sur-abstraction |

---

## 9. Éléments manquants ou insuffisamment spécifiés

| Élément | Gravité | Localisation | Action |
|---|---|---|---|
| **Slicing vertical pour la première démo** | Critique | Absent du corpus | Ajouter un guide "chemin critique démo" |
| **Schéma JSON d'export** | Importante | 01-specs PO-01 | Définir un schéma minimal avant implémentation |
| **Contenu exact des seed data** | Importante | 01-specs PO-04 | Définir les 3 scénarios de seed data avec des exemples concrets |
| **Ordre d'implémentation recommandé** | Importante | Absent | Ajouter un guide d'implémentation incrémentale |
| **Critères de "démo prête"** | Importante | Absent | Définir les critères minimaux pour une première démo |
| **Gestion du cycle de vie Electron ↔ .NET** | Modérée | 02-architecture 13.2 | Spécifier le démarrage, arrêt, crash recovery |
| **Stratégie d'erreur globale** | Modérée | Absente | Page d'erreur, retry UI, state recovery |
| **Rate limiting concret par connecteur** | Modérée | 01-specs 3.1 | Donner les limites exactes (GDELT: 300/min, YouTube: 10k/jour) |

---

## 10. Recommandation sur l'aptitude à servir de base de développement

Le corpus **peut servir de base**, mais pas en l'état. Il faut :

1. **Recentrer le MVP** — Couper 40% du backlog, réduire le découpage, fusionner les vues UI.
2. **Ajouter un guide de slicing vertical** — Le corpus dit *quoi* construire, pas *dans quel ordre*.
3. **Simplifier le domaine** — Repousser les concepts non essentiels à V2.
4. **Définir les seed data** — C'est le cœur de la démontrabilité et ce sont les données de test les plus importantes.

Après ces coupes, le corpus sera un excellent point de départ.

---

## 11. Recommandation de recentrage MVP

Voir les détails dans `01-analyse-mvp-et-perimetre.md`.

En résumé, le "vrai MVP solo" devrait contenir :

- **2 connecteurs** : RSS + GDELT (pas de YouTube/STAC)
- **5-6 projets C#** : Domain, Application, Infrastructure, Connectors (un seul projet), Api, Worker
- **5-6 vues UI** : Dashboard, Carte+Timeline, EventCase, Observations, Paramètres
- **10-11 types de domaine** : SourceConnector, RawItem, Observation, Entity, EventCase, Location, Contradiction, ConfidenceScore, AnalystFeedback, AuditEntry, (Watchlist en Should)
- **15-18 items de backlog** au lieu de 32
- **Scoring à 3 composantes** : FiabilitéSource + Corroboration + FeedbackAnalyste

---

## 12. Décision proposée

**GO AVEC COUPES**

Le corpus est solide dans ses fondements (positionnement, provenance, scoring, éthique). Il nécessite un recentrage significatif avant implémentation pour éviter la paralysie architecturale et le glissement de périmètre.

Les coupes sont **non négociables** si le projet doit rester faisable par un porteur solo en 3-4 mois.

---

## 13. Top 10 des actions correctives prioritaires

| # | Action | Gravité | Urgence | Impact |
|---|---|---|---|---|
| 1 | **Réduire à 6-7 projets C#** — Fusionner Connectors.* en un seul projet, fusionner Fusion+Scoring+CaseManagement | Critique | Avant implémentation | Architecture/Planning |
| 2 | **Couper le backlog MVP à 15-18 items** — Repousser YouTube, STAC, Import, Watchlists, Export PDF en V2 | Critique | Avant implémentation | Produit/Planning |
| 3 | **Réduire à 5-6 vues UI** — Fusionner Contradictions dans EventCase, Export dans EventCase, Paramètres en modal | Importante | Avant implémentation | UX/Planning |
| 4 | **Simplifier le scoring à 3 composantes V1** — FiabilitéSource, Corroboration, FeedbackAnalyste | Importante | Avant implémentation | Domaine |
| 5 | **Repousser Claim, Evidence, MediaAsset, SearchQuery en V2** | Importante | Avant implémentation | Domaine |
| 6 | **Ajouter un guide de slicing vertical** — Chemin critique pour la première démo fonctionnelle | Critique | Avant implémentation | Planning |
| 7 | **Définir le contenu des seed data** — 3 scénarios avec exemples concrets | Importante | Avant implémentation | Démontrabilité |
| 8 | **Réduire les endpoints API à 15-20** — Regrouper, éliminer les CRUD secondaires | Modérée | Pendant implémentation | Architecture |
| 9 | **Simplifier le plan de tests** — 5 catégories au lieu de 11, pas de tests performance avant V1 stable | Modérée | Pendant implémentation | QA |
| 10 | **Spécifier le cycle de vie Electron ↔ .NET** — Démarrage, arrêt, crash recovery, port conflict | Modérée | Avant implémentation | Architecture |

---

## Références

- Analyse MVP et périmètre : [01-analyse-mvp-et-perimetre.md](01-analyse-mvp-et-perimetre.md)
- Analyse modèle de domaine : [02-analyse-modele-de-domaine.md](02-analyse-modele-de-domaine.md)
- Analyse architecture technique : [03-analyse-architecture-technique.md](03-analyse-architecture-technique.md)
- Analyse UI/UX : [04-analyse-ui-ux.md](04-analyse-ui-ux.md)
- Analyse connecteurs OSINT : [05-analyse-connecteurs-osint.md](05-analyse-connecteurs-osint.md)
- Analyse plan de tests : [06-analyse-plan-de-tests.md](06-analyse-plan-de-tests.md)
- Risques et recommandations : [07-risques-et-recommandations.md](07-risques-et-recommandations.md)
- Checklist avant implémentation : [08-checklist-avant-implementation.md](08-checklist-avant-implementation.md)