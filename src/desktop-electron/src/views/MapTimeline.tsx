import React from 'react';
import MapLibreMap, { type MapMarker } from '../components/MapLibreMap';

type MapTimelineItem = {
  eventCaseId: string;
  title: string;
  score: number;
  status: string;
  category: string;
  date: string;
  sources: string[];
  sourceTypes: string[];
  observationCount: number;
  latitude?: number | null;
  longitude?: number | null;
  locationName?: string | null;
  region?: string | null;
  country?: string | null;
  scenario?: string | null;
  scenarioLabel?: string | null;
};

type MapTimelinePayload = {
  items: MapTimelineItem[];
  totalCount: number;
  withoutCoordinatesCount: number;
  periodStart?: string | null;
  periodEnd?: string | null;
  scenarios: string[];
  sourceTypes: string[];
};

type ApiEnvelope<T> = { success: boolean; data: T; error?: string | null };
type Filters = { source: string; minScore: number; scenario: string };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';
const scoreSteps = [0, 0.25, 0.5, 0.7];


const MapTimeline: React.FC = () => {
  const [payload, setPayload] = React.useState<MapTimelinePayload | null>(null);
  const [selectedId, setSelectedId] = React.useState<string | null>(null);
  const [zoomToId, setZoomToId] = React.useState<string | null>(null);
  const [filters, setFilters] = React.useState<Filters>({ source: 'all', minScore: 0, scenario: 'all' });
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const loadData = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams();
      if (filters.source !== 'all') params.set('source', filters.source);
      if (filters.minScore > 0) params.set('minScore', filters.minScore.toString());
      if (filters.scenario !== 'all') params.set('scenario', filters.scenario);
      const query = params.toString();
      const response = await fetch(`${API_BASE_URL}/api/map-timeline${query ? `?${query}` : ''}`);
      const envelope = await response.json() as ApiEnvelope<MapTimelinePayload>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Carte + Timeline indisponible (${response.status})`);
      setPayload(envelope.data);
      setSelectedId(previous => envelope.data.items.some(item => item.eventCaseId === previous) ? previous : envelope.data.items[0]?.eventCaseId ?? null);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Erreur Carte + Timeline inconnue');
    } finally {
      setLoading(false);
    }
  }, [filters]);

  React.useEffect(() => { void loadData(); }, [loadData]);

  const items = payload?.items ?? [];
  const selected = items.find(item => item.eventCaseId === selectedId) ?? items[0] ?? null;

  return <div>
    <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', alignItems: 'start', flexWrap: 'wrap' }}>
      <div>
        <h1 style={{ color: 'var(--accent)', marginBottom: '0.5rem' }}>Carte + Timeline</h1>
        <p style={{ color: 'var(--text-secondary)', marginTop: 0 }}>Vue V1 des EventCases géolocalisés et de leur chronologie, alimentée par le seed/API local.</p>
      </div>
      <button type="button" onClick={() => void loadData()} disabled={loading} style={primaryButtonStyle(loading)}>{loading ? 'Chargement…' : 'Rafraîchir'}</button>
    </div>

    <FilterBar payload={payload} filters={filters} onChange={setFilters} />

    {loading && <LoadingState />}
    {error && <ErrorState message={error} onRetry={loadData} />}
    {!loading && !error && payload && items.length === 0 && <EmptyState />}

    {!loading && !error && payload && items.length > 0 && <>
      <SummaryBar payload={payload} />
      <div style={{ display: 'grid', gridTemplateColumns: 'minmax(420px, 2fr) minmax(280px, 1fr)', gap: '1rem', marginTop: '1rem' }}>
         <section style={panelStyle}>
          <h2 style={sectionHeadingStyle}>Carte interactive avec zoom global → terrain</h2>
          <MapLibreMap
            markers={buildMapMarkers(items, selected?.eventCaseId ?? null)}
            selectedId={selected?.eventCaseId ?? null}
            onSelect={setSelectedId}
            zoomToId={zoomToId}
            onZoomToComplete={() => setZoomToId(null)}
          />
        </section>
        <DetailPanel item={selected} onZoomTo={setZoomToId} />
      </div>
      <section style={{ ...panelStyle, marginTop: '1rem' }}>
        <h2 style={sectionHeadingStyle}>Timeline simple</h2>
        <TimelineSimple items={items} selectedId={selected?.eventCaseId ?? null} onSelect={setSelectedId} />
      </section>
    </>}
  </div>;
};

const FilterBar: React.FC<{ payload: MapTimelinePayload | null; filters: Filters; onChange: (filters: Filters) => void }> = ({ payload, filters, onChange }) => {
  const sourceTypes = payload?.sourceTypes.length ? payload.sourceTypes : ['Rss', 'Gdelt'];
  const scenarios = payload?.scenarios ?? [];
  return <section aria-label="Filtres Carte Timeline" style={{ ...panelStyle, display: 'flex', gap: '0.75rem', flexWrap: 'wrap', alignItems: 'end', marginTop: '1rem' }}>
    <label style={labelStyle}>Source
      <select aria-label="Filtrer par source" value={filters.source} onChange={event => onChange({ ...filters, source: event.target.value })} style={selectStyle}>
        <option value="all">Toutes</option>
        {sourceTypes.map(source => <option key={source} value={source}>{source}</option>)}
      </select>
    </label>
    <label style={labelStyle}>Score minimal
      <select aria-label="Filtrer par score minimal" value={filters.minScore} onChange={event => onChange({ ...filters, minScore: Number(event.target.value) })} style={selectStyle}>
        {scoreSteps.map(score => <option key={score} value={score}>{score === 0 ? 'Tous' : `≥ ${Math.round(score * 100)}%`}</option>)}
      </select>
    </label>
    <label style={labelStyle}>Scénario
      <select aria-label="Filtrer par scénario" value={filters.scenario} onChange={event => onChange({ ...filters, scenario: event.target.value })} style={selectStyle}>
        <option value="all">Tous</option>
        {scenarios.map(scenario => <option key={scenario} value={scenario}>{scenario}</option>)}
      </select>
    </label>
  </section>;
};

const SummaryBar: React.FC<{ payload: MapTimelinePayload }> = ({ payload }) => <section aria-label="Synthèse Carte Timeline" style={{ display: 'grid', gridTemplateColumns: 'repeat(4, minmax(140px, 1fr))', gap: '0.75rem', marginTop: '1rem' }}>
  <Metric label="Éléments affichés" value={payload.totalCount.toString()} />
  <Metric label="Sans coordonnées" value={payload.withoutCoordinatesCount.toString()} />
  <Metric label="Période" value={formatPeriod(payload.periodStart, payload.periodEnd)} />
  <Metric label="Sources" value={payload.sourceTypes.join(' / ') || 'n/a'} />
</section>;

const Metric: React.FC<{ label: string; value: string }> = ({ label, value }) => <div style={metricStyle}>
  <span style={{ color: 'var(--text-secondary)', fontSize: '0.82rem' }}>{label}</span>
  <strong style={{ display: 'block', marginTop: '0.35rem' }}>{value}</strong>
</div>;

function buildMapMarkers(items: MapTimelineItem[], _selectedId: string | null): MapMarker[] {
  return items.filter(hasCoordinates).map(item => ({
    id: item.eventCaseId,
    lon: item.longitude as number,
    lat: item.latitude as number,
    title: item.title,
    color: markerColor(item),
    locationLabel: buildLocationLabel(item),
  }));
}

function buildLocationLabel(item: MapTimelineItem): string | null {
  const parts = [item.locationName, item.region, item.country].filter(Boolean);
  return parts.length > 0 ? parts.join(', ') : null;
}

const TimelineSimple: React.FC<{ items: MapTimelineItem[]; selectedId: string | null; onSelect: (id: string) => void }> = ({ items, selectedId, onSelect }) => <ol aria-label="Timeline des EventCases" style={{ listStyle: 'none', display: 'grid', gap: '0.6rem', marginTop: '0.75rem' }}>
  {[...items].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime()).map(item => {
    const active = item.eventCaseId === selectedId;
    return <li key={item.eventCaseId}>
      <button type="button" onClick={() => onSelect(item.eventCaseId)} style={timelineButtonStyle(active)}>
        <span style={{ color: 'var(--text-secondary)', minWidth: '135px', textAlign: 'left' }}>{formatDate(item.date)}</span>
        <span style={{ width: '12px', height: '12px', borderRadius: '999px', background: markerColor(item), boxShadow: active ? '0 0 0 4px rgba(74,144,217,0.25)' : 'none' }} />
        <span style={{ flex: 1, textAlign: 'left' }}><strong>{item.title}</strong><br /><small style={{ color: 'var(--text-secondary)' }}>{item.locationName ?? 'Localisation textuelle indisponible'} · {item.observationCount} observations · {item.sourceTypes.join('/')}</small></span>
      </button>
    </li>;
  })}
</ol>;

const DetailPanel: React.FC<{ item: MapTimelineItem | null; onZoomTo: (id: string) => void }> = ({ item, onZoomTo }) => <aside style={panelStyle} aria-label="Détail sélection Carte Timeline">
  <h2 style={sectionHeadingStyle}>Détail sélection</h2>
  {!item ? <p style={{ color: 'var(--text-secondary)' }}>Sélectionnez un marqueur ou un item timeline.</p> : <>
    <h3 style={{ marginTop: 0 }}>{item.title}</h3>
    <dl style={definitionGridStyle}>
      <dt>Score</dt><dd>{Math.round(item.score * 100)}%</dd>
      <dt>Statut</dt><dd>{item.status}</dd>
      <dt>Catégorie</dt><dd>{item.category}</dd>
      <dt>Date</dt><dd>{formatDate(item.date)}</dd>
      <dt>Sources</dt><dd>{item.sources.join(' · ') || 'n/a'}</dd>
      <dt>Observations</dt><dd>{item.observationCount}</dd>
      <dt>Lieu</dt><dd>{[item.locationName, item.region, item.country].filter(Boolean).join(' · ') || 'Sans localisation textuelle'}</dd>
      <dt>Coordonnées</dt><dd>{hasCoordinates(item) ? `${item.latitude?.toFixed(3)}, ${item.longitude?.toFixed(3)}` : 'Sans coordonnées'}</dd>
      <dt>Scénario</dt><dd>{item.scenarioLabel ?? item.scenario ?? 'n/a'}</dd>
    </dl>
    <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', alignItems: 'center' }}>
      <a href={`/event-case?id=${item.eventCaseId}`} style={linkButtonStyle}>Ouvrir dans EventCase</a>
      {hasCoordinates(item) && (
        <button
          type="button"
          onClick={() => onZoomTo(item.eventCaseId)}
          title="Centrer la carte sur cet emplacement"
          style={zoomButtonStyle}
        >
          🔍 Zoomer sur le lieu
        </button>
      )}
    </div>
  </>}
</aside>;

const LoadingState = () => <div style={stateStyle}>Chargement Carte + Timeline…</div>;
const ErrorState: React.FC<{ message: string; onRetry: () => Promise<void> }> = ({ message, onRetry }) => <div role="alert" style={stateStyle}><p style={{ color: '#ff7676' }}>{message}</p><button type="button" onClick={() => void onRetry()} style={primaryButtonStyle(false)}>Réessayer</button></div>;
const EmptyState = () => <div style={stateStyle}>Aucune donnée Carte + Timeline disponible pour ces critères. Chargez le seed démo ou assouplissez les filtres.</div>;

function hasCoordinates(item: MapTimelineItem): boolean { return typeof item.latitude === 'number' && typeof item.longitude === 'number'; }
function markerColor(item: MapTimelineItem): string { return item.score >= 0.7 ? '#22c55e' : item.score >= 0.5 ? '#f59e0b' : '#ef4444'; }
function formatDate(value: string): string { return new Date(value).toLocaleString('fr-FR', { dateStyle: 'medium', timeStyle: 'short' }); }
function formatPeriod(start?: string | null, end?: string | null): string { return start && end ? `${new Date(start).toLocaleDateString('fr-FR')} → ${new Date(end).toLocaleDateString('fr-FR')}` : 'n/a'; }

const panelStyle: React.CSSProperties = { background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem' };
const sectionHeadingStyle: React.CSSProperties = { marginTop: 0, marginBottom: '0.75rem', color: 'var(--accent)', fontSize: '1rem' };
const labelStyle: React.CSSProperties = { display: 'grid', gap: '0.35rem', color: 'var(--text-secondary)', fontSize: '0.9rem' };
const selectStyle: React.CSSProperties = { background: 'var(--bg-primary)', color: 'var(--text-primary)', border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.5rem', minWidth: '150px' };
const metricStyle: React.CSSProperties = { background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '0.85rem' };
const stateStyle: React.CSSProperties = { background: 'var(--bg-secondary)', borderRadius: '8px', padding: '2rem', border: '1px solid #2a2a3e', marginTop: '1rem', color: 'var(--text-secondary)' };
const definitionGridStyle: React.CSSProperties = { display: 'grid', gridTemplateColumns: 'max-content 1fr', gap: '0.55rem 1rem', margin: '0 0 1rem', color: 'var(--text-secondary)' };
const linkButtonStyle: React.CSSProperties = { display: 'inline-block', background: 'var(--accent)', color: '#fff', textDecoration: 'none', borderRadius: '6px', padding: '0.55rem 0.8rem' };
function primaryButtonStyle(disabled: boolean): React.CSSProperties { return { background: 'var(--accent)', color: '#fff', border: 0, borderRadius: '6px', padding: '0.55rem 0.8rem', cursor: disabled ? 'wait' : 'pointer', opacity: disabled ? 0.75 : 1 }; }
function timelineButtonStyle(active: boolean): React.CSSProperties { return { width: '100%', display: 'flex', alignItems: 'center', gap: '0.75rem', background: active ? 'rgba(74, 144, 217, 0.18)' : 'var(--bg-primary)', color: 'var(--text-primary)', border: active ? '1px solid var(--accent)' : '1px solid #2a2a3e', borderRadius: '8px', padding: '0.75rem', cursor: 'pointer' }; }

const zoomButtonStyle: React.CSSProperties = {
  display: 'inline-block',
  background: 'transparent',
  color: 'var(--accent)',
  border: '1px solid var(--accent)',
  borderRadius: '6px',
  padding: '0.55rem 0.8rem',
  cursor: 'pointer',
  fontSize: '0.85rem',
};

export default MapTimeline;