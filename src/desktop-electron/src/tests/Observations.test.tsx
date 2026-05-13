import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import Observations from '../views/Observations';

describe('Observations view', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('loads and displays observations returned by the API', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [observation('obs-1', 'RSS Test Title')] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: provenance('obs-1', 'RSS Test Title') }) } as Response);

    render(<Observations />);

    expect((await screen.findAllByText('RSS Test Title')).length).toBeGreaterThan(0);
    expect(screen.getAllByText(/RSS Démo/).length).toBeGreaterThan(0);
    expect((await screen.findAllByText(/Provenance réelle/i)).length).toBeGreaterThan(0);
    expect(screen.getAllByText(/hash-obs-1/i).length).toBeGreaterThan(0);
    expect(screen.getByRole('link', { name: /ouvrir/i })).toBeTruthy();
  });

  it('submits analyst feedback and reloads provenance', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [observation('obs-1', 'RSS Test Title')] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: provenance('obs-1', 'RSS Test Title') }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { feedback: { id: 'fb-1' } } }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { ...provenance('obs-1', 'RSS Test Title'), feedbacks: [{ id: 'fb-1', action: 'Confirm', details: 'ok', createdAt: '2026-04-28T10:10:00Z' }] } }) } as Response);

    render(<Observations />);
    await screen.findByText(/Provenance réelle/i);
    fireEvent.change(screen.getByLabelText(/Note feedback/i), { target: { value: 'ok' } });
    fireEvent.click(screen.getByRole('button', { name: 'Confirm' }));

    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/feedback'),
      expect.objectContaining({ method: 'POST' }),
    ));
    expect((await screen.findAllByText(/Confirm/)).length).toBeGreaterThan(0);
  });

  it('triggers RSS ingestion then reloads observations', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { itemsCreated: 1 } }) } as Response)
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({
          success: true,
          data: [observation('obs-2', 'Observation après ingestion')],
        }),
      } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: provenance('obs-2', 'Observation après ingestion') }) } as Response);

    render(<Observations />);
    await screen.findByText(/aucune observation persistée/i);

    fireEvent.click(screen.getByRole('button', { name: /déclencher rss démo/i }));

    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/ingestion/rss/run'),
      expect.objectContaining({ method: 'POST' }),
    ));
    expect((await screen.findAllByText('Observation après ingestion')).length).toBeGreaterThan(0);
  });

  function observation(id: string, title: string) {
    return { id, title, content: 'RSS Test Content', status: 'New', observedAt: '2026-04-28T10:00:00Z', sourceType: 'Rss', sourceName: 'RSS Démo', sourceUrl: 'https://example.test/article' };
  }

  function provenance(id: string, title: string) {
    return {
      observationId: id,
      title,
      sourceUrl: 'https://example.test/article',
      sourceConnector: { name: 'RSS Démo', connectorType: 'Rss', status: 'Active' },
      rawItem: { id: `raw-${id}`, sourceHash: `hash-${id}`, contentType: 'Json', collectedAt: '2026-04-28T10:00:00Z', metadata: { feedTitle: 'Feed' } },
      ingestionJob: { id: 'job-1', startedAt: '2026-04-28T09:59:00Z', status: 'Completed', itemsCollected: 1, itemsNormalized: 1 },
      score: { value: 0.6, algorithmVersion: 'V1-heuristic-3c-2026-04', components: [{ name: 'AnalystFeedback', value: 0.5, weight: 0.3, contribution: 0.15, explanation: 'Feedback neutre' }] },
      feedbacks: [],
      metadata: { source: 'rss' },
    };
  }
});