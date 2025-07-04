# Contextual Adaptive Agency/Sovereignty Router Backend PRD (Hexagonal, Mesh Layered)

## TL;DR

The Contextual Adaptive Agency/Sovereignty Router mediates between
Capgemini-style autonomous agentic workflows and Cognitive
Sovereignty-driven contexts. It dynamically selects the optimal level of
agent autonomy, agency, and authority per task, with mesh-layered
architecture and full audit, override, and compliance baked in. This
backend ensures enterprises can balance efficiency with authorship
preservation, supporting context-sensitive, user-driven agency.

------------------------------------------------------------------------

## Goals

### Business Goals

- Enable dynamic, per-task configuration of agent autonomy, agency, and
  authority in mesh environments.

- Ensure robust, real-time auditability and compliance for all agentic
  and sovereignty-driven interactions.

- Support regulatory and organizational mandates for authorship, safety,
  and transparency.

- Minimize risk and liability from misaligned agentic actions through
  adaptive routing and overrides.

- Deliver fast, composable backend services for integration into mesh
  and enterprise platforms.

### User Goals

- Provide organizations control over agent behavior—ranging from fully
  autonomous to sovereignty-preserving—on a contextual, per-task basis.

- Empower admins and users to override agent autonomy or revert to human
  authorship mode with immediate effect.

- Offer full visibility and traceability into how and why agentic
  decisions are made and acted upon.

- Guarantee compliance with evolving privacy, audit, and regional data
  residency requirements.

- Maintain seamless, resilient operation even under edge cases, schema
  drift, or network interruption.

### Non-Goals

- Building UI control panels or front-end widgets (deferred to mesh
  widget PRD).

- Supporting non-cognitive, purely physical robotics or IoT
  applications.

- Providing real-time, sub-50ms latency for every task (SLA is
  contextual).

- Implementing human-in-the-loop UX flows not originated from mesh
  system calls.

------------------------------------------------------------------------

## User Stories

**Org Admin**

- As an Org Admin, I want to configure task policies that specify the
  agent's level of autonomy, so that mission-critical tasks always
  require human confirmation.

- As an Org Admin, I want access to a full audit log of agentic
  decisions, so that I can prove compliance during audits.

- As an Org Admin, I want to receive notifications when agentic
  overrides or policy breaches occur, so that I can intervene when
  needed.

**Knowledge Worker**

- As a Knowledge Worker, I want to request a downgrade to
  "sovereignty-first" mode on sensitive content, so that I retain
  authorship over AI-assisted outputs.

- As a Knowledge Worker, I want to see agent actions flagged when
  operating in full autonomy, so that I stay aware of all automated
  decisions.

**Compliance Officer**

- As a Compliance Officer, I want to export agent/routing logs filtered
  by period and user, so that I can satisfy regulatory requirements.

- As a Compliance Officer, I want assurance that agent data paths never
  cross regional or tenant boundaries, so that privacy is preserved.

**System Integrator**

- As a System Integrator, I want to receive override and error feedback
  from the Agency Router, so that I can surface these events in
  dashboards.

------------------------------------------------------------------------

## Functional Requirements

- **Dynamic Agency/Autonomy Routing** (Priority: Must)

  - Receives task context, analyzes Cognitive Sovereignty Index (CSI)
    and Cognitive Impact Assessment (CIA), applies org/task policy to
    choose AAA (Autonomy, Agency, Authority).

  - Exposes AgencyModePort for downstream mesh system calls (OpenAPI:
    /docs/spec/adaptive-agency.yaml#/paths/~1v1~1agency-mode~1set).

  - G/W/T: If task CIA \> policy threshold, enforce sovereignty-first;
    else, allow agentic autonomy.

- **User/Profile Override Handling** (Priority: Must)

  - OverridePort supports user/admin-initiated downgrades or elevations
    of autonomy.

  - Must process within 150ms, and maintain immutability in logs.

  - G/W/T: On network or schema mismatch, returns error and logs with
    correlationID; retries up to 3x with exponential backoff.

- **CIA/CSI Exposure** (Priority: Should)

  - CIA/CSIIntrospectPort surfaces current context's impact/sovereignty
    scores for use by downstream logic and UI.

- **Audit & Logging Adapters** (Priority: Must)

  - All agency routing, overrides, and agent actions logged via
    AuditLoggingAdapter (OpenAPI:
    /docs/spec/audit.yaml#/paths/~1v1~1log~1event).

  - Multi-tenant and regional audit enforcement.

- **Notification and Consent Adapters** (Priority: Must)

  - All policy breaches or high-autonomy paths trigger notifications
    (OpenAPI: /docs/spec/notifications.yaml#/paths/~1v1~1notify~1event)
    and consent overlays when required.

- **Error & Offline Handling** (Priority: Must)

  - Adapters handle G/W/T scenarios for normal success, protocol/version
    mismatch, error, and offline queue.

  - Retries: 3x, exponential backoff (250ms plus ≤50ms jitter, max delay
    1s). Afterwards, fail gracefully and trigger circuit breaker.

- **Manual Escalation** (Priority: Should)

  - ManualEscalationPort available for explicit admin intervention when
    stuck in sovereignty-lock or policy escalation scenario.

------------------------------------------------------------------------

## User Experience

While this is a backend PRD, the API/adapter designs are meant to drive
clarity and consistency for client developers and system integrators:

- **Entry Point**

  - Task context is sent via AgencyModePort with AAA and context
    metadata.

  - If first-time configuration, onboarding endpoint allows admin to set
    policy defaults (org- or tenant-wide).

  - Consent workflows initiate as needed per org policy.

- **Core Experience**

  - Step 1: Request received, router computes CSI and CIA for context.

    - Backend matches task type, origin, and policy.

    - Applies context-aware logic: authorship preservation for
      creative/knowledge tasks; full autonomy for repeatable and
      well-scoped tasks.

  - Step 2: Sends AAA and agency mode selection back to requesting
    client.

    - Keeps detailed routing logs for every decision (raw context, CSI,
      mode overrides).

    - If CIA is at or above "sovereignty lock" threshold, forces
      authorship reservation.

  - Step 3: Adapters ensure logging, consent, and notification flows
    complete per event.

    - On errors or offline, follows retry/backoff policy. If unresolved,
      fails gracefully and records escalation event.

  - Step 4: OverridePort can be invoked at any time; immediate effect,
    logged for audit.

  - Step 5: All relevant events made available to downstream systems
    (e.g., mesh front ends, admin tools) via pub-sub or direct pull.

- **Advanced Features & Edge Cases**

  - Circuit breaker: if too many errors, circuit opens and all new
    requests are queued until error rate decreases (resumes after 5m of
    success).

  - Policy drift or schema version mismatches: system returns
    error-envelope with required fields, disables agentic actions until
    schema upgraded or admin review completed.

  - Tenant/regional isolation: all actions and logs strictly segmented
    per policy and regulator.

- **UI/UX Highlights for API Consumers**

  - Consistent, clear error-envelope ({ error_code, message,
    correlationID }) on all failures for quick diagnosis.

  - API responses include provenance metadata and current agency mode
    for downstream surfacing.

  - Consent and high-risk notifications are standardized for easy
    handling in mesh surfaces.

------------------------------------------------------------------------

## Narrative

In the rapidly evolving world of AI, organizations now face a critical
design challenge: when should AI agents be fully autonomous, and when
must human authorship, intent, and sovereignty be preserved? The
Contextual Adaptive Agency/Sovereignty Router ensures that each business
process, task, or user interaction is governed by the right mix of
autonomy, agency, and authority—always balancing efficiency with
control.

For an admin or compliance officer, every agentic decision is traceable,
auditable, and paired with policy-grade controls so that oversight is
simple and robust. For knowledge workers, moment-to-moment authorship is
never compromised: the system only delegates as far as human, org, or
regulatory policy allows—down to the per-task and per-context level.
Override options are immediate, notifications are reliable, and full
auditability is guaranteed. Ultimately, this backend enables the
enterprise to enjoy the power of agentic systems while never sacrificing
their own standards of responsibility, authorship, and compliance.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- % of tasks routed in correct agency mode (AAA match to
  policy/context).

- User/admin satisfaction with agency/override controls (survey/NPS).

- Override response time (\<350ms at P99).

- Frequency of successful/failed override events.

### Business Metrics

- Audit-log completeness and accuracy (manual review).

- Reduction in policy breach or regulatory incidents.

- Administrative time to approve, review, or override agentic events.

- Stakeholder adoption and retention rates.

### Technical Metrics

- API response time SLA (\<200ms normal, \<350ms override).

- System uptime \>99.99%.

- Audit log export latency (\<5m worst-case).

- Error rate, retries, and circuit-breaker activations.

### Tracking Plan

- Agency mode set/cancelled events

- Override events (user/admin, direction, context)

- Consent granted/denied triggers

- Audit/export events by period

- Error-envelope returns (by type)

- Policy/schema drift flags

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- ReasoningLayer: AgencyRouterCore, ContextAnalyzer

- BusinessApps: AgencyModePort, OverridePort, CIA/CSIIntrospectPort

- Foundation: ConsentAdapter, AuditLoggingAdapter, NotificationAdapter

- AgencyLayer: registration and management for MultiAgentBots

### Integration Points

- Mesh orchestrators and widgets (for control/override and state query)

- Org policy and user identity management for profile-specific routing

- Audit, compliance, and consent storage systems (multi-tenant,
  regionalized)

### Data Storage & Privacy

- Tenant and region-segregated logs (SOC2, GDPR aligned)

- All AAA/override events and agency selections are immutable/traceable

- Agency mode, CSI, and CIA scores available for post-hoc analysis

### Scalability & Performance

- Designed for high throughput, low-latency decision routing (\<200ms
  SLA)

- Handles large multi-agent task loads; queues and circuits for
  resilience

- Scales horizontally by tenant/org/policy domain

### Potential Challenges

- Policy drift across tenants/orgs requiring multi-policy arbitration

- Schema evolution versioning and compatibility

- Ensuring fast override propagation under heavy load

- Third-party mesh integration reliability

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

Medium: 2–4 weeks

### Team Size & Composition

Small Team: 2 people

- 1 Core Backend Engineer (owns Reasoning/Business/Fdn/Agency Layers)

- 1 Product/Compliance Designer (policy, metrics, audit system)

### Suggested Phases

**Phase 1: Core Routing & Audit Support (1 week)**

- Key Deliverables:

  - AgencyRouterCore, AgencyModePort, AuditLoggingAdapter

- Dependencies:

  - Mesh orchestrator API, minimal org/user policy config

**Phase 2: Override, Consent, Notification Adapters (1 week)**

- Key Deliverables:

  - OverridePort, ConsentAdapter, NotificationAdapter

- Dependencies:

  - Identity integration for user/admin override authority

**Phase 3: Policy Edge Cases, CIA/CSI Ports, Compliance/QA (1 week)**

- Key Deliverables:

  - CIA/CSIIntrospectPort, manual escalation, all remaining test harness

  - Visuals: component and override sequence diagrams

- Dependencies:

  - Full test harness integration, mesh-driven scenario tests

**Phase 4: Schema Versioning, NFR Harden, CI/CD Integration (1 week)**

- Key Deliverables:

  - Complete OpenAPI, deprecation logic, governance triggers

- Dependencies:

  - GitHub/registry hooks, admin notification logic

------------------------------------------------------------------------

## 1. Mesh Layer Mapping & Component Diagram

![Component
Diagram](docs/diagrams/adaptive-agency-router/component-thumb.svg)

*Figure: Component Architecture Overview*

------------------------------------------------------------------------

## 2. Ports & Adapters (MoSCoW, G/W/T, OpenAPI JSON Pointer)

- **AgencyModePort** with priority (Must) and uses
  /docs/spec/adaptive-agency.yaml#/paths/~1v1~1agency-mode~1set.

- **OverridePort** with priority (Must) handles user/admin downgrades or
  autonomy changes; G/W/T scenario driven.

- **CIA/CSIIntrospectPort** prioritized (Should) for context scoring;
  downstream use detailed.

Retries: All adapters use 3x exponential backoff (250ms+, ≤50ms jitter,
1s max), then circuit breaker open—returns error envelope, logs context
and correlationID.

------------------------------------------------------------------------

## 3. Adaptive Routing Logic

**Business logic:**

- Receives context: task type, user profile, org policy, CSI, CIA.

- Decision table: If agent path is chosen and CIA/CSI out of bounds,
  system forces post-hoc audit (no irreversible action until human/admin
  sign-off).

------------------------------------------------------------------------

## 4. Error, Audit, Compliance, and Escalation

- All events—routing, override, agent action, and adaptive decision—are
  logged via AuditLoggingAdapter (OpenAPI:
  /spec/audit.yaml#/paths/~1v1~1log~1event).

- Multi-tenant and region-aware: enforces tenant/org isolation, regional
  requirements (GDPR, SOC2).

- High-autonomy or policy breach: triggers notification overlay, consent
  prompt, and admin audit queue.

- ManualEscalationPort enables forced admin intervention and lockdown in
  event of sovereignty lock, breach, or unknown state.

### Error Envelope Schema Appendix

- { error_code, message, correlationID } standardized for all error
  states; cross-referenced in communication with adapters.

------------------------------------------------------------------------

## 5. Service-Specific NFRs

- **Latency**: API responses \<200ms.

- **Memory**: \<20MB/process.

- **Payload**: \<200kB.

- **Resilience**: \>99.9% uptime.

SOC2, GDPR, and org-specific rules are enforced on all logs, consents,
and notifications.

------------------------------------------------------------------------

## 6. Test Harness Matrix & Visuals

<table style="min-width: 150px">
<tbody>
<tr>
<th><p>Port/Adaptor</p></th>
<th><p>Normal</p></th>
<th><p>Error</p></th>
<th><p>Override</p></th>
<th><p>Offline</p></th>
<th><p>Drift</p></th>
</tr>
&#10;<tr>
<td><p>AgencyModePort</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
</tr>
<tr>
<td><p>OverridePort</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
</tr>
<tr>
<td><p>CIA/CSIIntrospect</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✗</p></td>
<td><p>✓</p></td>
<td><p>✗</p></td>
</tr>
<tr>
<td><p>AuditLoggingAdapter</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✗</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
</tr>
<tr>
<td><p>ConsentAdapter</p></td>
<td><p>✓</p></td>
<td><p>✓</p></td>
<td><p>✗</p></td>
<td><p>✓</p></td>
<td><p>✗</p></td>
</tr>
</tbody>
</table>

*Visuals:*

- 

![Sequence
Diagram](docs/diagrams/adaptive-agency-router/sequence-thumb.svg)

*Figure: Adaptive Mode Override Flow*

------------------------------------------------------------------------

## 7. Versioning, Deprecation, Governance

- API ports always support n–1 versions (semver), with required backward
  compatibility.

- Deprecation workflow: 90-day warning via GitHub issue, registry/email
  alert to all stakeholders, migration guide posted.

- Explicit notification via email, Slack, and registry dashboard; no
  production publish until consumer acknowledgement is logged.

- Any policy or schema change triggers admin review and risk flags;
  breaking changes require full sunset doc and rollback path for all
  mesh consumers.

- All major changes logged, and exportable with timestamped versions for
  regulatory review.

------------------------------------------------------------------------

## 8. Policy Table Examples

Example Policies:

<table style="min-width: 50px">
<tbody>
<tr>
<th><p>Condition</p></th>
<th><p>Action</p></th>
</tr>
&#10;<tr>
<td><p>CIA &gt; 0.8</p></td>
<td><p>Enforce Sovereignty Mode</p></td>
</tr>
<tr>
<td><p>AAA.agency = finance &amp;&amp; CSI &lt; 0.5</p></td>
<td><p>Require Manual Consent</p></td>
</tr>
</tbody>
</table>

------------------------------------------------------------------------
