# Decision Tree: Choosing an Orchestration Engine

Use this flowchart to narrow down the best orchestration approach for your workload.

## Primary Decision Flow

```
START: What is your primary workload type?
│
├── Interactive (human-in-the-loop, developer tools, CLIs)
│   │
│   ├── Need multi-agent AI coordination?
│   │   ├── YES → LangGraph (1st) or Semantic Kernel (2nd)
│   │   │         If governance required → cognitive-mesh patterns
│   │   └── NO  → agentkit-forge patterns (1st) or Inngest (2nd)
│   │
│   └── Need < 50ms coordination latency?
│       ├── YES → In-process orchestration (agentkit-forge, custom)
│       └── NO  → Inngest or lightweight workflow engine
│
├── Batch (ETL, scheduled jobs, background processing)
│   │
│   ├── Need durable execution (survive crashes)?
│   │   ├── YES → Temporal (1st) or Cadence (2nd)
│   │   └── NO  → Prefect (1st) or Dagster (2nd)
│   │
│   ├── Already on Kubernetes?
│   │   ├── YES → Argo Workflows (1st) or Airflow on K8s (2nd)
│   │   └── NO  → Prefect (1st) or Dagster (2nd)
│   │
│   └── Need complex DAG orchestration?
│       ├── YES → Airflow (1st) or Dagster (2nd)
│       └── NO  → Prefect (1st) or Inngest (2nd)
│
├── Long-running durable (sagas, approval chains, multi-day)
│   │
│   └── This is Temporal's sweet spot
│       ├── 1st choice: Temporal
│       ├── 2nd choice: Cadence
│       └── 3rd choice: Inngest (for simpler step-function patterns)
│
├── Event-driven serverless (webhooks, event streams, functions)
│   │
│   ├── On serverless platform (Vercel, AWS Lambda, etc.)?
│   │   ├── YES → Inngest (1st) or AWS Step Functions (2nd)
│   │   └── NO  → n8n (1st, if visual workflow needed)
│   │           → Temporal (1st, if durability needed)
│   │
│   └── Need visual workflow builder?
│       ├── YES → n8n (1st) or Flowise (2nd, AI-focused)
│       └── NO  → Inngest (1st) or custom event handlers
│
└── Multi-agent AI reasoning (LLM coordination, debate, consensus)
    │
    ├── Need governance / ethics gates?
    │   ├── YES → cognitive-mesh patterns (1st)
    │   └── NO  → See below
    │
    ├── Need structured agent graphs?
    │   ├── YES → LangGraph (1st) or Semantic Kernel (2nd)
    │   └── NO  → CrewAI (1st, role-based) or AutoGen (2nd, chat-based)
    │
    └── Need coding-agent parallelism?
        ├── YES → Fleet orchestration (worktree-based)
        └── NO  → LangGraph (1st) or CrewAI (2nd)
```

## Secondary Decision Factors

After narrowing to 2-3 candidates via the primary flow, apply these filters:

### Language / Runtime Constraints

| If your stack is... | Prefer... | Avoid... |
|--------------------|-----------|----------|
| Python | Prefect, Dagster, LangGraph, CrewAI | agentkit-forge (Node.js) |
| Node.js / TypeScript | Inngest, agentkit-forge, n8n | Dagster, Prefect |
| .NET / C# | Semantic Kernel, Temporal (.NET SDK), cognitive-mesh | Python-native tools |
| JVM (Java/Kotlin) | Temporal, Cadence | Inngest, n8n |
| Polyglot | Temporal (multi-SDK), n8n (HTTP) | Single-language frameworks |

### Operational Complexity Budget

| Complexity tolerance | Prefer... | Avoid... |
|---------------------|-----------|----------|
| Minimal (no infra team) | Inngest Cloud, Prefect Cloud | Self-hosted Temporal, Airflow |
| Moderate (small ops team) | Dagster Cloud, n8n Cloud | Raw Temporal, Argo |
| High (dedicated platform team) | Temporal, Argo Workflows | Inngest (overkill) |

### Vendor Dependency Tolerance

| Tolerance | Prefer... | Avoid... |
|-----------|-----------|----------|
| No vendor lock-in | Temporal (OSS), Airflow, Argo | Inngest Cloud, proprietary |
| Acceptable for productivity | Inngest, Prefect Cloud, Dagster Cloud | — |
| Enterprise vendor required | Temporal Cloud, Airflow (managed) | Community-only projects |

## Quick Reference: Top 3 by Use Case

| Use Case | 1st | 2nd | 3rd |
|----------|-----|-----|-----|
| Interactive developer tools | agentkit-forge | Inngest | LangGraph |
| Batch ETL / data pipelines | Temporal | Prefect | Dagster |
| Long-running sagas | Temporal | Cadence | Inngest |
| Event-driven serverless | Inngest | n8n | Temporal |
| Multi-agent AI | LangGraph | Semantic Kernel | cognitive-mesh |
| Coding-agent fleets | Fleet orchestration | LangGraph | Custom |
| Visual workflow builder | n8n | Flowise | Prefect |
| Kubernetes-native | Argo Workflows | Airflow (K8s) | Temporal |
