---
Module: WhatStinksFrictionDetection
Primary Personas: Knowledge Workers, Ops Leads, System Admins
Core Value Proposition: Continuous, explainable detection of workflow friction and inefficiency
Priority: P1
License Tier: Enterprise
Platform Layers: Metacognitive, Business Apps
Main Integration Points: LlamaIndex, Trulens, Chroma, Prefect, AKS
Ecosystem: Analytics & Intelligence
---

# "What Stinks?" Friction Detection System PRD v2.0

### TL;DR

A continuous, metacognitive friction detection engine that analyzes
workflow logs, dynamically surfaces inefficiencies, and explains "what
stinks" using LlamaIndex (vector pattern search), Trulens
(chain-of-thought capture), Chroma (history tracking), and Prefect
(automated scanning/re-scanning). Deployed as a Helm/Bicep microservice
on Azure, with widgets and CLI as thin API consumers.

------------------------------------------------------------------------

## Goals

### Business Goals

- Reduce time lost to workflow friction by at least 20% per quarter.

- Give leadership real-time visibility into emerging operational
  bottlenecks and inefficiencies.

- Increase the pace and targeting of workflow optimization initiatives.

### User Goals

- Provide users with always-updated insights into friction points and
  allow them to see "why" problems are flagged.

- Deliver actionable and prioritized recommendations for eliminating
  workflow pain.

- Automatically surface new areas to improve as old frictions are
  resolved.

### Non-Goals

- Not intended for direct editing or management of workflow systems.

- Does not replace core incident response or on-call monitoring tools.

- Does not deeply profile individual user behaviors beyond anonymized
  event logs.

------------------------------------------------------------------------

## Stakeholders

- Product Owner: defines requirements and tracks adoption.

- Knowledge Workers: receive real-time friction insights and explanations.

- Ops Leads: monitor bottlenecks and receive notifications of new friction points.

- System Admins: manage deployment and security.

- Platform Engineers: maintain infrastructure and integrations.

- QA/Test Automation: validate reliability and error handling.

------------------------------------------------------------------------

## User Stories

**Persona: Knowledge Worker**

- As a knowledge worker, I want real-time visibility into where my
  workflow "stinks," so that I know what to fix next.

- As a knowledge worker, I want actionable explanations ("why did this
  stink?") so I can understand and resolve inefficiencies quickly.

**Persona: Ops Lead**

- As an ops lead, I want a dashboard that predicts workflow bottlenecks,
  so I can proactively address operational risks.

- As an ops lead, I want notifications when new friction points emerge
  after previous issues are fixed.

**Persona: System/Platform**

- As a system, I want to automatically detect and surface new friction
  each week so that the organization continually gets better, not stuck
  on old problems.

- As a system, I want to attach chain-of-thought (CoT) reasoning to
  every surfaced friction point, supporting audits and continuous
  improvement.

------------------------------------------------------------------------

## Functional Requirements

- **Friction Analyzer** (High Priority)

  - Ingest, index, and analyze workflow logs/events using LlamaIndex for
    "smell" patterns, inefficiencies, and common dead-ends via vector
    and LLM matching.

- **Chain-of-Thought (CoT) Instrumentation** (High Priority)

  - Use Trulens to wrap the LLM-based detection, capturing a full CoT
    for every flagged friction/'stink', enabling transparency and
    traceability.

- **Chroma Store** (Medium Priority)

  - Persist friction patterns, events, and analyses for cross-team trend
    mining and longitudinal reporting.

- **Prefect Orchestration** (Medium Priority)

  - Schedule/trigger friction scans as Azure CronJobs or on-demand via
    API. Auto-trigger a "frost-peak" rescan (new analysis) 24 hours
    after a friction is marked eliminated.

- **FrostPeak Loop & Notification** (Medium Priority)

  - Following friction elimination, automatically re-analyze to uncover
    and notify on newly prioritized friction.

- **API Endpoints** (High Priority)

  - POST /v1/friction/analyze: Ingest workflow events and return
    friction analysis.

  - POST /v1/friction/strategy: Return action plan with CoT
    explanations.

  - POST /v1/friction/eliminated: Mark an item as resolved and trigger
    re-analysis.

  - GET /v1/friction/emerging: Widget/CLI queries for top new friction.

  - GET /v1/friction/chain-of-thought: Retrieve full CoT for a given
    friction.

  - API returns { error_code, message, correlationID, data }.

- **Front-End/Client** (Medium Priority)

  - Dashboard widget and CLI as stateless API consumers (CDN/mesh UI
    plugin).

- **Security & Ops** (High Priority)

  - All API ingress gated by Azure Application Gateway/service mesh,
    secrets via Azure Key Vault, logs to Azure Monitor.

------------------------------------------------------------------------

## Non-Functional Requirements

- ≥99.9% uptime for all friction detection and analysis endpoints.

- 100% audit trail coverage for all friction analysis and elimination events.

- Automated test coverage of at least 80% for critical code paths.

- All data encrypted at rest and in transit.

- Friction analysis response time <300ms.

------------------------------------------------------------------------

## User Experience

**Entry Point & First-Time User Experience**

- Users discover the friction dashboard via mesh UI, intranet, or
  notification.

- First-time users see a simple onboarding pop-up explaining friction
  insights and CoT explanations.

**Core Experience**

- **Step 1:** User opens the Friction Dashboard widget.

  - UI loads friction points surfaced by the analyzer.

  - Each point has a "why" link opening the CoT explanation.

- **Step 2:** User marks a friction point as "eliminated" after
  addressing it.

  - System confirms action and informs the user that a new analysis will
    run in 24 hours.

- **Step 3:** After 24 hours, user receives a notification in the
  dashboard widget of any newly prioritized friction points.

  - New top frictions displayed—each with "why" and action roadmap.

- **Step 4:** Ops leads can export or subscribe to weekly summary
  reports.

  - Reports list all major friction points, resolutions, and explanatory
    chains.

**Advanced Features & Edge Cases**

- If no friction points surface, the dashboard displays "All clear!"
  along with recent action history.

- If analysis fails, user sees clear messaging and troubleshooting
  steps.

- Power users can filter frictions (by type, source system) and
  drill-down into historical CoT logs.

**UI/UX Highlights**

- High-contrast and accessible design—meets WCAG AA standards.

- Responsive layout and touch-friendly.

- Friction points are sortable and clearly prioritized.

- All explanations are one click away, transparent, and non-technical.

------------------------------------------------------------------------

## Narrative

After new automations roll out, the team faces new and unforeseen
workflow strains—dead-ends, silent time-wasters, avoidable double
handling. Enter the Friction Detection System: a mesh-integrated tool
that automatically ingests workflow event logs, applies LlamaIndex to
surface "what stinks," and, crucially, uses Trulens chain-of-thought
capture to explain exactly why these issues matter. As team members
resolve friction, they mark items as "eliminated." The system triggers
the "frost-peak" loop, reruns analysis a day later, then reveals the
next set of friction targets. Longitudinal trends accumulate in Chroma,
so future reports get smarter. At every touchpoint, explanations are
clear, actionable, and quickly surfaced so every team has a live radar
for inefficiency—turning reactive firefighting into a culture of
everyday improvement.

------------------------------------------------------------------------

## Success Metrics

### User-Centric Metrics

- ≥85% of users rate friction insights and explanations as "helpful" or
  "very helpful."

- 70%+ user engagement (weekly unique users checking friction
  dashboard).

### Business Metrics

- 70% or more of flagged friction points resolved or eliminated each
  quarter.

- 20%+ reduction in time spent on repetitive operational issues
  (measured via survey and event logs).

### Technical Metrics

- 80%+ of friction points surfaced with full chain-of-thought logging.

- Analysis pipeline P99 latency under 500ms per request.

- API uptime/availability 99.9% as monitored in Azure Monitor.

### Tracking Plan

- Track friction analysis, elimination, and re-analysis events.

- Monitor user engagement with dashboard and recommendations.

- Log all API calls and dashboard interactions.

- Track cross-team adoption and workflow optimization outcomes.

------------------------------------------------------------------------

## Technical Architecture & Integrations

- **Friction Analyzer:** LlamaIndex-powered vector and LLM-based pattern detection.

- **CoT Instrumentation:** Trulens for chain-of-thought capture and explanation.

- **Chroma Store:** Longitudinal storage of friction patterns and events.

- **Prefect Orchestrator:** Automated and on-demand friction scan scheduling.

- **AKS/Helm/Bicep:** Containerized deployment and orchestration.

- **API Endpoints:**
  - POST /v1/friction/analyze: Friction analysis
  - POST /v1/friction/strategy: Action plan with CoT
  - POST /v1/friction/eliminated: Mark resolved
  - GET /v1/friction/emerging: Top new friction
  - GET /v1/friction/chain-of-thought: CoT retrieval

- **Dashboard Widget:** Embedded interface for friction detection and management.

------------------------------------------------------------------------

## Technical Considerations

### Technical Needs

- Containerized analyzer, CoT logger, and vector store as separate AKS
  microservices (Helm/Bicep).

- All services expose OpenAPI endpoints versioned at
  /docs/openapi/friction/v2.

- Widget/CLI as static assets/mesh plugins—call REST APIs only.

### Integration Points

- LlamaIndex for pattern/LLM vector search.

- Trulens for CoT instrumentation and explanation logging.

- Chroma for vector persistence (upgrade to Weaviate if org-wide mesh
  memory needed).

- Prefect for workflow/orchestration running in Kubernetes CronJob.

- Azure Key Vault for all secrets/tokens; API ingress and rate limiting
  via Azure App Gateway.

### Data Storage & Privacy

- Workflow logs/analysis/results stored with pseudonymized IDs only.

- Friction patterns, CoT logs, and meta-analytics in Chroma vector DB.

- Strict GDPR compliance: No identifiable personal data in logs or
  explanations.

### Scalability & Performance

- Microservices horizontally scalable (AKS/Helm).

- Friction analysis scales linearly with workflow event stream volume;
  test up to 1,000,000 events/day.

- UI widget/CLI stateless, infinitely horizontally scalable.

### Potential Challenges

- Ensuring permission boundaries on log/access (devs see their own, ops
  see all).

- Avoiding alert fatigue by properly tuning friction surfacing
  sensitivity.

- Maintaining clarity and relevance of chain-of-thought explanations as
  the system evolves.

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- MVP (XS): 2 days (LlamaIndex, friction API, and dashboard widget
  skeleton).

- Small: 1 week (Trulens integration, Prefect orchestration, basic
  Chroma and notification).

- Medium: 2 weeks (Full frost-peak loop, advanced reporting/alerts,
  NFRs/ops hardening).

### Team Size & Composition

- Extra-small: 1–2 person team (product engineer + backend/infra).

- Small Team: 2–3 total (add front-end dev for widget polish).

### Suggested Phases

**Phase 1: Analyzer API + Vector Store** (2 days)

- Product/Backend: Ingest workflow logs, set up LlamaIndex, basic
  /friction/analyze API.

- Infra: Helm/Bicep deploy to AKS.

- Frontend: Widget skeleton displays friction output.

**Phase 2: CoT Integration, Widget, FrostPeak Loop** (3–5 days)

- Backend: Trulens wraps analyzer outputs for CoT logging.

- Product: Design and ship dashboard widget frontend.

- Orchestration: Prefect CronJob for daily/triggered re-analysis.

**Phase 3: Continuous Scan, Reporting, Ops Hardening** (5–7 days)

- Analytics: Chroma integration, reporting endpoints, and summary jobs.

- Ops: Azure Monitor pipeline, NFR tuning, security review.

- Final polish: Error handling and advanced user controls.

------------------------------------------------------------------------

## Key Integration Strategy

- **LlamaIndex (recommended):** Superior for LLM, meta-analysis, and
  vector search workflows. Prefer over FAISS for rich similarity and
  explanation tasks.

- **Trulens (recommended):** Off-the-shelf chain-of-thought
  instrumentation and OpenAI/Triton interop. Prefer over homegrown
  tracing/logging for rapid devops integration.

- **Chroma (recommended):** Lightweight, Python-native vector DB for
  friction points. Upgrade to Weaviate later for larger or org-wide use
  case.

- **Prefect (recommended):** Modern DAG/workflow tool with Azure
  container support; easier AKS integration than alternatives like
  Airflow.

------------------------------------------------------------------------

## Layer Mapping & Shared Schemas

- Analyzer, CoT, and vector store: AKS microservices (Helm/Bicep).

- Widget/CLI: CDN mesh UI plugins (stateless REST API consumers).

- Shared friction/event/CoT schemas versioned OpenAPI at
  /docs/openapi/friction/v2.

- Standard error contract: { error_code, message, correlationID, data }.

------------------------------------------------------------------------

## Service-Specific NFR Table

<table style="min-width: 125px">
<tbody>
<tr>
<th><p>Component</p></th>
<th><p>P99 Latency</p></th>
<th><p>Update Freq</p></th>
<th><p>Memory</p></th>
<th><p>Availability</p></th>
</tr>
&#10;<tr>
<td><p>LlamaIndex Engine</p></td>
<td><p>&lt;300ms</p></td>
<td><p>on search</p></td>
<td><p>&lt;96MB</p></td>
<td><p>99.95%</p></td>
</tr>
<tr>
<td><p>Trulens CoT Logger</p></td>
<td><p>&lt;250ms</p></td>
<td><p>per event</p></td>
<td><p>&lt;32MB</p></td>
<td><p>99.9%</p></td>
</tr>
<tr>
<td><p>Prefect Job Runner</p></td>
<td><p>&lt;1s</p></td>
<td><p>24h/event</p></td>
<td><p>&lt;64MB</p></td>
<td><p>99.9%</p></td>
</tr>
<tr>
<td><p>Chroma Store</p></td>
<td><p>&lt;150ms</p></td>
<td><p>per persist</p></td>
<td><p>&lt;64MB</p></td>
<td><p>99.9%</p></td>
</tr>
</tbody>
</table>

------------------------------------------------------------------------

## Test-Matrix Table

<table style="min-width: 75px">
<tbody>
<tr>
<th><p>Feature/API</p></th>
<th><p>Scenario</p></th>
<th><p>Expected Outcome</p></th>
</tr>
&#10;<tr>
<td><p>/friction/analyze</p></td>
<td><p>Ingest workflow data</p></td>
<td><p>Friction point(s) returned</p></td>
</tr>
<tr>
<td><p>/friction/strategy</p></td>
<td><p>Post-analysis, get action roadmap</p></td>
<td><p>Strategy with CoT explanations</p></td>
</tr>
<tr>
<td><p>/friction/eliminated</p></td>
<td><p>Friction eliminated, frost-peak loop</p></td>
<td><p>New friction detected after 24h</p></td>
</tr>
<tr>
<td><p>/friction/emerging</p></td>
<td><p>Widget requests new friction</p></td>
<td><p>List top 3, with CoT</p></td>
</tr>
<tr>
<td><p>/friction/chain-of-thought</p></td>
<td><p>Request for specific friction</p></td>
<td><p>Full reasoning chain output</p></td>
</tr>
<tr>
<td><p><em>Any API</em></p></td>
<td><p>Bad input/auth</p></td>
<td><p>Error envelope</p></td>
</tr>
</tbody>
</table>
