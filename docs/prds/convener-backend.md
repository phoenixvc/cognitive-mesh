# Convener Backend Architecture PRD

### TL;DR  
The Convener backend is a secure, API-driven platform that underpins champion discovery, community monitoring, innovation tracking, and psychological-safety insights.  Acting as the orchestration layer for all Convener widgets and dashboard integrations, it enables organisations to measure and enhance collective intelligence and innovation while strictly enforcing privacy, consent, and provenance.

---

## Goals  

### Business Goals  
* Accelerate collaboration and innovation workflows that deliver measurable outcomes.  
* Provide objective, data-driven measurement of collective intelligence and the factors driving successful team interactions.  
* Protect community trust through psychological-safety safeguards and transparent data practices.  
* Offer reliable, extensible APIs to power all Convener widgets and mesh integrations.

### User Goals  
* **Community Managers** receive real-time champion and connector recommendations.  
* **Team Leads** build high-performing, cross-functional teams via collaboration suggestions.  
* **End Users** see how their contributions catalyse learning and innovation and control what data is shared.  
* **Compliance Officers** can audit provenance, access, and approval logs quickly.

### Out-of-Scope  
* UI / dashboard layout (handled in `convener-widget.md`).  
* Low-level dashboard or plugin framework infrastructure.  
* Generic mesh platform APIs unrelated to social catalysis, discovery, or learning.

---

## User Stories  

| Persona | Story | Priority |
|---------|-------|----------|
| Community Manager | “See top champions and connectors so I can accelerate project outcomes.” | Must |
| Community Manager | “View engagement and psychological-safety pulse to support at-risk members.” | Must |
| Team Lead | “Get collaborator recommendations based on expertise and participation.” | Must |
| Team Lead | “Receive notifications of successful innovation patterns to replicate.” | Should |
| End User | “See where my contributions catalysed learning/innovation so I feel recognised.” | Must |
| End User | “Control and see transparency of shared social/learning data.” | Must |
| Compliance Officer | “Audit provenance, access, and approvals to ensure policy compliance.” | Must |

---

## Functional Requirements  

| Service | Priority | Key Responsibilities |
|---------|----------|----------------------|
| **Champion Discovery** | Must | Rank champions/connectors using interaction data, endorsements, event signals. |
| **Community Pulse** | Must | Aggregate engagement, sentiment, psychological-safety metrics by team / org / cohort. |
| **Learning Catalyst** | Should | Curate, tag, and push learning logs; link contributions to outcomes. |
| **Innovation Spread Engine** | Should | Detect, log, and propagate innovations; track adoption lineage. |
| **Approval / Consent** | Must | Manage explicit user/admin approvals for sharing or recommendations. |
| **Provenance Event Logging** | Must | Persist provenance logs for every system action or recommendation. |
| **Notification / Push** | Should | Dispatch real-time signals and feedback requests to widgets/users. |

### Acceptance Criteria Highlights  
* **Champion Discovery** returns accurate, auditable rankings; provenance 100 %.  
* **Community Pulse** refreshes metrics within 5 min; tenant-scoped.  
* **Approval / Consent** blocks any personal data exposure without explicit consent.  
* **Notification Service** delivers ≥ 95 % messages within 500 ms on active sessions.

---

## Feature Prioritisation  

| Feature | Priority | Future Opportunity |
|---------|----------|--------------------|
| Champion Discovery | Must | Ingest external community signals |
| Community Pulse | Must | Deep-dive analytics & trend projections |
| Approval / Consent | Must | Org-level consent policies; MFA approval |
| Provenance Logging | Must | Real-time public provenance feeds |
| Learning Catalyst | Should | Personalised learning-plan recommendations |
| Innovation Spread | Should | ROI analytics; automated replication |
| Notification / Push | Should | Omnichannel delivery (email, SMS, in-app) |

---

## API & Data Contracts  

* **Bundled OpenAPI**: [`../openapi.yaml`](../openapi.yaml) – canonical, version-controlled spec.  
* All endpoints require JWT auth + tenant boundary check; versioned `/v1/`, `/v2/`; n-1 compatibility.  
* Every response includes provenance & audit fields.

### Core Endpoints (v1)  

| Path | Verb | Description |
|------|------|-------------|
| `/champions/discover` | GET | Ranked champions list |
| `/pulse/aggregate` | GET | Engagement & risk metrics |
| `/learning/catalysts` | POST | Curated catalyst events |
| `/innovation/spread` | GET | Innovation lineage & metrics |
| `/approvals/request` | POST | Request user/admin approval |
| `/provenance/events` | GET | Query provenance log |

---

## Distributed Architecture Mapping  

Convener functionality is distributed across the *existing* Cognitive-Mesh
layers rather than being implemented in a standalone sub-layer.

```text
┌─────────────────────────────────────────────────────────────┐
│                        Consumers                            │
│          (Widgets / External Callers / Agents)              │
└───────────────┬─────────────────────────────────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│  BusinessApplications (API Orchestration & Approval Flows) │
│  • REST / gRPC endpoints (`/champions`, `/pulse`, …)       │
│  • Approval / Consent Service                               │
│  • Notification / Push Service                             │
└───────────────┬─────────────────────────────────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│        MetacognitiveLayer (Monitoring & Trends)            │
│  • Community Pulse Service – sentiment & safety indices     │
│  • Learning Catalyst Orchestrator – experiment outcomes     │
└───────────────┬─────────────────────────────────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│         ReasoningLayer (Algorithms & Scoring)              │
│  • Champion Discovery Engine – endorsement ranking          │
│  • Innovation Spread Detector – virality / pattern mining   │
│  • Competitive Reality Check – external benchmark models    │
└───────────────┬─────────────────────────────────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│      FoundationLayer (Data Ingestion & Persistence)        │
│  • Raw social / learning event capture & tenant streams     │
│  • Secure Postgres / Event Store for provenance logs        │
│  • Authentication & multi-tenancy enforcement               │
└───────────────┬─────────────────────────────────────────────┘
                ▼
┌─────────────────────────────────────────────────────────────┐
│            AgencyLayer (Automation & Nudgers)              │
│  • Event-driven agents that trigger champion nudges         │
│  • CLI / SDK runners for batch discovery or audits          │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities & Convener Components

| Layer | Convener Responsibilities |
|-------|---------------------------|
| **FoundationLayer** | Raw event capture (social interactions, endorsements, learning logs); tenant-scoped streams; secure provenance storage. |
| **ReasoningLayer** | Champion Discovery algorithms; Innovation Spread detection; Competitive benchmarks. |
| **MetacognitiveLayer** | Community Pulse metrics; Learning Catalyst outcome correlation; trend analysis. |
| **BusinessApplications** | Public REST/gRPC endpoints; approval & consent workflows; notification fan-out. |
| **AgencyLayer** | Automation agents (“nudgers”) that, once consented, trigger workflows (e-mail champs, create collaboration spaces). |

This distributed approach maintains **architectural consistency**, maximises
code reuse, and keeps each Convener capability in the layer best suited to its
responsibility.

---

## Data, Privacy & Security  

* **Storage**: Tenant-isolated Postgres, partitioned event logs with FK provenance.  
* **Encryption**: TLS 1.2+ in transit, AES-256 at rest (Key Vault-backed keys).  
* **Consent Enforcement**: Policy engine checks consent tokens before data access or outbound share.  
* **Rate Limiting**: Global & per-tenant quotas with exponential back-off.  
* **Observability**: OpenTelemetry traces, Grafana dashboards (latency, error, quota).

---

## NFR Compliance (inherits `global-nfr.md`)  

| Category | Compliance Highlights |
|----------|-----------------------|
| Security | TLS 1.2+, AES-256, mTLS, pen-testing quarterly |
| Telemetry | OpenTelemetry, WORM audit storage |
| Availability | ≥ 99.9 %, multi-AZ failover, hourly hot backups |
| Performance | P95 < 200 ms read; < 500 ms notification |
| DR | RPO ≤ 1 h, RTO ≤ 2 h, quarterly DR drills |
| Accessibility | API only (UI handled in widget layer) |

---

## Processing Flows (Sequence)  

### Champion Matchmaking  
1. Widget ⟶ `GET /champions/discover`  
2. App Layer calls Domain `ChampionScorer`  
3. Scores persisted, provenance log written  
4. Response returned to widget

### Pulse Aggregation  
1. Widget ⟶ `GET /pulse/aggregate`  
2. App Layer aggregates metrics, calls `SentimentAggregator`  
3. Risk flags computed, logged  
4. Widget displays results

*Detailed diagrams in shared architecture repo.*

---

## Error & Resilience  

* Cached last-known data served during outages.  
* Graceful degradation: if sentiment API down → return “metrics unavailable”.  
* Incident response: Sev 1 ack ≤ 5 min, mitigation ≤ 30 min.  
* Chaos-engineering: node kill, DB failover drills quarterly.

---

## Milestones  

| Phase | Weeks | Key Deliverables |
|-------|-------|------------------|
| **MVP** | 1-4 | Champion Discovery, Community Pulse, Approval/Consent APIs + tracing |
| **Post-MVP** | 5-8 | Learning Catalyst, Innovation Spread, notifications, enhanced analytics |
| **Hardening** | 9-10 | Pen-test remediation, DR drills, performance tuning |
| **GA** | — | Production rollout; widgets integrated; SLAs met for 30 days |

---

## Risks & Mitigations  

| Risk | Mitigation |
|------|------------|
| Data drift in champion scoring | Scheduled re-training + human-in-the-loop corrections |
| Privacy breach / consent failure | Strict consent pipeline + real-time privacy monitoring |
| High load scalability | Auto-scaling, quotas, chaos drills |
| Signal fatigue | Feedback loop, analytics-driven signal tuning |

---

## Open Questions  
1. Real-time vs hourly champion scoring refresh?  
2. Optional BYOK graph store for regulated tenants?  
3. Async vs sync export for diffusion graphs?

---

_Last updated: 2025-07-01_  
_Maintainer: Convener Backend Team_
