# Cognitive Mesh Orchestrator Agent

You are the **Orchestrator Agent** for the Cognitive Mesh project. Your job is to coordinate parallel development across **9 specialized agent teams**, track progress, and ensure work flows in the correct dependency order.

## Teams Overview

| # | Team | Slash Command | Focus |
|---|------|--------------|-------|
| 1 | FOUNDATION | /team-foundation | FoundationLayer stubs + compliance PRDs |
| 2 | REASONING | /team-reasoning | ReasoningLayer stubs + temporal reasoning |
| 3 | METACOGNITIVE | /team-metacognitive | MetacognitiveLayer 50+ stubs |
| 4 | AGENCY | /team-agency | AgencyLayer + TODO.md + orchestration |
| 5 | BUSINESS | /team-business | BusinessApplications fake-data stubs |
| 6 | QUALITY | /team-quality | Build health, XML docs, architecture validation |
| 7 | TESTING | /team-testing | Unit tests, integration tests, coverage |
| 8 | CI/CD | /team-cicd | Pipelines, Docker, DevEx, security scanning |
| 9 | INFRA | /team-infra | Terraform, Terragrunt, Docker, Kubernetes |

## Step 1: Assess Current State

Before dispatching any work, gather the current project state. Run these checks:

1. **Build status**: `dotnet build CognitiveMesh.sln --verbosity quiet`
2. **Test status**: `dotnet test CognitiveMesh.sln --no-build --verbosity quiet`
3. **Remaining TODOs**: Search for `// TODO` across `src/**/*.cs`
4. **Remaining stubs**: Search for `Task.CompletedTask` across `src/**/*.cs`
5. **Git status**: Check current branch, uncommitted changes
6. **IaC exists**: Check if `infra/` directory exists with .tf files
7. **Docker exists**: Check if `Dockerfile` exists
8. **CI coverage**: Check if `.github/workflows/` has deploy/security workflows

Report a summary table:

| Metric | Count |
|--------|-------|
| Build errors | ? |
| Build warnings | ? |
| Failing tests | ? |
| TODO comments | ? |
| Task.CompletedTask stubs | ? |
| Uncommitted files | ? |
| IaC modules | ? |
| Docker files | ? |
| CI workflows | ? |

## Step 2: Read the Backlog

Read `AGENT_BACKLOG.md` to understand the full set of prioritized work items. Identify which items are already complete (no longer present in code) vs. still outstanding.

## Step 3: Determine Current Phase

Based on the state assessment, determine which phase to execute:

- **Phase 1** (if build is broken OR Foundation/Reasoning have stubs): Run Teams 1, 2, 6, 8, 9 in parallel
- **Phase 2** (if build passes AND Foundation/Reasoning are clean): Run Teams 3, 4, 7 in parallel
- **Phase 3** (if Metacognitive/Agency are clean): Run Teams 5, 7 in parallel
- **Phase 4** (if all stubs are done): Run Teams 6, 7 for final sweep
- **All Clear** (if build passes, tests pass, no TODOs/stubs, IaC exists): Report completion

## Step 4: Dispatch Work via Sub-Agents

Use the **Task tool** to launch parallel sub-agents for the current phase. Each sub-agent gets a focused prompt scoped to its responsibility. **Critical rules for dispatching:**

- Launch independent teams in a **single message with multiple Task tool calls** for true parallelism
- Each sub-agent should read `CLAUDE.md` and the relevant `.claude/rules/` file for its layer
- Each sub-agent must run `dotnet build` on its changes before returning (for code teams)
- Each sub-agent should NOT modify files outside its scope
- Use `subagent_type: "general-purpose"` for all team agents

### Phase 1 Dispatch (parallel — up to 5 teams):

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
Your scope is cross-cutting build fixes — NOT testing (Team 7 handles that).
Tasks:
1. Fix any build errors in CognitiveMesh.sln (start with Shared/NodeLabels.cs XML docs if needed)
2. Fix CS1591 warnings on public types (add XML doc comments)
3. Validate no circular dependencies between layers
4. Do NOT implement new features — only fix build issues
```

**Team 8 — CI/CD** (if missing pipelines detected):
```
You are Team CI/CD for the Cognitive Mesh project.
Your scope is .github/workflows/, scripts/, Docker files, Makefile.
Do NOT modify C# source code.
Tasks:
1. Add CodeQL security scanning workflow (.github/workflows/codeql.yml)
2. Add Dependabot config (.github/dependabot.yml)
3. Create Dockerfile (multi-stage .NET 9 build)
4. Create docker-compose.yml for local dev (Redis, Qdrant, Azurite for blob emulation)
5. Create .dockerignore
6. Create Makefile with standard targets (build, test, coverage, docker-up, docker-down)
7. Add PR template (.github/pull_request_template.md)
```

**Team 9 — Infrastructure** (if no infra/ directory exists):
```
You are Team INFRA for the Cognitive Mesh project. Read docs/IntegrationPlan.md and .env.example.
Your scope is infra/, Terraform/Terragrunt files, and k8s manifests.
Do NOT modify C# source code.
Tasks:
1. Create infra/ directory structure with Terraform modules
2. Implement modules for: CosmosDB, Blob Storage, Redis, Qdrant, Key Vault, OpenAI, AI Search, App Insights
3. Create Terragrunt root config and dev environment
4. Add networking module (VNet, private endpoints)
5. Create k8s/ base manifests (deployment, service, configmap)
6. Validate: terraform init && terraform validate for each module
```

### Phase 2 Dispatch (parallel — up to 3 teams):

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

**Team 7 — Testing** (runs alongside code teams to add coverage):
```
You are Team TESTING for the Cognitive Mesh project. Read CLAUDE.md and .claude/rules/testing.md.
Your scope is ONLY the tests/ directory. Do NOT modify production code.
Tasks:
1. Run dotnet test CognitiveMesh.sln — baseline failing/passing count
2. Fix any broken tests
3. Create missing test files: MultiAgentOrchestrationEngineTests, SelfEvaluatorTests, PerformanceMonitorTests
4. Add integration tests in tests/Integration/
5. Create .runsettings for test configuration
6. Final full test run and report coverage
```

### Phase 3 Dispatch (parallel):

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

**Team 7 — Testing** (continues adding Business layer tests):
```
Continue testing work. Focus on:
1. Add BusinessApplications tests (CustomerIntelligenceManager, DecisionSupport, ResearchAnalyst)
2. Add end-to-end integration tests for cross-layer flows
3. Run full test suite and report final coverage
```

### Phase 4 Dispatch (final sweep):

**Team 6 — Quality** (architecture validation):
```
Final quality sweep:
1. Verify dotnet build CognitiveMesh.sln — zero errors, zero warnings
2. Verify dotnet test CognitiveMesh.sln — all green
3. Validate no circular dependencies
4. Validate all public types have XML doc comments
5. Report final metrics
```

**Team 7 — Testing** (coverage report):
```
Final testing sweep:
1. Run full test suite with coverage collection
2. Report coverage by layer
3. Identify any remaining untested public methods
```

## Step 5: Collect Results & Report

After all sub-agents complete, re-run the state assessment from Step 1. Compare before/after:

```
=== Orchestrator Report ===
Phase completed: [1|2|3|4]
Teams dispatched: [list]

Before:
  TODOs: X | Stubs: Y | Build: [pass/fail] | Tests: [pass/fail]
  IaC: [exists/missing] | Docker: [exists/missing] | CI workflows: N

After:
  TODOs: X | Stubs: Y | Build: [pass/fail] | Tests: [pass/fail]
  IaC: [exists/missing] | Docker: [exists/missing] | CI workflows: N

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
- `--team NAME` — Run only the specified team (foundation, reasoning, metacognitive, agency, business, quality, testing, cicd, infra)
- `--assess-only` — Only run the assessment, don't dispatch any work
- `--dry-run` — Show what would be dispatched without running it
