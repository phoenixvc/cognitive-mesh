---
Module: AugmentedIIdentityWidget
Primary Personas: Knowledge Workers, HR Reviewers, Chief Augmentation Officers
Core Value Proposition: Widget for managing and visualizing identity augmentation
Priority: P2
License Tier: Professional
Platform Layers: UI, Business Applications
Main Integration Points: Augmented-I backend, Dashboard system, HR platforms
---

# Augmented-I Identity Mesh Widget PRD (Hexagonal, Mesh Layered, Sharpened)

### TL;DR

Widgets operationalize the Augmented-I identity, F-Scale journey,
mindset embedding, and reckless non-augmentation detection for every
mesh user. This is accomplished through rigorously hexagonal
architecture with strict ports/adapters, comprehensive Given/When/Then
coverage for every adapter, exact OpenAPI contract references, robust
error and offline handling, and a CI-friendly test matrix.

------------------------------------------------------------------------

## Goals

### Business Goals

- Accelerate enterprise-wide adoption of Augmented-I identity and
  AI-first competency.

- Reduce lag in AI-professional self-transformation, increasing team
  competitiveness.

- Provide actionable, organization-wide augmentation analytics and risk
  detection.

- Ensure compliance, audit, and transparency for sensitive identity- and
  HR-related flows.

- Enable seamless onboarding and self-upgrade of workforce augmentation
  state.

### User Goals

- Enable every user to track their Augmented-I status and F-Scale
  position in real time.

- Surface practical next actions to accelerate journey from "Fear" to
  "Fun."

- Confidently know if AI-augmentation is sufficient for competitive
  standards.

- Receive transparent notifications and consent gates for all
  HR/identity insights.

- Achieve full accessibility, resilience (offline, retry), and
  consistent upgrade experience.

### Non-Goals

- Does not prescribe any non-AI powered self-improvement content.

- Excludes automated HR actions (e.g., hiring/firing) based solely on
  widget results.

- Not intended for anonymous/guest users—requires mesh identity.

------------------------------------------------------------------------

## User Stories

**Persona 1: Product Manager**

- As a Product Manager, I want to view my Augmented-I level, so I can
  benchmark myself and take targeted steps towards full identity
  integration.

- As a Product Manager, I want an immediate nudge or suggestion if my
  F-Scale journey is stuck (e.g., in “Familiar” stage), so that I don’t
  get left behind as others advance.

- As a Product Manager, I want notifications if my workflows are flagged
  “reckless” (not using augmentation where reasonable), so I can
  self-correct before HR/intervention.

**Persona 2: HR Director**

- As an HR Director, I want to see aggregate augmentation trends and
  risks across teams, so that I can deliver targeted upskilling or
  compliance programs.

- As an HR Director, I want every HR-surfaced recommendation or risk to
  be gated through explicit user consent and manual review, so we
  protect user rights and auditability.

**Persona 3: Individual Contributor**

- As an Individual Contributor, I want to see contextual “How can
  Augmented-I do this better?” prompts whenever I begin a new workflow,
  so I can develop AI-first reflexes.

- As an Individual Contributor, I want to be alerted if my version or
  schema is out-of-date, so I only act on valid and current data.

------------------------------------------------------------------------

## Functional Requirements

### Widget Panels & Hexagonal Adapters

- **Identity Status Panel** (Must)

  - Show user’s current Augmented-I score, F-Scale stage, and bias value
    (0.0–1.0).

  - DataAPIAdapterPort gets
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1identity-integration~1get,
    response /components/schemas/IdentityStatus.

  - ConsentAdapter must confirm user opt-in before showing any HR-risked
    attribute.

- **F-Scale Tracker Panel** (Must)

  - Live F-Scale (Fear, Familiar, Fluent, Fun) with auto-journey
    prompts.

  - DataAPIAdapterPort:
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1fscale~1get, schema
    /components/schemas/FScaleStatus.

- **Mindset Reflex Panel** (Must)

  - Auto-prompt “How can Augmented-I do this better?” and track
    last-reflex status.

  - DataAPIAdapterPort:
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1reflex~1get, schema
    /components/schemas/ReflexStatus.

- **Reckless Detection Panel** (Must)

  - Expose reckless bias score (0 – not augmented, 1 – fully augmented).

  - DataAPIAdapterPort:
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1reckless-detection~1get,
    schema /components/schemas/RecklessStatus.

  - HR/ManualReviewPort:
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1manual-review~1post,
    schema /components/schemas/ReviewRequest.

- **Org Augmentation Overview** (Should)

  - Team/Org-level distribution of bias scores, F-Scale positions, CAO
    status, and risk flags.

  - DataAPIAdapterPort:
    /docs/spec/augmented-identity.yaml#/paths/~1v1~1org~1overview~1get,
    schema /components/schemas/OrgAugmentationOverview.

- **ConsentAdapter** (Must)

  - All personal or HR/identity data requires explicit runtime consent.

  - EventAdapter, TelemetryAdapter, RendererAdapter wrap notifications,
    theme changes, a11y validation, and all state transitions.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Access through main mesh dashboard or individual profile pane.

- First launch shows Augmented-I intro modal (with onboarding
  micro-tutorials).

- Consent modal triggers before any non-personal attribute or
  HR-surfaced data is displayed.

**Core Experience**

- **Step 1:** User opens widget panel; context-init triggers
  DataAPIAdapterPort fetch.

  - Loader and retry while fetching.

  - In case of offline, persistent “Retry” overlay, cache fallback.

- **Step 2:** Identity/F-Scale/Reflex/Reckless scores are displayed,
  bias is color-mapped (0: gray/underutilized, 1.0: green/augmented).

  - User immediately sees actionable suggestions if bias < 0.5.

- **Step 3:** User clicks “Reflex Nudge” or “Help Me Improve,” opens
  prompt or in-widget action center.

- **Step 4:** If Org/Human oversight required (e.g., Reckless flagged),
  ConsentAdapter blocks until confirmation, HR/ManualReviewPanel shows
  review workflow.

- **Step 5:** On version mismatch or API deprecation, overlay disables
  widget: “Please update to vX.Y for latest results.”

**Advanced Features & Edge Cases**

- Theme switcher (dark/light/adapt) is globally honored; all panels a11y
  by default.

- RendererAdapter replaces “critical” error overlays for stale or
  mismatched schema, triggers alert/log for engineering.

- HR and Org panels always require explicit permission before rendering.

**UI/UX Highlights**

- Clear badge coloring for bias/F-Scale, with semantic icons.

- Persistent Retry overlay at all times user is offline.

- All content WCAG 2.1 AA, keyboard-nav tested, font/scaling invariant.

------------------------------------------------------------------------

## Shared UX Patterns

- **UX Conventions:**

  - Overlay styling consistent for all panels.

  - Standardized button states and transitions.

  - Identical “Retry” behavior across panels.

  - Common error banner design.

  - Uniform consent dialog presentation.

  - Consistent theme transition effects.

------------------------------------------------------------------------

## Narrative

In a rapidly evolving AI landscape, the greatest challenge facing modern
professionals isn’t technical—it’s psychological and identity-based.
Jane, an ambitious PM, often thinks of AI as just another tool and
hesitates to fully integrate it into her daily routine. Her organization
is ahead of the curve, but there’s no visibility into who is genuinely
AI-augmented and who is simply going through the motions.

With the Augmented-I Mesh Widget, Jane’s identity, mindset, and
professional reflexes are surfaced in real time. She opens her dashboard
to immediately see her “Augmented-I” score, where she sits on the
F-Scale, and whether she’s at risk of professional “recklessness” for
not using available augmentation. Practical prompts nudge her to move
from Fear to Fluent to Fun, and consent banners make it clear when HR or
review events will be triggered—all via resilient, context-aware ports
and adapters.

Within weeks, Jane isn’t just “using AI;” she is an Augmented
Professional. Her teams are measurably accelerating through the identity
transformation, her HR sees organization-wide risk indices dropping, and
her business leaps ahead—now executing at the true speed of cognitive
mesh.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- % users achieving Augmented-I bias score ≥0.8 within 3 months

- % users moving up at least one F-Scale stage/quarter

-

<!-- -->

- <3% user drop-off due to version mismatch or deprecation

### Business Metrics

- Aggregate organizational reckless risk index <0.2 after rollout

-

<!-- -->

- Employee self-reported augmentation confidence +30%

### Technical Metrics

- 99.95% uptime for all DataAPIAdapterPort and Consent flows

- 99% of consent events logged/audited with <2s lag

- <1% of API contract mismatches on live traffic

### Tracking Plan

- Event: Widget load, panel open

- Event: Identity/F-Scale/reflex update

- Event: Consent granted/denied

- Event: Retry pressed, offline state trigger

- Event: Version warning or deprecation overlay shown

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Panels map 1:1 to mesh DataAPIAdapterPorts + auxiliary Consent, Event,
  Telemetry, Renderer adapters.

- All adapters keyed to exact OpenAPI fragments.

- Modular RendererAdapter for a11y, theme, offline UI.

### Integration Points

- Tightly coupled to mesh-core orchestration and org HR/score
  dashboards.

- Links to org-wide audit and notification mesh for event signals.

### Data Storage & Privacy

- All cached data TTL 30m; immediate invalidation on version/schema bump
  or user “Refresh.”

- Consent required for all HR/identity datapaths.

- All error, consent, deprecation events have correlationIDs for audit
  and compliance.

### Scalability & Performance

- All adapters require <250ms P99 response for end-user actions.

- Org panels gracefully degrade if batch API is delayed; persistent
  overlay until resolved.

### Potential Challenges

- Preventing silent drift between widget and API schema—mandate contract
  check in CI.

- Handling manual review and consent edge cases under high load.

- Ensuring full accessibility across rapid theming/feature upgrades.

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- Medium: 2–4 weeks (lean, focused team)

### Team Size & Composition

- Small Team: 1–2 (Full-stack dev with design/UX collaborator; lean
  test/CI support)

### Suggested Phases

**Phase 1: Architecture and Adapter Stubs (1 week)**

- Deliverable: Hexagonal widget skeleton, complete DataAPIAdapterPort
  stubs, initial OpenAPI wiring.

- Responsible: Lead engineer

- Dependencies: Mesh API OpenAPI specs, theme tokens

**Phase 2: Panel Implementation & Contracts (1 week)**

- Deliverable: Identity, F-Scale, Mindset, Reckless, Org Overview panels
  live; Consent & HR adapters fully CI tested.

- Responsible: Full-stack dev, UX

- Dependencies: Backend test endpoints; Storybook/mock harness

**Phase 3: CI Harness, Error/Offline, and A11y (1 week)**

- Deliverable: Persistent Retry UI, error overlays, full accessibility
  sweep; full Storybook & Cypress suite; contract mismatch/compatibility
  warnings.

- Responsible: Test/QA, dev

- Dependencies: CI pipeline, mesh shell registry

**Phase 4: Diagrams, Launch, and Feedback (1 week)**

- Deliverable: Diagrams embedded, feedback loop open; registry
  documentation and guided launch toolkit.

- Responsible: All

- Dependencies: /docs/diagrams/augmented-i/; registry/launch infra

------------------------------------------------------------------------

## 1. Core Widgets, Storybook Stories, and Mesh Adapters

- **Identity Status Panel**: Shows { augmented_i_level, f_scale, bias },
  requires ConsentAdapter on first view.

- **F-Scale Tracker**: Full journey with context-specific prompts.

- **Mindset Reflex Panel**: Reflex nudge functionality with real usage
  logging.

- **Reckless Detection Panel**: Exposes reckless bias (0–1); manual
  review and HR escalation if high risk.

- **Org Augmentation Overview**: Team/org level for HR; only visible
  with org-level consent.

- **Adapters**: DataAPIAdapterPort, ConsentAdapter, TelemetryAdapter,
  EventAdapter, HR/ManualReviewPort, RendererAdapter.

- **Storybook Scenarios**: Each panel tested for: pass, fail,
  stale-cache, version-mismatch, offline, theme-change, a11y-violation,
  consent denied, HR/manual review, deprecated call overlay.

------------------------------------------------------------------------

## 2. MoSCoW + G/W/T for Every Adapter/Panel

**DataAPIAdapterPort (Identity Status)**

- Must: Given user online with valid token, When user opens widget, Then
  GET
  /docs/spec/augmented-identity.yaml#/paths/~1v1~1identity-integration~1get
  and render data.

- Must: Given user is offline, When user opens widget, Then show “Retry”
  overlay and use cached data if less than TTL.

- Must: Given response is error 422, When API returns, Then show
  friendly error, overlay, and unique correlationID in logs.

**ConsentAdapter**

- Must: Given HR-sensitive data requested, When consent not yet given,
  Then block panel, show consent modal, and do not call backend.

- Must: Given user denies consent, When widget retries, Then prevent API
  call and log correlationID.

**TelemetryAdapter**

- Must: Given widget panel loaded, When API responds, Then log load
  event and status.

- Should: Given fail/error, When error is displayed, Then emit telemetry
  with error_code and user context.

**EventAdapter**

- Must: Given F-Scale nudge clicked, When user acts, Then post event
  /docs/spec/augmented-identity.yaml#/paths/~1v1~1event~1post and show
  confirmation toast.

**HR/ManualReviewPort**

- Must: Given reckless risk >0.8, When detection is triggered, Then
  submit tokenized review to
  /docs/spec/augmented-identity.yaml#/paths/~1v1~1manual-review~1post;
  block escalation until review complete.

**RendererAdapter**

- Must: Given version/schema mismatch, When render attempted, Then block
  UI and show “Please update” overlay.

**Error/Offline**

- Must: Given API 5xx, When retry limit reached, Then queue in batch
  (offline) and alert user.

------------------------------------------------------------------------

## 3. API Contracts (OpenAPI Pointers)

- **IdentityIntegration Get:**

  /docs/spec/augmented-identity.yaml#/paths/~1v1~1identity-integration~1get

  Response:
  /docs/spec/augmented-identity.yaml#/components/schemas/IdentityStatus

- **F-Scale Get:**

  /docs/spec/augmented-identity.yaml#/paths/~1v1~1fscale~1get

  Response:
  /docs/spec/augmented-identity.yaml#/components/schemas/FScaleStatus

- **Reflex Get:**

  /docs/spec/augmented-identity.yaml#/paths/~1v1~1reflex~1get

  Response:
  /docs/spec/augmented-identity.yaml#/components/schemas/ReflexStatus

- **Reckless Detection Get:**

  /docs/spec/augmented-identity.yaml#/paths/~1v1~1reckless-detection~1get

  Response:
  /docs/spec/augmented-identity.yaml#/components/schemas/RecklessStatus

- **Manual Review Post:**

  /docs/spec/augmented-identity.yaml#/paths/~1v1~1manual-review~1post

  Response:
  /docs/spec/augmented-identity.yaml#/components/schemas/ReviewRequest

- **All contracts verified via CI at release; incompatible widgets
  auto-blocked.**

------------------------------------------------------------------------

## 4. Error Envelope, Retry Policy, Offline, and Caching

- **Error Envelope**: Every API/Adapter returns { error_code, message,
  correlationID }

- **Retry Policy**: initial 200ms, doubles with jitter ±50ms up to 3
  attempts. After max retries, event queued for offline batch.

- **Offline Handling**: Persistent Retry UI for all failed panels;
  fallback to last good cache ≤30m old, else “Cannot load – check
  connection.”

- **Caching**: TTL 30min, invalidation on user refresh, API
  version/schema change, or mismatch detected.

------------------------------------------------------------------------

## 5. Test Harness Matrix

**Scenarios:**

- Pass, fail

- Stale-cache

- Version-mismatch

- Offline

- Theme-change

- a11y-violation

- Consent denied

- HR/manual review

- Deprecated call overlay

------------------------------------------------------------------------

## 6. Diagrams: Component & Sequence Flow

- **Component Diagram:**

  ![Component Diagram
  Thumbnail](/docs/diagrams/augmented-i/component.svg)

- **Sequence Diagram:**

  ![Sequence Diagram Thumbnail](/docs/diagrams/augmented-i/sequence.svg)

  *Detailed views are located in* /docs/diagrams/augmented-i/*.*

------------------------------------------------------------------------

## 7. Versioning, Compatibility, Deprecation

- **Versioning:**

  - All widgets semver’ed. On contract change, shell compares declared
    contract in widget vs API:

    - If mismatched, disable widget, show “Please update to vX.Y”.

    - Contract compliance checked at CI/publish time; incompatible
      widgets fail registry approval.

- **Deprecation & Sunset:**

  - Old widgets sunset after 2 major versions or 90 days, whichever
    comes first.

  - Deprecation workflow: PR/issue labeled ‘deprecation,’ send update
    notice to consumers, overlay disables widget after window closes.

- **Migration:**

  - Registry and release notes track breaking changes, with migration
    matrix for all org integrators.

- **Data Contract Evolution:**

  - Additive changes preferred; deprecated fields tagged in schema,
    warning overlay and notification prior to removal.

  - Client notification on breaking schema changes, mandatory registry
    re-approval.

------------------------------------------------------------------------

## 8. Service-Specific Non-Functional Requirements

- Must: memory budget <20MB.

- Max bundle size 300kB.

- Cold start <1s.

- Telemetry batch flush <10s.

------------------------------------------------------------------------

## 9. Accessibility Failure Handling

- If any panel fails critical accessibility check in production, widget
  auto-disables, logs error to TelemetryPort with user context and
  renders a dismissible 'Accessibility Issue' banner for user awareness;
  also triggers internal alert for fix.

------------------------------------------------------------------------

## 10. Telemetry & Analytics - Event Schema Appendix

### Event Fields:

- **timestamp** (ISO)

- **panelId**

- **userId**

- **correlationID**

- **error_code**

- **action**

- **widgetVersion**

- Required fields for all compliance/analytics events.

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh UI components** rather than introducing an entirely new stack. The table and notes below map new widgets, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| New Component | Action Required | Integration Point(s) |
|---|---|---|
| **Identity Status Panel**<br>**F-Scale Tracker Panel**<br>**Mindset Reflex Panel**<br>**Reckless Detection Panel**<br>**Org Augmentation Overview** | • Create new React components for each widget (`src/UILayer/AgencyWidgets/Panels/`).<br>• Register each via `IWidgetRegistry.RegisterWidgetAsync()` with a unique `WidgetDefinition`. | • `IWidgetRegistry` (`src/UILayer/Core/WidgetRegistry.cs`) to make them available in the dashboard.<br>• `DashboardLayoutService` (`src/UILayer/Services/DashboardLayoutService.cs`) to manage their position and state.<br>• Rendered by the main dashboard shell. |
| **WidgetLifecyclePort** (NEW Interface) | • Define a new TypeScript interface (`src/UILayer/AgencyWidgets/Adapters/IWidgetLifecycleAdapters.ts`) for widget lifecycle events (e.g., `onInit`, `onContextUpdate`, `onDestroy`). | • The main dashboard shell will implement this port to pass context and lifecycle events to each widget instance. |
| **DataAPIAdapterPort** (UPDATE) | • Extend the existing `IDataAPIAdapterPort` to include methods for calling the new backend endpoints (`/v1/identity-integration/get`, `/v1/fscale/get`, `/v1/reckless-detection/get`).<br>• Implement the adapter to consume the backend's OpenAPI spec. | • `IAgencyWidgetAdapters.cs` (or a new `.ts` file).<br>• Consumes endpoints from the new backend services.<br>• Uses standardized `ErrorEnvelope` model. |
| **ConsentAdapter** (UPDATE) | • Extend the existing `IConsentAdapter` to handle consent flows specific to identity/HR diagnostics.<br>• The consent request must be triggered before calling any HR-sensitive endpoint. | • `IConsentAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Triggers the shared `Consent/Notification Modal` component. |
| **TelemetryAdapter** (UPDATE) | • Extend the existing `ITelemetryAdapter` to log new event types: `AugmentedIStatusViewed`, `FScaleUpdated`, `RecklessDetectionAcknowledged`. | • `ITelemetryAdapter` (`src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs`).<br>• Uses the `TelemetryEvent` model (`src/UILayer/AgencyWidgets/Models/AgencyWidgetModels.cs`). |
| **Shared UX Pattern Library** (UPDATE) | • Add new shared components for the F-Scale journey visualization, mindset reflex prompts, and reckless detection banners. | • Resides in `src/UILayer/AgencyWidgets/Components/`.<br>• Ensures visual and functional consistency across all widgets. |

### Summary of File-Level Changes

*   **New Files:**
    *   `src/UILayer/AgencyWidgets/Panels/IdentityStatusPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/FScaleTrackerPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/MindsetReflexPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/RecklessDetectionPanel.tsx`
    *   `src/UILayer/AgencyWidgets/Panels/OrgAugmentationOverview.tsx`
    *   `src/UILayer/AgencyWidgets/Adapters/IWidgetLifecycleAdapters.ts` (or similar for new ports)

*   **Updated Files:**
    *   `src/UILayer/AgencyWidgets/Adapters/IAgencyWidgetAdapters.cs` (or `.ts` equivalent): Update `IDataAPIAdapterPort`, `IConsentAdapter`, and `ITelemetryAdapter`.
    *   `src/UILayer/AgencyWidgets/Components/`: Add new shared UI components for identity/F-Scale visualizations.
    *   **Dashboard Shell Component**: Update to implement `IWidgetLifecyclePort` and pass events to widgets.
    *   **OpenAPI Client**: Regenerate to include new backend endpoints for Augmented-I.

No other layers require structural change; the widget system integrates cleanly with the existing UI shell and backend APIs via the adapter pattern.
