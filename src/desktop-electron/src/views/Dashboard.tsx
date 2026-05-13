import React from 'react';

type DashboardData = {
  connectors: number;
  rawItems: number;
  observations: number;
  jobs: number;
  events: number;
  contradictions: number;
  lastIngestion?: {
    id: string;
    startedAt: string;
    completedAt?: string | null;
    status: string;
    errorMessage?: string | null;
  } | null;
  recentErrors: Array<{ id: string; startedAt: string; errorMessage: string }>;
  recentEvents: EventCaseSummary[];
  highScoreEvents: EventCaseSummary[];
  categoryDistribution: Array<{ category: string; count: number }>;
  sourceDistribution: Array<{ source: string; count: number }>;
};

type EventCaseSummary = {
  id: string;
  title: string;
  category: string;
  status: string;
  updatedAt: string;
  observationCount: number;
  corroborationCount: number;
  score: number;
  sources: string[];
};

type ApiEnvelope<T> = {
  success: boolean;
  data: T;
  error?: string | null;
};

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';

const Dashboard: React.FC = () => {
  const [data, setData] = React.useState<DashboardData | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  React.useEffect(() => {
    let cancelled = false;

    const loadDashboard = async () => {
      setLoading(true);
      setError(null);

      try {
        const response = await fetch(`${API_BASE_URL}/api/dashboard`);
        if (!response.ok) {
          throw new Error(`API dashboard indisponible (${response.status})`);
        }

        const envelope = await response.json() as ApiEnvelope<DashboardData>;
        if (!envelope.success) {
          throw new Error(envelope.error ?? 'Réponse API dashboard invalide');
        }

        if (!cancelled) {
          setData(envelope.data);
        }
      } catch (loadError) {
        if (!cancelled) {
          setError(loadError instanceof Error ? loadError.message : 'Erreur inconnue');
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    };

    void loadDashboard();

    return () => {
      cancelled = true;
    };
  }, []);

  const cards = data ? [
    { label: 'Connecteurs', value: data.connectors, color: 'var(--accent)' },
    { label: 'RawItems', value: data.rawItems, color: 'var(--text-primary)' },
    { label: 'Observations', value: data.observations, color: 'var(--success)' },
    { label: 'EventCases', value: data.events, color: 'var(--warning)' },
  ] : [];

  return (
    <div>
      <h1 style={{ color: 'var(--accent)', marginBottom: '1rem' }}>Dashboard</h1>
      <p style={{ color: 'var(--text-secondary)' }}>
        Vue synthétique alimentée par l’API réelle /api/dashboard.
      </p>

      {loading && <p style={{ color: 'var(--text-secondary)' }}>Chargement du dashboard…</p>}
      {error && <p role="alert" style={{ color: '#ff7676' }}>{error}</p>}
      {!loading && !error && data === null && (
        <div style={{ background: 'var(--bg-secondary)', borderRadius: '8px', padding: '2rem', border: '1px solid #2a2a3e', marginTop: '1rem' }}>
          <p style={{ color: 'var(--text-secondary)' }}>Aucune donnée dashboard disponible.</p>
        </div>
      )}

      {data && (
        <>
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))',
        gap: '1rem',
        marginTop: '1.5rem',
      }}>
        {cards.map(card => (
          <div key={card.label} style={{
            background: 'var(--bg-secondary)',
            padding: '1.2rem',
            borderRadius: '8px',
            border: '1px solid #2a2a3e',
          }}>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>
              {card.label}
            </div>
            <div style={{ color: card.color, fontSize: '1.8rem', fontWeight: 'bold' }}>
              {card.value}
            </div>
          </div>
        ))}
      </div>

      <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', marginTop: '1rem' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>EventCases prioritaires</h2>
        {data.highScoreEvents.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>Aucun EventCase reconstruit pour le moment.</p>
        ) : (
          <div style={{ display: 'grid', gap: '0.5rem' }}>
            {data.highScoreEvents.map(eventCase => (
              <article key={eventCase.id} style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem' }}>
                  <strong>{eventCase.title}</strong>
                  <span style={{ color: scoreColor(eventCase.score), fontWeight: 'bold' }}>{formatScore(eventCase.score)}</span>
                </div>
                <p style={{ color: 'var(--text-secondary)', margin: '0.4rem 0 0' }}>
                  {eventCase.observationCount} observation(s) · {eventCase.sources.join(', ') || 'sources inconnues'} · {eventCase.category}
                </p>
              </article>
            ))}
          </div>
        )}
      </section>

      <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', marginTop: '1rem' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>Répartition EventCases</h2>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '1rem' }}>
          <div>
            <h3 style={{ fontSize: '0.95rem' }}>Par catégorie</h3>
            {data.categoryDistribution.length === 0 ? <p style={{ color: 'var(--text-secondary)' }}>Aucune catégorie.</p> : (
              <ul>{data.categoryDistribution.map(item => <li key={item.category}>{item.category} : {item.count}</li>)}</ul>
            )}
          </div>
          <div>
            <h3 style={{ fontSize: '0.95rem' }}>Par source</h3>
            {data.sourceDistribution.length === 0 ? <p style={{ color: 'var(--text-secondary)' }}>Aucune source liée.</p> : (
              <ul>{data.sourceDistribution.map(item => <li key={item.source}>{item.source} : {item.count}</li>)}</ul>
            )}
          </div>
        </div>
      </section>

      <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', marginTop: '1rem' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>Dernière ingestion</h2>
        {data.lastIngestion ? (
          <dl style={{ display: 'grid', gridTemplateColumns: 'max-content 1fr', gap: '0.5rem 1rem', margin: 0, color: 'var(--text-secondary)' }}>
            <dt>Statut</dt><dd style={{ margin: 0 }}>{data.lastIngestion.status}</dd>
            <dt>Démarrage</dt><dd style={{ margin: 0 }}>{new Date(data.lastIngestion.startedAt).toLocaleString()}</dd>
            <dt>Fin</dt><dd style={{ margin: 0 }}>{data.lastIngestion.completedAt ? new Date(data.lastIngestion.completedAt).toLocaleString() : 'en cours'}</dd>
          </dl>
        ) : (
          <p style={{ color: 'var(--text-secondary)' }}>Aucun job d’ingestion enregistré.</p>
        )}
      </section>

      <section style={{ background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', marginTop: '1rem' }}>
        <h2 style={{ marginTop: 0, fontSize: '1.05rem' }}>Erreurs récentes</h2>
        {data.recentErrors.length === 0 ? (
          <p style={{ color: 'var(--text-secondary)' }}>Aucune erreur d’ingestion récente.</p>
        ) : (
          <ul style={{ color: '#ffb3b3', marginBottom: 0 }}>
            {data.recentErrors.map(item => <li key={item.id}>{item.errorMessage}</li>)}
          </ul>
        )}
      </section>
        </>
      )}
    </div>
  );
};

export default Dashboard;

function formatScore(score: number): string {
  return `${Math.round(score * 100)}%`;
}

function scoreColor(score: number): string {
  if (score >= 0.7) return 'var(--success)';
  if (score >= 0.45) return 'var(--warning)';
  return '#ff7676';
}