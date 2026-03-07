# Inngest Integration Playbook

## Overview

Inngest provides event-driven step functions with durable execution. Integration means converting orchestration logic into Inngest functions with typed event contracts.

**Best for**: Event-driven serverless, interactive developer tools, webhook handlers, lightweight durable workflows.

## Prerequisites

- Inngest Cloud account or self-hosted Inngest instance
- Inngest SDK for your language (TypeScript primary; Python available)
- HTTP framework (Next.js, Express, FastAPI, etc.)

---

## agentkit-forge → Inngest

### Approach: Lifecycle Phases as Inngest Steps

Map the 5-phase lifecycle to Inngest step functions. Each phase is a durable step.

```typescript
const agentLifecycle = inngest.createFunction(
  { id: "agent-lifecycle", retries: 3 },
  { event: "agent/task.created" },
  async ({ event, step }) => {
    const discovery = await step.run("discovery", async () => {
      return runDiscovery(event.data);
    });

    const plan = await step.run("planning", async () => {
      return runPlanning(discovery);
    });

    const impl = await step.run("implementation", async () => {
      return runImplementation(plan);
    });

    const validated = await step.run("validation", async () => {
      return runValidation(impl);
    });

    return await step.run("ship", async () => {
      return runShip(validated);
    });
  }
);
```

### Migration Steps

**Phase 1: Define Event Schema**
- Map task JSON schema to typed Inngest events
- `task.created`, `task.completed`, `task.failed` events

**Phase 2: Convert Phase Handlers to Steps**
- Each lifecycle phase → `step.run()`
- Handoff locks → Inngest's built-in deduplication (event ID)
- JSONL event log → Inngest function run history (automatic)

**Phase 3: Replace File-Based State**
- Task files → event payloads (Inngest persists step state)
- Session lock → Inngest function concurrency keys
- Events log → Inngest dashboard

**Phase 4: Dual-Run & Cutover**
- Dispatch events to both systems during transition
- Compare outcomes before cutting over

### What You Gain
- Step-level durability (resume from failed step)
- Zero-infrastructure event handling
- Built-in retries per function
- Dashboard for monitoring

### What You Lose
- File-based debugging (direct inspection)
- Local-only execution (Inngest needs a server or cloud)
- JSONL event log format (replaced by Inngest's proprietary format)

### Effort: Small (1–2 weeks, 1 engineer)
### Risk: LOW

---

## codeflow-engine → Inngest

### Approach: Workflow Engine → Inngest Functions, Event Fan-Out → Event Triggers

```typescript
const codeflowWorkflow = inngest.createFunction(
  {
    id: "codeflow-workflow",
    retries: 3,
    concurrency: { limit: 10 }  // replaces max_concurrent=10
  },
  { event: "codeflow/workflow.triggered" },
  async ({ event, step }) => {
    const validated = await step.run("validate", async () => {
      return validateInput(event.data);
    });

    const result = await step.run("execute", async () => {
      return executeAction(validated);
    });

    // Fan-out: emit events for downstream workflows
    await step.run("notify", async () => {
      await inngest.send({
        name: "codeflow/action.completed",
        data: result
      });
    });

    return result;
  }
);
```

### Migration Steps

**Phase 1: Map Workflows to Functions**
- `WorkflowEngine.execute_workflow` → Inngest function
- Retry config: `attempts=3, delay=5` → `retries: 3` (Inngest handles backoff)
- Timeout: `workflow_timeout=300` → Inngest step timeout

**Phase 2: Convert Event Fan-Out**
- `process_event` scan → Inngest event triggers (each handler registers its trigger)
- No scanning needed — Inngest routes events to matching functions

**Phase 3: Replace In-Memory State**
- `running_workflows` dict → Inngest manages function state
- `MAX_WORKFLOW_HISTORY=1000` → Inngest retains full history (cloud)

**Phase 4: Entry-Point Plugins**
- Each entry-point plugin → separate Inngest function with its own trigger
- Plugin discovery → Inngest function registry via `serve()`

### Note on AutoGen Path
AutoGen group chat can run as a long Inngest step:
```typescript
await step.run("autogen-chat", async () => {
  return runAutoGenGroupChat(input, { maxRound: 10 });
});
```
This step will be retried if it fails but is not checkpointed within the chat.

### Effort: Medium (2–3 weeks, 1–2 engineers)
### Risk: MEDIUM

---

## cognitive-mesh → Inngest

### Approach: Coordination Patterns as Event-Driven Workflows

This is the most complex integration due to cognitive-mesh's rich orchestration patterns.

### Migration Steps

**Phase 1: Parallel Coordination**
```typescript
const parallelCoordination = inngest.createFunction(
  { id: "parallel-coordination" },
  { event: "mesh/orchestrate.parallel" },
  async ({ event, step }) => {
    // Fan-out: send events for each sub-task
    await step.run("dispatch", async () => {
      await Promise.all(
        event.data.subTasks.map(task =>
          inngest.send({ name: "mesh/agent.execute", data: task })
        )
      );
    });

    // Fan-in: wait for all agent completions (one waitForEvent per sub-task)
    const results = await Promise.all(
      event.data.subTasks.map((task, index) =>
        step.waitForEvent(`agent-completed-${index}`, {
          event: "mesh/agent.completed",
          timeout: "30m",
          match: "data.orchestrationId",
          if: `async.data.taskId == '${task.taskId}'`
        })
      )
    );

    return aggregate(results);
  }
);
```

**Phase 2: Swarm Coordination**
```typescript
const swarmCoordination = inngest.createFunction(
  { id: "swarm-coordination" },
  { event: "mesh/orchestrate.swarm" },
  async ({ event, step }) => {
    let state = event.data.initialState;

    for (let i = 0; i < 5; i++) {
      state = await step.run(`iteration-${i}`, async () => {
        return runSwarmIteration(state);
      });

      if (state.converged) break;
    }

    return state;
  }
);
```

**Phase 3: Governance Gates**
- Ethics checks → `step.run("ethics-check", ...)` that throws on violation
- Approval flow → `step.waitForEvent("mesh/approval.received", { timeout: "24h" })`
- Authority scope → validated in the first step before execution

**Phase 4: Real-Time Telemetry**
- SignalR hub → Inngest sends events that a SignalR adapter can relay
- Or: poll Inngest API for function run status

### Limitations
- CollaborativeSwarm and Competitive patterns don't map perfectly to step functions
- Governance gate latency may increase (Inngest round-trips vs in-process)
- SignalR real-time push becomes polling-based

### Effort: Large (6–10 weeks, 2–3 engineers)
### Risk: HIGH

---

## HouseOfVeritas → Inngest (Already Integrated)

HouseOfVeritas is **already on Inngest**. No migration needed.

### Optimization Opportunities

1. **Modularize registry**: Split the single route file into domain modules
2. **Standardize retries**: Align all functions to consistent retry policy
3. **Add concurrency keys**: Prevent duplicate processing per entity
4. **Error escalation**: Replace silent error swallowing in `routeToInngest` with retry + alert
5. **Shared event schema**: Align with other repos if they migrate to Inngest

### Effort: Small (1 week, 1 engineer)
### Risk: LOW
