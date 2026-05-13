# Phase 6B — Carte + Timeline V1

## Résumé

La Phase 6B remplace le placeholder **Carte + Timeline** par une vue V1 réelle et démontrable. La vue affiche les EventCases reconstruits depuis les observations seed selon deux axes : une carte simple offline et une timeline chronologique. La sélection depuis la carte ou la timeline met à jour un panneau détail.

## Choix technique

- Carte : SVG React local, désormais alimenté en Phase 6C par un fond Natural Earth 1:110m offline, sans dépendance externe, sans tuiles réseau, sans géocodage.
- Polish carte : viewport SVG piloté par `viewBox`, monde logique nettement plus grand que le cadrage initial, zoom avant/dézoom bornés, bouton **Vue globale**, reset stable vers la vue initiale de travail, déplacement par drag borné et molette souris interceptée sans scroll page parasite.
- Timeline : composant React custom, liste chronologique simple.
- Données : endpoint sobre `GET /api/map-timeline`, alimenté par EventCases, Observations, SourceConnectors, ConfidenceScores et Locations.
- Filtres V1 : source (`Rss`, `Gdelt`, toutes), score minimal, scénario.

Ce choix évite les frictions Leaflet/MapLibre en packaging V1 et respecte la priorité démo stable/offline. Leaflet ou MapLibre pourront remplacer le SVG ultérieurement sans changer le contrat métier.

## Données utilisées

Le seed `examples/demo-data/v1-seed.json` contient maintenant des coordonnées déterministes non sensibles pour les 8 groupes :

- Sahel Civic Security : Noria road corridor, Talar river shelters, Maro market, Kivu civic center, Luma clinic.
- Aden Maritime Incident : Gulf of Aden eastbound corridor, Bab el Mandeb traffic lane, Djibouti rescue coordination zone.

Au chargement du seed, ces lieux créent des `Location` locales et lient les observations via `GeoLocationId`. Le rebuild EventCases propage le lieu majoritaire vers `EventCase.LocationId`.

## API

Endpoint ajouté : `GET /api/map-timeline`.

Réponse : EventCaseId, titre, score, statut, catégorie, date, sources, types source, nombre d’observations, latitude/longitude si disponibles, localisation textuelle, région/pays, scénario. Paramètres optionnels : `source`, `minScore`, `scenario`.

## Limites V1 assumées

- Pas de SIG complet.
- Pas de fonds satellite, WMS/WFS, routage, heatmap ou clustering avancé.
- Pas de géocodage réseau ni Nominatim.
- Carte SVG indicative, conçue pour démonstration locale stable : depuis la Phase 6C, le sketch maison est remplacé par Natural Earth public domain, mais ce n’est toujours pas un SIG complet.
- Le reset revient au cadrage initial de travail et conserve la sélection carte/timeline courante.
- Le bouton **Vue globale** et le dézoom maximum affichent un contexte SVG offline élargi autour des marqueurs ; ils ne chargent pas de tuiles ni de données cartographiques supplémentaires.

## Suite Phase 6C

Voir [`phase6c-public-basemap.md`](phase6c-public-basemap.md) pour la source Natural Earth, l’attribution, les données GeoJSON versionnées et les limites de précision.

## Tests

- Frontend : `src/desktop-electron/src/tests/MapTimeline.test.tsx` couvre rendu, loading/error/empty, marqueurs SVG, timeline, sélection carte/timeline, filtres, changements de `viewBox` après zoom, dézoom avec contexte élargi, bouton **Vue globale**, reset du viewport initial, conservation de la sélection, marqueurs sélectionnables en vue globale, désactivation des boutons aux bornes zoom min/max et interception testable de la molette via `preventDefault`.
- Backend : test API `MapTimeline_Returns_Seed_EventCases_With_Geo_Timeline_And_Filters` couvre le seed géographique, l’endpoint et les filtres.

## Statut

Carte + Timeline V1 terminée pour le périmètre MVP Solo V1.