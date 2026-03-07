# Use-Case Ranking: Event-Driven Serverless

**Profile**: Event-Driven Serverless (Reactive, Function-Based Workloads)

**Priority**: Latency (0.20) > Integration Ease (0.20) > Efficiency (0.15) > Fault Tolerance (0.12) = Throughput (0.12)

## Complete Rankings

| Rank | System | Score | Category |
|:----:|--------|:-----:|----------|
| **1st** | Inngest | 82.0% | Workflow Engine |
| **2nd** | agentkit-forge | 79.2% | Internal Repo |
| **3rd** | Temporal | 72.0% | Workflow Engine |
| 3rd | n8n | 72.0% | Workflow Engine |
| 4th | Prefect | 70.4% | Workflow Engine |
| 5th | Cadence | 68.0% | Workflow Engine |
| 5th | Semantic Kernel | 68.0% | Agent Runtime |
| 6th | Dagster | 66.4% | Workflow Engine |
| 6th | OpenAI Agents SDK | 66.0% | Agent Runtime |
| 7th | LangGraph | 64.4% | Agent Runtime |
| 7th | LlamaIndex Workflows | 64.0% | Agent Runtime |
| 8th | Flowise | 62.0% | Agent Runtime |
| 8th | codeflow-engine | 62.0% | Internal Repo |
| 9th | cognitive-mesh / HouseOfVeritas | 61.2% | Internal Repo |
| 10th | Argo Workflows | 60.8% | Workflow Engine |
| 11th | Haystack | 60.0% | Agent Runtime |
| 11th | CrewAI | 60.0% | Agent Runtime |
| 12th | Fleet Orchestration | 56.0% | Coding-Agent |
| 13th | AutoGen | 56.0% | Agent Runtime |
| 14th | Airflow | 54.4% | Workflow Engine |

## Top 3 Analysis

### 1st — Inngest (82.0%)

**Why it wins**: Purpose-built for serverless event processing. HTTP-based integration works with any framework. Pay-per-invocation efficiency. Step-level durability provides reliability without infrastructure complexity.

**Best for**: Webhook handlers, event stream processing, serverless function orchestration, Next.js/Vercel-based applications.

**Trade-off**: Less control than Temporal for complex workflows. Self-hosted requires infrastructure.

### 2nd — agentkit-forge (79.2%)

**Why it ranks high**: Low-latency local coordination. Strong integration contracts via YAML specs. Deterministic routing.

**Best for**: Event-driven developer tool pipelines. Local event processing with file-based state.

**Trade-off**: File-based coordination isn't truly serverless. No cloud-native scaling.

### 3rd — Temporal (72.0%) / n8n (72.0%)

**Temporal**: Can handle events but adds infrastructure overhead. Best when events trigger durable workflows.

**n8n**: Strong webhook support. Visual builder for event-driven automations. 400+ integrations. But high churn and no durable execution.

## Anti-Patterns: What NOT to Pick

- **Airflow** — Scheduler-based, not event-reactive. 5s polling interval.
- **Argo Workflows** — Pod startup latency disqualifies it for reactive event handling
- **AutoGen** — Chat-based coordination adds unnecessary overhead for event processing
