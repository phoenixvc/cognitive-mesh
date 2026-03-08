# Letta (formerly MemGPT)

## What It Is

Letta is an open-source platform for building stateful AI agents with advanced memory management. Originally the MemGPT research project, Letta provides a production infrastructure where agents can remember, learn, and self-improve over time through a hierarchical memory architecture (core, recall, archival) with self-editing capabilities.

## Architecture & Orchestration Pattern

**Pattern**: LLM-as-Operating-System with hierarchical memory and self-editing agent loops

```
┌─────────────────────────────────────────────────────┐
│                     Letta Platform                    │
│                                                      │
│  ┌──────────────────────────────────────────┐        │
│  │           Agent Runtime                   │       │
│  │  - ReAct-inspired reasoning loop          │       │
│  │  - Tool execution                         │       │
│  │  - Multi-agent orchestration              │       │
│  │  - Programmatic tool calling              │       │
│  └──────────────────────────────────────────┘        │
│                                                      │
│  Memory Hierarchy:                                   │
│  ┌────────┐ ┌────────┐ ┌──────────────┐             │
│  │  Core  │ │ Recall │ │   Archival   │             │
│  │Always  │ │Search- │ │ Large-scale  │             │
│  │in-ctx  │ │able    │ │ persistent   │             │
│  │Self-   │ │convo   │ │ knowledge    │             │
│  │editing │ │history │ │ store        │             │
│  └────────┘ └────────┘ └──────────────┘             │
│                                                      │
│  State Persistence: Database-backed (not in-memory)  │
│  Conversations API: Shared memory across sessions    │
│  Context Repos: Git-based context versioning         │
│  Models: Model-agnostic (Opus 4.5, GPT-5 recommended) │
│  SDKs: Python, TypeScript                            │
└──────────────────────────────────────────────────────┘
```

## Key Features for Agent Orchestration

- **Hierarchical memory**: Core memory (always in-context, self-editing), recall memory (searchable conversation history), archival memory (large-scale persistent storage)
- **Self-editing memory**: Agents actively manage their own memory using tools — a key MemGPT innovation
- **Database-backed state**: Unlike most frameworks that keep state in Python variables, Letta persists agent state in databases
- **Conversations API**: Shared memory across parallel user experiences (launched Jan 2026)
- **Context Repositories**: Git-based versioning for agent context (launched Feb 2026)
- **Model-agnostic**: Supports any LLM; recommends Claude Opus 4.5 and GPT-5.2 for best performance
- **Letta Code**: Memory-first coding agent, #1 model-agnostic agent on Terminal-Bench (Dec 2025)
- **Programmatic tool calling**: Agents generate their own workflows via API (Dec 2025)
- **Letta Evals**: Open-source evaluation framework for stateful agents (Oct 2025)

## Fault Tolerance

- Database-backed state survives process restarts
- Memory persistence ensures no conversation data loss
- No explicit retry policies or circuit breakers at orchestration level
- State recovery depends on database backend reliability

## Scalability

- Server-based architecture (Letta server) — can run locally or cloud-hosted
- Database backend enables multi-instance deployment
- No documented auto-scaling or elastic infrastructure

## Concurrency / Throughput

| Parameter | Default | Configurable |
|-----------|---------|:------------:|
| Concurrent agents | Server-limited | Yes |
| Memory search latency | Database-dependent | — |
| Context window management | Automatic (core MemGPT innovation) | Yes |
| Agent state persistence | Always (database-backed) | — |

## Integration / Plugin Architecture

- **SDKs**: Python, TypeScript
- **API**: Full-featured REST API for agent management
- **Tool ecosystem**: Agents can call external tools; programmatic tool calling enables workflow generation
- **MCP support**: Not documented (uses own tool protocol)
- **No .NET SDK**: Gap for cognitive-mesh integration

## Config Defaults

| Setting | Default | Configurable |
|---------|---------|:------------:|
| Memory tiers | Core + Recall + Archival | Yes (can configure per agent) |
| State persistence | Database-backed (always on) | — |
| Model | User-specified (recommends Opus 4.5 / GPT-5.2) | Yes |
| Context window | Managed by MemGPT architecture | Yes |

## Maturity Signals

- **GitHub stars**: ~14k (letta-ai/letta)
- **License**: Apache 2.0
- **Origin**: UC Berkeley research (MemGPT paper, NeurIPS 2023)
- **Corporate backing**: Letta Inc. (venture-funded, spun out of MemGPT research)
- **Release cadence**: Active monthly releases (V1 agent architecture, Letta Code, Conversations API, Context Repos)
- **Community**: Growing; research-backed credibility
- **Letta Code**: #1 model-agnostic agent on Terminal-Bench

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.0 | 60.0% | Medium | Memory search adds latency. Context management overhead. Model inference dominates. |
| Scalability | 3.0 | 60.0% | Low | Database-backed state enables multi-instance. No auto-scaling documentation. |
| Efficiency | 3.5 | 70.0% | Medium | Context window management is the core innovation — efficiently uses limited context. |
| Fault Tolerance | 3.5 | 70.0% | Medium | Database-backed state persistence. No explicit retry/circuit breaker. |
| Throughput | 2.5 | 50.0% | Low | Single-agent focus. Multi-agent support exists but not throughput-optimized. |
| Maintainability | 3.5 | 70.0% | Medium | Clean API. Memory hierarchy is well-documented. Research-backed architecture. |
| Determinism | 3.5 | 70.0% | Medium | Database-backed state provides audit trail. Self-editing memory is deterministic in state management. |
| Integration Ease | 3.0 | 60.0% | Medium | REST API + Python/TypeScript SDKs. No .NET. Limited enterprise connectors. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.15 | 63.0% |
| Batch | 3.09 | 61.8% |
| Long-Running Durable | 3.28 | 65.6% |
| Event-Driven Serverless | 3.06 | 61.2% |
| Multi-Agent Reasoning | 3.22 | 64.4% |

## When to Use

- Long-running agent interactions where memory persistence across sessions is critical
- Agents that need to learn and improve from past interactions
- Research-oriented projects exploring stateful agent architectures
- Coding agents (Letta Code has proven benchmark performance)

## When NOT to Use

- High-throughput batch orchestration (not optimized for fan-out)
- .NET/C# environments (no SDK support)
- Real-time interactive UIs requiring sub-second responses
- Teams needing enterprise connectors and managed infrastructure

## Known Weaknesses

- **Single-agent focus**: Multi-agent orchestration exists but is secondary to memory management
- **No .NET SDK**: Python and TypeScript only
- **Scalability gaps**: No documented auto-scaling or elastic infrastructure
- **Throughput limitations**: Not optimized for high-concurrency batch workloads
- **Young platform**: Production infrastructure is newer than the research; battle-testing is limited
- **Context overhead**: Memory management adds latency to every agent interaction
- **Self-editing risk**: Agents modifying their own memory can lead to drift or corruption without guardrails

## Pricing Model

- **Open source**: Apache 2.0 (self-hosted, free)
- **Letta Cloud**: Managed hosting (pricing details not publicly available)
- **Model costs**: Pass-through to model provider (OpenAI, Anthropic, etc.)
