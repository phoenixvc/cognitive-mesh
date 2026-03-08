# Evidence Index

File-level source-of-truth references per repository, organized by evaluation metric. These are the specific files/locations where scoring evidence was found.

## agentkit-forge

### Contracts / DTOs / Schemas

| File | Contains |
|------|----------|
| `spec/teams.yaml` | Team ID definitions and routing specs |
| `.agentkit/state/tasks/*.json` | Task lifecycle state and payload schema |
| `.agentkit/state/events.log` | JSONL event schema |

### Retry / Timeout Code Paths

| File | Contains |
|------|----------|
| `orchestrator.mjs` | `LOCK_STALE_MS` (30m session lock TTL) |
| `task-protocol.mjs` | Terminal task states; handoff lock protocol |

### State Storage + Lock Logic

| File | Contains |
|------|----------|
| `orchestrator.mjs` | Session locking with stale detection |
| `task-protocol.mjs` | `withHandoffLock` atomic file creation (`wx`) |
| `.agentkit/state/` | File-based state persistence directory |

### Telemetry Emitters

| File | Contains |
|------|----------|
| `orchestrator.mjs` → `events.log` | JSONL structured event emission |

## codeflow-engine

### Contracts / DTOs / Schemas

| File | Contains |
|------|----------|
| `config/settings.py` | Pydantic config model (workflow defaults) |
| Entry points (setup.py/pyproject.toml) | Plugin registration contracts |

### Retry / Timeout Code Paths

| File | Contains |
|------|----------|
| `workflows/engine.py` | Retry logic (attempts=3, delay=5, exponential backoff) |
| `workflows/engine.py` | `asyncio.wait_for(timeout=self.config.workflow_timeout)` |

### State Storage + Lock Logic

| File | Contains |
|------|----------|
| `workflows/engine.py` | `running_workflows` dict; `MAX_WORKFLOW_HISTORY=1000` |
| In-memory only | No durable state persistence identified |

### Telemetry Emitters

| File | Contains |
|------|----------|
| `workflows/engine.py` | Metrics fields; error history recording |

### Multi-Agent

| File | Contains |
|------|----------|
| `autogen_multi_agent.py` | AutoGen GroupChat wrapper; `max_round=10` |

## cognitive-mesh

### Contracts / DTOs / Schemas

| File | Contains |
|------|----------|
| `IMultiAgentOrchestrationPort` | Port interface + DTOs (task definition, coordination pattern, autonomy level) |
| Task definition DTO | `CoordinationPattern` default: `CollaborativeSwarm` |

### Retry / Timeout Code Paths

| File | Contains |
|------|----------|
| Adapter interfaces | Retry/timeout behavior is adapter-defined (not in engine) |
| Ethics exception handling | `catch` blocks in engine — log & continue pattern |

### State Storage + Lock Logic

| File | Contains |
|------|----------|
| `MultiAgentOrchestrationEngine.cs` | `ConcurrentDictionary` for `_activeTasks` (in-memory) |
| Adapter interfaces | Durable persistence is adapter-dependent |

### Telemetry Emitters

| File | Contains |
|------|----------|
| `CognitiveMeshHub` | SignalR real-time updates for agent/workflow progress |

### Coordination Patterns

| File | Contains |
|------|----------|
| `MultiAgentOrchestrationEngine.cs` | Parallel (`Task.WhenAll`), Hierarchical, Competitive, CollaborativeSwarm |

### Governance

| File | Contains |
|------|----------|
| `MultiAgentOrchestrationEngine.cs` | Autonomy levels, authority scope, ethics checks, approval adapter |

## HouseOfVeritas

### Contracts / DTOs / Schemas

| File | Contains |
|------|----------|
| `lib/workflows/schema.ts` | Typed event names + payload interfaces |
| Workflow env vars doc | Feature flag documentation |

### Retry / Timeout Code Paths

| File | Contains |
|------|----------|
| Individual workflow files | `retries: 2` per function; Inngest-managed timeouts |

### State Storage + Lock Logic

| File | Contains |
|------|----------|
| Inngest platform | State managed by Inngest (step functions) |

### Telemetry Emitters

| File | Contains |
|------|----------|
| `routeToInngest` | Event dispatch with error logging (errors swallowed) |

### Dispatch + Registry

| File | Contains |
|------|----------|
| Next.js API route | `serve({ client, functions: [...] })` — central registry |
| `routeToInngest` | `inngest.send({ name, data })` dispatch helper |

## Cross-Repo Evidence Gaps

| Evidence Type | agentkit-forge | codeflow-engine | cognitive-mesh | HouseOfVeritas |
|---------------|:-:|:-:|:-:|:-:|
| Benchmark data | Missing | Missing | Missing | Missing |
| Concurrency enforcement | Missing | Unclear | Missing | Inngest-managed |
| Circuit breaker | Missing | Unclear | Missing | Missing |
| Idempotency strategy | Partial (locks) | Missing | Missing | Per-event ID |
| Durable persistence | File-based | Missing | Adapter-dependent | Inngest-managed |
