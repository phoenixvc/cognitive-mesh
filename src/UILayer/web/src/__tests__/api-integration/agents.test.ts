/**
 * API Integration Tests — Agent Registry
 *
 * Tests that the useAgentStore.fetchAgents() correctly calls the API
 * and maps the response into the store's Agent type.
 */
import { act } from "@testing-library/react";
import { useAgentStore } from "@/stores/useAgentStore";

// Mock the API client to use our controlled mock
const mockGet = jest.fn();
jest.mock("@/lib/api/client", () => ({
  agenticApi: { GET: (...args: unknown[]) => mockGet(...args) },
  servicesApi: { GET: jest.fn() },
  setAuthToken: jest.fn(),
  clearAuthToken: jest.fn(),
}));

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

describe("Agent API integration", () => {
  it("should call GET /registry to fetch agents", async () => {
    mockGet.mockResolvedValue({ data: [], error: undefined });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    expect(mockGet).toHaveBeenCalledWith("/registry", expect.any(Object));
  });

  it("should map API response fields to Agent interface", async () => {
    mockGet.mockResolvedValue({
      data: [
        {
          agentId: "agent-abc",
          agentType: "Orchestrator",
          name: "MainOrchestrator",
          status: "Active",
          capabilities: ["planning", "routing", "monitoring"],
          currentTasks: 5,
          registeredAt: "2025-06-15T10:30:00Z",
        },
      ],
      error: undefined,
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const agents = useAgentStore.getState().agents;
    expect(agents).toHaveLength(1);
    expect(agents[0]).toEqual({
      agentId: "agent-abc",
      agentType: "Orchestrator",
      name: "MainOrchestrator",
      status: "active",
      capabilities: ["planning", "routing", "monitoring"],
      currentTasks: 5,
      registeredAt: "2025-06-15T10:30:00Z",
    });
  });

  it("should handle empty agent list from API", async () => {
    mockGet.mockResolvedValue({ data: [], error: undefined });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    expect(useAgentStore.getState().agents).toEqual([]);
    expect(useAgentStore.getState().loading).toBe(false);
    expect(useAgentStore.getState().error).toBeNull();
  });

  it("should handle null data from API", async () => {
    mockGet.mockResolvedValue({ data: null, error: undefined });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    expect(useAgentStore.getState().agents).toEqual([]);
    expect(useAgentStore.getState().loading).toBe(false);
  });

  it("should set error state when API returns an error", async () => {
    mockGet.mockResolvedValue({
      data: undefined,
      error: { message: "Forbidden" },
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    expect(useAgentStore.getState().error).toBe("Failed to fetch agents");
    expect(useAgentStore.getState().agents).toEqual([]);
  });

  it("should set error state when fetch throws a network error", async () => {
    mockGet.mockRejectedValue(new Error("ECONNREFUSED"));

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    expect(useAgentStore.getState().error).toBe("ECONNREFUSED");
    expect(useAgentStore.getState().loading).toBe(false);
  });

  it("should handle agents with missing optional fields gracefully", async () => {
    mockGet.mockResolvedValue({
      data: [
        {
          agentId: "minimal-agent",
          // Missing: agentType, name, status, capabilities, currentTasks, registeredAt
        },
      ],
      error: undefined,
    });

    await act(async () => {
      await useAgentStore.getState().fetchAgents();
    });

    const agents = useAgentStore.getState().agents;
    expect(agents).toHaveLength(1);
    expect(agents[0].agentId).toBe("minimal-agent");
    expect(agents[0].agentType).toBe("");
    expect(agents[0].capabilities).toEqual([]);
    expect(agents[0].currentTasks).toBe(0);
  });
});
