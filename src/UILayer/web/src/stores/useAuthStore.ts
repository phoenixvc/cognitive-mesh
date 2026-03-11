/**
 * Auth store — mirrors AuthContext state into Zustand for non-React consumers
 * and future migration away from Context.
 *
 * Phase 14: This store is hydrated from AuthContext via a sync effect in AuthProvider.
 * Components can read auth state from either source; prefer useAuth() in components
 * and useAuthStore in non-component code (interceptors, SignalR).
 */
import { create } from "zustand"

interface User {
  id: string
  email: string
  name: string
  tenantId: string
  roles: string[]
}

interface AuthStoreState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  accessToken: string | null
}

interface AuthStoreActions {
  setAuth: (user: User, token: string) => void
  clearAuth: () => void
  setLoading: (loading: boolean) => void
}

export const useAuthStore = create<AuthStoreState & AuthStoreActions>((set) => ({
  user: null,
  isAuthenticated: false,
  isLoading: true,
  accessToken: null,

  setAuth: (user, token) =>
    set({ user, isAuthenticated: true, isLoading: false, accessToken: token }),

  clearAuth: () =>
    set({ user: null, isAuthenticated: false, isLoading: false, accessToken: null }),

  setLoading: (loading) => set({ isLoading: loading }),
}))
