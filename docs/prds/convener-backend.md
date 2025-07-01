# Convener Backend Architecture PRD

## TL;DR
The Convener backend is a suite of micro-services that power the **Champion Discovery**, **Community Pulse**, **Innovation Spread**, and **Learning Catalyst** features surfaced through Convener widgets in the Cognitive Mesh dashboard.  
Services are API-first, stateless where possible, enforce strict tenant isolation, and inherit all requirements in the **Global Non-Functional Requirements Appendix** (`global-nfr.md`).  
This PRD defines business goals, user stories, functional & non-functional requirements, public APIs, and delivery milestones for the GA release.

---

## Goals

### Business Goals
* Unlock data-driven insights that accelerate knowledge sharing and innovation across large enterprises.  
* Provide an extensible backend platform that other verticals (e.g., Decision Support, Customer Intelligence) can reuse.  
* Meet enterprise security, compliance (SOC 2, GDPR), and 99.9 % availability targets.

### User Goals
* **Knowledge Champions** find and connect with experts quickly.  
* **Community Managers** monitor engagement and sentiment in real-time.  
* **Innovation Leaders** track idea diffusion and adoption hotspots.  
* **Learners** receive personalized content and skill gap analysis.

### Non-Goals
* UI or widget rendering logic (covered in `convener-widget.md`).  
* Data ingestion connectors for non-standard, on-prem proprietary systems (handled by Foundation Layer).

---

## User Stories

| Persona | Story | Priority |
|---------|-------|----------|
| Champion Seeker | “As a project lead I want to query *Top 5 champions* for ‘MLOps’ so I can invite them to a Tiger Team.” | Must |
| Community Manager | “I need to view sentiment trend of #engineering Slack channel over the last 30 days.” | Must |
| Innovation Lead | “Show me diffusion map for Idea-123 across business units.” | Should |
| L&D Partner | “Recommend learning paths for employees with skill gap ‘Azure AI Studio’.” | Must |
| Admin | “Approve or revoke data-source access for Convener services.” | Must |
| Auditor | “Export last 90 days of Convener audit logs for compliance.” | Must |

---

## Functional Requirements

### 1. Champion Discovery Service
* **Endpoint**: `GET /v1/champions`
* **Inputs**: skill/tags, timeframe, maxResults.
* **Output**: ranked list with influence score & provenance metadata.

### 2. Community Pulse Service
* **Endpoint**: `GET /v1/community-pulse`
* **Inputs**: channelId, timeframe.
* **Output**: sentiment scores, engagement metrics, trending topics.

### 3. Innovation Spread Service
* **Endpoint**: `GET /v1/innovation-spread/{ideaId}`
* **Output**: adoption curve, network graph nodes & edges.

### 4. Learning Catalyst Service
* **Endpoint**: `POST /v1/learning-catalyst/recommend`
* **Inputs**: userId, desiredSkills[].
* **Output**: learning resources, skill gap scores.

### 5. Data Ingestion
* Batch + near-real-time pipelines from HRIS, LMS, Slack/Teams, Git, JIRA.

### 6. Auditing & Telemetry
* Emit OpenTelemetry traces; write immutable audit events to `convener-audit` store.

### 7. Tenant Isolation
* Row-level security via `TenantId` claim on every query.

---

## Non-Functional Requirements Compliance

| Category (see `global-nfr.md`) | Compliance | Notes |
|--------------------------------|------------|-------|
| Security | ✅ | TLS 1.2+, AES-256, Azure Key Vault secrets |
| Telemetry & Audit | ✅ | OpenTelemetry + WORM audit storage |
| Versioning | ✅ | All endpoints v1; semantic version header |
| Privacy/GDPR | ✅ | Data minimization + user consent scopes |
| Availability SLA | ✅ | 99.9 % with multi-AZ deployment |
| Performance | ✅ | P95 < 200 ms for read endpoints |
| Rate Limiting | ✅ | 30 r/s global, 10 r/s per-tenant burst |
| Disaster Recovery | ✅ | < 30 min RTO, < 5 min RPO |
| Accessibility (API) | n/a | UI handled in widget layer |

---

## Public API Specification
* Bundled OpenAPI 3 document: [`../openapi.yaml`](../openapi.yaml)
* Service-specific YAMLs:
  * `docs/spec/services/champion-discovery.yaml`
  * `docs/spec/services/community-pulse.yaml`
  * `docs/spec/services/innovation-spread.yaml`
  * `docs/spec/services/learning-catalyst.yaml`

---

## Architecture Overview
* **Gateway**: Azure API Management enforcing auth, quotas, request validation.  
* **Compute**: .NET 9 micro-services running on Azure AKS with HPA.  
* **Data Storage**:  
  * Graph DB (Cosmos DB Gremlin) – expertise graph & idea diffusion.  
  * Postgres Flexible Server – relational metrics & configs.  
  * Vector store (Azure AI Search) – semantic embeddings for skills & content.  
* **Event Bus**: Azure Service Bus for ingestion and async workflows.  
* **Security**: Azure AD OAuth2; service-to-service mTLS; Key Vault secrets.  
* **Observability**: OpenTelemetry → Azure Monitor / Grafana.  
* **CI/CD**: GitHub Actions → Azure Container Registry → AKS blue/green.

---

## Milestones & Timeline

| Phase | Duration | Deliverables | Acceptance Criteria |
|-------|----------|--------------|---------------------|
| **M1 – Service Skeletons** | 2 wks | Repo, CI/CD, gateway, empty endpoints | Pods deploy; `/healthz` 200 OK |
| **M2 – Champion Discovery MVP** | 2 wks | Ranking algo v1, Cosmos graph, API spec | P95 latency < 300 ms, unit tests 90 % |
| **M3 – Community Pulse & Sentiment** | 3 wks | NLP pipeline, sentiment model, REST API | Sentiment accuracy ≥ 80 % on test set |
| **M4 – Innovation Spread** | 2 wks | Diffusion analytics, adoption API | Adoption curve exported CSV |
| **M5 – Learning Catalyst** | 3 wks | Recommendation engine, skills DB | Top-5 recs relevance NDCG ≥ 0.7 |
| **M6 – Hardening & DR** | 2 wks | Pen-test fixes, chaos drills, docs | 0 high-sev vulns; DR runbook pass |
| **GA** | — | Prod rollout & widget integration | All SLAs met for 30 days |

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|-----------|
| Ingested data quality low | Poor recommendations | Implement data validation & fallback defaults |
| Privacy regulation change | Compliance breach | Regular legal review; feature flags for region lockout |
| NLP model drift | Sentiment accuracy drops | Scheduled model re-training pipeline |

---

## Open Questions
1. Do we require real-time champion ranking updates (< 5 s) or is hourly batch acceptable?  
2. Will certain tenants disallow central analytics? Need optional BYOK graph store?  
3. SLA for Diffusion graph export (sync vs async)?

---

_Last updated: 2025-07-01_  
_Maintainer: Convener Backend Team_
