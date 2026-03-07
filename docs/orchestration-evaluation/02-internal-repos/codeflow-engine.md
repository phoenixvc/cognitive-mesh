# codeflow-engine — JustAGhosT/codeflow-engine

## Overview

Central engine coordinating workflows, actions, and integrations with optional multi-agent support via AutoGen.

**Pattern**: Central `CodeFlowEngine` coordinates workflow execution, action processing, integration management, and LLM provider coordination. Event-trigger fan-out executes matching workflows per event type.

## Orchestration Architecture

```
┌──────────────────────────────────────────────┐
│              CodeFlowEngine                  │
│  ┌──────────────┐  ┌───────────────────┐    │
│  │WorkflowEngine│  │ IntegrationManager│    │
│  │  - validate  │  │ - GitHub, Linear  │    │
│  │  - retry     │  │ - Slack, Axolo    │    │
│  │  - timeout   │  │                   │    │
│  └──────────────┘  └───────────────────┘    │
│  ┌──────────────┐  ┌───────────────────┐    │
│  │ActionProcessor│ │  LLM Providers    │    │
│  │ - entry pts  │  │ - OpenAI, Claude  │    │
│  │ - quality    │  │ - Mistral, Groq   │    │
│  └──────────────┘  └───────────────────┘    │
│                                              │
│  Event Fan-Out: process_event → scan → exec  │
│  Config: max_concurrent=10, timeout=300s     │
│  History: MAX_WORKFLOW_HISTORY=1000          │
└──────────────────────────────────────────────┘
```

### Multi-Agent Orchestration (AutoGen Path)

- `autogen_multi_agent.py` imports AutoGen (`ConversableAgent`, `GroupChat`, `GroupChatManager`) behind `try/except`
- Default group chat max rounds = 10
- AutoGen is **not declared** in base dependencies — optional but not expressed as an install extra

### Workflow Execution

- Input validation + sanitization
- Retries with exponential backoff (defaults: `attempts=3`, `delay=5`)
- Timeout via `asyncio.wait_for(..., timeout=self.config.workflow_timeout)`
- Event fan-out: `process_event` scans registered workflows and executes matching handlers

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.0 | 60.0% | Medium | Async execution model is reasonable but no evidence of latency optimization. Event fan-out adds scan overhead. |
| Scalability | 4.0 | 80.0% | Medium | Async tasks + running_workflows tracking. Config supports `max_concurrent=10`. Actual enforcement location needs verification. |
| Efficiency | 3.0 | 60.0% | Medium | Asyncio is efficient but metrics lock and history retention (1000 entries) add overhead. |
| Fault Tolerance | 4.0 | 80.0% | High | Retries with exponential backoff (`attempts=3`, `delay=5`). Hard timeout. Error recording/history. |
| Throughput | 3.0 | 60.0% | Medium | Async task model exists but backpressure semantics unclear. `max_concurrent` enforcement not verified. |
| Maintainability | 3.0 | 60.0% | Medium | Central orchestration surface is clear. Entry-point registries add structure. Multiple orchestration paradigms increase complexity. |
| Determinism | 3.0 | 60.0% | Medium | Metrics fields and history retention exist. No explicit replay capability or deterministic state machine. |
| Integration Ease | 3.0 | 60.0% | High | Entry-point registries for actions/integrations/providers. But AutoGen dependency hygiene gap reduces score. 3 open PRs (stable). |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.22 | 64.4% |
| Batch | 3.44 | 68.8% |
| Long-Running Durable | 3.36 | 67.2% |
| Event-Driven Serverless | 3.10 | 62.0% |
| Multi-Agent Reasoning | 3.12 | 62.4% |

## Technical Dependencies (Orchestration-Relevant)

| Dependency | Purpose |
|-----------|---------|
| Python asyncio | Async workflow execution |
| Pydantic | Config validation (workflow defaults) |
| Entry points | Plugin registration (actions, integrations, providers) |
| AutoGen (optional) | Multi-agent group chat |

## Performance Characteristics (Code-Evidenced)

- **Concurrency**: Async tasks + `running_workflows` tracking; `max_concurrent=10` configured
- **Fault tolerance**: Retries + exponential backoff + hard timeout + error recording/history
- **Observability**: Metrics fields and history retention (`MAX_WORKFLOW_HISTORY=1000`)

## SWOT Analysis

### Strengths
- Central orchestration surface with structured retry/timeout semantics
- Clear extensibility via entry-point registries
- Config defaults exist and are discoverable in code
- Multiple LLM provider support

### Weaknesses
- Dependency hygiene gap: AutoGen mode is referenced but not declared as a dependency extra
- Concurrency and backpressure semantics not clearly enforceable from engine code
- Multiple orchestration paradigms (workflows, multi-agent, external schedulers) without a single "golden path"

### Opportunities
- Formalize orchestration "modes": baseline vs extras (`[autogen]`, `[temporal]`, etc.) with install groups and CI matrix
- Add benchmark harness (p95 latency, fan-out throughput, failure injection)
- Implement explicit concurrency control (semaphore/queue) for `max_concurrent`

### Threats
- Complexity creep from multiple orchestration paradigms
- Integration friction if optional dependencies aren't consistently packaged/tested
- Event fan-out could become a bottleneck without backpressure controls

## Config Defaults

| Setting | Value | Location |
|---------|-------|----------|
| Workflow timeout | 300 seconds | `config.workflow_timeout` in settings.py |
| Retry attempts | 3 | Default in engine.py |
| Retry delay | 5 seconds (exponential backoff) | Default in engine.py |
| Max concurrent workflows | 10 | `workflow.max_concurrent` in settings.py |
| History cap | 1000 entries | `MAX_WORKFLOW_HISTORY` |
| AutoGen max rounds | 10 | `autogen_multi_agent.py` |

## Evidence Index

| Aspect | Source Files |
|--------|-------------|
| Latency / Throughput / Fault Tolerance | Workflow engine retries + timeout + process_event fanout |
| Scalability / Concurrency | `running_workflows`, async task model; workflow config defaults |
| Integration Ease | Entry-point registries (actions/integrations/providers) |
| Multi-Agent | AutoGen group chat wrapper + `max_round=10` |

## Missing Information

- Where `workflow.max_concurrent` is enforced (queueing/backpressure mechanism)
- Circuit breaker policy usage (dependencies exist as extras; usage not verified)
- Load/perf tests for event fan-out and long-running workflows
- Concrete idempotency strategy for workflow re-execution

## Future Expansion Points

1. **Dependency extras** — add `[autogen]`, `[temporal]` install groups + CI validation per group
2. **Concurrency control** — implement and document semaphore/queue aligned to `max_concurrent`
3. **Trace IDs** — emit correlation IDs through workflow execution and event fan-out
4. **Benchmark harness** — p95 latency, fan-out throughput, failure injection scenarios
