# LlamaIndex Workflows

## What It Is

LlamaIndex Workflows is an event-driven orchestration framework within the LlamaIndex ecosystem. It provides a step-based workflow model where steps react to events, enabling complex AI workflows with RAG, agent loops, and data processing. Designed to work seamlessly with LlamaIndex's data ingestion and retrieval primitives.

## Architecture & Orchestration Pattern

**Pattern**: Event-driven step orchestration with LlamaIndex ecosystem integration

- **Workflows**: Define a sequence of steps triggered by events
- **Steps**: Async functions decorated with `@step` that consume and emit events
- **Events**: Typed messages that flow between steps
- **Context**: Shared state across steps within a workflow run

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.5 | 70.0% | Medium | In-process execution. Event-driven model is efficient. LLM/retrieval latency dominates. |
| Scalability | 3.0 | 60.0% | Low | Single-process. No distributed coordination built-in. Scales with host infrastructure. |
| Efficiency | 3.2 | 64.0% | Medium | Event-driven avoids polling. But LlamaIndex index operations can be I/O heavy. |
| Fault Tolerance | 2.8 | 56.0% | Low | Step-level error handling. No built-in retry. No checkpointing. In-memory state. |
| Throughput | 3.0 | 60.0% | Medium | Async step execution. Parallel steps possible via event branching. Python-bound. |
| Maintainability | 3.5 | 70.0% | Medium | Step/event model is clear. LlamaIndex ecosystem integration is smooth. Documentation improving. |
| Determinism | 3.0 | 60.0% | Low | Event flow is traceable. But no built-in replay or audit trail. |
| Integration Ease | 3.8 | 76.0% | Medium | Native LlamaIndex integration (indices, retrievers, agents). Python-only. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.40 | 68.0% |
| Batch | 3.10 | 62.0% |
| Long-Running Durable | 2.92 | 58.4% |
| Event-Driven Serverless | 3.20 | 64.0% |
| Multi-Agent Reasoning | 3.20 | 64.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| RAG-centric AI workflows | **1st** | Best integration with LlamaIndex data/retrieval stack. |
| LlamaIndex ecosystem projects | **1st** | Native workflow support for existing LlamaIndex users. |
| Multi-step AI pipelines | 3rd | Works but LangGraph has richer control flow. |

## When NOT to Use

- Non-LlamaIndex projects (framework-specific)
- Production systems needing durability or fault tolerance
- Non-Python environments
- Simple agent handoffs (OpenAI Agents SDK simpler)
