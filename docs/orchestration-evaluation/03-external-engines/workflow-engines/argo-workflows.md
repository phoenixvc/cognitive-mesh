# Argo Workflows

## What It Is

Argo Workflows is a Kubernetes-native workflow orchestration engine that defines workflows as Kubernetes CRDs (Custom Resource Definitions). Each workflow step runs as a container/pod, making it ideal for containerized batch processing and CI/CD pipelines.

## Architecture & Orchestration Pattern

**Pattern**: Kubernetes-native DAG/step orchestration with container-based execution

```text
┌─────────────────────────────────────┐
│        Kubernetes Cluster          │
│  ┌──────────────────────────┐     │
│  │   Argo Workflow Controller│     │
│  │   (watches CRDs)         │     │
│  └──────────────────────────┘     │
│  ┌──────┐ ┌──────┐ ┌──────┐     │
│  │ Pod  │ │ Pod  │ │ Pod  │     │
│  │Step1 │ │Step2 │ │Step3 │     │
│  └──────┘ └──────┘ └──────┘     │
│  ┌──────────────────────────┐     │
│  │ Artifact Storage (S3/GCS)│     │
│  └──────────────────────────┘     │
└─────────────────────────────────────┘
```

- Workflows defined in YAML as Kubernetes CRDs
- Each step = a container with inputs/outputs
- DAG and step-based execution
- Artifact passing between steps via S3/GCS

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.0 | 60.0% | High | Pod startup time (seconds). Not suitable for low-latency interactive use. |
| Scalability | 4.8 | 96.0% | High | Kubernetes-native scaling. Pod-based parallelism. Namespace isolation. |
| Efficiency | 3.5 | 70.0% | Medium | Pod overhead per step. Resource requests/limits configurable. Container image pull adds latency. |
| Fault Tolerance | 4.2 | 84.0% | High | RetryStrategy with limit/backoff. Pod-level restart policies. Artifact persistence. But no deterministic replay. |
| Throughput | 4.5 | 90.0% | High | DAG parallelism. Configurable `parallelism` field. Pod-based execution scales with cluster. |
| Maintainability | 3.0 | 60.0% | Medium | YAML-heavy. Complex workflows become verbose. Kubernetes knowledge required. |
| Determinism | 3.5 | 70.0% | Medium | Workflow status tracked in etcd. Artifact lineage. But no deterministic replay like Temporal. |
| Integration Ease | 3.5 | 70.0% | Medium | Kubernetes-native is a pro (if on K8s) and con (if not). REST API. Argo Events for triggers. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.00 | 60.0% |
| Batch | 4.22 | 84.4% |
| Long-Running Durable | 3.80 | 76.0% |
| Event-Driven Serverless | 3.04 | 60.8% |
| Multi-Agent Reasoning | 2.70 | 54.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Kubernetes-native batch processing | **1st** | Purpose-built. Pod-per-step is natural for containerized workloads. |
| CI/CD pipelines | **1st** | Natural fit with container-based build/test/deploy. |
| Batch enterprise automation | **3rd** | Strong for container workloads; Temporal better for code-first. |
| Data pipelines | 3rd | Works but Dagster/Prefect have better data abstractions. |

## When NOT to Use

- Non-Kubernetes environments (requires K8s)
- Low-latency interactive workflows (pod startup overhead)
- Simple function-based workflows (too much infrastructure)
- Teams without Kubernetes expertise
