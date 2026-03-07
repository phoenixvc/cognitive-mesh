# Operational Runbooks

Template runbooks for common orchestration operations. Adapt per system.

## Runbook 1: Debug a Stuck Orchestration

### Symptoms
- Task/workflow has not progressed for longer than expected
- No new events/logs appearing for a running task
- Downstream systems report no activity

### Diagnostic Steps

```
Step 1: Identify the execution
─────────────────────────────
- Find the execution ID / workflow ID / task ID
- Locate it in the orchestration state store

  agentkit-forge:  ls .agentkit/state/tasks/ | grep <task-id>
  codeflow-engine: query running_workflows dict or metrics history
  cognitive-mesh:  inspect _activeTasks ConcurrentDictionary
  HouseOfVeritas:  check Inngest dashboard for function run

Step 2: Inspect the event log / history
───────────────────────────────────────
- Look for the last recorded event/state transition
- Check for error entries

  agentkit-forge:  grep <task-id> .agentkit/state/events.log
  codeflow-engine: check workflow error_history
  cognitive-mesh:  check SignalR hub logs + adapter logs
  HouseOfVeritas:  Inngest dashboard → function run → step history

Step 3: Check for locks
───────────────────────
- Verify no stale locks are blocking progress

  agentkit-forge:  check handoff lock age vs LOCK_STALE_MS (30m)
  codeflow-engine: check running_workflows for zombie entries
  cognitive-mesh:  check _activeTasks for entries without recent updates
  HouseOfVeritas:  Inngest manages locks internally

Step 4: Check external dependencies
────────────────────────────────────
- Verify downstream services are reachable
- Check for timeout or rate-limit responses
- Verify credentials/tokens haven't expired

Step 5: Resolution
──────────────────
- If stale lock: clear the lock (agentkit-forge: delete lock file)
- If stuck state: manually transition to a terminal state or retry
- If external dependency: fix the dependency, then retry
- Document root cause for future prevention
```

### Post-Resolution

- Verify the task completes or reaches a terminal state
- Check no other tasks were affected
- Update monitoring/alerts if the failure mode was undetected

## Runbook 2: Replay an Orchestration

### When to Replay
- Original execution failed mid-way and needs to resume
- Need to verify idempotency of a workflow
- Debugging: reproduce a specific execution path

### Steps

```
Step 1: Reconstruct inputs
──────────────────────────
- Retrieve the original task/workflow input
- Verify no external state has changed that would alter results

  agentkit-forge:  read task JSON from .agentkit/state/tasks/<id>.json
  codeflow-engine: reconstruct from workflow history entries
  cognitive-mesh:  retrieve from _activeTasks or audit trail
  HouseOfVeritas:  retrieve event payload from Inngest dashboard

Step 2: Choose replay mode
──────────────────────────
- Full replay: re-execute from scratch with same inputs
- Resume: skip completed steps, continue from failure point
- Dry run: execute without side effects (if supported)

  Note: Most internal repos do NOT support skip-completed-steps
  resume. Full replay is the typical option.

Step 3: Execute replay
──────────────────────
- For full replay: submit the original input with a new execution ID
- For resume: submit with same execution ID (requires idempotency)

  Warning: If the system lacks idempotency, full replay may cause
  duplicate side effects (double emails, double API calls, etc.)

Step 4: Compare outputs
───────────────────────
- Compare original execution result with replay result
- Flag any differences for investigation
- Check side effects were not duplicated
```

### Replay Support by System

| System | Full Replay | Resume | Dry Run | Idempotent? |
|--------|:-----------:|:------:|:-------:|:-----------:|
| agentkit-forge | Yes (new task ID) | No | No | Partial (lock-based) |
| codeflow-engine | Yes (new workflow) | No | No | Not verified |
| cognitive-mesh | Yes (new task) | No | No | Not verified |
| HouseOfVeritas | Yes (re-send event) | Yes (Inngest step functions) | No | Per-event ID |
| Temporal | Yes | Yes (native) | No | Yes (workflow ID) |
| Inngest | Yes | Yes (step functions) | No | Yes (event ID) |

## Runbook 3: Interpret Trace / Event IDs

### Correlation ID Patterns

```
agentkit-forge
──────────────
- Session ID: unique per orchestration session
- Task ID: unique per delegated task
- Event entries in events.log reference both
- Correlation: session → task → events (1:N:M)

codeflow-engine
───────────────
- Workflow ID: unique per workflow execution
- No explicit correlation ID across event fan-out
- History entries reference workflow ID
- Correlation: workflow → history entries (1:N)

cognitive-mesh
──────────────
- AuditTrailId: unique per orchestration request
- Agent IDs: per-agent within orchestration
- Correlation: AuditTrailId → agent results (1:N)
- Note: persistence is adapter-dependent

HouseOfVeritas
──────────────
- Inngest function run ID: unique per function execution
- Event ID: unique per dispatched event
- Step IDs: unique per step within a function run
- Correlation: event → function run → steps (1:1:N)
```

### Where IDs Are Stored

| System | ID Type | Storage | Retention |
|--------|---------|---------|-----------|
| agentkit-forge | Session/Task | `.agentkit/state/` files | Until deleted |
| codeflow-engine | Workflow ID | In-memory history (cap: 1000) | Until eviction |
| cognitive-mesh | AuditTrailId | Adapter-dependent | Unknown |
| HouseOfVeritas | Function run ID | Inngest platform | Platform-dependent |

### Privacy Controls

- Ensure trace IDs do not embed PII
- Log retention policies must comply with data governance
- Audit trail access should be role-gated
- Consider encryption at rest for sensitive execution payloads
