# Phase 6G — Rendu cartographique viewport-driven + chargement local progressif

## 1) Résumé de la phase

La Phase 6G finalise la transition de la vue **Carte + Timeline** d’un rendu “poster SVG” vers un rendu **viewport-aware**, offline, piloté par un manifest et des couches chargées localement à la demande.

Objectifs atteints :

- sélection des couches cartographiques selon `viewBox` + zoom ;
- chargement progressif local depuis `public/map-data` ;
- filtrage des features avant rendu selon le viewport ;
- labels pays / EventCases régulés (priorités, limites, collisions simples) ;
- fallback robuste si couche locale indisponible ;
- validations frontend vertes (build/test/audit).

## 2) Cause racine traitée

Le problème n’était plus le seul niveau de zoom : la carte gardait un comportement de rendu insuffisamment lié au viewport (coûts et lisibilité), avec une dépendance implicite à des chargements non progressifs.

La correction Phase 6G cible donc le **pipeline de rendu** : “quelle couche charger et afficher selon ce que l’utilisateur voit réellement”.

## 3) Architecture cible (vue d’ensemble)

Le pipeline runtime est désormais :

1. calcul zoom (`getZoomLevel`) et LOD demandé (`selectMapDetailLevel`) ;
2. sélection des couches visibles (`selectVisibleMapLayers`) par zoom + intersection bbox ;
3. fetch local du `manifest.json` puis des GeoJSON ciblés ;
4. cache mémoire des promesses de chargement ;
5. filtrage des features au viewport (`filterFeaturesForView`) ;
6. génération labels pays + labels EventCase avec plafonds/collisions ;
7. rendu SVG multi-couches avec fallback automatique.

## 4) Manifest cartographique

Manifest public runtime : `src/desktop-electron/public/map-data/manifest.json`.

Il décrit 12 couches (`world/regional/local`) avec, par couche :

- `id`, `level`, `type`, `file`
- `bbox`, `minZoom`, `maxZoom`
- `priority`, `label`, `scaleLabel`, `attribution`

Ce manifest est lu via `fetch('/map-data/manifest.json')` (résolu par `mapAssetUrl`).

## 5) Chargement local progressif (lazy load)

Le hook `useViewportMapLayers(viewBox)` :

- tente d’abord le manifest public ;
- retombe sur `DEFAULT_MAP_LAYER_MANIFEST` si erreur ;
- charge uniquement les fichiers nécessaires au viewport courant ;
- mémorise les chargements via `mapLayerFileCache: Map<string, Promise<RenderableMapFeature[]>>`.

Les couches défaillantes sont suivies dans `failedFiles` pour éviter des retries inutiles et activer le fallback.

## 6) Sélection viewport + zoom

`selectVisibleMapLayers(viewBox, zoomLevel, manifest)` :

- filtre d’abord les couches qui intersectent le viewport (`bbox` projetée) ;
- conserve les couches dans la fenêtre `[minZoom, maxZoom]` ;
- sinon fallback hiérarchique : `local -> regional -> world` ;
- tri + dédoublonnage stable (`sortAndDedupeLayers`).

Résultat : on rend ce qui est pertinent visuellement, sans surcharger le DOM SVG.

## 7) Filtrage des features avant rendu

`filterFeaturesForView(features, viewBox, padding)` applique un culling géométrique par bbox projetée.

Dans le rendu des couches :

- padding modéré (`0.16`) pour éviter le pop visuel brutal ;
- seules les features visibles sont passées à `BasemapLayer`.

## 8) Labels pays et EventCases

Contrôles Phase 6G :

- affichage pays conditionné au zoom (`shouldShowCountryLabels`) ;
- affichage Event labels conditionné au zoom (`shouldShowEventLabels`) ;
- plafonds dynamiques de labels selon niveau ;
- tri par priorité ;
- rejet collision rectangle simple (`limitLabelsByViewportAndPriority`).

Les labels EventCases conservent une lisibilité stable en zoom profond via taille en unités carte (`screenToMapUnits`) et jitter déterministe léger.

## 9) Fallbacks et robustesse

Fallbacks explicitement couverts :

- manifest distant invalide/indisponible => manifest embarqué ;
- fichiers de couches indisponibles => exclusion de ces fichiers + re-sélection du meilleur LOD disponible ;
- badge/états UI de fallback dans la barre de contrôle (`Fallback LOD`, `Fallback couches`, `Manifest par défaut utilisé`).

## 10) Modifications principales de code

- `src/desktop-electron/src/views/mapRendering.ts`
  - module central Phase 6G (manifest, sélection, lazy-load, filtrage, labels, cache)
- `src/desktop-electron/src/views/MapTimeline.tsx`
  - intégration `useViewportMapLayers`, `summarizeMapLayers`, rendu multi-couches
  - attribution dynamique + badges LOD/fallback
- `src/desktop-electron/public/map-data/manifest.json`
  - manifest runtime public
- `src/desktop-electron/src/tests/MapTimeline.test.tsx`
  - stabilisation assertions (encodage / robustesse viewport-driven)

## 11) Validation exécutée

Validations frontend exécutées avec succès :

- `npm run build --prefix src/desktop-electron` ✅
- `npm test --prefix src/desktop-electron` ✅
- `npm audit --audit-level=low --prefix src/desktop-electron` ✅ (`found 0 vulnerabilities`)

Note : les tests `MapTimeline` sont verts mais émettent des warnings `act(...)` non bloquants sur certaines interactions successives de zoom.

## 12) Risques / limites restantes

- Le niveau `local` reste un extrait 1:10m (zone seed), pas un fond local mondial complet.
- La projection SVG actuelle reste volontairement simple (équirectangulaire) pour conserver la cible V1.
- Les warnings React `act(...)` peuvent être traités ultérieurement (amélioration de tests, pas un blocage fonctionnel).

## 13) Compatibilité docs V1

La documentation V1 est alignée sur :

- fond Natural Earth offline multi-échelle ;
- caméra zoom/pan continue ;
- distinction zoom utilisateur vs LOD discret ;
- absence de dépendance cartographique externe runtime.

## 14) Statut final Phase 6G

**Terminé côté frontend Phase 6G** : rendu viewport-driven + chargement local progressif + sélection de couches + filtrage + labels contrôlés + fallback + tests/build/audit frontend verts.

Les validations .NET et le récapitulatif final global restent à exécuter dans la séquence de clôture complète.
