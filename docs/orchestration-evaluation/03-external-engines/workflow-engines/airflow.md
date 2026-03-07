# Apache Airflow

## What It Is

Apache Airflow is the most widely-adopted open-source workflow orchestration platform. It uses Python-defined DAGs (Directed Acyclic Graphs) to schedule and monitor workflows. Originally built by Airbnb, now an Apache Foundation project. The de facto standard for ETL/data pipeline orchestration.

## Architecture & Orchestration Pattern

**Pattern**: DAG-based task scheduling with pluggable executors

```
┌──────────────────────────────────┐
│         Airflow Cluster         │
│  ┌───────────┐  ┌────────────┐ │
│  │ Webserver │  │ Scheduler  │ │
│  │ (UI)      │  │            │ │
│  └───────────┘  └────────────┘ │
│  ┌──────────────────────────┐  │
│  │  Metadata DB (PostgreSQL)│  │
│  └──────────────────────────┘  │
│  ┌────────────────────────────┐│
│  │ Executor (Local/Celery/K8s)││
│  └────────────────────────────┘│
└──────────────────────────────────┘
```

- **DAGs**: Python files defining task dependencies and scheduling
- **Operators**: Pre-built task types (BashOperator, PythonOperator, etc.)
- **Executors**: Pluggable execution backends (Local, Celery, Kubernetes, etc.)
- **XComs**: Cross-task communication for passing small data between tasks
- **Connections/Variables**: Centralized configuration management

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 2.5 | 50.0% | High | Scheduler polling interval (default: 5s). Task startup overhead. Not designed for real-time. |
| Scalability | 4.0 | 80.0% | High | Celery/Kubernetes executors scale horizontally. But scheduler can be a bottleneck. |
| Efficiency | 3.0 | 60.0% | Medium | Scheduler overhead. XCom metadata. DAG parsing on every scheduler loop. |
| Fault Tolerance | 3.5 | 70.0% | High | Task retries configurable. `execution_timeout`. But no durable execution — failed tasks restart from scratch. |
| Throughput | 3.5 | 70.0% | Medium | `parallelism=32` (global), `max_active_tasks_per_dag=16`, `max_active_tis_per_dag=8`. Executor-dependent. |
| Maintainability | 3.0 | 60.0% | High | Complex setup. DAG serialization issues. Python dependency management challenges. Large operator surface. |
| Determinism | 3.5 | 70.0% | Medium | Task instance tracking by `execution_date`. Audit log. But no replay mechanism. |
| Integration Ease | 4.0 | 80.0% | High | Massive operator ecosystem (600+ providers). REST API. Well-documented. But complex setup. ~200+ open PRs. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 2.90 | 58.0% |
| Batch | 4.00 | 80.0% |
| Long-Running Durable | 3.40 | 68.0% |
| Event-Driven Serverless | 2.72 | 54.4% |
| Multi-Agent Reasoning | 2.52 | 50.4% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Traditional ETL/data pipelines | **1st** | Industry standard. Massive ecosystem. Every team knows it. |
| Scheduled batch jobs | **2nd** | Strong scheduling. Well-proven. But Temporal/Dagster are more modern. |
| Existing Airflow shops | **1st** | Don't migrate unless pain points are severe. |

## When NOT to Use

- New projects starting fresh (consider Dagster, Prefect, or Temporal instead)
- Low-latency interactive workflows (scheduler polling is too slow)
- Durable long-running workflows (no replay/checkpoint)
- Event-driven architectures (designed for scheduled, not reactive)
- Small teams (operational complexity is high)
