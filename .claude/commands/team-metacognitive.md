# Team METACOGNITIVE — MetacognitiveLayer Agent

You are **Team METACOGNITIVE** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/metacognitive-layer.md` for layer-specific rules
3. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Source:** `src/MetacognitiveLayer/` ONLY
- **Tests:** `tests/MetacognitiveLayer/` ONLY
- **CRITICAL:** Do NOT reference AgencyLayer from this layer (circular dependency violation)

## Backlog (HIGHEST stub count — 50+ items)

### P1 — Stub Completions
1. `src/MetacognitiveLayer/SelfEvaluation/SelfEvaluator.cs`
   - Line 30: `// TODO: Implement actual performance evaluation logic` — returns hardcoded perfect scores
   - Line 46: `// TODO: Implement actual learning progress assessment logic`
   - Line 62: `// TODO: Implement actual insight generation logic`
   - Line 78: `// TODO: Implement actual behavior validation logic`

2. `src/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitor.cs`
   - Line 108: `// TODO: Implement threshold checking logic` — returns empty array

3. `src/MetacognitiveLayer/Protocols/ACP/ACPHandler.cs`
   - Line 240: `// TODO: Implement actual tool execution logic`

4. `src/MetacognitiveLayer/Protocols/Common/SessionManager.cs`
   - Line 86: UpdateSessionAsync — returns Task.CompletedTask

5. `src/MetacognitiveLayer/ContinuousLearning/LearningManager.cs`
   - **45 methods** all returning Task.CompletedTask (EnableXxxAsync pattern)
   - Group by category and implement config-based enable/disable logic

6. `src/MetacognitiveLayer/ContinuousLearning/ContinuousLearningComponent.cs`
   - Lines 455, 461: Placeholder implementations

### Tests
- Expand ReasoningTransparency and UncertaintyQuantification test suites
- Add tests for: SelfEvaluator, PerformanceMonitor, LearningManager, SessionManager, ACPHandler

## Workflow
1. Assess: Find all stubs/TODOs in `src/MetacognitiveLayer/`
2. Prioritize: SelfEvaluator > PerformanceMonitor > ACPHandler > SessionManager > LearningManager
3. Fix: Complete each stub. For LearningManager, implement a pattern-based approach
4. Test: Add unit tests for each implementation
5. Verify: `dotnet build CognitiveMesh.sln` passes clean
6. Report: Summary of completions

$ARGUMENTS
