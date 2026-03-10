/**
 * Agent store — manages agent registry state from the Agentic API.
 *
 * Hydrated via fetchAgents() which calls the real backend.
 * SignalR updates patch individual agents via updateAgent().
 */
import { create } from "zustand"
import { agenticApi } from "@/lib/api/client"

interface Agent {
  agentId: string
  agentType: string
  name: string
  status: "active" | "idle" | "error" | "deactivated"
  capabilities: string[]
  currentTasks: number
  registeredAt: string
}

interface AgentStoreState {
  agents: Agent[]
  selectedAgentId: string | null
  loading: boolean
  error: string | null
}

interface AgentStoreActions {
  fetchAgents: () => Promise<void>
  updateAgent: (agent: Agent) => void
  selectAgent: (agentId: string | null) => void
  clearError: () => void
}

export const useAgentStore = create<AgentStoreState & AgentStoreActions>(
  (set) => ({
    agents: [],
    selectedAgentId: null,
    loading: false,
    error: null,

    fetchAgents: async () => {
      set({ loading: true, error: null })
      try {
        const { data, error } = await agenticApi.GET("/registry", {
          params: {},
        })
        if (error) {
          set({ error: "Failed to fetch agents", loading: false })
          return
        }
        // Map generated types to our Agent interface
        const agents: Agent[] = ((data as unknown[]) ?? []).map(
          (item: unknown) => {
            const d = item as Record<string, unknown>
            return {
              agentId: String(d.agentId ?? ""),
              agentType: String(d.agentType ?? ""),
              name: String(d.name ?? d.agentType ?? ""),
              status: mapStatus(String(d.status ?? "Active")),
              capabilities: (d.capabilities as string[]) ?? [],
              currentTasks: Number(d.currentTasks ?? 0),
              registeredAt: String(d.registeredAt ?? new Date().toISOString()),
            }
          }
        )
        set({ agents, loading: false })
      } catch (err) {
        set({
          error:
            err instanceof Error ? err.message : "Failed to fetch agents",
          loading: false,
        })
      }
    },

    updateAgent: (updated) =>
      set((state) => ({
        agents: state.agents.map((a) =>
          a.agentId === updated.agentId ? updated : a
        ),
      })),

    selectAgent: (agentId) => set({ selectedAgentId: agentId }),

    clearError: () => set({ error: null }),
  })
)

function mapStatus(
  status: string
): "active" | "idle" | "error" | "deactivated" {
  switch (status) {
    case "Active":
      return "active"
    case "Retired":
      return "deactivated"
    case "Suspended":
      return "error"
    default:
      return "idle"
  }
}
