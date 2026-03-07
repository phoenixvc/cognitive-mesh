# Scoring Framework

## Scale

All metrics use a **1.0–5.0 decimal scale** with 0.1 granularity. Percentage equivalents: `(score / 5) × 100`.

| Raw Score | Percentage | Meaning |
|-----------|-----------|---------|
| 1.0 | 20.0% | Ad-hoc; unclear contracts; no safe defaults; brittle |
| 2.0 | 40.0% | Minimal; some structure but major gaps; unreliable under load |
| 3.0 | 60.0% | Workable; partial contracts; some defaults; mixed rigor |
| 4.0 | 80.0% | Strong; explicit contracts; tested failure modes; observable |
| 5.0 | 100.0% | Production-grade; explicit contracts; hardened failure modes; observable and repeatable |

Intermediate values (e.g., 3.4 = 68.0%) indicate systems that partially meet the next level's criteria.

## Metrics (8 total)

### 1. Coordination Latency / Responsiveness

Time from request arrival to orchestration decision to first action. Measures overhead introduced by the coordination layer itself.

- **1.0**: Seconds-level overhead; blocking coordination
- **3.0**: Sub-second coordination; acceptable for interactive use
- **5.0**: Negligible overhead; near-instant routing and dispatch

### 2. Scalability (Horizontal + Vertical)

Ability to handle increasing workload by adding resources (horizontal) or increasing capacity of existing resources (vertical).

- **1.0**: Single-process, single-host only; no clustering support
- **3.0**: Can scale vertically; limited horizontal support (e.g., file-based coordination)
- **5.0**: Native horizontal scaling; partitioning; elastic resource management

### 3. Resource Efficiency (CPU/Memory/I/O)

Resource consumption relative to useful work performed. Measures overhead of the orchestration layer.

- **1.0**: Heavy resource consumption for coordination; wasteful polling
- **3.0**: Moderate overhead; some unnecessary resource usage
- **5.0**: Minimal coordination overhead; efficient use of compute, memory, and I/O

### 4. Fault Tolerance / Recovery

Ability to handle failures gracefully — timeouts, retries, idempotency, durable state, crash recovery.

- **1.0**: No retry logic; failures crash the system; no state persistence
- **3.0**: Basic retries with backoff; some timeout handling; partial state recovery
- **5.0**: Durable execution; automatic retry with idempotency; full crash recovery; circuit breakers

### 5. Throughput / Concurrency

Ability to execute work in parallel with controlled fan-out, backpressure, and concurrent execution management.

- **1.0**: Sequential execution only; no parallelism
- **3.0**: Basic parallel execution; limited fan-out control; no backpressure
- **5.0**: Controlled fan-out with backpressure; bounded concurrency; high-throughput execution

### 6. Maintainability / Complexity

Coherence of patterns, test surface, code organization, and ease of understanding/modifying the orchestration logic.

- **1.0**: Tangled orchestration logic; no tests; unclear responsibilities
- **3.0**: Reasonable patterns; some test coverage; moderate complexity
- **5.0**: Clean separation of concerns; comprehensive tests; well-documented; easy to extend

### 7. Determinism / Auditability

Replayability of orchestration decisions, stable routing, traceability of actions, and ability to audit execution history.

- **1.0**: Non-deterministic routing; no execution history; untraceable decisions
- **3.0**: Partial logging; some replay capability; basic correlation IDs
- **5.0**: Full replay capability; deterministic state machines; complete audit trail with correlation

### 8. Integration Ease / Openness

How easily the system integrates with other tools, services, and workflows. Includes API contracts, plugin architecture, dependency hygiene, and documentation.

- **1.0**: No public APIs; monolithic; undocumented; tight coupling
- **3.0**: Some APIs; partial plugin support; moderate documentation
- **5.0**: Explicit versioned APIs; rich plugin/adapter architecture; clean dependencies; comprehensive docs

Factors evaluated:
- Explicit public APIs/contracts + versioning
- Plugin/adapter architecture
- Dependency hygiene (declared vs used)
- Documentation + configuration discoverability
- WIP risk signal (open PR volume / churn) as a proxy for integration stability

## Weighted Total Calculation

```text
weighted_total = Σ(metric_score × weight)
percentage = (weighted_total / 5) × 100
```

Where weights are defined per [workload profile](weight-profiles.md) and sum to 1.0.

## Confidence Levels

Scores are annotated with confidence levels where relevant:

| Confidence | Meaning |
|-----------|---------|
| **High** | Score based on direct code review or documented benchmarks |
| **Medium** | Score based on architecture review + public documentation |
| **Low** | Score based on public docs/marketing materials only; needs validation |
