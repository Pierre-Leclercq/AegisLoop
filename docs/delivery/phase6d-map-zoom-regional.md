# Phase 6D — Zoom régional/local Natural Earth

## Résumé

La Phase 6D adapte le zoom/pan de **Carte + Timeline** au fond Natural Earth offline introduit en Phase 6C. La carte reste en SVG `1200 × 650`, sans dépendance ajoutée, mais les bornes de `viewBox` permettent désormais de passer d’une vue monde à un cadrage régional puis local autour des EventCases seedés.

## Contrôles utilisateur

- **Vue globale** : affiche le monde Natural Earth complet (`0 0 1200 650`).
- **Réinitialiser** : revient au cadrage initial calculé depuis les EventCases localisés, avec marge confortable sur Sahel / Golfe d’Aden.
- **Zoom + / Zoom −** : zoom progressif autour de l’EventCase sélectionné si disponible, sinon autour du centre courant.
- **Molette** : zoom autour du curseur et intercepte le scroll page dans la zone carte.
- **Pan** : déplacement par glisser-déposer, borné dans le monde SVG.
- **Zoom sélection** : centre l’EventCase actif dans un viewBox local dédié, sans perdre la sélection.

## Bornes retenues

- Monde SVG : `1200 × 650`.
- Vue globale : `0 0 1200 650`.
- Vue initiale de référence : largeur `520`, hauteur `281.67`, ratio monde `1200/650`.
- Vue initiale runtime : calculée avec `fitViewBoxToPoints(points, 0.24, 520, 281.67)` pour englober les EventCases localisés.
- Zoom local minimal : largeur `80`, hauteur `43.33`, soit environ `6.5×` par rapport à la vue initiale.
- Zoom sélection : largeur `120`, hauteur `65`, soit environ `4.3×`.

Les marqueurs compensent visuellement le zoom afin de rester lisibles et cliquables lors du zoom local.

## Limite cartographique

Natural Earth 1:110m fournit un contexte monde/régional généralisé. Le zoom local sert à inspecter confortablement les groupes d’EventCases de démonstration, pas à offrir une précision SIG de rue, de routage ou de frontière opérationnelle fine.

## Tests

`src/desktop-electron/src/tests/MapTimeline.test.tsx` couvre :

- zoom avant beaucoup plus profond jusqu’au viewBox local minimal ;
- conservation de marqueurs visibles/sélectionnables après zoom fort ;
- retour Vue globale ;
- retour Réinitialiser vers le cadrage initial ;
- bouton Zoom sélection ;
- centrage du viewBox autour de l’EventCase sélectionné ;
- synchronisation carte/timeline/détail après zoom local ;
- interception de la molette sans propagation du scroll page.

## Statut

Correction UX ciblée terminée : le fond Natural Earth offline conserve la vue monde tout en permettant un zoom régional/local exploitable pour les EventCases seedés.