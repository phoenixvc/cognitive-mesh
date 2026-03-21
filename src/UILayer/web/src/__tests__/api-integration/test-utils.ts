/**
 * API Integration Test Setup
 *
 * Provides a lightweight fetch mock for testing API integration flows
 * without requiring MSW (Mock Service Worker).
 *
 * Usage:
 *   import { mockFetch, resetFetchMock } from "./setup";
 *
 *   beforeEach(() => resetFetchMock());
 *
 *   it("should call API", async () => {
 *     mockFetch("/api/v1/agent/registry", {
 *       status: 200,
 *       body: [{ agentId: "1", name: "Agent" }],
 *     });
 *     // ... invoke code that calls fetch ...
 *   });
 */

interface MockResponse {
  status?: number;
  body?: unknown;
  headers?: Record<string, string>;
}

const mockResponses = new Map<string, MockResponse>();
const fetchCalls: { url: string; init?: RequestInit }[] = [];

/**
 * Register a mock response for a given URL pattern.
 * The URL is matched as a substring of the full request URL.
 */
export function mockFetch(urlPattern: string, response: MockResponse): void {
  mockResponses.set(urlPattern, response);
}

/**
 * Reset all mock responses and recorded calls.
 */
export function resetFetchMock(): void {
  mockResponses.clear();
  fetchCalls.length = 0;
  global.fetch = jest.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = typeof input === "string" ? input : input.toString();
    fetchCalls.push({ url, init });

    for (const [pattern, response] of mockResponses) {
      if (url.includes(pattern)) {
        return {
          ok: (response.status ?? 200) >= 200 && (response.status ?? 200) < 300,
          status: response.status ?? 200,
          headers: new Headers(response.headers ?? {}),
          json: async () => response.body,
          text: async () => JSON.stringify(response.body),
        } as Response;
      }
    }

    // Default: 404 for unmocked endpoints
    return {
      ok: false,
      status: 404,
      headers: new Headers(),
      json: async () => ({ error: "Not Found", url }),
      text: async () => `Not Found: ${url}`,
    } as Response;
  }) as jest.Mock;
}

/**
 * Get all fetch calls recorded during the test.
 */
export function getFetchCalls(): { url: string; init?: RequestInit }[] {
  return [...fetchCalls];
}

/**
 * Get fetch calls that match a URL pattern.
 */
export function getFetchCallsMatching(
  urlPattern: string
): { url: string; init?: RequestInit }[] {
  return fetchCalls.filter((c) => c.url.includes(urlPattern));
}
