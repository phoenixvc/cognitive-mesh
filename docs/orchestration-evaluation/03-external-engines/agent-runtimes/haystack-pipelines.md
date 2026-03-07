# Haystack Pipelines

## What It Is

Haystack by deepset is an open-source framework for building NLP/LLM applications with a pipeline-based architecture. Pipelines connect components (retrievers, generators, converters) in directed graphs. Version 2.x introduced a modernized component API with clear input/output contracts.

## Architecture & Orchestration Pattern

**Pattern**: Component pipeline with directed graph execution

```
Pipeline:
  Component A → Component B → Component C
                     ↘ Component D (branching)
```

- **Components**: Self-contained units with typed inputs/outputs
- **Pipeline**: Connects components into a directed graph
- **Routers**: Conditional branching based on component outputs
- **Document stores**: Pluggable storage backends for RAG

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.2 | 64.0% | Medium | In-process pipeline execution. Component overhead is minimal. LLM/retrieval latency dominates. |
| Scalability | 3.0 | 60.0% | Medium | Single-process. Ray integration for distributed execution. Pipeline-level scaling. |
| Efficiency | 3.2 | 64.0% | Medium | Component model is efficient. Typed I/O reduces unnecessary serialization. |
| Fault Tolerance | 2.5 | 50.0% | Medium | No built-in retry. No checkpointing. Pipeline fails if any component fails. |
| Throughput | 3.0 | 60.0% | Medium | Pipeline-level parallelism limited. Component execution is sequential within a branch. |
| Maintainability | 3.8 | 76.0% | High | Clean component API (v2.x). Strong typing. Good test patterns. Well-documented. |
| Determinism | 3.2 | 64.0% | Medium | Pipeline structure is explicit and serializable. Component I/O is typed. But no replay. |
| Integration Ease | 3.5 | 70.0% | Medium | Rich component library. Multiple LLM providers. Document store plugins. Python-only. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.32 | 66.4% |
| Batch | 3.20 | 64.0% |
| Long-Running Durable | 2.80 | 56.0% |
| Event-Driven Serverless | 3.00 | 60.0% |
| Multi-Agent Reasoning | 3.00 | 60.0% |

## When to Use (Ranked by Use Case)

| Use Case | Rank | Fit |
|----------|:----:|-----|
| NLP/RAG pipeline development | **1st** | Purpose-built for document processing and retrieval. |
| Component-based AI composition | **2nd** | Clean component model with strong typing. LangGraph more flexible for agents. |
| Production NLP applications | **2nd** | v2.x API is production-ready. Good enterprise support from deepset. |

## When NOT to Use

- Agent-heavy workflows (limited agent abstractions)
- Durable long-running workflows
- Non-Python environments
- High-throughput batch processing
