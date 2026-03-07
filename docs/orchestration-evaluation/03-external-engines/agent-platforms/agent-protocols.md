# Agent Protocols: A2A & MCP

## Overview

Agent protocols define how agents communicate with each other (A2A) and how agents access tools and context (MCP). Unlike the platforms and runtimes evaluated elsewhere, these are **interoperability standards** — they don't orchestrate agents themselves, but they shape how orchestration systems compose and integrate.

---

## Google Agent-to-Agent Protocol (A2A)

### What It Is

A2A is an open protocol from Google for agent-to-agent communication, enabling agents built on different frameworks and platforms to discover, negotiate, and collaborate with each other. It aims to be the "HTTP of agents" — a universal standard for inter-agent interoperability.

### Architecture

```
Agent A (any framework)           Agent B (any framework)
     │                                    │
     │  ─── A2A Protocol ───────────────  │
     │  1. Discovery (Agent Cards)        │
     │  2. Task negotiation               │
     │  3. Message exchange               │
     │  4. Artifact passing               │
     └────────────────────────────────────┘
```

### Key Concepts

- **Agent Cards**: JSON metadata describing an agent's capabilities, supported input/output types, and authentication requirements. Enables dynamic agent discovery
- **Tasks**: The unit of work in A2A. A task has a lifecycle (submitted → working → completed/failed) with optional streaming updates
- **Messages**: Communication between agents within a task context. Support text, files, and structured data
- **Artifacts**: Named outputs from task execution that can be consumed by other agents
- **Push Notifications**: Server-sent events for async task updates

### Relevance to Agent Orchestration

| Aspect | Impact |
|--------|--------|
| **Multi-framework interop** | Agents built with LangGraph, AutoGen, Semantic Kernel, or custom frameworks can communicate without shared runtime |
| **Dynamic discovery** | Agent Cards enable orchestrators to discover and compose agents at runtime rather than design-time |
| **Vendor neutrality** | Reduces lock-in to any single agent platform — agents become pluggable |
| **Orchestrator implications** | Orchestrators don't need to embed all agent logic; they can delegate to external A2A-compatible agents |

### Maturity Signals

- **Backed by**: Google (primary), with participation from multiple vendors
- **Status**: Open specification, early adoption phase
- **Implementations**: Vertex AI Agent Builder supports A2A natively; community implementations emerging
- **Risk**: Competing with informal patterns (direct API calls, function-based delegation) that are simpler but less interoperable
- **Adoption**: Still early; most production systems use direct integration rather than A2A

### When It Matters

- Multi-vendor agent environments where agents are built on different stacks
- Agent marketplaces where agents are discovered and composed dynamically
- Enterprise environments with heterogeneous agent platforms across departments

### When It Doesn't Matter

- Single-framework environments (all agents on LangGraph, or all on AutoGen)
- Internal orchestration where all agents share the same runtime
- Latency-critical paths where protocol overhead is unacceptable

---

## Anthropic Model Context Protocol (MCP)

### What It Is

MCP is an open protocol from Anthropic that standardizes how AI agents and models access external tools, data sources, and context. It provides a universal interface for tool integration, replacing the need for custom tool adapters per agent framework.

### Architecture

```
┌─────────────────┐     MCP Protocol     ┌──────────────────┐
│   MCP Client    │ ←──────────────────→  │   MCP Server     │
│ (Agent/Model)   │                       │ (Tool Provider)  │
│                 │  - Tool discovery      │                  │
│ Claude, GPT,    │  - Tool invocation     │ GitHub, Slack,   │
│ Custom agents   │  - Resource access     │ Database, API,   │
│                 │  - Prompt templates    │ File system      │
└─────────────────┘                       └──────────────────┘
```

### Key Concepts

- **Tools**: Functions that agents can call. MCP servers expose tool schemas (JSON Schema) with descriptions, enabling agents to discover and use tools without framework-specific adapters
- **Resources**: Data sources that agents can read (files, database records, API responses)
- **Prompts**: Reusable prompt templates that servers can provide for common interactions
- **Sampling**: Servers can request model completions through the client (reverse flow)
- **Transport**: stdio (local) and HTTP+SSE (remote) transport mechanisms

### Relevance to Agent Orchestration

| Aspect | Impact |
|--------|--------|
| **Tool portability** | Same MCP server works with any MCP-compatible agent — no per-framework tool wrappers |
| **Reduced integration work** | Orchestrators don't need custom tool adapters for each external service |
| **Ecosystem effect** | Growing library of community MCP servers (hundreds available) creates a shared tool ecosystem |
| **Security model** | MCP defines tool access boundaries — useful for governance and authorization |

### Maturity Signals

- **Backed by**: Anthropic (primary), with broad industry adoption
- **Status**: Production-ready; widely adopted across frameworks
- **Implementations**: Claude (native), OpenAI Agents SDK (adopted), Inngest AgentKit, Cursor, Windsurf, many IDEs and tools
- **Ecosystem**: Hundreds of community-built MCP servers covering major APIs and services
- **Adoption**: Strong — becoming the de facto standard for tool integration

### When It Matters

- Building agents that need access to external tools (databases, APIs, services)
- Multi-framework environments where tool integration should be portable
- Governance scenarios where tool access needs to be centrally managed

### When It Doesn't Matter

- Simple agents with 1-2 hard-coded tools
- Environments where tools are already integrated via framework-native mechanisms (e.g., LangChain tools)
- Latency-critical paths where MCP protocol overhead is a concern

---

## A2A vs MCP: Complementary, Not Competing

| Dimension | A2A | MCP |
|-----------|-----|-----|
| **What it connects** | Agent ↔ Agent | Agent ↔ Tool/Data |
| **Problem solved** | Multi-agent interoperability | Tool integration portability |
| **Backed by** | Google | Anthropic |
| **Adoption stage** | Early | Production-ready |
| **Competes with** | Direct API calls, custom protocols | Framework-native tool wrappers |
| **Relevance to orchestration** | Enables dynamic agent composition | Reduces integration effort |

### Combined Architecture

```
┌────────────────────────────────────────────────────┐
│              Orchestration Layer                     │
│  (Temporal / Inngest / Custom)                      │
│                                                     │
│  Agent A ←── A2A ──→ Agent B ←── A2A ──→ Agent C  │
│     │                   │                   │       │
│     └── MCP ──→ Tools   └── MCP ──→ Tools   └── MCP│
│         (GitHub,            (Database,       (Slack,│
│          Search)             Vector DB)       Email)│
└────────────────────────────────────────────────────┘
```

In this model:
- **A2A** handles inter-agent communication and task delegation
- **MCP** handles tool and data access for each agent
- **Orchestration engine** manages workflow durability, retries, and coordination
- Each layer is independently replaceable

## Impact on Custom Implementations

For our internal repos (agentkit-forge, cognitive-mesh, codeflow-engine, HouseOfVeritas):

| Protocol | Impact | Priority |
|----------|--------|----------|
| **MCP adoption** | High — reduces custom tool integration work; enables tool sharing across repos | P1 |
| **A2A adoption** | Medium — enables agents built in different repos to communicate; reduces coupling | P2 |

**Recommendation**: Adopt MCP first (production-ready, immediate integration benefits), then evaluate A2A as it matures.
