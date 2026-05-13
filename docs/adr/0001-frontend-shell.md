# ADR 0001 — Choix du shell frontend : Electron

> **Statut :** Accepté  
> **Date :** 2026-04-23  
> **Décideur :** Architecte

---

## Contexte

Aegis Loop est un poste analyste desktop. Il faut un shell capable d'héberger une interface web riche, avec accès au système de fichiers, menu système, auto-update et icône de barre des tâches.

## Décision

Utiliser **Electron** comme shell desktop, avec **React + TypeScript** pour le frontend et **Tailwind CSS + shadcn/ui** pour les composants.

## Alternatives considérées

| Alternative | Avantages | Inconvénients |
|---|---|---|
| **Tauri** | Plus léger, Rust, meilleure sécurité | Écosystème moins mature, courbe d'apprentissage Rust, moins de ressources |
| **Qt / QML** | Performance native, mature | Pas d'interface web, langage propriétaire, difficile à recruter |
| **WPF / WinUI** | Natif Windows, .NET | Pas cross-platform, XAML complexe, pas d'écosystème web |
| **NW.js** | Similaire à Electron | Communauté plus petite, moins de support |

## Justification

1. **Écosystème mature** — Electron est le standard de facto pour les desktop apps web
2. **Cross-platform** — Windows, macOS, Linux avec un seul codebase
3. **Auto-update** — electron-updater intégré
4. **Accès système** — File dialogs, system tray, menus natifs
5. **React + TypeScript** — Écosystème de composants le plus riche, typage fort
6. **shadcn/ui** — Composants accessibles, customisables, pas de lock-in
7. **Porteur solo** — Beaucoup de ressources et de documentation disponibles

## Conséquences

- Taille du binaire plus importante que Tauri (~150 MB vs ~10 MB)
- Consommation mémoire plus élevée (Chromium)
- Complexité IPC à gérer entre Electron et le backend C#
- Nécessité de gérer le cycle de vie du processus backend .NET

## Références

- [02-architecture-technique.md](../specs/02-architecture-technique.md) section 2.1 et 13
- [07-glossaire-et-decisions.md](../specs/07-glossaire-et-decisions.md) arbitrage A5