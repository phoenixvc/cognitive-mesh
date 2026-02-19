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

### BLD-001: Fix Shared Project Build Errors
- **File:** `src/Shared/NodeLabels.cs`
- **Issue:** Missing XML doc comments causing CS1591 build failure (TreatWarningsAsErrors=true)
- **Fix:** Add `/// <summary>` XML docs to all public types and members
- **Team:** 6 (Quality) or 4 (Agency)
- **Verify:** `dotnet build CognitiveMesh.sln` passes clean

### BLD-002: Verify Core Test Suites Pass
- **Command:** `dotnet test CognitiveMesh.sln --no-build`
- **Focus:** DecisionExecutorTests, ConclAIveReasoningAdapterTests (per TODO.md)
- **Team:** 6 (Quality)

---

## P1-HIGH: Stub Implementations (Replace Fake Data / Placeholders)

### AGENCY Layer

#### AGN-001: DecisionExecutor — 3 stub methods
- **File:** `src/AgencyLayer/DecisionExecution/DecisionExecutor.cs`
- **Line 36:** `// TODO: Implement actual decision execution logic` — currently uses Task.Delay()
- **Line 82:** `// TODO: Implement actual status retrieval logic` — returns hardcoded success
- **Line 112:** `// TODO: Implement actual log retrieval logic` — returns hardcoded logs
- **Fix:** Integrate with IDecisionReasoningEngine and IMediator for real execution
- **Team:** 4 (Agency)

#### AGN-002: MultiAgentOrchestrationEngine — 2 placeholder methods
- **File:** `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs`
- **Lines 160, 169:** Methods returning Task.CompletedTask
- **Fix:** Implement actual agent lifecycle and learning insight logic
- **Team:** 4 (Agency)

#### AGN-003: InMemoryAgentKnowledgeRepository — 2 placeholders
- **File:** `src/AgencyLayer/MultiAgentOrchestration/Adapters/InMemoryAgentKnowledgeRepository.cs`
- **Lines 31, 52:** Placeholder implementations
- **Fix:** Implement proper in-memory knowledge storage with query support
- **Team:** 4 (Agency)

#### AGN-004: InMemoryCheckpointManager — PurgeWorkflowCheckpoints
- **File:** `src/AgencyLayer/Orchestration/Checkpointing/InMemoryCheckpointManager.cs`
- **Line 87:** Placeholder
- **Fix:** Implement actual checkpoint purge logic
- **Team:** 4 (Agency)

#### AGN-005: DurableWorkflowEngine — Placeholder
- **File:** `src/AgencyLayer/Orchestration/Execution/DurableWorkflowEngine.cs`
- **Line 118:** Placeholder
- **Fix:** Complete implementation
- **Team:** 4 (Agency)

#### AGN-006: Add MultiAgentOrchestrationEngine Tests (CRITICAL GAP)
- **Location:** `tests/AgencyLayer/` — NO test file exists for the core orchestration engine
- **Fix:** Create `tests/AgencyLayer/MultiAgentOrchestration/MultiAgentOrchestrationEngineTests.cs`
- **Cover:** RegisterAgent, ExecuteTask, SetAgentAutonomy, SpawnAgent, coordination patterns
- **Team:** 4 (Agency)

### METACOGNITIVE Layer

#### META-001: SelfEvaluator — 4 TODO methods
- **File:** `src/MetacognitiveLayer/SelfEvaluation/SelfEvaluator.cs`
- **Line 30:** `// TODO: Implement actual performance evaluation logic` — returns hardcoded perfect scores
- **Line 46:** `// TODO: Implement actual learning progress assessment logic`
- **Line 62:** `// TODO: Implement actual insight generation logic`
- **Line 78:** `// TODO: Implement actual behavior validation logic`
- **Fix:** Implement real evaluation using metrics from PerformanceMonitor and HybridMemoryStore
- **Team:** 3 (Metacognitive)

#### META-002: PerformanceMonitor — Threshold checking
- **File:** `src/MetacognitiveLayer/PerformanceMonitoring/PerformanceMonitor.cs`
- **Line 108:** `// TODO: Implement threshold checking logic` — returns Array.Empty
- **Fix:** Implement configurable threshold comparison against collected metrics
- **Team:** 3 (Metacognitive)

#### META-003: ACPHandler — Tool execution
- **File:** `src/MetacognitiveLayer/Protocols/ACP/ACPHandler.cs`
- **Line 240:** `// TODO: Implement actual tool execution logic`
- **Fix:** Implement tool dispatch based on registered tool interfaces
- **Team:** 3 (Metacognitive)

#### META-004: SessionManager — UpdateSession
- **File:** `src/MetacognitiveLayer/Protocols/Common/SessionManager.cs`
- **Line 86:** Placeholder returning Task.CompletedTask
- **Fix:** Implement session state persistence
- **Team:** 3 (Metacognitive)

#### META-005: LearningManager — 48 framework-enablement stubs
- **File:** `src/MetacognitiveLayer/ContinuousLearning/LearningManager.cs`
- **Lines:** 21, 27, 61, 72, 83, 94, 105, 116, 127, 138, 149, 160, 171, 182, 193, 204, 215, 226, 237, 248, 259, 270, 281, 292, 303, 314, 325, 336, 347, 358, 369, 380, 391, 402, 413, 424, 435, 446, 457, 468, 479, 490, 501, 512, 523, 534, 545, 556
- **Pattern:** Multiple `EnableXxxAsync()` methods all returning `Task.CompletedTask`
- **Fix:** Group by pattern (config-based frameworks vs. service-based) and implement enable/disable logic
- **Team:** 3 (Metacognitive)

#### META-006: ContinuousLearningComponent — 2 placeholders
- **File:** `src/MetacognitiveLayer/ContinuousLearning/ContinuousLearningComponent.cs`
- **Lines 455, 461:** Placeholder implementations
- **Fix:** Complete implementation
- **Team:** 3 (Metacognitive)

### FOUNDATION Layer

#### FND-001: DocumentIngestionFunction — Fabric integration
- **File:** `src/FoundationLayer/DocumentProcessing/DocumentIngestionFunction.cs`
- **Line 52:** `// Placeholder for Fabric integration`
- **Fix:** Implement document ingestion pipeline (or proper abstraction/port)
- **Team:** 1 (Foundation)

#### FND-002: EnhancedRAGSystem — Pipeline connections
- **File:** `src/FoundationLayer/SemanticSearch/EnhancedRAGSystem.cs`
- **Lines 208, 214:** `// Connect to Fabric/Orchestrate pipelines`
- **Fix:** Implement RAG pipeline connections
- **Team:** 1 (Foundation)

#### FND-003: SecretsManagementEngine — Placeholder
- **File:** `src/FoundationLayer/Security/Engines/SecretsManagementEngine.cs`
- **Line 117:** Placeholder
- **Fix:** Complete secrets management implementation
- **Team:** 1 (Foundation)

### REASONING Layer

#### RSN-001: SystemsReasoner — 2 placeholders
- **File:** `src/ReasoningLayer/SystemsReasoning/SystemsReasoner.cs`
- **Lines 79, 85:** Placeholder implementations
- **Fix:** Implement systems-level reasoning logic
- **Team:** 2 (Reasoning)

---

## P1-HIGH: CI/CD & DevOps (Team 8)

### CICD-001: Add CodeQL Security Scanning
- **Create:** `.github/workflows/codeql.yml`
- **Purpose:** Automated security vulnerability scanning for C# code
- **Trigger:** PR + weekly schedule
- **Team:** 8 (CI/CD)

### CICD-002: Add Dependabot Configuration
- **Create:** `.github/dependabot.yml`
- **Purpose:** Automated NuGet and GitHub Actions version updates
- **Team:** 8 (CI/CD)

### CICD-003: Create Dockerfile
- **Create:** `Dockerfile` (multi-stage .NET 9 build)
- **Purpose:** Containerize the application for deployment
- **Team:** 8 (CI/CD)

### CICD-004: Create docker-compose for Local Dev
- **Create:** `docker-compose.yml`
- **Services:** Redis, Qdrant, Azurite (Blob emulator), CosmosDB emulator
- **Purpose:** Local development environment matching production dependencies
- **Team:** 8 (CI/CD)

### CICD-005: Create Makefile
- **Create:** `Makefile`
- **Targets:** build, test, coverage, format, clean, docker-up, docker-down
- **Team:** 8 (CI/CD)

### CICD-006: Add PR and Issue Templates
- **Create:** `.github/pull_request_template.md`, `.github/ISSUE_TEMPLATE/`
- **Team:** 8 (CI/CD)

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

### IaC-001: Terraform Module — CosmosDB
- **Create:** `infra/modules/cosmosdb/` (main.tf, variables.tf, outputs.tf)
- **Resources:** azurerm_cosmosdb_account, azurerm_cosmosdb_sql_database
- **Referenced by:** src/FoundationLayer/AzureCosmosDB/CosmosDbAdapter.cs
- **Team:** 9 (Infra)

### IaC-002: Terraform Module — Blob Storage
- **Create:** `infra/modules/storage/`
- **Resources:** azurerm_storage_account, azurerm_storage_container, Data Lake
- **Referenced by:** src/FoundationLayer/AzureBlobStorage/BlobStorageManager.cs
- **Team:** 9 (Infra)

### IaC-003: Terraform Module — Redis Cache
- **Create:** `infra/modules/redis/`
- **Resources:** azurerm_redis_cache
- **Referenced by:** HybridMemoryStore (StackExchange.Redis v2.8.41)
- **Team:** 9 (Infra)

### IaC-004: Terraform Module — Qdrant Vector DB
- **Create:** `infra/modules/qdrant/`
- **Resources:** azurerm_container_group (Qdrant runs as container)
- **Referenced by:** src/FoundationLayer/VectorDatabase/QdrantVectorDatabaseAdapter.cs
- **Team:** 9 (Infra)

### IaC-005: Terraform Module — Azure OpenAI
- **Create:** `infra/modules/openai/`
- **Resources:** azurerm_cognitive_account, azurerm_cognitive_deployment (GPT-3.5, 4o, 4.1, Embeddings)
- **Team:** 9 (Infra)

### IaC-006: Terraform Module — Key Vault
- **Create:** `infra/modules/keyvault/`
- **Resources:** azurerm_key_vault, azurerm_key_vault_secret (for connection strings)
- **Team:** 9 (Infra)

### IaC-007: Terraform Module — AI Search
- **Create:** `infra/modules/ai-search/`
- **Resources:** azurerm_search_service
- **Team:** 9 (Infra)

### IaC-008: Terraform Module — Monitoring
- **Create:** `infra/modules/monitoring/`
- **Resources:** azurerm_application_insights, azurerm_log_analytics_workspace
- **Team:** 9 (Infra)

### IaC-009: Terraform Module — Networking
- **Create:** `infra/modules/networking/`
- **Resources:** azurerm_virtual_network, azurerm_private_endpoint (for CosmosDB, Redis, Storage)
- **Team:** 9 (Infra)

### IaC-010: Terragrunt Root Config + Dev Environment
- **Create:** `infra/terragrunt.hcl`, `infra/environments/dev/terragrunt.hcl`
- **Purpose:** Orchestrate modules with environment-specific variables
- **Team:** 9 (Infra)

### IaC-011: Kubernetes Manifests
- **Create:** `k8s/base/` (deployment.yaml, service.yaml, configmap.yaml)
- **Create:** `k8s/overlays/{dev,staging,prod}/kustomization.yaml`
- **Team:** 9 (Infra)

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

### TST-001: MultiAgentOrchestrationEngine tests
- **Gap:** No test file exists for the core multi-agent engine
- **Team:** 4 (Agency)
- **Note:** Tracked as P1 under AGN-006 — this entry kept for cross-reference

### TST-002: SelfEvaluator tests
- **Gap:** No dedicated test file
- **Team:** 3 (Metacognitive)

### TST-003: LearningManager tests
- **Gap:** 48 methods with no test coverage
- **Team:** 3 (Metacognitive)

### TST-004: PerformanceMonitor tests
- **Gap:** Limited test coverage for threshold checking
- **Team:** 3 (Metacognitive)

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

| Priority | Items | Description |
|----------|-------|-------------|
| P0-CRITICAL | 2 | Build fixes |
| P1-HIGH (stubs) | 16 | Core stub implementations |
| P1-HIGH (CI/CD) | 8 | Pipeline, Docker, DevEx |
| P1-HIGH (IaC) | 11 | Terraform modules + Terragrunt + K8s |
| P2-MEDIUM (stubs) | 4 | Business app fake data |
| P2-MEDIUM (tests) | 8 | Missing test coverage |
| P2-MEDIUM (PRDs) | 8 | Unstarted PRD implementations |
| P3-LOW | 10 | Future enhancements |
| **Total** | **67** | Actionable work items |

---

*Generated: 2026-02-19 | Updated with CI/CD + Infrastructure items*
