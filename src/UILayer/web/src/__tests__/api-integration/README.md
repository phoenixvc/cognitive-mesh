# API Integration Tests

This directory contains integration-level tests that verify the frontend's interaction with backend API endpoints.

## Approach

Tests use **lightweight fetch mocks** rather than MSW (Mock Service Worker). The `test-utils.ts` module provides:

- `mockFetch(urlPattern, response)` — Register a mock response for any fetch URL containing `urlPattern`
- `resetFetchMock()` — Clear all mocks and start fresh (call in `beforeEach`)
- `getFetchCalls()` — Inspect recorded fetch calls for assertions
- `getFetchCallsMatching(urlPattern)` — Filter recorded calls by URL pattern

For tests that go through Zustand stores (which use `openapi-fetch` clients), mock the `@/lib/api/client` module directly with `jest.mock()` as shown in `agents.test.ts`.

## Adding a New API Integration Test

1. Create a file named `{resource}.test.ts` in this directory
2. Mock the relevant API client:
   ```ts
   const mockGet = jest.fn();
   jest.mock("@/lib/api/client", () => ({
     servicesApi: { GET: mockGet },
     agenticApi: { GET: jest.fn() },
     setAuthToken: jest.fn(),
     clearAuthToken: jest.fn(),
   }));
   ```
3. Write tests that invoke store actions or hooks and assert on the mapped results
4. Verify both success and error paths

## Running

```bash
# Run all tests including integration tests
npm test -- --watchAll=false

# Run only integration tests
npx jest src/__tests__/api-integration --watchAll=false
```
