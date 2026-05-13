# Phase 6C — Fond cartographique public offline

## Résumé

La Phase 6C remplace le sketch SVG maison de **Carte + Timeline** par un fond cartographique public, local et offline basé sur Natural Earth. La vue conserve le zoom par `viewBox`, le pan borné, **Vue globale**, **Réinitialiser**, la molette sans scroll page, la timeline et la sélection synchronisée carte/timeline. Les bornes de zoom régional/local sont affinées en Phase 6D dans `docs/delivery/phase6d-map-zoom-regional.md`.

## Source cartographique utilisée

Fond utilisé : Natural Earth Vector 1:110m, GeoJSON local versionné dans `src/desktop-electron/src/map-data/natural-earth-110m/`.

Couches intégrées :

- `ne_110m_land.geojson` — terres / continents ;
- `ne_110m_admin_0_countries.geojson` — pays et frontières admin 0 ;
- `ne_110m_lakes.geojson` — lacs ;
- `ne_110m_rivers_lake_centerlines.geojson` — rivières principales.

Source officielle : <https://www.naturalearthdata.com/>. Les GeoJSON ont été récupérés depuis le miroir public Natural Earth Vector maintenu par le projet : <https://github.com/nvkelso/natural-earth-vector/tree/master/geojson>.

## Licence / attribution

Natural Earth est publié en domaine public. L’attribution visible dans la carte est :

> Map data: Natural Earth — public domain

## Choix technique de rendu

- Pas de Leaflet/MapLibre et pas de dépendance frontend ajoutée.
- Chargement local des GeoJSON via import Vite `?raw` puis `JSON.parse`.
- Projection equirectangulaire documentée en code : longitude `[-180, 180]` vers largeur SVG, latitude `[90, -90]` vers hauteur SVG.
- Rendu SVG React : océan/graticule, terres, lacs, frontières pays, rivières principales, quelques libellés régionaux, puis marqueurs EventCases par-dessus.
- Le `viewBox` travaille maintenant sur un espace mondial `1200 × 650` aligné avec le fond Natural Earth, avec cadrage initial Sahel/Golfe d’Aden et vue globale monde.

## Limites assumées

- Natural Earth 1:110m est adapté à une démo régionale/continentale, pas à une analyse SIG fine.
- Les frontières et côtes sont généralisées ; elles ne doivent pas servir de référence légale ou opérationnelle précise.
- Aucun routage, géocodage, recherche d’adresse, tuile raster/vectorielle externe ou moteur SIG complet n’est inclus en Phase 6C.
- Depuis la Phase 6D, le zoom local améliore l’inspection des EventCases, mais reste limité par la généralisation Natural Earth 1:110m : contexte régional, pas précision rue.

## Tests

`src/desktop-electron/src/tests/MapTimeline.test.tsx` vérifie désormais :

- rendu du fond Natural Earth offline ;
- attribution cartographique visible ;
- marqueurs EventCase toujours visibles/sélectionnables ;
- zoom avant/arrière, reset, vue globale et molette ;
- sélection synchronisée carte/timeline ;
- états loading/error/empty ;
- filtres API existants.

## Statut

Fond cartographique public offline terminé pour la V1 : aucune tuile externe, aucun service réseau cartographique et aucune clé API ne sont nécessaires au runtime.