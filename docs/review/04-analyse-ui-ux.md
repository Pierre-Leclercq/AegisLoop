# Analyse UI/UX analyste

> **Date :** 2026-04-23  
> **Axe d'audit :** F — GUI / UX  
> **Gravité :** Importante

---

## Verdict : UX bien pensée mais trop de vues pour un MVP solo

La posture "workbench analyste" est la bonne. La hiérarchie de l'information est correcte. La provenance et le scoring décomposé sont bien intégrés dans l'UI. Mais **9 vues distinctes pour un MVP solo** est excessif — certaines doivent être fusionnées ou simplifiées.

Score : **6.0/10** — Ambitieux, à réduire.

---

## 1. Points forts de l'UX

- **Posture workbench** — C'est le bon positionnement. L'analyste n'est pas passif, il est acteur.
- **Provenance visible** — Chaque observation montre sa chaîne de provenance. C'est un différenciateur.
- **Scoring décomposé** — L'analyste voit les 3 (ou 5) composantes du score. C'est essentiel pour la confiance.
- **Feedback analyste intégré** — Valider/invalider/corriger directement dans l'UI. C'est le cœur du produit.
- **Mode sombre** — Adapté au contexte analyste.
- **Raccourcis clavier** — Les plus importants (navigation, validation, export) sont bien choisis.

## 2. Problème principal : trop de vues

9 vues distinctes pour un MVP solo, c'est trop. Chaque vue implique :
- Un composant de routing
- Un layout spécifique
- Des états (chargement, vide, erreur)
- Des tests E2E
- Des données de démo

### Recommandation : réduire à 5-6 vues V1

| Vue V1 | Contient | Raison |
|---|---|---|
| **Dashboard** | KPIs, événements prioritaires, alertes, contradictions | Point d'entrée |
| **Carte + Timeline** | Observations géolocalisées, timeline simple | Vue spatiale/temporelle |
| **EventCase** | Détail événement, observations, provenance, score, contradictions, notes, feedback | Vue analytique centrale |
| **Observations** | Liste, filtres, recherche simple, détail observation | Exploration |
| **Paramètres** | Configuration connecteurs, mode démo | Configuration |
| *(Fusionné dans EventCase)* **Contradictions** | → Onglet dans EventCase | Pas une vue séparée |
| *(Fusionné dans EventCase)* **Export** | → Bouton dans EventCase | Pas une vue séparée |
| *(Fusionné dans Observations)* **Recherche avancée** | → Filtres dans Observations | Pas une vue séparée |
| *(Fusionné dans Dashboard)* **Connecteurs & Jobs** | → Panel dans Dashboard ou Paramètres | Pas une vue séparée |

**V1 : 5 vues** (Dashboard, Carte+Timeline, EventCase, Observations, Paramètres)

## 3. Composants UI — Bon mais à prioriser

14 composants réutilisables sont spécifiés. C'est ambitieux mais réalisable si on priorise.

### Composants V1 Must
1. **ObservationCard** — Affichage d'une observation avec provenance et score
2. **EventCaseHeader** — En-tête d'un dossier d'événement
3. **ConfidenceScoreBadge** — Score décomposé
4. **ProvenanceChain** — Chaîne de provenance
5. **TimelineSimple** — Timeline linéaire (pas de timeline avancée)
6. **MapContainer** — Carte MapLibre avec marqueurs
7. **FeedbackPanel** — Validation/invalidation/correction

### Composants V1 Should
8. **ContradictionAlert** — Alerte de contradiction
9. **SourceBadge** — Badge de source (RSS, GDELT, etc.)
10. **FilterBar** — Barre de filtres

### Composants V2
11-14. **SearchAdvanced, WatchlistManager, ExportWizard, TimelineAvancée**

## 4. Raccourcis clavier — 18 c'est trop

18 raccourcis pour un MVP, c'est excessif. L'analyste ne les mémorisera pas tous.

### Recommandation : 8-10 raccourcis V1

| Raccourci | Action | Priorité |
|---|---|---|
| `Ctrl+K` | Recherche rapide | Must |
| `Ctrl+Enter` | Valider une observation | Must |
| `Ctrl+Shift+X` | Invalider une observation | Must |
| `Ctrl+E` | Exporter le dossier courant | Must |
| `Ctrl+1-5` | Navigation entre vues | Must |
| `Escape` | Fermer panel/modal | Must |
| `Ctrl+F` | Filtrer dans la vue courante | Should |
| `Ctrl+D` | Aller au Dashboard | Should |
| `Ctrl+M` | Aller à la Carte | Should |
| `?` | Aide des raccourcis | Should |

Les autres raccourcis (notes, tags, mode sombre, etc.) seront ajoutés en V2.

## 5. Provenance et explicabilité en UI — Point fort

La spécification UI intègre bien la provenance :
- ProvenanceChain dans chaque ObservationCard ✅
- Score décomposé visible ✅
- Détail de la source accessible ✅
- "Pourquoi je vois cette alerte ?" répondable ✅

C'est un **vrai différenciateur**. Ne pas réduire.

## 6. Scénarios GUI — Réalistes mais à simplifier

Les 3 scénarios GUI sont utiles :
1. Analyse d'une crise géopolitique ✅
2. Suivi d'un événement maritime ✅
3. Détection et résolution d'une contradiction ✅

Ils sont bien choisis. Mais le scénario 3 (contradiction) peut être intégré dans le scénario 1 ou 2 plutôt que d'être séparé.

**Recommandation :** Garder 2 scénarios de démo en V1 (géopolitique + maritime), intégrer les contradictions dans ces scénarios.

## 7. Données de démo — Critique pour la crédibilité

Les seed data sont le cœur de la démontrabilité. Le corpus mentionne des datasets seed mais ne les définit pas assez concrètement.

**Recommandation :** Définir explicitement :
- Scénario 1 : Crise au Soudan — 50 observations, 3 sources, 5 événements, 2 contradictions
- Scénario 2 : Incident maritime Golfe d'Aden — 40 observations, 2 sources, 3 événements, 1 contradiction
- Chaque observation avec : titre, contenu, source, date, localisation, score

## 8. Résumé des recommandations UX

| Action | Gravité | Urgence | Impact |
|---|---|---|---|
| Réduire à 5 vues UI | Importante | Avant implémentation | UX/Planning |
| Prioriser 7-10 composants V1 | Importante | Avant implémentation | UX/Planning |
| Réduire à 8-10 raccourcis | Modérée | Pendant implémentation | UX |
| Définir les seed data concrètes | Importante | Avant implémentation | Démontrabilité |
| Intégrer Contradictions dans EventCase | Importante | Avant implémentation | UX |
| Intégrer Export dans EventCase | Modérée | Pendant implémentation | UX |
| Garder 2 scénarios de démo | Modérée | Pendant implémentation | Démontrabilité |