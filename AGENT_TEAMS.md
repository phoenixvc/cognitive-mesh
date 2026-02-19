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

### Team 6: QUALITY — Build Health & Architecture Validation

**Scope:** Cross-cutting — build errors, XML doc compliance, circular dependency checks

**Responsibilities (narrowed — testing moved to Team 7):**
1. **Build health:** Ensure `dotnet build CognitiveMesh.sln` passes with zero warnings
2. **CS1591 compliance:** Add XML doc comments to all public types
3. **Architecture validation:** Verify no circular dependencies between layers
4. **Code formatting:** `dotnet format` compliance

**Claude Code session prompt:** `/team-quality`

---

### Team 7: TESTING — Dedicated Test Coverage

**Scope:** `tests/` directory — unit tests, integration tests, benchmarks, test infrastructure

**Critical test gaps (no tests exist for):**
- MultiAgentOrchestrationEngine (core multi-agent coordinator)
- SelfEvaluator (self-evaluation)
- PerformanceMonitor (monitoring)
- LearningManager (45 methods, zero tests)
- CustomerIntelligenceManager, DecisionSupportManager, ResearchAnalyst

**Work items:**
- Get all existing 12 test projects green
- Create 7+ missing test files for core components
- Add cross-layer integration tests in `tests/Integration/`
- Create `.runsettings` for test configuration and coverage thresholds
- MAKER benchmark regression tests

**Claude Code session prompt:** `/team-testing`

---

### Team 8: CI/CD — Pipelines, DevOps & Developer Experience

**Scope:** `.github/workflows/`, `scripts/`, Docker files, Makefile, PR/issue templates

**Current state:**
- Only 3 workflows exist: `build.yml`, `api-docs.yml`, `migrate.yml`
- No deployment pipeline, no Docker, no security scanning, no dependency updates

**Work items:**
- Add CodeQL security scanning (`.github/workflows/codeql.yml`)
- Add Dependabot config (`.github/dependabot.yml`)
- Create `Dockerfile` (multi-stage .NET 9 build)
- Create `docker-compose.yml` for local dev (Redis, Qdrant, Azurite blob emulator)
- Create `Makefile` with standard targets
- Add PR template (`.github/pull_request_template.md`)
- Add deployment workflow for staging/production
- Add release workflow with changelog generation

**Claude Code session prompt:** `/team-cicd`

---

### Team 9: INFRA — Terraform, Terragrunt, Docker & Kubernetes

**Scope:** `infra/`, `k8s/`, Terraform modules, Terragrunt environments

**Current state:**
- **Zero IaC exists.** All Azure resources are manually provisioned.
- Known resources: CosmosDB, Blob Storage, Redis, Qdrant, Azure OpenAI, Key Vault, AI Search, App Insights, Event Grid

**Work items:**
- Create `infra/` directory with Terraform module structure
- Implement modules for all 12+ Azure services referenced in code
- Create Terragrunt orchestration for dev/staging/production environments
- Create Kubernetes manifests (base + Kustomize overlays per environment)
- Add networking module (VNet, Private Endpoints for production)
- Naming convention: `{env}-{region}-{resource}-cogmesh`

**Claude Code session prompt:** `/team-infra`

---

## Execution Order

```
Phase 1 (parallel — 5 teams):
  +-- Team 1: FOUNDATION    --- Fix stubs, implement FI-02
  +-- Team 2: REASONING     --- Complete SystemsReasoner, add temporal features
  +-- Team 6: QUALITY       --- Fix build errors, XML docs, architecture check
  +-- Team 8: CI/CD         --- Add Docker, CodeQL, Dependabot, Makefile
  +-- Team 9: INFRA         --- Create Terraform modules, Terragrunt envs

Phase 2 (parallel — 3 teams, after Phase 1 stabilizes):
  +-- Team 3: METACOGNITIVE --- Implement 50+ stubs (SelfEvaluator, LearningManager)
  +-- Team 4: AGENCY        --- Fix TODO.md items, complete orchestration
  +-- Team 7: TESTING       --- Add missing test files, integration tests

Phase 3 (parallel — 2 teams, after lower layers functional):
  +-- Team 5: BUSINESS APPS --- Replace all 12 fake-data stubs
  +-- Team 7: TESTING       --- Add Business layer tests

Phase 4 (final sweep):
  +-- Team 6: QUALITY       --- Full build validation, architecture check
  +-- Team 7: TESTING       --- Full test suite with coverage report
```

---

## How to Launch Agent Sessions

### Option A: Orchestrator Agent (RECOMMENDED — Single Session, Auto-Coordination)

The orchestrator reads the backlog, assesses project state, and dispatches parallel sub-agents:

```bash
# In any Claude Code session (CLI or Web):
/orchestrate

# Or with options:
/orchestrate --assess-only      # Discover + healthcheck without dispatching
/orchestrate --phase 2          # Force a specific phase
/orchestrate --team agency      # Run only Team Agency
/orchestrate --dry-run          # Show what would be dispatched
/orchestrate --status           # Show current state from persistent storage
/orchestrate --reset            # Clear state and start fresh
```

The orchestrator is **fully autonomous across sessions**:
1. Loads persistent state from `.claude/state/orchestrator.json`
2. Runs fresh `/discover` scan (finds new TODOs, stubs, regressions)
3. Runs `/healthcheck` (validates build, deps, interfaces before dispatch)
4. Dispatches the right phase teams in parallel
5. Runs `/sync-backlog`, `/review-pr`, `/pickup-comments` after phase
6. Saves state — next `/orchestrate` picks up from here

```
  Session 1: /orchestrate → Phase 1 → Save State
  Session 2: /orchestrate → Load State → Phase 2 → Save State
  Session 3: /orchestrate → Load State → Phase 3+4 → DONE
```

### Option B: Workflow Agents (run individually between phases)

```bash
/discover                  # Full codebase scan — find ALL remaining work
/discover --quick          # Fast scan (stubs + TODOs + build only)
/discover --layer agency   # Scan only AgencyLayer

/healthcheck               # Validate readiness for next phase
/healthcheck --phase 2     # Check Phase 2 prerequisites specifically

/sync-backlog              # Update AGENT_BACKLOG.md from current codebase
/review-pr 42              # Review PR #42 against all conventions
/pickup-comments           # Process GitHub PR/issue comments
```

### Option C: Team-Specific Slash Commands (Individual Sessions)

```bash
# Code teams (layer-scoped):
/team-foundation       # Team 1: FoundationLayer stubs + FI-02/03/04
/team-reasoning        # Team 2: ReasoningLayer stubs + TR-01/02/03
/team-metacognitive    # Team 3: MetacognitiveLayer 50+ stubs
/team-agency           # Team 4: AgencyLayer + TODO.md items
/team-business         # Team 5: BusinessApplications fake-data stubs

# Support teams (cross-cutting):
/team-quality          # Team 6: Build health + architecture validation
/team-testing          # Team 7: Unit tests, integration tests, coverage
/team-cicd             # Team 8: Pipelines, Docker, DevEx
/team-infra            # Team 9: Terraform, Terragrunt, Kubernetes
```

### Option D: CLI Launcher Script

```bash
./scripts/launch-agent-teams.sh --phase 1    # Foundation + Reasoning + Quality + CI/CD + Infra
./scripts/launch-agent-teams.sh --phase 2    # Metacognitive + Agency + Testing
./scripts/launch-agent-teams.sh --team infra # Single team
./scripts/launch-agent-teams.sh --phase 1 --bg  # Background with logs
```

---

## Slash Command Reference

### Orchestrator
| Command | Purpose |
|---------|---------|
| `/orchestrate` | **Master coordinator** — autonomous loop with state persistence |

### Code & Support Teams
| Command | Purpose | Scope |
|---------|---------|-------|
| `/team-foundation` | FoundationLayer stubs + compliance PRDs | `src/FoundationLayer/` |
| `/team-reasoning` | ReasoningLayer stubs + temporal reasoning PRDs | `src/ReasoningLayer/` |
| `/team-metacognitive` | 50+ MetacognitiveLayer stubs | `src/MetacognitiveLayer/` |
| `/team-agency` | AgencyLayer stubs + TODO.md + orchestration tests | `src/AgencyLayer/` |
| `/team-business` | 12 BusinessApplications fake-data stubs | `src/BusinessApplications/` |
| `/team-quality` | Build health, XML docs, architecture validation | Cross-cutting |
| `/team-testing` | Unit tests, integration tests, coverage, benchmarks | `tests/` |
| `/team-cicd` | Pipelines, Docker, security scanning, DevEx | `.github/`, `scripts/` |
| `/team-infra` | Terraform, Terragrunt, Docker, Kubernetes | `infra/`, `k8s/` |

### Workflow Agents
| Command | Purpose | When to Use |
|---------|---------|-------------|
| `/discover` | Fresh codebase scan — finds ALL remaining work | Start of each orchestrator loop |
| `/healthcheck` | Pre-flight validation — build, deps, interfaces | Before dispatching a phase |
| `/sync-backlog` | Update AGENT_BACKLOG.md with completions | After each phase |
| `/review-pr {N}` | Review PR against all project conventions | Before merging |
| `/pickup-comments` | Process GitHub PR/issue comments | Before starting next phase |

---

## Autonomous Development Loop

```
  ┌──────────────────────────────────────────────────────────────┐
  │  /orchestrate                                                │
  │                                                              │
  │  ┌─ Load State (.claude/state/orchestrator.json) ──────────┐ │
  │  │  Last phase: N | TODOs: X | Stubs: Y | Grade: ...       │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  ┌─ /discover ── Fresh scan (not from stale backlog) ─────┐ │
  │  │  Find ALL: TODOs, stubs, build errors, test gaps,       │ │
  │  │  new work from other teams, regressions, new PRDs       │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  ┌─ /healthcheck ── Validate readiness for next phase ────┐ │
  │  │  Build gate | Dependency gate | Interface gate           │ │
  │  │  FAIL → dispatch Team 6 (Quality) to fix blockers       │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  ┌─ Dispatch Code Teams (parallel via Task tool) ──────────┐ │
  │  │  Phase 1: Teams 1,2,6,8,9 | Phase 2: Teams 3,4,7       │ │
  │  │  Phase 3: Teams 5,7       | Phase 4: Teams 6,7          │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  ┌─ /sync-backlog ── Update completed/new items ───────────┐ │
  │  ├─ /review-pr ── Review changes before merge ─────────────┤ │
  │  ├─ /pickup-comments ── Gather feedback for next phase ────┤ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  ┌─ Save State (.claude/state/orchestrator.json) ──────────┐ │
  │  │  Update metrics, grades, phase_history, next_action      │ │
  │  └─────────────────────────────────────────────────────────┘ │
  │                         │                                    │
  │                         v                                    │
  │  Context room? ──yes──> Loop to Discover ──────────────────┘ │
  │         │                                                    │
  │         no ──> "Run /orchestrate in new session"             │
  └──────────────────────────────────────────────────────────────┘
```

---

## Work Item Summary

| Team | Focus | Stubs | TODOs | New Files | Priority |
|------|-------|-------|-------|-----------|----------|
| 1 Foundation | Layer stubs + PRDs | 3 | 0 | ~10 tests | P0 |
| 2 Reasoning | Layer stubs + PRDs | 2 | 0 | ~8 tests | P0/P1 |
| 3 Metacognitive | 50+ stubs | 50+ | 5 | ~15 tests | P1 |
| 4 Agency | Stubs + TODO.md | 8 | 5 | ~12 tests | P1 |
| 5 Business | Fake-data stubs | 14 | 12 | ~20 tests | P2 |
| 6 Quality | Build/XML/arch | -- | -- | -- | P0 |
| 7 Testing | Test coverage | -- | -- | ~30 test files | P1 |
| 8 CI/CD | Pipelines/Docker | -- | -- | ~8 configs | P1 |
| 9 Infra | Terraform/K8s | -- | -- | ~20 .tf files | P1 |
| **Total** | | **77+** | **22** | **~120+** | |

---

## Monitoring Progress

Run `/orchestrate --status` or check `.claude/state/orchestrator.json` directly.

Manual verification:
```bash
dotnet build CognitiveMesh.sln
dotnet test CognitiveMesh.sln --no-build
grep -r "// TODO" src/ --include="*.cs" | wc -l
grep -r "Task.CompletedTask" src/ --include="*.cs" | wc -l
```

---

*Generated: 2026-02-19 | 9 code teams + 5 workflow agents + autonomous state persistence*
