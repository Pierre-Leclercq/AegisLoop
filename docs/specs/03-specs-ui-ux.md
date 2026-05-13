# Aegis Loop — Spécifications UI/UX

> **Version :** 2.0  
> **Statut :** Aligné V1 officielle — Spec-first  
> **Dernière mise à jour :** 2026-04-23  
> **Lien vers progression :** [03-specs-ui-ux.progress.md](03-specs-ui-ux.progress.md)  
> **Document amont :** [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)  
> **Note :** Ce document est strictement aligné sur la V1 officielle. Tout écart est une erreur.

---

## 1. Principes UX

### 1.1 Posture : Workbench analyste

Aegis Loop n'est PAS un tableau de bord passif. C'est un **workbench** — un environnement de travail où l'analyste agit, décide, valide, enrichit et exporte.

**Implications :**
- L'interface est **dense mais organisée**, pas épurée au point d'en perdre l'information
- L'analyste travaille principalement au **clavier**, les raccourcis sont essentiels
- L'information est **hiérarchisée** : l'essentiel est visible, le détail est accessible en 1 clic
- Chaque élément affiché est **actionnable** : on peut cliquer, valider, corriger, annoter
- L'état du système est **toujours visible** : collectes en cours, contradictions, erreurs

### 1.2 Principes directeurs

| Principe | Application |
|---|---|
| **Densité informationnelle** | Afficher beaucoup d'information sans noyer l'utilisateur |
| **Hiérarchie visuelle** | L'essentiel est visible, le détail est accessible |
| **Provenance en un clic** | Tout élément affiché a sa provenance accessible immédiatement |
| **Explicabilité** | Tout score est cliquable et montre sa décomposition (3 composantes) |
| **Feedback immédiat** | Toute action produit un retour visible en < 300ms |
| **Navigation fluide** | Passer d'une vue à l'autre sans perdre le contexte |
| **Raccourcis clavier** | Les 9 raccourcis V1 couvrent toutes les actions fréquentes |
| **Mode sombre par défaut** | Les analystes travaillent souvent dans des environnements à faible luminosité |
| **Accessibilité raisonnable** | Contrastes suffisants, navigation clavier, taille de texte lisible |
| **Impressionnant en démo** | L'interface doit donner une impression de maturité et de sérieux |

### 1.3 Design system

- **Framework UI :** Tailwind CSS + shadcn/ui (composants accessibles, customisables)
- **Thème :** Mode sombre par défaut, mode clair optionnel
- **Typographie :** Inter (UI) / JetBrains Mono (données, code)
- **Icônes :** Lucide Icons (cohérent, open source)
- **Couleurs sémantiques :**
  - Primary : Bleu (#3B82F6) — actions principales
  - Success : Vert (#22C55E) — confirmations, scores élevés
  - Warning : Orange (#F59E0B) — contradictions, avertissements
  - Danger : Rouge (#EF4444) — alertes, scores faibles, erreurs
  - Neutral : Gris (#6B7280) — information secondaire
  - Provenance : Violet (#8B5CF6) — indicateurs de provenance
- **Spacing :** 4px grid (Tailwind default)
- **Border radius :** 6px (composants), 8px (cartes), 12px (modales)

---

## 2. Navigation — 5 vues V1

### 2.1 Structure de navigation

L'interface est organisée en **vue principale + panneau latéral**, avec une **barre de navigation verticale** à gauche.

```
┌─────────────────────────────────────────────────────────────┐
│ ┌───┐ ┌───────────────────────────────────────────────────┐ │
│ │   │ │  Barre de titre (recherche rapide Ctrl+K)        │ │
│ │ N │ ├───────────────────────────────────────────────────┤ │
│ │ A │ │                                                   │ │
│ │ V │ │           Vue principale                         │ │
│ │ I │ │           (contenu de l'onglet actif)             │ │
│ │ G │ │                                                   │ │
│ │ A │ │                                                   │ │
│ │ T │ │                                                   │ │
│ │ I │ │                                                   │ │
│ │ O │ │                                                   │ │
│ │ N │ │                                                   │ │
│ │   │ │                                                   │ │
│ └───┘ └───────────────────────────────────────────────────┘ │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │              Barre de statut (connecteurs, collecte)    │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 Vues V1 — 5 onglets

| Icône | Vue | Raccourci | Description |
|---|---|---|---|
| 📊 | **Dashboard** | `Ctrl+1` | KPIs, événements prioritaires, contradictions, activité connecteurs |
| 🗺️ | **Carte + Timeline** | `Ctrl+2` | Observations géolocalisées, timeline simple, clusters |
| 📁 | **EventCase** | `Ctrl+3` | Détail événement, observations, provenance, score décomposé, contradictions (onglet), export (bouton), notes/tags |
| 🔍 | **Observations** | `Ctrl+4` | Liste, filtres simples, détail observation, provenance, feedback |
| ⚙️ | **Paramètres** | `Ctrl+5` | Configuration connecteurs, scoring, mode démo, thème |

**Vues fusionnées V1 :**
- Contradictions → onglet dans EventCase
- Export → bouton dans EventCase
- Recherche avancée → filtres dans Observations + recherche rapide Ctrl+K
- Connecteurs & Jobs → panel dans Dashboard ou Paramètres

> **Exclus V1 :** Vue Contradictions séparée, vue Export séparée, vue Recherche avancée séparée, vue Connecteurs & Jobs séparée.

### 2.3 Barre de statut

La barre de statut en bas de l'écran affiche en permanence :
- Nombre de connecteurs actifs / en erreur
- Dernière collecte (horodatage)
- Nombre d'observations non lues
- Nombre de contradictions ouvertes
- Mode démo actif (indicateur visible "DÉMO")

---

## 3. Vues principales V1

### 3.1 Vue 1 — Dashboard (Ctrl+1)

**Objectif métier :** Donner à l'analyste une vue synthétique immédiate de la situation.

**Composants UI :**

| Zone | Composant | Données affichées | Actions |
|---|---|---|---|
| En-tête | Titre + filtres globaux | Période, source | Changer les filtres |
| Zone A | Carte de synthèse (4 KPIs) | Observations totales, Événements actifs, Contradictions ouvertes, Score moyen | Cliquer pour naviguer |
| Zone B | Liste des événements prioritaires | Top 10 événements triés par priorité | Cliquer pour ouvrir EventCase |
| Zone C | Dernières observations | 5 dernières observations collectées | Cliquer pour voir le détail |
| Zone D | Contradictions en attente | Contradictions non résolues | Cliquer pour résoudre |
| Zone E | Activité des connecteurs | Statut de chaque connecteur, dernier run | Cliquer pour configurer |

**États d'écran :**
- **Normal :** Données chargées, tout visible
- **Vide :** Aucune donnée collectée → Message "Commencez par activer des connecteurs ou charger les données de démo" avec boutons d'action
- **Chargement :** Skeleton loading, pas de spinner bloquant
- **Erreur :** Bannière d'erreur avec bouton "Réessayer"

**Règles d'ergonomie :**
- Le dashboard doit se charger en < 2 secondes avec les données de démo
- Les KPIs sont toujours visibles sans scroll
- Les événements sont cliquables et mènent à la vue EventCase
- Le score de confiance est affiché avec un code couleur (vert/orange/rouge) et cliquable pour voir la décomposition (3 composantes)

**Données de démo attendues :**
- 4 KPIs avec des valeurs réalistes (ex : 90 observations, 8 événements, 3 contradictions, score moyen 0.72)
- 5 événements prioritaires avec des titres crédibles

---

### 3.2 Vue 2 — Carte + Timeline (Ctrl+2)

**Objectif métier :** Visualiser la distribution géographique et temporelle des observations et événements.

**Composants UI :**

| Zone | Composant | Données affichées | Actions |
|---|---|---|---|
| Zone principale (70%) | Carte MapLibre GL JS | Marqueurs d'observations, clusters | Zoom, clic sur marqueur |
| Zone inférieure (30%) | TimelineSimple | Marqueurs temporels, sélection de plage | Glisser pour sélectionner une période |
| Panneau latéral droit | Panneau de détail | Détail de l'observation ou événement sélectionné | Fermer, ouvrir dans la vue complète |

**Interactions carte :**
- Clic sur marqueur → tooltip avec résumé + bouton "Voir le détail"
- Clic sur cluster → zoom sur le cluster
- Couches : observations, événements
- Fonds de carte : OpenStreetMap (défaut)

**Interactions timeline :**
- Glisser pour sélectionner une plage temporelle
- Clic sur un marqueur → tooltip + sélection
- Code couleur par type d'événement ou source
- Synchronisation carte ↔ timeline

**États :**
- **Normal :** Carte et timeline avec données
- **Vide :** Aucune observation géolocalisée → Message "Aucune donnée géolocalisée disponible"
- **Chargement :** Carte affiche les tuiles, les marqueurs apparaissent progressivement

> **Note V1 :** Timeline simple uniquement. Pas de timeline avancée (zoom complexe, filtres avancés, catégories multiples) — repoussée V2. Pas de heat map, pas de polygones GeoJSON importés (connecteurs V2).

---

### 3.3 Vue 3 — EventCase (Ctrl+3)

**Objectif métier :** Explorer un événement en détail : observations, provenance, score, contradictions, notes, export.

**Composants UI :**

| Zone | Composant | Données affichées | Actions |
|---|---|---|---|
| En-tête | EventCaseHeader | Titre, catégorie, statut, score (3 composantes cliquables) | Modifier le statut |
| Zone A | Liste des observations | Observations triées par date/score, avec provenance | Filtrer, trier, cliquer pour détail |
| Zone B | ProvenanceChain | Chaîne de provenance complète | Cliquer sur chaque maillon |
| Zone C | ConfidenceScoreBadge | Score décomposé (FiabilitéSource, Corroboration, FeedbackAnalyste) | Cliquer pour voir le détail |
| Zone D | Onglet Contradictions | Contradictions détectées dans l'événement | Résoudre une contradiction |
| Zone E | Notes et tags | Notes de l'analyste, tags appliqués | Ajouter/éditer/supprimer |
| Zone F | Bouton Export | Export Markdown / JSON | Cliquer pour exporter (Ctrl+E) |
| Zone G | Mini-carte | Localisation de l'événement | Zoom, clic |

**Raccourcis clavier dans EventCase :**
- `Ctrl+E` — Exporter le dossier courant
- `Ctrl+Enter` — Valider l'observation sélectionnée
- `Ctrl+Shift+X` — Invalider l'observation sélectionnée
- `Escape` — Fermer le panneau latéral / la modale

**Provenance — Design spécifique :**

La provenance est affichée comme une **chaîne horizontale** (breadcrumb) :
```
[Source: RSS/LeMonde] → [Collecté: 2026-04-23 14:32] → [Normalisé] → [Score: 0.75]
```
Chaque maillon est cliquable pour voir le détail de la transformation.

**États :**
- **Normal :** Événement chargé avec toutes ses données
- **Événement vide :** Aucune observation → "Cet événement n'a pas encore d'observations"
- **Score non calculé :** "Le score est en cours de calcul"

---

### 3.4 Vue 4 — Observations (Ctrl+4)

**Objectif métier :** Explorer les observations, filtrer, consulter la provenance, fournir du feedback.

**Composants UI :**

| Zone | Composant | Données affichées | Actions |
|---|---|---|---|
| En-tête | FilterBar | Filtres : source, période, score, type, tags | Filtrer la liste |
| Zone A | Liste des observations | ObservationCard pour chaque observation | Cliquer pour voir le détail |
| Zone B | Panneau de détail | Observation complète avec provenance, score, feedback | Valider/invalider/corriger |
| Zone C | FeedbackPanel | Boutons Confirmer/Invalider/Corriger + historique | Soumettre un feedback |

**Filtres V1 :**
- Source connecteur (RSS, GDELT)
- Période (date range)
- Score de confiance minimum (slider 0–1)
- Statut de l'observation (Nouvelle, Confirmée, Invalidée, Contredite)
- Tags

**Recherche rapide :** `Ctrl+K` ouvre une barre de recherche textuelle rapide.

> **Exclus V1 :** Recherche avancée (full-text indexée, requêtes sauvegardées, filtres géographiques complexes, filtres par entités). Ces fonctionnalités sont repoussées en V2.

**États :**
- **Normal :** Liste avec filtres
- **Vide :** "Aucune observation pour ces critères"
- **Observation sélectionnée :** Panneau de détail avec provenance et feedback

---

### 3.5 Vue 5 — Paramètres (Ctrl+5)

**Objectif métier :** Configurer les connecteurs, le scoring, le mode démo et les préférences.

**Onglets de paramètres :**

| Onglet | Contenu |
|---|---|
| Connecteurs | Configuration RSS et GDELT, statut, activation/désactivation |
| Scoring | Poids des 3 composantes du score, fiabilité des sources |
| Mode démo | Activer/désactiver le mode démo, réinitialiser les données |
| Général | Thème (sombre/clair), langue, préférences d'affichage |
| Données | Emplacement de la base, purge des anciens items |

**Formulaire de connecteur RSS :**
- URL du flux (text input, validation URL)
- Fréquence de collecte (dropdown : 15 min, 30 min, 1h, 6h, 24h)
- Bouton "Tester la connexion"

**Formulaire de connecteur GDELT :**
- Requête (text input)
- Filtres pays (multi-select)
- Filtres thématiques (multi-select)
- Fréquence de collecte
- Bouton "Tester la connexion"

> **Exclus V1 :** Formulaires YouTube, STAC, Import manuel (connecteurs V2).

---

## 4. Composants UI V1 (10)

### 4.1 Composants Must (7)

| Composant | Description | Utilisation |
|---|---|---|
| `ObservationCard` | Carte résumé d'une observation avec titre, source, score, date | Dashboard, Observations |
| `EventCaseHeader` | En-tête d'événement avec titre, catégorie, statut, score décomposable | EventCase |
| `ConfidenceScoreBadge` | Badge avec score, code couleur, cliquable pour décomposition (3 composantes) | Partout où un score est affiché |
| `ProvenanceChain` | Chaîne de provenance horizontale (breadcrumb) | Observations, EventCase |
| `TimelineSimple` | Timeline horizontale simple avec marqueurs et sélection de plage | Carte + Timeline |
| `MapContainer` | Conteneur de carte MapLibre GL JS avec marqueurs et clusters | Carte + Timeline |
| `FeedbackPanel` | Panel de validation/correction (Confirmer/Invalider/Corriger) | Observations, EventCase |

### 4.2 Composants Should (3)

| Composant | Description | Utilisation |
|---|---|---|
| `ContradictionAlert` | Alerte avec type de contradiction et observations en conflit | Dashboard, EventCase |
| `SourceBadge` | Badge avec type de source (RSS/GDELT) et fiabilité | Partout où une source est mentionnée |
| `FilterBar` | Barre de filtres combinables (source, date, score, type) | Observations |

> **Exclus V1 :** SearchAdvanced, WatchlistManager, ExportWizard, CaseFileTree, EntityPill (composants séparés), TimelineAvancée.

---

## 5. Raccourcis clavier V1 (9)

| Raccourci | Action | Priorité |
|---|---|---|
| `Ctrl+1` | Dashboard | Must |
| `Ctrl+2` | Carte + Timeline | Must |
| `Ctrl+3` | EventCase | Must |
| `Ctrl+4` | Observations | Must |
| `Ctrl+5` | Paramètres | Must |
| `Ctrl+Enter` | Valider une observation | Must |
| `Ctrl+Shift+X` | Invalider une observation | Must |
| `Ctrl+E` | Exporter le dossier courant | Must |
| `Ctrl+K` | Recherche rapide | Must |
| `Escape` | Fermer panel/modal | Must |
| `Ctrl+F` | Filtrer dans la vue courante | Should |
| `Ctrl+D` | Aller au Dashboard | Should |
| `?` | Aide des raccourcis | Should |

> **Note :** 6 raccourcis Must + 3 Should = 9 raccourcis V1 au total (les Must essentiels + les Should). Les 13+ raccourcis supprimés (Ctrl+6 pour Contradictions, Ctrl+N pour nouveau case file, V/X/N/P hors contexte, etc.) sont repoussés en V2.

---

## 6. Scénarios GUI V1

### 6.1 Scénario MUST 1 — Surveillance d'une crise émergente

1. **Lancement** — L'analyste ouvre Aegis Loop. Le dashboard affiche les KPIs et les événements prioritaires.
2. **Activation des connecteurs** — `Ctrl+5` → Paramètres → Connecteurs → Active le connecteur RSS "Crise" et le connecteur GDELT "Conflit".
3. **Collecte** — Le Worker lance la collecte. Des observations apparaissent sur le dashboard après 30s.
4. **Exploration** — L'analyste voit un événement "Tension dans la région X" avec un score de 0.68. Il clique dessus.
5. **EventCase** — La vue EventCase affiche 7 observations de 4 sources. Une contradiction est signalée dans l'onglet contradictions.
6. **Provenance** — L'analyste clique sur le badge de provenance d'une observation. La chaîne de provenance s'affiche : RSS/LeMonde → Collecté le 23/04 → Normalisé → Score: 0.75.
7. **Feedback** — L'analyste clique "Confirmer" sur une observation fiable. Le score passe de 0.68 à 0.78. Le score de l'événement est recalculé.
8. **Contradiction** — L'analyste clique sur l'onglet contradictions dans l'EventCase. Deux observations avec des lieux différents. Il confirme la localisation de la source la plus fiable.
9. **Export** — L'analyste exporte le dossier en Markdown avec `Ctrl+E`.

### 6.2 Scénario MUST 2 — Incident maritime Golfe d'Aden

1. **Activation** — L'analyste configure GDELT (filtre maritime) et RSS (sources maritimes) dans les Paramètres.
2. **Carte** — `Ctrl+2` → La carte affiche les observations géolocalisées avec clusters.
3. **Sélection** — L'analyste clique sur un cluster dans le Golfe d'Aden.
4. **EventCase** — L'EventCase montre les observations avec provenance. Une contradiction sur la localisation est détectée.
5. **Résolution** — L'analyste résout la contradiction en confirmant la source la plus fiable.
6. **Export** — L'analyste exporte le dossier en JSON.

### 6.3 Scénario SHOULD — Mode démo

1. **Activation** — `Ctrl+5` → Paramètres → Mode démo → Activer.
2. **Chargement** — Les datasets seed sont chargés en < 5s. Le dashboard affiche les données avec un badge "DÉMO".
3. **Exploration** — L'analyste explore les événements, les observations, les contradictions, les scores.
4. **Feedback** — Il valide et corrige des observations. Les scores se mettent à jour.
5. **Export** — Il exporte un dossier événement.
6. **Réinitialisation** — Paramètres → Mode démo → "Réinitialiser la démo".

---

## 7. Logique de tri et priorisation

### 7.1 Tri des événements

Par défaut, les événements sont triés par **priorité** :
```
Priorité = ScoreConfiance × FacteurCorroboration
```
- Les événements avec plus de sources indépendantes sont prioritaires

Tri alternatif possible : par date, par nombre d'observations, par score de confiance.

### 7.2 Tri des observations

Par défaut : par date décroissante dans un événement.
Tri alternatif : par score de confiance, par source, par statut.

### 7.3 Tri des contradictions

Par défaut : par date de détection décroissante.
Les contradictions non résolues sont affichées en premier.

---

## 8. Responsive et densité

### 8.1 Résolution cible

- **Résolution minimale :** 1280 × 720
- **Résolution recommandée :** 1920 × 1080
- **Résolution optimale :** 2560 × 1440 (moniteur analyste)

### 8.2 Densité informationnelle

- Mode "compact" par défaut (densité élevée, analyste)
- Mode "confortable" optionnel (plus d'espace, taille de texte plus grande)
- La bascule se fait dans les paramètres
- En mode compact : police 13px, espacement réduit, lignes serrées
- En mode confortable : police 15px, espacement standard

---

## 9. Accessibilité

- Tous les textes ont un contraste suffisant (WCAG 2.1 AA minimum)
- Navigation clavier complète (9 raccourcis V1 listés)
- Focus visible sur tous les éléments interactifs
- Labels ARIA sur les composants interactifs
- Pas de contenu uniquement visuel — tout a un équivalent textuel
- Annonces pour les lecteurs d'écran sur les changements dynamiques importants
- Mode démo avec des données réalistes qui testent tous les composants

---

## 10. Mode sombre / clair

- Mode sombre par défaut (analystes = travail en environnement sombre)
- Basculement dans Paramètres → Général → Thème
- Le choix est persisté
- Les deux thèmes sont testés pour un contraste suffisant
- Les cartes et visualisations s'adaptent au thème

---

## 11. États de chargement et d'erreur

### 11.1 États de chargement

- **Skeleton loading :** Pour les listes et les cartes, afficher des squelettes gris animés pendant le chargement
- **Progress bar :** Pour les actions longues (collecte, export), afficher une barre de progression
- **Toast notification :** Pour les actions réussies (feedback soumis, export terminé)
- **Error boundary :** En cas d'erreur, afficher un message clair avec bouton "Réessayer"

### 11.2 États vides

Chaque vue a un état vide explicatif :
- **Dashboard vide :** "Commencez par activer des connecteurs ou charger les données de démo" + boutons
- **Carte vide :** "Aucune observation géolocalisée. Les observations avec des coordonnées apparaîtront ici."
- **EventCase vide :** "Aucun événement sélectionné"
- **Observations vides :** "Aucune observation pour ces critères"

---

## 12. Références croisées

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)
- Spécifications fonctionnelles : [01-specs-fonctionnelles.md](01-specs-fonctionnelles.md)
- Architecture technique : [02-architecture-technique.md](02-architecture-technique.md)
- Manuel utilisateur : [04-manuel-utilisateur.md](04-manuel-utilisateur.md)