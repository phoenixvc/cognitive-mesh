# Temporal

## What It Is

Temporal is a durable execution platform for building reliable, long-running workflows. Originally forked from Uber's Cadence, it provides deterministic workflow replay, automatic retry, and crash recovery. Workflows survive process restarts by persisting state after every step.

## Architecture & Orchestration Pattern

**Pattern**: Durable execution with deterministic replay + worker-based task processing

```
┌──────────────────────────────────────────────┐
│              Temporal Server                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ Frontend │  │ History  │  │ Matching │  │
│  │ Service  │  │ Service  │  │ Service  │  │
│  └──────────┘  └──────────┘  └──────────┘  │
│  ┌──────────────────────────────────────┐   │
│  │        Persistence (DB + ES)         │   │
│  └──────────────────────────────────────┘   │
└──────────────────────────────────────────────┘
         ↕ gRPC              ↕ gRPC
┌──────────────┐     ┌──────────────┐
│   Worker 1   │     │   Worker 2   │
│ (workflows)  │     │ (activities) │
└──────────────┘     └──────────────┘
```

- **Workflows**: Deterministic code that orchestrates activities; replayed from event history on recovery
- **Activities**: Non-deterministic side-effecting operations (API calls, DB writes)
- **Workers**: Processes that poll for and execute workflows/activities; horizontally scalable
- **Task Queues**: Named queues that route work to appropriate workers

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 4.0 | 80.0% | Medium | gRPC is efficient. Worker task queue polling is optimized. Not sub-10ms but well-suited for workflow coordination. Recent `activity_as_tool` helper for AI agent support. |
| Scalability | 5.0 | 100.0% | High | Horizontally scalable workers, partitioned namespaces, multi-cluster replication. Proven at Uber/Netflix scale. |
| Efficiency | 4.0 | 80.0% | Medium | Event history grows per workflow. Worker polling consumes resources. But efficient at scale due to task queue partitioning. |
| Fault Tolerance | 5.0 | 100.0% | High | Deterministic replay from event history. Configurable retry policies per activity. Heartbeating for long activities. Workflow-level timeouts. |
| Throughput | 5.0 | 100.0% | High | Worker-based parallelism with task queue routing. Configurable max concurrent activities per worker. Proven high-throughput at scale. |
| Maintainability | 3.5 | 70.0% | Medium | Clean SDK design across Go/Java/Python/TypeScript/.NET/PHP. Determinism constraints require learning curve. Workflow versioning via if/else creates tech debt. Non-determinism errors are a common footgun. |
| Determinism | 5.0 | 100.0% | High | Core design principle: workflows must be deterministic. Full event history replay. Complete audit trail. |
| Integration Ease | 4.0 | 80.0% | Medium | Multi-language SDKs. Namespace isolation. ~158 open PRs (high activity but mature project). Strong documentation. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.50 | 70.0% |
| Batch | 4.57 | 91.4% |
| Long-Running Durable | 4.72 | 94.4% |
| Event-Driven Serverless | 3.60 | 72.0% |
| Multi-Agent Reasoning | 3.78 | 75.6% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Long-running durable workflows | **1st** | Purpose-built for this. Unmatched replay + durability. |
| Batch enterprise automation | **1st** | Scalable workers + fault tolerance + retry policies. |
| Multi-agent AI coordination | 3rd | Can orchestrate agents as activities, but no native agent abstractions. |
| Event-driven serverless | 4th | Can handle events but adds infrastructure overhead for simple cases. |
| Interactive developer tools | 5th | gRPC latency overhead makes it suboptimal for sub-50ms requirements. |

## When NOT to Use

- Simple event handlers that don't need durability (use Inngest or direct handlers)
- Latency-critical interactive UIs (polling + gRPC adds overhead)
- Small teams without infrastructure capacity to run Temporal Server (consider Temporal Cloud)
- Short-lived workflows that complete in < 1 second (overhead not justified)

## Config Defaults

| Setting | Default | Configurable |
|---------|---------|:------------:|
| Workflow execution timeout | None (must set) | Yes |
| Activity start-to-close timeout | Required (no default) | Yes |
| Activity retry policy | Default: unlimited retries, 1s initial, 100s max, 2.0 backoff | Yes |
| Workflow task timeout | 10 seconds | Yes |
| Worker max concurrent activities | 200 | Yes |
| Worker max concurrent workflow tasks | 200 | Yes |
| History limit | 50K events / 50MB per execution | Use ContinueAsNew |
| Minimum memory footprint | ~832MB | — |

## Maturity Signals

- **GitHub stars**: ~18.7k
- **Open PRs**: ~158
- **Release cadence**: Regular releases; server + SDKs versioned independently
- **Corporate backing**: Temporal Technologies (venture-funded)
- **Production users**: Uber, Netflix, Stripe, Snap, HashiCorp
- **Community**: Active Slack, comprehensive docs, conference talks
