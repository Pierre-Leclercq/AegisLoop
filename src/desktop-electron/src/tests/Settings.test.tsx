import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import Settings from '../views/Settings';

describe('Settings demo mode', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  it('loads demo status and triggers seed/reset/rebuild/recalculate actions with mock API', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(false, 0, 0) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { status: demoStatus(true, 90, 8) } }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(true, 90, 8) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [{ id: 'audit-1', date: '2026-04-28T10:00:00Z', category: 'Configuration', action: 'DemoSeedLoaded', message: 'ok', level: 'Info', actor: 'system-demo' }] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { status: demoStatus(false, 0, 0) } }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(false, 0, 0) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: { eventCasesCreated: 8 } }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(true, 90, 8) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(true, 90, 8) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: demoStatus(true, 90, 8) }) } as Response)
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: [] }) } as Response);

    render(<Settings />);
    expect(await screen.findByText(/Mode démo seed\/replay V1/i)).toBeTruthy();
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/audit?take=200')));
    expect(await screen.findByText('Vide')).toBeTruthy();

    fireEvent.click(screen.getByRole('button', { name: /Charger seed/i }));
    await screen.findByText(/Chargement seed terminé/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/demo/load'), expect.objectContaining({ method: 'POST' })));

    fireEvent.click(screen.getByRole('button', { name: /Reset démo/i }));
    await screen.findByText(/Reset démo terminé/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/demo/reset'), expect.objectContaining({ method: 'POST' })));

    fireEvent.click(screen.getByRole('button', { name: /Rebuild EventCases/i }));
    await screen.findByText(/Rebuild EventCases terminé/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/demo/rebuild'), expect.objectContaining({ method: 'POST' })));

    fireEvent.click(screen.getByRole('button', { name: /Recalcul scores/i }));
    await screen.findByText(/Recalcul scores terminé/i);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledWith(expect.stringContaining('/api/demo/recalculate'), expect.objectContaining({ method: 'POST' })));
  });

  function demoStatus(loaded: boolean, observations: number, eventCases: number) {
    return {
      datasetVersion: 'v1-seed-2026-04',
      loaded,
      connectors: loaded ? 5 : 0,
      rawItems: observations,
      observations,
      eventCases,
      scores: loaded ? observations + eventCases : 0,
      feedbacks: loaded ? 5 : 0,
      auditEntries: loaded ? 2 : 0,
      scenarios: loaded ? ['sahel-civic-security', 'aden-maritime-incident'] : [],
    };
  }
});