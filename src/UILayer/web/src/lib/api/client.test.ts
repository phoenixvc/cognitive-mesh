/**
 * Tests for the API client module.
 *
 * We mock openapi-fetch so that createClient returns a controllable
 * fake with .use() and .eject() spies. Then we verify setAuthToken
 * and clearAuthToken correctly manage the auth middleware lifecycle.
 */

const mockUse = jest.fn();
const mockEject = jest.fn();
const mockCreateClient = jest.fn(() => ({
  GET: jest.fn(),
  POST: jest.fn(),
  PUT: jest.fn(),
  DELETE: jest.fn(),
  use: mockUse,
  eject: mockEject,
}));

jest.mock("openapi-fetch", () => ({
  __esModule: true,
  default: mockCreateClient,
}));

// Use require() instead of import to avoid hoisting above the jest.fn() initialisations
// eslint-disable-next-line @typescript-eslint/no-var-requires
const { setAuthToken, clearAuthToken, servicesApi, agenticApi } = require("./client");

describe("API client", () => {
  // NOTE: createClient calls happen at module load time, so we check them
  // before clearing mocks. These tests run in the first describe block.
  it("should create servicesApi and agenticApi clients", () => {
    expect(servicesApi).toBeDefined();
    expect(agenticApi).toBeDefined();
    expect(mockCreateClient).toHaveBeenCalledTimes(2);
  });

  it("should call createClient with correct base URLs", () => {
    const calls = mockCreateClient.mock.calls as unknown as Array<[{ baseUrl: string }]>;
    // First call: servicesApi
    expect(calls[0][0]).toEqual(
      expect.objectContaining({
        baseUrl: expect.stringContaining("/api/v1"),
      })
    );
    // Second call: agenticApi — baseUrl includes /api/v1/agent
    expect(calls[1][0]).toEqual(
      expect.objectContaining({
        baseUrl: expect.stringContaining("/api/v1/agent"),
      })
    );
  });
});

describe("setAuthToken / clearAuthToken", () => {
  beforeEach(() => {
    // Clear call history but keep mock implementations
    mockUse.mockClear();
    mockEject.mockClear();
    // Ensure no middleware is registered from previous tests
    clearAuthToken();
    mockUse.mockClear();
    mockEject.mockClear();
  });

  it("should register auth middleware on both clients when setAuthToken is called", () => {
    setAuthToken("test-token-123");
    // use() should be called once per client (2 total)
    expect(mockUse).toHaveBeenCalledTimes(2);
    const middleware = mockUse.mock.calls[0][0];
    expect(middleware).toBeDefined();
    expect(typeof middleware.onRequest).toBe("function");
  });

  it("should set Authorization header via middleware onRequest", async () => {
    setAuthToken("my-bearer-token");
    const middleware = mockUse.mock.calls[0][0];
    const headers = new Headers();
    const request = { headers } as unknown as Request;
    const result = await middleware.onRequest({ request });
    expect(result.headers.get("Authorization")).toBe(
      "Bearer my-bearer-token"
    );
  });

  it("should eject previous middleware when setAuthToken is called again", () => {
    setAuthToken("token-1");
    expect(mockEject).not.toHaveBeenCalled();

    mockUse.mockClear();
    setAuthToken("token-2");
    // Should eject the previous middleware from both clients
    expect(mockEject).toHaveBeenCalledTimes(2);
    // And register the new one
    expect(mockUse).toHaveBeenCalledTimes(2);
  });

  it("should eject middleware on clearAuthToken", () => {
    setAuthToken("token-1");
    mockEject.mockClear();

    clearAuthToken();
    // Should eject from both clients
    expect(mockEject).toHaveBeenCalledTimes(2);
  });

  it("should be safe to call clearAuthToken when no middleware is set", () => {
    // No setAuthToken beforehand — should not throw
    expect(() => clearAuthToken()).not.toThrow();
  });
});
