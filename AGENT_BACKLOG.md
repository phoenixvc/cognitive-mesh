# Cognitive Mesh — Agent Backlog

> Prioritized, actionable work items. Backend is 100% complete (70/70 items). Remaining 39 items are all frontend integration.

---

## Completed (Backend — 70/70)

<details>
<summary>All backend items complete — click to expand</summary>

- **P0-CRITICAL (3/3)**: Build fixes, architecture violations — all resolved
- **P1-HIGH Stubs (16/16)**: All core stub implementations replaced with real logic
  - AGN-001–006: DecisionExecutor, MultiAgentOrchestration, InMemoryAgentKnowledge, CheckpointManager, DurableWorkflow, tests
  - META-001–006: SelfEvaluator, PerformanceMonitor, ACPHandler, SessionManager, LearningManager, ContinuousLearning
  - FND-001–003: DocumentIngestion, EnhancedRAG, SecretsManagement
  - RSN-001–004: SystemsReasoner, DomainSpecificReasoner, ValueGeneration, AnalyticalReasoner
- **P1-HIGH CI/CD (8/8)**: CodeQL, Dependabot, Dockerfile, docker-compose, Makefile, templates, deploy pipeline, coverage
- **P1-HIGH IaC (11/11)**: 9 Terraform modules, Terragrunt, Kubernetes manifests
- **P2-MEDIUM Stubs (5/5)**: CustomerIntelligence, DecisionSupport, ResearchAnalyst, ConvenerController, KnowledgeManager
- **P2-MEDIUM Tests (9/9)**: 334 new backend tests across all layers
- **P2-MEDIUM PRDs (8/8)**: NIST, AdaptiveBalance, CognitiveSandwich, CognitiveSovereignty, TemporalDecision, MemoryStrategy, ValueGeneration, ImpactMetrics
- **P3-LOW (10/10)**: Telemetry, performance monitoring, real-time SignalR, notifications, E2E, i18n, accessibility, code splitting, service worker, D3 visualizations

</details>

---

## Priority Legend

- **P0-CRITICAL**: Blocks all other frontend work. Do first.
- **P1-HIGH**: Core frontend functionality. Do in Phase 1-2.
- **P2-MEDIUM**: Feature completeness. Do in Phase 2-3.
- **P3-LOW**: Enhancement / polish. Do in Phase 4.

---

## P0-CRITICAL: Frontend API Foundation (4 items)

### FE-001: Generate TypeScript API Client from OpenAPI
- **File:** `docs/openapi.yaml` → `src/UILayer/web/src/lib/api/generated/`
- **Action:** Use `openapi-typescript-codegen` or `orval` to auto-generate typed API client from OpenAPI spec. Configure as npm script (`npm run generate-api`).
- **Note:** 11 backend controllers are HTTP-exposed. 2 controllers (AdaptiveBalance, NISTCompliance) need route decorators added first.
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

## P1-HIGH: Frontend State & Infrastructure (6 items)

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
- **Action:** Notification preferences panel: channel toggles (email, push, SMS, in-app), category filters (approvals, security, system), quiet hours with timezone. Wire to backend Notification Preferences API.
- **Team:** 10 (Frontend)

### FE-010: User Profile Page
- **Files:** `src/UILayer/web/src/app/profile/`
- **Action:** User profile view: account info, role display, consent management (GDPR), data export request, session history. Wire to ComplianceController for consent records.
- **Team:** 10 (Frontend)

---

## P1-HIGH: Widget PRD Implementations (5 items)

### FE-011: NIST Compliance Dashboard Widget
- **PRD:** `docs/prds/01-foundational/nist-ai-rmf-maturity/mesh-widget.md`
- **Backend:** `NISTComplianceController` — score, checklist, evidence, gap analysis, roadmap
- **Action:** Maturity score gauge, pillar breakdown, evidence upload form, gap analysis table, roadmap timeline. Wire to 7 REST endpoints.
- **Blocker:** NISTComplianceController needs `[ApiController]` + `[Route]` decorators.
- **Team:** 10 (Frontend)

### FE-012: Adaptive Balance Widget
- **PRD:** `docs/prds/02-adaptive-balance/mesh-widget.md`
- **Backend:** `AdaptiveBalanceController` — spectrum, history, override, learning, reflexion, recommendations
- **Action:** Interactive spectrum sliders (5 dimensions), override controls with approval flow, audit trail, recommendation cards. Real-time updates via SignalR.
- **Blocker:** AdaptiveBalanceController needs `[ApiController]` + `[Route]` decorators.
- **Team:** 10 (Frontend)

### FE-013: Value Generation Widget (upgrade existing)
- **PRD:** `docs/prds/04-value-impact/value-generation/mesh-widget.md`
- **Backend:** `ValueGenerationController` — value-diagnostic, org-blindness, employability
- **Action:** Upgrade existing TwoHundredDollarTestWidget + ValueDiagnosticDashboard. Add consent flow, scoring visualization, strengths/opportunities radar chart, org blindness heatmap. Wire to real API.
- **Team:** 10 (Frontend)

### FE-014: Impact Metrics Widget
- **PRD:** `docs/prds/04-value-impact/impact-driven-ai/mesh-widget.md`
- **Backend:** `ImpactMetricsController` — safety, alignment, adoption, assessment
- **Action:** Psychological safety gauge (6 dimensions), mission alignment radar, adoption telemetry timeline, resistance indicator cards. 8 REST endpoints.
- **Team:** 10 (Frontend)

### FE-015: Cognitive Sandwich Widget
- **PRD:** `docs/prds/01-foundational-infrastructure/mesh-orchestration-hitl.md`
- **Backend:** `CognitiveSandwichController` — create, get, advance, step-back, audit, debt
- **Action:** Phase stepper UI (Human→AI→Human), HITL approval modal, cognitive debt tracker, audit log viewer. Real-time updates via SignalR.
- **Team:** 10 (Frontend)

---

## P2-MEDIUM: Additional Widgets & Navigation (8 items)

### FE-016: Context Engineering Widget
- **PRD:** `docs/prds/03-agentic-cognitive-systems/context-engineering-widget.md`
- **Action:** AI context frame management UI — context window visualizer, token budget, frame composition.

### FE-017: Agentic System Control Widget (upgrade existing)
- **PRD:** `docs/prds/07-agentic-systems/agentic-ai-system/mesh-widget.md`
- **Backend:** `AgentController` — registry CRUD, orchestrate, authority
- **Action:** Upgrade existing AgentControlCenter. Add agent lifecycle management, authority configuration, orchestration trigger with results viewer.

### FE-018: Convener Widget
- **PRD:** `docs/prds/03-convener/convener-widget.md`
- **Backend:** `ConvenerController` — innovation spread, learning catalyst
- **Action:** Innovation spread visualization (Rogers diffusion curve), learning recommendation cards, champion discovery with skill matching.

### FE-019: Widget Marketplace UI
- **Backend:** C# `WidgetRegistry`, `PluginOrchestrator`, `MarketplaceEntry` models
- **Action:** Widget marketplace page: browse, install/uninstall, version management, security sandbox info.

### FE-020: Organizational Mesh Widget
- **PRD:** `docs/prds/08-organizational-transformation/org-mesh-widget.md`
- **Action:** Organization-level cognitive mesh visualization — department network graph, capability heatmap, transformation progress tracker.

### FE-021: Multi-Page Routing
- **Files:** `src/UILayer/web/src/app/` — new route directories
- **Action:** Create pages: `/dashboard`, `/settings`, `/agents`, `/compliance`, `/analytics`, `/marketplace`. Add route-level layouts with shared sidebar. Use Next.js App Router with loading.tsx and error.tsx per route.

### FE-022: Navigation Component
- **Files:** `src/UILayer/web/src/components/Navigation/`
- **Action:** Sidebar navigation with collapsible sections, breadcrumbs, mobile hamburger menu, responsive drawer. Active route highlighting.

### FE-023: Role-Based UI Gating
- **Files:** `src/UILayer/web/src/lib/auth/permissions.ts`
- **Action:** Role-based component visibility (Admin, Analyst, Viewer). Admin-only routes. Permission-gated buttons and forms. Roles from JWT claims.
- **Depends on:** FE-004

---

## P2-MEDIUM: Frontend CI/CD & Deployment (6 items)

### FECICD-001: Add Frontend Build/Test/Lint to CI Pipeline
- **File:** `.github/workflows/build.yml` (update)
- **Action:** Add frontend job: `npm ci`, `npm run lint`, `npm run build`, `npm test -- --ci --coverage`. Upload coverage to Codecov.

### FECICD-002: Frontend Docker Container
- **File:** `src/UILayer/web/Dockerfile` (new)
- **Action:** Multi-stage Docker build: Node 20 build → Nginx alpine runtime. SPA routing, health check, env injection.

### FECICD-003: Add Frontend to docker-compose.yml
- **File:** `docker-compose.yml` (update)
- **Action:** Add `web` service with frontend Dockerfile, port 3000, depends_on backend API.

### FECICD-004: Frontend Deployment Pipeline
- **File:** `.github/workflows/deploy.yml` (update)
- **Action:** Add frontend deployment step: build, push to ACR, deploy to Azure Static Web Apps or AKS.

### FECICD-005: Kubernetes Frontend Manifests
- **Files:** `k8s/base/frontend-deployment.yaml`, `k8s/base/frontend-service.yaml`
- **Action:** K8s deployment, Ingress with TLS, ConfigMap for API URL, HPA for auto-scaling.

### FECICD-006: Terraform Frontend Infrastructure
- **File:** `infra/modules/frontend/` (new)
- **Action:** Terraform module for Azure Static Web Apps or CDN, custom domain, TLS, WAF.

---

## P2-MEDIUM: Frontend Testing (5 items)

### FETEST-001: Component Unit Test Coverage (Target 80%)
- **Files:** `src/UILayer/web/src/components/**/*.test.tsx`
- **Action:** Add unit tests for all 47 components. Currently only 1 test file exists (CognitiveMeshButton). Jest + Testing Library.

### FETEST-002: API Integration Tests
- **Files:** `src/UILayer/web/src/lib/api/__tests__/`
- **Action:** Test generated API client against mock server (MSW). Verify all 13 controller integrations.

### FETEST-003: E2E Tests with Real API
- **Files:** `cypress/e2e/`
- **Action:** Update existing Cypress tests to use real API. Add E2E flows: login → dashboard → agent management → settings → logout.

### FETEST-004: Visual Regression Testing
- **Files:** `.storybook/`, `chromatic.config.js`
- **Action:** Add Chromatic or Percy for visual regression on Storybook stories.

### FETEST-005: Lighthouse CI Performance Monitoring
- **File:** `.github/workflows/lighthouse.yml` (new)
- **Action:** Lighthouse CI on PRs. Thresholds: Performance >= 80, Accessibility >= 95, Best Practices >= 90, SEO >= 80.

---

## P3-LOW: Frontend Advanced Features (5 items)

### FE-024: Dashboard Export (PDF/PNG)
- **Action:** Export dashboard as PDF/PNG via html2canvas + jsPDF.

### FE-025: Command Palette (Cmd+K)
- **Action:** Global keyboard shortcut for search across pages, agents, widgets, settings.

### FE-026: Real-Time Collaboration Presence
- **Action:** Show active users on dashboard via SignalR presence tracking.
- **Depends on:** FE-003

### FE-027: Additional Locale Support
- **Action:** Add es-ES, ja-JP, zh-CN to existing i18n framework (170+ keys each).

### FE-028: PWA Enhancements
- **Action:** Web app manifest, install prompt, push notifications, offline dashboard.

---

## Summary

| Priority | Total | Done | Remaining |
|----------|-------|------|-----------|
| P0-CRITICAL (frontend) | 4 | 0 | **4** |
| P1-HIGH (frontend infra) | 6 | 0 | **6** |
| P1-HIGH (widget PRDs) | 5 | 0 | **5** |
| P2-MEDIUM (widgets + nav) | 8 | 0 | **8** |
| P2-MEDIUM (CI/CD) | 6 | 0 | **6** |
| P2-MEDIUM (testing) | 5 | 0 | **5** |
| P3-LOW (features) | 5 | 0 | **5** |
| **Total remaining** | **39** | **0** | **39** |

## Implementation Phases

```
Phase 13 (Foundation):   FE-001, FE-004, FE-006, FECICD-001
  - API client generation, auth flow, error handling, CI integration
  - Unblockers: Fix 2 controllers missing route decorators (AdaptiveBalance, NIST)

Phase 14 (Core UX):     FE-002, FE-003, FE-005, FE-007, FE-021, FE-022
  - Replace mocks with real API, SignalR, state management, routing, navigation

Phase 15 (Widgets):     FE-011–FE-015, FE-008, FE-009, FE-010
  - 5 PRD widget implementations, settings, notifications, profile

Phase 16 (Expansion):   FE-016–FE-020, FE-023, FECICD-002–006
  - Additional widgets, RBAC, Docker, K8s, Terraform, deployment

Phase 17 (Quality):     FETEST-001–005, FE-024–FE-028
  - Test coverage, visual regression, Lighthouse, export, Cmd+K, PWA
```

### Key Blockers
1. **AdaptiveBalanceController** + **NISTComplianceController** — need `[ApiController]` and `[Route]` attributes before frontend can integrate
2. **No OpenAPI/Swagger config** — backend needs `AddSwaggerGen()` or native OpenAPI support to generate the spec that FE-001 depends on

---

*Updated: 2026-03-10 | Backend 100% complete (70/70). Frontend integration round: 39 items across 5 phases.*
