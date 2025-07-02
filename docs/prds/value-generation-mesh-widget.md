# Value Generation Mesh Widget PRD (Hexagonal, Mesh Layered, Tightened)

### TL;DR

Widgets surface actionable value, creativity, blindness, and employability diagnostics for mesh users/orgs. They connect directly to backend ports with strict MoSCoW prioritization, comprehensive Given/When/Then testing, OpenAPI contract linkage, shared overlays, and CI/telemetry compliance.

------------------------------------------------------------------------

## Goals

### Business Goals

- Empower organizations to detect, diagnose, and act on hidden value and employability opportunities/risks.
- Reduce "organizational blindness" through transparent, accessible dashboards.
- Ensure regulatory-grade auditability and compliant UI interactions in all HR-sensitive flows.
- Launch a robust mesh widget suite to drive adoption/usage in enterprise environments.
- Maintain rapid iteration without drift or UX inconsistency.

### User Goals

- Instantly access clear diagnostics on personal/organizational value, creativity, and employability.
- Receive actionable insights and nudges with minimal friction or technical know-how.
- Experience a visually consistent and accessible interface across all value-focused panels.
- Always retain visibility and control over consent and privacy status on employability flows.

### Non-Goals

- No automated employment decision-making—widgets only deliver diagnostics, not final HR actions.
- No DIY custom widgets/plugins outside the vetted suite in this release.
- No support for regional legal compliance management beyond defined locales (EU, US, as specified).

------------------------------------------------------------------------

## User Stories

### HR Manager

- As an HR Manager, I want to view organization-wide value blind spots, so that I can address areas that hinder performance or growth.
- As an HR Manager, I want automated alerts if employability risk is detected, so that I can intervene proactively.

### Employee/User

- As an employee, I want to see my personal value and creativity diagnostic, so that I can understand how I’m perceived and where I can develop.
- As an employee, I want to consent before submitting employability information, so that I control my sensitive data.

### Compliance Officer

- As a compliance officer, I want every diagnostic and consent event logged, so that audits and reviews are always possible.
- As a compliance officer, I want the system to flag and prevent actions if any accessibility or compliance criteria are breached.

------------------------------------------------------------------------

## Functional Requirements

- **Value Diagnostic Panels** (Priority: Must)

  - $200 Test Widget: Presents quick, actionable personal value score.
  - Value Diagnostic Dashboard: Trends, scores, and historical data for individuals and organizations.
  - Org Blindness Trends: Panel aggregating and visualizing organizational value blindness.

- **Employability Panel** (Priority: Must)

  - Employability Score Widget: Shows employability/risk metrics per user (requires ConsentAdapter for data access).

- **Consent and Notification Adapters** (Priority: Must)

  - Consent Overlay: Modal for explicit opt-in, must block widget actions if declined.
  - Notification Banner: Real-time alerts, success/error overlays tied to key events (diagnostics, consent, failure, or update).

- **Telemetry and Theme Adapters** (Priority: Should)

  - TelemetryAdapter: All user actions, errors, state transitions logged and sent per schema.
  - ThemeAdapter: Ensures UI context (dark/light, accessibility modes) propagates to all widgets/panels.

- **DataAPIAdapterPort** (Priority: Must)

  - REST/gRPC interface to all backend diagnostic ports; contract locked to OpenAPI spec with versioning and schema enforcement.

- **Shared Overlay/Pattern Library** (Priority: Must)

  - All error, retry, consent, notification, and a11y overlays follow a single design, reviewed/approved once for all panels.

------------------------------------------------------------------------

## User Experience

### Entry Point & First-Time User Experience

- Users access the dashboard via enterprise shell or deep link.
- On first access, quick onboarding wizard highlights value, employability, blindness features.
- Consent overlay is shown on any employability or personally-identifying data submission.
- Widgets explain in plain language what data is used and how, before running new diagnostics.

### Core Experience

- **Step 1:** User selects desired panel (e.g., $200 Test, Blindness Trends).
  - Panels display data from backend via DataAPIAdapterPort, respecting caching (30 min TTL).
  - If cache is stale, user can manual refresh (with progress indication).
- **Step 2:** Widget fetches current data, shows loading spinner.
  - Success: Results, insights, and trends display with clear, actionable recommendations.
  - Error (schema, network, auth): Shared overlay with {error_code, message, correlationID}, persistent Retry option.
- **Step 3:** Employability Panel triggers consent if needed.
  - If consent given, fetch and display; if denied or missing, show overlay and disable actions.
- **Step 4:** All panels surface notification banners for key events (success, error, update, deprecation).
- **Step 5:** User can navigate between panels, all of which inherit theme and a11y settings.

### Advanced Features & Edge Cases

- Offline: Widgets degrade gracefully; overlay prompts Retry or notify for later.
- Version Drift: Detects backend/frontend schema mismatch; disables widget and displays migration/upgrade overlay.
- Accessibility: If any a11y rule fails, the affected panel disables, logs telemetry, and surfaces an overlay to inform the user.
- Deprecation/Sunset: Overlay and registry notification when widget/API is within 60/30/10 days of deprecation.

### UI/UX Highlights

- Central shared overlay library ensures error, consent, retry, and notification banners behave and appear identically.
- Color contrast and sizing meet WCAG AA; all controls are keyboard and screen-reader accessible.
- Every widget adjusts to team/org color schemes and supports dark/light themes.

------------------------------------------------------------------------

## Narrative

Imagine Sofia, an HR manager at a fast-moving technology firm, struggling to identify under-the-radar talent and looming employability risks. Her organization runs periodic engagement surveys, but the results come back late and lack actionable depth. With the Value Generation Mesh Widget suite, Sofia logs into her dashboard and—without hunting through reports—receives instant, clearly visualized diagnostics of which teams excel, where hidden value lies untapped, and which departments might be drifting into organizational blindness.

For employees like Alex, the experience is just as empowering: he checks his personal “$200 Value Test,” discovers how his creativity is currently perceived, and receives a nudge about learning opportunities. When prompted for employability diagnostics, he sees a clear consent modal and can opt in (or out), knowing that his data won’t move without his approval.

Meanwhile, Tracy in Compliance reviews the robust logs: every diagnostic, consent, or employment-sensitive call is archived, all with retrievable correlation IDs and accessible notifications if errors, exceptions, or a11y issues ever arise. With consistent overlays and zero guesswork, the mesh widget suite transforms value detection from a tedious compliance box into a strategic, user-centric superpower.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- Widget DAU/WAU/MAU per panel and organization
- User satisfaction (CSAT) with diagnostics/overlays
- Consent completion rate on employability flows
- Time to actionable insight (user-perceived latency)

### Business Metrics

- Organizational adoption rate (number of orgs onboarded per quarter)
- Reduction in “value blindness” incidents (self-reported or flagged via panel)
- Audit pass rate on compliance/a11y checks

### Technical Metrics

- Average widget bundle size below 300kB
- Panel cold start within 1 second
- 95th percentile API response & overlay render <300ms
- Error overlay activation rate below 1%

### Tracking Plan

- Diagnostic request & panel load
- Consent shown/granted/denied events
- Error/retry overlay appearances
- Notification/banner click-throughs
- API version/migration overlay displays
- a11y overlay activations

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Frontend panels built as modular mesh widgets, each talking to backend via DataAPIAdapterPort (REST/gRPC, OpenAPI enforced)
- Consent, notification, telemetry, and theme handled via adapters for plug-and-play across panels
- Strict adherence to OpenAPI and event schema for every data call and telemetry record
- Storybook/Cypress bi-directional coverage (screenshots, overlay snapshots, event simulation)

### Integration Points

- Mesh platform shell for widget hosting, theme, and shell-wide notification hooks
- Auth layer for Single Sign-On, user/org context
- Backend diagnostic ports: ValueDiagnostic, OrgBlindnessDetection, Employability, Consent, Notification, Audit

### Data Storage & Privacy

- Telemetry routed backend via TelemetryAdapter, no PII stored in the browser
- ConsentAdapter manages explicit opt-in for employability data
- All interactions, consent, and error events logged with correlationID (for HR/compliance retrieval)
- Compliance: Follows defined locale (EU/US) rules for sensitive workflows

### Scalability & Performance

- Optimized for hundreds of concurrent widgets per org, batch telemetry upload every 10s
- Widget bundle ≤300kB; memory ≈20MB max per widget in browser
- Supports lean/fallback mode for legacy browsers

### Potential Challenges

- Backend schema/version drift
- Accessibility testing and maintaining AA/AAA compliance
- Drifts in UX/overlay pattern library consistency among rapid feature revs

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- Medium: 3–5 weeks (including design, storybook, integration, and full CI/test matrix)

### Team Size & Composition

- Small Team: 1–2 (frontend+product) for widgets, with shared oversight from backend, QA, and design

### Suggested Phases

1.  **Mesh Widget/Overlay Foundation (1 week)**

    - Deliverables: Core overlay, consent, notification, theme adapters
    - Dependencies: Mesh shell, backend API schema

2.  **Panel Implementation & Storybook Integration (1.5 weeks)**

    - Deliverables: $200 Test, Value Diagnostic, Org Blindness, Employability Score panels
    - Dependencies: Overlay/component library, OpenAPI validation

3.  **Test Matrix, Accessibility, Compliance (1 week)**

    - Deliverables: Cypress/Storybook coverage, a11y failover overlays
    - Dependencies: Real/test data access, CI pipeline

4.  **CI/CD & Deprecation Framework (0.5 week)**

    - Deliverables: Registry integration, version gating, deprecation/overlay triggers
    - Dependencies: All prior phases, contract/test stability

5.  **Stakeholder Demo & Launch (1 week)**

    - Deliverables: UAT, onboarding docs, stakeholder review, rollout comms
    - Dependencies: Passed test matrix, user feedback

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh UI components** rather than introducing an entirely new stack. The table and notes below map new widgets, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| New Component | Action Required | Integration Point(s) |
|---|---|---|
| **Value Diagnostic Panels**<br>**Employability Panel** | • Create new React components for each widget (`src/UILayer/AgencyWidgets/Panels/`).<br>• Register each via `IWidgetRegistry.RegisterWidgetAsync()` with a unique `WidgetDefinition`. | • `IWidgetRegistry` (`src/UILayer/Core/WidgetRegistry.cs`) to make them available in the dashboard.<br>• `DashboardLayoutService` (`src/UILayer/Services/DashboardLayoutService.cs`) to manage their position and state.<br>• Rendered by the main dashboard shell. |
| **DataAPIAdapterPort** (UPDATE) | • Extend the existing `IDataAPIAdapterPort` to include methods for calling the new backend endpoints (`/v1/value-diagnostic`, `/v1/org-blindness`, `/v1/employability`).<br>• Implement the adapter to consume the backend's OpenAPI spec. | • `IAgencyWidgetAdapters.cs` (or a new `.ts` file).<br>• Consumes endpoints from the new `ValueGenerationController`.<br>• Uses standardized `ErrorEnvelope` model. |
| **ConsentAdapter** (UPDATE) | • Extend the existing `IConsentAdapter` to handle consent flows specific to employability diagnostics.<br>• The consent request must be triggered before calling the employability endpoint. | • `IConsentAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Triggers the shared `Consent/Notification Modal` component. |
| **NotificationAdapter** (UPDATE) | • Extend the existing `INotificationAdapter` to handle new notification types for high-severity diagnostics and manual adjudication requests. | • `INotificationAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Displays alerts using the shared notification banner component. |
| **TelemetryAdapter** (UPDATE) | • Extend the existing `ITelemetryAdapter` to log new event types: `ValueDiagnosticViewed`, `EmployabilityConsentGranted`, `OrgBlindnessReportGenerated`. | • `ITelemetryAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Uses the `TelemetryEvent` model (`src/UILayer/AgencyWidgets/Models/AgencyWidgetModels.cs`). |
| **Shared UX Pattern Library** (UPDATE) | • Add new shared components for value diagnostics displays, risk score visualizations, and the `$200 Test` widget UI. | • Resides in `src/UILayer/AgencyWidgets/Components/`.<br>• Ensures visual and functional consistency across all widgets. |

### Summary of File-Level Changes

*   **New Files:**
    *   `src/UILayer/AgencyWidgets/Panels/ValueDiagnosticDashboard.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/OrgBlindnessTrends.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/EmployabilityScoreWidget.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/TwoHundredDollarTestWidget.tsx`

*   **Updated Files:**
    *   `src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs` (or `.ts` equivalent): Update `IDataAPIAdapterPort`, `IConsentAdapter`, `INotificationAdapter`, and `ITelemetryAdapter`.
    *   `src/UILayer/AgencyWidgets/Components/`: Add new shared UI components for diagnostic visualizations.
    *   **Dashboard Shell Component**: Update to register the new widgets with the `IWidgetRegistry`.
    *   **OpenAPI Client**: Regenerate to include new backend endpoints for value generation.

No other layers require structural change; the widget system integrates cleanly with the existing UI shell and backend APIs via the adapter pattern.
