# LangGraph Integration Playbook

## Overview

LangGraph provides graph-based agent orchestration with checkpointing and human-in-the-loop support. Integration means converting multi-agent coordination logic into LangGraph state graphs.

**Best for**: Multi-agent AI reasoning, conversational AI with memory, complex agent interaction patterns.

## Prerequisites

- Python 3.10+
- `langgraph` package
- LLM API keys (OpenAI, Anthropic, etc.)
- Optional: `langgraph-checkpoint-postgres` for durable state
- Optional: LangGraph Cloud for managed deployment

---

## agentkit-forge → LangGraph

### Approach: Lifecycle as Linear Graph with Conditional Edges

```python
from langgraph.graph import StateGraph, END

class AgentForgeState(TypedDict):
    task_input: dict
    discovery: dict | None
    plan: dict | None
    implementation: dict | None
    validation: dict | None
    result: dict | None
    phase: str

graph = StateGraph(AgentForgeState)
graph.add_node("discovery", run_discovery)
graph.add_node("planning", run_planning)
graph.add_node("implementation", run_implementation)
graph.add_node("validation", run_validation)
graph.add_node("ship", run_ship)

graph.add_edge("discovery", "planning")
graph.add_edge("planning", "implementation")
graph.add_edge("implementation", "validation")

# Conditional: if validation fails, loop back
graph.add_conditional_edges("validation", check_validation,
    {"pass": "ship", "fail": "implementation"})
graph.add_edge("ship", END)

graph.set_entry_point("discovery")
app = graph.compile(checkpointer=PostgresSaver(...))
```

### What You Gain
- Checkpoint-based resume (replaces file-based state)
- Conditional routing (validation loops, error handling)
- State visualization via LangGraph Studio

### What This Doesn't Help With
- agentkit-forge's strength is deterministic lifecycle, not AI agent reasoning
- LangGraph adds complexity without clear benefit for non-LLM orchestration
- **Recommendation**: Only use LangGraph if agentkit-forge evolves to include AI agents

### Effort: Medium (2–3 weeks)
### Fit: LOW — agentkit-forge is better served by Temporal or Inngest

---

## codeflow-engine → LangGraph

### Approach: AutoGen Path → LangGraph Multi-Agent

Replace the AutoGen optional integration with LangGraph's native multi-agent support.

```python
from langgraph.graph import StateGraph

class MultiAgentState(TypedDict):
    messages: list[dict]
    current_agent: str
    tool_results: list[dict]
    round: int

def agent_node(state, config):
    """Each agent is a graph node that processes messages."""
    agent = get_agent(state["current_agent"])
    response = agent.invoke(state["messages"])
    return {"messages": state["messages"] + [response]}

def router(state):
    """Route to next agent or end."""
    if state["round"] >= 10:  # max_round=10
        return END
    return select_next_agent(state)

graph = StateGraph(MultiAgentState)
graph.add_node("planner", agent_node)
graph.add_node("executor", agent_node)
graph.add_node("reviewer", agent_node)
graph.add_conditional_edges("planner", router)
graph.add_conditional_edges("executor", router)
graph.add_conditional_edges("reviewer", router)
```

### Migration Steps

**Phase 1**: Replace `autogen_multi_agent.py` with LangGraph graph
**Phase 2**: Map GroupChat `max_round=10` to graph round counter
**Phase 3**: Add checkpointer for conversation state persistence
**Phase 4**: Keep workflow engine for non-agent workflows; LangGraph for agent coordination only

### Effort: Medium (2–4 weeks)
### Fit: MEDIUM — good for the multi-agent path only

---

## cognitive-mesh → LangGraph

### Approach: Coordination Patterns as LangGraph Subgraphs

This is the **highest-value integration**. cognitive-mesh's 4 coordination patterns map naturally to LangGraph's graph model.

### Parallel Coordination

```python
from langgraph.graph import StateGraph, Send

def parallel_dispatch(state):
    """Fan-out to multiple agents."""
    return [Send(f"agent_{i}", task)
            for i, task in enumerate(state["sub_tasks"])]

graph = StateGraph(ParallelState)
graph.add_node("dispatch", parallel_dispatch)
for i in range(MAX_AGENTS):
    graph.add_node(f"agent_{i}", run_agent)
graph.add_node("aggregate", aggregate_results)
```

### Hierarchical Coordination

```python
# Leader delegates to sub-agents
graph = StateGraph(HierarchicalState)
graph.add_node("leader", leader_plan)
graph.add_node("delegate", delegate_subtasks)  # Uses Send()
graph.add_node("sub_agent", execute_subtask)
graph.add_node("leader_review", leader_review)

graph.add_edge("leader", "delegate")
graph.add_conditional_edges("delegate", route_to_sub_agents)
graph.add_edge("sub_agent", "leader_review")
```

### Competitive Coordination

```python
# Parallel execution → conflict resolution
graph = StateGraph(CompetitiveState)
graph.add_node("parallel_execute", parallel_agents)
graph.add_node("resolve_conflicts", conflict_resolution)
graph.add_edge("parallel_execute", "resolve_conflicts")
```

### CollaborativeSwarm

```python
# Iterative loop with convergence check
graph = StateGraph(SwarmState)
graph.add_node("iterate", swarm_iteration)
graph.add_node("check_convergence", check_convergence)

graph.add_edge("iterate", "check_convergence")
graph.add_conditional_edges("check_convergence",
    lambda s: "end" if s["converged"] or s["iteration"] >= 5 else "iterate",
    {"iterate": "iterate", "end": END})
```

### Governance Gates as Interrupt Points

```python
# Ethics + approval as interrupt_before
graph = graph.compile(
    checkpointer=PostgresSaver(...),
    interrupt_before=["execute_action"]  # Human approval gate
)

# Ethics check as a node before action
graph.add_node("ethics_check", check_ethics)
graph.add_conditional_edges("ethics_check",
    lambda s: "execute" if s["ethics_passed"] else "reject",
    {"execute": "execute_action", "reject": "reject_action"})
```

### Migration Steps

**Phase 1**: Implement Parallel and Hierarchical patterns as LangGraph subgraphs
**Phase 2**: Map governance gates to LangGraph interrupt points
**Phase 3**: Replace convergence heuristic (string "COMPLETE") with state-based scoring
**Phase 4**: Add PostgreSQL checkpointer for durable state (replaces ConcurrentDictionary)
**Phase 5**: Replace SignalR telemetry with LangGraph streaming callbacks

### What You Gain
- Native graph-based coordination (replaces pattern-matching code)
- Checkpoint-based durability (replaces in-memory ConcurrentDictionary)
- Human-in-the-loop via interrupts (replaces approval adapter)
- State visualization via LangGraph Studio
- Convergence as graph state (replaces string heuristic)

### What You Lose
- .NET runtime (LangGraph is Python-only)
- SignalR real-time push (replaced by streaming)
- Hexagonal architecture purity (LangGraph has its own patterns)
- Custom autonomy level model (needs reimplementation)

### Effort: Large (6–10 weeks, 2–3 engineers)
### Risk: HIGH — requires Python port of core orchestration
### Fit: HIGH — strongest alignment with cognitive-mesh's multi-agent model

---

## HouseOfVeritas → LangGraph

### Fit: LOW

HouseOfVeritas is event-driven workflow orchestration, not multi-agent reasoning. LangGraph adds complexity without clear benefit.

**Recommendation**: Keep HouseOfVeritas on Inngest. Only consider LangGraph if HouseOfVeritas evolves to include AI agent decision-making in its workflows.

---

## Integration Architecture Summary

```
┌──────────────────────────────────────────────────┐
│                Production Stack                  │
│                                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐      │
│  │ Temporal │  │ Inngest  │  │ LangGraph│      │
│  │(durable) │  │(events)  │  │(agents)  │      │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘      │
│       │             │             │              │
│  ┌────┴─────────────┴─────────────┴────┐        │
│  │    Shared Event Bus / API Layer     │        │
│  └────┬─────────────┬─────────────┬────┘        │
│       │             │             │              │
│  ┌────┴────┐  ┌─────┴────┐  ┌────┴─────┐       │
│  │agentkit │  │codeflow  │  │cognitive │       │
│  │forge    │  │engine    │  │mesh      │       │
│  └─────────┘  └──────────┘  └──────────┘       │
│                                                  │
│  HouseOfVeritas: stays on Inngest (native)      │
└──────────────────────────────────────────────────┘
```
