# Natural Earth 1:110m offline basemap

## Source

These GeoJSON files are from Natural Earth Vector, 1:110m cultural/physical layers:

- `ne_110m_admin_0_countries.geojson`
- `ne_110m_land.geojson`
- `ne_110m_lakes.geojson`
- `ne_110m_rivers_lake_centerlines.geojson`

Official project: <https://www.naturalearthdata.com/>

Repository mirror used for the checked-in GeoJSON export: <https://github.com/nvkelso/natural-earth-vector/tree/master/geojson>

## Licence / attribution

Natural Earth data is public domain. The Carte + Timeline view displays the attribution:

`Map data: Natural Earth — public domain`

Since Phase 6E this directory is the `world` LOD layer set. Regional and local LODs are stored in sibling directories:

- `../natural-earth-50m/` for `regional`;
- `../natural-earth-10m/` for the clipped `local` demo extract.

## Intended use in AegisLoop V1

This dataset is intentionally small and offline. It provides a credible presentation basemap at continental/regional scale for the Sahel, Gulf of Aden and surrounding Africa/Middle East context. It is not intended for street-level analysis, routing, precise boundaries or legal/geodetic decisions.

No map tiles, live OpenStreetMap endpoint, geocoder, API key or external cartographic service is used at runtime.