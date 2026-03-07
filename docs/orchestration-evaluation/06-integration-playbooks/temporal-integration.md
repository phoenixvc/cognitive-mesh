# Temporal Integration Playbook

## Overview

Temporal provides durable workflow execution with deterministic replay. Integration means converting orchestration logic from each internal repo into Temporal workflows and activities.

**Best for**: Batch automation, long-running durable workflows, enterprise-grade reliability.

## Prerequisites

- Temporal Server (self-hosted) or Temporal Cloud account
- Temporal SDK for your language (.NET for cognitive-mesh, Python for codeflow-engine, TypeScript/Node for agentkit-forge and HouseOfVeritas)
- PostgreSQL or MySQL for Temporal persistence (self-hosted)

---

## agentkit-forge → Temporal

### Approach: Lifecycle Phases as Workflow Steps

Map the 5-phase lifecycle directly to Temporal workflow steps. Each phase becomes a Temporal activity.

```
Temporal Workflow: AgentForgeLifecycle
├── Activity: Discovery
├── Activity: Planning
├── Activity: Implementation
├── Activity: Validation
└── Activity: Ship
```

### Migration Steps

**Phase 1: Define Temporal Workflow**
```typescript
// Workflow definition (deterministic)
async function agentForgeLifecycle(input: TaskInput): Promise<TaskResult> {
  const discovery = await activities.runDiscovery(input);
  const plan = await activities.runPlanning(discovery);
  const impl = await activities.runImplementation(plan);
  const validation = await activities.runValidation(impl);
  return await activities.runShip(validation);
}
```

**Phase 2: Convert Task Protocol to Activities**
- Each phase handler becomes a Temporal activity
- Task JSON files become workflow inputs/outputs
- Handoff locks are replaced by Temporal's task queue routing
- Event log entries become Temporal workflow events (automatic)

**Phase 3: Dual-Run Period**
- Run both systems for new tasks
- File-based system handles existing tasks
- Compare results for validation

**Phase 4: Cutover**
- Disable file-based orchestrator
- Remove lock/state file management code

### What You Gain
- Crash recovery (deterministic replay replaces file-based state)
- Horizontal scaling (multiple workers)
- Built-in retry policies per activity
- Complete audit trail (Temporal event history)

### What You Lose
- Simplicity of file-based coordination
- Zero-infrastructure local execution
- Direct file inspection for debugging

### Effort: Medium (2–4 weeks, 1–2 engineers)
### Risk: LOW-MEDIUM ([details](../appendices/migration-risk-matrix.md))

---

## codeflow-engine → Temporal

### Approach: Workflow Engine as Temporal Workflow, Plugins as Activities

Map `WorkflowEngine.execute_workflow` to a Temporal workflow. Entry-point plugins (actions, integrations, providers) become Temporal activities.

```
Temporal Workflow: CodeFlowWorkflow
├── Activity: ValidateInput
├── Activity: RunAction (per plugin)
├── Activity: CallIntegration (GitHub, Linear, Slack)
└── Activity: ProcessLLMRequest (per provider)
```

### Migration Steps

**Phase 1: Define Core Workflow**
```python
@workflow.defn
class CodeFlowWorkflow:
    @workflow.run
    async def run(self, input: WorkflowInput) -> WorkflowResult:
        validated = await workflow.execute_activity(
            validate_input, input,
            start_to_close_timeout=timedelta(seconds=30),
            retry_policy=RetryPolicy(maximum_attempts=3)
        )
        # Replace self.config.workflow_timeout with Temporal timeout
        result = await workflow.execute_activity(
            run_action, validated,
            start_to_close_timeout=timedelta(seconds=300)
        )
        return result
```

**Phase 2: Convert Entry-Point Plugins**
- Each entry-point (action, integration, provider) becomes a Temporal activity
- Plugin registration via entry points → Temporal task queue routing
- `max_concurrent=10` → Temporal worker `max_concurrent_activities=10`

**Phase 3: Replace Retry Logic**
- Remove custom exponential backoff code
- Use Temporal `RetryPolicy(initial_interval, backoff_coefficient, maximum_attempts)`
- Remove `asyncio.wait_for` timeout → use Temporal `start_to_close_timeout`

**Phase 4: Migrate AutoGen Path**
- AutoGen group chat becomes a long-running Temporal activity
- `max_round=10` stays as AutoGen config within the activity

### What You Gain
- Durable state (replaces in-memory `running_workflows`)
- Built-in retry/timeout (replaces custom implementation)
- `max_concurrent` enforced at worker level
- Event fan-out becomes child workflows (with proper tracking)

### What You Lose
- Direct asyncio control
- In-process plugin loading speed

### Effort: Medium (3–5 weeks, 2 engineers)
### Risk: MEDIUM ([details](../appendices/migration-risk-matrix.md))

---

## cognitive-mesh → Temporal

### Approach: Coordination Patterns as Temporal Workflow Variants

Each coordination pattern becomes a distinct Temporal workflow. Governance gates become activity interceptors or workflow interceptors.

```
Temporal Workflows:
├── ParallelCoordination     → child workflows (Task.WhenAll equivalent)
├── HierarchicalCoordination → parent→child workflow delegation
├── CompetitiveCoordination  → parallel children → resolution activity
└── CollaborativeSwarm       → loop with continue-as-new for iteration
```

### Migration Steps

**Phase 1: Map Coordination Patterns**

```csharp
// Parallel → Temporal child workflows
[Workflow]
public class ParallelCoordinationWorkflow
{
    [WorkflowRun]
    public async Task<OrchestrationResult> RunAsync(OrchestrationInput input)
    {
        var childTasks = input.SubTasks.Select(task =>
            Workflow.ExecuteChildWorkflowAsync<AgentTaskWorkflow>(task));
        var results = await Task.WhenAll(childTasks);
        return Aggregate(results);
    }
}

// Swarm → continue-as-new pattern (clears history each iteration)
[Workflow]
public class SwarmCoordinationWorkflow
{
    [WorkflowRun]
    public async Task<OrchestrationResult> RunAsync(SwarmInput input)
    {
        var result = await Workflow.ExecuteActivityAsync(
            RunSwarmIteration, input);

        if (result.Converged || input.Iteration >= 5)
            return result;

        // Continue-as-new restarts the workflow with fresh history,
        // preventing unbounded event history growth.
        throw Workflow.CreateContinueAsNewException<SwarmCoordinationWorkflow>(
            result.NextInput with { Iteration = input.Iteration + 1 });
    }
}
```

**Phase 2: Integrate Governance Gates**
- Ethics checks → Temporal activity with timeout
- Approval flow → Temporal signal (wait for human approval)
- Authority scope → activity-level authorization interceptor

**Phase 3: Replace In-Memory State**
- `ConcurrentDictionary<_activeTasks>` → Temporal workflow state (automatic)
- `AuditTrailId` → Temporal workflow ID
- SignalR telemetry → Temporal visibility API + custom search attributes

**Phase 4: Adapter Migration**
- `IAgentRuntimeAdapter` → Temporal activity implementations
- `IApprovalAdapter` → Temporal signal handler

### What You Gain
- Durable state for all coordination patterns
- Crash recovery with deterministic replay
- Built-in audit trail (event history)
- Scalable multi-node execution

### What You Lose
- Real-time SignalR push (replaced by polling Temporal visibility API)
- In-process `ConcurrentDictionary` speed
- Direct adapter control

### Effort: Large (6–10 weeks, 2–3 engineers)
### Risk: HIGH ([details](../appendices/migration-risk-matrix.md))

---

## HouseOfVeritas → Temporal

### Approach: Inngest Functions as Temporal Workflows

Map Inngest `createFunction` calls to Temporal workflows. Event triggers become Temporal schedule triggers or signal handlers.

### Migration Steps

**Phase 1: Convert Functions to Workflows**
- Each Inngest function → Temporal workflow
- Each `step.run()` → Temporal activity
- `retries: 2` → `RetryPolicy(maximum_attempts=2)`
- Cron triggers → Temporal schedules

**Phase 2: Replace Event Dispatch**
- `routeToInngest` → Temporal `start_workflow` or `signal_workflow`
- Event schema (schema.ts) → Temporal payload types

**Phase 3: Feature Flags**
- `USE_INNGEST_APPROVALS` → Temporal signal-based approval workflows

### Effort: Medium (2–4 weeks, 1–2 engineers)
### Risk: LOW-MEDIUM ([details](../appendices/migration-risk-matrix.md))

---

## Hybrid Approach: Temporal + Inngest

For organizations with both batch/durable and event-driven/serverless needs:

| Workload | Engine | Rationale |
|----------|--------|-----------|
| Long-running approvals, sagas | Temporal | Durable replay required |
| Event-driven webhooks, quick functions | Inngest | Lower overhead, better DX |
| Batch processing, data pipelines | Temporal | Scalability + fault tolerance |
| Real-time notifications, triggers | Inngest | Low latency, simple integration |

**Bridge pattern**: Inngest functions can start Temporal workflows for complex, long-running operations. Temporal workflows can emit events to Inngest for lightweight follow-up actions.
