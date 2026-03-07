# Use-Case Ranking: Interactive Developer UX

**Profile**: Interactive (Developer UX / Human-in-the-Loop)

**Priority**: Latency (0.22) > Integration Ease (0.18) > Maintainability (0.16) > Fault Tolerance (0.12)

## Complete Rankings

| Rank | System | Score | Category |
|:----:|--------|:-----:|----------|
| **1st** | Inngest | 80.0% | Workflow Engine |
| **2nd** | agentkit-forge | 78.6% | Internal Repo |
| **3rd** | Semantic Kernel | 74.8% | Agent Runtime |
| 4th | Prefect | 74.0% | Workflow Engine |
| 4th | OpenAI Agents SDK | 74.0% | Agent Runtime |
| 5th | n8n | 72.4% | Workflow Engine |
| 6th | LangGraph | 72.0% | Agent Runtime |
| 7th | Dagster | 71.6% | Workflow Engine |
| 8th | CrewAI | 70.8% | Agent Runtime |
| 9th | Temporal | 70.0% | Workflow Engine |
| 9th | Flowise | 70.0% | Agent Runtime |
| 10th | LlamaIndex Workflows | 68.0% | Agent Runtime |
| 11th | Haystack | 66.4% | Agent Runtime |
| 12th | Cadence | 66.0% | Workflow Engine |
| 13th | HouseOfVeritas | 64.2% | Internal Repo |
| 14th | codeflow-engine | 64.4% | Internal Repo |
| 15th | Fleet Orchestration | 64.0% | Coding-Agent |
| 16th | AutoGen | 62.4% | Agent Runtime |
| 17th | cognitive-mesh | 62.0% | Internal Repo |
| 18th | Argo Workflows | 60.0% | Workflow Engine |
| 19th | Airflow | 58.0% | Workflow Engine |

## Top 3 Analysis

### 1st — Inngest (80.0%)

**Why it wins**: Lowest latency overhead for event-driven interactions. HTTP-based integration is trivial. Step functions provide durability without infrastructure complexity. Excellent developer experience with TypeScript support.

**Best for**: Next.js/serverless developer tools, webhook-driven workflows, rapid prototyping with production-grade reliability.

**Trade-off**: Less powerful than Temporal for complex durable workflows. Not ideal for heavy batch processing.

### 2nd — agentkit-forge (78.6%)

**Why it ranks high**: Deterministic lifecycle (100% Determinism) gives developers predictable behavior. File-based coordination has near-zero latency. Strong security hardening.

**Best for**: CLI-based developer tools, code generation pipelines, spec-driven automation.

**Trade-off**: File-based coordination limits multi-host scaling. No built-in fan-out.

### 3rd — Semantic Kernel (74.8%)

**Why it ranks high**: In-process execution is fast. Multi-language support (C#, Python, Java). Strong Microsoft/enterprise ecosystem integration.

**Best for**: .NET developer tools, AI-assisted IDE features, enterprise AI applications.

**Trade-off**: No built-in durability. Limited agent graph capabilities compared to LangGraph.

## Anti-Patterns: What NOT to Pick

- **Airflow** — Scheduler polling interval (5s default) makes it unacceptable for interactive use
- **Argo Workflows** — Pod startup time adds seconds of latency per step
- **AutoGen** — Chat-based coordination adds unnecessary round-trips for interactive tools
