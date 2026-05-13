# Guide de récupération des données cartographiques LOD offline

Ce guide documente comment reproduire les datasets utilisés par la carte multi-échelle offline AegisLoop V1. Il n’est pas exécuté au runtime : les fichiers GeoJSON sont versionnés localement.

## Source

- Projet officiel : <https://www.naturalearthdata.com/>
- Miroir GeoJSON pratique : <https://github.com/nvkelso/natural-earth-vector/tree/master/geojson>
- Licence : Natural Earth public domain.

## Fichiers à récupérer

### Monde — 1:110m

- `ne_110m_admin_0_countries.geojson`
- `ne_110m_land.geojson`
- `ne_110m_lakes.geojson`
- `ne_110m_rivers_lake_centerlines.geojson`

Destination : `src/desktop-electron/src/map-data/natural-earth-110m/`

### Régional — 1:50m

- `ne_50m_admin_0_countries.geojson`
- `ne_50m_land.geojson`
- `ne_50m_lakes.geojson`
- `ne_50m_rivers_lake_centerlines.geojson`

Destination : `src/desktop-electron/src/map-data/natural-earth-50m/`

### Local — 1:10m extrait démo

Télécharger les couches 1:10m équivalentes, puis découper sur les zones seed :

- Sahel : lon `-20..35`, lat `8..25` ;
- Golfe d’Aden : lon `35..55`, lat `5..18`.

Destination : `src/desktop-electron/src/map-data/natural-earth-10m/`

La découpe actuelle conserve les features dont la bbox intersecte au moins une zone seed. Elle évite de versionner le 1:10m mondial complet tout en gardant un vrai enrichissement local public et offline.

## Commande PowerShell indicative

```powershell
$root = "src/desktop-electron/src/map-data"
$base = "https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/geojson"
$files = @(
  "ne_50m_admin_0_countries.geojson",
  "ne_50m_land.geojson",
  "ne_50m_lakes.geojson",
  "ne_50m_rivers_lake_centerlines.geojson"
)
New-Item -ItemType Directory -Force -Path "$root/natural-earth-50m"
foreach ($file in $files) {
  Invoke-WebRequest -Uri "$base/$file" -OutFile "$root/natural-earth-50m/$file"
}
```

## Contrôles après récupération

1. Vérifier que l’application build sans appel réseau : `npm run build --prefix src/desktop-electron`.
2. Vérifier que le badge UI change entre monde/régional/local.
3. Vérifier l’attribution `Map data: Natural Earth — public domain`.
4. Vérifier le poids des fichiers avant commit.
