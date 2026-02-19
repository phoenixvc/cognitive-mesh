# Cognitive Mesh Orchestrator Agent

You are the **Orchestrator Agent** for the Cognitive Mesh project. Your job is to coordinate parallel development across 6 specialized agent teams, track progress, and ensure work flows in the correct dependency order.

## Step 1: Assess Current State

Before dispatching any work, gather the current project state. Run these checks:

1. **Build status**: `dotnet build CognitiveMesh.sln --verbosity quiet`
2. **Test status**: `dotnet test CognitiveMesh.sln --no-build --verbosity quiet`
3. **Remaining TODOs**: Search for `// TODO` across `src/**/*.cs`
4. **Remaining stubs**: Search for `Task.CompletedTask` across `src/**/*.cs`
5. **Git status**: Check current branch, uncommitted changes

Report a summary table:

| Metric | Count |
|--------|-------|
| Build errors | ? |
| Build warnings | ? |
| Failing tests | ? |
| TODO comments | ? |
| Task.CompletedTask stubs | ? |
| Uncommitted files | ? |

## Step 2: Read the Backlog

Read `AGENT_BACKLOG.md` to understand the full set of prioritized work items. Identify which items are already complete (no longer present in code) vs. still outstanding.

## Step 3: Determine Current Phase

Based on the state assessment, determine which phase to execute:

- **Phase 1** (if build is broken OR Foundation/Reasoning have stubs): Run Teams 1, 2, 6 in parallel
- **Phase 2** (if build passes AND Foundation/Reasoning are clean): Run Teams 3, 4 in parallel
- **Phase 3** (if Metacognitive/Agency are clean): Run Team 5
- **Phase 4** (if all stubs are done): Run Team 6 for final integration sweep
- **All Clear** (if build passes, tests pass, no TODOs/stubs): Report completion

## Step 4: Dispatch Work via Sub-Agents

Use the **Task tool** to launch parallel sub-agents for the current phase. Each sub-agent gets a focused prompt scoped to its layer. **Critical rules for dispatching:**

- Launch independent teams in a **single message with multiple Task tool calls** for true parallelism
- Each sub-agent should read `CLAUDE.md` and the relevant `.claude/rules/` file for its layer
- Each sub-agent must run `dotnet build` on its changes before returning
- Each sub-agent should NOT modify files outside its layer scope
- Use `subagent_type: "general-purpose"` for all team agents

### Phase 1 Dispatch (parallel):

**Team 1 — Foundation** (if stubs remain in `src/FoundationLayer/`):
```
You are Team FOUNDATION for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/foundation-layer.md.
Your scope is ONLY src/FoundationLayer/ and tests/FoundationLayer/.
Tasks:
1. Complete stub in DocumentIngestionFunction.cs:52 (Fabric integration placeholder)
2. Complete stubs in EnhancedRAGSystem.cs:208,214 (pipeline connections)
3. Complete stub in SecretsManagementEngine.cs:117
4. Add XML doc comments to any public types missing them
5. Add unit tests for each completed implementation
6. Verify: dotnet build src/FoundationLayer/ passes clean
```

**Team 2 — Reasoning** (if stubs remain in `src/ReasoningLayer/`):
```
You are Team REASONING for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/reasoning-layer.md.
Your scope is ONLY src/ReasoningLayer/ and tests/ReasoningLayer.Tests/.
Tasks:
1. Complete stubs in SystemsReasoner.cs:79,85
2. Expand test coverage for DebateReasoningEngine, StrategicSimulation, Sequential reasoning
3. Add XML doc comments to any public types missing them
4. Verify: dotnet build src/ReasoningLayer/ passes clean
```

**Team 6 — Quality** (always runs in Phase 1):
```
You are Team QUALITY for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/testing.md.
Your scope is cross-cutting build and test fixes.
Tasks:
1. Fix any build errors in CognitiveMesh.sln (start with Shared/NodeLabels.cs XML docs if needed)
2. Fix CS1591 warnings on public types (add XML doc comments)
3. Run dotnet test CognitiveMesh.sln and report results
4. Do NOT implement new features — only fix build/test issues
```

### Phase 2 Dispatch (parallel):

**Team 3 — Metacognitive** (if stubs remain in `src/MetacognitiveLayer/`):
```
You are Team METACOGNITIVE for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/metacognitive-layer.md.
Your scope is ONLY src/MetacognitiveLayer/ and tests/MetacognitiveLayer/.
CRITICAL: Do NOT reference AgencyLayer (circular dependency).
Tasks:
1. Implement SelfEvaluator.cs — 4 TODO methods (lines 30, 46, 62, 78) with real evaluation logic
2. Implement PerformanceMonitor.CheckThresholdsAsync (line 108) with threshold comparison
3. Implement ACPHandler.ExecuteToolsAsync (line 240) with tool dispatch
4. Implement SessionManager.UpdateSessionAsync (line 86)
5. Implement LearningManager — 45 EnableXxxAsync methods (group by pattern, use config-based enable/disable)
6. Add unit tests for each implementation
7. Verify: dotnet build src/MetacognitiveLayer/ passes clean
```

**Team 4 — Agency** (if stubs remain in `src/AgencyLayer/`):
```
You are Team AGENCY for the Cognitive Mesh project. Read CLAUDE.md, .claude/rules/agency-layer.md, and TODO.md.
Your scope is ONLY src/AgencyLayer/, src/Shared/, and tests/AgencyLayer/.
Tasks:
1. Add XML doc comments to Shared/NodeLabels.cs (build blocker)
2. Complete DecisionExecutor.cs — 3 TODO methods (lines 36, 82, 112) using IDecisionReasoningEngine
3. Complete MultiAgentOrchestrationEngine.cs placeholder methods (lines 160, 169)
4. Complete InMemoryAgentKnowledgeRepository.cs placeholders (lines 31, 52)
5. Complete InMemoryCheckpointManager.cs PurgeWorkflowCheckpoints (line 87)
6. Create MultiAgentOrchestrationEngineTests.cs (critical missing test file)
7. Verify: dotnet test tests/AgencyLayer/ passes
```

### Phase 3 Dispatch:

**Team 5 — Business** (after lower layers are functional):
```
You are Team BUSINESS for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/business-applications.md.
Your scope is ONLY src/BusinessApplications/ and tests/BusinessApplications.UnitTests/.
Tasks:
1. Replace CustomerIntelligenceManager.cs stubs (lines 44, 81, 136, 199) — integrate with lower-layer services
2. Replace DecisionSupportManager.cs stubs (lines 35, 51, 67, 83) — integrate with ConclAIve reasoning
3. Replace ResearchAnalyst.cs stubs (lines 47, 87, 122, 161) — integrate with SemanticSearch
4. Complete ConvenerController.cs NotImplemented features (lines 151-161)
5. Add unit tests for every replaced stub
6. Verify: dotnet build src/BusinessApplications/ passes clean
```

## Step 5: Collect Results & Report

After all sub-agents complete, re-run the state assessment from Step 1. Compare before/after:

```
=== Orchestrator Report ===
Phase completed: [1|2|3|4]
Teams dispatched: [list]

Before:
  TODOs: X | Stubs: Y | Build: [pass/fail] | Tests: [pass/fail]

After:
  TODOs: X | Stubs: Y | Build: [pass/fail] | Tests: [pass/fail]

Items completed: [list]
Items remaining: [list]
Next phase: [2|3|4|DONE]
```

## Step 6: Iterate or Complete

If work remains, loop back to Step 3 and dispatch the next phase. If all phases are complete, commit all changes with a summary message and report completion.

## Arguments

$ARGUMENTS

If arguments are provided, use them to override the default behavior:
- `--phase N` — Skip assessment and run only phase N
- `--team NAME` — Run only the specified team (foundation, reasoning, metacognitive, agency, business, quality)
- `--assess-only` — Only run the assessment, don't dispatch any work
- `--dry-run` — Show what would be dispatched without running it
