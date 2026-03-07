# Config Defaults Matrix

Unified comparison of orchestration configuration defaults across all evaluated systems.

## Internal Repositories

| Setting | agentkit-forge | codeflow-engine | cognitive-mesh | HouseOfVeritas |
|---------|---------------|-----------------|----------------|----------------|
| **Timeouts** | Session lock stale: 30m | Workflow: 300s | Adapter-defined (not shown) | Inngest-managed (not shown) |
| **Retries** | Not centrally defined | 3 attempts, delay=5s, exponential backoff | Adapter-defined (not shown) | Per-function, typically `retries: 2` |
| **Max Concurrency** | Not centrally defined | `max_concurrent=10` | No explicit cap (Task.WhenAll) | Inngest-managed (not shown) |
| **Lock TTL** | 30 minutes (`LOCK_STALE_MS`) | — | — | — |
| **History/Cap** | — | `MAX_WORKFLOW_HISTORY=1000` | — | — |
| **Iteration Limits** | — | AutoGen `max_round=10` | Swarm `maxIterations=5` | — |
| **Coordination Pattern** | 5-phase sequential | Event fan-out | CollaborativeSwarm (default) | Event + cron triggers |
| **Config Location** | orchestrator.mjs, task-protocol.mjs | config/settings.py, workflows/engine.py | MultiAgentOrchestrationEngine.cs | Individual workflow files |
| **Config Format** | Code constants | Pydantic model | Code constants | Inngest function options |

## External Workflow Engines

| Setting | Temporal | Inngest | Cadence | Argo Workflows | Prefect | Dagster | Airflow | n8n |
|---------|---------|---------|---------|----------------|---------|---------|---------|-----|
| **Timeouts** | Per-activity + per-workflow (configurable) | Step-level timeout | Per-activity + per-workflow | Pod-level + workflow-level | Task-level timeout | Op-level timeout | Task-level `execution_timeout` | Node-level timeout |
| **Retries** | Configurable per activity (policy-based) | Per-function `retries` param | Configurable per activity | `retryStrategy` with limit/backoff | `retries` param + delay | `RetryPolicy` | `retries` param (default 0) | Per-node retry |
| **Max Concurrency** | Worker-level task slots | Concurrency keys | Worker-level task slots | `parallelism` field | Task runner concurrency | Concurrency limits per op/job | `max_active_runs`, `max_active_tasks_per_dag` | Execution concurrency |
| **Idempotency** | Workflow ID deduplication | Event ID deduplication | Workflow ID deduplication | Pod deduplication | Task key-based | Run ID deduplication | Task instance idempotent by execution_date | — |
| **State Persistence** | Cassandra/MySQL/PostgreSQL | Built-in (cloud) or self-hosted | Cassandra/MySQL | Kubernetes etcd/S3/GCS | Orion DB (PostgreSQL) | PostgreSQL/SQLite | PostgreSQL/MySQL | SQLite/PostgreSQL |
| **Config Format** | Code (SDK) + namespace config | Function options + dashboard | Code (SDK) | YAML manifests | Python decorators + YAML | Python code + YAML | Python code + env vars | JSON workflow definitions |

## External Agent Runtimes

| Setting | AutoGen | CrewAI | LangGraph | Semantic Kernel | LlamaIndex Workflows | Haystack | Flowise |
|---------|---------|--------|-----------|-----------------|---------------------|----------|---------|
| **Timeouts** | Conversation-level | Task-level | Step-level | Plugin-level | Step-level | Pipeline-level | Node-level |
| **Retries** | Not built-in | Not built-in (delegated) | Checkpoint + resume | Plugin-level retry | Step retry | Not built-in | Not built-in |
| **Max Concurrency** | GroupChat `max_round` | Sequential by default | Configurable parallelism | Kernel-level | Async steps | Pipeline parallelism | — |
| **State Persistence** | In-memory (default) | In-memory | Checkpointer (SQLite/PostgreSQL) | In-memory/custom | In-memory | In-memory | Database-backed flows |
| **Config Format** | Python code | Python/YAML | Python code | C#/Python code | Python code | Python/YAML | Visual UI + JSON |

## Gap Analysis

### What's Missing Across Systems

| Gap | Affected Systems | Risk |
|-----|-----------------|------|
| No centralized retry defaults | agentkit-forge | Silent failures on external calls |
| Concurrency enforcement unclear | codeflow-engine, cognitive-mesh | Potential resource exhaustion |
| No measured latency baselines | All internal repos | Cannot validate latency scores |
| Timeout defaults not documented | cognitive-mesh, HouseOfVeritas | Unpredictable behavior under load |
| No circuit breaker config | All internal repos | Cascading failures possible |
| No idempotency strategy | agentkit-forge, cognitive-mesh | Double-processing risk |

### Recommended Minimum Config Set

Every orchestration system should define these defaults explicitly:

```
orchestration:
  timeout_seconds: <value>           # Max time per task/workflow
  retry_attempts: <value>            # Max retries before failure
  retry_delay_seconds: <value>       # Initial retry delay
  retry_backoff_multiplier: <value>  # Exponential backoff factor
  max_concurrent: <value>            # Max parallel tasks
  circuit_breaker_threshold: <value> # Failures before circuit opens
  idempotency_key: <strategy>        # How to deduplicate
  lock_ttl_seconds: <value>          # Lock expiry for coordination
```
