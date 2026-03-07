# OpenAI Agents SDK

## What It Is

The OpenAI Agents SDK (formerly Swarm) is OpenAI's official framework for building multi-agent applications. It provides lightweight agent definitions with handoff mechanics, tool use, and guardrails. Designed for tight integration with OpenAI models and emphasizes simplicity over infrastructure complexity.

## Architecture & Orchestration Pattern

**Pattern**: Lightweight agent handoff with tool-use orchestration

```
┌────────────────────────────────────┐
│        Agents SDK Runtime         │
│  ┌──────────┐  ┌──────────┐      │
│  │  Agent A │──→│  Agent B │      │
│  │(instruct,│  │(instruct,│      │
│  │ tools)   │  │ tools)   │      │
│  └──────────┘  └──────────┘      │
│  ┌──────────────────────────┐    │
│  │      Runner              │    │
│  │  (manages agent loop)    │    │
│  └──────────────────────────┘    │
│  ┌──────────────────────────┐    │
│  │    Guardrails + Tracing  │    │
│  └──────────────────────────┘    │
└────────────────────────────────────┘
```

- **Agent**: Defined by instructions, tools, and handoff targets
- **Runner**: Executes the agent loop (call model → execute tools → check handoff → repeat)
- **Handoffs**: Agents delegate to other agents via handoff tools
- **Guardrails**: Input/output validation hooks
- **Tracing**: Built-in execution tracing for observability

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.8 | 76.0% | Medium | Direct OpenAI API calls. Minimal framework overhead. But handoff chains add round-trips. |
| Scalability | 2.8 | 56.0% | Low | In-process execution. No distributed coordination. Scales with API rate limits. |
| Efficiency | 3.2 | 64.0% | Medium | Lightweight framework. But handoff chains multiply API calls. Token usage depends on agent instructions. |
| Fault Tolerance | 2.5 | 50.0% | Medium | No built-in retry. No checkpointing. Runner loop handles tool errors. But no durable state. |
| Throughput | 2.5 | 50.0% | Medium | Sequential agent loop. No built-in parallelism for multi-agent execution. |
| Maintainability | 4.0 | 80.0% | High | Very simple API. Easy to understand. Small codebase. Good documentation. |
| Determinism | 3.5 | 70.0% | Medium | Built-in tracing provides execution history. Guardrails add predictability. But LLM non-determinism. |
| Integration Ease | 4.0 | 80.0% | Medium | OpenAI-native. Simple Python API. But vendor-locked to OpenAI models. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.70 | 74.0% |
| Batch | 2.90 | 58.0% |
| Long-Running Durable | 2.60 | 52.0% |
| Event-Driven Serverless | 3.30 | 66.0% |
| Multi-Agent Reasoning | 3.50 | 70.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Simple multi-agent with OpenAI models | **1st** | Lightest-weight option for OpenAI-native development. |
| Interactive AI assistants | **2nd** | Low latency, simple handoff. Good DX. |
| Agent prototyping | **2nd** | Fast to get started. Minimal boilerplate. |

## When NOT to Use

- Non-OpenAI model providers (vendor-locked)
- Production systems requiring durability or fault tolerance
- Complex graph-based agent workflows (use LangGraph)
- High-throughput batch agent execution
- Teams needing model provider flexibility

## Maturity Signals

- **Corporate backing**: OpenAI (direct investment)
- **Maturity**: Relatively new; evolved from Swarm (research prototype)
- **Risk**: Vendor lock-in to OpenAI models and API
- **Community**: Growing; backed by OpenAI documentation
