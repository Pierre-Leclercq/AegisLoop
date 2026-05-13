import React from 'react';

type Observation = { id: string; title: string; content: string; status: string; observedAt: string; sourceType: string; sourceName: string; sourceUrl?: string | null };
type Feedback = { id: string; action: string; details?: string | null; createdAt: string };
type ScoreComponent = { name: string; value: number; weight: number; contribution: number; explanation: string };
type ScoreBreakdown = { value: number; algorithmVersion: string; components: ScoreComponent[] };
type ObservationProvenance = {
  observationId: string;
  title: string;
  sourceUrl?: string | null;
  sourceConnector?: { name: string; connectorType: string; status: string } | null;
  rawItem?: { id: string; sourceHash: string; contentType: string; collectedAt: string; metadata: Record<string, string | null> } | null;
  ingestionJob?: { id: string; startedAt: string; status: string; itemsCollected: number; itemsNormalized: number } | null;
  score?: ScoreBreakdown | null;
  feedbacks: Feedback[];
  metadata: Record<string, string | null>;
};
type ApiEnvelope<T> = { success: boolean; data: T; error?: string | null };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';

const Observations: React.FC = () => {
  const [observations, setObservations] = React.useState<Observation[]>([]);
  const [selected, setSelected] = React.useState<Observation | null>(null);
  const [provenance, setProvenance] = React.useState<ObservationProvenance | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [detailLoading, setDetailLoading] = React.useState(false);
  const [runningIngestion, setRunningIngestion] = React.useState(false);
  const [error, setError] = React.useState<string | null>(null);

  const loadProvenance = React.useCallback(async (observation: Observation) => {
    setSelected(observation);
    setDetailLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/observations/${observation.id}/provenance`);
      const envelope = await response.json() as ApiEnvelope<ObservationProvenance>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Provenance indisponible (${response.status})`);
      setProvenance(envelope.data);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Erreur provenance inconnue');
    } finally {
      setDetailLoading(false);
    }
  }, []);

  const loadObservations = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/observations`);
      if (!response.ok) throw new Error(`API observations indisponible (${response.status})`);
      const envelope = await response.json() as ApiEnvelope<Observation[]>;
      if (!envelope.success) throw new Error(envelope.error ?? 'Réponse API observations invalide');
      setObservations(envelope.data);
      if (envelope.data.length > 0 && !selected) await loadProvenance(envelope.data[0]);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Erreur inconnue');
    } finally {
      setLoading(false);
    }
  }, [loadProvenance]);

  React.useEffect(() => { void loadObservations(); }, [loadObservations]);

  const runIngestion = async () => {
    setRunningIngestion(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/ingestion/rss/run`, { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ maxItems: 10 }) });
      const envelope = await response.json() as ApiEnvelope<unknown>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Ingestion RSS échouée (${response.status})`);
      setSelected(null);
      setProvenance(null);
      await loadObservations();
    } catch (ingestionError) {
      setError(ingestionError instanceof Error ? ingestionError.message : 'Erreur ingestion inconnue');
    } finally {
      setRunningIngestion(false);
    }
  };

  const submitFeedback = async (action: string, details: string) => {
    if (!selected) return;
    const response = await fetch(`${API_BASE_URL}/api/feedback`, {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ targetId: selected.id, targetType: 'Observation', action, details }),
    });
    const envelope = await response.json() as ApiEnvelope<unknown>;
    if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Feedback échoué (${response.status})`);
    await loadProvenance(selected);
  };

  return <div>
    <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', alignItems: 'center' }}>
      <div><h1 style={{ color: 'var(--accent)', marginBottom: '0.5rem' }}>Observations</h1><p style={{ color: 'var(--text-secondary)', marginTop: 0 }}>Liste, provenance réelle et feedback analyste connecté.</p></div>
      <button type="button" onClick={runIngestion} disabled={runningIngestion} style={primaryButtonStyle(runningIngestion)}>{runningIngestion ? 'Ingestion RSS…' : 'Déclencher RSS démo'}</button>
    </div>
    {loading && <p style={{ color: 'var(--text-secondary)' }}>Chargement des observations…</p>}
    {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
    {!loading && !error && observations.length === 0 && <Empty text="Aucune observation persistée. Déclenchez l’ingestion RSS de démonstration." />}
    {observations.length > 0 && <div style={{ display: 'grid', gridTemplateColumns: 'minmax(280px, 42%) 1fr', gap: '1rem', marginTop: '1rem' }}>
      <section style={{ display: 'grid', gap: '0.75rem', alignContent: 'start' }} aria-label="Liste observations">
        {observations.map(observation => <button key={observation.id} type="button" onClick={() => void loadProvenance(observation)} style={cardButtonStyle(selected?.id === observation.id)}>
          <strong style={{ display: 'block', textAlign: 'left' }}>{observation.title}</strong>
          <span style={{ display: 'block', color: 'var(--text-secondary)', textAlign: 'left', marginTop: '0.4rem' }}>{observation.sourceName} · {observation.sourceType} · {new Date(observation.observedAt).toLocaleString()}</span>
        </button>)}
      </section>
      <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem' }}>
        {detailLoading && <p style={{ color: 'var(--text-secondary)' }}>Chargement provenance…</p>}
        {!detailLoading && selected && provenance && <ObservationDetail observation={selected} provenance={provenance} onSubmitFeedback={submitFeedback} />}
      </section>
    </div>}
  </div>;
};

const ObservationDetail: React.FC<{ observation: Observation; provenance: ObservationProvenance; onSubmitFeedback: (action: string, details: string) => Promise<void> }> = ({ observation, provenance, onSubmitFeedback }) => <div>
  <h2 style={{ marginTop: 0 }}>{observation.title}</h2><p style={{ color: 'var(--text-secondary)' }}>{observation.content}</p>
  <ProvenancePanel provenance={provenance} /><ScorePanel score={provenance.score} /><FeedbackPanel feedbacks={provenance.feedbacks} onSubmit={onSubmitFeedback} />
</div>;

const ProvenancePanel: React.FC<{ provenance: ObservationProvenance }> = ({ provenance }) => <section style={panelStyle}>
  <h3 style={{ marginTop: 0 }}>Provenance réelle</h3>
  <dl style={definitionGridStyle}>
    <dt>SourceConnector</dt><dd style={{ margin: 0 }}>{provenance.sourceConnector ? `${provenance.sourceConnector.name} (${provenance.sourceConnector.connectorType})` : 'n/a'}</dd>
    <dt>RawItem</dt><dd style={{ margin: 0 }}>{provenance.rawItem?.id ?? 'n/a'}</dd>
    <dt>Hash</dt><dd style={{ margin: 0, wordBreak: 'break-all' }}>{provenance.rawItem?.sourceHash ?? 'n/a'}</dd>
    <dt>Ingestion</dt><dd style={{ margin: 0 }}>{provenance.ingestionJob ? `${provenance.ingestionJob.status} · ${new Date(provenance.ingestionJob.startedAt).toLocaleString()}` : 'n/a'}</dd>
    <dt>URL</dt><dd style={{ margin: 0 }}>{provenance.sourceUrl ? <a href={provenance.sourceUrl} target="_blank" rel="noreferrer" style={{ color: 'var(--accent)' }}>ouvrir source</a> : 'n/a'}</dd>
  </dl>
  {Object.keys(provenance.metadata).length > 0 && <small style={{ color: 'var(--text-secondary)' }}>Métadonnées : {Object.entries(provenance.metadata).map(([k, v]) => `${k}=${v ?? 'n/a'}`).join(' · ')}</small>}
</section>;

const ScorePanel: React.FC<{ score?: ScoreBreakdown | null }> = ({ score }) => <section style={panelStyle}>
  <h3 style={{ marginTop: 0 }}>Impact score</h3>
  {!score ? <p style={{ color: 'var(--text-secondary)' }}>Aucun score calculé.</p> : <ul>{score.components.map(c => <li key={c.name}><strong>{c.name}</strong> {c.value.toFixed(2)} × {c.weight.toFixed(2)} = {c.contribution.toFixed(2)}</li>)}</ul>}
</section>;

const FeedbackPanel: React.FC<{ feedbacks: Feedback[]; onSubmit: (action: string, details: string) => Promise<void> }> = ({ feedbacks, onSubmit }) => {
  const [details, setDetails] = React.useState('');
  const [submitting, setSubmitting] = React.useState<string | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const submit = async (action: string) => { setSubmitting(action); setError(null); try { await onSubmit(action, details); setDetails(''); } catch (e) { setError(e instanceof Error ? e.message : 'Erreur feedback'); } finally { setSubmitting(null); } };
  return <section style={panelStyle}>
    <h3 style={{ marginTop: 0 }}>Feedback analyste</h3>
    <textarea aria-label="Note feedback" value={details} onChange={e => setDetails(e.target.value)} placeholder="Note ou correction simple…" style={{ width: '100%', minHeight: '4rem', background: 'var(--bg-primary)', color: 'var(--text-primary)', border: '1px solid #2a2a3e', borderRadius: '6px' }} />
    <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap', marginTop: '0.5rem' }}>{['Confirm', 'Invalidate', 'Correct', 'Note'].map(action => <button key={action} type="button" disabled={!!submitting} onClick={() => void submit(action)} style={primaryButtonStyle(!!submitting)}>{submitting === action ? 'Envoi…' : action}</button>)}</div>
    {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
    {feedbacks.length === 0 ? <p style={{ color: 'var(--text-secondary)' }}>Aucun feedback existant.</p> : <ul>{feedbacks.map(f => <li key={f.id}><strong>{f.action}</strong> · {new Date(f.createdAt).toLocaleString()} {f.details ? `— ${f.details}` : ''}</li>)}</ul>}
  </section>;
};

const Empty: React.FC<{ text: string }> = ({ text }) => <div style={{ background: 'var(--bg-secondary)', borderRadius: '8px', padding: '2rem', border: '1px solid #2a2a3e', marginTop: '1rem' }}><p style={{ color: 'var(--text-secondary)' }}>{text}</p></div>;
const panelStyle: React.CSSProperties = { border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem', marginBottom: '1rem' };
const definitionGridStyle: React.CSSProperties = { display: 'grid', gridTemplateColumns: 'max-content 1fr', gap: '0.5rem 1rem', margin: 0, color: 'var(--text-secondary)' };
function primaryButtonStyle(disabled: boolean): React.CSSProperties { return { background: 'var(--accent)', color: '#fff', border: 0, borderRadius: '6px', padding: '0.55rem 0.8rem', cursor: disabled ? 'wait' : 'pointer' }; }
function cardButtonStyle(active: boolean): React.CSSProperties { return { background: active ? 'rgba(74, 144, 217, 0.18)' : 'var(--bg-secondary)', color: 'var(--text-primary)', border: active ? '1px solid var(--accent)' : '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', cursor: 'pointer' }; }

export default Observations;