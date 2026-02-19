# Cognitive Mesh — Agent Team Setup

> How to organize parallel Claude Code agent sessions (or any AI coding agent team) to systematically implement PRDs, complete outstanding functionality, fix TODOs, and expand test coverage across this codebase.

---

## Strategy Overview

The Cognitive Mesh has **5 architectural layers**, **100+ PRDs**, **19 TODO comments**, **57 stub implementations**, and **12 test projects**. A single agent cannot efficiently tackle all of this. Instead, partition the work into **6 specialized agent teams**, each owning a layer or cross-cutting concern.

**Key principle:** Respect the dependency direction `Foundation <- Reasoning <- Metacognitive <- Agency <- Business`. Teams working on lower layers should complete first (or provide interfaces) before upper-layer teams integrate.

---

## Team Definitions

### Team 1: FOUNDATION — Infrastructure & Compliance

**Scope:** `src/FoundationLayer/` + Epics FI-02, FI-03, FI-04

**PRDs to implement:**
- `docs/prds/01-foundational/ethical-legal-compliance-framework.md` (FI-02, Ready)
- `docs/prds/01-foundational/nist-ai-rmf-maturity/backend-architecture.md` (FI-03)
- `docs/prds/01-foundational/nist-ai-rmf-maturity/mesh-widget.md` (FI-03)
- `docs/prds/02-adaptive-balance/backend-architecture.md` (FI-04)
- `docs/prds/01_Foundational-Infrastructure/Policy Access Control.md`
- `docs/prds/01_Foundational-Infrastructure/Connector Plugin.md`

**Outstanding stubs to complete:**
- `src/FoundationLayer/DocumentProcessing/DocumentIngestionFunction.cs:52` — Fabric integration placeholder
- `src/FoundationLayer/SemanticSearch/EnhancedRAGSystem.cs:208,214` — Fabric/pipeline connection stubs
- `src/FoundationLayer/Security/Engines/SecretsManagementEngine.cs:117` — Placeholder

**Tests to add:**
- Security policy engine tests exist (`tests/FoundationLayer/Security/`) — expand coverage
- Add tests for Document Processing, Semantic Search, Vector Database adapters

**Claude Code session prompt:**
```
You are working on the FoundationLayer of the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md for conventions. Your focus:
1. Implement the Ethical & Legal Compliance Core (FI-02) per docs/prds/01-foundational/ethical-legal-compliance-framework.md
2. Complete stubs in DocumentIngestionFunction.cs, EnhancedRAGSystem.cs, SecretsManagementEngine.cs
3. Add unit tests for all new implementations (xUnit + Moq + FluentAssertions)
4. Ensure dotnet build passes with zero warnings (TreatWarningsAsErrors=true, XML docs required on public types)
Do NOT modify files outside src/FoundationLayer/ and tests/FoundationLayer/ without explicit approval.
```

---

### Team 2: REASONING — Structured Reasoning & Temporal Decision

**Scope:** `src/ReasoningLayer/` + Epics TR-01, TR-02, TR-03

**PRDs to implement:**
- `docs/prds/04_Temporal-Flexible-Reasoning/real-time-cognitive-augmentation.md` (TR-01)
- `docs/prds/04_Temporal-Flexible-Reasoning/preparation-vs-performance-dual-architecture.md` (TR-02)
- `docs/prds/04_Temporal-Flexible-Reasoning/time-bound-decision-classifier.md` (TR-01)
- NOT_INTEGRATED variants in same folder indicate these features need codebase integration

**Outstanding stubs to complete:**
- `src/ReasoningLayer/SystemsReasoning/SystemsReasoner.cs:79,85` — Placeholder implementations

**Tests to add:**
- `tests/ReasoningLayer.Tests/` exists with Debate + ConclAIve tests — expand
- Add tests for StrategicSimulation, Sequential reasoning, SystemsReasoner
- Add tests for temporal decision capabilities once implemented

**Claude Code session prompt:**
```
You are working on the ReasoningLayer of the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md for conventions. Your focus:
1. Complete SystemsReasoner stub implementations
2. Implement Temporal Decision Core (TR-01) per docs/prds/04_Temporal-Flexible-Reasoning/
3. Add the time-bound-decision-classifier capability
4. Expand test coverage for all reasoning engines
5. Ensure all public types have XML doc comments (CS1591)
Do NOT create dependencies on AgencyLayer or MetacognitiveLayer (dependency flows upward only).
```

---

### Team 3: METACOGNITIVE — Memory, Self-Evaluation & Monitoring

**Scope:** `src/MetacognitiveLayer/` + related epics

**PRDs to implement:**
- `docs/prds/03_Agentic-Cognitive-Systems/Context Engineer.md`
- `docs/prds/03_Agentic-Cognitive-Systems/Human Boundary.md`
- `docs/prds/01_Foundational-Infrastructure/Human-AI Boundary Definition Protocol PRD.md`

**Outstanding stubs to complete (HIGH PRIORITY — 50+ stubs):**
- `src/MetacognitiveLayer/SelfEvaluation/SelfEvaluator.cs:30,46,62,78` — 4 TODO stubs returning hardcoded perfect scores
- `src/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitor.cs:108` — Threshold checking returns empty
- `src/MetacognitiveLayer/Protocols/ACP/ACPHandler.cs:240` — Tool execution placeholder
- `src/MetacognitiveLayer/Protocols/Common/SessionManager.cs:86` — UpdateSession placeholder
- `src/MetacognitiveLayer/ContinuousLearning/LearningManager.cs` — **45 methods** returning Task.CompletedTask
- `src/MetacognitiveLayer/ContinuousLearning/ContinuousLearningComponent.cs:455,461` — Placeholders

**Tests to add:**
- `tests/MetacognitiveLayer/ReasoningTransparency/` — exists, expand
- `tests/MetacognitiveLayer/UncertaintyQuantification.Tests/` — exists, expand
- Add tests for SelfEvaluator, PerformanceMonitor, LearningManager, SessionManager

**Claude Code session prompt:**
```
You are working on the MetacognitiveLayer of the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md for conventions. Your focus:
1. Implement SelfEvaluator (4 TODO methods) with real evaluation logic
2. Implement PerformanceMonitor.CheckThresholdsAsync with actual threshold checking
3. Implement ACPHandler.ExecuteToolsAsync with proper tool dispatch
4. Implement the 45 LearningManager framework-enablement methods (group by pattern)
5. Add comprehensive tests for all implementations
CRITICAL: Do NOT reference AgencyLayer from this layer (circular dependency violation).
```

---

### Team 4: AGENCY — Orchestration, Multi-Agent & Workflows

**Scope:** `src/AgencyLayer/` + Epics AC-01, AC-02, AC-03

**PRDs to implement:**
- `docs/prds/07-agentic-systems/agentic-ai-system/agentic-ai-system.md` (AC-01, In Progress)
- `docs/prds/07-agentic-systems/agentic-ai-system/backend-architecture.md`
- `docs/prds/07-agentic-systems/agentic-ai-system/implementation.md`
- `docs/prds/01_Foundational-Infrastructure/mesh-orchestration-hitl.md` (AC-02)
- `docs/prds/03_Agentic-Cognitive-Systems/Agent Comms.md`
- `docs/prds/03_Agentic-Cognitive-Systems/Mesh Agent Communication Protocols.md`

**Outstanding stubs to complete:**
- `src/AgencyLayer/DecisionExecution/DecisionExecutor.cs:36,82,112` — 3 TODO stubs with Task.Delay simulation
- `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs:160,169` — Placeholder methods
- `src/AgencyLayer/MultiAgentOrchestration/Adapters/InMemoryAgentKnowledgeRepository.cs:31,52` — Placeholders
- `src/AgencyLayer/Orchestration/Checkpointing/InMemoryCheckpointManager.cs:87` — PurgeWorkflowCheckpoints placeholder
- `src/AgencyLayer/Orchestration/Execution/DurableWorkflowEngine.cs:118` — Placeholder

**TODO.md immediate tasks (owned by this team):**
1. Fix build errors in Shared project (NodeLabels.cs XML comments)
2. Verify DecisionExecutorTests and ConclAIveReasoningAdapterTests pass
3. Verify persistence aligns with Knowledge Graph schema
4. Integration with ActionPlanner workflow
5. Reasoning recipe selection refinement for DecisionType

**Tests to expand:**
- `tests/AgencyLayer/Orchestration/` — MAKER benchmark + workflow engine tests exist
- `tests/AgencyLayer/DecisionExecution/` — Decision executor tests exist but need verification
- `tests/AgencyLayer/ActionPlanning/` — exists
- `tests/AgencyLayer/HumanCollaboration/` — exists
- `tests/AgencyLayer/ToolIntegration/` — 11 tool test files exist
- Add multi-agent orchestration engine tests (none currently exist)

**Claude Code session prompt:**
```
You are working on the AgencyLayer of the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md and TODO.md for context. Your focus:
1. Fix build: add XML doc comments to Shared/NodeLabels.cs
2. Complete DecisionExecutor stubs (replace Task.Delay with real logic using IDecisionReasoningEngine)
3. Complete MultiAgentOrchestrationEngine placeholder methods
4. Add tests for MultiAgentOrchestrationEngine (none exist yet — critical gap)
5. Implement Cognitive Sandwich Workflow (AC-02) per mesh-orchestration-hitl.md
6. Verify all existing tests pass: dotnet test tests/AgencyLayer/ --no-build
```

---

### Team 5: BUSINESS APPS — Customer Intelligence, Decision Support & Compliance

**Scope:** `src/BusinessApplications/` + Epics VI-01, VI-02

**PRDs to implement:**
- `docs/prds/04-value-impact/value-generation/value-generation.md` (VI-01, In Progress)
- `docs/prds/04-value-impact/value-generation/backend-architecture.md`
- `docs/prds/04-value-impact/impact-driven-ai/impact-driven-ai.md` (VI-02)
- `docs/prds/04-value-impact/impact-driven-ai/backend-architecture.md`
- `docs/prds/03-convener/convener-backend.md`
- `docs/prds/05_Value-Impact/` — multiple PRDs for opportunity detection, value tracking, friction detection

**Outstanding stubs to complete (ALL high priority):**
- `src/BusinessApplications/CustomerIntelligence/CustomerIntelligenceManager.cs:44,81,136,199` — 4 TODO methods returning fake data
- `src/BusinessApplications/DecisionSupport/DecisionSupportManager.cs:35,51,67,83` — 4 TODO methods returning hardcoded values
- `src/BusinessApplications/ResearchAnalysis/ResearchAnalyst.cs:47,87,122,161` — 4 TODO methods returning fake data
- `src/BusinessApplications/ConvenerServices/ConvenerController.cs:151-161` — NotImplemented: Innovation Spread + Learning Catalyst

**Tests to add:**
- `tests/BusinessApplications.UnitTests/AgentRegistry/` — exists, expand significantly
- Add tests for CustomerIntelligenceManager, DecisionSupportManager, ResearchAnalyst
- Add integration tests for ConvenerServices

**Claude Code session prompt:**
```
You are working on the BusinessApplications layer of the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md for conventions. Your focus:
1. Replace all 12 TODO stub methods with real implementations that integrate with lower-layer services
2. CustomerIntelligenceManager: integrate with HybridMemoryStore + reasoning engines for real insights
3. DecisionSupportManager: integrate with ConclAIve reasoning for actual decision analysis
4. ResearchAnalyst: integrate with SemanticSearch/RAG for real research capabilities
5. Complete ConvenerController NotImplemented features (Innovation Spread, Learning Catalyst)
6. Add comprehensive unit tests for every implementation
This layer depends on Foundation, Reasoning, Metacognitive, and Agency layers.
```

---

### Team 6: QUALITY & INTEGRATION — Build, Tests, CI/CD

**Scope:** Cross-cutting — build health, test gaps, integration tests

**Responsibilities:**
1. **Build health:** Ensure `dotnet build CognitiveMesh.sln` passes clean
2. **Test gaps:** Identify and fill missing test coverage across all layers
3. **Integration tests:** Expand `tests/Integration/` with cross-layer scenarios
4. **CI/CD:** Ensure GitHub Actions workflow (`dotnet.yml`) runs green

**Priority work items:**
- Fix CS1591 warnings (missing XML doc comments on public types)
- Verify all 12 test projects pass: `dotnet test CognitiveMesh.sln`
- Add integration tests for: DecisionExecutor -> ConclAIve -> Persistence flow
- Add integration tests for: MultiAgent orchestration -> Ethical checks flow
- Add MAKER benchmark regression tests
- Verify no circular dependencies between layers

**Claude Code session prompt:**
```
You are the Quality & Integration team for the Cognitive Mesh .NET 9 solution.
Read CLAUDE.md for conventions. Your focus:
1. Run `dotnet build CognitiveMesh.sln` and fix ALL warnings/errors
2. Run `dotnet test CognitiveMesh.sln` and fix ALL failing tests
3. Add missing XML doc comments on public types (CS1591)
4. Add integration tests in tests/Integration/ for cross-layer workflows
5. Verify no circular references between layers (Foundation<-Reasoning<-Metacognitive<-Agency<-Business)
6. Report build/test status and remaining gaps
```

---

## Execution Order

```
Phase 1 (parallel):
  +-- Team 1: FOUNDATION  --- Fix stubs, implement FI-02
  +-- Team 2: REASONING   --- Complete SystemsReasoner, add temporal features
  +-- Team 6: QUALITY     --- Fix build, get all existing tests green

Phase 2 (parallel, after Phase 1 interfaces stabilize):
  +-- Team 3: METACOGNITIVE --- Implement 50+ stubs (SelfEvaluator, LearningManager)
  +-- Team 4: AGENCY        --- Fix TODO.md items, complete orchestration

Phase 3 (after lower layers are functional):
  +-- Team 5: BUSINESS APPS --- Replace all 12 fake-data stubs with real integrations

Phase 4 (final):
  +-- Team 6: QUALITY --- Full integration test pass, CI green, coverage report
```

---

## How to Launch Agent Sessions

### Option A: Orchestrator Agent (RECOMMENDED — Single Session, Auto-Coordination)

The orchestrator reads the backlog, assesses project state, and dispatches parallel sub-agents:

```bash
# In any Claude Code session (CLI or Web):
/orchestrate

# Or with options:
/orchestrate --assess-only      # Just report current state
/orchestrate --phase 2          # Skip to Phase 2
/orchestrate --team agency      # Run only Team Agency
/orchestrate --dry-run          # Show plan without executing
```

The orchestrator automatically:
1. Checks build/test status and remaining TODOs/stubs
2. Determines the correct phase based on project state
3. Launches the right teams in parallel via Task sub-agents
4. Collects results and reports before/after metrics

### Option B: Team-Specific Slash Commands (Individual Sessions)

Run a single team in a dedicated Claude Code session:

```bash
/team-foundation       # Team 1: FoundationLayer stubs + FI-02/03/04
/team-reasoning        # Team 2: ReasoningLayer stubs + TR-01/02/03
/team-metacognitive    # Team 3: MetacognitiveLayer 50+ stubs
/team-agency           # Team 4: AgencyLayer + TODO.md items
/team-business         # Team 5: BusinessApplications fake-data stubs
/team-quality          # Team 6: Build fixes, test gaps, integration
```

### Option C: CLI Launcher Script (Multiple Parallel Terminals)

```bash
# Launch specific phase (opens instructions for parallel terminals):
./scripts/launch-agent-teams.sh --phase 1

# Launch single team:
./scripts/launch-agent-teams.sh --team foundation

# Background mode with log files:
./scripts/launch-agent-teams.sh --phase 1 --bg

# Dry run:
./scripts/launch-agent-teams.sh --phase 2 --dry-run
```

### Option D: Claude Code Web (Multiple Sessions)

1. Open multiple Claude Code web sessions on this repository
2. Type the team slash command (e.g., `/team-agency`) in each session
3. Each session works independently on its scoped layer
4. Use dedicated branches per team to avoid merge conflicts:
   - `claude/team-foundation`
   - `claude/team-reasoning`
   - `claude/team-metacognitive`
   - `claude/team-agency`
   - `claude/team-business`
   - `claude/team-quality`

---

## Slash Command Reference

| Command | Purpose | Scope |
|---------|---------|-------|
| `/orchestrate` | Master coordinator — auto-detects phase, dispatches teams | All layers |
| `/team-foundation` | FoundationLayer stubs + compliance PRDs | `src/FoundationLayer/` |
| `/team-reasoning` | ReasoningLayer stubs + temporal reasoning PRDs | `src/ReasoningLayer/` |
| `/team-metacognitive` | 50+ MetacognitiveLayer stubs | `src/MetacognitiveLayer/` |
| `/team-agency` | AgencyLayer stubs + TODO.md + orchestration tests | `src/AgencyLayer/` |
| `/team-business` | 12 BusinessApplications fake-data stubs | `src/BusinessApplications/` |
| `/team-quality` | Cross-cutting build/test/integration fixes | All layers |

---

## Work Item Summary

| Team | Layer | Stubs to Fix | TODOs | Tests to Add | PRDs to Implement | Priority |
|------|-------|-------------|-------|-------------|-------------------|----------|
| 1 | Foundation | 3 | 0 | ~10 | 6 | P0 |
| 2 | Reasoning | 2 | 0 | ~8 | 3 | P0/P1 |
| 3 | Metacognitive | 50+ | 5 | ~15 | 3 | P1 |
| 4 | Agency | 8 | 5 | ~12 | 6 | P1 |
| 5 | Business | 14 | 12 | ~20 | 6+ | P2 |
| 6 | Quality | -- | -- | ~10 integration | -- | Cross-cutting |
| **Total** | | **77+** | **22** | **~75** | **24+** | |

---

## Monitoring Progress

After each agent session, verify:

```bash
# Build passes
dotnet build CognitiveMesh.sln

# All tests pass
dotnet test CognitiveMesh.sln --no-build

# MAKER benchmark specifically
dotnet test tests/AgencyLayer/Orchestration/Orchestration.Tests.csproj --no-build

# Check for remaining TODOs
grep -r "// TODO" src/ --include="*.cs" | wc -l

# Check for remaining stubs
grep -r "Task.CompletedTask" src/ --include="*.cs" | wc -l
```

---

*Generated: 2026-02-19*
