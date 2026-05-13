# Package layout — double package.json

AegisLoop currently contains two `package.json` files:

| Location | Role |
|---|---|
| `/package.json` | Root-level, contains `maplibre-gl` and `jsdom` |
| `/src/desktop-electron/package.json` | Desktop app, contains Electron + React + Vite + Vitest |

## Observations

- **`/package.json`** declares `maplibre-gl` (map rendering library) and `jsdom` as dependencies. There are no scripts defined. This file appears to serve as a placeholder or a leftover from prototyping.
- **`/src/desktop-electron/package.json`** is the active frontend workspace. It contains all scripts (`dev`, `build`, `lint`, `test`, `electron:dev`, `electron:build`), all UI dependencies (`react`, `react-dom`, `react-router-dom`), and all dev tooling (`vite`, `vitest`, `electron`, `typescript`, etc.). The CI workflow (`ci.yml`) targets this directory exclusively.

## Recommendation

- **Consolidate into `src/desktop-electron/package.json`**: if `maplibre-gl` and `jsdom` are needed by the desktop-electron frontend, they should be moved into `src/desktop-electron/package.json` as dependencies. The root `package.json` can then be removed, or kept as a lightweight npm workspace root if a monorepo structure is desired later.
- **Alternatively**, adopt a proper npm workspaces setup where both `package.json` files are part of a declared workspace. This would allow `npm install` from the root to install both sets of dependencies.

No destructive action is taken in this pass. The decision to consolidate or formalize the dual layout is left as a follow-up task.