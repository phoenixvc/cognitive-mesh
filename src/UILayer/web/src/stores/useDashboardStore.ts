/**
 * Dashboard store — replaces useDashboardData hook + DashboardAPI mock.
 *
 * Uses direct fetch() for dashboard endpoints (not yet in OpenAPI spec).
 * When backend adds /api/v1/dashboard/* endpoints, switch to servicesApi.
 * SignalR patches update individual fields.
 */
import { create } from "zustand"

const API_BASE =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000"

interface Layer {
  id: string
  name: string
  icon: string
  color: string
  uptime: number
  description: string
}

interface Metric {
  id: string
  label: string
  value: string
  change: string
  status: "up" | "stable" | "down"
  energy: number
  icon: string
}

interface AgentSummary {
  name: string
  status: "active" | "idle"
  tasks: number
  energy: number
}

interface Activity {
  time: string
  event: string
  type: string
}

interface SystemStatus {
  power: number
  load: number
  neuralNetwork: boolean
  quantumProcessing: boolean
}

interface DashboardStoreState {
  layers: Layer[]
  metrics: Metric[]
  agents: AgentSummary[]
  activities: Activity[]
  systemStatus: SystemStatus | null
  loading: boolean
  error: string | null
  lastFetchedAt: number | null
}

interface DashboardStoreActions {
  fetchAll: () => Promise<void>
  refreshMetrics: () => Promise<void>
  refreshActivities: () => Promise<void>
  patchSystemStatus: (patch: Partial<SystemStatus>) => void
  patchMetric: (id: string, patch: Partial<Metric>) => void
  clearError: () => void
}

async function fetchJson<T>(path: string): Promise<T> {
  const token = typeof localStorage !== "undefined"
    ? localStorage.getItem("cm_access_token")
    : null
  const res = await fetch(`${API_BASE}${path}`, {
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  })
  if (!res.ok) {
    throw new Error(`${res.status} ${res.statusText}: ${path}`)
  }
  return (await res.json()) as T
}

export const useDashboardStore = create<
  DashboardStoreState & DashboardStoreActions
>((set, get) => ({
  layers: [],
  metrics: [],
  agents: [],
  activities: [],
  systemStatus: null,
  loading: false,
  error: null,
  lastFetchedAt: null,

  fetchAll: async () => {
    set({ loading: true, error: null })
    try {
      const results = await Promise.allSettled([
        fetchJson<Layer[]>("/api/v1/dashboard/layers"),
        fetchJson<Metric[]>("/api/v1/dashboard/metrics"),
        fetchJson<SystemStatus>("/api/v1/dashboard/status"),
      ])

      const layers = results[0].status === "fulfilled" ? results[0].value : get().layers
      const metrics = results[1].status === "fulfilled" ? results[1].value : get().metrics
      const status = results[2].status === "fulfilled" ? results[2].value : get().systemStatus

      const failures = results.filter(r => r.status === "rejected")

      set({
        layers,
        metrics,
        systemStatus: status,
        loading: false,
        error: failures.length > 0 ? "Some dashboard data failed to load" : null,
        lastFetchedAt: Date.now(),
      })
    } catch (err) {
      set({
        error:
          err instanceof Error ? err.message : "Failed to fetch dashboard data",
        loading: false,
      })
    }
  },

  refreshMetrics: async () => {
    try {
      const metrics = await fetchJson<Metric[]>("/api/v1/dashboard/metrics")
      set({ metrics, error: null })
    } catch (err) {
      set({ error: err instanceof Error ? err.message : "Failed to refresh metrics" })
    }
  },

  refreshActivities: async () => {
    try {
      const activities = await fetchJson<Activity[]>("/api/v1/dashboard/activities")
      set({ activities, error: null })
    } catch (err) {
      set({ error: err instanceof Error ? err.message : "Failed to refresh activities" })
    }
  },

  patchSystemStatus: (patch) =>
    set((state) => ({
      systemStatus: state.systemStatus
        ? { ...state.systemStatus, ...patch }
        : null,
    })),

  patchMetric: (id, patch) =>
    set((state) => ({
      metrics: state.metrics.map((m) =>
        m.id === id ? { ...m, ...patch } : m
      ),
    })),

  clearError: () => set({ error: null }),
}))
