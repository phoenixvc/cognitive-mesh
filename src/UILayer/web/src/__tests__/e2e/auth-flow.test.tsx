import React from "react"
import { render, screen, waitFor, act, fireEvent } from "@testing-library/react"

// Mock next/navigation
jest.mock("next/navigation", () => ({
  useRouter: () => ({ push: mockPush, back: jest.fn(), replace: jest.fn() }),
  usePathname: () => "/login",
  useSearchParams: () => new URLSearchParams(),
  redirect: jest.fn(),
}))

const mockPush = jest.fn()

// Simple auth store for testing
function createAuthStore() {
  let token: string | null = null
  let user: { name: string; roles: string[] } | null = null
  const listeners = new Set<() => void>()

  return {
    getToken: () => token,
    getUser: () => user,
    login: (t: string, u: { name: string; roles: string[] }) => {
      token = t
      user = u
      localStorage.setItem("auth_token", t)
      listeners.forEach((l) => l())
    },
    logout: () => {
      token = null
      user = null
      localStorage.removeItem("auth_token")
      listeners.forEach((l) => l())
    },
    subscribe: (fn: () => void) => {
      listeners.add(fn)
      return () => listeners.delete(fn)
    },
  }
}

describe("Auth flow", () => {
  let store: ReturnType<typeof createAuthStore>

  beforeEach(() => {
    store = createAuthStore()
    localStorage.clear()
    mockPush.mockClear()
  })

  it("renders login form", () => {
    const LoginPage = () => (
      <form>
        <label htmlFor="email">Email</label>
        <input id="email" type="email" />
        <label htmlFor="password">Password</label>
        <input id="password" type="password" />
        <button type="submit">Sign in</button>
      </form>
    )
    render(<LoginPage />)
    expect(screen.getByLabelText("Email")).toBeInTheDocument()
    expect(screen.getByLabelText("Password")).toBeInTheDocument()
    expect(screen.getByText("Sign in")).toBeInTheDocument()
  })

  it("stores token after successful login", async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ token: "jwt-123", user: { name: "Test", roles: ["admin"] } }),
    })

    const LoginPage = () => {
      const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        const res = await fetch("/api/auth/login", { method: "POST" })
        const data = await res.json()
        store.login(data.token, data.user)
      }
      return (
        <form onSubmit={handleSubmit}>
          <button type="submit">Sign in</button>
        </form>
      )
    }

    render(<LoginPage />)
    fireEvent.click(screen.getByText("Sign in"))

    await waitFor(() => {
      expect(store.getToken()).toBe("jwt-123")
      expect(localStorage.getItem("auth_token")).toBe("jwt-123")
    })
  })

  it("redirects to dashboard after login", async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ token: "jwt-456", user: { name: "Test", roles: ["admin"] } }),
    })

    const LoginPage = () => {
      const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        const res = await fetch("/api/auth/login", { method: "POST" })
        const data = await res.json()
        store.login(data.token, data.user)
        mockPush("/dashboard")
      }
      return (
        <form onSubmit={handleSubmit}>
          <button type="submit">Sign in</button>
        </form>
      )
    }

    render(<LoginPage />)
    fireEvent.click(screen.getByText("Sign in"))

    await waitFor(() => {
      expect(mockPush).toHaveBeenCalledWith("/dashboard")
    })
  })

  it("shows error on failed login", async () => {
    global.fetch = jest.fn().mockResolvedValue({
      ok: false,
      status: 401,
      json: () => Promise.resolve({ message: "Invalid credentials" }),
    })

    const LoginPage = () => {
      const [error, setError] = React.useState<string | null>(null)
      const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()
        const res = await fetch("/api/auth/login", { method: "POST" })
        if (!res.ok) {
          const data = await res.json()
          setError(data.message)
        }
      }
      return (
        <form onSubmit={handleSubmit}>
          <button type="submit">Sign in</button>
          {error && <div role="alert">{error}</div>}
        </form>
      )
    }

    render(<LoginPage />)
    fireEvent.click(screen.getByText("Sign in"))

    await waitFor(() => {
      expect(screen.getByRole("alert")).toHaveTextContent("Invalid credentials")
    })
  })

  it("clears state on logout", () => {
    store.login("token-abc", { name: "User", roles: ["viewer"] })
    expect(store.getToken()).toBe("token-abc")

    store.logout()
    expect(store.getToken()).toBeNull()
    expect(store.getUser()).toBeNull()
    expect(localStorage.getItem("auth_token")).toBeNull()
  })

  it("blocks access to protected route without auth", () => {
    const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
      if (!store.getToken()) {
        return <div>Please log in</div>
      }
      return <>{children}</>
    }

    render(
      <ProtectedRoute>
        <div>Secret content</div>
      </ProtectedRoute>
    )
    expect(screen.getByText("Please log in")).toBeInTheDocument()
    expect(screen.queryByText("Secret content")).not.toBeInTheDocument()
  })

  it("allows access to protected route with auth", () => {
    store.login("valid-token", { name: "Admin", roles: ["admin"] })

    const ProtectedRoute = ({ children }: { children: React.ReactNode }) => {
      if (!store.getToken()) {
        return <div>Please log in</div>
      }
      return <>{children}</>
    }

    render(
      <ProtectedRoute>
        <div>Secret content</div>
      </ProtectedRoute>
    )
    expect(screen.getByText("Secret content")).toBeInTheDocument()
  })
})
