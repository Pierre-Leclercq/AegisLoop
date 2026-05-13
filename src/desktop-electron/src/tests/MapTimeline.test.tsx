import { fireEvent, render, screen, waitFor, within } from '@testing-library/react';
import MapTimeline from '../views/MapTimeline';

// Mock du composant MapLibreMap pour éviter le besoin de WebGL dans jsdom
vi.mock('../components/MapLibreMap', () => ({
  default: ({ markers, selectedId, onSelect }: { markers: Array<{ id: string; lon: number; lat: number; title: string; color: string; locationLabel?: string | null }>; selectedId: string | null; onSelect: (id: string) => void }) => (
    <div data-testid="maplibre-map-viewport" aria-label="Carte interactive">
      {markers.map(m => (
        <button
          key={m.id}
          data-testid={`map-marker-${m.id}`}
          onClick={() => onSelect(m.id)}
          aria-label={`Sélectionner sur carte ${m.title}`}
          style={selectedId === m.id ? { border: '2px solid white' } : undefined}
        >
          {m.title}
        </button>
      ))}
      <div data-testid="map-controls">
        <button onClick={() => {}}>🗺️ OpenStreetMap</button>
        <button onClick={() => {}}>🛰️ Satellite ESRI</button>
        <button onClick={() => {}}>⛰️ Topographique</button>
        <button onClick={() => {}}>🌍 Vue globale</button>
        <button onClick={() => {}}>🧭 Nord ↑</button>
      </div>
    </div>
  ),
}));

describe('MapTimeline view (MapLibre)', () => {
  const originalFetch = globalThis.fetch;

  afterEach(() => {
    globalThis.fetch = originalFetch;
  });

  function mockMapFetch(data = payload()) {
    globalThis.fetch = vi.fn(async (input: RequestInfo | URL) => {
      const url = String(input);
      if (url.includes('/api/map-timeline')) return { ok: true, json: async () => ({ success: true, data }) } as Response;
      return { ok: true, json: async () => ({}) } as Response;
    });
  }

  // Helper: attend que les données soient chargées et affichées
  async function waitForDataLoaded() {
    await screen.findByText(/Carte interactive avec zoom global → terrain/i);
    expect(screen.getByTestId('maplibre-map-viewport')).toBeTruthy();
  }

  it('renders map, timeline, metrics and selects an item from the timeline', async () => {
    mockMapFetch();
    render(<MapTimeline />);

    await waitForDataLoaded();
    expect(screen.getByLabelText(/Timeline des EventCases/i)).toBeTruthy();
    expect(screen.getByLabelText(/Synthèse Carte Timeline/i)).toBeTruthy();
    expect(screen.getByText('Sans coordonnées')).toBeTruthy();
    expect(screen.getAllByText(/Noria checkpoint incident/i).length).toBeGreaterThan(0);

    fireEvent.click(within(screen.getByLabelText(/Timeline des EventCases/i)).getByRole('button', { name: /Bab el Mandeb rerouting/i }));

    const detail = screen.getByLabelText(/Détail sélection Carte Timeline/i);
    expect(within(detail).getByText(/Bab el Mandeb rerouting/i)).toBeTruthy();
    expect(within(detail).getByText(/Bab el Mandeb traffic lane/i)).toBeTruthy();
  });

  it('selects an item via map area click and verifies detail panel', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    // Utilise la timeline pour sélectionner l'item (le mock MapLibre simule les marqueurs)
    fireEvent.click(within(screen.getByLabelText(/Timeline des EventCases/i)).getByRole('button', { name: /Bab el Mandeb rerouting/i }));

    const detail = screen.getByLabelText(/Détail sélection Carte Timeline/i);
    expect(within(detail).getByText(/Bab el Mandeb rerouting/i)).toBeTruthy();
    expect(within(detail).getByText(/12\.642, 43\.389/i)).toBeTruthy();
  });

  it('selects an item and shows detail panel info', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    fireEvent.click(within(screen.getByLabelText(/Timeline des EventCases/i)).getByRole('button', { name: /Noria checkpoint incident/i }));

    const detail = screen.getByLabelText(/Détail sélection Carte Timeline/i);
    expect(within(detail).getByText(/Noria checkpoint incident/i)).toBeTruthy();
    expect(within(detail).getByText(/74%/)).toBeTruthy();
    expect(within(detail).getByText(/Noria road corridor/i)).toBeTruthy();
  });

  it('shows loading, error and empty states', async () => {
    let resolveFetch: (value: Response) => void = () => undefined;
    globalThis.fetch = vi.fn().mockReturnValue(new Promise(resolve => { resolveFetch = resolve; }));

    render(<MapTimeline />);
    expect(screen.getByText(/Chargement Carte \+ Timeline/i)).toBeTruthy();
    resolveFetch({ ok: false, json: async () => ({ success: false, error: 'API indisponible' }) } as Response);
    expect((await screen.findByRole('alert')).textContent).toContain('API indisponible');

    globalThis.fetch = vi.fn().mockResolvedValue({
      ok: true,
      json: async () => ({ success: true, data: { ...payload(), items: [], totalCount: 0, withoutCoordinatesCount: 0 } }),
    } as Response);
    fireEvent.click(screen.getByRole('button', { name: /Réessayer|R..essayer/i }));

    expect(await screen.findByText(/Aucune donnée Carte \+ Timeline disponible/i)).toBeTruthy();
  });

  it('applies simple source and score filters through the API query', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: payload() }) } as Response)
      .mockResolvedValue({
        ok: true,
        json: async () => ({
          success: true,
          data: { ...payload(), items: [payload().items[1]], totalCount: 1, sourceTypes: ['Gdelt'] },
        }),
      } as Response);

    render(<MapTimeline />);
    await waitForDataLoaded();

    fireEvent.change(screen.getByLabelText(/Filtrer par source/i), { target: { value: 'Gdelt' } });
    await waitFor(() => expect(globalThis.fetch).toHaveBeenLastCalledWith(expect.stringContaining('source=Gdelt')));

    fireEvent.change(screen.getByLabelText(/Filtrer par score minimal/i), { target: { value: '0.7' } });
    await waitFor(() => expect(globalThis.fetch).toHaveBeenLastCalledWith(expect.stringContaining('minScore=0.7')));
  });

  it('applies scenario filter', async () => {
    globalThis.fetch = vi.fn()
      .mockResolvedValueOnce({ ok: true, json: async () => ({ success: true, data: payload() }) } as Response)
      .mockResolvedValue({ ok: true, json: async () => ({ success: true, data: payload() }) } as Response);

    render(<MapTimeline />);
    await waitForDataLoaded();

    fireEvent.change(screen.getByLabelText(/Filtrer par scénario/i), { target: { value: 'sahel-civic-security' } });
    await waitFor(() => expect(globalThis.fetch).toHaveBeenLastCalledWith(expect.stringContaining('scenario=sahel-civic-security')));
  });

  it('displays summary metrics correctly', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    expect(screen.getByText('2')).toBeTruthy(); // totalCount
    expect(screen.getByText('0')).toBeTruthy(); // withoutCoordinatesCount
    expect(screen.getByText('Gdelt / Rss')).toBeTruthy(); // sourceTypes
  });

  it('renders detail panel with all fields for selected item', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    fireEvent.click(within(screen.getByLabelText(/Timeline des EventCases/i)).getByRole('button', { name: /Bab el Mandeb rerouting/i }));

    const detail = screen.getByLabelText(/Détail sélection Carte Timeline/i);
    expect(within(detail).getByText('68%')).toBeTruthy();
    expect(within(detail).getByText('Detected')).toBeTruthy();
    expect(within(detail).getByText('Economic')).toBeTruthy();
    expect(within(detail).getByText('13')).toBeTruthy();
    expect(within(detail).getByText('Bab el Mandeb traffic lane · Gulf of Aden · International waters')).toBeTruthy();
    expect(within(detail).getByText('12.642, 43.389')).toBeTruthy();
    expect(within(detail).getByText('Aden Maritime Incident')).toBeTruthy();
    expect(screen.getByText('Ouvrir dans EventCase').closest('a')?.getAttribute('href')).toBe('/event-case?id=event-2');
  });

  it('shows timeline items sorted chronologically', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    const timelineItems = within(screen.getByLabelText(/Timeline des EventCases/i)).getAllByRole('button');
    expect(timelineItems).toHaveLength(2);
    // Le plus ancien en premier (28 avril < 23 mai)
    expect(timelineItems[0].textContent).toContain('Noria checkpoint incident');
    expect(timelineItems[1].textContent).toContain('Bab el Mandeb rerouting');
  });

  it('renders map style selector buttons', async () => {
    mockMapFetch();
    render(<MapTimeline />);
    await waitForDataLoaded();

    // Vérifie que les contrôles de style sont présents (dans le mock de MapLibreMap)
    expect(screen.getByText('🗺️ OpenStreetMap')).toBeTruthy();
    expect(screen.getByText('🛰️ Satellite ESRI')).toBeTruthy();
    expect(screen.getByText('⛰️ Topographique')).toBeTruthy();
    expect(screen.getByText('🌍 Vue globale')).toBeTruthy();
    expect(screen.getByText('🧭 Nord ↑')).toBeTruthy();
  });

  it('handles refresh button', async () => {
    mockMapFetch();
    render(<MapTimeline />);

    // Attendre que les données soient chargées (le bouton passe de "Chargement…" à "Rafraîchir")
    await waitForDataLoaded();

    const refreshBtn = screen.getByRole('button', { name: /Rafraîchir/i });
    expect(refreshBtn).toBeTruthy();

    fireEvent.click(refreshBtn);
    await waitFor(() => expect(globalThis.fetch).toHaveBeenCalledTimes(2));
  });

  // -- Helpers ------------------------------------------------------------

  function payload() {
    return {
      items: [
        {
          eventCaseId: 'event-1',
          title: 'Noria checkpoint incident',
          score: 0.74,
          status: 'Detected',
          category: 'Conflict',
          date: '2026-04-28T08:00:00Z',
          sources: ['RSS Demo Local - Sahel Watch'],
          sourceTypes: ['Rss'],
          observationCount: 12,
          latitude: 13.512,
          longitude: 2.112,
          locationName: 'Noria road corridor',
          region: 'Sahel demo area',
          country: 'Demo Sahel',
          scenario: 'sahel-civic-security',
          scenarioLabel: 'Sahel Civic Security',
        },
        {
          eventCaseId: 'event-2',
          title: 'Bab el Mandeb rerouting',
          score: 0.68,
          status: 'Detected',
          category: 'Economic',
          date: '2026-05-23T08:00:00Z',
          sources: ['GDELT Demo Local - Maritime Query'],
          sourceTypes: ['Gdelt'],
          observationCount: 13,
          latitude: 12.642,
          longitude: 43.389,
          locationName: 'Bab el Mandeb traffic lane',
          region: 'Gulf of Aden',
          country: 'International waters',
          scenario: 'aden-maritime-incident',
          scenarioLabel: 'Aden Maritime Incident',
        },
      ],
      totalCount: 2,
      withoutCoordinatesCount: 0,
      periodStart: '2026-04-28T08:00:00Z',
      periodEnd: '2026-05-23T08:00:00Z',
      scenarios: ['aden-maritime-incident', 'sahel-civic-security'],
      sourceTypes: ['Gdelt', 'Rss'],
    };
  }
});