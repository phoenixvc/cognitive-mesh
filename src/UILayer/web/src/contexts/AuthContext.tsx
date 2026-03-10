"use client"

import React, { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react"
import { setAuthToken, clearAuthToken } from "@/lib/api/client"

interface User {
  id: string
  email: string
  name: string
  tenantId: string
  roles: string[]
}

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
}

interface AuthContextValue extends AuthState {
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  refreshToken: () => Promise<boolean>
}

const AuthContext = createContext<AuthContextValue | null>(null)

const TOKEN_KEY = "cm_access_token"
// TODO(Phase 14): Move refresh token to httpOnly cookie via backend /api/auth/refresh endpoint
const REFRESH_TOKEN_KEY = "cm_refresh_token"

function parseJwt(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split(".")[1]
    if (!base64Url) return null
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/")
    return JSON.parse(atob(base64))
  } catch {
    return null
  }
}

function isTokenExpired(token: string): boolean {
  const payload = parseJwt(token)
  if (!payload || typeof payload.exp !== "number") return true
  return Date.now() >= payload.exp * 1000
}

function extractUser(token: string): User | null {
  const payload = parseJwt(token)
  if (!payload) return null
  return {
    id: (payload.sub as string) ?? "",
    email: (payload.email as string) ?? "",
    name: (payload.name as string) ?? (payload.preferred_username as string) ?? "",
    tenantId: (payload.tenant_id as string) ?? "",
    roles: Array.isArray(payload.roles) ? payload.roles as string[] : [],
  }
}

const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5000"

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, setState] = useState<AuthState>({
    user: null,
    isAuthenticated: false,
    isLoading: true,
  })

  const applyToken = useCallback((accessToken: string) => {
    const user = extractUser(accessToken)
    if (!user) return false
    localStorage.setItem(TOKEN_KEY, accessToken)
    // Also set cookie so Next.js middleware can check auth server-side
    const secure = typeof window !== "undefined" && window.location.protocol === "https:" ? "; Secure" : ""
    document.cookie = `cm_access_token=${accessToken}; path=/; max-age=86400; SameSite=Lax${secure}`
    setAuthToken(accessToken)
    setState({ user, isAuthenticated: true, isLoading: false })
    return true
  }, [])

  const refreshToken = useCallback(async (): Promise<boolean> => {
    const stored = localStorage.getItem(REFRESH_TOKEN_KEY)
    if (!stored) return false
    try {
      const res = await fetch(`${API_BASE_URL}/api/v1/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken: stored }),
      })
      if (!res.ok) return false
      const data = await res.json()
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
      return applyToken(data.accessToken)
    } catch {
      return false
    }
  }, [applyToken])

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await fetch(`${API_BASE_URL}/api/v1/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
      })
      if (!res.ok) {
        const err = await res.json().catch(() => ({ message: "Login failed" }))
        throw new Error(err.message ?? "Login failed")
      }
      const data = await res.json()
      localStorage.setItem(REFRESH_TOKEN_KEY, data.refreshToken)
      if (!applyToken(data.accessToken)) {
        throw new Error("Invalid token received")
      }
    },
    [applyToken],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    document.cookie = "cm_access_token=; path=/; max-age=0"
    clearAuthToken()
    setState({ user: null, isAuthenticated: false, isLoading: false })
  }, [])

  // Restore session on mount
  useEffect(() => {
    const token = localStorage.getItem(TOKEN_KEY)
    if (token && !isTokenExpired(token)) {
      if (!applyToken(token)) {
        setState({ user: null, isAuthenticated: false, isLoading: false })
      }
    } else if (token) {
      // Token expired — try refresh
      refreshToken().then((ok) => {
        if (!ok) {
          localStorage.removeItem(TOKEN_KEY)
          localStorage.removeItem(REFRESH_TOKEN_KEY)
          document.cookie = "cm_access_token=; path=/; max-age=0"
          setState({ user: null, isAuthenticated: false, isLoading: false })
        }
      })
    } else {
      setState((prev) => ({ ...prev, isLoading: false }))
    }
  }, [applyToken, refreshToken])

  // Proactive token refresh — reschedule on each new token
  useEffect(() => {
    if (!state.isAuthenticated || !state.user) return
    const token = localStorage.getItem(TOKEN_KEY)
    if (!token) return
    const payload = parseJwt(token)
    if (!payload || typeof payload.exp !== "number") return
    const msUntilExpiry = payload.exp * 1000 - Date.now()
    const refreshIn = Math.max(msUntilExpiry - 60_000, 0)
    const timer = setTimeout(() => {
      refreshToken().then((ok) => {
        if (!ok) {
          logout()
        }
      })
    }, refreshIn)
    return () => clearTimeout(timer)
  }, [state.isAuthenticated, state.user, refreshToken, logout])

  const value = useMemo<AuthContextValue>(
    () => ({ ...state, login, logout, refreshToken }),
    [state, login, logout, refreshToken],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error("useAuth must be used within an AuthProvider")
  return ctx
}
