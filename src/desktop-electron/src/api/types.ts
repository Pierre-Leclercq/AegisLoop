/** Types partagés pour l'API AegisLoop V1 — source unique de vérité frontend. */

// Enveloppe standard de réponse API
export type ApiEnvelope<T> = {
  success: boolean;
  data: T;
  error?: string | null;
  meta?: { totalCount: number };
};

// Dashboard
export type EventCaseSummary = {
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

export type DashboardData = {
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

// Observations
export type ObservationDto = {
  id: string;
  rawItemId?: string | null;
  title: string;
  content: string;
  claimText?: string | null;
  status: string;
  observedAt: string;
  sourceConnectorId: string;
  sourceType: string;
  sourceName: string;
  sourceUrl?: string | null;
  sourceReliability: number;
  language?: string | null;
};

// EventCase
export type EventCaseDetail = EventCaseSummary & {
  summary?: string | null;
  startedAt: string;
  createdAt: string;
  scoreBreakdown?: ScoreBreakdown | null;
  observations: ObservationDto[];
};

export type ScoreBreakdown = {
  targetId: string;
  targetType: string;
  value: number;
  calculatedAt: string;
  algorithmVersion: string;
  components: ScoreComponent[];
};

export type ScoreComponent = {
  name: string;
  value: number;
  weight: number;
  contribution: number;
  explanation: string;
};

// Map-Timeline
export type MapTimelineItem = {
  eventCaseId: string;
  title: string;
  score: number;
  status: string;
  category: string;
  date: string;
  sources: string[];
  sourceTypes: string[];
  observationCount: number;
  latitude?: number | null;
  longitude?: number | null;
  locationName?: string | null;
  region?: string | null;
  country?: string | null;
  scenario?: string | null;
  scenarioLabel?: string | null;
};

export type MapTimelinePayload = {
  items: MapTimelineItem[];
  totalCount: number;
  withoutCoordinatesCount: number;
  periodStart?: string | null;
  periodEnd?: string | null;
  scenarios: string[];
  sourceTypes: string[];
};

// Feedback
export type SubmitFeedbackRequest = {
  targetId: string;
  targetType: string;
  action: string;
  details?: string | null;
};

// Demo
export type DemoStatus = {
  datasetVersion: string;
  seedPath: string;
  loaded: boolean;
  connectors: number;
  rawItems: number;
  observations: number;
  eventCases: number;
  scores: number;
  feedbacks: number;
  auditEntries: number;
  scenarios: string[];
};

// Audit
export type AuditEntry = {
  id: string;
  date: string;
  category: string;
  action: string;
  targetType?: string | null;
  targetId?: string | null;
  message: string;
  level: string;
  actor: string;
};

// Connectors
export type SourceConnectorDto = {
  id: string;
  type: string;
  name: string;
  status: string;
  lastRunAt?: string | null;
  errorCount: number;
};

// Ingestion
export type IngestionJobDto = {
  id: string;
  connectorId: string;
  startedAt: string;
  completedAt?: string | null;
  status: string;
  itemsCollected: number;
  itemsNormalized: number;
  errorMessage?: string | null;
};