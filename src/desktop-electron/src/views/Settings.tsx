import React from 'react';

type AuditEntry = { id: string; date: string; category: string; action: string; targetType?: string | null; targetId?: string | null; message: string; level: string; actor: string };
type DemoStatus = { datasetVersion: string; loaded: boolean; connectors: number; rawItems: number; observations: number; eventCases: number; scores: number; feedbacks: number; auditEntries: number; scenarios: string[] };
type ApiEnvelope<T> = { success: boolean; data: T; error?: string | null };

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';

const Settings: React.FC = () => {
  const [audit, setAudit] = React.useState<AuditEntry[]>([]);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);
  const [demoStatus, setDemoStatus] = React.useState<DemoStatus | null>(null);
  const [demoLoading, setDemoLoading] = React.useState(true);
  const [demoAction, setDemoAction] = React.useState<string | null>(null);
  const [demoMessage, setDemoMessage] = React.useState<string | null>(null);
  const [demoError, setDemoError] = React.useState<string | null>(null);

  const loadAudit = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/audit?take=200`);
      const envelope = await response.json() as ApiEnvelope<AuditEntry[]>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Audit indisponible (${response.status})`);
      setAudit(envelope.data);
    } catch (loadError) {
      setError(loadError instanceof Error ? loadError.message : 'Erreur audit inconnue');
    } finally {
      setLoading(false);
    }
  }, []);

  React.useEffect(() => { void loadAudit(); }, [loadAudit]);

  const loadDemoStatus = React.useCallback(async () => {
    setDemoLoading(true);
    setDemoError(null);
    try {
      const response = await fetch(`${API_BASE_URL}/api/demo/status`);
      const envelope = await response.json() as ApiEnvelope<DemoStatus>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `Statut démo indisponible (${response.status})`);
      setDemoStatus(envelope.data);
    } catch (loadError) {
      setDemoError(loadError instanceof Error ? loadError.message : 'Erreur statut démo inconnue');
    } finally {
      setDemoLoading(false);
    }
  }, []);

  React.useEffect(() => { void loadDemoStatus(); }, [loadDemoStatus]);

  const runDemoAction = async (label: string, endpoint: string) => {
    setDemoAction(label);
    setDemoMessage(null);
    setDemoError(null);
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, { method: 'POST' });
      const envelope = await response.json() as ApiEnvelope<unknown>;
      if (!response.ok || !envelope.success) throw new Error(envelope.error ?? `${label} échoué (${response.status})`);
      setDemoMessage(`${label} terminé.`);
      await loadDemoStatus();
      await loadAudit();
    } catch (actionError) {
      setDemoError(actionError instanceof Error ? actionError.message : `Erreur ${label}`);
    } finally {
      setDemoAction(null);
    }
  };

  return <div>
    <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem', alignItems: 'center' }}>
      <div>
        <h1 style={{ color: 'var(--accent)', marginBottom: '1rem' }}>Paramètres</h1>
        <p style={{ color: 'var(--text-secondary)' }}>Configuration connecteurs, scoring, mode démo — et audit V1 exploitable.</p>
      </div>
      <button type="button" onClick={() => void loadAudit()} style={{ background: 'var(--accent)', color: '#fff', border: 0, borderRadius: '6px', padding: '0.7rem 1rem', cursor: 'pointer' }}>Rafraîchir audit</button>
    </div>
    <section style={{ background: 'var(--bg-secondary)', borderRadius: '8px', padding: '1rem', border: '1px solid #2a2a3e', marginTop: '1rem' }}>
      <h2 style={{ marginTop: 0 }}>Mode démo seed/replay V1</h2>
      <p style={{ color: 'var(--text-secondary)' }}>Dataset local déterministe, reset, rebuild EventCases et recalcul des scores sans dépendance réseau.</p>
      {demoLoading && <p style={{ color: 'var(--text-secondary)' }}>Chargement statut démo…</p>}
      {demoError && <p role="alert" style={{ color: '#ff7676' }}>{demoError}</p>}
      {demoMessage && <p role="status" style={{ color: 'var(--success)' }}>{demoMessage}</p>}
      {demoStatus && <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '0.75rem', marginBottom: '1rem' }}>
        <Metric label="État" value={demoStatus.loaded ? 'Chargé' : 'Vide'} />
        <Metric label="Observations" value={demoStatus.observations} />
        <Metric label="EventCases" value={demoStatus.eventCases} />
        <Metric label="Sources" value={demoStatus.connectors} />
        <Metric label="Scores" value={demoStatus.scores} />
        <Metric label="Feedbacks" value={demoStatus.feedbacks} />
      </div>}
      {demoStatus && <p style={{ color: 'var(--text-secondary)' }}>Version {demoStatus.datasetVersion} · scénarios : {demoStatus.scenarios.join(', ') || 'n/a'}</p>}
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
        <button type="button" disabled={!!demoAction} onClick={() => void runDemoAction('Chargement seed', '/api/demo/load')} style={primaryButtonStyle(!!demoAction)}>{demoAction === 'Chargement seed' ? 'Chargement…' : 'Charger seed'}</button>
        <button type="button" disabled={!!demoAction} onClick={() => void runDemoAction('Reset démo', '/api/demo/reset')} style={secondaryButtonStyle(!!demoAction)}>{demoAction === 'Reset démo' ? 'Reset…' : 'Reset démo'}</button>
        <button type="button" disabled={!!demoAction} onClick={() => void runDemoAction('Rebuild EventCases', '/api/demo/rebuild')} style={secondaryButtonStyle(!!demoAction)}>{demoAction === 'Rebuild EventCases' ? 'Rebuild…' : 'Rebuild EventCases'}</button>
        <button type="button" disabled={!!demoAction} onClick={() => void runDemoAction('Recalcul scores', '/api/demo/recalculate')} style={secondaryButtonStyle(!!demoAction)}>{demoAction === 'Recalcul scores' ? 'Recalcul…' : 'Recalcul scores'}</button>
      </div>
    </section>
    <section style={{ background: 'var(--bg-secondary)', borderRadius: '8px', padding: '1rem', border: '1px solid #2a2a3e', marginTop: '1rem' }}>
      <h2 style={{ marginTop: 0 }}>Audit log</h2>
      <p style={{ color: 'var(--text-secondary)' }}>Les 200 dernières entrées sont chargées afin de conserver visibles reset, load, rebuild, recalcul, feedback et export pendant une démo complète.</p>
      {loading && <p style={{ color: 'var(--text-secondary)' }}>Chargement audit…</p>}
      {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
      {!loading && !error && audit.length === 0 && <p style={{ color: 'var(--text-secondary)' }}>Aucune entrée d’audit.</p>}
      {audit.length > 0 && <div style={{ display: 'grid', gap: '0.6rem' }}>
        {audit.map(entry => <article key={entry.id} style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem' }}>
          <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem' }}><strong>{entry.category} · {entry.action}</strong><span style={{ color: entry.level === 'Error' ? '#ff7676' : 'var(--text-secondary)' }}>{entry.level}</span></div>
          <small style={{ color: 'var(--text-secondary)' }}>{new Date(entry.date).toLocaleString()} · {entry.actor} · {entry.targetType ?? 'n/a'} {entry.targetId ?? ''}</small>
          <p style={{ color: 'var(--text-secondary)', wordBreak: 'break-word' }}>{entry.message}</p>
        </article>)}
      </div>}
    </section>
  </div>;
};

const Metric: React.FC<{ label: string; value: string | number }> = ({ label, value }) => <div style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem' }}>
  <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem' }}>{label}</div>
  <strong>{value}</strong>
</div>;

function primaryButtonStyle(disabled: boolean): React.CSSProperties {
  return { background: 'var(--accent)', color: '#fff', border: 0, borderRadius: '6px', padding: '0.7rem 1rem', cursor: disabled ? 'wait' : 'pointer' };
}

function secondaryButtonStyle(disabled: boolean): React.CSSProperties {
  return { background: 'transparent', color: 'var(--accent)', border: '1px solid var(--accent)', borderRadius: '6px', padding: '0.7rem 1rem', cursor: disabled ? 'wait' : 'pointer' };
}

export default Settings;