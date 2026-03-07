# Microsoft AutoGen

## What It Is

AutoGen is Microsoft's multi-agent conversation framework. Agents interact through chat-based message passing in group conversations managed by a GroupChatManager. Originally positioned as the primary multi-agent framework from Microsoft, recent guidance directs new users toward the Microsoft Agent Framework, while AutoGen continues to receive maintenance.

## Architecture & Orchestration Pattern

**Pattern**: Chat-based multi-agent conversation with group management

```
┌────────────────────────────────────┐
│        GroupChat                   │
│  ┌──────────┐  ┌──────────┐      │
│  │  Agent 1 │  │  Agent 2 │      │
│  │(Converse)│  │(Converse)│      │
│  └──────────┘  └──────────┘      │
│  ┌──────────────────────────┐    │
│  │  GroupChatManager        │    │
│  │  max_round=10            │    │
│  │  speaker selection       │    │
│  └──────────────────────────┘    │
└────────────────────────────────────┘
```

- **ConversableAgent**: Base agent with chat + tool execution
- **GroupChat**: Multi-agent conversation with speaker selection
- **GroupChatManager**: Manages turn order and termination
- **Code execution**: Built-in sandboxed code execution

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.0 | 60.0% | Medium | Chat-based coordination adds message round-trips. LLM calls per turn. |
| Scalability | 2.8 | 56.0% | Medium | In-memory conversation. No distributed coordination. Group size limited by context. |
| Efficiency | 2.8 | 56.0% | Medium | Token-heavy due to full conversation history per agent call. Multiple LLM calls per round. |
| Fault Tolerance | 2.5 | 50.0% | Medium | No built-in retry. No checkpointing. Conversation state is in-memory. `max_round` prevents infinite loops. |
| Throughput | 2.5 | 50.0% | Medium | Sequential conversation turns. Async messaging support added. But fundamentally turn-based. |
| Maintainability | 3.2 | 64.0% | Medium | Clean agent API. But ~190 open PRs + transition to MS Agent Framework creates uncertainty. |
| Determinism | 3.2 | 64.0% | Medium | Conversation history is complete. Speaker selection is deterministic. But LLM outputs vary. |
| Integration Ease | 3.0 | 60.0% | Medium | Python-native. Tool integration. But transition to MS Agent Framework adds uncertainty. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.12 | 62.4% |
| Batch | 2.90 | 58.0% |
| Long-Running Durable | 2.62 | 52.4% |
| Event-Driven Serverless | 2.80 | 56.0% |
| Multi-Agent Reasoning | 3.40 | 68.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Chat-based multi-agent conversations | **2nd** | Purpose-built for this; but LangGraph offers more control. |
| Research/prototyping multi-agent systems | **2nd** | Good for exploration. CrewAI simpler for quick prototypes. |
| Code generation with validation | 3rd | Built-in code execution is unique. |

## When NOT to Use

- New projects (Microsoft recommends MS Agent Framework for new work)
- Production systems requiring durability or fault tolerance
- Non-chat agent coordination patterns (use LangGraph)
- Long-term investments given transition uncertainty

## Maturity Signals

- **GitHub stars**: 35k+ (high visibility)
- **Open PRs**: ~190 (high churn)
- **Corporate backing**: Microsoft Research
- **Direction**: Maintenance mode; new features directed to MS Agent Framework
- **Risk**: Long-term direction uncertain
