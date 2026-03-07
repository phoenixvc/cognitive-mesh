# Dagster

## What It Is

Dagster is a data orchestration platform built around the concept of "software-defined assets." It provides a unified programming model for data pipelines, integrating compute, storage, and lineage tracking. Strong focus on data quality, testing, and observability.

## Architecture & Orchestration Pattern

**Pattern**: Asset-centric DAG orchestration with software-defined assets

```text
┌──────────────────────────────────┐
│        Dagster Instance         │
│  ┌─────────┐  ┌──────────────┐ │
│  │ Dagit   │  │ Daemon       │ │
│  │ (Web UI)│  │ (Scheduler,  │ │
│  │         │  │  Sensors)    │ │
│  └─────────┘  └──────────────┘ │
│  ┌──────────────────────────┐  │
│  │ Run Storage (PostgreSQL) │  │
│  └──────────────────────────┘  │
└──────────────────────────────────┘
         ↕
┌──────────────────┐
│  Code Locations  │
│  ├── Assets      │
│  ├── Ops/Graphs  │
│  └── Resources   │
└──────────────────┘
```

- **Assets**: The primary abstraction — declarative definitions of data artifacts
- **Ops/Graphs**: Imperative computation units (traditional workflow model)
- **Resources**: External dependencies (databases, APIs) with configurable implementations
- **Sensors + Schedules**: Event-driven and time-based triggers
- **Partitions**: Built-in data partitioning for time-series and categorical data

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.3 | 66.0% | Medium | Daemon-based scheduling. Run startup overhead. Not optimized for sub-second. |
| Scalability | 4.2 | 84.0% | Medium | Code location isolation. Kubernetes executor. Partitioned execution. Dagster Cloud auto-scaling. |
| Efficiency | 3.8 | 76.0% | Medium | Asset materialization avoids unnecessary recomputation. Resource management. IO managers reduce boilerplate. |
| Fault Tolerance | 4.0 | 80.0% | High | RetryPolicy per op. Run-level retry. Asset materialization tracking. But no deterministic replay. |
| Throughput | 4.0 | 80.0% | Medium | Partitioned execution enables parallel processing. Concurrent runs. Multi-process executor. |
| Maintainability | 4.5 | 90.0% | High | Strong typing. Testable assets. Clear separation of business logic and infrastructure. Excellent documentation. |
| Determinism | 3.8 | 76.0% | Medium | Asset lineage tracking. Run history. Partition tracking. But no workflow-level replay. |
| Integration Ease | 4.0 | 80.0% | Medium | Rich integration library (dbt, Spark, pandas, etc.). REST API. Python-native. ~100+ open PRs. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.58 | 71.6% |
| Batch | 4.12 | 82.4% |
| Long-Running Durable | 3.50 | 70.0% |
| Event-Driven Serverless | 3.32 | 66.4% |
| Multi-Agent Reasoning | 2.90 | 58.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Data pipeline orchestration | **1st** | Asset-centric model is purpose-built for data workflows. |
| ML lifecycle management | **1st** | Partitions + lineage + testing = ideal for ML data. |
| Batch enterprise automation | **2nd** | Strong for data-centric batch; Temporal better for general workflows. |

## When NOT to Use

- Non-data workflows (general task orchestration → Temporal)
- Non-Python environments
- Simple event-driven functions (use Inngest)
- Long-running durable workflows without data assets
