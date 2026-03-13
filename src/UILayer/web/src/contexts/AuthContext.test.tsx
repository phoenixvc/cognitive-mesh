import React from "react";
import { render, screen, act, waitFor } from "@testing-library/react";
import { AuthProvider, useAuth } from "./AuthContext";

// Mock the API client module
jest.mock("@/lib/api/client", () => ({
  setAuthToken: jest.fn(),
  clearAuthToken: jest.fn(),
}));

import { setAuthToken, clearAuthToken } from "@/lib/api/client";

// Helper to create a fake JWT
function createFakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" }));
  const body = btoa(JSON.stringify(payload));
  const signature = "fake-signature";
  return `${header}.${body}.${signature}`;
}

// Unexpired token (expires in 1 hour)
function validToken(overrides: Record<string, unknown> = {}) {
  return createFakeJwt({
    sub: "user-1",
    email: "test@example.com",
    name: "Test User",
    tenant_id: "tenant-1",
    roles: ["admin"],
    exp: Math.floor(Date.now() / 1000) + 3600,
    ...overrides,
  });
}

// Expired token
function expiredToken() {
  return createFakeJwt({
    sub: "user-1",
    email: "test@example.com",
    name: "Test User",
    tenant_id: "tenant-1",
    roles: [],
    exp: Math.floor(Date.now() / 1000) - 3600,
  });
}

// Test component that exposes auth context
function AuthConsumer() {
  const { user, isAuthenticated, isLoading, login, logout } = useAuth();
  return (
    <div>
      <span data-testid="loading">{String(isLoading)}</span>
      <span data-testid="authenticated">{String(isAuthenticated)}</span>
      <span data-testid="user-email">{user?.email ?? "none"}</span>
      <span data-testid="user-name">{user?.name ?? "none"}</span>
      <button
        onClick={() => login("test@example.com", "password123")}
        data-testid="login-btn"
      >
        Login
      </button>
      <button onClick={logout} data-testid="logout-btn">
        Logout
      </button>
    </div>
  );
}

// Reset between tests
beforeEach(() => {
  localStorage.clear();
  document.cookie = "cm_access_token=; path=/; max-age=0";
  jest.clearAllMocks();
  (global.fetch as jest.Mock)?.mockReset?.();
  global.fetch = jest.fn();
});

afterEach(() => {
  jest.restoreAllMocks();
});

describe("AuthContext", () => {
  it("should throw when useAuth is used outside AuthProvider", () => {
    // Suppress React error boundary console output
    const spy = jest.spyOn(console, "error").mockImplementation(() => {});
    expect(() => render(<AuthConsumer />)).toThrow(
      "useAuth must be used within an AuthProvider"
    );
    spy.mockRestore();
  });

  it("should start with isLoading true and isAuthenticated false", () => {
    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );
    // Initially loading
    expect(screen.getByTestId("authenticated").textContent).toBe("false");
  });

  it("should restore session from localStorage when token is valid", async () => {
    const token = validToken();
    localStorage.setItem("cm_access_token", token);

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("authenticated").textContent).toBe("true");
    });
    expect(screen.getByTestId("user-email").textContent).toBe(
      "test@example.com"
    );
    expect(setAuthToken).toHaveBeenCalledWith(token);
  });

  it("should not authenticate when stored token is expired and no refresh token", async () => {
    const token = expiredToken();
    localStorage.setItem("cm_access_token", token);
    // No refresh token stored

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("loading").textContent).toBe("false");
    });
    expect(screen.getByTestId("authenticated").textContent).toBe("false");
  });

  it("should login successfully and set user state", async () => {
    const token = validToken();
    (global.fetch as jest.Mock).mockResolvedValue({
      ok: true,
      json: async () => ({
        accessToken: token,
        refreshToken: "refresh-token-123",
      }),
    });

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("loading").textContent).toBe("false");
    });

    await act(async () => {
      screen.getByTestId("login-btn").click();
    });

    await waitFor(() => {
      expect(screen.getByTestId("authenticated").textContent).toBe("true");
    });
    expect(screen.getByTestId("user-email").textContent).toBe(
      "test@example.com"
    );
    expect(localStorage.getItem("cm_access_token")).toBe(token);
    expect(localStorage.getItem("cm_refresh_token")).toBe("refresh-token-123");
  });

  it("should logout and clear state", async () => {
    const token = validToken();
    localStorage.setItem("cm_access_token", token);

    render(
      <AuthProvider>
        <AuthConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("authenticated").textContent).toBe("true");
    });

    await act(async () => {
      screen.getByTestId("logout-btn").click();
    });

    expect(screen.getByTestId("authenticated").textContent).toBe("false");
    expect(screen.getByTestId("user-email").textContent).toBe("none");
    expect(localStorage.getItem("cm_access_token")).toBeNull();
    expect(clearAuthToken).toHaveBeenCalled();
  });

  it("should throw on login failure", async () => {
    (global.fetch as jest.Mock).mockResolvedValue({
      ok: false,
      json: async () => ({ message: "Invalid credentials" }),
    });

    let loginError: Error | null = null;

    function LoginErrorConsumer() {
      const { login, isLoading } = useAuth();
      return (
        <button
          data-testid="login-err"
          onClick={async () => {
            try {
              await login("bad@example.com", "wrong");
            } catch (e) {
              loginError = e as Error;
            }
          }}
        >
          Login
        </button>
      );
    }

    render(
      <AuthProvider>
        <LoginErrorConsumer />
      </AuthProvider>
    );

    // Wait for initial loading to complete
    await act(async () => {
      await new Promise((r) => setTimeout(r, 50));
    });

    await act(async () => {
      screen.getByTestId("login-err").click();
    });

    expect(loginError).not.toBeNull();
    expect(loginError!.message).toBe("Invalid credentials");
  });

  it("should extract roles from JWT payload", async () => {
    const token = validToken({ roles: ["admin", "viewer"] });
    localStorage.setItem("cm_access_token", token);

    function RoleConsumer() {
      const { user } = useAuth();
      return (
        <span data-testid="roles">{user?.roles?.join(",") ?? "none"}</span>
      );
    }

    render(
      <AuthProvider>
        <RoleConsumer />
      </AuthProvider>
    );

    await waitFor(() => {
      expect(screen.getByTestId("roles").textContent).toBe("admin,viewer");
    });
  });
});
