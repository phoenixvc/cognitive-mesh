# Cognitive Mesh — Agent Backlog

> Prioritized, actionable work items. Backend is 100% complete (70/70 items). Frontend Phase 13 complete (4/4). Remaining 35 frontend items + 6 DevOps evaluation tickets across Phases 14–18.

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

## Completed (Frontend Phase 13 — 4/4)

<details>
<summary>Phase 13 Foundation items complete — click to expand</summary>

- **FE-001**: TypeScript API client from OpenAPI — openapi-typescript + openapi-fetch, type-safe generated clients for Services and Agentic APIs
- **FE-004**: Auth flow — login page, JWT context with access/refresh tokens, protected route wrapper, middleware auth guard, cookie sync for SSR
- **FE-006**: Error handling — ErrorBoundary, Toast system with auto-dismiss, API interceptors (401→logout+redirect, 403→toast, 429→rate limit toast, 5xx→error toast)
- **FECICD-001**: Frontend CI job — npm ci, TypeScript check, Jest tests, coverage upload in build.yml
- **Blocker resolved**: AdaptiveBalance + NIST controller route decorators added

</details>

---

## Priority Legend

- **P0-CRITICAL**: Blocks all other frontend work. Do first.
- **P1-HIGH**: Core frontend functionality. Do in Phase 14-15.
- **P2-MEDIUM**: Feature completeness. Do in Phase 16.
- **P3-LOW**: Enhancement / polish. Do in Phase 17.
- **DEVOPS**: Tool evaluation and selection. Phase 18.

---

## P0-CRITICAL: Frontend API Foundation (4 items — ALL COMPLETE)

~~All items completed in Phase 13. See "Completed (Frontend Phase 13)" above.~~

- ~~FE-001: Generate TypeScript API Client from OpenAPI~~ ✓
- ~~FE-002: Replace Mocked API Service~~ → Moved to Phase 14
- ~~FE-003: Add SignalR Client~~ → Moved to Phase 14
- ~~FE-004: Add Authentication Flow~~ ✓

---

## P1-HIGH: Frontend State & Infrastructure (6 items)

### FE-002: Replace Mocked API Service with Real Backend Integration
- **File:** `src/UILayer/web/src/services/api.ts`
- **Action:** Replace hardcoded `DashboardAPI` singleton with real HTTP calls to all 13 backend controllers. Wire generated client from FE-001. Remove all `Math.random()` and simulated delays.
- **Depends on:** FE-001 ✓
- **Team:** 10 (Frontend)

### FE-003: Add SignalR Client for Real-Time Updates
- **File:** `src/UILayer/web/src/lib/realtime/` (new)
- **Action:** Install `@microsoft/signalr`, connect to `CognitiveMeshHub`. Replace 5-second polling interval with real-time subscriptions (JoinDashboardGroup, SubscribeToAgent). Add reconnection logic with exponential backoff. Connection state indicator in UI.
- **Team:** 10 (Frontend)

### FE-005: Add Global State Management
- **File:** `src/UILayer/web/src/stores/` (new)
- **Action:** Add Zustand stores: `useAuthStore`, `useAgentStore`, `useDashboardStore`, `useNotificationStore`, `usePreferencesStore`. Replace scattered useState with centralized stores. Add persistence middleware for preferences.
- **Depends on:** FE-004 ✓
- **Team:** 10 (Frontend)

### FE-007: Add Loading States & Skeleton Screens
- **Files:** `src/UILayer/web/src/components/skeletons/`
- **Action:** Skeleton components for dashboard panels, agent lists, metrics cards. Suspense boundaries per route. Optimistic updates for mutations (agent status changes, settings saves).
- **Team:** 10 (Frontend)

### FE-008: Settings Page — Theme, Language & Accessibility
- **Files:** `src/UILayer/web/src/app/settings/`
- **Action:** Settings page with sections: Theme (light/dark/system), Language (en-US/fr-FR/de-DE), Accessibility (reduced motion, high contrast, font size), Data & Privacy (consent toggles). Persist to backend user preferences API + localStorage fallback.
- **Team:** 10 (Frontend)

### FE-009: Notification Preferences UI
- **Files:** `src/UILayer/web/src/app/settings/notifications/`
- **Action:** Notification preferences panel: channel toggles (email, push, SMS, in-app), category filters (approvals, security, system), quiet hours with timezone. Wire to backend Notification Preferences API.
- **Team:** 10 (Frontend)

---

## P1-HIGH: Widget PRD Implementations (5 items)

### FE-011: NIST Compliance Dashboard Widget
- **PRD:** `docs/prds/01-foundational/nist-ai-rmf-maturity/mesh-widget.md`
- **Backend:** `NISTComplianceController` — score, checklist, evidence, gap analysis, roadmap
- **Action:** Maturity score gauge (D3 radial), pillar breakdown cards, evidence upload form, gap analysis table with severity indicators, roadmap timeline (Gantt-style). Wire to 7 REST endpoints.
- **Team:** 10 (Frontend)

### FE-012: Adaptive Balance Widget
- **PRD:** `docs/prds/02-adaptive-balance/mesh-widget.md`
- **Backend:** `AdaptiveBalanceController` — spectrum, history, override, learning, reflexion, recommendations
- **Action:** Interactive spectrum sliders (5 dimensions), real-time position updates via SignalR, override controls with approval flow modal, audit trail table, recommendation cards with accept/dismiss.
- **Depends on:** FE-003
- **Team:** 10 (Frontend)

### FE-013: Value Generation Widget (upgrade existing)
- **PRD:** `docs/prds/04-value-impact/value-generation/mesh-widget.md`
- **Backend:** `ValueGenerationController` — value-diagnostic, org-blindness, employability
- **Action:** Upgrade existing TwoHundredDollarTestWidget + ValueDiagnosticDashboard. Add consent flow, scoring visualization (radar chart), strengths/opportunities display, org blindness heatmap. Wire to real API.
- **Team:** 10 (Frontend)

### FE-014: Impact Metrics Widget
- **PRD:** `docs/prds/04-value-impact/impact-driven-ai/mesh-widget.md`
- **Backend:** `ImpactMetricsController` — safety, alignment, adoption, assessment
- **Action:** Psychological safety gauge (6 dimensions), mission alignment radar, adoption telemetry timeline, resistance indicator cards. 8 REST endpoints.
- **Team:** 10 (Frontend)

### FE-015: Cognitive Sandwich Widget
- **PRD:** `docs/prds/01-foundational-infrastructure/mesh-orchestration-hitl.md`
- **Backend:** `CognitiveSandwichController` — create, get, advance, step-back, audit, debt
- **Action:** Phase stepper UI (Human→AI→Human flow), HITL approval modal with diff viewer, cognitive debt tracker (burndown chart), audit log viewer with filtering. Real-time via SignalR.
- **Depends on:** FE-003
- **Team:** 10 (Frontend)

---

## P2-MEDIUM: Additional Widgets & Navigation (8 items)

### FE-010: User Profile Page
- **Files:** `src/UILayer/web/src/app/profile/`
- **Action:** Account info display, role badges, GDPR consent management, data export request button, session history table. Wire to ComplianceController for consent records.

### FE-016: Context Engineering Widget
- **PRD:** `docs/prds/03-agentic-cognitive-systems/context-engineering-widget.md`
- **Action:** AI context frame management UI — context window visualizer, token budget, frame composition editor.

### FE-017: Agentic System Control Widget (upgrade existing)
- **PRD:** `docs/prds/07-agentic-systems/agentic-ai-system/mesh-widget.md`
- **Backend:** `AgentController` — registry CRUD, orchestrate, authority
- **Action:** Upgrade existing AgentControlCenter. Add agent lifecycle management, authority scope configurator, orchestration trigger with real-time results streaming.

### FE-018: Convener Widget
- **PRD:** `docs/prds/03-convener/convener-widget.md`
- **Backend:** `ConvenerController` — innovation spread, learning catalyst
- **Action:** Innovation spread visualization (Rogers diffusion S-curve), learning recommendation cards, champion discovery with skill matching.

### FE-019: Widget Marketplace UI
- **Backend:** C# `WidgetRegistry`, `PluginOrchestrator`, `MarketplaceEntry` models
- **Action:** Widget marketplace page: browse, install/uninstall, version management, security sandbox info badges, dependency resolution display.

### FE-020: Organizational Mesh Widget
- **PRD:** `docs/prds/08-organizational-transformation/org-mesh-widget.md`
- **Action:** Organization-level cognitive mesh visualization — department network graph (D3 force layout), capability heatmap, transformation progress tracker with milestones.

### FE-021: Multi-Page Routing
- **Files:** `src/UILayer/web/src/app/` — new route directories
- **Action:** Create route directories: `/dashboard`, `/settings`, `/agents`, `/compliance`, `/analytics`, `/marketplace`. Add `loading.tsx`, `error.tsx`, `layout.tsx` per route group. Implement parallel route loading.

### FE-022: Navigation Component
- **Files:** `src/UILayer/web/src/components/Navigation/`
- **Action:** Sidebar with collapsible sections, breadcrumbs, mobile hamburger menu. Active route highlighting. Responsive drawer (< 768px). Keyboard navigation (arrow keys).

---

## P2-MEDIUM: Frontend Security & Gating (1 item)

### FE-023: Role-Based UI Gating
- **Files:** `src/UILayer/web/src/lib/auth/permissions.ts`
- **Action:** Permission map from JWT claims. `<RequireRole>` wrapper component. Admin-only routes. Permission-gated buttons/forms. Roles: Admin, Analyst, Viewer.
- **Depends on:** FE-004 ✓

---

## P2-MEDIUM: Frontend CI/CD & Deployment (5 items)

### FECICD-002: Frontend Docker Container
- **File:** `src/UILayer/web/Dockerfile` (new)
- **Action:** Multi-stage build: Node 22 → nginx:alpine. SPA routing config, health check endpoint, runtime env injection via `entrypoint.sh`.

### FECICD-003: Add Frontend to docker-compose.yml
- **File:** `docker-compose.yml` (update)
- **Action:** Add `web` service, port 3000, `depends_on: [api]`, volume mount for config.

### FECICD-004: Frontend Deployment Pipeline
- **File:** `.github/workflows/deploy.yml` (update)
- **Action:** Build → push to ACR → deploy to Azure Static Web Apps or AKS. Staging + production environments.

### FECICD-005: Kubernetes Frontend Manifests
- **Files:** `k8s/base/frontend-deployment.yaml`, `k8s/base/frontend-service.yaml`
- **Action:** Deployment, Service, Ingress with TLS, ConfigMap for `NEXT_PUBLIC_API_BASE_URL`, HPA (2–10 replicas).

### FECICD-006: Terraform Frontend Infrastructure
- **File:** `infra/modules/frontend/` (new)
- **Action:** Azure Static Web Apps module, custom domain, TLS cert, WAF rules, CDN integration.

---

## P2-MEDIUM: Frontend Testing (5 items)

### FETEST-001: Component Unit Test Coverage (Target 80%)
- **Files:** `src/UILayer/web/src/components/**/*.test.tsx`
- **Action:** Tests for all 47+ components. Jest + Testing Library. Mock API calls with MSW. Snapshot tests for static components.

### FETEST-002: API Integration Tests
- **Files:** `src/UILayer/web/src/lib/api/__tests__/`
- **Action:** Test generated API client against MSW mock server. Verify all 13 controller integrations. Error scenario coverage (401, 403, 500, timeout).

### FETEST-003: E2E Tests
- **Files:** `cypress/e2e/` or `playwright/`
- **Action:** Playwright or Cypress. Flows: login → dashboard → agent management → settings → logout. Cross-browser (Chrome, Firefox, Safari).

### FETEST-004: Visual Regression Testing
- **Files:** `.storybook/`, `chromatic.config.js`
- **Action:** Chromatic or Percy integration with Storybook. Baseline screenshots for all widget states. PR comments for visual diffs.

### FETEST-005: Lighthouse CI Performance Monitoring
- **File:** `.github/workflows/lighthouse.yml` (new)
- **Action:** Lighthouse CI on PRs. Thresholds: Performance >= 80, Accessibility >= 95, Best Practices >= 90, SEO >= 80. Block merge on regression.

---

## P3-LOW: Frontend Advanced Features (5 items)

### FE-024: Dashboard Export (PDF/PNG)
- **Action:** html2canvas + jsPDF. Export current dashboard layout with all widget states. Include timestamp and user info.

### FE-025: Command Palette (Cmd+K)
- **Action:** Global keyboard shortcut. Search: pages, agents, widgets, settings, recent actions. Fuzzy matching. Keyboard-navigable results.

### FE-026: Real-Time Collaboration Presence
- **Action:** SignalR presence tracking. Avatar bubbles showing active users per dashboard. Cursor/selection sharing (optional).
- **Depends on:** FE-003

### FE-027: Additional Locale Support
- **Action:** es-ES, ja-JP, zh-CN (170+ keys each). RTL consideration for future ar-SA. Locale-aware number/date formatting.

### FE-028: PWA Enhancements
- **Action:** Web app manifest, install prompt, push notification subscription, offline dashboard with cached last-known state.

---

## DEVOPS: Tool Evaluation & Selection (6 items)

> Each ticket produces a weighted decision matrix and recommendation. Criteria weights: Feature fit 30%, Cost 20%, Integration effort 20%, MCP availability 15%, Community/support 15%. Score 1–5 per candidate.

### DEVOPS-001: Deployment Pipeline Evaluation
- **Candidates:** GitHub Actions (current), Azure DevOps Pipelines, Octopus Deploy
- **Evaluate:** Multi-environment support, approval gates, rollback capability, audit trail, MCP server availability for AI-driven deployment automation
- **Decision matrix:** Feature fit (multi-env, blue-green, canary), Cost (per-seat vs per-pipeline), Integration effort (existing GitHub ecosystem), MCP (agent-triggered deploys), Community (docs, extensions)

### DEVOPS-002: Code Quality Platform Evaluation
- **Candidates:** SonarQube (current), Codacy, CodeClimate, Qodana (JetBrains)
- **Evaluate:** .NET + TypeScript language coverage, PR integration quality, custom rule authoring, MCP server for AI-agent quality queries
- **Decision matrix:** Feature fit (language support, rule depth), Cost (OSS vs cloud), Integration effort (CI hooks, IDE plugins), MCP (query findings from agents), Community (plugin ecosystem)

### DEVOPS-003: Dependency Security Evaluation
- **Candidates:** Dependabot (current), Snyk, Sonatype (Nexus Lifecycle), Mend (WhiteSource)
- **Evaluate:** CVE database coverage, license compliance scanning, auto-remediation PRs, MCP server for agent-driven vulnerability triage
- **Decision matrix:** Feature fit (CVE coverage, license audit), Cost (free tier limits), Integration effort (GitHub native vs external), MCP (auto-triage from agents), Community (enterprise adoption)

### DEVOPS-004: AI Knowledge Graph Evaluation
- **Candidates:** Neo4j, ScapeGraph, Azure AI Search (Cognitive Search)
- **Evaluate:** Knowledge extraction from codebase, semantic reasoning integration, graph query performance, MCP server for agent knowledge queries
- **Decision matrix:** Feature fit (graph modeling, semantic search), Cost (hosted vs self-managed), Integration effort (.NET SDK, REST API), MCP (agent-queryable knowledge base), Community (AI/ML ecosystem)

### DEVOPS-005: Observability Platform Evaluation
- **Candidates:** Azure Monitor (current partial), OpenTelemetry + Grafana, Datadog
- **Evaluate:** .NET auto-instrumentation, distributed tracing, custom dashboards, alerting, MCP server for agent-driven observability queries
- **Decision matrix:** Feature fit (traces, metrics, logs unified), Cost (per-host, per-GB), Integration effort (.NET SDK, collector setup), MCP (query metrics from agents), Community (CNCF backing, docs)

### DEVOPS-006: Task Orchestration Platform Evaluation
- **Candidates:** Azure Boards (current partial), Linear, Serena
- **Evaluate:** AI agent integration for automated task management, MCP server availability, sprint/kanban support, automation rules
- **Decision matrix:** Feature fit (automation, API richness), Cost (per-seat), Integration effort (GitHub sync, webhook), MCP (agent-managed backlogs), Community (developer experience)

---

## Summary

| Priority | Total | Done | Remaining |
|----------|-------|------|-----------|
| P0-CRITICAL (frontend) | 4 | 4 | **0** |
| P1-HIGH (frontend infra) | 6 | 0 | **6** |
| P1-HIGH (widget PRDs) | 5 | 0 | **5** |
| P2-MEDIUM (widgets + nav) | 8 | 0 | **8** |
| P2-MEDIUM (security) | 1 | 0 | **1** |
| P2-MEDIUM (CI/CD) | 5 | 0 | **5** |
| P2-MEDIUM (testing) | 5 | 0 | **5** |
| P3-LOW (features) | 5 | 0 | **5** |
| DEVOPS (evaluation) | 6 | 0 | **6** |
| **Total remaining** | **45** | **4** | **41** |

---

## Implementation Phases

### Phase 13 — Foundation ✓ COMPLETE

**Items:** FE-001, FE-004, FE-006, FECICD-001
**Status:** Complete. All PR comments addressed. CI green.

| Item | Description | Status |
| ---- | ----------- | ------ |
| FE-001 | TypeScript API client (openapi-typescript + openapi-fetch) | ✓ Done |
| FE-004 | Auth flow: login page, JWT context, protected routes, middleware | ✓ Done |
| FE-006 | Error handling: ErrorBoundary, Toast, API interceptors | ✓ Done |
| FECICD-001 | Frontend CI job (lint, tsc, jest, coverage) | ✓ Done |
| Blocker | AdaptiveBalance + NIST controller route decorators | ✓ Done |

**Also completed:** PR review fixes — NotImplementedException stubs replaced in AgentRegistryService (10 methods) and AuthorityService (17 methods), fire-and-forget safety, Guid.Empty fix, SSR-safe ApiBootstrap, middleware JWT validation, Toast memory leak fix, returnTo flow, dependency cleanup.

#### Gate → Phase 14a

- [x] All CI checks green (frontend job, CodeQL, .NET build)
- [x] All P1 PR review comments resolved
- [x] CodeQL findings addressed (structured logging — safe pattern)
- [x] package-lock.json committed, dependency issues fixed

---

### Phase 14a — Dependency Automation

**Items:** CICD-REN-001, CICD-REN-002, CICD-REN-003
**Goal:** Automate dependency management — automerge minor/patch, auto-assign Codex for major migrations.

| Item | Description | Key Work |
| ---- | ----------- | -------- |
| CICD-REN-001 | Renovate automerge for minor/patch | Extend existing patch-only rule to minor+patch. Add `platformAutomerge: true` for GitHub-native merge. Covers both NuGet and npm deps. |
| CICD-REN-002 | Codex auto-assignment for majors | Add `assignees: ["codex[bot]"]` rule for major updates. Gate via `dependencyDashboardApproval`. Codex reads changelog, updates call sites, fixes breaking changes. |
| CICD-REN-003 | agentkit-forge template update | Create issue on agentkit-forge to include Renovate+Codex pattern in bootstrap template for all PhoenixVC repos. |

**Prerequisites:**

- Enable Codex coding agent in Copilot settings for target repos
- Verify Codex bot username after first assignment (`gh api repos/{owner}/{repo}/pulls/{n} --jq '.assignees[].login'`)
- No branch protection on main (verified — automerge will work without bypass config)

#### Gate → Phase 14

- [ ] Renovate minor/patch PRs auto-merging after CI passes
- [ ] Major version PRs assigned to Codex, Codex producing migration diffs
- [ ] agentkit-forge ticket created for template propagation

---

### Phase 14 — Core UX (NEXT)

**Items:** FE-002, FE-003, FE-005, FE-007, FE-021, FE-022
**Goal:** Replace all mocked data with real API calls, add real-time updates, state management, and multi-page navigation.

| Item | Description | Key Work |
| ---- | ----------- | -------- |
| FE-002 | Replace mocked API with real backend | Remove `DashboardAPI` singleton + `Math.random()` calls. Wire all 13 controllers through generated client. Implement request/response mapping for each endpoint. |
| FE-003 | SignalR real-time client | Install `@microsoft/signalr`. Connect to `CognitiveMeshHub`. Replace polling with subscriptions (`JoinDashboardGroup`, `SubscribeToAgent`). Exponential backoff reconnection. Connection state indicator. |
| FE-005 | Zustand state management | Create stores: `useAuthStore`, `useAgentStore`, `useDashboardStore`, `useNotificationStore`, `usePreferencesStore`. Replace scattered `useState`. Add persistence middleware for preferences. |
| FE-007 | Loading states + skeletons | Skeleton components for dashboard panels, agent lists, metrics cards. Suspense boundaries per route. Optimistic updates for mutations. |
| FE-021 | Multi-page routing | Create route dirs: `/dashboard`, `/settings`, `/agents`, `/compliance`, `/analytics`, `/marketplace`. Add `loading.tsx`, `error.tsx`, `layout.tsx` per route group. Parallel route loading. |
| FE-022 | Navigation component | Sidebar with collapsible sections, breadcrumbs, mobile hamburger menu. Active route highlighting. Responsive drawer (< 768px). Keyboard navigation. |

**Also in Phase 14 (deferred from Phase 13):**

- httpOnly cookie for refresh token (requires backend `/api/auth/refresh` set-cookie endpoint)
- Full JWT validation in middleware (requires JWKS endpoint or shared secret config)
- Backend auth middleware in `Program.cs` (`AddAuthentication`/`AddAuthorization`)

#### Gate → Phase 15

- [ ] All 13 backend controllers callable from frontend (no mocked data remains)
- [ ] SignalR connection established and reconnecting properly
- [ ] Navigation between all 6 routes works with loading/error states
- [ ] Zustand stores hydrating from API on mount
- [ ] Storybook stories exist for skeleton and navigation components

---

### Phase 15 — Widgets & User Settings

**Items:** FE-011–FE-015, FE-008, FE-009, FE-010
**Goal:** Implement the 5 PRD-defined widgets and user-facing settings/profile pages.

| Item | Description | Key Work |
| ---- | ----------- | -------- |
| FE-011 | NIST Compliance Dashboard | Maturity score gauge (D3 radial), pillar breakdown cards, evidence upload, gap analysis table, roadmap timeline. 7 endpoints. |
| FE-012 | Adaptive Balance Widget | Interactive spectrum sliders (5 dims), real-time via SignalR, override approval flow, audit trail, recommendations. |
| FE-013 | Value Generation Widget | Upgrade TwoHundredDollarTestWidget + ValueDiagnosticDashboard. Consent flow, radar chart, org blindness heatmap. |
| FE-014 | Impact Metrics Widget | Psychological safety gauge (6 dims), mission alignment radar, adoption timeline, resistance cards. 8 endpoints. |
| FE-015 | Cognitive Sandwich Widget | Phase stepper (Human→AI→Human), HITL approval with diff viewer, cognitive debt burndown, audit log. SignalR. |
| FE-008 | Settings Page | Theme (light/dark/system), language, accessibility (reduced motion, high contrast, font size), privacy consent toggles. |
| FE-009 | Notification Preferences | Channel toggles (email, push, SMS, in-app), category filters, quiet hours with timezone picker. |
| FE-010 | User Profile | Account info, role badges, GDPR consent management, data export, session history. |

#### Gate → Phase 16

- [ ] All 5 PRD widgets rendering with real API data
- [ ] Each widget has at least 1 Storybook story and 1 unit test
- [ ] Settings page persists preferences and applies them (theme, language)
- [ ] GDPR consent flow works end-to-end (consent → backend → audit record)

---

### Phase 16 — Expansion & Deployment

**Items:** FE-016–FE-020, FE-023, FECICD-002–006
**Goal:** Additional widgets, role-based access, containerization, and deployment infrastructure.

| Item | Description | Key Work |
| ---- | ----------- | -------- |
| FE-016 | Context Engineering Widget | Token budget visualizer, frame composition editor, context window gauge. |
| FE-017 | Agentic System Control | Upgrade AgentControlCenter. Lifecycle CRUD, authority scope config, orchestration trigger with streaming results. |
| FE-018 | Convener Widget | Rogers diffusion S-curve, learning recommendations, champion discovery with skill matching. |
| FE-019 | Widget Marketplace | Browse/install/uninstall, version management, security sandbox badges, dependency resolution. |
| FE-020 | Organizational Mesh Widget | Department network graph (D3 force), capability heatmap, transformation progress tracker. |
| FE-023 | Role-Based UI Gating | `<RequireRole>` wrapper, admin-only routes, permission-gated buttons. Roles: Admin, Analyst, Viewer. |
| FECICD-002 | Frontend Docker | Multi-stage Node 22 → nginx:alpine. SPA routing, health check, runtime env injection. |
| FECICD-003 | docker-compose integration | `web` service, port 3000, depends_on api, config volume. |
| FECICD-004 | Deploy pipeline | Build → ACR → Azure Static Web Apps. Staging + production. |
| FECICD-005 | K8s manifests | Deployment, Service, Ingress with TLS, ConfigMap, HPA (2–10 replicas). |
| FECICD-006 | Terraform frontend | Azure Static Web Apps module, custom domain, TLS, WAF, CDN. |

#### Gate → Phase 17

- [ ] Docker build succeeds and container starts cleanly
- [ ] `docker-compose up` brings up full stack (API + frontend + dependencies)
- [ ] RBAC enforced: Viewer cannot access admin routes, roles shown in UI
- [ ] Staging deployment works via CI pipeline
- [ ] All widgets render (even if some show placeholder data)

---

### Phase 17 — Quality & Polish

**Items:** FETEST-001–005, FE-024–FE-028
**Goal:** Comprehensive test coverage, visual regression, performance monitoring, and advanced features.

| Item | Description | Key Work |
| ---- | ----------- | -------- |
| FETEST-001 | Component unit tests (80%) | All 47+ components. Jest + Testing Library. MSW mocks. Snapshot tests for static components. |
| FETEST-002 | API integration tests | Generated client vs MSW mock server. All 13 controllers. Error scenarios (401, 403, 500, timeout). |
| FETEST-003 | E2E tests | Playwright or Cypress. Login → dashboard → agents → settings → logout. Cross-browser. |
| FETEST-004 | Visual regression | Chromatic or Percy + Storybook. Baseline screenshots. PR comments for visual diffs. |
| FETEST-005 | Lighthouse CI | Performance >= 80, A11y >= 95, Best Practices >= 90, SEO >= 80. Block merge on regression. |
| FE-024 | Dashboard export | html2canvas + jsPDF. PDF/PNG export with timestamp and user info. |
| FE-025 | Command palette (Cmd+K) | Global search: pages, agents, widgets, settings, recent actions. Fuzzy matching. |
| FE-026 | Collaboration presence | SignalR presence. Active user avatars per dashboard. Optional cursor sharing. |
| FE-027 | Additional locales | es-ES, ja-JP, zh-CN (170+ keys each). RTL prep for ar-SA. Locale-aware formatting. |
| FE-028 | PWA enhancements | Web manifest, install prompt, push notifications, offline dashboard (cached last state). |

#### Gate → Production

- [ ] Unit test coverage >= 80% across all components
- [ ] All E2E flows passing in CI
- [ ] Lighthouse scores meeting thresholds
- [ ] No visual regressions from baseline
- [ ] PWA installable and functional offline (read-only)

---

### Phase 18 — DevOps & Quality Tooling (Evaluation)

**Items:** DEVOPS-001–006
**Goal:** Evaluate and select best-fit DevOps tools. Each ticket produces a weighted decision matrix and recommendation.

| Ticket | Category | Candidates | MCP Integration |
| ------ | -------- | ---------- | --------------- |
| DEVOPS-001 | Deployment Pipeline | GitHub Actions, Azure DevOps, Octopus Deploy | Agent-triggered deploys, auto-rollback |
| DEVOPS-002 | Code Quality | SonarQube, Codacy, CodeClimate, Qodana | Agent-queryable findings, auto-fix suggestions |
| DEVOPS-003 | Dependency Security | Dependabot, Snyk, Sonatype, Mend | Agent-driven vulnerability triage |
| DEVOPS-004 | AI Knowledge Graphs | Neo4j, ScapeGraph, Azure AI Search | Agent-queryable knowledge base |
| DEVOPS-005 | Observability | Azure Monitor, OpenTelemetry+Grafana, Datadog | Agent-driven metrics queries, auto-alerting |
| DEVOPS-006 | Task Orchestration | Azure Boards, Linear, Serena | Agent-managed backlogs, auto-triage |

**Weighted criteria:** Feature fit 30%, Cost 20%, Integration effort 20%, MCP availability 15%, Community 15%

#### Gate → Adoption

- [ ] Decision matrix reviewed by team for each category
- [ ] Budget approved for selected tools
- [ ] PoC completed for top pick in each category
- [ ] MCP server built or validated for at least 3 of 6 categories

---

*Updated: 2026-03-10 | Backend 100% complete (70/70). Frontend Phase 13 complete (4/4). Remaining: 35 frontend items + 6 DevOps evaluations across Phases 14–18.*
