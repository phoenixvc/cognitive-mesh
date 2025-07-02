# Cognitive Mesh NIST AI RMF Maturity Framework Mesh Widget PRD (Hexagonal, Mesh Layered, Testable)

### TL;DR

Mesh widgets empower users, admins, and auditors to directly manage, evidence, and track the NIST AI RMF maturity process. The UI suite covers end-to-end assessment checklists, artifact/evidence handling, scoring dashboards, improvement tracking, and compliance alerting—built for regulatory assurance, continuous improvement, and stakeholder transparency.

------------------------------------------------------------------------

## Goals

### Business Goals

- Achieve and maintain a minimum maturity score of 3.5/5 across all NIST RMF statements in year 1.
- Streamline regulatory compliance processes for enterprise clients, reducing audit preparation time by 50%.
- Deliver provable, evidence-based AI governance that differentiates the Cognitive Mesh offering.
- Increase customer trust and contract wins by demonstrating real-time auditability.
- Achieve user adoption by all key persona groups within 90 days post-launch.

### User Goals

- Enable users to easily complete and submit NIST assessments and supporting evidence with minimal training.
- Allow admins to review, approve, and comment on submitted artifacts in a centralized dashboard.
- Provide instant visibility into compliance status, maturity scores, and roadmap gaps.
- Keep all stakeholders informed of required upgrades, drift, or deprecation through UI notifications.
- Ensure users can locate and understand requirements, evidence, and improvement plans at a glance.

### Non-Goals

- The UI will not serve as a primary interface for managing user authentication or access policies.
- Will not automate legal or regulatory filings outside the provided evidence export features.
- Will not support custom maturity models outside of the NIST AI RMF scope (V1).

------------------------------------------------------------------------

## User Stories

### Persona: Compliance Officer

- As a Compliance Officer, I want to walk through the NIST checklist, so that I can identify maturity gaps across the organization.
- As a Compliance Officer, I want to review uploaded evidence, so that I ensure regulatory standards are met.
- As a Compliance Officer, I want to receive notifications of upcoming deprecation or drift, so that I can proactively plan for migrations.

### Persona: AI Product Owner

- As an AI Product Owner, I want to see a real-time maturity dashboard, so I can communicate status and next steps to leadership.
- As an AI Product Owner, I want to submit supporting documentation for each NIST requirement, so that progress tracking is centralized.

### Persona: Internal Auditor

- As an Internal Auditor, I want to view, filter, and export complete audit trails (who submitted what, when), so I can confidently pass compliance reviews.

### Persona: Business Executive

- As a Business Executive, I want executive summaries and improvement roadmaps presented concisely, so I can make informed resource investment decisions.

### Persona: External Stakeholder

- As an External Reviewer, I want to see clear, immutable evidence and maturity scores, so I can verify the organization’s AI maturity claims.

------------------------------------------------------------------------

## Functional Requirements

- **Assessment Panels (Priority: Must)**

  - NIST RMF Checklist Panel: Walks users through each NIST pillar/topic; one-click progression and “mark complete.”
  - Evidence Upload Panel: Allows direct markdown/file artifact uploads, reviewer assignment, and artifact audit linking.
  - Scoring Dashboard: Real-time maturity scoring and RMF pillar sub-scores.
  - Improvement Roadmap Panel: Prioritized, actionable plan aligned to maturity assessment gaps.
  - Notification Panel: Registry/contract drift, error, migration, and review requests.
  - Audit & Activity Log: Time-stamped, filterable, and exportable logs of every artifact, review, and state change.

- **Adapters (Priority: Must)**

  - DataAPIAdapterPort (per topic/statement) with OpenAPI JSON pointer mapping.
  - EvidenceUploadAdapter: Handles markdown/files, artifact size/format validation.
  - DashboardAdapter: Fetches, visualizes, and exports scoring/status.
  - NotificationAdapter: Real-time alerts, overlays, in-app and registry integration.
  - TelemetryAdapter: Logs all user actions, errors, a11y events, state transitions.

- **G/W/T Acceptance Criteria (Priority: Must)**

  - For every action/endpoint, acceptance is defined for normal, error, migration, a11y, registry drift, and off-line states.

- **Shared Error/Envelope Handling (Priority: Must)**

  - All error events and overlays reference the shared { error_code, message, correlationID } schema.

- **Registry/Migration Handling (Priority: Must)**

  - Overlays and guides for deprecation, contract drift, version upgrades; explicit registry acknowledgement required for breaking changes.

- **Executive & Compliance Dashboards (Priority: Should)**

  - One-click executive summary, export of audit/compliance report, and improvement plan snapshot.

------------------------------------------------------------------------

## User Experience

### Entry Point & First-Time User Experience

- Users land on a guided NIST assessment dashboard via SSO or workspace invite.
- First-time onboarding includes a brief interactive tutorial, highlights navigation, and checklist usage.

### Core Experience

- Step 1: User selects a NIST topic or statement from the left-side checklist navigation.
  - Immediate feedback: Progress bar, statement status, and “artifact required” flag are visible.
  - If the panel is incomplete, prompt to upload artifact or add explanation.
- Step 2: Artifact/Evidence panel is accessible.
  - Markdown/file uploader (e.g., drag-and-drop), field for reviewer assignment, strict validation on file size/format.
  - G/W/T overlays for error (e.g., oversize, invalid), migration, deprecation, or incomplete submission.
- Step 3: Reviewer receives notification in the Notification panel and via registry/email if assigned.
  - Reviewer views artifact, leaves comment/approves, initiates feedback loop.
- Step 4: Dashboard auto-updates with scoring based on evidence and status; improvement roadmap panel updates with next steps and gaps.
- Step 5: All user actions, artifact uploads, review decisions, notifications, and migrations are logged in the Audit panel, filterable and exportable.
- Step 6: If registry notices drift or deprecation, overlay modal blocks progress until migration acknowledged or upgrade guide completed.

### Advanced Features & Edge Cases

- Power-user shortcut toggles for mass artifact upload, evidence bulk review, or rapid tab-bar navigation.
- All panels and overlays support colorblind/a11y, platform responsiveness, and offline/failure fallback.

### UI/UX Highlights

- Immediate visual feedback for state (complete/incomplete, reviewed, drift/deprecated).
- Consistent overlay behaviors for error, notification, deprecation, feedback, and registry events.
- Pattern library used for all critical overlays, states, notifications, and audit snapshots.
- All interactive components keyboard accessible, a11y-optimized, responsive on all target devices.
- Direct links out to current OpenAPI spec/documentation for every evidence or scoring action.

------------------------------------------------------------------------

## Narrative

A compliance officer logs in to the Cognitive Mesh maturity dashboard. With a few clicks, they’re guided through each NIST RMF statement—understanding exactly what’s required, what evidence must be gathered, and who needs to participate in review. They upload audit artifacts, assign reviewers, and immediately see scoring progress and improvement gaps. Whenever a requirement, API, or contract changes, the officer is proactively notified: overlays guide the upgrade, while every submission, comment, and migration is tracked in a centralized, filterable audit log. Executives can instantly view compliance status and actionable plans—knowing with certainty that their governance posture is robust and always NIST-aligned. This experience gives enterprise teams the speed, clarity, and confidence to demonstrate—at any moment—that their AI systems are governed and maturing with discipline.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- Completion rate of NIST checklist panels (% per user, per org)
- Average artifact review/approval time
- User satisfaction (CSAT/NPS) on widget usability and clarity
- Number of actionable notifications resolved per quarter

### Business Metrics

- Reduction in compliance prep time (target: 50%)
- Number of audits passed without remediation
- Number of enterprise upgrades to compliant version within migration window

### Technical Metrics

- Widget bundle size (<400kB), cold start (<1s), memory (<30MB)
- Evidence upload error rate (<0.1%)
- UI latency on scoring dashboard (<180ms P95)
- Telemetry flush latency (<10s)

### Tracking Plan

- User: statement opened/closed, artifact upload, evidence submit, reviewer assignment, notification received, audit export, migration guide triggered
- System: API error, drift/deprecation, registry notice, a11y incident

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Core APIs: Checklist/state (GET/PUT), Evidence (POST/GET), Reviewer Actions (POST), Scoring Summary (GET), Notifications (GET/ACK), Telemetry (POST), AuditLog (GET)
- All adapters and panels point to exact OpenAPI JSON Pointers (e.g., `/docs/spec/nist.yaml#/paths/1governance1nist1evidence1submit`)
- Shared Error Envelope schema in all endpoint and event models
- Registry hooks for migration/deprecation, evidence versioning
- Live telemetry/feedback pipelines for all actions and errors

### Integration Points

- **Security & Zero-Trust Framework:**
  - **SSO/IDP:** For authenticated and authorized access to the dashboard and its functions.
  - **Secrets Management:** Secure handling of API keys needed to communicate with backend services.
  - **Encryption:** All uploaded evidence and artifacts must be encrypted at rest using the framework's specified standards (e.g., AES-256).
- **Ethical & Legal Compliance Framework:**
  - **Audit Trail:** The `Audit & Activity Log` must use the foundational `AuditLoggingAdapter` to ensure all actions are immutably logged and meet regulatory requirements (e.g., GDPR, EU AI Act).
  - **Data Handling:** Evidence containing PII or sensitive information must be handled according to policies defined in the legal framework, including data retention and redaction rules.
- **External Systems:**
  - Slack/email for stakeholder review/notification.

### Data Storage & Privacy

- Artifacts stored encrypted, versioned, and audit-linked beyond retention window.
- Access/review only to permitted and assigned users, enforced by the Security & Zero-Trust Framework.
- All actions/data changes audit-logged with timestamp and correlationID.
- GDPR and sector-aligned data handling (evidence/file expiration policies) enforced by the Ethical & Legal Compliance Framework.

### Scalability & Performance

- Designed for 1–5k simultaneous users/org (burst for audit reviews)
- Dynamic cache for audit dash, O(1) scoring, lazy-load artifact previews

### Potential Challenges

- Handling schema drift during contract migrations
- Review bottlenecks in large evidence runs
- UI bundle within strict NFR (size/memory/latency)
- Ensuring all states and overlays are a11y-compliant and testable

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- Medium: 3–5 weeks for MVP/full test coverage in a startup environment

### Team Size & Composition

- Small Team: 2–3 core members (Product + Frontend Engineer + Backend/API)
  - Option: Borrow Design or QA for a few days for review/storybook/testing

### Suggested Phases

**1. Design & OpenAPI Integration (1 week)**

- Deliverables: Wireframes, pattern library, OpenAPI contract mapping
- Dependencies: Backend API stubs, NIST checklist input

**2. Panel/Adapter Implementation, Test Matrix (1.5–2 weeks)**

- Deliverables: Checklist, evidence, dashboard, notification, telemetry panels
- Dependencies: Design sign-off, OpenAPI JSON-pointer tests

**3. Audit, A11y, Error/Drift, and Migration Handling (1 week)**

- Deliverables: Audit panel, overlays, test harness (all G/W/T, a11y, drift/migration)
- Dependencies: Audit event bus, registry integration

**4. Storybook/End-to-End Testing, Documentation, CI Integration (1 week)**

- Deliverables: Storybook entries, full matrix tests, template YAML/JSON configs, onboarding doc, CI pass-gating
- Dependencies: All core panel/adapters built

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh UI components** rather than introducing an entirely new stack. The table and notes below map new widgets, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| New Component | Action Required | Integration Point(s) |
|---|---|---|
| **NIST RMF Checklist Panel**<br>**Evidence Upload Panel**<br>**Scoring Dashboard**<br>**Improvement Roadmap Panel**<br>**Notification Panel**<br>**Audit & Activity Log** | • Create new React components for each widget (`src/UILayer/AgencyWidgets/Panels/`).<br>• Register each via `IWidgetRegistry.RegisterWidgetAsync()` with a unique `WidgetDefinition`. | • `IWidgetRegistry` (`src/UILayer/Core/WidgetRegistry.cs`) to make them available in the dashboard.<br>• `DashboardLayoutService` (`src/UILayer/Services/DashboardLayoutService.cs`) to manage their position and state.<br>• Rendered by the main dashboard shell. |
| **DataAPIAdapterPort** (UPDATE) | • Extend the existing `IDataAPIAdapterPort` to include methods for calling the new backend endpoints (`/v1/assessment/nist/checklist`, `/v1/governance/nist/evidence/submit`, `/v1/governance/nist/review/submit`, `/v1/assessment/nist/score`).<br>• Implement the adapter to consume the backend's OpenAPI spec. | • `IAgencyWidgetAdapters.cs` (or a new `.ts` file).<br>• Consumes endpoints from the new backend services.<br>• Uses standardized `ErrorEnvelope` model. |
| **ConsentAdapter** (UPDATE) | • Extend the existing `IConsentAdapter` to handle consent flows specific to NIST RMF evidence submission (e.g., for sensitive data uploads).<br>• The consent request must be triggered before calling any privileged NIST endpoint. | • `IConsentAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Triggers the shared `Consent/Notification Modal` component. |
| **NotificationAdapter** (UPDATE) | • Extend the existing `INotificationAdapter` to handle new notification types for NIST RMF events (e.g., `EvidenceApproved`, `MaturityDriftDetected`, `DeprecationNotice`). | • `INotificationAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Displays alerts using the shared notification banner component. |
| **TelemetryAdapter** (UPDATE) | • Extend the existing `ITelemetryAdapter` to log new event types: `NISTChecklistCompleted`, `EvidenceSubmitted`, `MaturityScoreViewed`, `ComplianceAlertAcknowledged`. | • `ITelemetryAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Uses the `TelemetryEvent` model (`src/UILayer/AgencyWidgets/Models/AgencyWidgetModels.cs`). |
| **Shared UX Pattern Library** (UPDATE) | • Add new shared components for NIST RMF checklist progress, evidence upload status, maturity score visualizations, and improvement roadmap displays. | • Resides in `src/UILayer/AgencyWidgets/Components/`.<br>• Ensures visual and functional consistency across all widgets. |

### Summary of File-Level Changes

*   **New Files:**
    *   `src/UILayer/AgencyWidgets/Panels/NISTChecklistPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/EvidenceUploadPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/ScoringDashboard.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/ImprovementRoadmapPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/NISTNotificationPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/NISTAuditActivityLog.tsx`

*   **Updated Files:**
    *   `src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs` (or `.ts` equivalent): Update `IDataAPIAdapterPort`, `IConsentAdapter`, `INotificationAdapter`, and `ITelemetryAdapter`.
    *   `src/UILayer/AgencyWidgets/Components/`: Add new shared UI components for NIST RMF visualizations.
    *   **Dashboard Shell Component**: Update to register the new widgets with the `IWidgetRegistry`.
    *   **OpenAPI Client**: Regenerate to include new backend endpoints for NIST RMF.

No other layers require structural change; the widget system integrates cleanly with the existing UI shell and backend APIs via the adapter pattern.

------------------------------------------------------------------------

## Shared Schemas, OpenAPI Pointers, NFRs, & Test Matrix

### OpenAPI JSON Pointer References

| Adapter | OpenAPI JSON Pointer |
|---|---|
| Checklist | `/docs/spec/nist.yaml#/paths/1assessment1nist1checklist1get` |
| Evidence Upload | `/docs/spec/nist.yaml#/paths/1governance1nist1evidence1submit` |
| Reviewer Action | `/docs/spec/nist.yaml#/paths/1governance1nist1review1submit` |
| Scoring Dashboard | `/docs/spec/nist.yaml#/paths/1assessment1nist1score1get` |
| Notification | `/docs/spec/nist.yaml#/paths/1registry1notifications~1get` |
| Audit Log | `/docs/spec/nist.yaml#/paths/1audit1event~1get` |

### Shared Error Envelope Schema

```plaintext
{
  error_code: string,
  message: string,
  correlationID: string
}
```

Referenced for all error overlays, event logging, and notification actions.

### Widget NFR Table

| Measure | Target |
|---|---|
| Bundle Size | <400kB |
| Panel Memory Footprint | <30MB |
| Local Cache Quota | <10MB |
| Cold Start | <1s |
| Telemetry Flush | <10s |
| Accessibility | Strict AA Conformance |

### Telemetry Schema (activity, error, feedback events)

| Field | Type | Description |
|---|---|---|
| panelId | String | Widget or panel generating the event |
| userId | String | User session/account identifier |
| eventType | String | Action/notification/error type |
| timestamp | String | ISO 8601 UTC timestamp |
| correlationID | String | Trace/audit correlation |
| error_code | String | If applicable, error envelope code |

### CI/Test Matrix Example

| Adapter/Panel | Normal Pass | 4xx/5xx | Migration | A11y | Drift | Registry Notice | Offline |
|---|---|---|---|---|---|---|---|
| Checklist | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Evidence Upload | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Reviewer Action | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Scoring Dashboard | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Notification | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| Audit Log | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

------------------------------------------------------------------------

## Pattern Library, Visuals, Policy/Config Snippets

- All critical overlays, checklist progress, migration/error states are implemented via the shared mesh pattern library.
- Visual assets:
  - Component/sequence diagram thumbnails (linked to `/docs/diagrams/v3/nist_component.png`)
  - OpenAPI pointer snapshots for evidence and scoring
  - Animated flows for migration, audit review, and notification
- Example Policy/Config Snippet (YAML):

```yaml
checklist:
  - id: MAP-1.1
    status: pending
    evidence: []
    assigned_reviewer: "compliance_officer"
dashboard:
  scoring_threshold: 3.5
  drift_notification: enabled
review:
  enable_bulk: true
```

### Layer Mapping

| Concern | Cognitive Mesh Layer |
|---|---|
| NIST Assessment & Scoring | ReasoningLayer |
| EvidenceCollect/Scoring/Progress APIs | BusinessApplications |
| Registry Alerts, Audit Logging | FoundationLayer |
| Migration & Deprecation Hooks | FoundationLayer |
