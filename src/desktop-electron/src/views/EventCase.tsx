import React from 'react';

type Observation = {
  id: string;
  title: string;
  content: string;
  status: string;
  observedAt: string;
  sourceType: string;
  sourceName: string;
  sourceUrl?: string | null;
};

type ScoreComponent = {
  name: string;
  value: number;
  weight: number;
  contribution: number;
  explanation: string;
};

type ScoreBreakdown = {
  targetId: string;
  targetType: string;
  value: number;
  calculatedAt: string;
  algorithmVersion: string;
  components: ScoreComponent[];
};

type Feedback = { id: string; action: string; details?: string | null; createdAt: string };

type EventCaseProvenance = {
  eventCaseId: string;
  observations: Array<{ observationId: string; title: string; rawItem?: { id: string; sourceHash: string; collectedAt: string } | null; sourceConnector?: { name: string; connectorType: string } | null; feedbacks: Feedback[]; score?: ScoreBreakdown | null }>;
  sources: Array<{ id: string; name: string; connectorType: string }>;
  rawItems: Array<{ id: string; sourceHash: string; collectedAt: string }>;
  hashes: string[];
  feedbacks: Feedback[];
  score?: ScoreBreakdown | null;
};

type EventCaseSummary = {
  id: string;
  title: string;
  summary?: string | null;
  category: string;
  status: string;
  startedAt: string;
  createdAt: string;
  updatedAt: string;
  observationCount: number;
  corroborationCount: number;
  score: number;
  sources: string[];
  scoreBreakdown?: ScoreBreakdown | null;
};

type EventCaseDetail = EventCaseSummary & {
  observations: Observation[];
  provenance?: EventCaseProvenance | null;
};

type ApiEnvelope<T> = {
  success: boolean;
  data: T;
  error?: string | null;
};

type LoadEventsOptions = {
  selectFirst?: boolean;
  preserveSelectionId?: string | null;
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';

const EventCase: React.FC = () => {
  const [events, setEvents] = React.useState<EventCaseSummary[]>([]);
  const [selected, setSelected] = React.useState<EventCaseDetail | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [detailLoading, setDetailLoading] = React.useState(false);
  const [rebuilding, setRebuilding] = React.useState(false);
  const [exporting, setExporting] = React.useState<string | null>(null);
  const [exportMessage, setExportMessage] = React.useState<string | null>(null);
  const [error, setError] = React.useState<string | null>(null);

  const loadDetail = React.useCallback(async (id: string) => {
    setDetailLoading(true);
    setError(null);
    setExportMessage(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/events/${id}`);
      if (!response.ok) throw new Error(`Détail EventCase indisponible (${response.status})`);
      const envelope = await response.json() as ApiEnvelope<EventCaseDetail>;
      if (!envelope.success) throw new Error(envelope.error ?? 'Réponse détail EventCase invalide');
      const provenanceResponse = await fetch(`${API_BASE_URL}/api/events/${id}/provenance`);
      const provenanceEnvelope = await provenanceResponse.json() as ApiEnvelope<EventCaseProvenance>;
      setSelected({ ...envelope.data, provenance: provenanceResponse.ok && provenanceEnvelope.success ? provenanceEnvelope.data : null });
    } catch (detailError) {
      setError(detailError instanceof Error ? detailError.message : 'Erreur inconnue');
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const loadEvents = React.useCallback(async (options: LoadEventsOptions = {}) => {
    const { selectFirst = true, preserveSelectionId = null } = options;
    setLoading(true);
    setError(null);

    try {
      const response = await fetch(`${API_BASE_URL}/api/events`);
      if (!response.ok) throw new Error(`API events indisponible (${response.status})`);
      const envelope = await response.json() as ApiEnvelope<EventCaseSummary[]>;
      if (!envelope.success) throw new Error(envelope.error ?? 'Réponse API events invalide');
      setEvents(envelope.data);
      if (selectFirst && envelope.data.length > 0) {
        const idToLoad = preserveSelectionId && envelope.data.some(eventCase => eventCase.id === preserveSelectionId)
          ? preserveSelectionId
          : envelope.data[0].id;
        await loadDetail(idToLoad);
      }
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Erreur inconnue');
    } finally {
      setLoading(false);
    }
  }, [loadDetail]);

  React.useEffect(() => {
    void loadEvents();
  }, [loadEvents]);

  const rebuildEvents = async () => {
    setRebuilding(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/events/rebuild`, { method: 'POST' });
      const envelope = await response.json() as ApiEnvelope<unknown>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Rebuild EventCases échoué (${response.status})`);
      setSelected(null);
      await loadEvents();
    } catch (rebuildError) {
      setError(rebuildError instanceof Error ? rebuildError.message : 'Erreur rebuild inconnue');
    } finally {
      setRebuilding(false);
    }
  };

  const submitEventFeedback = async (action: string, details: string) => {
    if (!selected) return;
    const selectedId = selected.id;
    const response = await fetch(`${API_BASE_URL}/api/feedback`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ targetId: selectedId, targetType: 'EventCase', action, details }),
    });
    const envelope = await response.json() as ApiEnvelope<unknown>;
    if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Feedback EventCase échoué (${response.status})`);
    await loadDetail(selectedId);
    await loadEvents({ selectFirst: false, preserveSelectionId: selectedId });
  };

  const exportEventCase = async (format: 'json' | 'markdown') => {
    if (!selected) return;
    setExporting(format);
    setExportMessage(null);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/export/${selected.id}?format=${format}`);
      if (!response.ok) throw new Error(`Export ${format} échoué (${response.status})`);
      const fileName = `aegisloop-eventcase-${selected.id}.${format === 'json' ? 'json' : 'md'}`;
      if (format === 'json') {
        const envelope = await response.json() as ApiEnvelope<unknown>;
        if (!envelope.success) throw new Error(envelope.error ?? 'Export JSON invalide');
        downloadTextFile(JSON.stringify(envelope, null, 2), 'application/json;charset=utf-8', fileName);
      } else {
        const markdown = await response.text();
        if (!markdown.includes('## Limites / incertitudes')) throw new Error('Export Markdown incomplet');
        downloadTextFile(markdown, 'text/markdown;charset=utf-8', fileName);
      }
      setExportMessage(`Export ${format === 'json' ? 'JSON' : 'Markdown'} généré : ${fileName}. Fichier récupérable via le téléchargement Electron/navigateur.`);
    } catch (exportError) {
      setError(exportError instanceof Error ? exportError.message : 'Erreur export inconnue');
    } finally {
      setExporting(null);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', alignItems: 'center' }}>
        <div>
          <h1 style={{ color: 'var(--accent)', marginBottom: '0.5rem' }}>EventCase</h1>
          <p style={{ color: 'var(--text-secondary)', marginTop: 0 }}>
            Liste et détail alimentés par les endpoints réels /api/events.
          </p>
        </div>
        <button type="button" onClick={rebuildEvents} disabled={rebuilding} style={primaryButtonStyle(rebuilding)}>
          {rebuilding ? 'Reconstruction…' : 'Reconstruire EventCases'}
        </button>
      </div>

      {loading && <p style={{ color: 'var(--text-secondary)' }}>Chargement des EventCases…</p>}
      {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
      {!loading && !error && events.length === 0 && (
        <div style={{ background: 'var(--bg-secondary)', borderRadius: '8px', padding: '2rem', border: '1px solid #2a2a3e', marginTop: '1rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>Aucun EventCase. Ingestez des observations puis lancez la reconstruction.</p>
        </div>
      )}

      {events.length > 0 && (
        <div style={{ display: 'grid', gridTemplateColumns: 'minmax(280px, 38%) 1fr', gap: '1rem', marginTop: '1rem' }}>
          <section style={{ display: 'grid', gap: '0.75rem', alignContent: 'start' }} aria-label="Liste EventCases">
            {events.map(eventCase => (
              <button key={eventCase.id} type="button" onClick={() => void loadDetail(eventCase.id)} style={eventButtonStyle(selected?.id === eventCase.id)}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: '0.75rem' }}>
                  <strong style={{ textAlign: 'left' }}>{eventCase.title}</strong>
                  <span style={{ color: scoreColor(eventCase.score), fontWeight: 'bold' }}>{formatScore(eventCase.score)}</span>
                </div>
                <div style={{ color: 'var(--text-secondary)', marginTop: '0.5rem', textAlign: 'left' }}>
                  {eventCase.observationCount} observation(s) · {eventCase.sources.join(', ') || 'sources inconnues'}
                </div>
                <div style={{ color: 'var(--text-secondary)', marginTop: '0.25rem', textAlign: 'left' }}>
                  Maj {new Date(eventCase.updatedAt).toLocaleString()} · {eventCase.status}
                </div>
              </button>
            ))}
          </section>

          <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem' }}>
            {detailLoading && <p style={{ color: 'var(--text-secondary)' }}>Chargement du détail…</p>}
            {!detailLoading && selected === null && <p style={{ color: 'var(--text-secondary)' }}>Sélectionnez un EventCase.</p>}
            {!detailLoading && selected && <EventCaseDetailPanel eventCase={selected} onSubmitFeedback={submitEventFeedback} onExport={exportEventCase} exporting={exporting} exportMessage={exportMessage} />}
          </section>
        </div>
      )}
    </div>
  );
};

const EventCaseDetailPanel: React.FC<{ eventCase: EventCaseDetail; onSubmitFeedback: (action: string, details: string) => Promise<void>; onExport: (format: 'json' | 'markdown') => Promise<void>; exporting: string | null; exportMessage: string | null }> = ({ eventCase, onSubmitFeedback, onExport, exporting, exportMessage }) => (
  <div>
    <header style={{ borderBottom: '1px solid #2a2a3e', paddingBottom: '0.75rem', marginBottom: '1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem' }}>
        <div>
          <h2 style={{ marginTop: 0 }}>{eventCase.title}</h2>
          <p style={{ color: 'var(--text-secondary)' }}>{eventCase.summary}</p>
        </div>
        <div style={{ color: scoreColor(eventCase.score), fontSize: '1.8rem', fontWeight: 'bold' }}>{formatScore(eventCase.score)}</div>
      </div>
      <dl style={{ display: 'grid', gridTemplateColumns: 'repeat(4, max-content)', gap: '0.5rem 1rem', margin: 0, color: 'var(--text-secondary)' }}>
        <dt>Catégorie</dt><dd style={{ margin: 0 }}>{eventCase.category}</dd>
        <dt>Statut</dt><dd style={{ margin: 0 }}>{eventCase.status}</dd>
        <dt>Sources</dt><dd style={{ margin: 0 }}>{eventCase.sources.join(', ') || 'n/a'}</dd>
        <dt>Observations</dt><dd style={{ margin: 0 }}>{eventCase.observationCount}</dd>
      </dl>
      <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginTop: '0.75rem' }}>
        <button type="button" disabled={!!exporting} onClick={() => void onExport('json')} style={primaryButtonStyle(!!exporting)}>{exporting === 'json' ? 'Export JSON…' : 'Exporter JSON'}</button>
        <button type="button" disabled={!!exporting} onClick={() => void onExport('markdown')} style={secondaryButtonStyle(!!exporting)}>{exporting === 'markdown' ? 'Export Markdown…' : 'Exporter Markdown'}</button>
      </div>
      {exportMessage && <p role="status" style={{ color: 'var(--success)', marginBottom: 0 }}>{exportMessage}</p>}
    </header>

    <ScoreBreakdownPanel breakdown={eventCase.scoreBreakdown} />
    <EventProvenancePanel provenance={eventCase.provenance} />
    <FeedbackPanel feedbacks={eventCase.provenance?.feedbacks ?? []} onSubmit={onSubmitFeedback} />

    <h3>Observations liées</h3>
    {eventCase.observations.length === 0 ? (
      <p style={{ color: 'var(--text-secondary)' }}>Aucune observation liée.</p>
    ) : (
      <div style={{ display: 'grid', gap: '0.75rem' }}>
        {eventCase.observations.map(observation => (
          <article key={observation.id} style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem' }}>
            <strong>{observation.title}</strong>
            <p style={{ color: 'var(--text-secondary)' }}>{observation.content.length > 220 ? `${observation.content.slice(0, 220)}…` : observation.content}</p>
            <small style={{ color: 'var(--text-secondary)' }}>{observation.sourceName} · {new Date(observation.observedAt).toLocaleString()} · {observation.status}</small>
          </article>
        ))}
      </div>
    )}
  </div>
);

const EventProvenancePanel: React.FC<{ provenance?: EventCaseProvenance | null }> = ({ provenance }) => (
  <section style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem', marginBottom: '1rem' }}>
    <h3 style={{ marginTop: 0 }}>Provenance agrégée</h3>
    {!provenance ? <p style={{ color: 'var(--text-secondary)' }}>Provenance EventCase indisponible.</p> : (
      <div>
        <p style={{ color: 'var(--text-secondary)' }}>{provenance.observations.length} observation(s), {provenance.sources.length} source(s), {provenance.rawItems.length} RawItem(s).</p>
        <p style={{ color: 'var(--text-secondary)', wordBreak: 'break-all' }}>Hashes : {provenance.hashes.join(', ') || 'n/a'}</p>
        <ul>
          {provenance.observations.map(obs => <li key={obs.observationId}><strong>{obs.title}</strong> — {obs.sourceConnector?.name ?? 'Source inconnue'} — {obs.rawItem?.sourceHash ?? 'hash n/a'}</li>)}
        </ul>
      </div>
    )}
  </section>
);

const FeedbackPanel: React.FC<{ feedbacks: Feedback[]; onSubmit: (action: string, details: string) => Promise<void> }> = ({ feedbacks, onSubmit }) => {
  const [details, setDetails] = React.useState('');
  const [submitting, setSubmitting] = React.useState<string | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const submit = async (action: string) => {
    setSubmitting(action);
    setError(null);
    try { await onSubmit(action, details); setDetails(''); } catch (submitError) { setError(submitError instanceof Error ? submitError.message : 'Erreur feedback'); } finally { setSubmitting(null); }
  };
  return <section style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem', marginBottom: '1rem' }}>
    <h3 style={{ marginTop: 0 }}>Feedback EventCase</h3>
    <textarea aria-label="Note feedback EventCase" value={details} onChange={e => setDetails(e.target.value)} placeholder="Note ou correction simple…" style={{ width: '100%', minHeight: '4rem', background: 'var(--bg-primary)', color: 'var(--text-primary)', border: '1px solid #2a2a3e', borderRadius: '6px' }} />
    <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginTop: '0.5rem' }}>{['Confirm', 'Invalidate', 'Correct', 'Note'].map(action => <button key={action} type="button" disabled={!!submitting} onClick={() => void submit(action)} style={primaryButtonStyle(!!submitting)}>{submitting === action ? 'Envoi…' : action}</button>)}</div>
    {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
    {feedbacks.length === 0 ? <p style={{ color: 'var(--text-secondary)' }}>Aucun feedback existant.</p> : <ul>{feedbacks.map(f => <li key={f.id}><strong>{f.action}</strong> · {new Date(f.createdAt).toLocaleString()} {f.details ? `— ${f.details}` : ''}</li>)}</ul>}
  </section>;
};

const ScoreBreakdownPanel: React.FC<{ breakdown?: ScoreBreakdown | null }> = ({ breakdown }) => (
  <section style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem', marginBottom: '1rem' }}>
    <h3 style={{ marginTop: 0 }}>Breakdown du score</h3>
    {!breakdown ? <p style={{ color: 'var(--text-secondary)' }}>Aucun breakdown disponible.</p> : (
      <div>
        <p style={{ color: 'var(--text-secondary)' }}>Algorithme {breakdown.algorithmVersion} · score {formatScore(breakdown.value)}</p>
        <ul style={{ paddingLeft: '1.2rem' }}>
          {breakdown.components.map(component => (
            <li key={component.name}>
              <strong>{component.name}</strong> : valeur {component.value.toFixed(2)}, poids {component.weight.toFixed(2)}, contribution {component.contribution.toFixed(2)} — {component.explanation}
            </li>
          ))}
        </ul>
      </div>
    )}
  </section>
);

function formatScore(score: number): string {
  return `${Math.round(score * 100)}%`;
}

function scoreColor(score: number): string {
  if (score >= 0.7) return 'var(--success)';
  if (score >= 0.45) return 'var(--warning)';
  return '#ff7676';
}

function primaryButtonStyle(disabled: boolean): React.CSSProperties {
  return {
    background: 'var(--accent)',
    color: '#fff',
    border: 0,
    borderRadius: '6px',
    padding: '0.7rem 1rem',
    cursor: disabled ? 'wait' : 'pointer',
  };
}

function secondaryButtonStyle(disabled: boolean): React.CSSProperties {
  return {
    background: 'transparent',
    color: 'var(--accent)',
    border: '1px solid var(--accent)',
    borderRadius: '6px',
    padding: '0.7rem 1rem',
    cursor: disabled ? 'wait' : 'pointer',
  };
}

function eventButtonStyle(active: boolean): React.CSSProperties {
  return {
    background: active ? 'rgba(74, 144, 217, 0.18)' : 'var(--bg-secondary)',
    color: 'var(--text-primary)',
    border: active ? '1px solid var(--accent)' : '1px solid #2a2a3e',
    borderRadius: '8px',
    padding: '1rem',
    cursor: 'pointer',
  };
}

function downloadTextFile(content: string, mimeType: string, fileName: string): void {
  const createObjectUrl = globalThis.URL?.createObjectURL;
  if (typeof document === 'undefined' || typeof Blob === 'undefined' || !createObjectUrl) {
    return;
  }

  const blob = new Blob([content], { type: mimeType });
  const url = createObjectUrl.call(globalThis.URL, blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  globalThis.URL?.revokeObjectURL?.(url);
}

export default EventCase;