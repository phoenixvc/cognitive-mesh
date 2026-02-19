# Team AGENCY — AgencyLayer Agent

You are **Team AGENCY** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/agency-layer.md` for layer-specific rules
3. Read `TODO.md` for immediate action items owned by this team
4. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Source:** `src/AgencyLayer/` and `src/Shared/` (for NodeLabels.cs fix)
- **Tests:** `tests/AgencyLayer/`
- **Dependencies:** May reference all lower layers

## Backlog

### P0 — Build Blockers (from TODO.md)
1. `src/Shared/NodeLabels.cs` — Add missing XML doc comments (CS1591 build errors)
2. Verify DecisionExecutorTests and ConclAIveReasoningAdapterTests pass

### P1 — Stub Completions
1. `src/AgencyLayer/DecisionExecution/DecisionExecutor.cs`
   - Line 36: `// TODO: Implement actual decision execution logic` — uses Task.Delay
   - Line 82: `// TODO: Implement actual status retrieval logic`
   - Line 112: `// TODO: Implement actual log retrieval logic`

2. `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs`
   - Lines 160, 169: Placeholder methods returning Task.CompletedTask

3. `src/AgencyLayer/MultiAgentOrchestration/Adapters/InMemoryAgentKnowledgeRepository.cs`
   - Lines 31, 52: Placeholder implementations

4. `src/AgencyLayer/Orchestration/Checkpointing/InMemoryCheckpointManager.cs`
   - Line 87: PurgeWorkflowCheckpointsAsync placeholder

5. `src/AgencyLayer/Orchestration/Execution/DurableWorkflowEngine.cs`
   - Line 118: Placeholder

### P1 — Critical Test Gap
- **No tests exist** for MultiAgentOrchestrationEngine — create `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs`
- Cover: RegisterAgent, ExecuteTask, SetAgentAutonomy, SpawnAgent, all coordination patterns

### P2 — TODO.md Integration Tasks
- Integration with ActionPlanner workflow
- Reasoning recipe selection refinement for DecisionType
- Persistence alignment with Knowledge Graph schema

## Workflow
1. Fix build: NodeLabels.cs XML docs first (unblocks everything)
2. Verify existing tests pass
3. Complete stubs in priority order (DecisionExecutor > MultiAgentOrchestration > Checkpointing)
4. Add MultiAgentOrchestrationEngine tests
5. Verify: `dotnet test tests/AgencyLayer/ --no-build` all green
6. Report: Summary of completions

$ARGUMENTS
