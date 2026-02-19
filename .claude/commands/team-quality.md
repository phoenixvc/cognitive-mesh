# Team QUALITY — Build, Tests & Integration Agent

You are **Team QUALITY** for the Cognitive Mesh project.

## Setup
1. Read `CLAUDE.md` for project conventions
2. Read `.claude/rules/testing.md` for test conventions
3. Read `.claude/rules/architecture.md` for dependency rules

## Scope
- **Cross-cutting** — you may touch any layer to fix build/test issues
- **Primary focus:** `tests/` directory and XML doc comments
- **Do NOT** implement new features — only fix build, test, and quality issues

## Backlog

### P0 — Build Health
1. Run `dotnet build CognitiveMesh.sln` — fix ALL errors and warnings
2. Fix CS1591 warnings: add `/// <summary>` XML doc comments to public types missing them
3. Ensure TreatWarningsAsErrors passes clean across all projects

### P0 — Test Health
1. Run `dotnet test CognitiveMesh.sln` — identify and fix all failing tests
2. Verify MAKER benchmark tests: `dotnet test tests/AgencyLayer/Orchestration/Orchestration.Tests.csproj`

### P1 — Test Coverage Gaps
1. **Missing test files** (critical gaps):
   - MultiAgentOrchestrationEngine — no tests exist
   - SelfEvaluator — no tests
   - LearningManager — no tests
   - CustomerIntelligenceManager — no tests
   - DecisionSupportManager — no tests
   - ResearchAnalyst — no tests

2. **Integration tests** (`tests/Integration/`):
   - DecisionExecutor -> ConclAIve -> Persistence flow
   - MultiAgent orchestration -> Ethical checks flow
   - MAKER benchmark regression tests

### P2 — Architecture Validation
1. Verify no circular dependencies between layers
2. Check that lower layers don't reference higher layers
3. Validate all public types have XML doc comments

## Workflow
1. Build: `dotnet build CognitiveMesh.sln` — fix everything
2. Test: `dotnet test CognitiveMesh.sln` — fix failures
3. Scan: grep for missing XML docs, circular references
4. Add: Integration tests for cross-layer flows
5. Verify: Full clean build + all tests green
6. Report: Build/test status table, remaining gaps

$ARGUMENTS
