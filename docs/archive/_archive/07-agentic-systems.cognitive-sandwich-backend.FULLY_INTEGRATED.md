# Cognitive Sandwich Backend Architecture PRD (Hexagonal, Mesh Layered, Configurable, Feedback-Looped)

### TL;DR

The backend enables a fully configurable, feedback-driven Cognitive
Sandwich process (3–7 phases), binding seamlessly with Cognitive
Sovereignty, Agentic AI, and Cognitive Mesh layers. Pre- and
postcondition enforcement, feedback/step-back logic, and cognitive debt
monitoring are first-class citizens. Deep auditability and research
hooks underpin compliance, analytics, and empirical pilots.

------------------------------------------------------------------------

## Goals

### Business Goals

- Ensure robust, auditable human-AI collaboration by enforcing critical
  human decision checkpoints and systematic validation at every
  organizational scale.

- Reduce risk of cognitive dependency and AI error propagation by
  integrating cognitive debt monitoring and feedback loops at every
  phase.

- Provide configurable process flows per organization/policy, adaptable
  from individual work to enterprise roll-out, with governance and
  compliance built in.

- Support data-driven research and continuous improvement via
  high-fidelity analytics, event trails, and pilot study
  instrumentation.

### User Goals

- Allow users and teams to tailor and adjust the cognitive process
  (phases, conditions, rewinds) to fit context and domain.

- Enable seamless recovery from mistakes with step-back/rewind control
  and transparent history, maintaining user confidence and
  accountability.

- Surface actionable rationale and validation requirements at every
  decision point, minimizing friction without sacrificing rigor.

- Provide deep visibility and real-time sovereignty/cognitive engagement
  metrics to inform trust and performance.

### Non-Goals

- Does not define or implement user-facing UI/widget components (covered
  in the widget PRD).

- Does not prescribe specific human or AI algorithms for phase
  logic—focuses on orchestration, enforcement, compliance, and
  integration.

- Not responsible for external data management or third-party
  solution-market integrations outside of described API endpoints.

------------------------------------------------------------------------

## User Stories

**Persona: Knowledge Worker**

- As a knowledge worker, I want to request a tailored cognitive process
  (e.g., 5 phases, certain preconditions), so that my decision workflow
  meets organizational and regulatory needs.

- As a knowledge worker, I want to pause or step back on any phase if an
  error is found, so that my critical thinking leads, not the system's
  momentum.

- As a knowledge worker, I want clear feedback if preconditions or
  validations are missing, so I can resolve issues before moving
  forward.

**Persona: Application Admin**

- As an admin, I want to configure organization-wide phase templates and
  feedback permissions, so that enterprise and compliance requirements
  are enforced without exception.

- As an admin, I want phase transition and feedback events deeply
  logged, so our audits and research are future-proof.

**Persona: Compliance Auditor / Researcher**

- As a compliance auditor, I want every phase, transition, rationale,
  and cognitive metric captured and retrievable, so audit trails support
  both investigation and continuous improvement.

- As a researcher, I want experimental hooks into all process metrics
  and transitions, so pilot studies and meta-analyses are streamlined.

------------------------------------------------------------------------

## Functional Requirements

- **Phase Configuration & Governance (Priority: Must)**

  - *PhaseManagerPort*:

    - GET/POST/PATCH /v1/sandwich/phases for listing, initializing, or
      reorganizing phases (configurable 3–7, labeled per org context).

    - Enable/disable phases at runtime (enforced with contract).

    - Configure/retrieve phase descriptions, required
      pre/postconditions, and permissions.

    - Given phase config changes, when dependent processes exist, then
      block or batch migration as required.

  - *OpenAPI JSON Pointer*:
    /spec/sandwich.yaml#/paths/v1sandwich~1phases

- **Phase Transition, Pre/Postcondition Enforcement (Priority: Must)**

  - *PhaseTransitionPort*:

    - POST /v1/sandwich/phase/{n}/transition with validation of all pre-
      and postconditions per config/schema.

    - Errors or unmet conditions must block transition, surface
      rationale, and log event (see audit).

  - *ConditionAdapterPort*:

    - GET /v1/sandwich/phase/{n}/pre and /post—returns current status,
      descriptive rationale, and any required resolution.

    - Explicit response envelope: { phaseId, condition, met,
      requirement, rationale, timestamp }

  - *OpenAPI JSON Pointer*:
    /spec/sandwich.yaml#/paths/v1sandwich1phase1{n}~1transition

- **Feedback/Step-Back/Rewind Logic (Priority: Must)**

  - *StepBackPort*:

    - POST /v1/sandwich/stepback/{phaseId}

    - Roll back to prior phase, log user/system-provided rationale and
      current state, enforce pre/postcondition checks of the pending
      state.

    - Given process already step-backed in current session, when user
      attempts redundant step-back, then surface contract-specified
      block/warning.

    - All step-back events versioned for research/meta-analytics.

  - *OpenAPI JSON Pointer*:
    /spec/sandwich.yaml#/paths/v1sandwich1stepback1{phaseId}

- **Cognitive Debt & Engagement Monitoring (Priority: Must)**

  - *CognitiveDebtMonitorPort*:

    - GET/POST at every phase entry/exit to assess and record
      engagement, cognitive baseline shift, risk of dependency, and
      neural connectivity (per MIT research integration).

    - Contract: returns metrics on engagement, critical thinking,
      dependency, with recommended actions if thresholds breached.

- **Audit Logging & Analytics (Priority: Must)**

  - *AuditLogPort* and *AnalyticsEventPort*:

    - Log all phase entries/exits, transitions, feedback/step-back
      events, error/contract failures, rationale, cognitive metric
      deltas, and compliance/sovereignty snapshots.

    - Events keyed by session, user, phaseId, rationale.

    - SLA: ingest and persist within \<1s; batch export available for
      research and compliance audits.

- **Multi-Agent, Sovereignty, & Orchestration Integration (Priority:
  Must)**

  - *SandwichOrchestrator*:

    - Enables handoff to mesh agent orchestrator at configured phases;
      supports multi-agent entry/exit and policy overlays per step.

    - All agent/sovereignty handoffs governed by
      CognitiveSovereigntyBridge:

      - Phase entry triggers CSI/sovereignty check, overlays org, team,
        and cultural constraints per Capgemini Agentic AI or org policy.

      - Optionally captures user intervention/autonomy preference
        (manual/hybrid/auto-assist), sent to frontend when required.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Users (via API clients or mesh orchestrators) access the POST
  /v1/sandwich/phases endpoint to retrieve or initialize the process
  template, with labels and permission metadata.

- System returns current configuration: phases 1–N, each with
  descriptions, input/output schemas, enabled feedback/rewind points,
  and pre/postconditions.

- On first process initiation, system logs onboarding event and, if
  configured, sends context-driven tutorial pointers to widget-layer
  clients.

**Core Experience**

- **Step 1:** Initiate a sandwich process with user, org, or project
  context.

  - System validates process config, enabled phases, and returns phase 1
    overhead.

  - Precondition contract is checked (e.g., necessary domain knowledge
    confirmed).

- **Step 2:** User (via API or mesh UI) completes or advances a phase.

  - Calls /v1/sandwich/phase/{n}/transition with payload, rationale, and
    context.

  - System enforces all preconditions (may call out to
    3rd-party/compliance if set).

  - If no errors, logs phase entry, context, and cognitive baseline.

- **Step 3:** On transition request, the postcondition contract
  validates the result.

  - If issue found (missing validation, engagement too low), step is
    blocked, error recorded, rationale surfaced.

  - User/system may be prompted to step back or readdress requirements.

- **Step 4:** Feedback or step-back can be called at most transition
  points (policy-driven).

  - Triggers re-entry into previous phase, logs rationale, state diff,
    and engagement change.

- **Step 5:** Multi-agent or sovereignty overlays are injected at tagged
  phases.

  - CognitiveSovereigntyBridge validates agentic mode, overlays
    policy/cultural configs, logs all sovereignty interventions.

- **Step 6:** At every phase entry/exit, cognitive debt metrics are
  assessed and added to session log.

**Advanced Features & Edge Cases**

- Blind-spot detection: If a phase is (intentionally or accidentally)
  skipped, system blocks next transition and logs alert, requiring
  administrative override.

- Dynamic config update: Org can re-sequence or disable phases live;
  changes logged as config mutations, batch migration triggered.

- Compliance audit: On external request or regulatory audit flag, full
  session logs are retrieved and exported, with redaction as required by
  org policy.

- Step-back limits: Prevent infinite feedback loops by max step-back
  count per session (configurable).

- Deep research hooks: Phase, event, feedback, and engagement logs can
  be optionally mirrored to research DB for pilot studies.

**UI/UX Highlights**

- All errors, blocks, rationale requirements, and transition verdicts
  are returned in a standardized error envelope ({ error_code, message,
  correlationID, phaseId }).

- Phase contract errors include actionable next steps for
  user/system/ops.

- Accessibility-first: All feedback, rationale, and engagement prompts
  include machine-readable labels and multi-language support in
  error/display fields.

- Pre/postconditions and step-back permissibility surfaced in contract
  for UI display prior to user action.

------------------------------------------------------------------------

## Narrative

A leading professional services firm faces a challenge: how do you
harness AI's computational power without losing human critical
leadership? Their teams adopt the Cognitive Sandwich process—now
orchestrated by the new backend. A consultant sets up a custom 5-phase
flow, ensuring team members start with deep problem decomposition. When
AI proposes a dazzling solution, the backend enforces a human validation
phase, requiring logic checks and bias detection before anyone can move
forward.  
Partway through, an anomaly is detected—one validator hits the step-back
button. Instantly, the process rewinds, preconditions are resurfaced,
the phase is revisited, and all actions/reasons are logged for audit and
improvement. Compliance teams track every rationale, cognitive
engagement stat, and sovereignty handoff in the audit trail.  
Ultimately the team's final decision is not just AI-assisted—it's
proven, human-led, and trustable. Leadership has metrics for cognitive
health, engagement, and process effectiveness; risk is reduced, and
organizational learning flourishes.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- % of human validation phases completed without error or override

- Number of step-back/rewind events initiated and successfully resolved

- User satisfaction with process configurability and error recovery

### Business Metrics

- Reduction in strategic/policy errors traceable to missing or
  incomplete validations

- Consistency of validated decision-making across distributed teams

- Measurable drop in cognitive dependency or over-reliance on AI, by
  engagement scores

### Technical Metrics

- P99 phase transition time (\<200ms target, \<500ms with audit/research
  log)

- SLA adherence on audit event ingestion (\<1s)

- n–1 version support for all endpoints

### Tracking Plan

- Session start/end, phase config events, and organizational policy
  changes

- All phase entries, exits, transitions, and rationale logs

- Pre/postcondition check invocations, failures, and success outcomes

- Step-back, feedback, and recovery events with rationale and state
  delta

- All agent/sovereignty handoffs, cognitive metric assessments, and
  related anomalies

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Stateless, horizontally scalable sandwiched phase adapters and
  orchestrator

- Modular open API with strict contract enforcement, supporting runtime
  phase add/remove/patch/configure

- Deduplicated, append-only audit and research log store with live query

- Multi-tenant data isolation, secure access with SSO compatibility

### Integration Points

- Cognitive Mesh AgencyLayer, ReasoningLayer, BusinessApps for phase
  transitions, multi-agent handoff, sovereignty overlays

- Cognitive Sovereignty: SovereigntyManager, CognitiveSovereigntyBridge
  for phase-specific mode and policy enforcement

- Cognitive Debt Monitor and research DB integration

- Audit/Compliance: event bus, analytic DB, and redaction/export
  subsystems

### Data Storage & Privacy

- Strong session/user/phase/event keying for all audit logs and metrics

- Contract-based redaction on export; 100% end-to-end traceability

- Regulatory mode (SOC2, GDPR, AI Act) with phase-defined retention
  policies and compliance attestations

- Raw cognitive metrics and rationale export gated by explicit research
  flag and user org consent

### Scalability & Performance

- Designed for 10,000+ concurrent active sandwich sessions; 100+ orgs;
  low-latency feedback loops and high-throughput batch event logging

- Backpressure and offline/outage queuing on all phase and transition
  paths

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

- 1–2 Backend Engineers (with partial Research/Compliance SME support as
  needed)

### Suggested Phases

**Phase 1: Core Phase Process & Feedback Engine (1 Week)**

- Key Deliverables: PhaseManagerPort, StepBackPort, config endpoints,
  basic pre/postcondition enforcement.

- Dependencies: Existing mesh or orchestration infra.

**Phase 2: Debt Monitoring, Audit & Analytics (1 Week)**

- Key Deliverables: CognitiveDebtMonitorPort, full audit/analytic event
  ingestion and export, error envelope contracts.

- Dependencies: Basic phase engine online.

**Phase 3: Sovereignty & Multi-Agent Integration (1–1.5 Weeks)**

- Key Deliverables: CognitiveSovereigntyBridge enablement, multi-agent
  phase config, org/cultural policy overlays and handoff.

- Dependencies: Core process and audit engines operational.

**Phase 4: Research, Compliance, Test Harness, Governance (0.5–1.5
Weeks)**

- Key Deliverables: Research hooks, meta-event logging for pilot study,
  compliance snapshot/export, full test harness (phase × transition ×
  edge), versioning, and deprecation controls.

- Dependencies: Previous phases released.

------------------------------------------------------------------------

## 1. Layer Map & Major Components

<table style="min-width: 75px">
<tbody>
<tr>
<th><p>Layer</p></th>
<th><p>Component</p></th>
<th><p>Description</p></th>
</tr>
&#10;<tr>
<td><p>Individual</p></td>
<td><p>CognitiveSandwichProcess, HumanAnalysisEngine,
ValidationFramework, DecisionAuthority, CognitiveDebtMonitor</p></td>
<td><p>Configurable, phase-driven cognitive process with engagement/debt
tracking.</p></td>
</tr>
<tr>
<td><p>ReasoningLayer</p></td>
<td><p>SandwichOrchestrator, CognitiveSovereigntyBridge</p></td>
<td><p>Binds agentic/sovereignty overlays to mesh phase
transitions.</p></td>
</tr>
<tr>
<td><p>BusinessApps</p></td>
<td><p>PhaseManagerPort, PhaseTransitionPort, StepBackPort</p></td>
<td><p>APIs for phase config, transition, feedback/rewind;
pre/postcondition enforcement.</p></td>
</tr>
<tr>
<td><p>Foundation</p></td>
<td><p>AuditLogPort, AnalyticsEventPort</p></td>
<td><p>Comprehensive session/phase/feedback/engagement auditing and
research instrumentation.</p></td>
</tr>
<tr>
<td><p>AgencyLayer</p></td>
<td><p>MultiAgentHandoff, OrgConfig/Policy</p></td>
<td><p>Points for agent orchestration, sovereignty mode, and
org-specific overlays across each phase.</p></td>
</tr>
</tbody>
</table>

**Phase Flow Diagram:**  
1–7 configurable box labels (e.g., Human Analysis, AI Processing,
etc.)  
→ step arrows for normal flow  
→ feedback/rewind arrows (configurable at each phase)  
→ audit/log trail overlay  
→ multi-agent/sovereignty handoff injectors at configured steps

------------------------------------------------------------------------

## 2. Phase Configuration, Step-Back, Governance

**Phase Manager API:**

- /v1/sandwich/phases: GET/POST/PATCH configures number/order/labels,
  enables/disables phases at runtime.

- Org-level or template-based policies.

- G/W/T:

  - Given phase config changed while sessions active, when incompatible,
    then batch migration or session block.

**StepBackPort:**

- /v1/sandwich/stepback/{phaseId}:

  - Rolls back process state, records rationale, and checks policies for
    next permissible step.

- All step-back actions are logged and visible for audit/research.

**Pre/Postcondition Endpoints:**

- /v1/sandwich/phase/{n}/pre and /post:

  - Reports condition status, rationale, and next step or block action.

**Governance Highlights:**

- All phase, transition, step-back, and condition contracts versioned
  (n–1 support).

- Migration docs required for any phase/contract change.

- Feedback/step-back feature flag gate on runtime.

**OpenAPI Pointers:**

- Each phase, transition, feedback loop, policy, and agent injection
  exposed with contract/schema pointer for integration/test.

------------------------------------------------------------------------

## 3. Phase Logic & Pre/Postcondition Enforcement

- Every phase exposes and enforces pre and postconditions as defined in
  /components/schemas/PhaseCondition (e.g., data integrity, cognitive
  engagement, completed validation checklist).

- On transition, the backend checks all conditions; errors or failures
  block the forward move and log the exact reason, surfacing necessary
  next steps.

- Feedback/rewind logic can be enabled/disabled per
  phase/policy/template.

- Transition interface supports custom/optional rationale, evidence, or
  scored checklist for validation.

- Policy override required (with audit log) for forced bypass.

------------------------------------------------------------------------

## 4. Audit Logging, Cognitive Debt & Analytics

- All process events, transitions, phase state, rationale, and feedback
  events are audit-logged (userId, phaseId, timestamps, rationale).

- CognitiveDebtMonitor runs at phase entry and exit, recording
  engagement levels, cognitive/critical thinking scores, and dependency
  risk.

- Metrics (neural connectivity proxy, feedback freq, time-in-phase,
  validation completeness/error rates) exported to analytics DB and
  exposed for research.

- Optional research export flag for pilot/empirical studies.

------------------------------------------------------------------------

## 5. Multi-Agent and Sovereignty Integration

- Each phase configuration allows handoff points for mesh agent
  orchestration, with per-phase sovereignty overlays as needed.

- Org/cultural policies (e.g., Capgemini Agentic AI, Hofstede, or
  custom) are validated via SovereigntyManager and govern advance,
  feedback, or handoff logic.

- CSI (Cognitive Sovereignty Index) checked and enforced at each agentic
  phase; mode transitions and rationale logged.

- Autonomous, hybrid, or manual flows governed by current org/template
  policy and surfaced at each phase endpoint.

------------------------------------------------------------------------

## 6. NFRs, Versioning, Deprecation Table

<table style="min-width: 75px">
<tbody>
<tr>
<th><p>Area</p></th>
<th><p>SLA/Threshold</p></th>
<th><p>Notes</p></th>
</tr>
&#10;<tr>
<td><p>Phase API</p></td>
<td><p>&lt;200ms normal, &lt;500ms with audit/research</p></td>
<td><p>End-to-end including transition, step-back etc.</p></td>
</tr>
<tr>
<td><p>Audit/Event</p></td>
<td><p>Persist within &lt;1s</p></td>
<td><p>Export batch for research/compliance &lt;10s</p></td>
</tr>
<tr>
<td><p>Versioning</p></td>
<td><p>n–1 contract migration supported for all ports</p></td>
<td><p>Migration docs/alerts mandatory</p></td>
</tr>
<tr>
<td><p>Feedback</p></td>
<td><p>Feature-flag gate, full contract coverage</p></td>
<td><p>Max step-back ×N/session (configurable)</p></td>
</tr>
<tr>
<td><p>Test/Govern.</p></td>
<td><p>Full matrix, gating on phase×transition×edge</p></td>
<td><p>No release without 100% pass + documentation</p></td>
</tr>
</tbody>
</table>

------------------------------------------------------------------------

## 7. Diagram Library, Test Harness Matrix, Research Hooks

**Diagram Library:**

- Phase stepper/timeline with configurable boxes (phases), arrows for
  normal and feedback/rewind, overlays for multi-agent and sovereignty
  injection.

- State overlays for pre/postconditions and audit/logs at each
  edge/transition.

**Test Harness Matrix:**

- Axis 1: Phases (configurable 3–7)

- Axis 2: Transition (normal, feedback, forced, failed pre/post)

- Axis 3: Edge cases (disable/enable, skip, failover, policy override,
  compliance export)

- Required: 100% coverage before gating

**Research Hooks:**

- Every phase, feedback, rationale, and cognitive engagement event can
  be mirrored (via flag) to research DB.

- Pilot session markers and metrics (as agreed with research/PI) emitted
  securely for empirical study.

- Compliance support: full retention/export, per-org policy, participant
  opt-in.

------------------------------------------------------------------------
