# agentkit-forge — JustAGhosT/agentkit-forge

## Overview

Centralized coordinator with a deterministic lifecycle pipeline and file-based delegation protocol.

**Pattern**: 5-phase state machine (Discovery → Planning → Implementation → Validation → Ship) with structured task delegation via JSON files and JSONL event logging.

## Agent Set

Agents are implemented as teams and handlers routed via spec-defined IDs. Team IDs are loaded from `spec/teams.yaml` with fallback to defaults. The system does not use LLM personas in code — routing is configuration-driven.

## Orchestration Architecture

```text
┌─────────────────────────────────────────────┐
│              Orchestrator Engine            │
│  ┌─────────┐  ┌──────────┐  ┌───────────┐  │
│  │Discovery│→ │ Planning │→ │Implementa-│  │
│  │         │  │          │  │   tion     │  │
│  └─────────┘  └──────────┘  └───────────┘  │
│                      ↓                      │
│  ┌───────────┐  ┌──────────┐               │
│  │Validation │→ │   Ship   │               │
│  └───────────┘  └──────────┘               │
│                                             │
│  Session Lock (TTL: 30m) ←── LOCK_STALE_MS  │
│  Task Files: .agentkit/state/tasks/*.json   │
│  Event Log: .agentkit/state/events.log      │
└─────────────────────────────────────────────┘
```

### Delegation Protocol ("A2A-lite")

- Tasks are JSON files under `.agentkit/state/tasks/` with explicit lifecycle states
- Handoff locks prevent double-processing
- State transitions are enumerated and validated
- Terminal states are explicitly defined

### Security

- Quality gate runner enforces allowlisted executables
- Reduces spec-based command injection risk

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 4.0 | 80.0% | High | Mostly local file I/O + JSON/YAML parse. Negligible network overhead. |
| Scalability | 3.0 | 60.0% | High | File-based coordination limits multi-host concurrency. Single-host only without shared filesystem. |
| Efficiency | 4.0 | 80.0% | High | Low orchestration overhead: file reads, JSON parse, YAML parse. No polling loops. |
| Fault Tolerance | 4.0 | 80.0% | High | Session lock with stale TTL (30m). Handoff locks reduce double-processing. Terminal task states prevent re-execution. |
| Throughput | 3.0 | 60.0% | Medium | Sequential step execution in check runner. No built-in fan-out primitives. |
| Maintainability | 4.0 | 80.0% | High | Deterministic pipeline with clear phase boundaries. Consistent patterns. |
| Determinism | 5.0 | 100.0% | High | Explicit state machine with enumerated transitions. JSONL event log provides full audit trail. Deterministic routing. |
| Integration Ease | 4.0 | 80.0% | Medium | Teams loaded from YAML specs. CLI-centric contract surface. 7 open PRs (moderate churn). |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.93 | 78.6% |
| Batch | 3.69 | 73.8% |
| Long-Running Durable | 3.94 | 78.8% |
| Event-Driven Serverless | 3.96 | 79.2% |
| Multi-Agent Reasoning | 4.20 | 84.0% |

## Technical Dependencies (Orchestration-Relevant)

| Dependency | Purpose |
|-----------|---------|
| Node.js runtime | Orchestration engine (ESM modules) |
| js-yaml | YAML spec parsing |
| Atomic file creation (`wx`) | Handoff lock implementation |
| JSONL format | Structured event logging |

## Performance Characteristics (Code-Evidenced)

- **Low orchestration overhead**: File I/O + JSON/YAML parse dominates; no network calls in coordination path
- **Deterministic coordination**: Explicit phases + enumerated valid state transitions
- **Fault containment**: Lock TTL prevents stuck sessions; task-level locks reduce double-processing

## SWOT Analysis

### Strengths
- Deterministic pipeline with explicit lifecycle and routing
- Observability via consistent JSONL events — every state transition is logged
- Security hardening: runner allowlists reduce command injection surface
- Simple mental model: 5 phases, clear entry/exit criteria

### Weaknesses
- File-based coordination limits multi-host concurrency (no shared durable store)
- Throughput bounded by sequential step execution
- No built-in parallelism primitives for fan-out tasks

### Opportunities
- Optional persistence backend (SQLite/Redis) while keeping file backend as default
- Add bounded parallelism primitives for fan-out tasks with backpressure
- Schema versioning for task/event contracts

### Threats
- Spec drift: YAML specs are the de facto "API"; changes can break orchestration without schema/version gating
- Workload growth: `tasks/` and `events/` directories become hotspots without compaction/rotation

## Config Defaults

| Setting | Value | Location |
|---------|-------|----------|
| Session lock stale TTL | 30 minutes | `LOCK_STALE_MS` in orchestrator.mjs |
| Retries | Not centrally defined | — |
| Max concurrency | Not centrally defined | — |
| Task states | Explicit lifecycle + terminal states | task-protocol.mjs |

## Evidence Index

| Aspect | Source Files |
|--------|-------------|
| Latency / Efficiency | Orchestrator runtime paths, file I/O patterns |
| Scalability / Concurrency | Session lock TTL (`LOCK_STALE_MS`), handoff lock (`withHandoffLock`) |
| Fault Tolerance | Stale lock handling, terminal task states |
| Throughput | Check step builder, sequential gating behavior |
| Determinism / Auditability | Phase state machine, events.log schema |
| Integration Ease | teams.yaml loading, defaults routing, CLI contract surface |

## Missing Information (Blocking Higher Confidence)

- Benchmarks: p95 end-to-end orchestrate time
- Check runner p95 across different stacks
- Explicit defaults for timeouts/retries on external calls (exec commands, networked ops)

## Future Expansion Points

1. **Version + validate task schema** — emit `schema_version` per task; add validator command
2. **Bounded concurrency controls** — add concurrency limits for fan-out tasks; emit queue depth + concurrency in events.log
3. **Failure injection mode** — simulate tool failures to verify propagation paths
4. **Optional durable backend** — SQLite or Redis for multi-host coordination
