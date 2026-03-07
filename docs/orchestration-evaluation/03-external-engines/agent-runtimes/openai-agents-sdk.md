# OpenAI Agents SDK

## What It Is

The OpenAI Agents SDK (evolved from the experimental Swarm project, March 2025) is OpenAI's lightweight, minimalist framework for multi-agent workflows. Built on four core primitives: Agents, Handoffs, Tools, and Guardrails. Available in Python and TypeScript with feature parity. Designed for tight integration with OpenAI models while supporting 100+ LLMs via Chat Completions API.

## Architecture & Orchestration Pattern

**Pattern**: Minimalist agent loop with handoff-based delegation

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
│  │  reasoning → tool call → │    │
│  │  validation → next step  │    │
│  └──────────────────────────┘    │
│  ┌──────────────────────────┐    │
│  │  Guardrails + Tracing    │    │
│  └──────────────────────────┘    │
└────────────────────────────────────┘
```

- **Agent**: Defined by instructions, tools, and handoff targets
- **Runner**: Executes the agent loop (LLM drives control flow)
- **Handoffs**: Agents delegate to other agents via handoff tools
- **Guardrails**: Input/output validation hooks for governance
- **Tracing**: Built-in execution tracing and visualization
- **Realtime Agents**: Voice agent support

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 4.3 | 86.0% | Medium | Direct OpenAI API calls. Minimal framework overhead. Lightweight footprint. But handoff chains add round-trips. |
| Scalability | 2.5 | 50.0% | Medium | Stateless and lightweight SDK. No built-in distributed execution. Scales only with developer infrastructure. |
| Efficiency | 4.5 | 90.0% | Medium | Minimal dependency footprint. Low overhead per agent. No heavyweight abstractions. |
| Fault Tolerance | 1.5 | 30.0% | High | **Intentionally minimal.** No durable execution, no checkpoint/resume, no crash recovery. Short-term session history only. Developers must layer in their own resilience. |
| Throughput | 3.2 | 64.0% | Medium | Multi-agent parallel workflows supported. No concurrency primitives beyond async/await. LLM-bound. |
| Maintainability | 4.5 | 90.0% | High | Very simple API. Four primitives. Minimal boilerplate. Easy to understand. Small codebase. |
| Determinism | 2.5 | 50.0% | Medium | Built-in tracing. Guardrails add predictability. But LLM-driven control flow reduces determinism fundamentally. |
| Integration Ease | 3.8 | 76.0% | Medium | MCP tool support. Provider-agnostic claim. Python + TypeScript. But optimized for OpenAI models. Pre-1.0 (v0.10.2). |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.82 | 76.4% |
| Batch | 2.62 | 52.4% |
| Long-Running Durable | 2.18 | 43.6% |
| Event-Driven Serverless | 3.56 | 71.2% |
| Multi-Agent Reasoning | 3.14 | 62.8% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Rapid agent prototyping | **1st** | Fastest path to a working multi-agent system. Four primitives. Minimal boilerplate. |
| Simple multi-agent handoff | **1st** | Lightest-weight option for handoff patterns. |
| Interactive AI assistants | **2nd** | Lowest latency. Simple integration. Good DX. |
| Voice/realtime agents | **1st** | Built-in Realtime Agent support. |

## When NOT to Use

- Production systems needing fault tolerance (score: 1.5/5 = 30%)
- Long-running or durable workflows
- Complex stateful agent workflows requiring checkpointing
- When vendor independence from OpenAI matters (despite provider-agnostic claims, optimized for OpenAI)
- High-throughput batch agent execution
- Pre-1.0 API creates instability risk

## Maturity Signals

- **GitHub stars**: ~19k (Python), ~2.4k (TypeScript)
- **Version**: v0.10.2 (Feb 2026) — still pre-1.0
- **Release cadence**: Active bi-weekly releases
- **Corporate backing**: OpenAI
- **Risk**: Pre-1.0 API instability; LLM-driven control flow limits determinism
