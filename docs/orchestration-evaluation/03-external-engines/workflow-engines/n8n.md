# n8n

## What It Is

n8n is a visual workflow automation platform with 400+ integrations and AI capabilities. It provides a node-based UI for building workflows that connect services, transform data, and trigger actions. Available as cloud-hosted or self-hosted. Strong in the "Zapier alternative" category with code extensibility.

## Architecture & Orchestration Pattern

**Pattern**: Visual node-based workflow automation with webhook/schedule triggers

```
┌──────────────────────────────┐
│         n8n Instance         │
│  ┌────────┐  ┌────────────┐ │
│  │ Editor │  │ Execution  │ │
│  │ (UI)   │  │ Engine     │ │
│  └────────┘  └────────────┘ │
│  ┌──────────────────────┐   │
│  │ DB (SQLite/Postgres) │   │
│  └──────────────────────┘   │
└──────────────────────────────┘
```

- Visual editor for workflow construction
- 400+ pre-built integration nodes
- AI nodes (LLM chains, agents, tools)
- Webhook and schedule triggers
- Code nodes for custom JavaScript/Python logic

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.5 | 70.0% | Medium | Webhook-triggered execution is fast. Internal node processing is efficient. |
| Scalability | 3.2 | 64.0% | Medium | Queue mode for scaling workers. But designed primarily for single-instance use. |
| Efficiency | 3.0 | 60.0% | Medium | Node.js runtime is efficient. But large workflows with many nodes consume memory. |
| Fault Tolerance | 3.0 | 60.0% | Medium | Per-node retry. Error handling nodes. But no durable execution or replay. |
| Throughput | 3.0 | 60.0% | Medium | Sequential node execution. Queue mode adds parallelism. But not designed for high-throughput batch. |
| Maintainability | 3.5 | 70.0% | High | Visual workflows are intuitive. But complex workflows become "spaghetti diagrams." ~947 open PRs (very high churn). Linear memory growth (2GB+ for complex workflows). |
| Determinism | 2.5 | 50.0% | Medium | Execution history. But visual workflows are harder to version/diff. No replay mechanism. |
| Integration Ease | 4.5 | 90.0% | High | 400+ integrations. REST API. Community nodes. Self-hostable. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.62 | 72.4% |
| Batch | 3.22 | 64.4% |
| Long-Running Durable | 2.84 | 56.8% |
| Event-Driven Serverless | 3.60 | 72.0% |
| Multi-Agent Reasoning | 2.80 | 56.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Integration automation (connect services) | **1st** | 400+ nodes. Visual builder. Low code barrier. |
| Visual workflow prototyping | **1st** | Best-in-class visual editor for non-developers. |
| Event-driven webhooks | 3rd | Good webhook support; Inngest better for production. |
| AI workflow chains | 3rd | AI nodes exist but purpose-built agent runtimes are richer. |

## When NOT to Use

- High-throughput batch processing (not designed for it)
- Durable long-running workflows (no replay/checkpoint)
- Version-controlled, code-first orchestration (visual workflows are hard to diff)
- Teams requiring deterministic execution guarantees
- Stability-critical environments (~947 open PRs; ~178k GitHub stars but fair-code license, not truly open source)
- Memory-intensive workloads (single-process limitation; 2GB+ for complex workflows)
