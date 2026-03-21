/**
 * Cognitive Mesh API Clients
 *
 * Type-safe API clients backed by openapi-fetch + auto-generated types.
 * Usage:
 *   import { servicesApi, agenticApi } from '@/lib/api/client';
 *   const { data, error } = await servicesApi.GET('/champion-discovery/matches');
 */

import createClient, { type Middleware } from 'openapi-fetch';
import type { paths as ServicePaths } from './generated/services';
import type { paths as AgenticPaths } from './generated/agentic';

function getApiBaseUrl(): string {
  const url = process.env.NEXT_PUBLIC_API_BASE_URL;
  if (url) return url;
  // During build/SSR prerendering, env vars may not be available.
  // Fall back to localhost; the real value is injected at runtime.
  return 'http://localhost:5000';
}

const API_BASE_URL = getApiBaseUrl();

/** Services API — champion discovery, community pulse, learning, innovation, approvals, provenance, notifications */
export const servicesApi = createClient<ServicePaths>({
  baseUrl: `${API_BASE_URL}/api/v1`,
});

/** Agentic AI API — agent registry, orchestration, authority, consent */
export const agenticApi = createClient<AgenticPaths>({
  baseUrl: `${API_BASE_URL}/api/v1/agent`,
});

let authMiddleware: Middleware | null = null;

/**
 * Attach a bearer token to all requests.
 * Call this after login / token refresh.
 */
export function setAuthToken(token: string) {
  if (authMiddleware) {
    servicesApi.eject(authMiddleware);
    agenticApi.eject(authMiddleware);
  }
  authMiddleware = {
    async onRequest({ request }) {
      request.headers.set('Authorization', `Bearer ${token}`);
      return request;
    },
  };
  servicesApi.use(authMiddleware);
  agenticApi.use(authMiddleware);
}

/**
 * Clear auth tokens (on logout).
 */
export function clearAuthToken() {
  if (authMiddleware) {
    servicesApi.eject(authMiddleware);
    agenticApi.eject(authMiddleware);
    authMiddleware = null;
  }
}
