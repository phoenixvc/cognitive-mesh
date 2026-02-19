# Team BUSINESS — BusinessApplications Agent

You are **Team BUSINESS** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/business-applications.md` for layer-specific rules
3. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Source:** `src/BusinessApplications/` ONLY
- **Tests:** `tests/BusinessApplications.UnitTests/` ONLY
- **Dependencies:** May reference all lower layers (Foundation, Reasoning, Metacognitive, Agency)

## Backlog (12 fake-data stubs to replace)

### P2 — Stub Completions

1. `src/BusinessApplications/CustomerIntelligence/CustomerIntelligenceManager.cs`
   - Line 44: `// TODO: Implement actual customer profile retrieval` — returns sample data via Task.Delay
   - Line 81: `// TODO: Implement actual segment retrieval logic`
   - Line 136: `// TODO: Implement actual insight generation logic`
   - Line 199: `// TODO: Implement actual prediction logic`
   - **Integration target:** HybridMemoryStore for data, reasoning engines for insights

2. `src/BusinessApplications/DecisionSupport/DecisionSupportManager.cs`
   - Line 35: `// TODO: Implement actual decision analysis logic`
   - Line 51: `// TODO: Implement actual risk evaluation logic` — returns hardcoded low risk
   - Line 67: `// TODO: Implement actual recommendation generation logic` — returns empty
   - Line 83: `// TODO: Implement actual outcome simulation logic` — returns empty
   - **Integration target:** ConclAIve reasoning engines

3. `src/BusinessApplications/ResearchAnalysis/ResearchAnalyst.cs`
   - Line 47: `// TODO: Implement actual research analysis logic`
   - Line 87: `// TODO: Implement actual research result retrieval logic`
   - Line 122: `// TODO: Implement actual research search logic`
   - Line 161: `// TODO: Implement actual research update logic`
   - **Integration target:** SemanticSearch/RAG + knowledge graph

4. `src/BusinessApplications/ConvenerServices/ConvenerController.cs`
   - Lines 151-161: NotImplemented — Innovation Spread tracking + Learning Catalyst recommendations
   - **PRD:** `docs/prds/03-convener/convener-backend.md`

### Tests (none exist for these managers)
- Add CustomerIntelligenceManagerTests.cs
- Add DecisionSupportManagerTests.cs
- Add ResearchAnalystTests.cs
- Add ConvenerControllerTests.cs

## Workflow
1. Read the interfaces these managers depend on to understand available services
2. Replace each stub with real logic using injected dependencies
3. Add comprehensive unit tests with mocked dependencies
4. Verify: `dotnet build CognitiveMesh.sln` passes clean
5. Report: Summary of completions

$ARGUMENTS
