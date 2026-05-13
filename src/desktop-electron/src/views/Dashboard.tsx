import React from 'react';
import { fetchDashboard } from '../api/aegisLoopApi';
import type { DashboardData } from '../api/types';
import LoadingState from '../components/LoadingState';
import ErrorState from '../components/ErrorState';
import ScoreBadge from '../components/ScoreBadge';

const Dashboard: React.FC = () => {
  const [data, setData] = React.useState<DashboardData | null>(null);
  const [loading, setLoading] = React.useState(true);
  const [error, setError] = React.useState<string | null>(null);

  const loadData = React.useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const envelope = await fetchDashboard();
      if (!envelope.success) throw new Error(envelope.error ?? 'Réponse invalide');
      setData(envelope.data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erreur inconnue');
    } finally {
      setLoading(false);
    }
  }, []);

  React.useEffect(() => {
    void loadData();
  }, [loadData]);

  if (loading) return <LoadingState />;
  if (error) return <ErrorState message={error} onRetry={loadData} />;
  if (!data) return (
    <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--text-secondary)' }}>
      Aucune donnée dashboard disponible.
    </div>
  );

  const cards = [
    { label: 'Connecteurs', value: data.connectors, color: 'var(--accent)' },
    { label: 'RawItems', value: data.rawItems, color: 'var(--text-primary)' },
    { label: 'Observations', value: data.observations, color: 'var(--success)' },
    { label: 'EventCases', value: data.events, color: 'var(--warning)' },
  ];

  return (
    <div>
      <h1 style={{ color: 'var(--accent)', marginBottom: '1rem' }}>Dashboard</h1>
      <p style={{ color: 'var(--text-secondary)' }}>Vue synthétique alimentée par /api/dashboard.</p>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1rem', marginTop: '1.5rem' }}>
        {cards.map(card => (
          <div key={card.label} style={{ background: 'var(--bg-secondary)', padding: '1.2rem', borderRadius: '8px', border: '1px solid #2a2a3e' }}>
            <div style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', marginBottom: '0.5rem' }}>{card.label}</div>
            <div style={{ color: card.color, fontSize: '1.8rem', fontWeight: 'bold' }}>{card.value}</div>
          </div>
        ))}
      </div>

      <section style={sectionStyle}>
        <h2 style={h2Style}>EventCases prioritaires</h2>
        {data.highScoreEvents.length === 0 ? <EmptyMessage>Aucun EventCase reconstruit.</EmptyMessage> : (
          <div style={{ display: 'grid', gap: '0.5rem' }}>
            {data.highScoreEvents.map(ec => (
              <article key={ec.id} style={{ border: '1px solid #2a2a3e', borderRadius: '6px', padding: '0.75rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', gap: '1rem' }}>
                  <strong>{ec.title}</strong>
                  <ScoreBadge score={ec.score} />
                </div>
                <p style={{ color: 'var(--text-secondary)', margin: '0.4rem 0 0' }}>
                  {ec.observationCount} observation(s) · {ec.sources.join(', ') || 'sources inconnues'} · {ec.category}
                </p>
              </article>
            ))}
          </div>
        )}
      </section>

      <section style={sectionStyle}>
        <h2 style={h2Style}>Répartition EventCases</h2>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '1rem' }}>
          <div>
            <h3 style={h3Style}>Par catégorie</h3>
            {data.categoryDistribution.length === 0 ? <EmptyMessage>Aucune catégorie.</EmptyMessage> : (
              <ul>{data.categoryDistribution.map(i => <li key={i.category}>{i.category} : {i.count}</li>)}</ul>
            )}
          </div>
          <div>
            <h3 style={h3Style}>Par source</h3>
            {data.sourceDistribution.length === 0 ? <EmptyMessage>Aucune source.</EmptyMessage> : (
              <ul>{data.sourceDistribution.map(i => <li key={i.source}>{i.source} : {i.count}</li>)}</ul>
            )}
          </div>
        </div>
      </section>

      <section style={sectionStyle}>
        <h2 style={h2Style}>Dernière ingestion</h2>
        {data.lastIngestion ? (
          <dl style={{ display: 'grid', gridTemplateColumns: 'max-content 1fr', gap: '0.5rem 1rem', margin: 0, color: 'var(--text-secondary)' }}>
            <dt>Statut</dt><dd style={{ margin: 0 }}>{data.lastIngestion.status}</dd>
            <dt>Démarrage</dt><dd style={{ margin: 0 }}>{new Date(data.lastIngestion.startedAt).toLocaleString()}</dd>
            <dt>Fin</dt><dd style={{ margin: 0 }}>{data.lastIngestion.completedAt ? new Date(data.lastIngestion.completedAt).toLocaleString() : 'en cours'}</dd>
          </dl>
        ) : <EmptyMessage>Aucun job d'ingestion enregistré.</EmptyMessage>}
      </section>

      <section style={sectionStyle}>
        <h2 style={h2Style}>Erreurs récentes</h2>
        {data.recentErrors.length === 0 ? <EmptyMessage>Aucune erreur récente.</EmptyMessage> : (
          <ul style={{ color: '#ffb3b3', marginBottom: 0 }}>
            {data.recentErrors.map(item => <li key={item.id}>{item.errorMessage}</li>)}
          </ul>
        )}
      </section>
    </div>
  );
};

// Shared sub-components and style helpers
const sectionStyle: React.CSSProperties = { background: 'var(--bg-secondary)', border: '1px solid #2a2a3e', borderRadius: '8px', padding: '1rem', marginTop: '1rem' };
const h2Style: React.CSSProperties = { marginTop: 0, fontSize: '1.05rem' };
const h3Style: React.CSSProperties = { fontSize: '0.95rem' };

const EmptyMessage: React.FC<{ children: React.ReactNode }> = ({ children }) => (
  <p style={{ color: 'var(--text-secondary)' }}>{children}</p>
);

export default Dashboard;