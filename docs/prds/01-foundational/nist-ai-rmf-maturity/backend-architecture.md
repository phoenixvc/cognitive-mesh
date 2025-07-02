# Cognitive Mesh NIST AI RMF Maturity Framework Backend PRD (Hexagonal, Mesh Layered, Testable)

### TL;DR

Backend implements the complete NIST AI RMF maturity model—66 statements as modular, auditable mesh-native services and adapters. Includes evidence submission, real-time scoring, dashboards, strict G/W/T contracts with OpenAPI pointers, and a fully versioned, compliance-aligned maturity platform for enterprise AI systems.

------------------------------------------------------------------------

## Goals

### Business Goals

- Achieve industry-leading, verifiable NIST AI RMF compliance across all key AI risk domains
- Provide an auditable, modular backend for evidence-based maturity scoring and compliance dashboards
- Ensure rapid regulatory response: proactive notifications, fast migration, and downtime prevention
- Enable continuous improvement and targeted gap closure on all maturity dimensions

### User Goals

- Allow users and auditors to submit, review, and verify compliance evidence for every NIST statement
- Empower organizations to track and improve their NIST RMF maturity in real time
- Deliver clear feedback and actionable improvement plans to business and technical stakeholders
- Enable QA and compliance teams to quickly identify, reproduce, and resolve gaps or audit failures

### Non-Goals

- This system does not automate policy writing or external legal compliance outside NIST scope
- Does not provide a graphical user interface (covered by the widget/frontend PRD)
- Does not directly generate interpretive legal opinions

------------------------------------------------------------------------

## User Stories

**Persona: Compliance Program Manager**

- As a Compliance Manager, I want to upload and tag compliance evidence, so that I can prove fulfillment of a NIST RMF requirement.
- As a Compliance Manager, I want to see the current maturity score by pillar, so I can prioritize improvements.

**Persona: Auditor**

- As an Auditor, I want to view evidence submissions and their audit logs, so that I can verify traceability and integrity.
- As an Auditor, I want to download the full compliance trail for regulatory review.

**Persona: QA Engineer**

- As a QA Engineer, I want to run automated tests for every endpoint and adapter under normal, error, drift, override, offline, and a11y conditions, so I can ensure full coverage.

**Persona: Head of Engineering**

- As Head of Engineering, I want to monitor SLAs on evidence/scoring latency and storage, so I can ensure the system meets resource and performance targets.
- As Head of Engineering, I want to receive notifications about endpoints that require migration or adaptation, so I can block rollouts until acknowledged.

------------------------------------------------------------------------

## Functional Requirements

- **NIST Maturity Services (High Priority)**

  - `NISTMaturityAssessment Engine`: Exposes endpoints for each NIST topic/statement; computes and aggregates pillar/dimension scores.
  - `EvidenceCollector`: Accepts, logs, and tags submitted artifacts, enforces format/size constraints, assigns reviewer, maintains audit trail.
  - `ScoringEngine`: Calculates statement-level scores (1-5), applies scoring rubric, returns explanatory feedback and recommendations.
  - `ProgressTracker`: Maps improvement trajectory across all statements and topics.
  - `NISTEnhancedCognitiveMesh Integration Adapter`: Aggregates all NIST-related services for external/internal mesh calls.

- **Notification & Compliance (High Priority)**

  - `RegistryHandler`: Records all API/contract changes and migration requirements; auto-generates registry/email/Slack notifications.
  - `Migration Enforcer`: Disables non-migrated endpoints after a 30-day window, overlays guidance, and blocks further contract changes until acknowledgment.
  - `AuditLogger`: Fully records all update and access events.

- **API Contracts & Error Handling (High Priority)**

  - All endpoints expose strict OpenAPI contracts; every operation is mapped by JSON pointer and has a Shared Error Envelope ({ error_code, message, correlationID }).

- **Testability & CI Harness (High Priority)**

  - For each port/adapter, enforce complete G/W/T specification and automate coverage for all states (normal, 4xx/5xx, drift, override, offline, a11y).
  - Provide scenario examples, harness results, and real-time CI gating.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Users discover endpoints via API documentation with strict OpenAPI pointers.
- On first use, organizations must register with the `RegistryHandler` and obtain access credentials.
- Evidence submission endpoints guide users through required fields and file format/size validations.

**Core Experience**

- **Step 1: Submit Evidence via `EvidenceCollector`**
  - User submits artifact (file, URL, text) tagged to a specific NIST statement/topic.
  - Artifact is validated (<10MB, required fields); on error, error envelope is returned and registry is alerted.
  - Success: Unique id and timestamp are assigned, reviewer (human/auto) assigned, audit log entry created.
- **Step 2: `ScoringEngine` Processing**
  - Artifacts are routed for scoring and context-aware rubric application (1-5).
  - Statement score, pillar/dimension aggregation, and feedback are returned alongside evidence/Audit IDs.
- **Step 3: Monitor Improvement via `ProgressTracker`**
  - Dashboards receive real-time updates of coverage, scores, and active gaps.
  - Users can filter and view maturity trajectory by pillar, topic, timeframe.
- **Step 4: Change/Audit Events**
  - Any underlying contract/endpoint change triggers notifications (registry banner, email, Slack alert).
  - On migration, affected endpoints are disabled after 30 days if not acknowledged.

**Advanced Features & Edge Cases**

- **Overrides:** Admins can trigger manual scoring or rollback for audited evidence; all such events are logged.
- **Drift Handling:** When scoring or evidence format changes, clients are notified with required migration details.
- **Offline mode:** Evidence submissions are rejected with `error_code` and guidance to retry.

**UI/UX Highlights**

- All actions are transparent, audit-trailed, and reference strict OpenAPI locations.
- Error envelopes and success/failure feedback are designed for machine parsing and automation.
- Full A11y compliance for external inspection/monitoring clients.

------------------------------------------------------------------------

## Narrative

In today's fast-evolving AI landscape, regulatory and business risk is a constant concern for enterprise teams. Hans leads a global organization pursuing state-of-the-art cognitive architectures. However, his teams struggle to systematically prove compliance, address audit gaps, and track progress. With the Cognitive Mesh NIST AI RMF Maturity Framework Backend, Hans can break the cycle: compliance teams upload digital evidence for every statement, see their maturity in live dashboards, and receive notifications when frameworks or endpoints evolve. SLAs and error handling are precise. All improvements—whether in business logic, risk modeling, or governance—are logged and tracked. The outcome? Hans’ organization outpaces the competition with an always-ready, future-proof compliance platform, full stakeholder and auditor confidence, and audit trails that make regulatory review a breeze.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- Evidence upload and scoring cycle time <2 minutes for 95% of cases
- Zero unacknowledged migrations at any time
- User (compliance/auditor) satisfaction score >90% in quarterly surveys

### Business Metrics

- Year-over-year increase in average pillar/dimension maturity score by +0.5 points
- 100% audit pass rate with no critical findings due to evidence gaps
- 24-hour migration response time for major contract changes

### Technical Metrics

- 100% test/CI scenario pass for every adapter in every state
- `EvidenceCollector`/`ScoringEngine` latency P99 <500ms per call
- No memory constraints or core service outages under simulated load

### Tracking Plan

- Evidence submissions, scoring completions, audit log entries
- Migration notifications, acknowledgment timestamps
- API error/exception counts, scenario/harness coverage stats

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Modular mesh adapters: `NISTMaturityAssessment`, `EvidenceCollector`, `ProgressTracker`, `ScoringEngine`, `Integration Adapter`
- REST APIs, OpenAPI 3.1 definitions, strict schema validation
- Evidence file/object store, audit log DB, registry/notification pub-sub
- Automated test runner for G/W/T scenarios

### Integration Points

- **Security & Zero-Trust Framework:**
  - **SSO/IDP:** For authenticated and authorized access to all API endpoints.
  - **Secrets Management:** Secure handling of API keys needed to communicate between services.
  - **Encryption:** All uploaded evidence and artifacts must be encrypted at rest using the framework's specified standards (e.g., AES-256).
- **Ethical & Legal Compliance Framework:**
  - **Audit Trail:** The `AuditLogger` must use the foundational `AuditLoggingAdapter` to ensure all actions are immutably logged and meet regulatory requirements (e.g., GDPR, EU AI Act).
  - **Data Handling:** Evidence containing PII or sensitive information must be handled according to policies defined in the legal framework, including data retention and redaction rules.
- **External Systems:**
  - Registry/Notification infrastructure for downstream consumers.
  - Audit and reporting dashboard systems.
  - CI/CD gating and contract migration handler.

### Data Storage & Privacy

- **Evidence:** Stored with artifact type, timestamps, reviewer notes, versioning, and audit log.
- **Audit logs:** Immutable, full lifecycle traceable.
- **Compliance:** Data handling must be GDPR/PII compliant. Only necessary business data for evidence is retained.

### Scalability & Performance

- **Expected load:** Up to 1M evidence uploads/day. Auto-scale adapters and evidence store.
- **Latency:** All critical endpoints SLA <500ms P99.
- **File-size:** Uploads capped at 10MB/artifact; API payloads <200kB.

### Potential Challenges

- Coordinating rapid policy/statement changes; enforcing timely migration/notification.
- Handling evidence file storage at scale.
- Allowing extensibility for evolving RMF standards and future regulatory overlays.

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- **Medium Team:** 3–5 people for initial MVP; 4–6 weeks typical for functional compliance and first full audit cycle.

### Team Size & Composition

- Product owner (1), Backend engineer (2), QA/automation (1), DevOps (0.5), Compliance analyst input (as needed)

### Suggested Phases

**Phase 1: API & Adapter Development (2 weeks)**

- **Deliverables:** `NISTMaturityAssessment`, `EvidenceCollector`, `ScoringEngine`; OpenAPI definitions, error schema, test harness skeleton.
- **Dependencies:** Confirm registry/notification system, audit log DB.

**Phase 2: Audit/Scoring Progress Dashboard & Migration Engine (2 weeks)**

- **Deliverables:** `ProgressTracker`, registry integration, notification/migration overlay logic, initial dashboards.
- **Dependencies:** Complete artifact and audit DB integration.

**Phase 3: CI Harness, Scenario Mapping, and Success Metrics (1 week)**

- **Deliverables:** Full test matrix, G/W/T enforcement, tracking/metrics implementation, final performance tuning.
- **Dependencies:** All prior adapters and registry notification in place.

------------------------------------------------------------------------

## Architecture Integration & Component Mapping

The functionality in this PRD extends **existing cognitive-mesh components** rather than introducing an entirely new stack. The table and notes below map new engines, ports, and adapters to the current layered hexagonal architecture, calling out *exact* files or interfaces that must be created or amended.

| Layer | New / Updated Component | Action Required | Integration Point(s) |
|---|---|---|---|
| **ReasoningLayer** | `NISTMaturityAssessmentEngine` (NEW) | • Add pure-domain engine (`src/ReasoningLayer/NIST/Engines/NISTMaturityAssessmentEngine.cs`).<br>• Expose via **new port** `INISTMaturityAssessmentPort`. | Consumed by BusinessApplications API adapter. |
| **BusinessApplications** | `NISTController` (NEW) | • Create a new controller to route to the new ports.<br>• Add OpenAPI paths: `/v1/assessment/nist/checklist`, `/v1/governance/nist/evidence/submit`, `/v1/governance/nist/review/submit`, `/v1/assessment/nist/score`. | OpenAPI file `docs/openapi.yaml` – add/modify schemas & operations. |
| | `EvidenceCollector` (NEW) | • Implement a new service (`src/BusinessApplications/NIST/Services/EvidenceCollector.cs`) to handle evidence submission, tagging, and reviewer assignment. | Integrates with `INISTMaturityAssessmentPort` and `AuditLoggingAdapter`. |
| | `ScoringEngine` (NEW) | • Implement a new service (`src/BusinessApplications/NIST/Services/ScoringEngine.cs`) to calculate maturity scores based on evidence and rubric. | Consumes data from `EvidenceCollector` and `NISTMaturityAssessmentEngine`. |
| | `ProgressTracker` (NEW) | • Implement a new service (`src/BusinessApplications/NIST/Services/ProgressTracker.cs`) to map and visualize improvement trajectory. | Consumes data from `ScoringEngine` and `NISTMaturityAssessmentEngine`. |
| **FoundationLayer** | `AuditLoggingAdapter` (UPDATE) | • Add new event types: `NISTAssessmentSubmitted`, `EvidenceUploaded`, `MaturityScoreCalculated`, `ReviewerAssigned`, `NISTComplianceAlert`.<br>• Ensure searchable ≤ 1s SLA. | Existing audit DB + event ingestion queue. |
| | `NotificationAdapter` (UPDATE) | • Broadcast NIST-related notifications (e.g., `ReviewerAssigned`, `MaturityDriftDetected`, `DeprecationNotice`) to the widget bus. | Mesh event bus (`Topic: nist-rmf-events`). |
| | `NISTEvidenceRepository` (NEW) | • Implement a new repository for persisting `NISTEvidence` and `NISTAssessment` entities. | • New EF Core configuration and repository in `src/FoundationLayer/ConvenerData/`. |
| **AgencyLayer** | `MigrationEnforcer` (NEW) | • Implement a new agent (`src/AgencyLayer/NISTAgents/MigrationEnforcer.cs`) to disable non-migrated endpoints and enforce contract changes. | • Triggered by `RegistryHandler` notifications.<br>• Integrates with `MultiAgentOrchestrationEngine`. |
| **MetacognitiveLayer** | `ComplianceMonitor` (NEW) | • Implement a new service (`src/MetacognitiveLayer/NIST/Monitors/ComplianceMonitor.cs`) to detect and alert on compliance drift or policy violations. | • Subscribes to `AuditLoggingAdapter` events.<br>• Integrates with `NotificationAdapter` for alerts. |

### Required OpenAPI Specification Updates

1.  **Paths added:**
    *   `POST /v1/assessment/nist/checklist`
    *   `POST /v1/governance/nist/evidence/submit`
    *   `POST /v1/governance/nist/review/submit`
    *   `GET /v1/assessment/nist/score`
    *   `GET /v1/assessment/nist/roadmap`
    *   `GET /v1/governance/nist/audit-log`
2.  **Schemas added:** `NISTChecklist`, `NISTEvidenceSubmission`, `NISTReviewAction`, `NISTMaturityScore`, `NISTImprovementRoadmap`, `NISTAuditLogEntry`.
3.  **ErrorEnvelope** reused – no change.
4.  **Security Schemes** for governance endpoints must include `ComplianceOfficer` and `Admin` scopes.

### Summary of File-Level Changes

*   **ReasoningLayer:**
    *   `src/ReasoningLayer/NIST/` (new directory)
    *   `Engines/NISTMaturityAssessmentEngine.cs` (new)
    *   `Ports/INISTMaturityAssessmentPort.cs` (new interface)
*   **BusinessApplications:**
    *   `src/BusinessApplications/NIST/` (new directory)
    *   `Controllers/NISTController.cs` (new)
    *   `Services/EvidenceCollector.cs` (new)
    *   `Services/ScoringEngine.cs` (new)
    *   `Services/ProgressTracker.cs` (new)
    *   OpenAPI YAML – add paths/schemas.
*   **FoundationLayer:**
    *   `Infrastructure/Repositories/NISTEvidenceRepository.cs` (new)
    *   `ConvenerData/Entities/NISTAssessment.cs` (new entity)
    *   `AuditLoggingAdapter.cs` (update with new NIST event types).
    *   `NotificationAdapter.cs` (update with new NIST notification types).
*   **AgencyLayer:**
    *   `src/AgencyLayer/NISTAgents/` (new directory)
    *   `Agents/MigrationEnforcer.cs` (new)
*   **MetacognitiveLayer:**
    *   `src/MetacognitiveLayer/NIST/` (new directory)
    *   `Monitors/ComplianceMonitor.cs` (new)

No other layers require structural change; global NFR inheritance and existing DR/observability patterns remain valid.

------------------------------------------------------------------------

## Appendices

### 1. Topics, Endpoints & G/W/T (Sample & Contracts)

- `MAP`, `MEA`, `MAN`, `GOV` and all 66 statements exposed as isolated endpoints.
- **Sample Endpoint:** Evidence Submission
  - **Path:** `/docs/spec/nist.yaml#/paths/~1governance~1nist~1evidence~1submit`
  - **G/W/T Example:**
    - **Given** a reviewer uploads an artifact >10MB via `EvidenceCollector`
    - **When** the API receives the file
    - **Then** it returns a shared error envelope and triggers a registry alert
- All endpoint contracts are referenced by OpenAPI JSON-Pointer; every operation mandates error/4xx/5xx/offline/retry/migration coverage in G/W/T.

### 2. Service-Specific NFRs, Error Envelope & Policy Snippets

- **Performance SLAs:**
  - `EvidenceCollector`, `ScoringEngine`: memory <200MB/adapter
  - Evidence API payloads <200kB each; upload cap 10MB/file
  - `ScoringEngine` response <500ms P99
- **Shared Error Envelope Schema** (applies everywhere): `{ error_code, message, correlationID }`
- **Policy Snippet (YAML):**
  ```yaml
  evidence_review_policy:
    default_reviewer: "compliance_manager"
    max_file_size_mb: 10
    required_tags: [ "NIST_topic", "pillar" ]
  scoring_rubric: "/docs/policy/nist_rubric_v3.yaml"
  ```

### 3. Test Harness Matrix Table

| Port/Adapter | Normal | 4xx/5xx | Migration | Drift | Override | Offline | A11y |
|---|---|---|---|---|---|---|---|
| `EvidenceCollector` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `ScoringEngine` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `ProgressTracker` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `NISTMaturityAdapter` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `RegistryHandler` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |
| `MigrationEnforcer` | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ | ✓ |

### 4. Visuals, Audit, and Success Metrics

- **Component/Layers Diagram:** (Full Size: `/docs/diagrams/nist/mesh-flow.svg`)
- **Audit Log & Reviewer Dashboard:** Thumbnail: (Full: `/docs/diagrams/nist/reviewer.png`)
- **Sample OpenAPI Overlay:** (Full: `/docs/diagrams/nist/openapi-overlay.png`)
- Audit flows, full evidence trail, and maturity score evolution dashboard available live and for export.
- **Success:**
  - Raise org’s average maturity scores
  - Close every red/yellow gap to green by year-end
  - No non-acknowledged migrations or registry-vulnerable clients at any time
  - 100% data and audit trace retrieval within 1 day of request
