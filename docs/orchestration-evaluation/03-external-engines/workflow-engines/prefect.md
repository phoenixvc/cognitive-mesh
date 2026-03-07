# Prefect

## What It Is

Prefect is a Python-native workflow orchestration framework focused on data pipelines and ML workflows. It emphasizes "positive engineering" — workflows work locally as plain Python, then gain orchestration features (retries, scheduling, observability) when deployed to the Prefect server.

## Architecture & Orchestration Pattern

**Pattern**: Decorator-based Python workflow orchestration with optional server coordination

```
┌──────────────────────────────────┐
│       Prefect Server / Cloud    │
│  ┌─────────┐  ┌──────────────┐ │
│  │ API     │  │ Scheduler    │ │
│  │ Server  │  │              │ │
│  └─────────┘  └──────────────┘ │
│  ┌──────────────────────────┐  │
│  │    Database (PostgreSQL) │  │
│  └──────────────────────────┘  │
└──────────────────────────────────┘
         ↕ HTTP
┌──────────────────┐
│  Work Pool       │
│  ├── Worker 1    │
│  ├── Worker 2    │
│  └── Worker N    │
└──────────────────┘
```

- `@flow` and `@task` decorators transform plain Python functions
- Flows can be tested locally without a server
- Work pools and workers for distributed execution
- Native async support

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.7 | 74.0% | Medium | Local execution is fast. Server adds HTTP overhead. Work pool scheduling adds latency. |
| Scalability | 4.0 | 80.0% | Medium | Work pools with multiple workers. Cloud auto-scaling. Kubernetes infrastructure option. |
| Efficiency | 3.5 | 70.0% | Medium | Python overhead. Task-level granularity. Worker processes consume resources. |
| Fault Tolerance | 3.8 | 76.0% | High | `retries` and `retry_delay_seconds` per task. State persistence. But no deterministic replay. |
| Throughput | 3.5 | 70.0% | Medium | Task-level concurrency. ConcurrentTaskRunner. But Python GIL limits per-worker throughput. |
| Maintainability | 4.2 | 84.0% | High | Clean Python API. Flows are testable locally. Good documentation. Active community. |
| Determinism | 3.2 | 64.0% | Medium | Flow run tracking. Task state persistence. But no replay — failed runs must be re-executed. |
| Integration Ease | 4.0 | 80.0% | Medium | Rich integration library. REST API. Python-native. ~100+ open PRs. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.70 | 74.0% |
| Batch | 4.00 | 80.0% |
| Long-Running Durable | 3.44 | 68.8% |
| Event-Driven Serverless | 3.52 | 70.4% |
| Multi-Agent Reasoning | 2.90 | 58.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Python data pipelines | **1st** | Purpose-built. Pythonic API. Great local dev experience. |
| ML workflow orchestration | **2nd** | Strong Python ecosystem integration. Dagster is comparable. |
| Batch enterprise automation | 3rd | Works well; Temporal stronger for non-Python workloads. |
| Interactive developer tools | 3rd | Good DX but server adds overhead. |

## When NOT to Use

- Non-Python environments
- Long-running durable workflows requiring deterministic replay (use Temporal)
- Kubernetes-native container orchestration (use Argo)
- Sub-10ms latency requirements
