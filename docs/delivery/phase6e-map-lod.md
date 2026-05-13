# Phase 6E — Carte multi-échelle offline / LOD cartographique

## Résumé

La Phase 6E a introduit le **niveau de détail cartographique offline** : le fond n’est plus un unique Natural Earth 1:110m agrandi, car la carte choisit un jeu de couches offline selon le `viewBox` visible. La Phase 6F complète cette étape en explicitant que ce LOD discret ne doit pas remplacer le zoom utilisateur : la caméra est désormais continue et le LOD devient seulement une conséquence automatique du zoom courant.

## Datasets utilisés

| Niveau | Seuil Phase 6E historique | Dataset rendu | Répertoire |
|---|---:|---|---|
| `world` | `> 700` unités SVG | Natural Earth 1:110m complet | `src/desktop-electron/src/map-data/natural-earth-110m/` |
| `regional` | `> 250` et `<= 700` | Natural Earth 1:50m complet | `src/desktop-electron/src/map-data/natural-earth-50m/` |
| `local` | `<= 250` | Natural Earth 1:10m extrait Sahel/Aden | `src/desktop-electron/src/map-data/natural-earth-10m/` |

Les couches utilisées à chaque niveau sont : terres, pays/admin, lacs et rivières/lake centerlines. Les marqueurs EventCases restent superposés au-dessus du fond actif.

## Source, licence et poids

Source : Natural Earth Vector, domaine public, via le miroir GeoJSON public <https://github.com/nvkelso/natural-earth-vector/tree/master/geojson>. L’attribution UI reste visible : `Map data: Natural Earth — public domain`, enrichie avec l’échelle active.

Poids constaté des données cartographiques versionnées : environ `20,305,587` octets avant overhead Git, dont environ `1.05 MB` pour 1:110m, `6.42 MB` pour 1:50m et `12.83 MB` pour l’extrait 1:10m local. Le 1:10m complet n’est pas versionné : l’extrait conserve les features intersectant les bboxes démo Sahel (`-20..35`, `8..25`) et Aden (`35..55`, `5..18`).

## Fonction de sélection LOD

La fonction exportée et testable est :

```ts
selectMapDetailLevel(viewBox): MapDetailLevel
```

En Phase 6E, elle retournait historiquement :

- `world` si `viewBox.width > 700` ;
- `regional` si `250 < viewBox.width <= 700` ;
- `local` si `viewBox.width <= 250`.

La résolution effective passe par `resolveMapLayerSet`, qui fallback sans crash vers le meilleur niveau disponible : `local → regional → world`.

Depuis la Phase 6F, la sélection reste pure et testable, mais elle est exprimée en fonction du zoom calculé depuis le monde complet (`GLOBAL_VIEWBOX.width / currentViewBox.width`) :

- `zoomLevel < 2` → `world` ;
- `2 <= zoomLevel < 8` → `regional` ;
- `zoomLevel >= 8` → `local`.

Ces seuils ne contraignent pas le `viewBox` à trois tailles. Plusieurs viewBox intermédiaires peuvent appartenir au même LOD.

## Rendu UI

La barre de contrôle affiche un badge discret :

- `Fond carte : Monde 1:110m` ;
- `Fond carte : Régional 1:50m` ;
- `Fond carte : Local 1:10m extrait seed`.

Le `<g data-testid="natural-earth-basemap">` porte aussi `data-map-detail-level` pour les tests. Si un niveau détaillé manque, un message de fallback est prévu dans la barre de contrôle.

## Pourquoi pas des tuiles OSM live ?

La V1 doit rester reproductible et offline : pas d’appel réseau runtime, pas de clé API, pas de dépendance à un serveur de tuiles public et pas de bulk-download de tuiles. Des GeoJSON Natural Earth versionnés localement suffisent pour une carte de démonstration crédible à l’échelle monde/région/local seed, sans transformer l’application en SIG complet.

## Phase 6F — découplage zoom / LOD

Voir [`phase6f-continuous-zoom.md`](phase6f-continuous-zoom.md) pour la correction UX ciblée : zoom molette continu, boutons multiplicatifs, pan borné à tous les niveaux, Vue globale, Réinitialiser, Zoom sélection et marqueurs conservés.

## Limites

- Natural Earth reste une carte généralisée, non adaptée aux décisions légales, cadastrales, routage ou précision rue.
- Le niveau `local` est un extrait 1:10m démo, pas un fond local mondial complet.
- La projection SVG équirectangulaire existante est conservée pour limiter le changement d’architecture.
- Les libellés restent simples et régionaux.

## Validations Phase 6E

Les tests frontend ajoutés vérifient la sélection LOD, le changement de dataset après zoom, le retour world après Vue globale, la sélection de marqueurs après changement LOD et le fallback de résolution.
