---
Module: CognitiveSandwichBackend
Primary Personas: Knowledge Workers, Application Admins, Compliance Auditors
Core Value Proposition: Backend for configurable cognitive sandwich process orchestration
Priority: P1
License Tier: Professional
Platform Layers: Business Applications, Metacognitive, Reasoning
Main Integration Points: Cognitive processes, Audit systems, Research platforms
---

# Cognitive Sandwich Backend Architecture PRD (Hexagonal, Mesh Layered, Configurable, Feedback-Looped)

### TL;DR

The backend enables a fully configurable, feedback-driven Cognitive Sandwich process (3–7 phases), binding seamlessly with Cognitive Sovereignty, Agentic AI, and Cognitive Mesh layers. Pre- and post-condition enforcement, feedback/step-back logic, and cognitive debt monitoring are first-class citizens. Deep auditability and research hooks underpin compliance, analytics, and empirical pilots.

------------------------------------------------------------------------

## Goals

### Business Goals

- Ensure robust, auditable human-AI collaboration by enforcing critical human decision checkpoints and systematic validation at every organizational scale.
- Reduce risk of cognitive dependency and AI error propagation by integrating cognitive debt monitoring and feedback loops at every phase.
- Provide configurable process flows per organization/policy, adaptable from individual work to enterprise roll-out, with governance and compliance built in.
- Support data-driven research and continuous improvement via high-fidelity analytics, event trails, and pilot study instrumentation.

### User Goals

- Allow users and teams to tailor and adjust the cognitive process (phases, conditions, rewinds) to fit context and domain.
- Enable seamless recovery from mistakes with step-back/rewind control and transparent history, maintaining user confidence and accountability.
- Surface actionable rationale and validation requirements at every decision point, minimizing friction without sacrificing rigor.
- Provide deep visibility and real-time sovereignty/cognitive engagement metrics to inform trust and performance.

### Non-Goals

- Does not define or implement user-facing UI/widget components (covered in the widget PRD).
- Does not prescribe specific human or AI algorithms for phase logic—focuses on orchestration, enforcement, compliance, and integration.
- Not responsible for external data management or third-party solution-market integrations outside of described API endpoints.

------------------------------------------------------------------------

## User Stories

**Persona: Knowledge Worker**

- As a knowledge worker, I want to request a tailored cognitive process (e.g., 5 phases, certain preconditions), so that my decision workflow meets organizational and regulatory needs.
- As a knowledge worker, I want to pause or step back on any phase if an error is found, so that my critical thinking leads, not the system's momentum.
- As a knowledge worker, I want clear feedback if preconditions or validations are missing, so I can resolve issues before moving forward.

**Persona: Application Admin**

- As an admin, I want to configure organization-wide phase templates and feedback permissions, so that enterprise and compliance requirements are enforced without exception.
- As an admin, I want phase transition and feedback events deeply logged, so our audits and research are future-proof.

**Persona: Compliance Auditor / Researcher**

- As a compliance auditor, I want every phase, transition, rationale, and cognitive metric captured and retrievable, so audit trails support both investigation and continuous improvement.
- As a researcher, I want experimental hooks into all process metrics and transitions, so pilot studies and meta-analyses are streamlined.

------------------------------------------------------------------------

## Functional Requirements

- **Phase Configuration & Governance (Priority: Must)**

  - PhaseManagerPort:
    - GET/POST/PATCH /v1/sandwich/phases for listing, initializing, or reorganizing phases (configurable 3–7, labeled per org context).
    - Enable/disable phases at runtime (enforced with contract).
    - Configure/retrieve phase descriptions, required pre/postconditions, and permissions.
    - Given phase config changes, when dependent processes exist, then block or batch migration as required.
  - OpenAPI JSON Pointer: `/spec/sandwich.yaml#/paths/v1sandwich~1phases`

- **Phase Transition, Pre/Postcondition Enforcement (Priority: Must)**

  - PhaseTransitionPort:
    - POST /v1/sandwich/phase/{n}/transition with validation of all pre- and postconditions per config/schema.
    - Errors or unmet conditions must block transition, surface rationale, and log event (see audit).
  - ConditionAdapterPort:
    - GET /v1/sandwich/phase/{n}/pre and /post—returns current status, descriptive rationale, and any required resolution.
    - Explicit response envelope: { phaseId, condition, met, requirement, rationale, timestamp }
  - OpenAPI JSON Pointer: `/spec/sandwich.yaml#/paths/v1sandwich1phase1{n}~1transition`

- **Feedback/Step-Back/Rewind Logic (Priority: Must)**

  - StepBackPort:
    - POST /v1/sandwich/stepback/{phaseId}
    - Roll back to prior phase, log user/system-provided rationale and current state, enforce pre/postcondition checks of the pending state.
    - Given process already step-backed in current session, when user attempts redundant step-back, then surface contract-specified block/warning.
    - All step-back events versioned for research/meta-analytics.
  - OpenAPI JSON Pointer: `/spec/sandwich.yaml#/paths/v1sandwich1stepback1{phaseId}`

- **Cognitive Debt & Engagement Monitoring (Priority: Must)**

  - CognitiveDebtMonitorPort:
    - GET/POST at every phase entry/exit to assess and record engagement, cognitive baseline shift, risk of dependency, and neural connectivity (per MIT research integration).
    - Contract: returns metrics on engagement, critical thinking, dependency, with recommended actions if thresholds breached.

- **Audit Logging & Analytics (Priority: Must)**

  - AuditLogPort and AnalyticsEventPort:
    - Log all phase entries/exits, transitions, feedback/step-back events, error/contract failures, rationale, cognitive metric deltas, and compliance/sovereignty snapshots.
    - Events keyed by session, user, phaseId, rationale.
    - SLA: ingest and persist within <1s; batch export available for research and compliance audits.

- **Multi-Agent, Sovereignty, & Orchestration Integration (Priority: Must)**

  - SandwichOrchestrator:
    - Enables handoff to mesh agent orchestrator at configured phases; supports multi-agent entry/exit and policy overlays per step.
    - All agent/sovereignty handoffs governed by CognitiveSovereigntyBridge:
      - Phase entry triggers CSI/sovereignty check, overlays org, team, and cultural constraints per Capgemini Agentic AI or org policy.
      - Optionally captures user intervention/autonomy preference (manual/hybrid/auto-assist), sent to frontend when required.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Users (via API clients or mesh orchestrators) access the POST /v1/sandwich/phases endpoint to retrieve or initialize the process template, with labels and permission metadata.
- System returns current configuration: phases 1–N, each with descriptions, input/output schemas, enabled feedback/rewind points, and pre/postconditions.
- On first process initiation, system logs onboarding event and, if configured, sends context-driven tutorial pointers to widget-layer clients.

**Core Experience**

- Step 1: Initiate a sandwich process with user, org, or project context.
  - System validates process config, enabled phases, and returns phase 1 overhead.
  - Precondition contract is checked (e.g., necessary domain knowledge confirmed).
- Step 2: User (via API or mesh UI) completes or advances a phase.
  - Calls /v1/sandwich/phase/{n}/transition with payload, rationale, and context.
  - System enforces all preconditions (may call out to 3rd-party/compliance if set).
  - If no errors, logs phase entry, context, and cognitive baseline.
- Step 3: On transition request, the postcondition contract validates the result.
  - If issue found (missing validation, engagement too low), step is blocked, error recorded, rationale surfaced.
  - User/system may be prompted to step back or readdress requirements.
- Step 4: Feedback or step-back can be called at most transition points (policy-driven).
  - Triggers re-entry into previous phase, logs rationale, state diff, and engagement change.
- Step 5: Multi-agent or sovereignty overlays are injected at tagged phases.
  - CognitiveSovereigntyBridge validates agentic mode, overlays policy/cultural configs, logs all sovereignty interventions.
- Step 6: At every phase entry/exit, cognitive debt metrics are assessed and added to session log.

**Advanced Features & Edge Cases**

- Blind-spot detection: If a phase is (intentionally or accidentally) skipped, system blocks next transition and logs alert, requiring administrative override.
- Dynamic config update: Org can re-sequence or disable phases live; changes logged as config mutations, batch migration triggered.
- Compliance audit: On external request or regulatory audit flag, full session logs are retrieved and exported, with redaction as required by org policy.
- Step-back limits: Prevent infinite feedback loops by max step-back count per session (configurable).
- Deep research hooks: Phase, event, feedback, and engagement logs can be optionally mirrored to research DB for pilot studies.

**UI/UX Highlights**

- All errors, blocks, rationale requirements, and transition verdicts are returned in a standardized error envelope ({ error_code, message, correlationID, phaseId }).
- Phase contract errors include actionable next steps for user/system/ops.
- Accessibility-first: All feedback, rationale, and engagement prompts include machine-readable labels and multi-language support in error/display fields.
- Pre/postconditions and step-back permissibility surfaced in contract for UI display prior to user action.

------------------------------------------------------------------------

## Narrative

A leading professional services firm faces a challenge: how do you harness AI's computational power without losing human critical leadership? Their teams adopt the Cognitive Sandwich process—now orchestrated by the new backend. A consultant sets up a custom 5-phase flow, ensuring team members start with deep problem decomposition. When AI proposes a dazzling solution, the backend enforces a human validation phase, requiring logic checks and bias detection before anyone can move forward. Partway through, an anomaly is detected—one validator hits the step-back button. Instantly, the process rewinds, preconditions are resurfaced, the phase is revisited, and all actions/reasons are logged for audit and improvement. Compliance teams track every rationale, cognitive engagement stat, and sovereignty handoff in the audit trail. Ultimately the team's final decision is not just AI-assisted—it's proven, human-led, and trustable. Leadership has metrics for cognitive health, engagement, and process effectiveness; risk is reduced, and organizational learning flourishes.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- % of human validation phases completed without error or override
- Number of step-back/rewind events initiated and successfully resolved
- User satisfaction with process configurability and error recovery

### Business Metrics

- Reduction in strategic/policy errors traceable to missing or incomplete validations
- Consistency of validated decision-making across distributed teams
- Measurable drop in cognitive dependency or over-reliance on AI, by engagement scores

### Technical Metrics

- P99 phase transition time (<200ms target, <500ms with audit/research log)
- SLA adherence on audit event ingestion (<1s)
- n–1 version support for all endpoints

### Tracking Plan

- Session start/end, phase config events, and organizational policy changes
- All phase entries, exits, transitions, and rationale logs
- Pre/postcondition check invocations, failures, and success outcomes
- Step-back, feedback, and recovery events with rationale and state delta
- All agent/sovereignty handoffs, cognitive metric assessments, and related anomalies

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Stateless, horizontally scalable sandwiched phase adapters and orchestrator
- Modular open API with strict contract enforcement, supporting runtime phase add/remove/patch/configure
- Deduplicated, append-only audit and research log store with live query
- Multi-tenant data isolation, secure access with SSO compatibility

### Integration Points

- Cognitive Mesh AgencyLayer, ReasoningLayer, BusinessApps for phase transitions, multi-agent handoff, sovereignty overlays
- Cognitive Sovereignty: SovereigntyManager, CognitiveSovereigntyBridge for phase-specific mode and policy enforcement
- Cognitive Debt Monitor and research DB integration
- Audit/Compliance: event bus, analytic DB, and redaction/export subsystems

### Data Storage & Privacy

- Strong session/user/phase/event keying for all audit logs and metrics
- Contract-based redaction on export; 100% end-to-end traceability
- Regulatory mode (SOC2, GDPR, AI Act) with phase-defined retention policies and compliance attestations
- Raw cognitive metrics and rationale export gated by explicit research flag and user org consent

### Scalability & Performance

- Designed for 10,000+ concurrent active sandwich sessions; 100+ orgs; low-latency feedback loops and high-throughput batch event logging
- Backpressure and offline/outage queuing on all phase and transition paths

### Potential Challenges

- Race conditions on concurrent config updates in org-wide live sessions
- Ensuring clarity for pre/postcondition failures and feedback at scale
- Maintaining fast audit ingestion under heavy feedback/recovery loads
- Governance of phase changes with in-flight/running sessions

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

Medium: 3–5 weeks

### Team Size & Composition

Small Team: 2–3 total people

- 1 Product Owner
- 1–2 Backend Engineers (with partial Research/Compliance SME support as needed)

### Suggested Phases

**Phase 1: Core Phase Process & Feedback Engine (1 Week)**

- Key Deliverables: PhaseManagerPort, StepBackPort, config endpoints, basic pre/postcondition enforcement.
- Dependencies: Existing mesh or orchestration infra.

**Phase 2: Debt Monitoring, Audit & Analytics (1 Week)**

- Key Deliverables: CognitiveDebtMonitorPort, full audit/analytic event ingestion and export, error envelope contracts.
- Dependencies: Basic phase engine online.

**Phase 3: Sovereignty & Multi-Agent Integration (1–1.5 Weeks)**

- Key Deliverables: CognitiveSovereigntyBridge enablement, multi-agent phase config, org/cultural policy overlays and handoff.
- Dependencies: Core process and audit engines operational.

**Phase 4: Research, Compliance, Test Harness, Governance (0.5–1.5 Weeks)**

- Key Deliverables: Research hooks, meta-event logging for pilot study, compliance snapshot/export, full test harness (phase × transition × edge), versioning, and deprecation controls.
- Dependencies: Previous phases released.

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh components** rather than introducing an entirely new stack. The table and notes below map new engines, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| Layer | New / Updated Component | Action Required | Integration Point(s) |
|---|---|---|---|
| **ReasoningLayer** | `SandwichOrchestrator` (NEW)<br>`CognitiveSovereigntyBridge` (NEW) | • Add pure-domain engines (`src/ReasoningLayer/CognitiveSandwich/`).<br>• Expose via **new ports**: `IPhaseManagerPort`, `IPhaseTransitionPort`, `IStepBackPort`. | • Consumed by BusinessApplications API adapter.<br>• Integrates with `ContextualAdaptiveAgencyEngine` for sovereignty checks. |
| **BusinessApplications** | `CognitiveSandwichController` (NEW) | • Create a new controller to route to the new ports.<br>• Add OpenAPI paths: `/v1/sandwich/phases`, `/v1/sandwich/phase/{n}/transition`, `/v1/sandwich/stepback/{phaseId}`. | OpenAPI file `docs/openapi.yaml` – add/modify schemas & operations. |
| **MetacognitiveLayer** | `CognitiveDebtMonitor` (NEW) | • Implement a new service to assess and record engagement and cognitive debt.<br>• Expose via **new port** `ICognitiveDebtMonitorPort`. | • Consumed by the `SandwichOrchestrator` at each phase transition.<br>• Integrates with existing `CommunityPulseService` for baseline metrics. |
| **FoundationLayer** | `AuditLoggingAdapter` (UPDATE) | • Add new event types: `PhaseTransitioned`, `StepBackInitiated`, `CognitiveDebtAssessed`, `PreconditionFailed`.<br>• Ensure searchable ≤ 1s SLA. | Existing audit DB + event ingestion queue. |
| | `NotificationAdapter` (UPDATE) | • Broadcast pre/postcondition failures and step-back confirmations to the widget bus. | Mesh event bus (`Topic: cognitive-sandwich-events`). |
| | `SandwichProcessRepository` (NEW) | • Implement a new repository for persisting the state of active `CognitiveSandwichProcess` instances. | • New EF Core configuration and repository in `src/FoundationLayer/ConvenerData/`. |
| **AgencyLayer** | `MultiAgentOrchestrationEngine` (UPDATE) | • Update to accept handoffs from the `SandwichOrchestrator` at configured phases.<br>• The engine will receive the task context and execute the multi-agent workflow. | `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs`. |

### Required OpenAPI Specification Updates

1.  **Paths added:**
    *   `POST /v1/sandwich/phases`
    *   `GET /v1/sandwich/phases`
    *   `PATCH /v1/sandwich/phases/{phaseId}`
    *   `POST /v1/sandwich/phase/{n}/transition`
    *   `POST /v1/sandwich/stepback/{phaseId}`
    *   `GET /v1/sandwich/phase/{n}/pre`
    *   `GET /v1/sandwich/phase/{n}/post`
    *   `POST /v1/sandwich/cognitive-debt`
2.  **Schemas added:** `CognitiveSandwichProcess`, `PhaseConfiguration`, `PhaseCondition`, `StepBackRequest`, `CognitiveDebtMetrics`.
3.  **ErrorEnvelope** reused – no change.
4.  **Security Schemes** for phase configuration endpoints must include an `Admin` scope.

### Summary of File-Level Changes

*   **ReasoningLayer:**
    *   `src/ReasoningLayer/CognitiveSandwich/` (new directory)
    *   `Engines/SandwichOrchestrator.cs` (new)
    *   `Engines/CognitiveSovereigntyBridge.cs` (new)
    *   `Ports/IPhaseManagerPort.cs`, `Ports/IPhaseTransitionPort.cs`, `Ports/IStepBackPort.cs` (new interfaces)

*   **BusinessApplications:**
    *   `src/BusinessApplications/CognitiveSandwich/` (new directory)
    *   `Controllers/CognitiveSandwichController.cs` (new)
    *   OpenAPI YAML – add paths/schemas.

*   **MetacognitiveLayer:**
    *   `src/MetacognitiveLayer/CognitiveDebt/` (new directory)
    *   `Engines/CognitiveDebtMonitor.cs` (new)
    *   `Ports/ICognitiveDebtMonitorPort.cs` (new interface)

*   **FoundationLayer:**
    *   `Infrastructure/Repositories/SandwichProcessRepository.cs` (new)
    *   `ConvenerData/Entities/CognitiveSandwichProcess.cs` (new entity)
    *   `AuditLoggingAdapter.cs` – append new event types.
    *   `NotificationAdapter.cs` – add new message kinds & topic.

*   **AgencyLayer:**
    *   `MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs` (update)

No other layers require structural change; global NFR inheritance and existing DR/observability patterns remain valid.

------------------------------------------------------------------------

## [Integrated from 08-cognitive-frameworks.backend-content.PARTIAL.md on 2025-07-03]

(See original partial for any additional unique user stories, requirements, or technical details not already present above. This section is for traceability and completeness.)
