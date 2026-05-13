# Progress — 02-architecture-technique.md

- **Aligné V1 officielle :** Oui
- **Date de synchronisation :** 2026-04-23
- **Version :** 2.0

## Points stables
- 6 projets C# + desktop-electron
- 11 types de domaine V1
- ~20 endpoints API
- Scoring 3 composantes
- Cycle de vie Electron ↔ .NET
- Connecteurs RSS + GDELT uniquement
- Persistance SQLite + EF Core
- Stratégie de géocodage (Nominatim + cache)

## Points encore ouverts
- Composant timeline exact (lib vs custom, pendant Phase 2)
- Stratégie de géocodage offline (Nominatim + cache, GeoNames locale en V2)
- Format exact du fichier user config (pendant implémentation)