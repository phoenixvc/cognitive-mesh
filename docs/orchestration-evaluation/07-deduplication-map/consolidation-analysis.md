# Deduplication Map & Consolidation Analysis

## Orchestration Responsibility Overlap

### Responsibility Matrix

Each cell indicates whether a repo implements (Y), partially implements (P), or does not implement (—) each orchestration responsibility.

| Responsibility | agentkit-forge | codeflow-engine | cognitive-mesh | HouseOfVeritas |
|---------------|:-:|:-:|:-:|:-:|
| **Task routing/dispatch** | Y | Y | Y | Y |
| **State machine / lifecycle** | Y | P | P | — |
| **Retry / backoff** | — | Y | — | Y |
| **Timeout handling** | P (lock TTL) | Y | — | Y (Inngest) |
| **Event-driven triggers** | — | Y | — | Y |
| **Cron / scheduled execution** | — | — | — | Y |
| **Multi-agent coordination** | P (teams) | P (AutoGen) | Y | — |
| **Governance / ethics gates** | — | — | Y | — |
| **Approval workflows** | — | — | Y | P (feature flag) |
| **Real-time telemetry** | — | — | Y (SignalR) | — |
| **Audit trail / event log** | Y (JSONL) | P (history) | P (adapter) | P (Inngest) |
| **Fan-out / parallel execution** | — | Y | Y | Y (Inngest) |
| **Durable state persistence** | P (file-based) | — | — | Y (Inngest) |
| **Plugin / extension registry** | P (YAML specs) | Y (entry points) | Y (ports/adapters) | — |
| **LLM provider management** | — | Y | — | — |
| **Security hardening** | Y (allowlists) | — | P (authority scope) | — |

### Overlap Visualization

```
                    Task Routing & Dispatch
                    ┌───────────────────┐
                    │   ALL FOUR REPOS  │
                    └───────────────────┘

    State Machine          Retry/Timeout          Event-Driven
    ┌───────────┐         ┌───────────┐         ┌───────────┐
    │ agentkit  │         │ codeflow  │         │ codeflow  │
    │ forge     │         │ engine    │         │ engine    │
    │           │         │ HoV       │         │ HoV       │
    └───────────┘         └───────────┘         └───────────┘

    Multi-Agent            Governance         Fan-Out Parallel
    ┌───────────┐         ┌───────────┐       ┌───────────────┐
    │ cognitive │         │ cognitive │       │ codeflow-eng  │
    │ mesh      │         │ mesh      │       │ cognitive-mesh│
    │ codeflow* │         │ (unique)  │       │ HoV (Inngest) │
    └───────────┘         └───────────┘       └───────────────┘
    * = optional/partial
```

### Key Overlaps

1. **Task routing**: All 4 repos implement task routing/dispatch, each differently. This is the highest overlap area.
2. **Retry + fan-out**: codeflow-engine and HouseOfVeritas both implement retry and fan-out, but with different patterns (async Python vs Inngest step functions).
3. **Multi-agent**: cognitive-mesh and codeflow-engine both support multi-agent coordination, but cognitive-mesh is purpose-built while codeflow-engine's is an optional bolt-on (AutoGen).

### Unique Capabilities (No Overlap)

| Capability | Only In | Strategic Value |
|-----------|---------|----------------|
| Governance gates (ethics + approval) | cognitive-mesh | High — regulatory compliance |
| Real-time telemetry (SignalR) | cognitive-mesh | Medium — operational visibility |
| Cron-triggered workflows | HouseOfVeritas | Medium — batch automation |
| LLM provider management | codeflow-engine | Medium — multi-model flexibility |
| Security-hardened runner | agentkit-forge | High — supply chain safety |
| Deterministic 5-phase lifecycle | agentkit-forge | High — predictability |

## Consolidation Paths

### Option A: agentkit-forge as Orchestration Core

**Approach**: Use agentkit-forge's deterministic lifecycle as the orchestration backbone. Integrate cognitive-mesh's governance gates and codeflow-engine's retry/integration capabilities as plugins.

```
agentkit-forge (core lifecycle + routing)
  ├── + cognitive-mesh governance gates (as validation step)
  ├── + codeflow-engine retry/timeout (as execution wrapper)
  └── + HouseOfVeritas event patterns (for async/cron tasks)
```

| Pros | Cons |
|------|------|
| Strongest determinism and auditability | Requires Node.js runtime for orchestration |
| Clear phase boundaries for plugin injection | File-based coordination limits scale |
| Security hardening already built in | No native multi-agent support |

**Effort**: Large (L) — requires adapter development for all integration points.

**Best for**: Teams prioritizing predictability, auditability, and security.

### Option B: cognitive-mesh as Orchestration Core

**Approach**: Use cognitive-mesh's hexagonal architecture as the integration hub. Implement agentkit-forge's lifecycle as a coordination pattern. Plug codeflow-engine's retry and HouseOfVeritas's event patterns into adapters.

```
cognitive-mesh (hexagonal core + multi-agent)
  ├── + agentkit-forge lifecycle (as coordination pattern)
  ├── + codeflow-engine retry/LLM management (as adapters)
  └── + HouseOfVeritas event dispatch (as adapter)
```

| Pros | Cons |
|------|------|
| Richest pattern set (4 coordination modes) | Adapter implementations unverified |
| Governance built-in | Higher complexity / learning curve |
| Hexagonal architecture naturally accommodates integration | 14 open PRs indicate churn |

**Effort**: Medium-Large (M-L) — adapter interfaces exist; implementations needed.

**Best for**: Teams prioritizing governance, multi-agent reasoning, and architectural flexibility.

### Option C: External Engine as Core + Internal Repos as Plugins

**Approach**: Adopt an external engine (Temporal or Inngest) as the orchestration backbone. Reduce internal repos to domain-specific plugins/workers.

```
Temporal or Inngest (external orchestration core)
  ├── agentkit-forge → worker/activity (lifecycle validation)
  ├── codeflow-engine → worker/activity (LLM coordination)
  ├── cognitive-mesh → worker/activity (multi-agent + governance)
  └── HouseOfVeritas → already on Inngest (minimal change)
```

| Pros | Cons |
|------|------|
| Battle-tested orchestration (Temporal: durable; Inngest: serverless) | Vendor/platform dependency |
| Eliminates orchestration responsibility overlap | Migration effort across all repos |
| Scalability and fault tolerance "for free" | Cognitive-mesh governance gates need adaptation |

**Effort**: Large (L) — migration of all repos; most impactful long-term.

**Best for**: Teams prioritizing production-grade reliability, scalability, and reduced maintenance burden.

## Recommended Path

**Option C (External Engine as Core)** is recommended for production deployments, with Temporal for durable/batch workloads and Inngest for event-driven/serverless workloads.

**Rationale**:
- Orchestration is infrastructure, not business logic. External engines handle it better.
- Internal repos should focus on their unique capabilities (governance, lifecycle, LLM management).
- HouseOfVeritas is already on Inngest, reducing migration scope.
- Temporal's replay + durability addresses the biggest scoring gaps across all internal repos.

**Phase 1**: Migrate HouseOfVeritas event patterns to a shared Inngest instance (low effort — already on Inngest).
**Phase 2**: Wrap cognitive-mesh's multi-agent patterns as Temporal activities with governance checks.
**Phase 3**: Convert agentkit-forge's lifecycle phases into Temporal workflow steps.
**Phase 4**: Integrate codeflow-engine's LLM provider management as a shared service (not orchestration-bound).

## Overlap Risk Summary

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Divergent retry semantics across repos | High | Medium | Standardize on Temporal/Inngest retry policies |
| Duplicate task routing logic | High | Medium | Consolidate to single routing layer |
| Inconsistent audit trails | Medium | High | Adopt Temporal's event sourcing or shared event schema |
| Governance gates bypassed in some paths | Medium | High | Ensure all orchestration paths route through cognitive-mesh gates |
