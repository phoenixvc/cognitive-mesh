---
Module: ConvenerWidget
Primary Personas: Project Leads, Community Managers, Innovation Leads
Core Value Proposition: UI widgets for champion discovery, community pulse, and innovation spread
Priority: P2
License Tier: Professional
Platform Layers: UI, Business Applications
Main Integration Points: Convener backend, Dashboard system, Widget registry
---

# Convener Widget / Plugin PRD  
_Path: docs/prds/convener-widget.md_

## TL;DR  
The Convener Widget is a collection of UI plugins for the Cognitive Mesh dashboard that surface **Champion Discovery**, **Community Pulse**, **Innovation Spread**, and **Learning Catalyst** insights produced by the Convener backend (`convener-backend.md`).  
Each widget registers via the dashboard's `registerWidget()` API, runs in a sandbox, follows the sandwich-pattern orchestration, and fully complies with the **Global NFR Appendix** (`global-nfr.md`).

---

## Goals  
| Type | Goal |
|------|------|
| Business | Increase Convener feature adoption by ≥ 30 % within 90 days of launch. |
| User | Provide at-a-glance, actionable insights with visible data provenance and consent controls. |
| Technical | Deliver pluggable widgets that load in < 1 s, honour tenant isolation, and emit OpenTelemetry. |

### Non-Goals  
* Implementing backend data science / ranking logic (handled in `convener-backend.md`).  
* Providing deep workflow automations (covered by future roadmap widgets).

---

## User Stories  

| Persona | Story | Priority |
|---------|-------|----------|
| Project Lead | "I drag the **Champion Finder** widget onto my dashboard and immediately see the top experts for 'MLOps'." | Must |
| Community Manager | "I resize the **Community Pulse** widget to full width to view sentiment over the past quarter." | Must |
| Innovation Lead | "Hovering a node in **Innovation Spread** shows adoption metrics and provenance." | Should |
| Learner | "The **Learning Catalyst** widget suggests courses and lets me mark them complete." | Must |
| Admin | "I review the widget's permission scopes and approve them for the Finance tenant." | Must |
| Auditor | "I export the widget's consent and provenance logs for the last 90 days." | Must |

---

## Functional Requirements  

### 1. Widget Variants  
| ID | Title | Backend Endpoint | Primary Vis | Critical Actions |
|----|-------|-----------------|-------------|------------------|
| champion-finder | Champion Finder | `GET /v1/champions` | Ranked list + network mini-graph | "Invite Champion", "View Profile" |
| community-pulse | Community Pulse | `GET /v1/community-pulse` | Time-series sentiment chart | "Drill-down", "Export CSV" |
| innovation-spread | Innovation Spread | `GET /v1/innovation-spread/{ideaId}` | Diffusion network graph | "Highlight Hotspots" |
| learning-catalyst | Learning Catalyst | `POST /v1/learning-catalyst/recommend` | Card carousel | "Mark Complete", "Dismiss" |

### 2. Registration & Lifecycle  
* Each widget calls  
  ```js
  registerWidget({
    id, title, version, permissions, dataSources, renderFn
  })
  ```  
  during `window.pluginInit`.
* Widget sandbox = `<iframe sandbox="allow-scripts allow-same-origin">`.

### 3. Data Visualisation & Interaction  
* Use dashboard shared React component library (**Design System v2**).  
* All charts support dark/light mode, keyboard navigation, and WCAG 2.1 AA.

### 4. Consent Management  
* First activation triggers shell-level **Consent Dialog** listing `permissions[]`.  
* Consent stored per-instance in `WidgetInstance.userConsents`.  
* Revocation hides widget and blocks API calls with 403.

### 5. Provenance Overlay  
* `ⓘ` icon toggles side panel showing:  
  * dataSources[] from `WidgetDefinition`,  
  * backend endpoint + timestamp,  
  * model version (e.g., `sentiment-model@2025-03`).  
* Overlay content signed with widget's `codeSignature` for tamper-proofing.

### 6. Intent & Orchestration  
* All API calls go through **PluginOrchestrator** → Convener backend → post-processing.  
* Critical actions (`Invite Champion`, `Export CSV`, etc.) include `requiresHumanApproval = true`.

### 7. Error & Empty States  
* Show skeleton loader ≤ 300 ms.  
* If backend 503, display graceful degradation card with "Retry" + docs link.

---

## Non-Functional Requirements Compliance (Δ)  

The widget inherits **all** items in `global-nfr.md`; specific deltas are tracked here.

| Category | Compliance | Notes |
|----------|------------|-------|
| Security | ✅ | Sandbox, CSP `default-src 'self'; frame-ancestors 'none'`. |
| Telemetry | ✅ | Emits `WidgetRender`, `WidgetAction` events with correlationId. |
| Performance | ✅ | JS bundle ≤ 75 KB gzip, P95 render < 500 ms. |
| Accessibility | ✅ | Tested with axe-core; no critical violations. |
| Data Residency | ⚠️ none | All data pulled from tenant-scoped Convener DB—no cross-region transfer. |

---

## UX Flows (Textual)

1. **Add Widget**  
   User opens Widget Library → selects **Champion Finder** → clicks **Add** → shell inserts widget (position via `DashboardLayoutManager.GetAvailablePosition`).

2. **First-Run Consent**  
   Consent Dialog lists `read:employee-profile`, `read:comm-metrics` → user **Accepts** → consent record saved.

3. **Normal Interaction**  
   User searches "MLOps" → widget shows ranked experts → user clicks **Invite Champion** → Orchestrator enforces HITL → action approved.

4. **Provenance View**  
   User clicks `ⓘ` → side panel reveals provenance trail (HRIS v2.1, Slack sentiment API call, LLM model hash).

5. **Permission Revocation**  
   Settings → Permissions → toggle off **read:comm-metrics** → dashboard auto-disables Community Pulse widget, logs event.

---

## Risks & Mitigations  

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Backend latency spikes | Poor UX | Client-side cache + loading indicators |
| Consent fatigue | Users skip reading | Provide concise plain-language summaries; group permissions |
| Graph overload on large tenants | Slow rendering | Progressive graph rendering + cluster collapse |

---

## Milestones & Timeline  

| Phase | Duration | Deliverables | Acceptance Criteria |
|-------|----------|--------------|---------------------|
| **M1** | 1 wk | Base widget framework + skeleton | Widget renders static lorem content |
| **M2** | 2 wks | Champion Finder + Community Pulse MVP | Live data, consent flow, provenance overlay |
| **M3** | 2 wks | Innovation Spread & Learning Catalyst MVP | Graph vis; card carousel |
| **M4** | 1 wk | HITL dialogs + error states | All actions routed via Orchestrator; 0 axe-core criticals |
| **GA** | — | Production rollout | Meets NFR performance & SLA for 30 days |

---

## API & Interface Links  
* **Convener Backend PRD:** [`convener-backend.md`](convener-backend.md)  
* **Bundled OpenAPI:** [`../openapi.yaml`](../openapi.yaml)  
* **Widget Definition Schema:** `src/UILayer/Models/WidgetDefinition.cs`

---

_Last updated: 2025-07-01_  
_Maintainer: Convener UI Guild_

------------------------------------------------------------------------

## [Integrated from 03-convener.widget-content.PARTIAL.md on 2025-07-03]

(See original partial for any additional unique user stories, requirements, or technical details not already present above. This section is for traceability and completeness.)
