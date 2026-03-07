# External Engines Evaluation

Evaluation of 19 external orchestration engines across three categories: workflow engines, agent runtimes, and coding-agent fleet orchestration.

## Categories

### [Workflow Engines](workflow-engines/)
General-purpose orchestration platforms for task coordination, retries, and durable execution.

| Engine | Interactive | Batch | Durable | Event-Driven | Multi-Agent |
|--------|:----------:|:-----:|:-------:|:------------:|:-----------:|
| [Temporal](workflow-engines/temporal.md) | 70.0% | **91.4%** | **94.4%** | 72.0% | 75.6% |
| [Inngest](workflow-engines/inngest.md) | **80.0%** | 78.0% | 77.2% | **82.0%** | 72.8% |
| [Cadence](workflow-engines/cadence.md) | 66.0% | 87.6% | 90.8% | 68.0% | 72.0% |
| [Dagster](workflow-engines/dagster.md) | 71.6% | 82.4% | 70.0% | 66.4% | 58.0% |
| [Prefect](workflow-engines/prefect.md) | 74.0% | 80.0% | 68.8% | 70.4% | 58.0% |
| [Argo Workflows](workflow-engines/argo-workflows.md) | 60.0% | 84.4% | 76.0% | 60.8% | 54.0% |
| [Airflow](workflow-engines/airflow.md) | 58.0% | 80.0% | 68.0% | 54.4% | 50.4% |
| [n8n](workflow-engines/n8n.md) | 72.4% | 64.4% | 56.8% | 72.0% | 56.0% |

### [Agent Runtimes](agent-runtimes/)
Frameworks specifically designed for AI agent coordination and multi-model orchestration.

| Engine | Interactive | Batch | Durable | Event-Driven | Multi-Agent |
|--------|:----------:|:-----:|:-------:|:------------:|:-----------:|
| [LangGraph](agent-runtimes/langgraph.md) | 72.0% | 68.0% | 70.4% | 64.4% | **80.4%** |
| [Semantic Kernel](agent-runtimes/semantic-kernel.md) | 74.8% | 68.8% | 66.0% | 68.0% | 76.8% |
| [CrewAI](agent-runtimes/crewai.md) | 70.8% | 60.0% | 54.4% | 60.0% | 72.0% |
| [AutoGen](agent-runtimes/autogen.md) | 62.4% | 58.0% | 52.4% | 56.0% | 68.0% |
| [OpenAI Agents SDK](agent-runtimes/openai-agents-sdk.md) | 74.0% | 58.0% | 52.0% | 66.0% | 70.0% |
| [LlamaIndex Workflows](agent-runtimes/llamaindex-workflows.md) | 68.0% | 62.0% | 58.4% | 64.0% | 64.0% |
| [Haystack](agent-runtimes/haystack-pipelines.md) | 66.4% | 64.0% | 56.0% | 60.0% | 60.0% |
| [Flowise](agent-runtimes/flowise.md) | 70.0% | 54.0% | 48.0% | 62.0% | 58.0% |

### [Coding-Agent Fleet Orchestration](coding-agent-orchestration/)
Multi-worktree orchestrators for parallel coding agents and PR automation.

| Pattern | Interactive | Batch | Durable | Event-Driven | Multi-Agent |
|---------|:----------:|:-----:|:-------:|:------------:|:-----------:|
| [Fleet Orchestration](coding-agent-orchestration/fleet-orchestration.md) | 64.0% | 70.0% | 58.0% | 56.0% | 68.0% |

## Master Rankings by Profile

### Interactive (Developer UX)
| Rank | Engine | Score |
|:----:|--------|:-----:|
| 1st | Inngest | 80.0% |
| 2nd | Semantic Kernel | 74.8% |
| 3rd | Prefect | 74.0% |
| 3rd | OpenAI Agents SDK | 74.0% |

### Batch (Enterprise Automation)
| Rank | Engine | Score |
|:----:|--------|:-----:|
| 1st | Temporal | 91.4% |
| 2nd | Cadence | 87.6% |
| 3rd | Argo Workflows | 84.4% |

### Long-Running Durable
| Rank | Engine | Score |
|:----:|--------|:-----:|
| 1st | Temporal | 94.4% |
| 2nd | Cadence | 90.8% |
| 3rd | Inngest | 77.2% |

### Event-Driven Serverless
| Rank | Engine | Score |
|:----:|--------|:-----:|
| 1st | Inngest | 82.0% |
| 2nd | Temporal | 72.0% |
| 3rd | n8n | 72.0% |

### Multi-Agent Reasoning
| Rank | Engine | Score |
|:----:|--------|:-----:|
| 1st | LangGraph | 80.4% |
| 2nd | Semantic Kernel | 76.8% |
| 3rd | Temporal | 75.6% |

## Churn / Stability Signals

| Engine | Open PRs (approx.) | Risk Level |
|--------|:-------------------:|:----------:|
| n8n | ~947 | Very High |
| CrewAI | ~330 | High |
| LangGraph | ~206 | High |
| AutoGen | ~190 | High |
| Temporal | ~158 | Moderate (mature) |
| Inngest | ~77 | Low-Moderate |
| Dagster | ~100+ | Moderate |
| Prefect | ~100+ | Moderate |
| Argo | ~80+ | Moderate |
| Airflow | ~200+ | Moderate (mature) |
