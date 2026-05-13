# Phase 6 — Demo readiness & polish V1

## Résumé

La Phase 6 a été exécutée comme un parcours de démonstration local complet : lancement desktop, readiness API, seed/rebuild/recalculate, dashboard, observations, provenance, feedback, EventCases, exports, audit et reset/reload déterministe.

## Parcours réellement vérifié

- `scripts\run-desktop.bat` lance Vite/Electron, build backend, API et Worker.
- `GET /health` retourne `200`.
- `POST /api/demo/reset`, `POST /api/demo/load`, `POST /api/demo/rebuild`, `POST /api/demo/recalculate` fonctionnent.
- Volumes seed observés : 5 connecteurs seed, 90 RawItems, 90 observations, 8 EventCases, 98 scores, 5 feedbacks seedés.
- Dashboard alimenté avec compteurs, EventCases prioritaires, dernière ingestion et erreurs récentes.
- Provenance observation disponible : SourceConnector, RawItem, hash, ingestion, métadonnées.
- Feedback observation `Confirm` vérifié avec score ajusté visible.
- Provenance EventCase disponible : observations, sources, RawItems, hashes, score.
- Carte + Timeline V1 disponible : endpoint `GET /api/map-timeline`, carte SVG offline, timeline simple, filtres source/score/scénario, sélection synchronisée.
- Exports JSON et Markdown vérifiés ; Markdown contient `## Limites / incertitudes`.
- Audit vérifié avec actions démo, feedback, scoring et export.
- Reset/reload vérifié : retour à 90 observations / 8 EventCases après rechargement.

## Irritants corrigés

- Export desktop : le bouton ne faisait que valider la réponse API ; il déclenche maintenant un téléchargement local et affiche le nom de fichier.
- Audit démo : la vue Paramètres charge maintenant 200 entrées pour éviter que reset/load/rebuild/recalculate/feedback/export soient masqués par les nombreuses entrées de scoring.
- Logs desktop/API : le niveau EF Core en Development est ramené à `Warning` pour éviter un bruit très important pendant une présentation.
- Persistance API/Worker : API et Worker pointent vers la même base SQLite locale au niveau racine du dépôt (`../../aegisloop.db`) afin d’éviter des états séparés selon le projet lancé.
- Feedback EventCase : la sélection courante est préservée après soumission pour éviter un retour involontaire au premier EventCase.

## Limites assumées V1

- Carte + Timeline est une V1 simple et offline : pas de fond de carte externe, pas de SIG complet, pas de clustering avancé, pas de heatmap, pas de géocodage réseau.
- Pas de PDF, pas de SSE, pas d’auth, pas de cloud, pas de nouveau modèle IA.
- Les données seed sont simulées.