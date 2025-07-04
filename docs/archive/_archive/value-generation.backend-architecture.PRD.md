# Value Generation Backend Architecture PRD (Hexagonal, Mesh Layered, Tightened)

### TL;DR

This backend delivers actionable value diagnostics,
creativity/employability risk detection, and organizational “value
blindness” analysis using a rigorous ports-and-adapters (hexagonal)
architecture mapped to mesh platform layers. All engines, ports, and
adapters carry explicit MoSCoW prioritization, testable
“Given/When/Then” acceptance criteria, precise API contract references,
detailed error/retry/fallback governance, and compliance lock-ins for
every HR-sensitive workflow.

------------------------------------------------------------------------

## 1. Mesh Layer Map, Adapters, and Core Engines

<table style="min-width: 100px">
<tbody>
<tr>
<th><p>Mesh Layer</p></th>
<th><p>Domain Engines / Services</p></th>
<th><p>Ports / Adapters</p></th>
<th><p>Key Data/Flow Connections</p></th>
</tr>
&#10;<tr>
<td><p>ReasoningLayer</p></td>
<td><p>ValueGenerationDiagnosticEngine,</p></td>
<td><p>–</p></td>
<td><p>Receives API payloads, executes analysis;</p></td>
</tr>
<tr>
<td></td>
<td><p>OrganizationalValueBlindnessEngine,</p></td>
<td></td>
<td><p>triggers outbound adapters upon diagnosis</p></td>
</tr>
<tr>
<td></td>
<td><p>EmployabilityPredictorEngine</p></td>
<td></td>
<td></td>
</tr>
<tr>
<td><p>BusinessApps</p></td>
<td><p>API Ports (REST/gRPC):</p></td>
<td><p>ConsentPort (HR)</p></td>
<td><p>Inbound points for frontend, HR, 3rd-party</p></td>
</tr>
<tr>
<td></td>
<td><p>– ValueDiagnosticPort</p></td>
<td><p>ManualAdjudicationPort (HR/manual)</p></td>
<td><p>Connections; consent gating; workflow state</p></td>
</tr>
<tr>
<td></td>
<td><p>– OrgBlindnessDetectionPort</p></td>
<td></td>
<td><p>management; version enforcement</p></td>
</tr>
<tr>
<td></td>
<td><p>– EmployabilityPort</p></td>
<td></td>
<td></td>
</tr>
<tr>
<td><p>FoundationLayer</p></td>
<td><p>AuditLoggingAdapter,</p></td>
<td><p>NotificationAdapter</p></td>
<td><p>Outbound logging; user/org notification;</p></td>
</tr>
<tr>
<td></td>
<td><p>EventBusAdapter, DBAdapter</p></td>
<td><p>Event/DB/Storage Adapters</p></td>
<td><p>state persistence; multi-region failover</p></td>
</tr>
<tr>
<td><p>AgencyLayer</p></td>
<td><p>NudgeAgentAdapter (optional)</p></td>
<td><p>Nudge Bot Adapters</p></td>
<td><p>Event-based triggers for automated feedback</p></td>
</tr>
</tbody>
</table>

**Visual Component Diagram**

- Widget/API client → BusinessApps Ports
  (REST/gRPC/Consent/ManualReview) → ReasoningLayer Core Engines →
  FoundationLayer (AuditLogging, Event, Notification)

- Optional: AgencyLayer Nudge Bots trigger on relevant events

- All flows auditable; schema validation and version gates at each port

------------------------------------------------------------------------

## 2. Ports & Adapters: Prioritization & Universal Tests

<table style="min-width: 75px">
<tbody>
<tr>
<th><p>Name</p></th>
<th><p>Priority</p></th>
<th><p>G/W/T Acceptance Test Example</p></th>
</tr>
&#10;<tr>
<td><p>ValueDiagnosticPort (REST)</p></td>
<td><p>Must</p></td>
<td><p>Given payload at
/docs/spec/value-diagnostic.yaml#/paths/~1v1~1value-diagnostic fails
schema,</p></td>
</tr>
<tr>
<td></td>
<td></td>
<td><p>When submitted, Then respond 400/error_code=INVALID_PAYLOAD, log
correlationID, retry once (exp backoff), queue to batch.</p></td>
</tr>
<tr>
<td><p>OrgBlindnessDetectionPort</p></td>
<td><p>Must</p></td>
<td><p>Given valid org diagnostic payload, When submitted, Then respond
200, result in &lt;250ms P99, log audit event.</p></td>
</tr>
<tr>
<td><p>EmployabilityPort</p></td>
<td><p>Must</p></td>
<td><p>Given employability check with missing consent, When submitted,
Then respond 403/error_code=CONSENT_MISSING, log attempt.</p></td>
</tr>
<tr>
<td><p>ConsentPort</p></td>
<td><p>Must</p></td>
<td><p>Given new HR-sensitive submission, When consent not present, Then
reject downstream work, banner UI, record event.</p></td>
</tr>
<tr>
<td><p>ManualAdjudicationPort</p></td>
<td><p>Should</p></td>
<td><p>Given flagged employability user, When case submitted, Then
require explicit human review/approval before data release.</p></td>
</tr>
<tr>
<td><p>AuditLoggingAdapter</p></td>
<td><p>Must</p></td>
<td><p>Given any value-diagnostic event, When processed, Then write
event to ledger with correlationID in &lt;100ms.</p></td>
</tr>
<tr>
<td><p>NotificationAdapter</p></td>
<td><p>Must</p></td>
<td><p>Given high-severity diagnostic, When triggered, Then notify
frontend/email in &lt;2s, with digest queue as fallback.</p></td>
</tr>
<tr>
<td><p>EventBusAdapter</p></td>
<td><p>Should</p></td>
<td><p>Given backend persistence fail, When event log unreachable, Then
escalate to multi-region failover, alert ops.</p></td>
</tr>
<tr>
<td><p>DBAdapter</p></td>
<td><p>Should</p></td>
<td><p>Given write conflict, When detected, Then retry (exp backoff),
max 3x, else queue for async recovery; log error.</p></td>
</tr>
<tr>
<td><p>NudgeAgentAdapter (optional)</p></td>
<td><p>Could</p></td>
<td><p>Given nudgable failed $200 test event, When detected, Then
trigger agent/notification, log actuation.</p></td>
</tr>
</tbody>
</table>

**Circuit-breaker/Retry Policy:**

- Max 3 retries: Initial 200ms, double up to 1s, jitter ±100ms

- On pass, circuit resets after 5 healthy calls; further failures reopen
  breaker

------------------------------------------------------------------------

## 3. API Contract Reference

- **ValueDiagnosticPort:**
  /docs/spec/value-diagnostic.yaml#/paths/~1v1~1value-diagnostic

- **OrgBlindnessDetectionPort:**
  /docs/spec/org-blindness.yaml#/paths/~1v1~1org-blindness

- **EmployabilityPort:**
  /docs/spec/employability.yaml#/paths/~1v1~1employability

- **ConsentPort:** /docs/spec/consent.yaml#/paths/~1v1~1consent

- **ManualAdjudicationPort:**
  /docs/spec/review.yaml#/paths/~1v1~1manual-review

- **AuditLoggingAdapter:**
  /docs/spec/audit.yaml#/paths/~1v1~1audit-event

- **NotificationAdapter:** /docs/spec/notify.yaml#/paths/~1v1~1notify

*Note: Any schema change or version bump mandates cross-functional
review (frontend, widget, HR/compliance, and ops).*

------------------------------------------------------------------------

## 4. State, Caching, and Circuit/Fallback Management

- **Adapter Caching:** Diagnostics, org results cached for 30 minutes by
  default

- **Invalidation Triggers:**

  - Manual refresh

  - New diagnostic run

  - API version change

  - Data expiry

- **Fallbacks:**

  - On repeated adapter failure: Switch to batch pipeline if available;
    user receives degraded (“pending” or “retry later”) status and
    notified via UI/ops banner

- **State Transition Logging:** All cache writes, invalidations, and
  restoration events are logged with correlationID for inspection/audit

------------------------------------------------------------------------

## 5. Error Envelope, Retry, and Reset Policy

- **Standard Error Envelope**:

  - { error_code: string, message: string, correlationID: string }

  - Example: { error_code: "INVALID_PAYLOAD", message: "Schema violation
    in field X", correlationID: "abc123" }

- **Retry Parameters**:

  - Max 3 attempts

  - Exponential backoff: 200ms, 400ms, 800ms (±100ms jitter each)

  - On persistent fail: escalate to queue, mark as OUTAGE if \>5 min

- **Circuit Reset**: After 5 consecutive healthy requests, breaker
  resets; failures reopen

- **User/Operator Alerts**:

  - Persistent error banners in ops UI; error details downloadable by
    support

------------------------------------------------------------------------

## 6. Service-Specific NFR Table

<table style="min-width: 100px">
<tbody>
<tr>
<th><p>Service/Port</p></th>
<th><p>SLA / Perf</p></th>
<th><p>Security &amp; Compliance</p></th>
<th><p>Special Notes</p></th>
</tr>
&#10;<tr>
<td><p>ValueDiagnosticPort</p></td>
<td><p>&lt;300ms P99</p></td>
<td><p>Uses Global NFR</p></td>
<td></td>
</tr>
<tr>
<td><p>OrgBlindnessDetectionPort</p></td>
<td><p>&lt;250ms P99</p></td>
<td><p>Pen-test annually, compliance</p></td>
<td><p>Data residency on EU+US basis</p></td>
</tr>
<tr>
<td><p>EmployabilityPort</p></td>
<td><p>&lt;350ms P99</p></td>
<td><p>Consent required, manual review</p></td>
<td><p>Regional data localization</p></td>
</tr>
<tr>
<td><p>AuditLoggingAdapter</p></td>
<td><p>&lt;100ms write</p></td>
<td><p>Multi-region failover, GDPR</p></td>
<td><p>Data retention: 2 years</p></td>
</tr>
<tr>
<td><p>NotificationAdapter</p></td>
<td><p>&lt;2s notif/digest</p></td>
<td><p>Encrypted at rest, audit trail</p></td>
<td><p>Escalation path if delivery fails</p></td>
</tr>
<tr>
<td><p>ManualAdjudicationPort</p></td>
<td><p>&lt;1h turn-around</p></td>
<td><p>Human-in-loop required</p></td>
<td><p>Access control for HR only</p></td>
</tr>
</tbody>
</table>

------------------------------------------------------------------------

## 7. Governance, Consent, and Manual Review Ports

- **ConsentPort:** Mandatory for all HR/employability data submissions;
  no processing without digital signature/acknowledgement

- **ManualAdjudicationPort:** All flagged employability/org-blindness
  outputs routed for explicit human review; requires logging of
  reviewer, timestamp, outcome before data is surfaced externally or
  used for employment action

- **Approval and Logging:** Every consent/decision action is auditable;
  logs must include unique user/org ID, consent hash, reviewer name, and
  timestamp

------------------------------------------------------------------------

## 8. Connective Diagrams: Sequence, Consent Flow, Component View

### (a) End-to-End Diagnostic Sequence

1.  **Widget/API client** initiates request →

2.  **BusinessApps API Port (REST/gRPC)** receives →

3.  **ReasoningLayer Engine** processes (diagnosis/calculation) →

4.  **FoundationLayer AuditLoggingAdapter** writes ledger;

5.  **NotificationAdapter** sends result/alert →

6.  **UI/ops widget** receives result/success/error

### (b) Consent/Manual Review Flow

- Submission → ConsentPort (if not present, block and log) →

- If high-risk/flagged → ManualAdjudicationPort → human review/approval
  recorded

- If approved → ReasoningLayer Engine → downstream (logging,
  notification, etc.)

### (c) Component Connectivity Overview

- **BusinessApps**: API/Consent/ManualReview Ports ↔ **ReasoningLayer**:
  Engines ↔ **FoundationLayer**: Adapters (Audit/Notify/Event/DB) ↔
  Storage/Event Bus

- **AgencyLayer**: Receives events (Nudge, Notification) directly from
  FoundationLayer or ReasoningLayer as needed

------------------------------------------------------------------------

## 9. CI/Test Harness & Contract Gating

- **Test Harness Requirements**

  - Mocks every port and adapter with pass, fail, edge (schema
    violation, network fail, offline, consent denied, HR rejection), and
    circuit reset paths

  - Linked to CI pipeline with:

    - OpenAPI lint/schema validation

    - SLA-driven performance/load test

    - Error envelope conformance (format, content)

    - Compliance/regulatory checklist (data residency, consent)

    - Deprecation notice validation for n–1 support

    - Onboarding: Registry contracts must pass all integration tests
      before publish/release

------------------------------------------------------------------------

## 10. Versioning, Deprecation, Change Management

- **n–1 Major Version Support:** Whenever vN+1 of a port is released,
  maintain n–1 (previous major) version for backwards compatibility for
  at least 90 days

- **Deprecation Procedure:**

  - Publish 90-day advance sunsetting warning on port release notes, API
    docs, and operator/consumer dashboards

  - Provide migration guide and diffs to frontend/widget teams and
    external integrators

  - Mobile/SDK clients required to handle API version drift by (a)
    warning users, (b) disabling non-compliant features, and (c)
    prompting upgrades/roll-backs as needed

- **Change Management:**

  - All version bumps and contract changes gate on registry onboarding,
    lint/test passing, and compliance signoff

  - Emergency break/fix patches escalate directly to ops with rolling
    restarts across adapters/ports

------------------------------------------------------------------------
