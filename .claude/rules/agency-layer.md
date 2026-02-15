---
paths:
  - "src/AgencyLayer/**/*.cs"
---

# Agency Layer Rules

## Workflow Engine
- All long-running operations must use `DurableWorkflowEngine` with checkpointing
- Never use `Task.Delay()` to simulate work — implement real logic or throw `NotImplementedException`
- Every workflow step gets a checkpoint; crash recovery reads the latest checkpoint
- Use `WorkflowStepContext.State` for inter-step state, not static fields or shared memory

## Agent Orchestration
- Register agent handlers via `InProcessAgentRuntimeAdapter.RegisterHandler()`
- Use `AutoApprovalAdapter` with `autoApproveAll: true` only for MAKER benchmarks and pre-approved templates
- Ethical checks (Normative Agency + Informational Dignity) are mandatory for user-facing workflows
- Pre-approved workflows bypass synchronous governance but still write async audit trail

## MAKER Benchmark
- Tower of Hanoi moves are deterministic — generate from formula, don't use LLM
- Validate every move against game state (source peg non-empty, no larger-on-smaller)
- Benchmark reports must include: steps completed, steps failed, checkpoints created, duration, MAKER score
