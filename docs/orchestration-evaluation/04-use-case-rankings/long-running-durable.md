# Use-Case Ranking: Long-Running Durable Workflows

**Profile**: Long-Running Durable (Workflows That Survive Restarts)

**Priority**: Fault Tolerance (0.28) > Determinism (0.20) > Scalability (0.15) > Throughput (0.10)

## Complete Rankings

| Rank | System | Score | Category |
|:----:|--------|:-----:|----------|
| **1st** | Temporal | 94.4% | Workflow Engine |
| **2nd** | Cadence | 90.8% | Workflow Engine |
| **3rd** | agentkit-forge | 78.8% | Internal Repo |
| 4th | Inngest | 77.2% | Workflow Engine |
| 5th | Argo Workflows | 76.0% | Workflow Engine |
| 6th | LangGraph | 70.4% | Agent Runtime |
| 7th | Dagster | 70.0% | Workflow Engine |
| 8th | HouseOfVeritas | 69.6% | Internal Repo |
| 9th | Prefect | 68.8% | Workflow Engine |
| 10th | Airflow | 68.0% | Workflow Engine |
| 11th | codeflow-engine | 67.2% | Internal Repo |
| 12th | Semantic Kernel | 66.0% | Agent Runtime |
| 13th | cognitive-mesh | 63.2% | Internal Repo |
| 14th | LlamaIndex Workflows | 58.4% | Agent Runtime |
| 15th | n8n | 56.8% | Workflow Engine |
| 16th | Haystack | 56.0% | Agent Runtime |
| 17th | Fleet Orchestration | 54.4% | Coding-Agent |
| 17th | CrewAI | 54.4% | Agent Runtime |
| 19th | AutoGen | 52.4% | Agent Runtime |
| 20th | Flowise | 48.0% | Agent Runtime |
| 21st | OpenAI Agents SDK | 43.6% | Agent Runtime |

## Top 3 Analysis

### 1st — Temporal (94.4%)

**Why it wins**: Deterministic replay from event history is the gold standard for durable execution. Workflows literally survive process crashes and resume from the exact point of failure. Fault tolerance (100%) + Determinism (100%) = dominant in this profile.

**Best for**: Multi-day approval workflows, saga patterns, financial settlement, any process that must complete despite infrastructure failures.

**Trade-off**: Workflows must be deterministic (no random, no current time, no direct I/O in workflow code).

### 2nd — Cadence (90.8%)

**Why it ranks high**: Same deterministic replay guarantees. Same durability model.

**Best for**: Existing Cadence deployments. Same use cases as Temporal.

**Trade-off**: Smaller ecosystem. Choose Temporal for new projects.

### 3rd — agentkit-forge (78.8%)

**Why it ranks high**: File-based state persistence survives process restarts. Deterministic lifecycle (100%) provides reproducibility. Lock TTL prevents stuck workflows.

**Best for**: Developer tool orchestration where simplicity matters more than enterprise scale.

**Trade-off**: No deterministic replay (just state persistence). File-based limits scale.

## Anti-Patterns: What NOT to Pick

- **Flowise, OpenAI Agents SDK, legacy AutoGen (in-memory only), CrewAI** — All in-memory, no durability. Note: the merged Microsoft Agent Framework (successor to legacy AutoGen) provides checkpoint-based durability and scores significantly higher (83.6%).
- **n8n** — No checkpoint/replay; visual workflows resist long-running state management
