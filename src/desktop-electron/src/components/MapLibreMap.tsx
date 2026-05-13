import React from 'react';
import maplibregl from 'maplibre-gl';
import 'maplibre-gl/dist/maplibre-gl.css';

// -- Styles de fond de carte disponibles ----------------------------------
export type MapStyleId = 'osm-standard' | 'satellite-imagery' | 'topographic-terrain';

const STYLE_URLS: Record<MapStyleId, string> = {
  'osm-standard': '/src/map-styles/osm-style.json',
  'satellite-imagery': '/src/map-styles/satellite-style.json',
  'topographic-terrain': '/src/map-styles/terrain-style.json',
};

const STYLE_LABELS: Record<MapStyleId, string> = {
  'osm-standard': '🗺️ OpenStreetMap',
  'satellite-imagery': '🛰️ Satellite ESRI',
  'topographic-terrain': '⛰️ Topographique',
};

// -- Type des marqueurs à afficher ----------------------------------------
export type MapMarker = {
  id: string;
  lon: number;
  lat: number;
  title: string;
  color: string;
  /** Nom du lieu pour le label sur la carte (ex: "Niamey, Niger") */
  locationLabel?: string | null;
};

// -- Props du composant ---------------------------------------------------
type MapLibreMapProps = {
  markers: MapMarker[];
  selectedId: string | null;
  onSelect: (id: string) => void;
  /** Callback optionnel : zoomer sur un marqueur spécifique depuis l'extérieur */
  zoomToId?: string | null;
  /** Reset le zoomToId après traitement */
  onZoomToComplete?: () => void;
};

const INITIAL_CENTER: [number, number] = [15, 25]; // centre Afrique/Sahel
const INITIAL_ZOOM = 2.5;
const MIN_ZOOM = 1;
const MAX_ZOOM = 18;
const SELECTED_ZOOM = 14;

// Seuils de zoom pour l'affichage des labels et clusters
const LABEL_MIN_ZOOM = 4;
const CLUSTER_MAX_ZOOM = 13;
const CLUSTER_RADIUS = 55;

// Couche source ID
const SOURCE_ID = 'events-source';
const CLUSTER_LAYER_ID = 'events-clusters';
const CLUSTER_COUNT_LAYER_ID = 'events-cluster-count';
const UNCLUSTERED_LAYER_ID = 'events-unclustered';
const LABEL_LAYER_ID = 'events-labels';

// =========================================================================
const MapLibreMap: React.FC<MapLibreMapProps> = ({
  markers,
  selectedId,
  onSelect,
  zoomToId,
  onZoomToComplete,
}) => {
  const mapContainerRef = React.useRef<HTMLDivElement | null>(null);
  const mapRef = React.useRef<maplibregl.Map | null>(null);
  const popupRef = React.useRef<maplibregl.Popup | null>(null);
  const [styleId, setStyleId] = React.useState<MapStyleId>('osm-standard');
  const [styleTransitioning, setStyleTransitioning] = React.useState(false);
  const isMapReadyRef = React.useRef(false);
  const hoveredIdRef = React.useRef<string | null>(null);
  const styleTransitionTimerRef = React.useRef<ReturnType<typeof setTimeout> | null>(null);

  // -- Initialisation de la carte -----------------------------------------
  React.useEffect(() => {
    if (!mapContainerRef.current || mapRef.current) return;

    try {
      const map = new maplibregl.Map({
        container: mapContainerRef.current,
        style: STYLE_URLS[styleId],
        center: INITIAL_CENTER,
        zoom: INITIAL_ZOOM,
        minZoom: MIN_ZOOM,
        maxZoom: MAX_ZOOM,
        attributionControl: {},
        pitchWithRotate: true,
      });

      map.addControl(new maplibregl.NavigationControl({ showCompass: true, visualizePitch: true }), 'top-right');
      map.addControl(new maplibregl.ScaleControl({ unit: 'metric' }), 'bottom-left');

      // -- Chargement des couches après le style ----------------------------
      map.once('load', () => {
        addEventSourceAndLayers(map);
        bindEventHandlers(map);
        isMapReadyRef.current = true;
      });

      mapRef.current = map;

      return () => {
        popupRef.current?.remove();
        map.remove();
        mapRef.current = null;
        isMapReadyRef.current = false;
      };
    } catch (webglError) {
      console.warn('MapLibre GL JS: WebGL indisponible, carte désactivée', webglError);
    }
    // eslint-disable-next-line react-compiler/react-compiler, react-hooks/exhaustive-deps
  }, []);

  // -- Fonction helper : ajoute la source GeoJSON + les 4 couches ---------
  const addEventSourceAndLayers = React.useCallback((map: maplibregl.Map) => {
    // Source GeoJSON avec clustering
    map.addSource(SOURCE_ID, {
      type: 'geojson',
      data: markersToGeoJSON(markers),
      cluster: true,
      clusterMaxZoom: CLUSTER_MAX_ZOOM,
      clusterRadius: CLUSTER_RADIUS,
    });

    // Couche clusters (cercles agrégés)
    map.addLayer({
      id: CLUSTER_LAYER_ID,
      type: 'circle',
      source: SOURCE_ID,
      filter: ['has', 'point_count'],
      paint: {
        'circle-color': [
          'step',
          ['get', 'point_count'],
          'rgba(74, 144, 217, 0.7)',
          5, 'rgba(245, 158, 11, 0.75)',
          15, 'rgba(239, 68, 68, 0.8)',
        ],
        'circle-radius': [
          'step',
          ['get', 'point_count'],
          22,
          5, 30,
          15, 38,
        ],
        'circle-stroke-width': 2,
        'circle-stroke-color': '#ffffff',
        'circle-opacity': 0.9,
      },
    });

    // Compteur texte sur les clusters
    map.addLayer({
      id: CLUSTER_COUNT_LAYER_ID,
      type: 'symbol',
      source: SOURCE_ID,
      filter: ['has', 'point_count'],
      layout: {
        'text-field': '{point_count_abbreviated}',
        'text-size': 13,
        'text-font': ['Open Sans Bold', 'Arial Unicode MS Bold'],
      },
      paint: {
        'text-color': '#ffffff',
        'text-halo-color': 'rgba(0,0,0,0.5)',
        'text-halo-width': 1.5,
      },
    });

    // Couche points individuels (cercles)
    map.addLayer({
      id: UNCLUSTERED_LAYER_ID,
      type: 'circle',
      source: SOURCE_ID,
      filter: ['!', ['has', 'point_count']],
      paint: {
        'circle-color': ['get', 'color'],
        'circle-radius': 8,
        'circle-stroke-width': 1.5,
        'circle-stroke-color': '#0a0a1a',
        'circle-opacity': 0.92,
      },
    });

    // Couche labels (noms des lieux)
    map.addLayer({
      id: LABEL_LAYER_ID,
      type: 'symbol',
      source: SOURCE_ID,
      filter: ['!', ['has', 'point_count']],
      minzoom: LABEL_MIN_ZOOM,
      layout: {
        'text-field': ['get', 'locationLabel'],
        'text-size': 10,
        'text-offset': [0, 1.6],
        'text-anchor': 'top',
        'text-font': ['Open Sans Regular', 'Arial Unicode MS Regular'],
        'text-allow-overlap': false,
        'text-optional': true,
      },
      paint: {
        'text-color': '#ffffff',
        'text-halo-color': 'rgba(5,16,28,0.85)',
        'text-halo-width': 1.8,
        'text-opacity': 0.9,
      },
    });
  }, [markers]);

  // -- Fonction helper : attache les event handlers (clic, survol) --------
  // Note: setStyle() supprime toutes les couches, donc les anciens handlers
  // liés aux couches sont automatiquement invalidés. Pas besoin de map.off().
  const bindEventHandlers = React.useCallback((map: maplibregl.Map) => {
    // -- Gestion des clics sur les clusters --------------------------------
    map.on('click', CLUSTER_LAYER_ID, async (e) => {
      const features = map.queryRenderedFeatures(e.point, {
        layers: [CLUSTER_LAYER_ID],
      });
      if (!features.length) return;
      const feature = features[0];
      const clusterId = feature.properties?.cluster_id as number;
      const source = map.getSource(SOURCE_ID) as maplibregl.GeoJSONSource;
      const zoom = await source.getClusterExpansionZoom(clusterId);
      map.easeTo({
        center: (feature.geometry as GeoJSON.Point).coordinates as [number, number],
        zoom: Math.min(zoom + 0.5, MAX_ZOOM),
        duration: 800,
      });
    });

    // -- Gestion des clics sur les points individuels -----------------------
    map.on('click', UNCLUSTERED_LAYER_ID, (e) => {
      const features = map.queryRenderedFeatures(e.point, {
        layers: [UNCLUSTERED_LAYER_ID],
      });
      if (!features.length) return;
      const featureId = features[0].properties?.id as string;
      if (featureId) onSelect(featureId);
    });

    // -- Survol : popup et curseur pointer ---------------------------------
    map.on('mouseenter', UNCLUSTERED_LAYER_ID, (e) => {
      map.getCanvas().style.cursor = 'pointer';
      const features = map.queryRenderedFeatures(e.point, {
        layers: [UNCLUSTERED_LAYER_ID],
      });
      if (!features.length) return;
      const props = features[0].properties;
      if (!props) return;
      const featId = props.id as string;
      if (featId === hoveredIdRef.current) return;
      hoveredIdRef.current = featId;

      popupRef.current?.remove();
      const coordinates = (features[0].geometry as GeoJSON.Point).coordinates.slice() as [number, number];
      const title = (props.title as string) || '';
      const locationLabel = (props.locationLabel as string) || '';
      const popupContent = `<div style="font-size:0.82rem;max-width:200px"><strong>${escapeHtml(title)}</strong>${locationLabel ? `<br/><small style="opacity:0.75">${escapeHtml(locationLabel)}</small>` : ''}</div>`;

      while (Math.abs(e.lngLat.lng - coordinates[0]) > 180) {
        coordinates[0] += e.lngLat.lng > coordinates[0] ? 360 : -360;
      }
      popupRef.current = new maplibregl.Popup({
        closeButton: false,
        closeOnClick: false,
        offset: 14,
        className: 'aegis-event-popup',
      })
        .setLngLat(coordinates)
        .setHTML(popupContent)
        .addTo(map);
    });

    map.on('mouseleave', UNCLUSTERED_LAYER_ID, () => {
      map.getCanvas().style.cursor = '';
      hoveredIdRef.current = null;
      popupRef.current?.remove();
      popupRef.current = null;
    });
  }, [onSelect]);

  // -- Changement de style de fond de carte -------------------------------
  const changeStyle = React.useCallback((newStyle: MapStyleId) => {
    setStyleId(newStyle);
    setStyleTransitioning(true);
    const map = mapRef.current;
    if (!map) return;

    // Nettoie le timer précédent si l'utilisateur change rapidement de style
    if (styleTransitionTimerRef.current) {
      clearTimeout(styleTransitionTimerRef.current);
      styleTransitionTimerRef.current = null;
    }

    map.setStyle(STYLE_URLS[newStyle]);
    map.once('style.load', () => {
      addEventSourceAndLayers(map);
      bindEventHandlers(map);
      isMapReadyRef.current = true;

      // Force le vidage du cache de textures GPU et le redimensionnement
      // pour éliminer les artefacts visuels du style précédent
      map.resize();
      map.triggerRepaint();

      // Délai de stabilisation pour laisser les premières tuiles se charger
      // avant de retirer l'overlay de transition
      styleTransitionTimerRef.current = setTimeout(() => {
        setStyleTransitioning(false);
        styleTransitionTimerRef.current = null;
      }, 250);
    });
  }, [addEventSourceAndLayers, bindEventHandlers]);

  // -- Nettoyage du timer de transition au démontage ----------------------
  React.useEffect(() => {
    return () => {
      if (styleTransitionTimerRef.current) {
        clearTimeout(styleTransitionTimerRef.current);
        styleTransitionTimerRef.current = null;
      }
    };
  }, []);

  // -- Mise à jour des données GeoJSON dans la source ---------------------
  React.useEffect(() => {
    const map = mapRef.current;
    if (!map || !isMapReadyRef.current) return;
    const source = map.getSource(SOURCE_ID) as maplibregl.GeoJSONSource | undefined;
    if (!source) return;
    source.setData(markersToGeoJSON(markers));
  }, [markers]);

  // -- Mise à jour de la sélection (taille/contour du cercle) -------------
  React.useEffect(() => {
    const map = mapRef.current;
    if (!map || !isMapReadyRef.current) return;
    const layer = map.getLayer(UNCLUSTERED_LAYER_ID);
    if (!layer) return;

    // Met à jour le filtre de couleur + taille pour le point sélectionné
    if (selectedId) {
      map.setPaintProperty(UNCLUSTERED_LAYER_ID, 'circle-radius', [
        'case',
        ['==', ['get', 'id'], selectedId],
        12,
        8,
      ]);
      map.setPaintProperty(UNCLUSTERED_LAYER_ID, 'circle-stroke-width', [
        'case',
        ['==', ['get', 'id'], selectedId],
        3,
        1.5,
      ]);
      map.setPaintProperty(UNCLUSTERED_LAYER_ID, 'circle-stroke-color', [
        'case',
        ['==', ['get', 'id'], selectedId],
        '#ffffff',
        '#0a0a1a',
      ]);
    }
  }, [selectedId]);

  // -- Zoom externe sur un marqueur spécifique (depuis le panneau détail) --
  React.useEffect(() => {
    const map = mapRef.current;
    if (!map || !zoomToId) return;
    const target = markers.find(m => m.id === zoomToId);
    if (!target) {
      onZoomToComplete?.();
      return;
    }
    map.flyTo({
      center: [target.lon, target.lat],
      zoom: SELECTED_ZOOM,
      duration: 1800,
    });
    // Signal completion after animation
    const timer = setTimeout(() => onZoomToComplete?.(), 1900);
    return () => clearTimeout(timer);
  }, [zoomToId, markers, onZoomToComplete]);

  // -- Centrage sur la sélection (interne) --------------------------------
  React.useEffect(() => {
    const map = mapRef.current;
    if (!map || !selectedId || zoomToId) return; // éviter conflit avec zoom externe
    const selected = markers.find(m => m.id === selectedId);
    if (!selected) return;
    map.flyTo({ center: [selected.lon, selected.lat], zoom: SELECTED_ZOOM, duration: 1800 });
  }, [selectedId, markers, zoomToId]);

  // -- Vue globale --------------------------------------------------------
  const showGlobalView = React.useCallback(() => {
    mapRef.current?.flyTo({ center: INITIAL_CENTER, zoom: INITIAL_ZOOM, duration: 1500 });
  }, []);

  // -- Reset tilt/bearing -------------------------------------------------
  const resetTilt = React.useCallback(() => {
    mapRef.current?.easeTo({ pitch: 0, bearing: 0, duration: 800 });
  }, []);

  // -----------------------------------------------------------------
  return (
    <div style={{ position: 'relative' }}>
      {/* Contrôles custom */}
      <div style={{
        display: 'flex', alignItems: 'center', gap: '0.5rem', flexWrap: 'wrap',
        marginBottom: '0.75rem', background: 'var(--bg-secondary)', border: '1px solid #2a2a3e',
        borderRadius: '8px', padding: '0.5rem 0.75rem',
      }}>
        {(['osm-standard', 'satellite-imagery', 'topographic-terrain'] as MapStyleId[]).map(id => (
          <button
            key={id}
            type="button"
            onClick={() => changeStyle(id)}
            disabled={styleId === id}
            aria-pressed={styleId === id}
            title={STYLE_LABELS[id]}
            style={{
              background: styleId === id ? 'rgba(74,144,217,0.25)' : 'transparent',
              color: styleId === id ? '#ffffff' : 'var(--text-secondary)',
              border: styleId === id ? '1px solid var(--accent)' : '1px solid #2a2a3e',
              borderRadius: '6px', padding: '0.35rem 0.65rem', cursor: 'pointer',
              fontSize: '0.82rem', whiteSpace: 'nowrap',
              opacity: styleId === id ? 1 : 0.75,
            }}
          >
            {STYLE_LABELS[id]}
          </button>
        ))}

        <div style={{ flex: 1 }} />

        <span style={{ color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
          👆 Molette = zoom • Clic-droit = rotation/inclinaison
        </span>

        <button
          type="button"
          onClick={showGlobalView}
          title="Revenir à la vue d'ensemble (monde)"
          style={{
            ...controlButtonStyle,
            background: 'rgba(74,144,217,0.18)',
            border: '1px solid var(--accent)',
            color: '#ffffff',
            fontWeight: 'bold',
            padding: '0.4rem 0.8rem',
          }}
        >
          🌍 Vue globale
        </button>
        <button type="button" onClick={resetTilt}
          style={controlButtonStyle}>
          🧭 Nord ↑
        </button>
      </div>

      {/* Container carte avec overlay de transition de style */}
      <div style={{ position: 'relative' }}>
        <div
          ref={mapContainerRef}
          data-testid="maplibre-map-viewport"
          style={{
            width: '100%', minHeight: '420px', borderRadius: '8px',
            border: '1px solid #30445a', overflow: 'hidden',
            ...(styleTransitioning ? { opacity: 0.5, filter: 'blur(2px)' } : {}),
          }}
        />

        {styleTransitioning && (
          <div
            data-testid="style-transition-overlay"
            style={{
              position: 'absolute', inset: 0,
              display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center',
              background: 'rgba(10, 12, 24, 0.55)', borderRadius: '8px',
              color: '#ffffff', fontSize: '0.92rem',
              zIndex: 10, pointerEvents: 'none',
            }}
          >
            <span style={{ marginBottom: '0.35rem', opacity: 0.9 }}>
              Chargement du fond de carte…
            </span>
            <span style={{ fontSize: '0.78rem', opacity: 0.65 }}>
              {STYLE_LABELS[styleId]}
            </span>
          </div>
        )}
      </div>

      <p style={{ margin: '0.6rem 0 0', color: 'var(--text-secondary)', fontSize: '0.82rem' }}>
        Astuce : maintenir Ctrl + glisser pour incliner la vue 3D. Le relief s'accentue avec le zoom.
        {' '}· Cercles numérotés = groupes d'événements (cliquez pour zoomer).
      </p>
    </div>
  );
};

// -- Helper: conversion MapMarker[] → GeoJSON FeatureCollection -----------
function markersToGeoJSON(markers: MapMarker[]): GeoJSON.FeatureCollection {
  return {
    type: 'FeatureCollection',
    features: markers.map(m => ({
      type: 'Feature' as const,
      geometry: {
        type: 'Point' as const,
        coordinates: [m.lon, m.lat],
      },
      properties: {
        id: m.id,
        title: m.title,
        color: m.color,
        locationLabel: m.locationLabel ?? m.title,
      },
    })),
  };
}

// -- Helper: échappement HTML pour les popups ------------------------------
function escapeHtml(text: string): string {
  return text
    .replace(/&/g, '&')
    .replace(/</g, '<')
    .replace(/>/g, '>')
    .replace(/"/g, '"');
}

const controlButtonStyle: React.CSSProperties = {
  background: 'transparent',
  color: 'var(--text-secondary)',
  border: '1px solid #30445a',
  borderRadius: '6px',
  padding: '0.35rem 0.65rem',
  cursor: 'pointer',
  fontSize: '0.82rem',
  whiteSpace: 'nowrap',
};

export default MapLibreMap;
export { STYLE_LABELS, STYLE_URLS };