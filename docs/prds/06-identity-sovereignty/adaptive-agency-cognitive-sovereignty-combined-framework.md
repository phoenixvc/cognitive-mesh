# Adaptive Agency–Cognitive Sovereignty PRD (Combined Mesh Framework)

### TL;DR

This document establishes a mesh-wide adaptive agency system that
dynamically tunes between autonomous agentic workflows (for efficiency)
and cognitive sovereignty modes (for user authorship and control),
according to context, task intent, and risk. Both backend and mesh
widgets deliver synchronized, auditable state routing and user
experiences. The system empowers organizations to address high-stakes,
complex problems while preserving human intentionality and transparency
in creative and knowledge work.

------------------------------------------------------------------------

## Goals

### Business Goals

- Accelerate adoption of mesh agent infrastructure by delivering
  efficient, auditable automation in enterprise environments.

- Ensure compliance and reduce risk by embedding real-time governance,
  audit, and override pathways for all agentic activity.

- Enhance user trust and engagement by providing clear authorship
  transparency and agent provenance throughout the system.

- Enable flexible configuration and rapid adaptation of workflow
  autonomy policies, tailored to organizational and regulatory needs.

### User Goals

- Allow users to shift seamlessly between “problem-stating” (full
  agentic automation) and “co-authorship” (cognitive sovereignty) modes,
  based on task and context.

- Provide users with real-time visibility into agent actions, authorship
  trails, and the ability to override or reclaim control at any moment.

- Minimize friction in routine, high-confidence domains while
  maintaining safeguards and transparency for creative or sensitive
  work.

- Empower users to define or adjust their autonomy preferences and
  receive proactive notifications of agent/system decisions that impact
  them.

### Non-Goals

- The system will not enable fully autonomous agent operations in
  regulated or critically audited domains without active human review.

- Will not support black-box agent decision-making without provenance or
  authorship trails.

- Out of scope: Extensions to non-mesh platforms or deep integrations
  with external agent orchestration frameworks not covered by OpenAPI
  contracts outlined herein.

------------------------------------------------------------------------

## User Stories

### Knowledge Worker Persona

- As a knowledge worker, I want to see whether I am authoring or the
  system is agentically solving, so that I have confidence in outcomes
  and can correct course when needed.

- As a knowledge worker, I want to reclaim control and retrofit
  decisions when I sense an agent has acted beyond its intended context,
  so that my work remains trustworthy.

### Business Manager Persona

- As a manager, I want to configure organization-level defaults for
  adaptive agency modes, so that high-stakes workflows require more
  human authorship and low-risk tasks run autonomously.

- As a manager, I want an audit view of all agentic actions,
  correlations, and overrides, so that I meet compliance and reporting
  obligations.

### System Administrator Persona

- As a system admin, I want to be notified of deprecated policies or
  breaking mode changes, so that migration and user transition are
  seamless.

------------------------------------------------------------------------

## Functional Requirements

- **Contextual Agency Router** (Priority: Must)

  - Dynamically selects autonomy, agency, and authority for mesh flows
    based on CIA/CSI, policy, user/org config, or workflow type.

  - Provides override pathways (manual/automated).

- **Agency Mode/Override Ports** (Priority: Must)

  - Mesh-wide APIs for querying, mutating, and recording agentic mode
    (autonomous, hybrid, sovereignty-first) for any workflow.

- **Cognitive Impact Assessment (CIA) & Sovereignty Index (CSI) Ports**
  (Priority: Must)

  - Compute and expose indices per task/workflow, driving real-time
    routing and audit.

- **Consent, Audit, and Notification Ports** (Priority: Must)

  - Enforce user/org consent flows, log all agentic actions and
    authorship decisions, and generate proactive notifications.

- **Provenance & Escalation Flows** (Priority: Must)

  - Real-time authorship trails, escalation trees for policy or a11y
    violations, manual rollbacks, circuit-break conditions.

- **Adaptive Mesh Widget Interfaces** (Priority: Must)

  - Surfaces visual agency/authorship state to user, offers “take
    control”/override affordances, exposes provenance, and synchronizes
    with backend.

- **Policy Table Configuration and Mini-DSL** (Priority: Should)

  - Language and interface for mapping CIA/CSI/output signals to routing
    policy, with fallback and notification rules.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Users access mesh platform via authenticated enterprise
  login—first-time users see a brief orientation banner explaining
  "adaptive agency" and the ability to choose between agentic and
  authorship modes.

- An onboarding tutorial highlights how to view/control agency modes and
  access provenance/override features.

**Core Experience**

- **Step 1:** User initiates a mesh workflow (e.g., submit a
  problem/task, open a dashboard, or request a solution).

  - UI shows current agency mode (agentic, hybrid, sovereignty) with a
    banner and clear iconography.

  - User can immediately see and adjust recommended mode, or let the
    Contextual Router decide.

- **Step 2:** Based on CIA/CSI and policy, system auto-routes to agentic
  or co-authorship pathway.

  - In agentic mode: User submits problem, system solves autonomously;
    shows stepwise trace and outcomes.

  - In co-authorship mode: User receives detailed agent suggestions, can
    accept, reject, or edit inline.

- **Step 3:** Agent actions, decisions, and provenance chains are always
  one click away via an “Audit/Provenance” panel.

  - Banners highlight policy-driven interventions, escalation,
    rollbacks, or consent checkpoints if thresholds/violations occur.

  - Errors, access issues, or policy violations trigger overlays per UX
    pattern library, with links to next actions.

- **Step 4:** Notifications and escalation overlays guide the user if
  agentic actions require manual review, fail compliance, or drift from
  intended policy.

- **Step 5:** All user overrides, authorship mode changes, and
  escalations are logged, visible in audit, and can trigger admin
  notifications.

**Advanced Features & Edge Cases**

- If widget or API detects schema drift or version mismatch, user
  receives a real-time overlay with instructions and rollback pathway.

- Accessibility violations result in auto-disable of agentic functions,
  notification banner, and telemetry ping for recovery/investigation.

- Admins can set global or context-specific overrides from the registry
  or via bulk-policy update.

**UI/UX Highlights**

- Color-coded banners indicate agency mode at all times.

- Unified overlay/interaction patterns for error, consent, escalation,
  and override flows.

- All author and agent operations keyboard-navigable and screen-reader
  friendly.

- Provenance, notification, and rollback affordances always top-level;
  zero buried actions.

- Inline diagram thumbnails for key flows, with links to
  /docs/diagrams/adaptive-agency-mesh/.

------------------------------------------------------------------------

## Narrative

At a leading enterprise, teams rely on a mesh platform that enables both
AI-automated power and human-authorship for their mission-critical work.
When a data science manager faces a complex challenge, she simply states
her goal in the mesh widget. Instantly, the system recommends an
autonomous agentic mode—highlighted by a clear blue banner. Because
she’s in a compliance-sensitive domain, the system’s CIA and CSI checks
route to “hybrid mode,” inviting her to review critical agent
suggestions as they happen.

She appreciates seeing a detailed audit trail of every agent’s action,
and the authorship overlay assures her that any override is one click
away. When a rare escalation occurs—an agent attempts an out-of-scope
decision—the system disables further action, alerts the manager, and
logs the event for follow-up. The mesh’s adaptive design means she and
her team always know who’s in control, and leadership has the governance
and audit visibility to meet every regulatory demand.

With the Adaptive Agency–Cognitive Sovereignty Mesh, the enterprise
drives unprecedented efficiency and safety—empowering teams to solve
faster, without ever surrendering control or clarity.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- % of workflows with explicit agency mode acknowledged or adjusted by
  user

- User NPS for authorship transparency and ability to override agentic
  actions

- Average time-to-intervention or rollback for agentic
  errors/escalations

### Business Metrics

- Reduction in policy/compliance incidents attributable to uncontrolled
  AI autonomy

- Acceleration in time-to-solution or task throughput on mesh-enabled
  workflows

- Adoption rate of adaptive agency feature set across business units

### Technical Metrics

- P95 port/API response time under full load

- Error rate or drift events detected and auto-handled vs. total
  workflows

- % automation of compliance audit log traces per workflow

### Tracking Plan

- Track all agency mode changes and overrides (user & system-initiated)

- Log every provenance/user action in audit trail

- Event telemetry: workflow launches, escalations, consent checkpoints,
  errors, version mismatches, a11y failures, notification delivery

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Contextual Agency Router: Dynamic AAA routing engine, pluggable policy
  DSL, resilience logic

- Mesh Ports & Adapters: API contracts for AgencyMode, Override,
  CIA/CSI, Consent, Telemetry, Notification, WidgetLifecycle

- Provenance Engine: Audit storage, indexing, and retrieval system with
  correlationID linkage

### Integration Points

- Enterprise SSO/user registry

- Organizational policy registry and mesh configuration systems

- 3rd-party compliance auditing and notification services

### Data Storage & Privacy

- All agentic, authorship, and override events stored with provenance
  metadata; full auditability

- Data residency, retention, and consent flows inherit SOC2/GDPR
  primitives

- Privacy policies enforce clear separation of agent and human data
  states

### Scalability & Performance

- Scalable mesh event bus for high-throughput agent/action tracking

- API/microservice contracts built for 10k+ concurrent workflows per org

- Resilient error handling: circuit-break, retry, queue-to-batch on any
  port/adaptor

### Potential Challenges

- Ensuring minimal friction for advanced authorship/override features in
  fast-moving workflows

- Real-time audit performance under high agentic task volumes

- Secure policy/config changes without introducing drift or escalating
  risk/untraceable agent actions

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- Medium: 2–4 weeks per major module (backend router, mesh widget,
  policy/audit engine, UX patterns)

- Full MVP delivery in sequential sprints over 6–8 weeks

### Team Size & Composition

- Small Team: 3–5 total people (1 Product, 2–3 Engineering, 1 Design/UX)

### Suggested Phases

**Phase 1: Agency Router & Core API Setup (2 Weeks)**

- Key Deliverables: AgencyModePort, CIA/CSI computation, policy DSL,
  test harness

- Dependencies: Access to mesh infra and baseline OpenAPI schemas

**Phase 2: Adaptive Mesh Widget & Pattern Library (2 Weeks)**

- Key Deliverables: Widget panel, banner/overlay/consent UI, provenance
  panel, a11y enforcement

- Dependencies: Backend agency router API completed

**Phase 3: Policy, Audit, & Notification (2 Weeks)**

- Key Deliverables: Audit log engine, consumer notification registry,
  admin/override flows

- Dependencies: Phase 1–2 code integration

**Phase 4: Combined CI/CD Testing & Governance (2 Weeks)**

- Key Deliverables: Complete test matrix, CI gating, versioning and
  deprecation hooks, migration docs

- Dependencies: All prior phases

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh components** rather than introducing an entirely new stack. The table and notes below map new engines, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| Layer | New / Updated Component | Action Required | Integration Point(s) |
|---|---|---|---|
| **ReasoningLayer** | `ContextualAdaptiveAgencyEngine` (NEW) | • Add pure-domain engine (`src/ReasoningLayer/AgencyRouter/Engines/ContextualAdaptiveAgencyEngine.cs`).<br>• Expose via **new port** `IAgencyRouterPort`. | Consumed by BusinessApplications API adapter. |
| **BusinessApplications** | `AgencyRouterController` (NEW) | • Create a new controller to route to the new ports.<br>• Add OpenAPI paths: `/v1/agency/route`, `/v1/agency/override`. | OpenAPI file `docs/openapi.yaml` – add/modify schemas & operations. |
| | `IConsentPort` (UPDATE) | • Extend to handle new consent types for adaptive agency decisions. | `src/BusinessApplications/ConvenerServices/Ports/IConsentPort.cs`. |
| **FoundationLayer** | `AuditLoggingAdapter` (UPDATE) | • Add new event types: `AgencyModeRouted`, `AgencyModeOverridden`, `PolicyApplied`.<br>• Ensure searchable ≤ 5 min SLA. | Existing audit DB + event ingestion queue. |
| | `NotificationAdapter` (UPDATE) | • Broadcast agency mode changes and override confirmations to the widget bus. | Mesh event bus (`Topic: adaptive-agency-events`). |
| | `PolicyRepository` (NEW) | • Implement a new repository for persisting `PolicyConfiguration` entities. | New EF Core configuration and repository in `src/FoundationLayer/ConvenerData/`. |
| **AgencyLayer** | `MultiAgentOrchestrationEngine` (UPDATE) | • Update to consume decisions from the `ContextualAdaptiveAgencyEngine`.<br>• The engine will now receive the `AgencyModeDecision` and execute tasks accordingly. | `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs`. |
| **UILayer** | `AgencyWidgets` (NEW) | • Create new React components for the `Agency Mode Banner`, `AAA Control Center`, and `Sovereignty Status Overlay` (`src/UILayer/AgencyWidgets/Panels/`).<br>• Register each via `IWidgetRegistry.RegisterWidgetAsync()`. | • `IWidgetRegistry` (`src/UILayer/Core/WidgetRegistry.cs`).<br>• `DashboardLayoutService` (`src/UILayer/Services/DashboardLayoutService.cs`). |
| | `IAgencyWidgetAdapters` (UPDATE) | • Extend the existing adapters to include methods for calling the new backend endpoints and handling new event types. | `src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`. |

### Required OpenAPI Specification Updates

1.  **Paths added:**
    *   `POST /v1/agency/route`
    *   `POST /v1/agency/override`
    *   `GET /v1/agency/policy/{tenantId}`
    *   `PUT /v1/agency/policy`
    *   `GET /v1/agency/introspection/{taskId}`
2.  **Schemas added:** `TaskContext`, `AgencyModeDecision`, `OverrideRequest`, `PolicyConfiguration`, `RoutingRule`.
3.  **ErrorEnvelope** reused – no change.
4.  **Security Schemes** for policy management endpoints must include an `Admin` scope.

### Summary of File-Level Changes

*   **ReasoningLayer:**
    *   `src/ReasoningLayer/AgencyRouter/` (new directory)
    *   `Engines/ContextualAdaptiveAgencyEngine.cs` (new)
    *   `Ports/IAgencyRouterPort.cs` (new interface)

*   **BusinessApplications:**
    *   `src/BusinessApplications/AgencyRouter/` (new directory)
    *   `Controllers/AgencyRouterController.cs` (new)
    *   `ConvenerServices/Ports/IConsentPort.cs` (update)
    *   OpenAPI YAML – add paths/schemas.

*   **FoundationLayer:**
    *   `Infrastructure/Repositories/PolicyRepository.cs` (new)
    *   `ConvenerData/Entities/PolicyConfiguration.cs` (new entity)
    *   `AuditLoggingAdapter.cs` – append new event types.
    *   `NotificationAdapter.cs` – add new message kinds & topic.

*   **AgencyLayer:**
    *   `MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs` (update)

*   **UILayer:**
    *   `src/UILayer/AgencyWidgets/Panels/AgencyModeBanner.tsx` (new)
    *   `src/UILayer/AgencyWidgets/Panels/AAAControlCenter.tsx` (new)
    *   `src/UILayer/AgencyWidgets/Panels/SovereigntyStatusOverlay.tsx` (new)
    *   `Adapters/IAgencyWidgetAdapters.cs` (or `.ts` equivalent): Update to include new methods.

No other layers require structural change; global NFR inheritance and existing DR/observability patterns remain valid.

------------------------------------------------------------------------

## Foundation: Agency–Sovereignty Spectrum

<table style="min-width: 100px">
<tbody>
<tr>
<th></th>
<th><p>Agentic Autonomy</p></th>
<th><p>Hybrid Mode</p></th>
<th><p>Cognitive Sovereignty</p></th>
</tr>
&#10;<tr>
<td><p><strong>User Input</strong></p></td>
<td><p>State problem</p></td>
<td><p>Review/confirm critical</p></td>
<td><p>Author/co-author solution</p></td>
</tr>
<tr>
<td><p><strong>System Action</strong></p></td>
<td><p>Full agent decision</p></td>
<td><p>Agent prompts; confirmation</p></td>
<td><p>Human-driven; agent suggestions</p></td>
</tr>
<tr>
<td><p><strong>Authorship Trail</strong></p></td>
<td><p>Full agent</p></td>
<td><p>Mixed</p></td>
<td><p>Fully human, agent transparent</p></td>
</tr>
<tr>
<td><p><strong>Risk Level</strong></p></td>
<td><p>Low/moderate</p></td>
<td><p>Moderate/high</p></td>
<td><p>High/creative/compliant</p></td>
</tr>
</tbody>
</table>

*Visual: Spectrum illustration showing gradient from "state my goal" to
"full authorship/override."*

------------------------------------------------------------------------

## Policy Table Examples

Example Policies:

<table style="min-width: 125px">
<tbody>
<tr>
<th><p>CIA</p></th>
<th><p>CSI</p></th>
<th><p>Workflow Type</p></th>
<th><p>Default Mode</p></th>
<th><p>Escalation Trigger</p></th>
</tr>
&#10;<tr>
<td><p>&gt;7</p></td>
<td><p>&lt;0.3</p></td>
<td><p>financial</p></td>
<td><p>Hybrid</p></td>
<td><p>Consent checkpoint, notify</p></td>
</tr>
<tr>
<td><p>≤5</p></td>
<td><p>—</p></td>
<td><p>creative</p></td>
<td><p>Sovereignty</p></td>
<td><p>Overlay, explain agent limits</p></td>
</tr>
<tr>
<td><p>≤3</p></td>
<td><p>&gt;0.5</p></td>
<td><p>routine</p></td>
<td><p>Agentic</p></td>
<td><p>Skip consent; log audit only</p></td>
</tr>
</tbody>
</table>

*All decision/routing tables are visible to users and configurable by org
policy.*

------------------------------------------------------------------------

## Combined Test Matrix & CI/CD Gates

<table style="min-width: 175px">
<tbody>
<tr>
<th><p>Scenario</p></th>
<th><p>AgencyMode</p></th>
<th><p>Override</p></th>
<th><p>Consent</p></th>
<th><p>Widget</p></th>
<th><p>Telemetry</p></th>
<th><p>Notification</p></th>
</tr>
&#10;<tr>
<td><p>Normal</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Error/Drift</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Offline/Fallback</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Escalation</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Manual Review</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Policy Migration</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
<tr>
<td><p>Authorship Restore</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
<td><p>✔</p></td>
</tr>
</tbody>
</table>

*Release only occurs if all scenarios pass for every port and widget in
CI.*

------------------------------------------------------------------------

### Error Envelope Appendix

**Error Schema:**

- error_code: Systematic, unique error identifier

- message: User-facing and developer-facing summary

- correlationID: Event and audit correlation for tracing

- details (optional): Extended debug or remediation data

------------------------------------------------------------------------

### End of Document
