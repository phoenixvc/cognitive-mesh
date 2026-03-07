# cognitive-mesh — phoenixvc/cognitive-mesh

## Overview

Hexagonal "ports and adapters" multi-agent orchestrator with multiple coordination patterns, governance gates, and real-time telemetry hub.

**Pattern**: Multi-agent orchestration engine implementing 4 coordination patterns (Parallel, Hierarchical, Competitive, CollaborativeSwarm) with autonomy levels and authority scoping. Real-time updates via SignalR hub.

## Orchestration Architecture

```
┌─────────────────────────────────────────────────────┐
│        MultiAgentOrchestrationEngine                │
│                                                     │
│  Coordination Patterns:                             │
│  ┌──────────┐ ┌────────────┐ ┌───────────┐        │
│  │ Parallel │ │Hierarchical│ │Competitive│        │
│  │WhenAll() │ │  leader →  │ │ parallel →│        │
│  │          │ │  subtasks  │ │ resolve   │        │
│  └──────────┘ └────────────┘ └───────────┘        │
│  ┌───────────────────────┐                         │
│  │ CollaborativeSwarm    │                         │
│  │ iterative loop        │                         │
│  │ maxIterations=5       │                         │
│  │ convergence check     │                         │
│  └───────────────────────┘                         │
│                                                     │
│  Governance Gates:                                  │
│  ├── Autonomy: RecommendOnly / ActWithConfirmation │
│  │              / FullyAutonomous                   │
│  ├── Authority: endpoints, budgets, data policies  │
│  ├── Ethics: normative + informational dignity     │
│  └── Approval: via _approvalAdapter                │
│                                                     │
│  Telemetry: SignalR Hub (CognitiveMeshHub)          │
│  State: ConcurrentDictionary (_activeTasks)         │
└─────────────────────────────────────────────────────┘
```

### Coordination Patterns

| Pattern | Implementation | Bounded? |
|---------|---------------|----------|
| Parallel | `Task.WhenAll(subTasks)` | No explicit cap |
| Hierarchical | Leader delegates subtasks | Leader-bounded |
| Competitive | Parallel then conflict resolution | No explicit cap |
| CollaborativeSwarm | Iterative loop, `maxIterations=5` | Yes — 5 iterations |

### Governance Gates

- **Autonomy levels**: `RecommendOnly`, `ActWithConfirmation`, `FullyAutonomous`
- **Authority scope**: Allowed endpoints, resource/budget caps, data policies
- **Ethics checks**: Normative + informational dignity; exceptions logged, do not crash orchestrator ("log & continue")
- **Approval flow**: When `ActWithConfirmation`, requests approval via `_approvalAdapter`

### Convergence

Swarm mode uses a simple convergence heuristic (string contains `"COMPLETE"`). This is a known limitation.

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.0 | 60.0% | Medium | Governance gates (approval + ethics) add latency. Adapter-dependent — remote adapters would increase further. |
| Scalability | 3.0 | 60.0% | Low | In-memory `ConcurrentDictionary` for active tasks. Multi-node behavior depends on unverified adapters. |
| Efficiency | 3.0 | 60.0% | Medium | `Task.WhenAll` is efficient for parallel patterns. Ethics checks add overhead per task. |
| Fault Tolerance | 3.0 | 60.0% | Low | Ethics exceptions absorbed gracefully. But retry/timeout behavior is adapter-defined and not shown. No verified crash recovery. |
| Throughput | 3.0 | 60.0% | Medium | Parallel execution path exists. No explicit fan-out cap or backpressure. Swarm bounded to 5 iterations. |
| Maintainability | 3.0 | 60.0% | High | Hexagonal architecture is clean. But 4 coordination patterns + governance increase cognitive load. |
| Determinism | 4.0 | 80.0% | Medium | Coordination patterns are explicit. Authority/autonomy model is well-defined. `AuditTrailId` exists but persistence is adapter-dependent. |
| Integration Ease | 3.0 | 60.0% | Medium | Ports-and-adapters model is inherently extensible. DTOs are well-defined. But 14 open PRs signal active churn. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.10 | 62.0% |
| Batch | 3.06 | 61.2% |
| Long-Running Durable | 3.16 | 63.2% |
| Event-Driven Serverless | 3.06 | 61.2% |
| Multi-Agent Reasoning | 3.34 | 66.8% |

## Technical Dependencies (Orchestration-Relevant)

| Dependency | Purpose |
|-----------|---------|
| .NET 9 / C# | Runtime |
| `ConcurrentDictionary` | Active task tracking |
| `Task.WhenAll` | Parallel coordination |
| SignalR | Real-time telemetry hub |
| Adapter interfaces | `IAgentRuntimeAdapter`, knowledge repository, approval adapter |

## Performance Characteristics (Code-Evidenced)

- **Parallel execution**: `Task.WhenAll` for parallel and competitive patterns
- **Swarm boundedness**: Iteration cap = 5, giving upper bound on orchestration loops
- **Fault containment**: Ethics exceptions absorbed; orchestration continues
- **Scale limitation**: Active task tracking uses in-memory structures; durability depends on adapters

## SWOT Analysis

### Strengths
- First-class coordination patterns (4 distinct modes) + autonomy/authority model
- Governance hooks (approval + ethics) built into the orchestration path
- Real-time hub (SignalR) for observability
- Clean hexagonal architecture with well-defined ports

### Weaknesses
- Adapter implementations not verified — key runtime/durability behavior is outside the engine
- Swarm convergence heuristic is simplistic (string contains "COMPLETE")
- No verified crash recovery or durable state persistence
- 14 open PRs indicate active churn on integration surfaces

### Opportunities
- Durable task storage + replay to raise auditability and crash resilience
- Formalize convergence with scoring/state-machine approach
- Add per-pattern backpressure controls (max parallelism per tenant/task)

### Threats
- 14 open PRs suggests integration surfaces may change quickly
- Governance expansion can add latency if adapters are remote or synchronous
- Adapter-dependent behavior means scores could shift significantly once implementations are verified

## Config Defaults

| Setting | Value | Location |
|---------|-------|----------|
| Timeouts | Adapter-defined (not shown) | — |
| Retries | Adapter-defined (not shown) | — |
| Max parallelism | No explicit cap (parallel `Task.WhenAll`) | MultiAgentOrchestrationEngine.cs |
| Swarm max iterations | 5 | MultiAgentOrchestrationEngine.cs |
| Coordination pattern default | CollaborativeSwarm | Task definition DTO |

## Evidence Index

| Aspect | Source Files |
|--------|-------------|
| Coordination patterns + concurrency | `MultiAgentOrchestrationEngine.cs` coordination methods |
| Governance gates | Autonomy/authority + approval + ethics logic in engine |
| Real-time telemetry | `CognitiveMeshHub` |
| Integration contract surface | `IMultiAgentOrchestrationPort` DTOs + interface |

## Missing Information (Blocking Higher Confidence)

- Concrete runtime adapter behavior: retries, timeouts, queueing, idempotency, durability
- End-to-end benchmark coverage (especially swarm modes under load)
- State persistence mechanism for `_activeTasks` and execution traces
- Multi-node deployment story

## Future Expansion Points

1. **Durable persistence** — persist `_activeTasks` and execution traces; tie to `AuditTrailId`
2. **Convergence overhaul** — replace string heuristic with explicit state machine or scoring criteria
3. **Backpressure controls** — max parallelism per pattern, per tenant, per task
4. **Adapter verification** — audit and document all adapter implementations for fault tolerance behavior
