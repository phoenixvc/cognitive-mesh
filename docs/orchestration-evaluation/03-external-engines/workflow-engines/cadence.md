# Cadence

## What It Is

Cadence is Uber's open-source durable execution platform — the predecessor to Temporal. It provides fault-tolerant workflow orchestration with deterministic replay, automatic retries, and crash recovery. Still actively maintained by Uber but with a smaller community than Temporal.

## Architecture & Orchestration Pattern

**Pattern**: Durable execution with deterministic replay (same lineage as Temporal)

Architecture is nearly identical to Temporal: frontend service, history service, matching service, and worker-based execution. Uses Thrift (vs Temporal's gRPC/Protobuf).

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.3 | 66.0% | Medium | Similar to Temporal but Thrift adds slightly more serialization overhead. |
| Scalability | 4.8 | 96.0% | High | Proven at Uber scale. Horizontally scalable. Multi-cluster support. |
| Efficiency | 3.8 | 76.0% | Medium | Similar to Temporal. Event history overhead. Worker polling. |
| Fault Tolerance | 5.0 | 100.0% | High | Same deterministic replay model as Temporal. Full crash recovery. |
| Throughput | 4.8 | 96.0% | High | Proven high throughput at Uber. Worker-based parallelism. |
| Maintainability | 3.5 | 70.0% | Medium | Fewer SDKs than Temporal (primarily Go, Java). Less documentation. Smaller community. |
| Determinism | 5.0 | 100.0% | High | Same deterministic replay guarantees as Temporal. |
| Integration Ease | 3.2 | 64.0% | Medium | Fewer language SDKs. Smaller community. Thrift vs Protobuf. Less tooling ecosystem. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.30 | 66.0% |
| Batch | 4.38 | 87.6% |
| Long-Running Durable | 4.54 | 90.8% |
| Event-Driven Serverless | 3.40 | 68.0% |
| Multi-Agent Reasoning | 3.60 | 72.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Long-running durable workflows | **2nd** | Same model as Temporal; choose if already invested in Cadence. |
| Batch enterprise automation | **2nd** | Proven at scale; Temporal is newer/better supported. |
| Interactive developer tools | 5th | Same latency concerns as Temporal plus smaller SDK ecosystem. |

## When NOT to Use

- New projects with no existing Cadence investment (choose Temporal instead)
- Teams needing Python, TypeScript, or .NET SDKs (limited)
- Projects requiring strong community support and documentation

## Maturity Signals

- **Corporate backing**: Uber (internal use)
- **Community**: Smaller than Temporal; some migration to Temporal observed
- **Production users**: Uber (primary), some other organizations
- **Future risk**: Temporal fork has attracted most of the community investment
