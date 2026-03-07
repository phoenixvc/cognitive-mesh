# Missing Evidence Checklist

Evidence gaps that block "high confidence" evaluation. Resolving these would allow score refinement with decimal precision.

## Cross-Repo Missing Evidence

### 1. Benchmark Data

| What's Needed | Why It Matters | Affected Metrics |
|--------------|----------------|------------------|
| **Interactive p95**: request → orchestration decision → first action | Validates Latency scores | Latency |
| **Batch throughput**: tasks/second under fan-out; queue depth behavior | Validates Throughput scores | Throughput, Scalability |
| **Resource profiling**: CPU/memory per orchestration decision | Validates Efficiency scores | Efficiency |

### 2. Concrete Concurrency Controls

| What's Needed | Why It Matters | Affected Metrics |
|--------------|----------------|------------------|
| Explicit max parallelism enforcement mechanism | Prevents resource exhaustion | Throughput, Scalability |
| Backpressure semantics (queueing, rejection, throttling) | Determines behavior under load | Throughput, Fault Tolerance |
| Per-tenant / per-task isolation | Multi-tenant safety | Scalability, Fault Tolerance |

### 3. Failure Semantics

| What's Needed | Why It Matters | Affected Metrics |
|--------------|----------------|------------------|
| Retry budgets (max retries × max tasks = blast radius) | Prevents retry storms | Fault Tolerance |
| Timeout defaults for every external call path | Prevents hung tasks | Fault Tolerance, Latency |
| Circuit breaker thresholds and recovery behavior | Prevents cascading failures | Fault Tolerance |
| Idempotency strategy per operation type | Prevents duplicate side effects | Determinism, Fault Tolerance |

### 4. Durability

| What's Needed | Why It Matters | Affected Metrics |
|--------------|----------------|------------------|
| What state survives process restart? | Core durability question | Fault Tolerance |
| Replay story: can completed steps be skipped? | Crash recovery efficiency | Determinism, Fault Tolerance |
| Audit trace continuity across restarts | Compliance and debugging | Determinism |

### 5. Integration Contracts

| What's Needed | Why It Matters | Affected Metrics |
|--------------|----------------|------------------|
| Schema/versioning for events, tasks, contracts | Breaking change detection | Integration Ease |
| Breaking change policy (semver adherence) | Integration planning | Integration Ease |
| Dependency declaration completeness | Install reliability | Integration Ease, Maintainability |

## Per-Repo Specific Gaps

### agentkit-forge
- [ ] p95 end-to-end orchestrate time
- [ ] Check runner p95 across stacks
- [ ] Explicit timeout/retry defaults for exec commands
- [ ] Task file compaction/rotation strategy

### codeflow-engine
- [ ] Where `max_concurrent` is enforced (semaphore? queue?)
- [ ] Circuit breaker policy usage verification
- [ ] Load/perf tests for event fan-out
- [ ] AutoGen dependency extra declaration

### cognitive-mesh
- [ ] Concrete adapter implementations (runtime, persistence, approval)
- [ ] End-to-end swarm mode benchmarks
- [ ] State persistence for `_activeTasks`
- [ ] Multi-node deployment verification

### HouseOfVeritas
- [ ] Inngest queue latency measurements (p50/p95)
- [ ] Runtime concurrency/rate limits
- [ ] Error escalation when `routeToInngest` fails
- [ ] Step-level instrumentation

## Resolution Priority

| Priority | Gap | Impact on Scoring |
|----------|-----|-------------------|
| **P0** | Benchmark data (latency + throughput) | Could shift Latency/Throughput scores by ±1.0 |
| **P0** | Concurrency enforcement verification | Could shift Throughput/Scalability by ±0.5 |
| **P1** | Failure semantics documentation | Could shift Fault Tolerance by ±0.5 |
| **P1** | Durability verification | Could shift Fault Tolerance/Determinism by ±0.5 |
| **P2** | Integration contract versioning | Could shift Integration Ease by ±0.3 |
