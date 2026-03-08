# Flowise

## What It Is

Flowise is a visual drag-and-drop tool for building LLM applications and chatbots. It provides a node-based UI similar to n8n but focused on AI/LLM workflows. Supports LangChain and LlamaIndex components. Best suited for rapid prototyping and low-code AI development.

## Architecture & Orchestration Pattern

**Pattern**: Visual node-based AI workflow builder

```
┌─────────────────────────────────┐
│        Flowise Instance        │
│  ┌────────┐  ┌──────────────┐ │
│  │ Visual │  │  Execution   │ │
│  │ Editor │  │  Engine      │ │
│  └────────┘  └──────────────┘ │
│  ┌──────────────────────────┐ │
│  │ DB (SQLite/MySQL/PG)    │ │
│  └──────────────────────────┘ │
└─────────────────────────────────┘
```

- Visual drag-and-drop workflow construction
- LangChain and LlamaIndex component integration
- Chat, API, and embed deployment modes
- Credential management for API keys
- Marketplace for community-built flows

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.5 | 70.0% | Medium | Direct API calls to LLM providers. Visual layer is UI-only; execution is standard. |
| Scalability | 2.5 | 50.0% | Medium | Single-instance by design. No distributed execution. Scales via deployment replicas. |
| Efficiency | 2.8 | 56.0% | Low | Node.js runtime. Visual workflows add metadata overhead. LLM calls dominate cost. |
| Fault Tolerance | 2.2 | 44.0% | Medium | No built-in retry. No checkpointing. Flow fails if any node fails. |
| Throughput | 2.2 | 44.0% | Medium | Sequential node execution. API-level concurrency only. |
| Maintainability | 3.5 | 70.0% | Medium | Visual flows are intuitive for simple cases. Complex flows become unmanageable. Hard to version control. |
| Determinism | 2.5 | 50.0% | Low | Execution logs exist. But visual workflows are hard to diff. No replay. |
| Integration Ease | 3.8 | 76.0% | Medium | LangChain/LlamaIndex components. API deployment. Credential management. Self-hostable. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.50 | 70.0% |
| Batch | 2.70 | 54.0% |
| Long-Running Durable | 2.40 | 48.0% |
| Event-Driven Serverless | 3.10 | 62.0% |
| Multi-Agent Reasoning | 2.90 | 58.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Rapid AI chatbot prototyping | **1st** | Fastest visual path to a working chatbot. |
| Low-code AI workflow building | **1st** | Best for non-developers or quick demos. |
| LangChain visual composition | **2nd** | Visual alternative to writing LangChain code. |

## When NOT to Use

- Production systems requiring reliability or scale
- Complex agent orchestration (use LangGraph)
- Version-controlled, code-first development
- High-throughput or batch workloads
- Teams with strong engineering practices (visual flows resist good software practices)
