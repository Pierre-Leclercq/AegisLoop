# Aegis Loop — Manuel utilisateur

> **Version :** 2.0  
> **Statut :** Aligné V1 officielle  
> **Dernière mise à jour :** 2026-04-23  
> **Document amont :** [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)

---

## 1. Introduction

Aegis Loop est un **workbench analyste desktop** qui fusionne, qualifie, trace et rend explicables des observations hétérogènes issues de sources OSINT publiques (RSS et GDELT).

Ce manuel couvre uniquement les fonctionnalités V1. Les fonctionnalités repoussées en V2/V3 (YouTube, STAC, Import manuel, Watchlists, Recherche avancée, Export PDF, Timeline avancée) ne sont pas documentées ici.

---

## 2. Démarrage

### 2.1 Installation

1. Cloner le repo : `git clone <repo-url>`
2. Prérequis : .NET 10 SDK 10.0.201+, Node.js 24+, npm 11+
3. Build backend : `dotnet build AegisLoop.sln --configuration Release`
4. Build frontend : `cd src/desktop-electron && npm install && npm run build`
5. Lancement desktop complet : `cd src/desktop-electron && npm run electron:dev`
6. API seule : `dotnet run --project src/AegisLoop.Api/AegisLoop.Api.csproj`
7. Worker seul : `dotnet run --project src/AegisLoop.Worker/AegisLoop.Worker.csproj`

### 2.2 Premier lancement

1. L'application démarre : Electron lance le backend .NET puis ouvre la fenêtre
2. Le dashboard s'affiche (vide si aucune donnée)
3. Le message "Commencez par activer des connecteurs ou charger les données de démo" s'affiche

---

## 3. Parcours utilisateur V1

### 3.1 Configurer les connecteurs RSS / GDELT

1. Ouvrir les **Paramètres** (`Ctrl+5`)
2. Aller dans l'onglet **Connecteurs**
3. **Configurer RSS :**
   - Saisir l'URL du flux RSS (ex : `https://feeds.lemonde.fr/mideast/rss.xml`)
   - Choisir la fréquence de polling (15 min par défaut)
   - Cliquer "Tester la connexion" pour vérifier
   - Activer le connecteur
4. **Configurer GDELT :**
   - Saisir la requête (ex : `sudan conflict`)
   - Choisir les filtres pays et/ou thème
   - Choisir la fréquence de polling (30 min par défaut)
   - Activer le connecteur

> **Note :** Aucune clé API n'est requise pour RSS ni GDELT en V1.

### 3.2 Lancer l'ingestion

1. Depuis le **Dashboard** ou les **Paramètres**, cliquer sur "Lancer une collecte manuelle"
2. Le Worker exécute les connecteurs actifs
3. Les nouvelles observations apparaissent progressivement sur le dashboard
4. Les événements sont détectés automatiquement par clustering

### 3.3 Consulter les événements

1. Ouvrir le **Dashboard** (`Ctrl+1`)
2. Les événements prioritaires sont affichés, triés par score de confiance
3. Cliquer sur un événement pour ouvrir la vue **EventCase** (`Ctrl+3`)
4. Dans l'EventCase :
   - Voir les observations associées avec provenance
   - Voir le score de confiance décomposé (3 composantes)
   - Consulter les contradictions dans l'onglet dédié
   - Ajouter des notes et tags

### 3.4 Consulter la provenance

1. Dans l'EventCase ou la vue Observations, cliquer sur le badge de provenance
2. La chaîne de provenance s'affiche :
   ```
   [Source: RSS/LeMonde] → [Collecté: 2026-04-23 14:32] → [Normalisé] → [Score: 0.75]
   ```
3. Chaque maillon est cliquable pour le détail
4. L'URL originale est cliquable (ouverture dans le navigateur)

### 3.5 Filtrer et annoter

**Filtrer les observations :**
1. Ouvrir la vue **Observations** (`Ctrl+4`)
2. Utiliser la **FilterBar** : source (RSS/GDELT), période, score minimum, statut, tags
3. Utiliser la recherche rapide (`Ctrl+K`)

**Annoter :**
1. Sélectionner une observation ou un événement
2. Ajouter un tag ou une note
3. L'annotation est enregistrée immédiatement

### 3.6 Valider ou corriger une observation

1. Sélectionner une observation
2. Consulter la provenance
3. Choisir une action :
   - **Confirmer** (`Ctrl+Enter`) — Le score FeedbackAnalyste augmente
   - **Invalider** (`Ctrl+Shift+X`) — Le score diminue
   - **Corriger** — Modifier un attribut (lieu, date, description)
4. Le score est recalculé immédiatement
5. L'action est tracée dans l'audit trail

> **Annulation :** L'analyste peut annuler son dernier feedback dans les 5 minutes.

### 3.7 Gérer les contradictions

1. Les contradictions sont détectées automatiquement après ingestion
2. Elles apparaissent sur le **Dashboard** et dans l'onglet **Contradictions** de l'EventCase
3. L'analyste consulte les observations en conflit avec leur provenance
4. Il résout la contradiction :
   - Confirmer l'une des observations
   - Garder la contradiction ouverte (en suspens)
5. La résolution est tracée et influence le scoring

### 3.8 Exporter un dossier événement

1. Ouvrir un **EventCase** (`Ctrl+3`)
2. Cliquer sur le bouton **Exporter** ou `Ctrl+E`
3. Choisir le format : **Markdown** ou **JSON**
4. Le rapport est téléchargé avec provenance et scores

> **Note :** Export PDF n'est pas disponible en V1 (repoussé V2).

---

## 4. Mode démo / replay

### 4.1 Activer le mode démo

1. Ouvrir les **Paramètres** (`Ctrl+5`)
2. Utiliser la section **Mode démo seed/replay V1**
3. Cliquer **Charger seed**
4. Les données seed locales sont chargées, les EventCases reconstruits et les scores recalculés
5. Le statut affiche le volume chargé : sources, RawItems, observations, EventCases, scores, feedbacks

### 4.2 Scénarios de démo disponibles

**Scénario 1 — Sahel Civic Security :**
- 50 observations de 3 sources simulées (2 RSS locales, 1 GDELT local)
- 5 EventCases : Noria checkpoint incident, Talar river flood response, Maro market supply disruption, Kivu mediator talks, Luma clinic evacuation
- Lieux déterministes non sensibles : Noria road corridor, Talar river shelters, Maro market, Kivu civic center, Luma clinic
- Feedbacks analyste seedés pour montrer l’impact sur le scoring

**Scénario 2 — Aden Maritime Incident :**
- 40 observations de 2 sources simulées (1 RSS maritime local, 1 GDELT maritime local)
- 3 EventCases : Aden vessel drone alert, Bab el Mandeb rerouting, Djibouti rescue coordination
- Lieux déterministes non sensibles : Gulf of Aden eastbound corridor, Bab el Mandeb traffic lane, Djibouti rescue coordination zone
- Scores volontairement variés via fiabilité source, corroboration et feedback analyste

**Total :** 90 observations, 8 EventCases attendus, 5 connecteurs simulés, 5 feedbacks seedés. Les contradictions avancées restent hors Phase 5 si elles ne sont pas déjà générées par les heuristiques V1.

### 4.3 Réinitialiser la démo

1. Paramètres → Mode démo seed/replay V1 → **Reset démo**
2. Les observations, RawItems, EventCases, scores, feedbacks et jobs de démo sont effacés
3. Cliquer **Charger seed** pour rejouer le scénario depuis un état propre

### 4.4 Rebuild / recalcul / audit

- **Rebuild EventCases** : relance le regroupement heuristique V1 sur les observations existantes.
- **Recalcul scores** : supprime les scores existants puis recalcule observations et EventCases.
- Chaque action démo écrit une entrée dans l’audit : `DemoSeedLoaded`, `DemoReset`, `DemoEventCasesRebuilt`, `DemoScoresRecalculated`.

### 4.5 Fonctionnement hors réseau

Le mode démo fonctionne **sans connexion réseau**. Le dataset est versionné sous `examples/demo-data/v1-seed.json`. Aucune clé API n'est requise.

### 4.6 Utiliser Carte + Timeline

1. Charger le seed depuis **Paramètres**.
2. Ouvrir **Carte + Timeline** (`Ctrl+2`).
3. La carte V1 affiche les EventCases avec coordonnées issues du seed local sur un fond Natural Earth public offline multi-échelle : monde 1:110m, régional 1:50m, local 1:10m extrait Sahel/Aden, avec terres, côtes, frontières pays, lacs/rivières principales et quelques libellés régionaux. Le rendu est viewport-driven : les couches sont sélectionnées selon le `viewBox`/zoom, puis chargées localement à la demande via le manifest `public/map-data/manifest.json`. Aucun fond de carte externe, tuile live, clé API ni géocodage réseau n’est appelé.
4. Utiliser **Zoom +** et **Zoom −** pour piloter une caméra SVG continue : chaque clic applique un facteur multiplicatif au `viewBox`, sans se limiter aux trois fonds `Monde/Régional/Local`. Le zoom avant réduit progressivement la fenêtre visible autour de l’EventCase sélectionné visible ou du centre courant ; le zoom arrière l’agrandit progressivement jusqu’à la vue globale.
5. Le bouton **Réinitialiser** revient au cadrage initial Sahel/Golfe d’Aden sans modifier la sélection courante. Le bouton **Vue globale** affiche directement le monde SVG complet (`0 0 1200 650`) et un indicateur du type `Vue globale · Zoom 1×`. Le bouton **Zoom sélection** centre un cadrage local autour de l’EventCase sélectionné ; après ce raccourci, Zoom +/− reste continu.
6. Faire glisser la carte permet de déplacer le `viewBox` à tous les niveaux de zoom, dans les bornes du monde SVG. Quand le curseur est au-dessus de la carte, la molette applique le même zoom multiplicatif autour du curseur et ne fait pas défiler la page.
7. Le badge `Fond carte` indique le niveau de détail cartographique actif : `Monde 1:110m` au grand contexte, `Régional 1:50m` au cadrage intermédiaire, puis `Local 1:10m extrait seed` lorsque le `viewBox` devient suffisamment étroit. Ce LOD est automatique et discret, mais il n’est pas le niveau de zoom utilisateur : plusieurs tailles de `viewBox` différentes peuvent partager le même fond.
8. La timeline liste les EventCases chronologiquement. Cliquer sur un item timeline ou un marqueur carte met à jour le panneau détail.
9. Les filtres V1 disponibles sont : source (`Rss`, `Gdelt`, toutes), score minimal et scénario.
10. Les compteurs indiquent le nombre d’éléments affichés, les éléments sans coordonnées, la période couverte et les sources.

> **Attribution :** Map data: Natural Earth — public domain. Les fichiers GeoJSON Natural Earth sont servis localement depuis `src/desktop-electron/public/map-data/` (manifest : `src/desktop-electron/public/map-data/manifest.json`, couches 110m/50m/10m sous `natural-earth-110m/`, `natural-earth-50m/`, `natural-earth-10m/`).

> **Limite V1 :** la carte reste une visualisation SVG indicative offline. Les niveaux Natural Earth 1:110m / 1:50m / extrait 1:10m améliorent le détail visuel selon le zoom, mais ne constituent pas un SIG complet ni une référence légale de précision : pas de tuiles, pas de fonds externes live, pas de routage et pas de géocodage réseau.

### 4.7 Parcours de démonstration recommandé

Le script détaillé prêt présentation est disponible dans [`docs/demo/demo-script-v1.md`](../demo/demo-script-v1.md).

1. Lancer l’application desktop.
2. Paramètres → **Charger seed**.
3. Dashboard → vérifier compteurs et EventCases prioritaires.
4. Carte + Timeline → vérifier marqueurs, timeline et sélection synchronisée.
5. EventCase → ouvrir un dossier.
6. Consulter provenance agrégée et observations liées.
7. Soumettre un feedback.
8. Observer le score ajusté et son breakdown.
9. Exporter en JSON puis Markdown.
10. Paramètres → vérifier l’audit des actions démo/export/feedback.

Les exports desktop sont téléchargés sous la forme `aegisloop-eventcase-<id>.json` et `aegisloop-eventcase-<id>.md`.

---

## 5. Navigation — 5 vues V1

| Vue | Raccourci | Usage |
|---|---|---|
| Dashboard | `Ctrl+1` | Vue synthétique, KPIs, événements prioritaires |
| Carte + Timeline | `Ctrl+2` | Visualisation géographique et temporelle V1 des EventCases seed/API |
| EventCase | `Ctrl+3` | Détail événement, observations, contradictions, export |
| Observations | `Ctrl+4` | Liste, filtres, provenance, feedback |
| Paramètres | `Ctrl+5` | Connecteurs, scoring, mode démo, thème |

### Raccourcis supplémentaires

| Raccourci | Action |
|---|---|
| `Ctrl+Enter` | Valider une observation |
| `Ctrl+Shift+X` | Invalider une observation |
| `Ctrl+E` | Exporter le dossier courant |
| `Ctrl+K` | Recherche rapide |
| `Ctrl+F` | Filtrer dans la vue courante |
| `Ctrl+D` | Aller au Dashboard |
| `Escape` | Fermer panel/modal |
| `?` | Aide des raccourcis |

---

## 6. Dépannage V1

| Problème | Solution |
|---|---|
| L'application ne démarre pas | Vérifier que .NET 10 SDK 10.0.201+ et Node.js 24+ sont installés |
| Le backend crash au démarrage | Vérifier que le port 5100 n'est pas occupé ; consulter `http://localhost:5100/health` lorsque l'API est lancée |
| Aucune observation ne apparaît | Vérifier que les connecteurs sont actifs et que la collecte a été lancée |
| Le flux RSS retourne une erreur | Vérifier l'URL du flux ; le système réessaie avec backoff exponentiel |
| GDELT ne retourne rien | Vérifier la requête et les filtres pays/thème |
| Le mode démo ne charge pas | Réinitialiser la démo via Paramètres → Mode démo |
| L'export ne fonctionne pas | Vérifier les permissions d'écriture du répertoire de destination |
| Le score ne se met pas à jour après feedback | Attendre 1-2 secondes pour le recalcul ; vérifier que le feedback n'a pas été annulé |

---

## 7. Références

- MVP Solo V1 Officiel : [10-mvp-solo-v1-officiel.md](../review/10-mvp-solo-v1-officiel.md)
- Spécifications fonctionnelles : [01-specs-fonctionnelles.md](01-specs-fonctionnelles.md)
- Spécifications UI/UX : [03-specs-ui-ux.md](03-specs-ui-ux.md)