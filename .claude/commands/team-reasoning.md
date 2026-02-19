# Team REASONING — ReasoningLayer Agent

You are **Team REASONING** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/reasoning-layer.md` for layer-specific rules
3. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Source:** `src/ReasoningLayer/` ONLY
- **Tests:** `tests/ReasoningLayer.Tests/` ONLY
- **Dependencies:** May reference FoundationLayer and Shared only
- **Do NOT** create dependencies on MetacognitiveLayer or AgencyLayer

## Backlog

### P1 — Stub Completions
1. `src/ReasoningLayer/SystemsReasoning/SystemsReasoner.cs:79,85` — 2 placeholder implementations

### P2 — PRD Implementations
- Temporal Decision Core (TR-01): `docs/prds/04_Temporal-Flexible-Reasoning/time-bound-decision-classifier.md`
- Real-time Cognitive Augmentation: `docs/prds/04_Temporal-Flexible-Reasoning/real-time-cognitive-augmentation.md`
- Preparation vs Performance: `docs/prds/04_Temporal-Flexible-Reasoning/preparation-vs-performance-dual-architecture.md`

### Tests
- Expand existing Debate + ConclAIve tests
- Add StrategicSimulation, Sequential reasoning, SystemsReasoner tests

## Workflow
1. Assess: Find all remaining stubs/TODOs in `src/ReasoningLayer/`
2. Fix: Complete each stub with real implementation
3. Test: Add unit tests (xUnit + Moq + FluentAssertions)
4. Verify: `dotnet build CognitiveMesh.sln` passes clean
5. Report: List completions and remaining items

$ARGUMENTS
