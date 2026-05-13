# Phase 6F — Zoom continu + LOD automatique découplé

## Résumé

La Phase 6F corrige la confusion UX entre **zoom utilisateur** et **niveau de détail cartographique**. Les trois datasets Natural Earth restent discrets (`world`, `regional`, `local`), mais la caméra SVG est maintenant pilotée par un `viewBox` continu : molette, boutons, pan et zoom sélection modifient directement `x`, `y`, `width` et `height` sans limiter l’utilisateur à trois paliers.

## Correction apportée

- **Zoom utilisateur continu** : chaque cran de molette et chaque clic **Zoom + / Zoom −** applique un facteur multiplicatif (`1.22`) au `viewBox` courant.
- **LOD automatique séparé** : `selectMapDetailLevel(viewBox)` choisit seulement le fond cartographique à rendre selon le zoom calculé ; elle n’impose pas les tailles possibles de caméra.
- **Zoom autour d’un ancrage** : la molette zoome autour du curseur ; les boutons zooment autour de l’EventCase sélectionné s’il est visible, sinon autour du centre courant.
- **Pan borné** : le déplacement par drag continue à déplacer le `viewBox` à tous les niveaux de zoom, avec clamp dans le monde SVG.
- **Raccourcis de caméra conservés** : **Vue globale**, **Réinitialiser** et **Zoom sélection** positionnent le `viewBox`, mais ne remplacent pas le zoom continu.
- **Marqueurs conservés** : les marqueurs SVG gardent une taille visuelle raisonnable via compensation d’échelle, et les boutons overlay restent cliquables après zoom profond.

## Nouvelle logique viewBox / caméra

L’état de caméra est le `viewBox` complet :

```ts
{ x: number; y: number; width: number; height: number }
```

Le zoom multiplicatif suit le principe :

```ts
newWidth = width / factor      // zoom avant
newWidth = width * factor      // zoom arrière via factor inverse
newHeight = newWidth / (1200 / 650)
```

L’ancrage conserve la position relative du curseur, du marqueur sélectionné ou du centre courant. Le résultat est toujours clampé dans `GLOBAL_VIEWBOX`.

## Nouvelle logique LOD

Le zoom affiché est calculé depuis le monde complet :

```ts
zoomLevel = GLOBAL_VIEWBOX.width / currentViewBox.width
```

La sélection LOD reste discrète :

| Zoom utilisateur calculé | Fond rendu |
|---:|---|
| `< 2×` | Monde — Natural Earth 1:110m |
| `>= 2×` et `< 8×` | Régional — Natural Earth 1:50m |
| `>= 8×` | Local — Natural Earth 1:10m extrait seed |

Ces seuils ne limitent pas la caméra : par exemple plusieurs viewBox entre `2×` et `8×` restent en `regional`, et plusieurs viewBox au-dessus de `8×` restent en `local`.

## Bornes retenues

- Monde SVG / Vue globale : `0 0 1200 650`.
- Ratio caméra : `1200 / 650`.
- Cadrage initial runtime : calculé depuis les EventCases localisés, largeur minimale `520`, hauteur `281.67`.
- Facteur de zoom : `1.22` par cran/clic.
- Zoom maximum : `MIN_VIEWBOX_WIDTH = 40`, `MIN_VIEWBOX_HEIGHT = 21.67`, soit `30×` par rapport au monde complet.
- Zoom sélection : largeur `80`, hauteur `43.33`, soit `15×` par rapport au monde complet.
- Zoom arrière maximum : `GLOBAL_VIEWBOX`, soit `1×`.

## Performance

Aucune dépendance n’a été ajoutée. Les GeoJSON restent importés et parsés une seule fois au niveau module. Le composant `BasemapLayer` est mémorisé avec `React.memo`, ce qui évite de reconstruire les chemins Natural Earth à chaque mouvement de souris ou changement de `viewBox` tant que le niveau LOD actif ne change pas.

## Tests

`src/desktop-electron/src/tests/MapTimeline.test.tsx` couvre notamment :

- zoom avant continu avec plus de trois tailles de `viewBox` ;
- zoom arrière continu jusqu’à la vue globale ;
- LOD découplé : plusieurs viewBox différents peuvent partager le même LOD ;
- molette avec `preventDefault` et interception du scroll page ;
- Zoom sélection puis poursuite de Zoom + / Zoom − depuis cette position ;
- marqueurs visibles et sélection carte/timeline/détail conservée après zoom profond.

## Limites assumées

- Le fond `local` reste un extrait Natural Earth 1:10m Sahel/Aden, pas un fond local mondial complet.
- Natural Earth reste généralisé : pas de précision rue, routage, cadastre ou usage légal/opérationnel fin.
- La projection SVG équirectangulaire existante est conservée pour rester dans une correction ciblée sans dépendance.

## Statut

Correction Phase 6F terminée : zoom utilisateur continu + LOD cartographique automatique découplé.