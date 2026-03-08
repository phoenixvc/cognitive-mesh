# Inngest

## What It Is

Inngest is an event-driven workflow engine optimized for serverless and edge environments. It provides durable step functions, automatic retries, and event-based triggers without requiring infrastructure management. Available as cloud-hosted or self-hosted.

## Architecture & Orchestration Pattern

**Pattern**: Event-driven step functions with durable execution

```text
┌─────────────────────────────────────┐
│          Inngest Platform           │
│  ┌──────────┐  ┌────────────────┐  │
│  │  Event    │  │  Execution     │  │
│  │  Router   │  │  Engine        │  │
│  │          │  │  (step state)  │  │
│  └──────────┘  └────────────────┘  │
│  ┌──────────────────────────────┐  │
│  │     Event Store + Queue      │  │
│  └──────────────────────────────┘  │
└─────────────────────────────────────┘
         ↕ HTTP
┌──────────────────┐
│  Your App        │
│  serve({         │
│    functions: [] │
│  })              │
└──────────────────┘
```

- **Functions**: Defined inline in your app with triggers and step logic
- **Steps**: Each `step.run()` is durably stored; function resumes from last step on retry
- **Events**: Typed payloads that trigger function execution
- **Concurrency**: Configurable per-function concurrency keys, throttling, debouncing, rate limiting
- **AgentKit**: Multi-agent networks in TypeScript with deterministic routing and MCP integration

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 4.0 | 80.0% | Medium | HTTP-based invocation is fast. Event ingestion to handler start is typically < 100ms. No heavy polling. |
| Scalability | 4.0 | 80.0% | Medium | Cloud-managed scaling. Self-hosted scaling depends on deployment. Concurrency keys enable per-key scaling. |
| Efficiency | 4.0 | 80.0% | Medium | Pay-per-invocation model (cloud). Minimal resource overhead per function. Step-level execution avoids re-running completed work. |
| Fault Tolerance | 4.0 | 80.0% | High | Per-function retry config. Step-level durability. Failed steps resume without re-running completed steps. |
| Throughput | 4.0 | 80.0% | Medium | Concurrent function execution. Configurable concurrency limits per function. Event batching support. |
| Maintainability | 4.0 | 80.0% | High | Simple function-based model. Steps are plain code. Low learning curve. Good TypeScript support. |
| Determinism | 4.0 | 80.0% | Medium | Step-level replay. Event IDs for deduplication. Function run history. But less rigorous than Temporal's full replay. |
| Integration Ease | 4.0 | 80.0% | High | Works with any HTTP framework (Next.js, Express, etc.). Simple `serve()` integration. ~77 open PRs (manageable). Good docs. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 4.00 | 80.0% |
| Batch | 3.90 | 78.0% |
| Long-Running Durable | 3.86 | 77.2% |
| Event-Driven Serverless | 4.10 | 82.0% |
| Multi-Agent Reasoning | 3.64 | 72.8% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Event-driven serverless | **1st** | Purpose-built for this. Zero infrastructure overhead. |
| Interactive developer tools | **1st** | Low latency, simple integration, good DX. |
| Batch enterprise automation | 3rd | Works well but less optimized than Temporal/Argo for heavy batch. |
| Long-running durable workflows | 3rd | Step functions provide durability; less replay rigor than Temporal. |
| Multi-agent AI coordination | 5th | No native agent abstractions; agents would be modeled as step functions. |

## When NOT to Use

- Workflows requiring sub-10ms coordination latency (HTTP overhead)
- Complex DAG orchestration with many interdependencies (Temporal or Dagster better fit)
- Kubernetes-native environments that prefer YAML-defined workflows (Argo better fit)
- Teams requiring full deterministic replay guarantees (Temporal)

## Config Defaults

| Setting | Default | Configurable |
|---------|---------|:------------:|
| Step retries | 4 per step (5 total attempts) | Yes |
| Step timeout | Up to 2 hours max | Yes |
| Concurrency limit | Unlimited (configurable per function) | Yes |
| Event batch size | 1 (configurable) | Yes |
| Debounce | None (configurable) | Yes |

## Maturity Signals

- **GitHub stars**: ~4.8k
- **Open PRs**: ~77
- **Release cadence**: Regular; self-hosting available since 1.0
- **Corporate backing**: Inngest Inc. (venture-funded)
- **License**: SSPL with delayed Apache 2.0 (SSPL is restrictive for some use cases)
- **Production users**: Growing; strong Next.js/Vercel ecosystem adoption
- **AI support**: AgentKit for multi-agent networks with MCP integration
- **Community**: Active Discord, good documentation
