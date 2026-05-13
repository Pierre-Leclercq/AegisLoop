import { render, screen } from '@testing-library/react';
import Dashboard from '../views/Dashboard';

describe('Dashboard view', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('loads and displays dashboard counters returned by the API', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({
        success: true,
        data: {
          connectors: 2,
          rawItems: 5,
          observations: 4,
          jobs: 3,
          events: 0,
          contradictions: 0,
          lastIngestion: {
            id: 'job-1',
            startedAt: '2026-04-28T10:00:00Z',
            completedAt: '2026-04-28T10:01:00Z',
            status: 'Completed',
            errorMessage: null,
          },
          recentErrors: [],
          recentEvents: [],
          highScoreEvents: [{
            id: 'event-1',
            title: 'Khartoum conflict incident',
            category: 'Conflict',
            status: 'Detected',
            updatedAt: '2026-04-28T10:02:00Z',
            observationCount: 2,
            corroborationCount: 2,
            score: 0.72,
            sources: ['RSS Démo', 'GDELT Démo'],
          }],
          categoryDistribution: [{ category: 'Conflict', count: 1 }],
          sourceDistribution: [{ source: 'Rss', count: 1 }],
        },
      }),
    } as Response);

    render(<Dashboard />);

    expect(await screen.findByText('Connecteurs')).toBeTruthy();
    expect(screen.getByText('2')).toBeTruthy();
    expect(screen.getByText('RawItems')).toBeTruthy();
    expect(screen.getByText('5')).toBeTruthy();
    expect(screen.getByText('Completed')).toBeTruthy();
    expect(screen.getByText('Khartoum conflict incident')).toBeTruthy();
    expect(screen.getByText('72%')).toBeTruthy();
    expect(screen.getByText(/Conflict : 1/)).toBeTruthy();
    expect(screen.getByText(/aucune erreur/i)).toBeTruthy();
  });

  it('shows an error state when the dashboard API fails', async () => {
    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: false,
      status: 500,
      json: async () => ({ success: false, error: 'boom' }),
    } as Response);

    render(<Dashboard />);

    expect((await screen.findByRole('alert')).textContent).toContain('API dashboard indisponible');
  });
});