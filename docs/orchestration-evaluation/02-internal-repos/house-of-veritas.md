# HouseOfVeritas — JustAGhosT/HouseOfVeritas

## Overview

Event-driven workflow orchestration via Inngest with typed event contracts, cron-triggered workflows, and domain-specific function registry.

**Pattern**: Inngest-based workflow orchestration with typed event names/payloads, centralized function registry, and mixed event + cron triggers. Not a reusable library — it's an application-level orchestration implementation.

## Orchestration Architecture

```
┌────────────────────────────────────────────────────┐
│                 HouseOfVeritas                     │
│                                                    │
│  Inngest Client: id="house-of-veritas"            │
│                                                    │
│  ┌────────────────────────────────────┐            │
│  │        Function Registry           │            │
│  │  serve({ client, functions: [...] })│           │
│  │  - expense workflows               │           │
│  │  - task workflows                   │           │
│  │  - incident workflows               │           │
│  │  - approval workflows               │           │
│  │  - maintenance workflows             │          │
│  └────────────────────────────────────┘            │
│                                                    │
│  Triggers:                                         │
│  ├── Event: inngest.send({ name, data })          │
│  └── Cron: createFunction(..., { cron: "..." })   │
│                                                    │
│  Event Schema: lib/workflows/schema.ts             │
│  Dispatch: routeToInngest (logs errors, swallows) │
│                                                    │
│  Config Toggle: USE_INNGEST_APPROVALS              │
└────────────────────────────────────────────────────┘
```

### Event Contract Surface

- Typed event names and payload interfaces centralized in `lib/workflows/schema.ts`
- Clear contract surface for consumers and producers
- Events cover domain operations (expense, task, incident, etc.)

### Dispatch Pattern

- `routeToInngest` calls `inngest.send({ name, data })` and logs errors
- Errors are swallowed (logged but not propagated) — potential silent failure risk

## Per-Metric Scores

| Metric | Score | % | Confidence | Evidence & Justification |
|--------|:-----:|:-:|:----------:|--------------------------|
| Latency | 3.0 | 60.0% | Medium | Event-driven is inherently async. Queue latency (event ingestion → handler start) is Inngest-runtime-dependent and not measured. |
| Scalability | 4.0 | 80.0% | Medium | Inngest handles scaling; the app delegates orchestration complexity to the platform. |
| Efficiency | 3.0 | 60.0% | Low | Pay-per-invocation model (Inngest cloud) is efficient. Self-hosted efficiency depends on deployment. Not measured. |
| Fault Tolerance | 4.0 | 80.0% | High | Many functions specify `retries: 2`. Inngest provides built-in retry mechanics. But `routeToInngest` swallows errors. |
| Throughput | 4.0 | 80.0% | Medium | Workflows run out-of-band from user interactions. Inngest handles concurrency. Cron-based periodic scans for batch operations. |
| Maintainability | 3.0 | 60.0% | High | Typed events are clear. But very large function registry in a single route file is a maintainability risk. |
| Determinism | 4.0 | 80.0% | Medium | Typed event schema provides contract clarity. Inngest step functions provide replay semantics. |
| Integration Ease | 2.0 | 40.0% | High | Application-level implementation, not a reusable library. Integration would require forking or copying patterns. Feature flag toggles exist. 0 open PRs (stable). |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.21 | 64.2% |
| Batch | 3.53 | 70.6% |
| Long-Running Durable | 3.48 | 69.6% |
| Event-Driven Serverless | 3.06 | 61.2% |
| Multi-Agent Reasoning | 3.32 | 66.4% |

## Technical Dependencies (Orchestration-Relevant)

| Dependency | Purpose |
|-----------|---------|
| Inngest for Next.js | Workflow orchestration via `serve` from `"inngest/next"` |
| `inngest.createFunction` | Function definition with retry config and triggers |
| Cron expressions | Periodic workflow execution |
| TypeScript interfaces | Typed event payloads |

## Performance Characteristics (Code-Evidenced)

- **Retries**: Many workflows specify `retries: 2`
- **Async coordination**: Workflows run out-of-band from user interactions (event-driven)
- **Periodic scans**: Cron-based workflows (e.g., `"0 8 * * *"` for overdue escalation)
- **Maintainability risk**: Very large function registry in a single route file

## SWOT Analysis

### Strengths
- Clear event contract surface with typed payloads (`schema.ts`)
- Concrete retry behavior per function
- Mix of event + cron triggers supports both reactive and batch automations
- 0 open PRs — stable integration surface

### Weaknesses
- Not a reusable orchestration component — it's an application
- Central function registry in a single file can become a change management bottleneck
- `routeToInngest` swallows errors (silent failure risk)

### Opportunities
- Modularize workflow registry by domain (compose arrays per module; auto-discovery)
- Add benchmark instrumentation for p95 job start latency and step durations
- Add failure escalation channel when `routeToInngest` fails

### Threats
- Workflow sprawl: without conventions, duplication and inconsistent retries/timeouts
- Silent failure in `routeToInngest` could cause data loss if event delivery fails
- Single-vendor dependency on Inngest (mitigated by self-hosting option)

## Config Defaults

| Setting | Value | Location |
|---------|-------|----------|
| Timeouts | Inngest/runtime-defined (not shown) | — |
| Retries | Many functions set `retries: 2` | Individual workflow files |
| Max concurrency | Not shown (Inngest-managed) | — |
| Cron schedules | Various (e.g., `"0 8 * * *"`) | Workflow definitions |
| Feature toggle | `USE_INNGEST_APPROVALS` | Environment variable |

## Evidence Index

| Aspect | Source Files |
|--------|-------------|
| Integration contract surface | `lib/workflows/schema.ts` — event names + payloads |
| Dispatch + registry | Next.js Inngest route registry |
| Event emission | `routeToInngest` function |
| Retry/cron defaults | Workflow definitions (expense, task, failure-propagate) |
| Feature flags | Workflow env vars documentation |

## Missing Information

- Measured queue latency (event ingestion → handler start)
- Concurrency limits / rate limits at Inngest runtime level
- Error escalation strategy when `routeToInngest` fails repeatedly
- Step-level instrumentation (p50/p95 durations)

## Future Expansion Points

1. **Modularize registry** — split by domain; compose arrays per module; add auto-discovery
2. **Error escalation** — add retry + escalation when `routeToInngest` fails
3. **Workflow metrics** — emit structured metrics per function (p50/p95 start delay, step duration, retries)
4. **Consistency checks** — automated validation of naming conventions, retry configs, idempotency tags
