# Microsoft AutoGen / Agent Framework

## What It Is

AutoGen is Microsoft Research's pioneering multi-agent framework that popularized conversational agent orchestration. In October 2025, it merged with Semantic Kernel into the unified **Microsoft Agent Framework**, which is in Release Candidate status targeting GA by end of Q1 2026. It supports Python and .NET with native Azure integration.

The framework has evolved from simple chat-based group conversations to a full event-driven, graph-based orchestration platform with durable state, A2A interoperability, and enterprise governance.

## Architecture & Orchestration Pattern

**Pattern**: Event-driven, graph-based workflow orchestration with multiple coordination modes

```
┌────────────────────────────────────────────┐
│      Microsoft Agent Framework             │
│  ┌──────────────────────────────────────┐ │
│  │      Agent Runtime                   │ │
│  │  ┌────────┐  ┌────────┐  ┌───────┐ │ │
│  │  │Agent 1 │  │Agent 2 │  │Agent N│ │ │
│  │  └────────┘  └────────┘  └───────┘ │ │
│  └──────────────────────────────────────┘ │
│  Orchestration Patterns:                  │
│  ├── Sequential                           │
│  ├── Concurrent (parallel)                │
│  ├── Group Chat                           │
│  ├── Handoff                              │
│  └── Magentic (dynamic)                   │
│                                            │
│  ┌──────────────────────────────────────┐ │
│  │  State + Checkpoint + Observability  │ │
│  │  (Azure Monitor, OpenTelemetry)      │ │
│  └──────────────────────────────────────┘ │
└────────────────────────────────────────────┘
```

- **Agents**: Communicate via message-passing with centralized runtime
- **Orchestration**: Sequential, concurrent, group chat, handoff, and "magentic" (dynamic) patterns
- **State**: Session-based management for long-running and human-in-the-loop workflows
- **Governance**: Prompt shields, PII detection, task adherence (Responsible AI)
- **Interoperability**: A2A (Agent-to-Agent) and MCP (Model Context Protocol)
- **YAML/JSON**: Declarative agent definitions supported

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.8 | 76.0% | Medium | Event-driven architecture reduces polling. Message-passing adds some overhead. Azure optimization helps. |
| Scalability | 4.7 | 94.0% | High | Enterprise-scale via Azure AI Foundry. 70,000+ organizations using it. Cross-language distributed runtimes. |
| Efficiency | 3.2 | 64.0% | Medium | Full conversation history per agent adds token overhead. Framework abstractions add memory usage. |
| Fault Tolerance | 4.5 | 90.0% | High | Checkpoint-based state persistence at critical nodes. Sub-workflow nesting. Crash recovery via durable state. Enterprise-grade. |
| Throughput | 4.2 | 84.0% | Medium | Concurrent orchestration pattern runs agents in parallel. Cross-language runtime enables distributed processing. |
| Maintainability | 3.0 | 60.0% | Medium | Large API surface. Merger creates migration complexity. Framework is pre-GA and evolving. Steep learning curve. |
| Determinism | 4.3 | 86.0% | High | Graph-based workflows with explicit execution paths. Azure Monitor + OpenTelemetry observability. Checkpoint-based audit trail. |
| Integration Ease | 4.0 | 80.0% | High | OpenAPI, MCP, A2A. Azure ecosystem. Multi-language (C#, Python, Java). ~55k GitHub stars. But merger churn. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.70 | 74.0% |
| Batch | 4.20 | 84.0% |
| Long-Running Durable | 4.18 | 83.6% |
| Event-Driven Serverless | 3.58 | 71.6% |
| Multi-Agent Reasoning | 3.82 | 76.4% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Enterprise multi-agent (Azure/.NET) | **1st** | Purpose-built. Azure backing. .NET/C# primary language. Enterprise governance. |
| Complex multi-agent with durability | **2nd** | Checkpoint-based state. Behind Temporal for workflow durability but ahead of all other agent runtimes. |
| Batch agent automation | **2nd** | Strong scalability + fault tolerance. Good for unattended agent workflows. |

## When NOT to Use

- Quick prototyping (API surface too large; use OpenAI Agents SDK or CrewAI)
- Teams wanting lightweight/minimal dependencies
- Non-Microsoft ecosystems where Azure lock-in is a concern
- Pre-GA status means API may still shift — risk for production systems built today

## Maturity Signals

- **GitHub stars**: ~55k (AutoGen repo), ~27k (Semantic Kernel repo)
- **Contributors**: 559
- **Status**: Release Candidate (March 2026), GA targeted Q1 2026
- **Corporate backing**: Microsoft (direct investment, Azure AI Foundry)
- **Production users**: 70,000+ organizations on Azure AI Foundry
- **Risk**: Merger creates migration complexity for existing AutoGen or SK users
