# Cognitive Mesh — Agent Backlog

> Prioritized, actionable work items extracted from codebase analysis. Each item includes the exact file, line number, what needs to change, and which agent team owns it.

---

## Priority Legend

- **P0-CRITICAL**: Blocks build or other teams. Do first.
- **P1-HIGH**: Core functionality gaps. Do in Phase 1-2.
- **P2-MEDIUM**: Feature completeness. Do in Phase 2-3.
- **P3-LOW**: Enhancement / polish. Do in Phase 4.

---

## P0-CRITICAL: Build & Infrastructure Fixes

### ~~BLD-001: Fix Shared Project Build Errors~~ DONE (Phase 1)
- **Status:** Shared/NodeLabels.cs already had XML docs. Team Quality added docs to 16 additional files across all layers.

### BLD-002: Verify Core Test Suites Pass
- **Command:** `dotnet test CognitiveMesh.sln --no-build`
- **Focus:** DecisionExecutorTests, ConclAIveReasoningAdapterTests (per TODO.md)
- **Team:** 6 (Quality)
- **Note:** Cannot verify in current environment (no .NET SDK). Needs CI pipeline validation.

### BLD-003: Fix Architecture Violations (NEW - Found in Phase 1)
- **Severity:** CRITICAL — circular dependencies block clean builds
- **Violation 1:** `src/MetacognitiveLayer/Protocols/Protocols.csproj` references `AgencyLayer/ToolIntegration` (Meta->Agency)
- **Violation 2:** `src/MetacognitiveLayer/UncertaintyQuantification/UncertaintyQuantification.csproj` references `AgencyLayer/HumanCollaboration` (Meta->Agency)
- **Violation 3:** `src/FoundationLayer/Notifications/Notifications.csproj` references `BusinessApplications/Common` (Foundation->Business, skips 3 layers)
- **Fix:** Extract shared interfaces to lower layers or Shared project
- **Team:** 6 (Quality)

---

## P1-HIGH: Stub Implementations (Replace Fake Data / Placeholders)

### AGENCY Layer

#### ~~AGN-001: DecisionExecutor — 3 stub methods~~ DONE (Phase 2)
- **Status:** Replaced all 3 Task.Delay stubs with real logic: Stopwatch-based timing, knowledge graph queries, LLM completion, execution tracking via ConcurrentDictionary, log buffer with date range filtering.

#### ~~AGN-002: MultiAgentOrchestrationEngine — 2 placeholder methods~~ DONE (Phase 2)
- **Status:** SetAgentAutonomyAsync now validates and persists autonomy changes as learning insights. ConfigureAgentAuthorityAsync logs endpoint details. Added GetAgentByIdAsync, ListAgentsAsync, UpdateAgentAsync, RetireAgentAsync.

#### ~~AGN-003: InMemoryAgentKnowledgeRepository — 2 placeholders~~ DONE (Phase 2)
- **Status:** Switched to ConcurrentDictionary keyed by InsightId. Multi-signal relevance scoring for GetRelevantInsightsAsync (type match, token overlap, confidence weighting).

#### ~~AGN-004: InMemoryCheckpointManager — PurgeWorkflowCheckpoints~~ DONE (Phase 2)
- **Status:** Input validation, cancellation support, explicit count+clear of removed checkpoints with structured logging.

#### ~~AGN-005: DurableWorkflowEngine — Placeholder~~ DONE (Phase 2)
- **Status:** CancelWorkflowAsync now validates, guards terminal states, signals CancellationTokenSource, saves cancellation checkpoint with step metadata.

#### ~~AGN-006: Add MultiAgentOrchestrationEngine Tests~~ DONE (Phase 2)
- **Status:** Created `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs` — 22 tests covering constructor guards, all coordination patterns (Parallel, Hierarchical, Competitive, CollaborativeSwarm), autonomy, ethical checks, learning insights, spawning.

### METACOGNITIVE Layer

#### ~~META-001: SelfEvaluator — 4 TODO methods~~ DONE (Phase 2)
- **Status:** Real evaluation logic: composite scoring from 7 metric types, domain-appropriate formulas, actionable recommendations. Learning progress from completionRate/iterations. Statistical insight generation with z-score outlier detection. Behavior validation for nulls, empty strings, NaN/Infinity.

#### ~~META-002: PerformanceMonitor — Threshold checking~~ DONE (Phase 2)
- **Status:** Added MetricThreshold config, ThresholdCondition/ThresholdAggregation enums, IMetricsStore interface. CheckThresholdsAsync evaluates registered thresholds against aggregated stats.

#### ~~META-003: ACPHandler — Tool execution~~ DONE (Phase 2)
- **Status:** Multi-dispatch pattern matching: IToolRunner, async Func delegate, sync Func delegate, raw fallback. RequiredTools iteration with error isolation.

#### ~~META-004: SessionManager — UpdateSession~~ DONE (Phase 2)
- **Status:** Atomic AddOrUpdate on ConcurrentDictionary. Handles re-add after cleanup timer removal.

#### ~~META-005: LearningManager — 48 framework-enablement stubs~~ DONE (Phase 2)
- **Status:** Complete rewrite: _enabledFrameworks ConcurrentDictionary, 42-entry prerequisites map, common EnableFrameworkAsync helper. All 48 methods now one-liner delegates with feature flag checks and prerequisite validation.

#### ~~META-006: ContinuousLearningComponent — 2 placeholders~~ DONE (Phase 2)
- **Status:** IntegrateWithFabricForFeedbackAsync generates LLM learning summaries, stores EnrichedFeedback in CosmosDB. IntegrateWithFabricForInteractionAsync detects weak dimensions (<0.7), generates learning signals.

### FOUNDATION Layer

#### ~~FND-001: DocumentIngestionFunction — Fabric integration~~ DONE (Phase 1)
- **Status:** Created `IFabricDataIntegrationPort` interface. Implemented real integration logic with graceful fallback.

#### ~~FND-002: EnhancedRAGSystem — Pipeline connections~~ DONE (Phase 1)
- **Status:** Created `IDataPipelinePort` interface with `ConnectToFabricEndpointsAsync`, `TriggerDataFactoryPipelineAsync`, `GetPipelineRunStatusAsync`. Full implementation with error handling.

#### ~~FND-003: SecretsManagementEngine — Placeholder~~ DONE (Phase 1)
- **Status:** `DeleteSecretAsync` now validates inputs, throws on missing secrets, clears sensitive data on removal.

### REASONING Layer

#### ~~RSN-001: SystemsReasoner — 2 placeholders~~ DONE (Phase 1)
- **Status:** Both methods (`IntegrateWithFabricDataEndpointsAsync`, `OrchestrateDataFactoryPipelinesAsync`) fully implemented with LLM-based logic, feature flags, typed result objects, XML docs.

#### ~~RSN-002: DomainSpecificReasoner — placeholder~~ DONE (Phase 1)
- **Status:** Created `IDomainKnowledgePort` interface. Removed `Task.Delay(100)` and hardcoded data. Real port-based retrieval.

#### ~~RSN-003: ValueGenerationEngine — placeholder data~~ DONE (Phase 1)
- **Status:** Replaced hardcoded strengths/opportunities with data-driven `DeriveStrengths()` and `DeriveDevelopmentOpportunities()` methods.

#### ~~RSN-004: AnalyticalReasoner — 3 Task.Delay~~ DONE (Phase 1)
- **Status:** Created `IDataPlatformIntegrationPort` interface. All 3 `Task.Delay` removed, replaced with port-based integration.

---

## P1-HIGH: CI/CD & DevOps (Team 8)

### ~~CICD-001: Add CodeQL Security Scanning~~ DONE (Phase 1)
- **Status:** Created `.github/workflows/codeql.yml` — C# analysis, PR + weekly triggers, CodeQL v3.

### ~~CICD-002: Add Dependabot Configuration~~ DONE (Phase 1)
- **Status:** Created `.github/dependabot.yml` — NuGet + GitHub Actions ecosystems, grouped updates.

### ~~CICD-003: Create Dockerfile~~ DONE (Phase 1)
- **Status:** Created `Dockerfile` — multi-stage .NET 9 build, non-root user, configurable entrypoint.

### ~~CICD-004: Create docker-compose for Local Dev~~ DONE (Phase 1)
- **Status:** Created `docker-compose.yml` — Redis, Qdrant, Azurite with health checks and persistent volumes.

### ~~CICD-005: Create Makefile~~ DONE (Phase 1)
- **Status:** Created `Makefile` — build, test, coverage, format, lint, clean, docker-up/down, help targets.

### ~~CICD-006: Add PR and Issue Templates~~ DONE (Phase 1)
- **Status:** Created PR template + bug report + feature request YAML forms.

### CICD-007: Add Deployment Pipeline
- **Create:** `.github/workflows/deploy.yml`
- **Purpose:** Build Docker image -> Push to ACR -> Deploy to staging -> Manual gate -> Production
- **Team:** 8 (CI/CD)

### CICD-008: Add Coverage Reporting
- **Fix:** Current `build.yml` collects coverage but doesn't publish
- **Add:** Codecov or SonarQube dashboard integration, coverage badge in README
- **Team:** 8 (CI/CD)

---

## P1-HIGH: Infrastructure-as-Code (Team 9)

### ~~IaC-001 through IaC-009: All 9 Terraform Modules~~ DONE (Phase 1)
- **Status:** Created `infra/modules/` with cosmosdb, storage, redis, qdrant, openai, keyvault, ai-search, monitoring, networking — 32 .tf files total. Root module wires all modules together with Key Vault secret storage.

### ~~IaC-010: Terragrunt Root Config + Dev Environment~~ DONE (Phase 1)
- **Status:** Created `infra/terragrunt.hcl` and `infra/environments/dev/terragrunt.hcl`.

### ~~IaC-011: Kubernetes Manifests~~ DONE (Phase 1)
- **Status:** Created `k8s/base/` (deployment, service, configmap, kustomization) and `k8s/overlays/` (dev, staging, prod) — 7 YAML files with Kustomize.

---

## P2-MEDIUM: Business Application Stubs

### BIZ-001: CustomerIntelligenceManager — 4 fake-data methods
- **File:** `src/BusinessApplications/CustomerIntelligence/CustomerIntelligenceManager.cs`
- **Line 44:** `// TODO: Implement actual customer profile retrieval` — uses Task.Delay, returns sample data
- **Line 81:** `// TODO: Implement actual segment retrieval logic`
- **Line 136:** `// TODO: Implement actual insight generation logic`
- **Line 199:** `// TODO: Implement actual prediction logic`
- **Fix:** Integrate with HybridMemoryStore for profile retrieval, reasoning engines for insights
- **Team:** 5 (Business)

### BIZ-002: DecisionSupportManager — 4 hardcoded methods
- **File:** `src/BusinessApplications/DecisionSupport/DecisionSupportManager.cs`
- **Line 35:** `// TODO: Implement actual decision analysis logic`
- **Line 51:** `// TODO: Implement actual risk evaluation logic` — returns `{ riskLevel: low, riskScore: 0.1 }`
- **Line 67:** `// TODO: Implement actual recommendation generation logic` — returns empty arrays
- **Line 83:** `// TODO: Implement actual outcome simulation logic` — returns empty results
- **Fix:** Integrate with ConclAIve reasoning engines for real analysis
- **Team:** 5 (Business)

### BIZ-003: ResearchAnalyst — 4 fake-data methods
- **File:** `src/BusinessApplications/ResearchAnalysis/ResearchAnalyst.cs`
- **Line 47:** `// TODO: Implement actual research analysis logic`
- **Line 87:** `// TODO: Implement actual research result retrieval logic`
- **Line 122:** `// TODO: Implement actual research search logic`
- **Line 161:** `// TODO: Implement actual research update logic`
- **Fix:** Integrate with SemanticSearch/RAG and knowledge graph for real research
- **Team:** 5 (Business)

### BIZ-004: ConvenerController — 2 NotImplemented features
- **File:** `src/BusinessApplications/ConvenerServices/ConvenerController.cs`
- **Lines 151-161:** Innovation Spread tracking + Learning Catalyst recommendations
- **Fix:** Implement endpoints per docs/prds/03-convener/convener-backend.md
- **Team:** 5 (Business)

---

## P2-MEDIUM: Missing Test Coverage

### ~~TST-001: MultiAgentOrchestrationEngine tests~~ DONE (Phase 2)
- **Status:** Created `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs` — 22 tests covering constructor guards, all coordination patterns, autonomy, ethical checks, learning insights, spawning.

### ~~TST-002: SelfEvaluator tests~~ DONE (Phase 2)
- **Status:** Created `tests/MetacognitiveLayer/SelfEvaluation/SelfEvaluatorTests.cs` — 17 tests covering all 4 evaluation methods, dispose, interface compliance.

### TST-003: LearningManager tests
- **Gap:** 48 methods with no test coverage (now implemented with config-based pattern)
- **Team:** 3 (Metacognitive)

### ~~TST-004: PerformanceMonitor tests~~ DONE (Phase 2)
- **Status:** Created `tests/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitorTests.cs` — 27 tests covering RecordMetric, GetAggregatedStats, QueryMetricsAsync, CheckThresholds, Dispose.

### ~~TST-004b: DecisionExecutor tests~~ DONE (Phase 2)
- **Status:** Created `tests/AgencyLayer/DecisionExecution/DecisionExecutorComprehensiveTests.cs` — 21 tests covering constructor guards, ExecuteDecision, GetStatus, GetLogs, model validation.

### TST-005: CustomerIntelligenceManager tests
- **Gap:** No dedicated test file
- **Team:** 5 (Business)

### TST-006: DecisionSupportManager tests
- **Gap:** No dedicated test file
- **Team:** 5 (Business)

### TST-007: ResearchAnalyst tests
- **Gap:** No dedicated test file
- **Team:** 5 (Business)

### TST-008: Cross-layer integration tests
- **Gap:** Only 1 integration test file exists
- **Need:** DecisionExecutor->ConclAIve->Persistence flow, MultiAgent->EthicalChecks flow
- **Team:** 6 (Quality)

---

## P2-MEDIUM: PRD Implementation (Not Yet Started)

### PRD-001: NIST AI RMF Governance Suite (FI-03)
- **PRDs:** `docs/prds/01-foundational/nist-ai-rmf-maturity/`
- **Deliverable:** AI risk register, maturity scoring dashboard
- **Team:** 1 (Foundation)

### PRD-002: Adaptive Balance & Continuous Improvement (FI-04)
- **PRDs:** `docs/prds/02-adaptive-balance/`
- **Deliverable:** Live spectrums, P95 decision error <=1%
- **Team:** 1 (Foundation)

### PRD-003: Cognitive Sandwich Workflow (AC-02)
- **PRD:** `docs/prds/01-foundational-infrastructure/mesh-orchestration-hitl.md`
- **Deliverable:** Phase-based HITL workflow, 40% hallucination reduction
- **Team:** 4 (Agency)

### PRD-004: Cognitive Sovereignty Control (AC-03)
- **PRD:** `docs/prds/03-agentic-cognitive-systems/human-boundary.md`
- **Deliverable:** User autonomy toggles, audit trail
- **Team:** 4 (Agency)

### PRD-005: Temporal Decision Core (TR-01)
- **PRDs:** `docs/prds/04-temporal-flexible-reasoning/`
- **Deliverable:** Dual-circuit gate, adaptive window, <5% spurious temporal links
- **Team:** 2 (Reasoning)

### PRD-006: Memory & Flexible Strategy (TR-02)
- **PRDs:** `docs/prds/04-temporal-flexible-reasoning/`
- **Deliverable:** Recall F1 +30%, recovery +50%
- **Team:** 2 (Reasoning)

### PRD-007: Value Generation Analytics (VI-01)
- **PRDs:** `docs/prds/04-value-impact/value-generation/`
- **Deliverable:** ROI dashboard, 90% telemetry coverage
- **Team:** 5 (Business)

### PRD-008: Impact-Driven AI Metrics (VI-02)
- **PRDs:** `docs/prds/04-value-impact/impact-driven-ai/`
- **Deliverable:** Psychological safety score >= 80/100
- **Team:** 5 (Business)


---

## P3-LOW: Future Enhancements (per docs/future_enhancements.md)

- Integration testing (Cypress E2E)
- Internationalization (i18n: en-US, fr-FR, de-DE)
- Advanced analytics telemetry
- Performance monitoring instrumentation
- WCAG 2.1 AA/AAA accessibility audit
- Code splitting (React.lazy)
- Service worker caching
- Audit timeline visualizations (D3.js)
- Real-time collaboration features
- Notification integration (email, Teams/Slack)

---

## Summary Counts

| Priority | Total | Done | Remaining | Description |
|----------|-------|------|-----------|-------------|
| P0-CRITICAL | 3 | 1 | 2 | Build fixes + arch violations (1 new) |
| P1-HIGH (stubs) | 16 | 16 | 0 | All core stub implementations complete |
| P1-HIGH (CI/CD) | 8 | 6 | 2 | Pipeline, Docker, DevEx |
| P1-HIGH (IaC) | 11 | 11 | 0 | Terraform modules + Terragrunt + K8s |
| P2-MEDIUM (stubs) | 4 | 0 | 4 | Business app fake data |
| P2-MEDIUM (tests) | 8 | 4 | 4 | 87 tests added, 4 gaps remain |
| P2-MEDIUM (PRDs) | 8 | 0 | 8 | Unstarted PRD implementations |
| P3-LOW | 10 | 0 | 10 | Future enhancements |
| **Total** | **68** | **38** | **30** | Phase 2: +13 items (cumulative 56%) |

---

*Generated: 2026-02-19 | Updated after Phase 2 completion (Metacognitive, Agency, Testing)*
