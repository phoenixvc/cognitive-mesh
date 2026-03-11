/**
 * API helper for Phase 15b widget dashboards.
 *
 * These endpoints are not yet in the auto-generated OpenAPI types, so we use
 * a typed fetch wrapper that reads the same NEXT_PUBLIC_API_BASE_URL env var
 * as the openapi-fetch client in `@/lib/api/client`.
 *
 * Once the OpenAPI spec is regenerated, migrate callers to `servicesApi.GET(...)`.
 */

function getApiBaseUrl(): string {
  const url = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (url) return url;
  return 'http://localhost:5000';
}

const BASE = getApiBaseUrl();

async function fetchJson<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...init?.headers },
    ...init,
  });
  if (!res.ok) {
    const text = await res.text().catch(() => res.statusText);
    throw new Error(`API ${res.status}: ${text}`);
  }
  return res.json() as Promise<T>;
}

// ───────────────────────────── NIST Compliance ─────────────────────────────

import type {
  NISTScoreResponse,
  NISTRoadmapResponse,
  NISTChecklistResponse,
  NISTAuditLogResponse,
  BalanceResponse,
  SpectrumHistoryResponse,
  ReflexionStatusResponse,
  ValueDiagnosticResponse,
  OrgBlindnessDetectionResponse,
  PsychologicalSafetyScore,
  ImpactReport,
  ResistanceIndicator,
  SandwichProcess,
  PhaseAuditEntry,
  CognitiveDebtAssessment,
} from './types';

export async function getNistScore(organizationId: string): Promise<NISTScoreResponse> {
  return fetchJson(`/api/v1/nist-compliance/organizations/${encodeURIComponent(organizationId)}/score`);
}

export async function getNistRoadmap(organizationId: string): Promise<NISTRoadmapResponse> {
  return fetchJson(`/api/v1/nist-compliance/organizations/${encodeURIComponent(organizationId)}/roadmap`);
}

export async function getNistChecklist(organizationId: string): Promise<NISTChecklistResponse> {
  return fetchJson(`/api/v1/nist-compliance/organizations/${encodeURIComponent(organizationId)}/checklist`);
}

export async function getNistAuditLog(organizationId: string, maxResults = 50): Promise<NISTAuditLogResponse> {
  return fetchJson(`/api/v1/nist-compliance/organizations/${encodeURIComponent(organizationId)}/audit-log?maxResults=${maxResults}`);
}

// ──────────────────────────── Adaptive Balance ──────────────────────────────

export async function getAdaptiveBalance(context: Record<string, string> = {}): Promise<BalanceResponse> {
  return fetchJson('/api/v1/adaptive-balance/balance', {
    method: 'POST',
    body: JSON.stringify({ context }),
  });
}

export async function getSpectrumHistory(dimension: string): Promise<SpectrumHistoryResponse> {
  return fetchJson(`/api/v1/adaptive-balance/history/${encodeURIComponent(dimension)}`);
}

export async function getReflexionStatus(): Promise<ReflexionStatusResponse> {
  return fetchJson('/api/v1/adaptive-balance/reflexion-status');
}

// ─────────────────────────── Value Generation ───────────────────────────────

export async function runValueDiagnostic(
  targetId: string,
  targetType: string,
  tenantId: string,
): Promise<ValueDiagnosticResponse> {
  return fetchJson('/api/v1/ValueGeneration/value-diagnostic', {
    method: 'POST',
    body: JSON.stringify({ targetId, targetType, tenantId }),
  });
}

export async function detectOrgBlindness(
  organizationId: string,
  tenantId: string,
  departmentFilters: string[] = [],
): Promise<OrgBlindnessDetectionResponse> {
  return fetchJson('/api/v1/ValueGeneration/org-blindness/detect', {
    method: 'POST',
    body: JSON.stringify({ organizationId, tenantId, departmentFilters }),
  });
}

// ──────────────────────────── Impact Metrics ────────────────────────────────

export async function getSafetyScoreHistory(
  teamId: string,
  tenantId: string,
): Promise<PsychologicalSafetyScore[]> {
  return fetchJson(`/api/v1/impact-metrics/safety-score/${encodeURIComponent(teamId)}/history?tenantId=${encodeURIComponent(tenantId)}`);
}

export async function getImpactReport(
  tenantId: string,
  periodStart?: string,
  periodEnd?: string,
): Promise<ImpactReport> {
  const params = new URLSearchParams();
  if (periodStart) params.set('periodStart', periodStart);
  if (periodEnd) params.set('periodEnd', periodEnd);
  const qs = params.toString();
  return fetchJson(`/api/v1/impact-metrics/report/${encodeURIComponent(tenantId)}${qs ? `?${qs}` : ''}`);
}

export async function getResistancePatterns(tenantId: string): Promise<ResistanceIndicator[]> {
  return fetchJson(`/api/v1/impact-metrics/telemetry/${encodeURIComponent(tenantId)}/resistance`);
}

// ────────────────────────── Cognitive Sandwich ──────────────────────────────

export async function getSandwichProcess(processId: string): Promise<SandwichProcess> {
  return fetchJson(`/api/v1/cognitive-sandwich/${encodeURIComponent(processId)}`);
}

export async function getSandwichAuditTrail(processId: string): Promise<PhaseAuditEntry[]> {
  return fetchJson(`/api/v1/cognitive-sandwich/${encodeURIComponent(processId)}/audit`);
}

export async function getSandwichDebt(processId: string): Promise<CognitiveDebtAssessment> {
  return fetchJson(`/api/v1/cognitive-sandwich/${encodeURIComponent(processId)}/debt`);
}
