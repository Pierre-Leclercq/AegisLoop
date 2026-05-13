import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import EventCase from '../views/EventCase';

describe('EventCase view', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('loads EventCases, opens detail and displays linked observations and score breakdown', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ success: true, data: [eventSummary()] }),
      } as Response)
      .mockResolvedValueOnce({
        ok: true,
        json: async () => ({ success: true, data: eventDetail() }),
      } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: eventProvenance() }) } as Response);

    render(<EventCase />);

    expect(await screen.findAllByText('Khartoum conflict incident')).toHaveLength(2);
    expect(await screen.findByText(/Breakdown du score/i)).toBeTruthy();
    expect(screen.getByText(/SourceReliability/)).toBeTruthy();
    expect(screen.getByText(/Corroboration/)).toBeTruthy();
    expect(screen.getByText(/Provenance agrégée/)).toBeTruthy();
    expect(screen.getAllByText(/hash-obs-1/).length).toBeGreaterThan(0);
    expect(screen.getAllByText('Observation liée RSS').length).toBeGreaterThan(0);
    expect(screen.getAllByText(/RSS Démo/).length).toBeGreaterThan(0);
  });

  it('triggers EventCase rebuild then reloads events', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { eventCasesCreated: 1 } }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [eventSummary()] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: eventDetail() }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: eventProvenance() }) } as Response);

    render(<EventCase />);
    await screen.findByText(/Aucun EventCase/i);

    fireEvent.click(screen.getByRole('button', { name: /Reconstruire EventCases/i }));

    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(
      expect.stringContaining('/api/events/rebuild'),
      expect.objectContaining({ method: 'POST' }),
    ));
    expect((await screen.findAllByText('Khartoum conflict incident')).length).toBeGreaterThan(0);
  });

  it('exports selected EventCase as JSON and Markdown with mock API', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [eventSummary()] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: eventDetail() }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: eventProvenance() }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { eventCase: { id: 'event-1' }, provenance: { observations: [] }, limits: [] } }) } as Response)
      .mockResolvedValueOnce({ ok: true, text: async () => '# Khartoum conflict incident\n\n## Limites / incertitudes\n- test' } as Response);

    render(<EventCase />);
    await screen.findByRole('button', { name: /Exporter JSON/i });

    fireEvent.click(screen.getByRole('button', { name: /Exporter JSON/i }));
    await screen.findByText(/Export JSON généré/i);
    await screen.findByText(/aegisloop-eventcase-event-1\.json/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/export/event-1?format=json')));

    fireEvent.click(screen.getByRole('button', { name: /Exporter Markdown/i }));
    await screen.findByText(/Export Markdown généré/i);
    await screen.findByText(/aegisloop-eventcase-event-1\.md/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/export/event-1?format=markdown')));
  });

  function eventSummary() {
    return {
      id: 'event-1',
      title: 'Khartoum conflict incident',
      summary: 'EventCase V1 généré',
      category: 'Conflict',
      status: 'Detected',
      startedAt: '2026-04-28T10:00:00Z',
      createdAt: '2026-04-28T10:02:00Z',
      updatedAt: '2026-04-28T10:02:00Z',
      observationCount: 2,
      corroborationCount: 2,
      score: 0.72,
      sources: ['RSS Démo', 'GDELT Démo'],
      scoreBreakdown: null,
    };
  }

  function eventDetail() {
    return {
      ...eventSummary(),
      scoreBreakdown: {
        targetId: 'event-1',
        targetType: 'EventCase',
        value: 0.72,
        calculatedAt: '2026-04-28T10:02:00Z',
        algorithmVersion: 'V1-heuristic-3c-2026-04',
        components: [
          { name: 'SourceReliability', value: 0.7, weight: 0.35, contribution: 0.245, explanation: 'Fiabilité source' },
          { name: 'Corroboration', value: 0.66, weight: 0.35, contribution: 0.231, explanation: 'Sources indépendantes' },
          { name: 'AnalystFeedback', value: 0.5, weight: 0.3, contribution: 0.15, explanation: 'Feedback neutre' },
        ],
      },
      observations: [{
        id: 'obs-1',
        title: 'Observation liée RSS',
        content: 'Contenu observation liée',
        status: 'New',
        observedAt: '2026-04-28T10:00:00Z',
        sourceType: 'Rss',
        sourceName: 'RSS Démo',
        sourceUrl: 'https://example.test/rss',
      }],
    };
  }

  function eventProvenance() {
    return {
      eventCaseId: 'event-1',
      observations: [{ observationId: 'obs-1', title: 'Observation liée RSS', sourceConnector: { name: 'RSS Démo', connectorType: 'Rss' }, rawItem: { id: 'raw-1', sourceHash: 'hash-obs-1', collectedAt: '2026-04-28T10:00:00Z' }, feedbacks: [] }],
      sources: [{ id: 'source-1', name: 'RSS Démo', connectorType: 'Rss' }],
      rawItems: [{ id: 'raw-1', sourceHash: 'hash-obs-1', collectedAt: '2026-04-28T10:00:00Z' }],
      hashes: ['hash-obs-1'],
      feedbacks: [],
      score: eventDetail().scoreBreakdown,
    };
  }
});