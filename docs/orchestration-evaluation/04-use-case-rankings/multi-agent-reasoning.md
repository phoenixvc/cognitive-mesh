# Use-Case Ranking: Multi-Agent Reasoning

**Profile**: Multi-Agent Reasoning (AI Agent Collaboration)

**Priority**: Determinism (0.22) > Maintainability (0.18) > Fault Tolerance (0.16) > Integration Ease (0.14) > Latency (0.10)

## Complete Rankings

| Rank | System | Score | Category |
|:----:|--------|:-----:|----------|
| **1st** | agentkit-forge | 84.0% | Internal Repo |
| **2nd** | LangGraph | 80.4% | Agent Runtime |
| **3rd** | Semantic Kernel | 76.8% | Agent Runtime |
| 4th | Temporal | 75.6% | Workflow Engine |
| 5th | Inngest | 72.8% | Workflow Engine |
| 6th | CrewAI | 72.0% | Agent Runtime |
| 6th | Cadence | 72.0% | Workflow Engine |
| 7th | OpenAI Agents SDK | 70.0% | Agent Runtime |
| 8th | AutoGen | 68.0% | Agent Runtime |
| 8th | Fleet Orchestration | 68.0% | Coding-Agent |
| 9th | cognitive-mesh | 66.8% | Internal Repo |
| 10th | HouseOfVeritas | 66.4% | Internal Repo |
| 11th | LlamaIndex Workflows | 64.0% | Agent Runtime |
| 12th | codeflow-engine | 62.4% | Internal Repo |
| 13th | Haystack | 60.0% | Agent Runtime |
| 14th | Dagster | 58.0% | Workflow Engine |
| 14th | Prefect | 58.0% | Workflow Engine |
| 15th | Flowise | 58.0% | Agent Runtime |
| 16th | n8n | 56.0% | Workflow Engine |
| 17th | Argo Workflows | 54.0% | Workflow Engine |
| 18th | Airflow | 50.4% | Workflow Engine |

## Top 3 Analysis

### 1st — agentkit-forge (84.0%)

**Why it wins**: Perfect Determinism score (100%) dominates this profile (weight: 0.22). Explicit state machine ensures reproducible agent coordination. JSONL event log provides complete audit trail. Strong maintainability (80%).

**Best for**: Agent orchestration where auditability and reproducibility are paramount. Regulatory environments. Debugging multi-agent interactions.

**Trade-off**: No built-in LLM agent abstractions. Teams model required. File-based coordination limits real-time multi-agent chat.

### 2nd — LangGraph (80.4%)

**Why it ranks high**: Purpose-built for multi-agent graph execution. Checkpoint-based replay (84% Determinism). Supports cycles, branching, and human-in-the-loop. State management is first-class.

**Best for**: Complex agent interaction patterns. Stateful conversation agents. Multi-step reasoning with branching logic.

**Trade-off**: Python-only. LangChain ecosystem dependency. High PR churn (~206).

### 3rd — Semantic Kernel (76.8%)

**Why it ranks high**: Strong maintainability (84%). Multi-language support. Filter hooks for governance/observability. Process framework for multi-step agent workflows. Microsoft backing for stability.

**Best for**: Enterprise multi-agent applications. .NET ecosystems. Azure-integrated AI agents.

**Trade-off**: Less mature agent graph capabilities than LangGraph. No checkpointing.

## cognitive-mesh Position (9th — 66.8%)

cognitive-mesh has the richest native multi-agent model (4 coordination patterns + governance gates) but scores lower due to:
- Adapter-dependent fault tolerance (reduced to 60%)
- Active churn (14 open PRs)
- Convergence heuristic is simplistic

**If adapter implementations are verified and convergence is improved**, cognitive-mesh could rise to 3rd-4th position in this profile.

## Anti-Patterns: What NOT to Pick

- **Airflow, Argo Workflows** — No agent abstractions; designed for data/container orchestration
- **n8n** — Visual workflows resist version control and complex agent reasoning patterns
- **Dagster, Prefect** — Data pipeline tools, not agent coordination frameworks
