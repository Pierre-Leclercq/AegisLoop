# Natural Earth 1:10m clipped local demo extract

Ces fichiers GeoJSON sont des extraits locaux issus de Natural Earth Vector 1:10m, limités aux zones de démonstration AegisLoop V1 : Sahel et Golfe d’Aden.

- `ne_10m_admin_0_countries.geojson`
- `ne_10m_land.geojson`
- `ne_10m_lakes.geojson`
- `ne_10m_rivers_lake_centerlines.geojson`

Source officielle : <https://www.naturalearthdata.com/>  
Miroir GeoJSON utilisé : <https://github.com/nvkelso/natural-earth-vector/tree/master/geojson>

Découpe appliquée pour maintenir un poids V1 raisonnable :

- Sahel demo bbox : longitude `-20..35`, latitude `8..25` ;
- Aden demo bbox : longitude `35..55`, latitude `5..18`.

La découpe conserve les features Natural Earth dont la bbox intersecte ces zones. Elle n’ajoute aucune source OSINT, aucun fond externe live et aucun contenu arbitraire. Ces couches servent le niveau LOD `local` lorsque le `viewBox` SVG descend sous le seuil local.
