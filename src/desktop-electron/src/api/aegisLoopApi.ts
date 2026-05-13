import type {
  ApiEnvelope,
  AuditEntry,
  DashboardData,
  DemoStatus,
  EventCaseDetail,
  EventCaseSummary,
  IngestionJobDto,
  MapTimelinePayload,
  ObservationDto,
  SourceConnectorDto,
  SubmitFeedbackRequest,
} from './types';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5100';

async function get<T>(path: string): Promise<ApiEnvelope<T>> {
  const response = await fetch(`${API_BASE_URL}${path}`);
  if (!response.ok) {
    const body = await response.json().catch(() => null);
    throw new Error(body?.error ?? `HTTP ${response.status}`);
  }
  return response.json();
}

async function post<T>(path: string, body?: unknown): Promise<ApiEnvelope<T>> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!response.ok) {
    const errorBody = await response.json().catch(() => null);
    throw new Error(errorBody?.error ?? `HTTP ${response.status}`);
  }
  return response.json();
}

// --- Dashboard ---
export function fetchDashboard(): Promise<ApiEnvelope<DashboardData>> {
  return get<DashboardData>('/api/dashboard');
}

// --- Observations ---
export function fetchObservations(take = 100): Promise<ApiEnvelope<ObservationDto[]>> {
  return get<ObservationDto[]>(`/api/observations?take=${take}`);
}

export function fetchObservation(id: string): Promise<ApiEnvelope<ObservationDto>> {
  return get<ObservationDto>(`/api/observations/${id}`);
}

// --- EventCases ---
export function fetchEventCases(take = 100): Promise<ApiEnvelope<EventCaseSummary[]>> {
  return get<EventCaseSummary[]>(`/api/events?take=${take}`);
}

export function fetchEventCase(id: string): Promise<ApiEnvelope<EventCaseDetail>> {
  return get<EventCaseDetail>(`/api/events/${id}`);
}

// --- Map-Timeline ---
export function fetchMapTimeline(filters?: {
  source?: string;
  minScore?: number;
  scenario?: string;
}): Promise<ApiEnvelope<MapTimelinePayload>> {
  const params = new URLSearchParams();
  if (filters?.source && filters.source !== 'all') params.set('source', filters.source);
  if (filters?.minScore !== undefined && filters.minScore > 0) params.set('minScore', String(filters.minScore));
  if (filters?.scenario && filters.scenario !== 'all') params.set('scenario', filters.scenario);
  const qs = params.toString();
  return get<MapTimelinePayload>(`/api/map-timeline${qs ? `?${qs}` : ''}`);
}

// --- Feedback ---
export function submitFeedback(request: SubmitFeedbackRequest): Promise<ApiEnvelope<unknown>> {
  return post<unknown>('/api/feedback', request);
}

// --- Demo ---
export function fetchDemoStatus(): Promise<ApiEnvelope<DemoStatus>> {
  return get<DemoStatus>('/api/demo/status');
}

export function loadDemoSeed(): Promise<ApiEnvelope<unknown>> {
  return post<unknown>('/api/demo/load');
}

export function resetDemo(): Promise<ApiEnvelope<unknown>> {
  return post<unknown>('/api/demo/reset');
}

// --- Audit ---
export function fetchAudit(take = 100): Promise<ApiEnvelope<AuditEntry[]>> {
  return get<AuditEntry[]>(`/api/audit?take=${take}`);
}

// --- Connectors ---
export function fetchConnectors(): Promise<ApiEnvelope<SourceConnectorDto[]>> {
  return get<SourceConnectorDto[]>('/api/connectors');
}

// --- Ingestion ---
export function runIngestion(connectorType: 'Rss' | 'Gdelt'): Promise<ApiEnvelope<IngestionJobDto>> {
  return post<IngestionJobDto>(`/api/ingestion/${connectorType.toLowerCase()}/run`);
}