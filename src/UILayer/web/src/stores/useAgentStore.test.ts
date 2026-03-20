import { useAgentStore } from "./useAgentStore";
import { act } from "@testing-library/react";

// Mock the API client
jest.mock("@/lib/api/client", () => ({
  agenticApi: {
    GET: jest.fn(),
  },
  servicesApi: {
    GET: jest.fn(),
  },
  setAuthToken: jest.fn(),
  clearAuthToken: jest.fn(),
}));

import { agenticApi } from "@/lib/api/client";
const mockGet = agenticApi.GET as jest.Mock;

// Reset store between tests
beforeEach(() => {
  jest.clearAllMocks();
  act(() => {
    useAgentStore.setState({
      agents: [],
      selectedAgentId: null,
      loading: false,
      error: null,
    });
  });
});

describe("useAgentStore", () => {
  it("should start with empty agents and no selection", () => {
    const state = useAgentStore.getState();
    expect(state.agents).toEqual([]);
    expect(state.selectedAgentId).toBeNull();
    expect(state.loading).toBe(false);
    expect(state.error).toBeNull();
  });

  it("should fetch agents and map response data correctly", async () => {
    mockGet.mockResolvedValue({
      data: [
        {
          agentId: "agent-1",
          agentType: "Orchestrator",
          name: "MainAgent",
          status: "Active",
          capabilities: ["planning", "routing"],
          currentTasks: 3,
          registeredAt: "2025-01-01T00:00:00Z",
        },
      ],
      error: undefined,
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const state = useAgentStore.getState();
    expect(state.loading).toBe(false);
    expect(state.error).toBeNull();
    expect(state.agents).toHaveLength(1);
    expect(state.agents[0]).toEqual({
      agentId: "agent-1",
      agentType: "Orchestrator",
      name: "MainAgent",
      status: "active", // mapped from "Active"
      capabilities: ["planning", "routing"],
      currentTasks: 3,
      registeredAt: "2025-01-01T00:00:00Z",
    });
  });

  it("should set loading to true during fetch", async () => {
    let resolvePromise: (v: unknown) => void;
    const promise = new Promise((r) => {
      resolvePromise = r;
    });
    mockGet.mockReturnValue(promise);

    // Start fetch but don't await
    let fetchPromise: Promise<void>;
    act(() => {
      fetchPromise = useAgentStore.getState().fetchAgents();
    });

    expect(useAgentStore.getState().loading).toBe(true);

    await act(async () => {
      resolvePromise!({ data: [], error: undefined });
      await fetchPromise!;
    });

    expect(useAgentStore.getState().loading).toBe(false);
  });

  it("should set error when API returns an error response", async () => {
    mockGet.mockResolvedValue({
      data: undefined,
      error: { message: "Unauthorized" },
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const state = useAgentStore.getState();
    expect(state.error).toBe("Failed to fetch agents");
    expect(state.loading).toBe(false);
    expect(state.agents).toEqual([]);
  });

  it("should set error when fetch throws an exception", async () => {
    mockGet.mockRejectedValue(new Error("Network error"));

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const state = useAgentStore.getState();
    expect(state.error).toBe("Network error");
    expect(state.loading).toBe(false);
  });

  it("should map status values correctly", async () => {
    mockGet.mockResolvedValue({
      data: [
        { agentId: "1", status: "Active" },
        { agentId: "2", status: "Retired" },
        { agentId: "3", status: "Suspended" },
        { agentId: "4", status: "Unknown" },
      ],
      error: undefined,
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const agents = useAgentStore.getState().agents;
    expect(agents[0].status).toBe("active");
    expect(agents[1].status).toBe("deactivated");
    expect(agents[2].status).toBe("error");
    expect(agents[3].status).toBe("idle");
  });

  it("should update an existing agent by agentId", () => {
    act(() => {
      useAgentStore.setState({
        agents: [
          {
            agentId: "a1",
            agentType: "Worker",
            name: "Worker1",
            status: "idle",
            capabilities: [],
            currentTasks: 0,
            registeredAt: "2025-01-01",
          },
        ],
      });
    });

    act(() => {
      useAgentStore.getState().updateAgent({
        agentId: "a1",
        agentType: "Worker",
        name: "Worker1-Updated",
        status: "active",
        capabilities: ["compute"],
        currentTasks: 2,
        registeredAt: "2025-01-01",
      });
    });

    const agent = useAgentStore.getState().agents[0];
    expect(agent.name).toBe("Worker1-Updated");
    expect(agent.status).toBe("active");
    expect(agent.capabilities).toEqual(["compute"]);
  });

  it("should select an agent by id", () => {
    act(() => {
      useAgentStore.getState().selectAgent("agent-42");
    });
    expect(useAgentStore.getState().selectedAgentId).toBe("agent-42");
  });

  it("should deselect agent by passing null", () => {
    act(() => {
      useAgentStore.getState().selectAgent("agent-42");
    });
    act(() => {
      useAgentStore.getState().selectAgent(null);
    });
    expect(useAgentStore.getState().selectedAgentId).toBeNull();
  });

  it("should clear error", () => {
    act(() => {
      useAgentStore.setState({ error: "some error" });
    });
    act(() => {
      useAgentStore.getState().clearError();
    });
    expect(useAgentStore.getState().error).toBeNull();
  });
});
