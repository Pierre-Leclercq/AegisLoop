# AegisLoop V1 — Script de démonstration locale

> **Phase 6 — Demo readiness & polish V1**  
> **Durée cible :** 12 à 15 minutes  
> **Mode :** local desktop, sans cloud, sans auth, sans réseau obligatoire pour le seed

## 1. Prérequis

- Windows 11.
- .NET SDK 10.0.201+ disponible dans le `PATH`.
- Node.js 24+ et npm 11+ disponibles dans le `PATH`.
- Ports locaux libres : `5100` pour l’API, `5173` pour Vite/Electron dev.
- Dépôt positionné à la racine `AegisLoop`.
- Dataset local présent : `examples/demo-data/v1-seed.json`.

Validation rapide avant présentation :

```bat
dotnet build AegisLoop.sln --configuration Release
npm run build --prefix src/desktop-electron
```

## 2. Commande de lancement

Depuis la racine du dépôt :

```bat
scripts\run-desktop.bat
```

Le lancement desktop doit montrer :

1. installation npm déjà à jour ou rapide ;
2. serveur Vite sur `http://localhost:5173` ;
3. build backend ;
4. API démarrée sur `http://localhost:5100` ;
5. Worker démarré ;
6. fenêtre Electron affichée après readiness API.

Health check attendu : `http://localhost:5100/health` répond `200`.

## 3. Scénario narratif

« AegisLoop V1 est un workbench analyste desktop qui transforme un lot d’observations OSINT simulées en dossiers événementiels traçables. Pendant la démo, on part d’un état local maîtrisé, on charge un seed déterministe, on inspecte un dashboard, on vérifie la provenance d’une observation, on ajoute un feedback analyste, puis on ouvre un EventCase, on vérifie sa provenance agrégée, on exporte le dossier et on contrôle l’audit. Enfin, on réinitialise et on recharge pour prouver la reproductibilité. »

## 4. Reset avant démo

1. Lancer le desktop.
2. Ouvrir **Paramètres** (`Ctrl+5`).
3. Cliquer **Reset démo**.
4. Vérifier le statut :
   - État : `Vide` ;
   - Observations : `0` ;
   - EventCases : `0` ;
   - Scores : `0` ;
   - Feedbacks : `0`.

## 5. Étapes détaillées de démonstration

### Étape A — Lancement desktop

À montrer :

- une seule commande : `scripts\run-desktop.bat` ;
- logs indiquant API + Worker ;
- fenêtre Electron ;
- health endpoint OK.

Message oral : « Le desktop orchestre l’API et le Worker locaux ; aucune plateforme cloud n’est requise. »

### Étape B — Chargement seed/replay

Vue : **Paramètres**.

1. Vérifier la section **Mode démo seed/replay V1**.
2. Cliquer **Charger seed**.
3. Cliquer **Rebuild EventCases**.
4. Cliquer **Recalcul scores**.

Données attendues après chargement :

- dataset : `v1-seed-2026-04` ;
- sources démo : `5` ;
- RawItems : `90` ;
- Observations : `90` ;
- EventCases : `8` ;
- Scores : `98` ;
- Feedbacks seedés : `5` ;
- scénarios : `aden-maritime-incident`, `sahel-civic-security`.

À montrer : messages de succès et absence d’erreur.

### Étape C — Dashboard

Vue : **Dashboard** (`Ctrl+1`).

À montrer :

- compteurs réels : RawItems, Observations, EventCases ;
- **EventCases prioritaires** triés par score ;
- répartition par catégorie/source ;
- dernière ingestion ;
- section erreurs récentes.

Données attendues typiques : 90 observations et 8 EventCases.

### Étape D — Carte + Timeline

Vue : **Carte + Timeline** (`Ctrl+2`).

À montrer :

- la vue n’est plus un placeholder ni un sketch cartographique maison ;
- carte V1 locale avec fond Natural Earth public offline multi-échelle : monde 1:110m, régional 1:50m, local 1:10m extrait Sahel/Aden, plus marqueurs EventCases issus du seed ;
- pipeline viewport-driven : sélection de couches selon `viewBox`/zoom, chargement local progressif via `public/map-data/manifest.json`, filtrage des features au viewport et fallback local si un fichier détaillé est indisponible ;
- attribution visible `Map data: Natural Earth — public domain` et badge de niveau actif (`Fond carte : Monde/Régional/Local`) ;
- zoom carte continu par viewport SVG : **Zoom +** et **Zoom −** appliquent un facteur multiplicatif au `viewBox`, sans sauter uniquement entre `Monde/Régional/Local` ; **Réinitialiser** revient au cadrage initial de travail, **Vue globale** affiche le monde SVG complet, **Zoom sélection** centre un EventCase actif ;
- déplacement par drag borné à tous les niveaux et molette réservée au zoom carte autour du curseur quand il est sur le SVG, sans scroll parasite de la page ;
- timeline simple triée chronologiquement ;
- compteurs : éléments affichés, sans coordonnées, période couverte, sources ;
- filtres V1 : source RSS/GDELT, score minimal, scénario ;
- clic sur un marqueur ou un item timeline → panneau détail synchronisé.

Message oral : « La carte est offline et utilise un vrai fond public Natural Earth, domaine public, servi localement via un manifest de couches : pas de tuiles externes, pas de clé API, pas de géocodage réseau. Le zoom utilisateur est maintenant une caméra continue pilotée par le viewBox : la molette et les boutons changent progressivement l’échelle. Le LOD cartographique reste discret et automatique : selon le viewBox courant, le fond bascule entre monde 1:110m, régional 1:50m et extrait local 1:10m Sahel/Aden. Le badge Fond carte permet de vérifier le fond actif, distinct du zoom affiché. »

### Étape E — Observations et provenance

Vue : **Observations** (`Ctrl+4`).

1. Ouvrir la première observation affichée.
2. Vérifier **Provenance réelle** : SourceConnector, RawItem, hash, ingestion, URL source locale.
3. Vérifier **Impact score** : composantes `SourceReliability`, `Corroboration`, `AnalystFeedback`.
4. Saisir une note courte : `Feedback démo observation confirmé`.
5. Cliquer **Confirm**.

À montrer :

- la provenance n’est pas un placeholder ;
- le score/breakdown est visible ;
- le feedback apparaît dans l’historique ;
- le score est recalculé, par exemple un feedback `Confirm` augmente la composante `AnalystFeedback`.

### Étape F — EventCases, feedback et export

Vue : **EventCase** (`Ctrl+3`).

1. Sélectionner l’EventCase prioritaire `Noria checkpoint incident...` ou le premier dossier affiché.
2. Vérifier :
   - titre et score ;
   - breakdown du score ;
   - provenance agrégée ;
   - observations liées ;
   - hashes RawItems.
3. Saisir une note EventCase : `Feedback démo EventCase`.
4. Cliquer **Confirm** ou **Correct** pour rendre l’impact score visible. `Note` reste volontairement neutre.
5. Cliquer **Exporter JSON**.
6. Cliquer **Exporter Markdown**.

Exports attendus :

- fichier JSON : `aegisloop-eventcase-<eventCaseId>.json` ;
- fichier Markdown : `aegisloop-eventcase-<eventCaseId>.md` ;
- récupération via téléchargement Electron/navigateur ;
- le Markdown contient la section `## Limites / incertitudes`.

À montrer : le message de succès affiche le nom de fichier généré.

### Étape G — Audit

Vue : **Paramètres** (`Ctrl+5`) puis **Audit log**.

Cliquer **Rafraîchir audit** si nécessaire.

Actions attendues dans les 200 dernières entrées :

- `DemoReset` ;
- `DemoSeedLoaded` ;
- `DemoEventCasesRebuilt` ;
- `DemoScoresRecalculated` ;
- `AnalystFeedbackSubmitted` ;
- `ObservationScoreCalculated` ;
- `EventCaseScoreCalculated` ;
- `EventCaseExported`.

À montrer : l’audit permet de relier feedback/export/scoring à des cibles concrètes.

### Étape H — Persistance et replay déterministe

1. Fermer l’application.
2. Relancer `scripts\run-desktop.bat`.
3. Vérifier que les données restent cohérentes dans Dashboard/Paramètres.
4. Retourner dans **Paramètres**.
5. Cliquer **Reset démo** puis **Charger seed**.
6. Vérifier à nouveau les volumes attendus : 90 observations, 8 EventCases, 98 scores, 5 feedbacks seedés.

Message oral : « Le reset nettoie l’état de démonstration, et le seed recharge un scénario stable et reproductible. »

## 6. Points à montrer absolument

- Lancement desktop unique.
- API + Worker automatiquement démarrés.
- Seed local déterministe.
- Dashboard alimenté.
- Carte + Timeline alimentée, avec sélection carte/timeline.
- Provenance observation et EventCase.
- Feedback analyste et score recalculé.
- Audit des actions.
- Exports JSON/Markdown récupérables.
- Reset/reload sans incohérence.

## 7. Erreurs connues / limites assumées V1

- **Carte + Timeline** est une V1 simple : fond Natural Earth public domain multi-échelle offline (`world` 1:110m, `regional` 1:50m, `local` extrait 1:10m Sahel/Aden), SVG local indicatif avec caméra `viewBox` continue, zoom/dézoom multiplicatif par boutons ou molette, pan borné, reset, **Vue globale**, **Zoom sélection** et LOD automatique découplé du zoom utilisateur, sans tuiles externes, sans SIG complet, sans clustering avancé, sans heatmap et sans géocodage réseau. Les frontières/côtes Natural Earth sont généralisées et non adaptées aux décisions de précision.
- **Export PDF** absent : explicitement hors V1.
- **SSE/temps réel** non démontré : hors périmètre Phase 6.
- Les données `demo.aegisloop.local` sont simulées et non sensibles.
- Les compteurs Dashboard incluent les connecteurs système par défaut ; le statut démo affiche les 5 connecteurs du seed local.
- Un feedback EventCase de type `Note` est neutre par design ; utiliser `Confirm`, `Correct` ou `Invalidate` pour montrer une variation de score.

## 8. Reset après démo

Option A — conserver l’état pour inspection : ne rien faire.

Option B — revenir à un état propre :

1. **Paramètres** → **Reset démo**.
2. Vérifier Observations/EventCases/Scores/Feedbacks à `0` dans le statut démo.
3. Fermer l’application.

## 9. Checklist prêt avant présentation

- [ ] `dotnet build AegisLoop.sln --configuration Release` vert.
- [ ] `dotnet test AegisLoop.sln --configuration Release --settings .runsettings` vert.
- [ ] `npm run build --prefix src/desktop-electron` vert.
- [ ] `npm test --prefix src/desktop-electron` vert.
- [ ] `dotnet list AegisLoop.sln package --vulnerable --include-transitive` sans vulnérabilité bloquante.
- [ ] `npm audit --audit-level=low --prefix src/desktop-electron` sans vulnérabilité bloquante.
- [ ] Ports 5100 et 5173 libres.
- [ ] `scripts\run-desktop.bat` lance Electron, API et Worker.
- [ ] Health endpoint `http://localhost:5100/health` OK.
- [ ] Reset puis Charger seed redonnent 90 observations / 8 EventCases.
- [ ] Carte + Timeline affiche les EventCases seed, la timeline et une sélection fonctionnelle.
- [ ] Exports JSON et Markdown testés sur un EventCase.