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

async function fetchJson<T>(path: string): Promise<T | null> {
  try {
    const token = typeof localStorage !== "undefined"
      ? localStorage.getItem("cm_access_token")
      : null
    const res = await fetch(`${API_BASE}${path}`, {
      headers: token ? { Authorization: `Bearer ${token}` } : {},
    })
    if (!res.ok) return null
    return (await res.json()) as T
  } catch {
    return null
  }
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
      const [layers, metrics, status] = await Promise.all([
        fetchJson<Layer[]>("/api/v1/dashboard/layers"),
        fetchJson<Metric[]>("/api/v1/dashboard/metrics"),
        fetchJson<SystemStatus>("/api/v1/dashboard/status"),
      ])

      set({
        layers: layers ?? get().layers,
        metrics: metrics ?? get().metrics,
        systemStatus: status ?? get().systemStatus,
        loading: false,
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
    const metrics = await fetchJson<Metric[]>("/api/v1/dashboard/metrics")
    if (metrics) set({ metrics })
  },

  refreshActivities: async () => {
    const activities = await fetchJson<Activity[]>("/api/v1/dashboard/activities")
    if (activities) set({ activities })
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
