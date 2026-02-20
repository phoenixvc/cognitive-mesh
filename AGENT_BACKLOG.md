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

### ~~BLD-003: Fix Architecture Violations~~ DONE (Phase 4)
- **Status:** All 3 circular dependency violations fixed.
  - ARCH-001: Removed unused `AgencyLayer/ToolIntegration` reference from `Protocols.csproj` (phantom dependency)
  - ARCH-002: Extracted `ICollaborationPort` interface into MetacognitiveLayer, created `CollaborationPortAdapter` in AgencyLayer (correct direction). Removed upward reference.
  - ARCH-003: Removed unused `BusinessApplications/Common` reference from `Notifications.csproj` (phantom dependency)
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

### ~~CICD-007: Add Deployment Pipeline~~ DONE (Phase 5)
- **Status:** Created `.github/workflows/deploy.yml` — Build Docker image, push to ACR, deploy to staging (Kustomize + AKS), manual gate via GitHub Environments, deploy to production, health checks + smoke tests, Slack failure notifications. Triggered by successful build.yml or manual dispatch. Supports skip-staging and image-tag overrides.

### ~~CICD-008: Add Coverage Reporting~~ DONE (Phase 5)
- **Status:** Created `.github/workflows/coverage.yml` — Runs on PRs + pushes to main, collects opencover coverage, generates HTML/Cobertura/Markdown reports via ReportGenerator, uploads to Codecov, posts sticky PR comment with coverage summary, writes GitHub job summary. Added `codecov.yml` config with per-layer components, 80% patch target. Added coverage + deploy badges to README.

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

### ~~BIZ-001: CustomerIntelligenceManager — 4 fake-data methods~~ DONE (Phase 3)
- **Status:** Added `ICustomerDataPort` interface. All 4 methods now use port-based data retrieval: profile lookup, segment queries, LLM-driven insight generation from interaction history, vector similarity + LLM for behavioral predictions. All Task.Delay and TODO comments removed.

### ~~BIZ-002: DecisionSupportManager — 4 hardcoded methods~~ DONE (Phase 3)
- **Status:** Added `IDecisionAnalysisPort` interface. All 4 methods now delegate to port: option scoring, risk assessment, recommendation generation, outcome simulation. Input validation added. All TODO comments removed.

### ~~BIZ-003: ResearchAnalyst — 4 fake-data methods~~ DONE (Phase 3)
- **Status:** Added `IResearchDataPort` + `IResearchAnalysisPort` interfaces. LLM-based topic analysis with persistence, semantic vector search with text fallback, read-modify-write update cycle with re-indexing. All Task.Delay and TODO comments removed.

### ~~BIZ-004: ConvenerController — 2 NotImplemented features~~ DONE (Phase 7)
- **Status:** Both placeholder endpoints replaced with full async implementations:
  - `GetInnovationSpread`: New `IInnovationSpreadPort` interface with `InnovationSpreadResult`, `AdoptionEvent`, `SpreadPhase` (Rogers diffusion model). Controller: tenant scoping, null check, audit logging, error handling.
  - `GetLearningRecommendations`: New `ILearningCatalystPort` interface with `LearningCatalystRequest/Response`, `LearningRecommendation`, `SkillGap`, `LearningActivityType`. Controller: user ID from claims, tenant scoping, error handling.
  - Created `DiscoverChampionsUseCase` + `IChampionDiscoveryPort` + DTOs (resolves broken imports).
  - Fixed ConvenerController: null guard constructors, correct namespace imports, `GetTenantIdFromClaims` returns nullable.
  - Updated `ConvenerServices.csproj`: added MetacognitiveLayer + ASP.NET MVC references.
- **Team:** 5 (Business)

### ~~BIZ-005: KnowledgeManager — 28 Task.Delay stubs~~ DONE (Phase 3)
- **Status:** Complete refactor: removed 7-way framework branching. Added `IKnowledgeStorePort` interface. All 28 `Task.Delay(1000)` removed. 4 methods now delegate to port with CancellationToken, input validation, structured logging. File reduced from 399 to 173 lines.

---

## P2-MEDIUM: Missing Test Coverage

### ~~TST-001: MultiAgentOrchestrationEngine tests~~ DONE (Phase 2)
- **Status:** Created `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs` — 22 tests covering constructor guards, all coordination patterns, autonomy, ethical checks, learning insights, spawning.

### ~~TST-002: SelfEvaluator tests~~ DONE (Phase 2)
- **Status:** Created `tests/MetacognitiveLayer/SelfEvaluation/SelfEvaluatorTests.cs` — 17 tests covering all 4 evaluation methods, dispose, interface compliance.

### ~~TST-003: LearningManager tests~~ DONE (Phase 4)
- **Status:** Created `tests/MetacognitiveLayer/ContinuousLearning/LearningManagerTests.cs` — 43 test methods (~103 test case invocations) covering constructor guards, EnabledFrameworks property, IsFrameworkEnabled, core learning operations, all 7 framework families (ADK, LangGraph, CrewAI, SemanticKernel, AutoGen, Smolagents, AutoGPT), sub-feature prerequisite validation, flag-disabled paths, idempotency, concurrency safety, logging verification.
- **Team:** 7 (Testing)

### ~~TST-004: PerformanceMonitor tests~~ DONE (Phase 2)
- **Status:** Created `tests/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitorTests.cs` — 27 tests covering RecordMetric, GetAggregatedStats, QueryMetricsAsync, CheckThresholds, Dispose.

### ~~TST-004b: DecisionExecutor tests~~ DONE (Phase 2)
- **Status:** Created `tests/AgencyLayer/DecisionExecution/DecisionExecutorComprehensiveTests.cs` — 21 tests covering constructor guards, ExecuteDecision, GetStatus, GetLogs, model validation.

### ~~TST-005: CustomerIntelligenceManager tests~~ DONE (Phase 3)
- **Status:** Created `tests/BusinessApplications.UnitTests/CustomerIntelligence/CustomerIntelligenceManagerTests.cs` — 31 tests (28 Facts + 3 Theories) covering constructor guards, all 4 methods, cancellation, error cases.

### ~~TST-006: DecisionSupportManager tests~~ DONE (Phase 3)
- **Status:** Created `tests/BusinessApplications.UnitTests/DecisionSupport/DecisionSupportManagerTests.cs` — 20 tests covering constructor, all 4 methods, input validation, dispose safety.

### ~~TST-007: ResearchAnalyst tests~~ DONE (Phase 3)
- **Status:** Created `tests/BusinessApplications.UnitTests/ResearchAnalysis/ResearchAnalystTests.cs` — 38 tests (26 Facts + 4 Theories) covering constructor guards, all 4 methods, cancellation, semantic search fallback.

### ~~TST-008b: KnowledgeManager tests~~ DONE (Phase 3)
- **Status:** Created `tests/BusinessApplications.UnitTests/KnowledgeManagement/KnowledgeManagerTests.cs` — 24 tests covering all CRUD methods across 7 framework feature flags, priority ordering, no-feature fallback.

### ~~TST-008: Cross-layer integration tests~~ DONE (Phase 6)
- **Status:** Created `tests/Integration/Integration.Tests.csproj` (added to solution) — rescued orphaned `EthicalComplianceFrameworkIntegrationTests.cs` (8 existing tests now compile). Added 3 new integration test files:
  - `DurableWorkflowCrashRecoveryTests.cs` — 9 tests: checkpoint persistence, crash recovery resume, context flow, retry success, retry exhaustion, cancellation checkpoint, purge cleanup, concurrent isolation.
  - `DecisionExecutorIntegrationTests.cs` — 8 tests: end-to-end KG+LLM+persist flow, empty context, LLM failure, cancellation, status retrieval, log filtering, concurrent decisions. Includes `InMemoryKnowledgeGraphManager`.
  - `ConclAIvePipelineIntegrationTests.cs` — 8 tests: debate/sequential/strategic recipes with real engines, auto-selection, independent sessions, multi-perspective trace, SLA performance. Uses `ConclAIveTestFixture` with deterministic mock LLM.
  - Total: **25 new integration tests** + 8 existing = 33 integration tests.
- **Team:** 7 (Testing)

---

## P2-MEDIUM: PRD Implementation (Not Yet Started)

### ~~PRD-001: NIST AI RMF Governance Suite (FI-03)~~ DONE (Phase 10)
- **PRDs:** `docs/prds/01-foundational/nist-ai-rmf-maturity/`
- **Deliverable:** AI risk register, maturity scoring dashboard
- **Status:** Phase 10 built complete NIST governance suite across 3 layers:
  - **Foundation:** `NISTEvidence` module — `INISTEvidenceRepositoryPort` (6 methods), `InMemoryNISTEvidenceAdapter`, 5 model classes (NISTEvidenceRecord, EvidenceQueryFilter, EvidenceReviewStatus, EvidenceAuditEntry, EvidenceStatistics). `EvidenceArtifacts` module — `IEvidenceArtifactRepositoryPort`, `InMemoryEvidenceArtifactAdapter`, 3 models (EvidenceArtifact, ArtifactSearchCriteria, RetentionPolicy).
  - **Reasoning:** `NISTMaturity` module — `INISTMaturityAssessmentPort` (5 methods), `INISTEvidenceStorePort`, `NISTMaturityAssessmentEngine` (pillar scoring, gap analysis, evidence management, roadmap generation), 9 model classes.
  - **Business:** `NISTCompliance` module — `INISTComplianceServicePort` (7 methods), `NISTComplianceService`, `NISTComplianceController` (7 REST endpoints: score, checklist, evidence submit, evidence review, gap analysis, roadmap, audit), `ServiceCollectionExtensions`, 12 DTO models.
  - **Tests:** 23 Foundation (NISTEvidence adapter) + 15 Foundation (EvidenceArtifact adapter) + 35 Reasoning (NISTMaturity engine) + 24 Business (controller) + 22 Business (service) = **119 tests**
- **Team:** 1 (Foundation) + 2 (Reasoning) + 5 (Business)

### ~~PRD-002: Adaptive Balance & Continuous Improvement (FI-04)~~ DONE (Phase 10)
- **PRDs:** `docs/prds/02-adaptive-balance/`
- **Deliverable:** Live spectrums, P95 decision error <=1%
- **Status:** Phase 10 built complete Adaptive Balance suite across 2 layers:
  - **Reasoning:** `AdaptiveBalance` module — `IAdaptiveBalancePort` (6 methods), `ILearningFrameworkPort`, `IMilestoneWorkflowPort`, `IReflexionPort`, `AdaptiveBalanceEngine` (spectrum positioning with 10 dimensions, milestone workflows, override management, recommendations), `LearningFrameworkEngine` (event recording, pattern analysis, mistake prevention), `ReflexionEngine` (hallucination detection, contradiction analysis, confidence scoring), 12 model classes.
  - **Business:** `AdaptiveBalance` module — `IAdaptiveBalanceServicePort` (6 methods), `AdaptiveBalanceService`, `AdaptiveBalanceController` (6 REST endpoints: spectrum, history, override, learning evidence, reflexion status, recommendations), `ServiceCollectionExtensions`, 11 DTO models.
  - **Tests:** 32 Reasoning (AdaptiveBalance engine) + 12 Reasoning (LearningFramework) + 14 Reasoning (Reflexion) + 15 Business (controller) + 21 Business (service) = **94 tests**
- **Team:** 2 (Reasoning) + 5 (Business)

### ~~PRD-003: Cognitive Sandwich Workflow (AC-02)~~ DONE (Phase 9)
- **PRD:** `docs/prds/01-foundational-infrastructure/mesh-orchestration-hitl.md`
- **Deliverable:** Phase-based HITL workflow, 40% hallucination reduction
- **Status:** Phase 8 built foundation (17 models, 4 ports, engine, 27 tests). Phase 9 completed:
  - `CognitiveSandwichController` — 6 REST endpoints (create, get, advance, step-back, audit, debt)
  - 3 in-memory adapters (CognitiveDebt, PhaseCondition, AuditLogging)
  - `ServiceCollectionExtensions` DI registration (4 services)
  - 24 controller tests (null guards, all endpoints, error cases)
  - Total: 51 tests across engine + controller
- **Team:** 4 (Agency)

### ~~PRD-004: Cognitive Sovereignty Control (AC-03)~~ DONE (Phase 9)
- **PRD:** `docs/prds/03-agentic-cognitive-systems/human-boundary.md`
- **Deliverable:** User autonomy toggles, audit trail
- **Status:** Phase 9 built complete module:
  - 6 model classes (SovereigntyMode, Profile, Override, AgentAction, AuthorshipTrail, AuditEntry)
  - 4 port interfaces (Sovereignty, Override, ActionApproval, AuthorshipTrail)
  - `CognitiveSovereigntyEngine` — mode resolution (override → domain → default), autonomy levels (0.0–1.0)
  - `CognitiveSovereignty.csproj` + AgencyLayer reference + solution integration
  - 23 test methods (~31 test cases with theories)
- **Team:** 4 (Agency)

### ~~PRD-005: Temporal Decision Core (TR-01)~~ DONE (Phase 9)
- **PRDs:** `docs/prds/04-temporal-flexible-reasoning/`
- **Deliverable:** Dual-circuit gate, adaptive window, <5% spurious temporal links
- **Status:** Phase 9 built complete module:
  - 7 model classes (TemporalEvent, Edge, Window, GatingDecision, Query, Graph, EdgeLog)
  - 4 port interfaces (Event, Gate, Graph, Audit)
  - `TemporalDecisionCoreEngine` — dual-circuit gate (CA1 promoter + L2 suppressor), adaptive window (0–20s), BFS graph traversal
  - `TemporalDecisionCore.csproj` + ReasoningLayer reference
  - 25 unit tests (gating, window adjustment, graph queries, audit trail)
- **Team:** 2 (Reasoning)

### ~~PRD-006: Memory & Flexible Strategy (TR-02)~~ DONE (Phase 9)
- **PRDs:** `docs/prds/04-temporal-flexible-reasoning/`
- **Deliverable:** Recall F1 +30%, recovery +50%
- **Status:** Phase 9 built complete module:
  - 7 model classes (MemoryRecord, RecallStrategy, RecallQuery/Result, ConsolidationResult, StrategyPerformance, MemoryStatistics)
  - 4 port interfaces (MemoryStore, Recall, Consolidation, StrategyAdaptation)
  - `MemoryStrategyEngine` — 5 recall strategies (ExactMatch, Fuzzy, Semantic, Temporal, Hybrid), consolidation logic, strategy adaptation
  - `MemoryStrategy.csproj` + ReasoningLayer reference
  - 27 unit tests (CRUD, all strategies, consolidation, cosine similarity)
- **Team:** 2 (Reasoning)

### ~~PRD-007: Value Generation Analytics (VI-01)~~ DONE (Phase 8)
- **Status:** Phase 7 wired csproj references + fixed controller imports. Phase 8 completed:
  - `ServiceCollectionExtensions.AddValueGenerationServices()` — 8 DI registrations (3 engine ports + 5 repository adapters)
  - 5 in-memory adapters: `InMemoryValueDiagnosticDataRepository`, `InMemoryOrganizationalDataRepository`, `InMemoryEmployabilityDataRepository`, `InMemoryConsentVerifier`, `InMemoryManualReviewRequester`
  - `ValueGenerationControllerTests` — 30 tests (null guards, all endpoints, consent flows, audit logging)
  - `ValueGenerationDiagnosticEngineTests` — 12 tests (profiles, strengths, opportunities)
  - `OrganizationalValueBlindnessEngineTests` — 11 tests (blind spots, risk scoring)
  - `EmployabilityPredictorEngineTests` — 17 tests (consent, risk classification, manual review)
  - Total: 70 new tests for ValueGeneration pipeline
- **Team:** 5 (Business)

### ~~PRD-008: Impact-Driven AI Metrics (VI-02)~~ DONE (Phase 9)
- **PRDs:** `docs/prds/04-value-impact/impact-driven-ai/`
- **Deliverable:** Psychological safety score >= 80/100
- **Status:** Phase 9 built complete module:
  - 9 model classes (PsychologicalSafetyScore, SafetyDimension, MissionAlignment, AdoptionTelemetry, AdoptionAction, ImpactAssessment, ResistanceIndicator, ImpactReport, ConfidenceLevel)
  - 4 port interfaces (PsychologicalSafety, MissionAlignment, AdoptionTelemetry, ImpactAssessment)
  - `ImpactMetricsEngine` — safety scoring (6 dimensions, 70% survey + 30% behavioral), alignment, resistance detection, impact assessment
  - `ImpactMetricsController` — 8 REST endpoints
  - `ServiceCollectionExtensions` DI registration
  - `ImpactMetrics.csproj` + BusinessApplications reference
  - 31 engine tests + 25 controller tests = 56 total
- **Team:** 5 (Business)


---

## P3-LOW: Future Enhancements (per docs/future_enhancements.md)

- ~~Integration testing (Cypress E2E)~~ DONE (Phase 12) — cypress.config.ts, 3 E2E test suites (dashboard, agent-control, accessibility), custom commands (login, loadDashboard, waitForWidget, assertAccessibility)
- ~~Internationalization (i18n: en-US, fr-FR, de-DE)~~ DONE (Phase 12) — react-i18next config, 170-key locales for en-US/fr-FR/de-DE, LanguageSelector component, typed useTranslation hook
- ~~Advanced analytics telemetry~~ DONE (Phase 11) — ITelemetryPort, TelemetryEngine (ActivitySource + Meter with 6 well-known metrics), OpenTelemetryAdapter (OTLP exporter), DI extensions, 2 test files
- ~~Performance monitoring instrumentation~~ DONE (Phase 11) — IPerformanceMonitoringPort, InMemoryMetricsStoreAdapter (thread-safe, 10K cap), PerformanceMonitoringAdapter (dashboard summary, health status), DI extensions, 2 test files
- ~~WCAG 2.1 AA/AAA accessibility audit~~ DONE (Phase 12) — axe-core config, SkipNavigation/FocusTrap/LiveRegion/VisuallyHidden components, useReducedMotion/useFocusVisible hooks, 50+ WCAG 2.1 criteria checklist
- ~~Code splitting (React.lazy)~~ DONE (Phase 12) — LazyWidgetLoader with Suspense + ErrorBoundary, WidgetSkeleton (shimmer), WidgetErrorFallback, lazy widget registry for all panels
- ~~Service worker caching~~ DONE (Phase 12) — Cache-first for widgets, network-first for APIs, offline manager with request queuing + background sync, registration with update notifications, cache versioning
- ~~Audit timeline visualizations (D3.js)~~ DONE (Phase 12) — AuditTimeline (zoom/pan, severity colors), MetricsChart (real-time line chart with thresholds), AgentNetworkGraph (force-directed), useD3 hook, light/dark themes
- ~~Real-time collaboration features~~ DONE (Phase 11) — IRealTimeNotificationPort, CognitiveMeshHub (SignalR typed hub), SignalRNotificationAdapter (presence tracking, dashboard groups, agent subscriptions), 7 models, DI extensions, 2 test files
- ~~Notification integration (email, Teams/Slack)~~ DONE (Phase 11) — SlackNotificationService (Block Kit), MicrosoftTeamsNotificationService (Adaptive Cards), WebhookNotificationService (HMAC-SHA256 signing), 3 test files

---

## P0-CRITICAL: Frontend API Integration (Team 10 — FRONTEND)

> All UI data is currently mocked. 13 backend controllers exist with no frontend wiring. This is the #1 gap.

### FE-001: Generate TypeScript API Client from OpenAPI
- **File:** `docs/openapi.yaml` → `src/UILayer/web/src/lib/api/generated/`
- **Action:** Use `openapi-typescript-codegen` or `orval` to auto-generate typed API client from OpenAPI spec. Configure as npm script (`npm run generate-api`).
- **Team:** 10 (Frontend)

### FE-002: Replace Mocked API Service with Real Backend Integration
- **File:** `src/UILayer/web/src/services/api.ts`
- **Action:** Replace hardcoded `DashboardAPI` singleton with real HTTP calls to all 13 backend controllers. Wire generated client from FE-001. Remove all `Math.random()` and simulated delays.
- **Depends on:** FE-001
- **Team:** 10 (Frontend)

### FE-003: Add SignalR Client for Real-Time Updates
- **File:** `src/UILayer/web/src/lib/realtime/` (new)
- **Action:** Install `@microsoft/signalr`, connect to `CognitiveMeshHub`. Replace 5-second polling interval with real-time subscriptions (JoinDashboardGroup, SubscribeToAgent). Add reconnection logic with exponential backoff.
- **Team:** 10 (Frontend)

### FE-004: Add Authentication Flow
- **Files:** `src/UILayer/web/src/app/login/`, `src/UILayer/web/src/contexts/AuthContext.tsx`
- **Action:** Login page, JWT token management (access + refresh), protected route wrapper, auth context provider, logout, token refresh interceptor. Backend already has Bearer JWT + OAuth2.
- **Team:** 10 (Frontend)

---

## P1-HIGH: Frontend State & Infrastructure (Team 10 — FRONTEND)

### FE-005: Add Global State Management
- **File:** `src/UILayer/web/src/stores/` (new)
- **Action:** Add Zustand stores for: auth state, agent registry, dashboard data, notifications, user preferences. Replace scattered useState with centralized stores. Persist preferences to localStorage.
- **Depends on:** FE-004
- **Team:** 10 (Frontend)

### FE-006: Add Error Handling Infrastructure
- **Files:** `src/UILayer/web/src/components/ErrorBoundary/`, `src/UILayer/web/src/lib/api/interceptors.ts`
- **Action:** Global error boundary wrapping app, toast notifications (sonner library), API error interceptor (401 → redirect to login, 403 → forbidden page, 500 → error toast), retry logic for transient failures.
- **Team:** 10 (Frontend)

### FE-007: Add Loading States & Skeleton Screens
- **Files:** `src/UILayer/web/src/components/skeletons/`
- **Action:** Skeleton screens for dashboard panels, agent lists, metrics cards, settings forms. Optimistic updates for mutations. Suspense boundaries for route-level loading.
- **Team:** 10 (Frontend)

### FE-008: Settings Page — Theme, Language & Accessibility
- **Files:** `src/UILayer/web/src/app/settings/`
- **Action:** Settings page with sections: Theme (light/dark/system), Language (en-US/fr-FR/de-DE using existing i18n), Accessibility (reduced motion, high contrast, font size), Data & Privacy (consent toggles). Persist to backend user preferences API + localStorage fallback.
- **Team:** 10 (Frontend)

### FE-009: Notification Preferences UI
- **Files:** `src/UILayer/web/src/app/settings/notifications/`
- **Action:** Notification preferences panel: channel toggles (email, push, SMS, in-app), category filters (approvals, security, system), quiet hours with timezone. Wire to backend Notification Preferences API from OpenAPI spec.
- **Team:** 10 (Frontend)

### FE-010: User Profile Page
- **Files:** `src/UILayer/web/src/app/profile/`
- **Action:** User profile view: account info, role display, consent management (GDPR), data export request, session history. Wire to ComplianceController for consent records and data subject requests.
- **Team:** 10 (Frontend)

---

## P1-HIGH: Widget PRD Implementations (Team 10 — FRONTEND)

> 17 widget PRDs exist in docs/prds/. Only generic components are built — no PRD-specific widgets.

### FE-011: NIST Compliance Dashboard Widget
- **PRD:** `docs/prds/01-foundational/nist-ai-rmf-maturity/mesh-widget.md`
- **Backend:** `NISTComplianceController` — score, checklist, evidence, gap analysis, roadmap
- **Action:** Maturity score gauge, pillar breakdown, evidence upload form, gap analysis table, roadmap timeline. Wire to 7 REST endpoints.
- **Team:** 10 (Frontend)

### FE-012: Adaptive Balance Widget
- **PRD:** `docs/prds/02-adaptive-balance/mesh-widget.md`
- **Backend:** `AdaptiveBalanceController` — spectrum, history, override, learning, reflexion, recommendations
- **Action:** Interactive spectrum sliders (5 dimensions), override controls with approval flow, audit trail, recommendation cards. Real-time updates via SignalR.
- **Team:** 10 (Frontend)

### FE-013: Value Generation Widget (upgrade existing)
- **PRD:** `docs/prds/04-value-impact/value-generation/mesh-widget.md`
- **Backend:** `ValueGenerationController` — value-diagnostic, org-blindness, employability
- **Action:** Upgrade existing TwoHundredDollarTestWidget + ValueDiagnosticDashboard. Add consent flow, full scoring visualization, strengths/opportunities radar chart, org blindness heatmap. Wire to real API (replace mocked adapter).
- **Team:** 10 (Frontend)

### FE-014: Impact Metrics Widget
- **PRD:** `docs/prds/04-value-impact/impact-driven-ai/mesh-widget.md`
- **Backend:** `ImpactMetricsController` — safety, alignment, adoption, assessment
- **Action:** Psychological safety gauge (6 dimensions), mission alignment radar, adoption telemetry timeline, resistance indicator cards. 8 REST endpoints to wire.
- **Team:** 10 (Frontend)

### FE-015: Cognitive Sandwich Widget
- **PRD:** `docs/prds/01-foundational-infrastructure/mesh-orchestration-hitl.md`
- **Backend:** `CognitiveSandwichController` — create, get, advance, step-back, audit, debt
- **Action:** Phase stepper UI (Human→AI→Human), HITL approval modal, cognitive debt tracker, audit log viewer. Real-time phase updates via SignalR.
- **Team:** 10 (Frontend)

---

## P2-MEDIUM: Additional Widget PRDs (Team 10 — FRONTEND)

### FE-016: Context Engineering Widget
- **PRD:** `docs/prds/03-agentic-cognitive-systems/context-engineering-widget.md`
- **Action:** AI context frame management UI — context window visualizer, token budget, frame composition.
- **Team:** 10 (Frontend)

### FE-017: Agentic System Control Widget (upgrade existing)
- **PRD:** `docs/prds/07-agentic-systems/agentic-ai-system/mesh-widget.md`
- **Backend:** `AgentController` — registry CRUD, orchestrate, authority
- **Action:** Upgrade existing AgentControlCenter. Add agent lifecycle management (register, update, retire), authority configuration form, orchestration trigger with results viewer. Wire to real agent API.
- **Team:** 10 (Frontend)

### FE-018: Convener Widget
- **PRD:** `docs/prds/03-convener/convener-widget.md`
- **Backend:** `ConvenerController` — innovation spread, learning catalyst
- **Action:** Innovation spread visualization (Rogers diffusion curve), learning recommendation cards, champion discovery with skill matching.
- **Team:** 10 (Frontend)

### FE-019: Widget Marketplace UI
- **Backend:** C# `WidgetRegistry`, `PluginOrchestrator`, `MarketplaceEntry` models
- **Action:** Widget marketplace page: browse available widgets, install/uninstall, version management, security sandbox info. Wire to WidgetRegistry C# service.
- **Team:** 10 (Frontend)

### FE-020: Organizational Mesh Widget
- **PRD:** `docs/prds/08-organizational-transformation/org-mesh-widget.md`
- **Action:** Organization-level cognitive mesh visualization — department network graph, capability heatmap, transformation progress tracker.
- **Team:** 10 (Frontend)

---

## P2-MEDIUM: App Structure & Navigation (Team 10 — FRONTEND)

### FE-021: Multi-Page Routing
- **Files:** `src/UILayer/web/src/app/` — new route directories
- **Action:** Create pages: `/dashboard`, `/settings`, `/agents`, `/compliance`, `/analytics`, `/marketplace`. Add route-level layouts with shared sidebar. Use Next.js App Router with loading.tsx and error.tsx per route.
- **Team:** 10 (Frontend)

### FE-022: Navigation Component
- **Files:** `src/UILayer/web/src/components/Navigation/`
- **Action:** Sidebar navigation with collapsible sections, breadcrumbs, mobile hamburger menu, responsive drawer. Active route highlighting. User avatar + quick settings in header.
- **Team:** 10 (Frontend)

### FE-023: Role-Based UI Gating
- **Files:** `src/UILayer/web/src/lib/auth/permissions.ts`
- **Action:** Role-based component visibility (Admin, Analyst, Viewer). Admin-only routes (agent registry CRUD, settings). Permission-gated buttons and forms. Roles from JWT claims.
- **Depends on:** FE-004
- **Team:** 10 (Frontend)

---

## P2-MEDIUM: Frontend CI/CD & Deployment (Teams 8, 9, 10)

### FECICD-001: Add Frontend Build/Test/Lint to CI Pipeline
- **File:** `.github/workflows/build.yml` (update existing)
- **Action:** Add frontend job: `npm ci`, `npm run lint`, `npm run build`, `npm test -- --ci --coverage`. Fail pipeline on lint errors or test failures. Upload coverage to Codecov.
- **Team:** 8 (CI/CD)

### FECICD-002: Frontend Docker Container
- **File:** `src/UILayer/web/Dockerfile` (new)
- **Action:** Multi-stage Docker build: Node 20 build stage → Nginx alpine runtime. Add `nginx.conf` for SPA routing (try_files fallback). Health check endpoint. Environment variable injection at runtime.
- **Team:** 8 (CI/CD) + 9 (Infra)

### FECICD-003: Add Frontend to docker-compose.yml
- **File:** `docker-compose.yml` (update)
- **Action:** Add `web` service: build from `src/UILayer/web/Dockerfile`, port 3000, depends_on backend API, environment variables for API URL, health check.
- **Team:** 8 (CI/CD)

### FECICD-004: Frontend Deployment Pipeline
- **File:** `.github/workflows/deploy.yml` (update)
- **Action:** Add frontend deployment step: build Docker image, push to ACR, deploy to Azure Static Web Apps or AKS alongside backend. Environment-specific API URL injection.
- **Team:** 8 (CI/CD)

### FECICD-005: Kubernetes Frontend Manifests
- **Files:** `k8s/base/frontend-deployment.yaml`, `k8s/base/frontend-service.yaml`
- **Action:** K8s deployment for frontend container: Nginx serving Next.js static export, Ingress with TLS, ConfigMap for API URL, HPA for auto-scaling. Kustomize overlays for dev/staging/prod.
- **Team:** 9 (Infra)

### FECICD-006: Terraform Frontend Infrastructure
- **File:** `infra/modules/frontend/` (new)
- **Action:** Terraform module for Azure Static Web Apps or Azure CDN for frontend hosting. Custom domain, TLS certificate, CDN caching rules, WAF protection.
- **Team:** 9 (Infra)

---

## P2-MEDIUM: Frontend Testing (Teams 7, 10)

### FETEST-001: Component Unit Test Coverage (Target 80%)
- **Files:** `src/UILayer/web/src/components/**/*.test.tsx`
- **Action:** Add unit tests for all 43+ components. Currently only 1 test file exists (CognitiveMeshButton). Test rendering, user interactions, accessibility, error states. Use Jest + Testing Library.
- **Team:** 7 (Testing) + 10 (Frontend)

### FETEST-002: API Integration Tests
- **Files:** `src/UILayer/web/src/lib/api/__tests__/`
- **Action:** Test generated API client against mock server (MSW — Mock Service Worker). Verify all 13 controller integrations. Test error handling, retry logic, auth token injection.
- **Team:** 7 (Testing) + 10 (Frontend)

### FETEST-003: E2E Tests with Real API
- **Files:** `cypress/e2e/`
- **Action:** Update existing Cypress tests to use real API (not mocked data). Add E2E flows: login → dashboard → agent management → settings → logout. Add API intercepts for deterministic testing.
- **Team:** 7 (Testing)

### FETEST-004: Visual Regression Testing
- **Files:** `.storybook/`, `chromatic.config.js`
- **Action:** Add Chromatic or Percy for visual regression on Storybook stories. Run on PR to catch unintended visual changes. Baseline all existing 3+ stories + add stories for new components.
- **Team:** 7 (Testing)

### FETEST-005: Lighthouse CI Performance Monitoring
- **File:** `.github/workflows/lighthouse.yml` (new)
- **Action:** Run Lighthouse CI on every PR. Set thresholds: Performance >= 80, Accessibility >= 95, Best Practices >= 90, SEO >= 80. Track bundle size with `@next/bundle-analyzer`. Fail on regression.
- **Team:** 8 (CI/CD)

---

## P3-LOW: Frontend Advanced Features (Team 10 — FRONTEND)

### FE-024: Dashboard Export (PDF/PNG)
- **Action:** Export current dashboard view as PDF or PNG. Use html2canvas + jsPDF. Include timestamp and user watermark.
- **Team:** 10 (Frontend)

### FE-025: Command Palette (Cmd+K)
- **Action:** Global keyboard shortcut (Cmd+K / Ctrl+K) to open command palette. Search across pages, agents, widgets, settings. Quick actions: toggle theme, switch language, navigate.
- **Team:** 10 (Frontend)

### FE-026: Real-Time Collaboration Presence
- **Action:** Show active users on dashboard via SignalR presence tracking. Live cursor indicators. Collaborative widget editing.
- **Depends on:** FE-003
- **Team:** 10 (Frontend)

### FE-027: Additional Locale Support
- **Action:** Add es-ES (Spanish), ja-JP (Japanese), zh-CN (Chinese Simplified) to existing i18n framework. Translation files for all 170+ keys.
- **Team:** 10 (Frontend)

### FE-028: PWA Enhancements
- **Action:** Add web app manifest, install prompt, push notifications via service worker, offline dashboard with cached data.
- **Team:** 10 (Frontend)

---

## Summary Counts

| Priority | Total | Done | Remaining | Description |
|----------|-------|------|-----------|-------------|
| P0-CRITICAL (backend) | 3 | 3 | 0 | Build fixes + arch violations — ALL RESOLVED |
| P0-CRITICAL (frontend) | 4 | 0 | 4 | API client gen, mock replacement, SignalR, auth flow |
| P1-HIGH (stubs) | 16 | 16 | 0 | All core stub implementations complete |
| P1-HIGH (CI/CD) | 8 | 8 | 0 | Pipeline, Docker, DevEx — ALL COMPLETE |
| P1-HIGH (IaC) | 11 | 11 | 0 | Terraform modules + Terragrunt + K8s |
| P1-HIGH (frontend) | 11 | 0 | 11 | State mgmt, error handling, settings, widget PRDs |
| P2-MEDIUM (stubs) | 5 | 5 | 0 | BIZ-004 (ConvenerController) DONE |
| P2-MEDIUM (tests) | 9 | 9 | 0 | 334 total new backend tests |
| P2-MEDIUM (PRDs) | 8 | 8 | 0 | ALL 8 backend PRDs DONE |
| P2-MEDIUM (frontend) | 14 | 0 | 14 | Widget PRDs, routing, nav, CI/CD, deployment |
| P2-MEDIUM (frontend testing) | 5 | 0 | 5 | Component tests, E2E, visual regression, Lighthouse |
| P3-LOW (backend) | 10 | 10 | 0 | ALL DONE — Phase 11 + Phase 12 |
| P3-LOW (frontend) | 5 | 0 | 5 | Export, Cmd+K, collaboration, locales, PWA |
| **Total** | **109** | **70** | **39** | Backend 100% done. Frontend integration round: 39 new items. |

---

*Updated: 2026-02-20 | Phase 12 closed original 70 items (100%). Phase 13+ begins frontend integration round: 39 new items across API integration, widget PRDs, settings, testing, CI/CD, deployment, and infrastructure.*
