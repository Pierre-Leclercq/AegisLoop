import React from 'react';

export type SvgViewBox = { x: number; y: number; width: number; height: number };
export type MapDetailLevel = 'world' | 'regional' | 'local';
export type MapLayerType = 'land' | 'countries' | 'borders' | 'rivers' | 'lakes' | 'labels';
export type Position = [number, number, ...number[]];
export type GeoJsonGeometry =
  | { type: 'LineString'; coordinates: Position[] }
  | { type: 'MultiLineString'; coordinates: Position[][] }
  | { type: 'Polygon'; coordinates: Position[][] }
  | { type: 'MultiPolygon'; coordinates: Position[][][] };
export type GeoJsonFeature = { type: 'Feature'; bbox?: number[]; properties?: Record<string, unknown>; geometry: GeoJsonGeometry | null };
export type GeoJsonFeatureCollection = { type: 'FeatureCollection'; features: GeoJsonFeature[] };
export type MapLayerDescriptor = {
  id: string;
  level: MapDetailLevel;
  type: MapLayerType;
  file: string;
  bbox: [number, number, number, number];
  minZoom: number;
  maxZoom: number;
  priority: number;
  label: string;
  scaleLabel: string;
  attribution: string;
};
export type MapDataManifest = { version: string; attribution: string; layers: MapLayerDescriptor[] };
export type RenderableMapFeature = GeoJsonFeature & { mapBBox: SvgViewBox; path: string; labelPoint: { x: number; y: number }; label?: string; labelPriority: number };
export type LoadedMapLayer = MapLayerDescriptor & { features: RenderableMapFeature[] };
export type MapLayerSummary = { level: MapDetailLevel; label: string; scaleLabel: string; sourcePath: string };
export type LabelRect = { x: number; y: number; width: number; height: number };
export type LabelCandidate = { id: string; text: string; x: number; y: number; priority: number; fontPx: number; fill: string; stroke: string; anchor?: 'start' | 'middle' | 'end'; weight?: number; kind: 'country' | 'event'; line2?: string };
export type PlacedLabel = LabelCandidate & { rect: LabelRect; fontSize: number; line2FontSize?: number };

export const MAP_WIDTH = 1200;
export const MAP_HEIGHT = 650;
export const WORLD_BOUNDS: SvgViewBox = { x: 0, y: 0, width: MAP_WIDTH, height: MAP_HEIGHT };
export const GLOBAL_VIEWBOX: SvgViewBox = { ...WORLD_BOUNDS };
export const VIEWBOX_ASPECT_RATIO = MAP_WIDTH / MAP_HEIGHT;
export const REGIONAL_LOD_MIN_ZOOM = 2;
export const LOCAL_LOD_MIN_ZOOM = 8;
const MAP_ASSET_ROOT = 'map-data';

function layer(id: string, level: MapDetailLevel, type: MapLayerType, file: string, bbox: [number, number, number, number], minZoom: number, maxZoom: number, priority: number, label: string, scaleLabel: string): MapLayerDescriptor {
  return { id, level, type, file, bbox, minZoom, maxZoom, priority, label, scaleLabel, attribution: 'Natural Earth — public domain' };
}

export const DEFAULT_MAP_LAYER_MANIFEST: MapDataManifest = {
  version: 'phase6g-viewport-map-rendering',
  attribution: 'Natural Earth — public domain',
  layers: [
    layer('world-land-110m', 'world', 'land', 'natural-earth-110m/ne_110m_land.geojson', [-180, -90, 180, 90], 1, 1.99, 10, 'Monde terres 1:110m', 'Natural Earth 1:110m'),
    layer('world-countries-110m', 'world', 'countries', 'natural-earth-110m/ne_110m_admin_0_countries.geojson', [-180, -90, 180, 90], 1, 1.99, 20, 'Monde frontières 1:110m', 'Natural Earth 1:110m'),
    layer('world-lakes-110m', 'world', 'lakes', 'natural-earth-110m/ne_110m_lakes.geojson', [-180, -90, 180, 90], 1, 1.99, 30, 'Monde lacs 1:110m', 'Natural Earth 1:110m'),
    layer('world-rivers-110m', 'world', 'rivers', 'natural-earth-110m/ne_110m_rivers_lake_centerlines.geojson', [-180, -90, 180, 90], 1, 1.99, 40, 'Monde rivières 1:110m', 'Natural Earth 1:110m'),
    layer('regional-land-50m', 'regional', 'land', 'natural-earth-50m/ne_50m_land.geojson', [-180, -90, 180, 90], 2, 7.99, 10, 'Régional terres 1:50m', 'Natural Earth 1:50m'),
    layer('regional-countries-50m', 'regional', 'countries', 'natural-earth-50m/ne_50m_admin_0_countries.geojson', [-180, -90, 180, 90], 2, 7.99, 20, 'Régional pays 1:50m', 'Natural Earth 1:50m'),
    layer('regional-lakes-50m', 'regional', 'lakes', 'natural-earth-50m/ne_50m_lakes.geojson', [-180, -90, 180, 90], 4, 12, 30, 'Régional lacs 1:50m', 'Natural Earth 1:50m'),
    layer('regional-rivers-50m', 'regional', 'rivers', 'natural-earth-50m/ne_50m_rivers_lake_centerlines.geojson', [-180, -90, 180, 90], 4, 12, 40, 'Régional rivières 1:50m', 'Natural Earth 1:50m'),
    layer('local-land-seed-10m', 'local', 'land', 'natural-earth-10m/ne_10m_land.geojson', [-18, 0, 56, 25], 8, 30, 10, 'Local seed terres 1:10m', 'Natural Earth 1:10m extrait Sahel/Aden'),
    layer('local-countries-seed-10m', 'local', 'countries', 'natural-earth-10m/ne_10m_admin_0_countries.geojson', [-18, 0, 56, 25], 8, 30, 20, 'Local seed pays 1:10m', 'Natural Earth 1:10m extrait Sahel/Aden'),
    layer('local-lakes-seed-10m', 'local', 'lakes', 'natural-earth-10m/ne_10m_lakes.geojson', [-18, 0, 56, 25], 9, 30, 30, 'Local seed lacs 1:10m', 'Natural Earth 1:10m extrait Sahel/Aden'),
    layer('local-rivers-seed-10m', 'local', 'rivers', 'natural-earth-10m/ne_10m_rivers_lake_centerlines.geojson', [-18, 0, 56, 25], 9, 30, 40, 'Local seed rivières 1:10m', 'Natural Earth 1:10m extrait Sahel/Aden'),
  ],
};

const mapLayerFileCache = new Map<string, Promise<RenderableMapFeature[]>>();

export function clearMapDataCacheForTests(): void { mapLayerFileCache.clear(); }

export function selectMapDetailLevel(viewBox: SvgViewBox): MapDetailLevel {
  const zoomLevel = getZoomLevel(viewBox);
  if (zoomLevel < REGIONAL_LOD_MIN_ZOOM) return 'world';
  if (zoomLevel < LOCAL_LOD_MIN_ZOOM) return 'regional';
  return 'local';
}

export function selectVisibleMapLayers(viewBox: SvgViewBox, zoomLevel: number, manifest: MapDataManifest | MapLayerDescriptor[]): MapLayerDescriptor[] {
  const layers = (Array.isArray(manifest) ? manifest : manifest.layers).filter(layer => intersectsViewBox(projectGeoBBox(layer.bbox), viewBox));
  const exact = layers.filter(layer => zoomLevel >= layer.minZoom && zoomLevel <= layer.maxZoom);
  if (exact.length > 0) return sortAndDedupeLayers(exact);

  const requested = selectMapDetailLevel(viewBox);
  const fallbackLevels: MapDetailLevel[] = requested === 'local' ? ['regional', 'world'] : requested === 'regional' ? ['world'] : [];
  for (const level of fallbackLevels) {
    const fallback = layers.filter(layer => layer.level === level);
    if (fallback.length > 0) return sortAndDedupeLayers(fallback);
  }
  return sortAndDedupeLayers(layers.filter(layer => layer.level === 'world'));
}

export function filterFeaturesForView<T extends GeoJsonFeature | RenderableMapFeature>(features: T[], viewBox: SvgViewBox, padding = 0): T[] {
  const padded = expandViewBox(viewBox, padding);
  return features.filter(feature => intersectsViewBox(getFeatureMapBBox(feature), padded));
}

export function shouldShowCountryLabels(zoomLevel: number): boolean { return zoomLevel >= 1.8 && zoomLevel <= 16; }
export function shouldShowEventLabels(zoomLevel: number): boolean { return zoomLevel >= 1.45; }

export function limitLabelsByViewportAndPriority(candidates: LabelCandidate[], viewBox: SvgViewBox, maxCount: number, reservedRects: LabelRect[] = []): PlacedLabel[] {
  const placed: PlacedLabel[] = [];
  const occupied = [...reservedRects];
  for (const candidate of [...candidates].sort((left, right) => right.priority - left.priority || left.text.localeCompare(right.text))) {
    if (placed.length >= maxCount) break;
    const fontSize = screenToMapUnits(candidate.fontPx, viewBox);
    const line2FontSize = candidate.line2 ? screenToMapUnits(Math.max(8, candidate.fontPx - 2), viewBox) : undefined;
    const width = screenToMapUnits(Math.max(22, candidate.text.length * candidate.fontPx * 0.58), viewBox);
    const height = screenToMapUnits(candidate.line2 ? candidate.fontPx * 2.4 : candidate.fontPx * 1.35, viewBox);
    const x = candidate.anchor === 'middle' ? candidate.x - width / 2 : candidate.anchor === 'end' ? candidate.x - width : candidate.x;
    const y = candidate.y - height;
    const rect = { x, y, width, height };
    if (!intersectsViewBox(rect, expandViewBox(viewBox, 0.02))) continue;
    if (occupied.some(existing => intersectsRect(existing, rect))) continue;
    occupied.push(rect);
    placed.push({ ...candidate, rect, fontSize, line2FontSize });
  }
  return placed;
}

export function useViewportMapLayers(viewBox: SvgViewBox): { descriptors: MapLayerDescriptor[]; renderedLayers: LoadedMapLayer[]; failedFiles: Set<string>; manifestError: string | null } {
  const [manifest, setManifest] = React.useState<MapDataManifest>(DEFAULT_MAP_LAYER_MANIFEST);
  const [manifestError, setManifestError] = React.useState<string | null>(null);
  const [loadedFiles, setLoadedFiles] = React.useState<Record<string, RenderableMapFeature[]>>({});
  const [failedFiles, setFailedFiles] = React.useState<Set<string>>(() => new Set());

  React.useEffect(() => {
    let cancelled = false;
    void fetch(mapAssetUrl('manifest.json'))
      .then(response => response.ok ? response.json() : Promise.reject(new Error(`Manifest carte indisponible (${response.status})`)))
      .then(json => {
        if (!cancelled && isMapDataManifest(json)) {
          setManifest(json);
          setManifestError(null);
        } else if (!cancelled) {
          setManifestError('Manifest carte invalide, manifest embarqué utilisé');
        }
      })
      .catch(error => { if (!cancelled) setManifestError(error instanceof Error ? error.message : 'Manifest carte indisponible'); });
    return () => { cancelled = true; };
  }, []);

  const availableManifest = React.useMemo<MapDataManifest>(() => ({ ...manifest, layers: manifest.layers.filter(layer => !failedFiles.has(layer.file)) }), [manifest, failedFiles]);
  const descriptors = React.useMemo(() => selectVisibleMapLayers(viewBox, getZoomLevel(viewBox), availableManifest), [viewBox, availableManifest]);
  const descriptorFiles = React.useMemo(() => [...new Set(descriptors.map(descriptor => descriptor.file))], [descriptors]);
  const descriptorFileKey = descriptorFiles.join('|');

  React.useEffect(() => {
    let cancelled = false;
    for (const file of descriptorFiles) {
      if (loadedFiles[file] || failedFiles.has(file)) continue;
      void loadMapLayerFile(file)
        .then(features => { if (!cancelled) setLoadedFiles(previous => previous[file] ? previous : { ...previous, [file]: features }); })
        .catch(() => { if (!cancelled) setFailedFiles(previous => new Set(previous).add(file)); });
    }
    return () => { cancelled = true; };
  }, [descriptorFileKey, descriptorFiles, failedFiles, loadedFiles]);

  const renderedLayers = React.useMemo<LoadedMapLayer[]>(() => descriptors.map(descriptor => ({
    ...descriptor,
    features: filterFeaturesForView(loadedFiles[descriptor.file] ?? [], viewBox, 0.16) as RenderableMapFeature[],
  })), [descriptors, loadedFiles, viewBox]);

  return { descriptors, renderedLayers, failedFiles, manifestError };
}

export function buildCountryLabels(layers: LoadedMapLayer[], viewBox: SvgViewBox, reservedRects: LabelRect[]): PlacedLabel[] {
  const zoom = getZoomLevel(viewBox);
  if (!shouldShowCountryLabels(zoom)) return [];
  const countryFeatures = layers.filter(layer => layer.type === 'countries').flatMap(layer => layer.features);
  const maxLabels = zoom < 2.5 ? 3 : zoom < 5 ? 8 : zoom < 9 ? 10 : 4;
  const fontPx = zoom < 3 ? 10 : zoom < 8 ? 9 : 8;
  const candidates: LabelCandidate[] = countryFeatures
    .map((feature, index) => ({
      id: `country-${index}-${feature.label ?? index}`,
      text: feature.label ?? '',
      x: feature.labelPoint.x,
      y: feature.labelPoint.y,
      priority: feature.labelPriority,
      fontPx,
      fill: 'rgba(235,245,255,0.58)',
      stroke: 'rgba(5,16,28,0.86)',
      anchor: 'middle' as const,
      weight: 600,
      kind: 'country' as const,
    }))
    .filter(label => label.text.length > 0 && containsPoint(expandViewBox(viewBox, 0.04), { x: label.x, y: label.y }));
  return limitLabelsByViewportAndPriority(candidates, viewBox, maxLabels, reservedRects);
}

export function summarizeMapLayers(descriptors: MapLayerDescriptor[]): MapLayerSummary {
  const level = descriptors.reduce<MapDetailLevel>((current, descriptor) => levelRank(descriptor.level) > levelRank(current) ? descriptor.level : current, descriptors[0]?.level ?? 'world');
  const scaleLabels = [...new Set(descriptors.filter(descriptor => descriptor.level === level).map(descriptor => descriptor.scaleLabel))];
  const fileRoots = [...new Set(descriptors.map(descriptor => descriptor.file.split('/')[0]))];
  return {
    level,
    label: level === 'world' ? 'Fond carte : Monde 1:110m' : level === 'regional' ? 'Fond carte : Régional 1:50m viewport' : 'Fond carte : Local 1:10m extrait seed viewport',
    scaleLabel: scaleLabels.join(' + ') || (level === 'world' ? 'Natural Earth 1:110m' : level === 'regional' ? 'Natural Earth 1:50m' : 'Natural Earth 1:10m'),
    sourcePath: `${MAP_ASSET_ROOT}/${fileRoots.join(', ')}`,
  };
}

export function getZoomLevel(viewBox: SvgViewBox): number { return GLOBAL_VIEWBOX.width / viewBox.width; }
export function projectLonLat(lon: number, lat: number): { x: number; y: number } {
  const normalizedLon = Math.max(-180, Math.min(180, lon));
  const normalizedLat = Math.max(-90, Math.min(90, lat));
  return { x: ((normalizedLon + 180) / 360) * MAP_WIDTH, y: ((90 - normalizedLat) / 180) * MAP_HEIGHT };
}
export function containsPoint(viewBox: SvgViewBox, point: { x: number; y: number }): boolean {
  return point.x >= viewBox.x && point.x <= viewBox.x + viewBox.width && point.y >= viewBox.y && point.y <= viewBox.y + viewBox.height;
}
export function expandViewBox(viewBox: SvgViewBox, paddingRatio: number): SvgViewBox {
  const padX = viewBox.width * paddingRatio;
  const padY = viewBox.height * paddingRatio;
  return { x: viewBox.x - padX, y: viewBox.y - padY, width: viewBox.width + padX * 2, height: viewBox.height + padY * 2 };
}
export function intersectsViewBox(left: SvgViewBox, right: SvgViewBox): boolean {
  return left.x <= right.x + right.width && left.x + left.width >= right.x && left.y <= right.y + right.height && left.y + left.height >= right.y;
}
export function screenToMapUnits(px: number, viewBox: SvgViewBox): number { return (px / 1000) * viewBox.width; }

function sortAndDedupeLayers(layers: MapLayerDescriptor[]): MapLayerDescriptor[] {
  const seen = new Set<string>();
  return [...layers]
    .sort((left, right) => levelRank(left.level) - levelRank(right.level) || left.priority - right.priority || left.id.localeCompare(right.id))
    .filter(layer => {
      const key = `${layer.type}:${layer.file}`;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    });
}
function levelRank(level: MapDetailLevel): number { return level === 'world' ? 1 : level === 'regional' ? 2 : 3; }
function projectGeoBBox(bbox: [number, number, number, number] | number[]): SvgViewBox {
  const [west, south, east, north] = bbox;
  const topLeft = projectLonLat(west, north);
  const bottomRight = projectLonLat(east, south);
  return { x: topLeft.x, y: topLeft.y, width: Math.max(0, bottomRight.x - topLeft.x), height: Math.max(0, bottomRight.y - topLeft.y) };
}
function featureToPath(feature: GeoJsonFeature): string { return feature.geometry ? geometryToPath(feature.geometry) : ''; }
function geometryToPath(geometry: GeoJsonGeometry): string {
  switch (geometry.type) {
    case 'LineString': return lineToPath(geometry.coordinates);
    case 'MultiLineString': return geometry.coordinates.map(lineToPath).join(' ');
    case 'Polygon': return polygonToPath(geometry.coordinates);
    case 'MultiPolygon': return geometry.coordinates.map(polygonToPath).join(' ');
    default: return '';
  }
}
function polygonToPath(rings: Position[][]): string { return rings.map(ring => `${lineToPath(ring)} Z`).join(' '); }
function lineToPath(points: Position[]): string {
  return points.map(([lon, lat], index) => {
    const point = projectLonLat(lon, lat);
    return `${index === 0 ? 'M' : 'L'}${Number(point.x.toFixed(2)).toString()} ${Number(point.y.toFixed(2)).toString()}`;
  }).join(' ');
}
function toRenderableFeature(feature: GeoJsonFeature): RenderableMapFeature | null {
  const mapBBox = getFeatureMapBBox(feature);
  const path = featureToPath(feature);
  if (!path || mapBBox.width <= 0 || mapBBox.height <= 0) return null;
  const labelPoint = { x: mapBBox.x + mapBBox.width / 2, y: mapBBox.y + mapBBox.height / 2 };
  return { ...feature, mapBBox, path, labelPoint, label: getCountryLabel(feature), labelPriority: getCountryLabelPriority(feature) };
}
function getFeatureMapBBox(feature: GeoJsonFeature | RenderableMapFeature): SvgViewBox {
  if ('mapBBox' in feature) return feature.mapBBox;
  if (feature.bbox && feature.bbox.length >= 4) return projectGeoBBox(feature.bbox);
  if (!feature.geometry) return { x: 0, y: 0, width: 0, height: 0 };
  const points: Array<{ x: number; y: number }> = [];
  forEachPosition(feature.geometry.coordinates, position => points.push(projectLonLat(position[0], position[1])));
  if (points.length === 0) return { x: 0, y: 0, width: 0, height: 0 };
  const minX = Math.min(...points.map(point => point.x));
  const maxX = Math.max(...points.map(point => point.x));
  const minY = Math.min(...points.map(point => point.y));
  const maxY = Math.max(...points.map(point => point.y));
  return { x: minX, y: minY, width: maxX - minX, height: maxY - minY };
}
function forEachPosition(coordinates: unknown, visit: (position: Position) => void): void {
  if (!Array.isArray(coordinates) || coordinates.length === 0) return;
  if (typeof coordinates[0] === 'number') { visit(coordinates as Position); return; }
  for (const child of coordinates) forEachPosition(child, visit);
}
function getCountryLabel(feature: GeoJsonFeature): string | undefined {
  const props = feature.properties ?? {};
  const value = props.NAME_FR ?? props.NAME ?? props.ADMIN ?? props.NAME_LONG;
  return typeof value === 'string' ? value : undefined;
}
function getCountryLabelPriority(feature: GeoJsonFeature): number {
  const props = feature.properties ?? {};
  const labelRank = typeof props.LABELRANK === 'number' ? props.LABELRANK : Number(props.LABELRANK ?? 9);
  const population = typeof props.POP_EST === 'number' ? props.POP_EST : Number(props.POP_EST ?? 0);
  return Math.max(0, 120 - labelRank * 12 + Math.log10(Math.max(1, population)) * 4);
}
function loadMapLayerFile(file: string): Promise<RenderableMapFeature[]> {
  const normalizedFile = file.replace(/^\/+/, '');
  const cached = mapLayerFileCache.get(normalizedFile);
  if (cached) return cached;
  const promise = fetch(mapAssetUrl(normalizedFile)).then(async response => {
    if (!response.ok) throw new Error(`Couche cartographique indisponible ${normalizedFile} (${response.status})`);
    const json = await response.json() as GeoJsonFeatureCollection;
    if (!json || json.type !== 'FeatureCollection' || !Array.isArray(json.features)) throw new Error(`Couche cartographique invalide ${normalizedFile}`);
    return json.features.map(toRenderableFeature).filter((feature): feature is RenderableMapFeature => feature !== null);
  });
  mapLayerFileCache.set(normalizedFile, promise);
  return promise;
}
function mapAssetUrl(relativePath: string): string {
  const cleanPath = relativePath.replace(/^\/+/, '');
  const base = (import.meta.env.BASE_URL ?? '/').replace(/\/$/, '');
  const prefix = base === '' || base === '.' ? '.' : base;
  return `${prefix}/${MAP_ASSET_ROOT}/${cleanPath}`;
}
function isMapDataManifest(value: unknown): value is MapDataManifest {
  return Boolean(value && typeof value === 'object' && Array.isArray((value as MapDataManifest).layers) && (value as MapDataManifest).layers.every(layer => typeof layer.id === 'string' && Array.isArray(layer.bbox) && typeof layer.file === 'string'));
}
function intersectsRect(left: LabelRect, right: LabelRect): boolean {
  return left.x <= right.x + right.width && left.x + left.width >= right.x && left.y <= right.y + right.height && left.y + left.height >= right.y;
}