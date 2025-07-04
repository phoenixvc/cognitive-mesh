---
Module: ResearchGen
Primary Personas: Researchers, Analysts
Core Value Proposition: Multi-agent research report orchestration
Priority: P0
License Tier: Enterprise
Platform Layers: Agency, Metacognitive
Main Integration Points: Mesh API, Plugin Registry
---

# MCP-Powered Multi-Agent Deep Researcher Workflow (ResearchGen) – Full Mesh Mapping

### TL;DR

ResearchGen enables multi-agent, stepwise research and analysis over
diverse sources through a mesh-native backend. Every action is
explainable, auditable, and policy-compliant—allowing users to generate
deeply cited, multi-domain reports with full modular agent
extensibility.

------------------------------------------------------------------------

## Goals

### Business Goals

- Achieve adoption by at least 2 research teams within 4 weeks of
  launch.

- Deliver a 90%+ reduction in time spent on deep research tasks versus
  current manual methods.

- Ensure every output is 100% auditable and explainable for compliance
  and trust.

### User Goals

- Complete in-depth research with full citations and actionable
  insights.

- Receive clear, explainable rationales for every recommendation and
  cited fact.

- Provide structured feedback on agent quality and referencing.

### Non-Goals

- No physical or real-world actuation—focus is solely on digital
  research/analysis.

- No closed-box agent logic; all actions and decisions must be
  transparent.

- Not a general-purpose document generator (focus is research,
  synthesis, and citation).

------------------------------------------------------------------------

## Stakeholders

- Product Owner: drives feature and business alignment.

- AI/Backend Engineering: builds agent orchestration, citation, analysis
  engines.

- QA: validates auditability, correctness, performance, and user
  experience.

- Security/Compliance: ensures policy and data handling for all actions.

- End Users (Analyst/Researcher): interact via UI/Widget and APIs.

- DevOps: supports deployment, monitoring, and scaling.

------------------------------------------------------------------------

## User Stories

- As a researcher, I want to orchestrate multi-agent research tasks so that I can generate comprehensive, multi-source reports efficiently.

- As an analyst, I want every research step to be auditable and explainable so that I can trust and defend my findings.

- As a compliance officer, I want to review audit logs and citation chains so that I can ensure regulatory requirements are met.

------------------------------------------------------------------------

## Functional Requirements

- **FR1:** Multi-agent orchestration engine supporting configurable
  workflows and pluggable agents.

- **FR2:** Research synthesis chain with explain/citation contract for
  every input and output.

- **FR3:** Modular, stepwise execution framework with granular audit,
  rollback, and restart capabilities.

- **FR4:** Dashboard widget for surfacing reports, reviewing citations,
  and capturing user feedback.

- **FR-Gov1:** Full audit logging of every action, role assignment,
  synthesis step, and all agent decisions with stable citation links and
  explainability records.

------------------------------------------------------------------------

## Non-Functional Requirements

- **NFR1:** ≥90% of system actions/verdicts must be explainable and
  directly cited.

- **NFR2:** 95% of research tasks must complete with P95 latency ≤3
  minutes.

- **NFR3:** 100% audit trail coverage for every phase, action, and
  agent.

- **NFR4:** Automated test coverage of at least 80% for critical code
  paths.

------------------------------------------------------------------------

## User Experience

- User accesses the ResearchGen dashboard or API.

- User configures the workflow: selects research goal, expert agents,
  domains, and data sources.

- System executes in a structured, stepwise fashion:

  - Agents are assigned tasks/subtasks.

  - Each agent collects data, analyzes sources, and delivers findings
    with explicit reasoning and citations.

  - The system synthesizes multi-agent output into a cohesive,
    end-to-end report.

- As each phase completes:

  - Users see a narrative with inline citations and rationale.

  - Review and approval screen allows the user to approve, request
    clarifications, or roll back steps.

  - Feedback loop allows users to rate quality, accuracy, and agent
    performance.

- Edge cases:

  - Agents are stuck: system invokes fallback logic, prompts user for
    input, or applies a timeout breaker.

  - Source fails or is unreachable: user is notified; system documents
    failure with reason and citation gap.

  - Policy/compliance block: user is alerted and given remediation path.

------------------------------------------------------------------------

## Narrative

A research team at a global consultancy needs to deliver a multi-domain, deeply cited report in record time. Using ResearchGen, they configure a workflow with domain-specific agents, each tasked with gathering and analyzing data from trusted sources. As the agents work, every step is logged and explainable. The team reviews the synthesized report, checks citations, and provides feedback. When a source fails, the system flags the gap and suggests alternatives. The final report is delivered with full auditability, boosting client trust and meeting compliance needs.

------------------------------------------------------------------------

## Success Metrics

- Number of research teams adopting ResearchGen within the first quarter.

- Reduction in average research task completion time.

- Percentage of outputs with full citation and explainability coverage.

- User satisfaction scores (CSAT/NPS) for research workflow.

- Audit/compliance pass rate for generated reports.

------------------------------------------------------------------------

## Tracking Plan

- Track workflow configuration events, agent assignment, and completion times.

- Log all citation and explainability events.

- Monitor user feedback and report approval/rollback actions.

- Track audit log access and compliance review events.

------------------------------------------------------------------------

## Technical Architecture & Integrations

- **FoundationLayer:** Agent registry (tracks pluggable experts),
  persistent audit database, and policy manager.

- **ReasoningLayer:** Orchestrator (multi-agent workflow runner),
  stepwise research analysis chain, citation/explainability engine.

- **MetacognitiveLayer:** Learning monitor, policy/compliance
  monitor—tracks improvement, applies policies, flags errors.

- **AgencyLayer:** Policy orchestrator (role/task assignment, approval,
  rollback engine).

- **BusinessApplications:** Exposed via REST API (/api/researchgen),
  dashboard report widget (browser/SPA), and feedback interface.

- **Adapters/Connectors:**

  - Source connectors for search engines, enterprise knowledge bases,
    and public APIs.

  - Plugin interfaces for adding/removing agents.

  - OpenAPI contracts and WidgetRegistry entries for onboarding and
    governance.

------------------------------------------------------------------------

### Expanded Technical Architecture Details

- **Main API Endpoints**:

  - /api/researchAgent/run: Launches a research task.

  - /api/researchAgent/status: Checks the status of ongoing research
    tasks.

  - /api/researchAgent/report: Fetches complete research reports.

- **Payload Schemas**:

  - **ResearchTaskRequest**: Defines the research objective, sources,
    and agents.

  - **ResearchReport**: Contains findings, citations, rationales, and
    synthesis details.

  - **FeedbackPayload**: Captures user feedback on report quality and
    agent performance.

- **Audit Event Taxonomy**:

  - **Agent Launch**: Triggers when a new agent is initiated.

  - **Step Complete/Error**: Logs successful completions or errors at
    each step.

  - **Phase Approval**: Records approval, rollback, or requests for
    clarification.

  - **Report Generated**: Captures the completion of full research
    report generation.

  - **Feedback Submitted**: Logs user feedback submission actions.

------------------------------------------------------------------------

## Milestones & Timeline

1.  **Phase 1 (1 week):**

- Deliver functional orchestrator, agent workflow config, workflow
  prototype.

2.  **Phase 2 (2 weeks):**

- Build and integrate agent/task plugins, citation/explainability chain,
  full synthesis/audit, REST API.

3.  **Phase 3 (1 week):**

- Implement compliance monitor, audit dashboards, and research report
  widget; finalize developer docs and onboarding.

4.  **Exit Criteria:**

- ≥2 research teams live.

- ≥90% of all verdicts have explain/cite coverage.

- 100% coverage for audit logs of major actions.

------------------------------------------------------------------------

## Risks & Mitigations

- **Orchestration Deadlock:** Add timeout breakers and circuit controls
  to prevent hangs; alert users on issues.

- **Policy/Compliance Gaps:** Full audit/test harness, explicit
  fail-safes, and policy pre-validation before runs.

- **Scalability Limits:** Support actor pools; monitor for contention
  and resource constraints.

- **Agent Config Drift:** Require all agent/plugin changes to flow
  through approval logs and active monitoring.

------------------------------------------------------------------------

## Open Questions

- Which agent roles/pluggable experts should be prioritized for MVP
  launch?

- Where should UI approvals and action states be stored—central policy
  store or per-agent logs?

- At what granularity should contracts and error envelopes be defined
  between steps for traceability?

------------------------------------------------------------------------

## Mesh Layer Mapping Appendix

<table style="min-width: 100px">
<tbody>
<tr>
<th><p><strong>Mesh Layer</strong></p></th>
<th><p><strong>Component</strong></p></th>
<th><p><strong>Description &amp; Contract/Port</strong></p></th>
<th><p><strong>Integration Point(s)</strong></p></th>
</tr>
&#10;<tr>
<td><p><strong>FoundationLayer</strong></p></td>
<td><p>Agent Registry, Audit Store</p></td>
<td><p>Tracks pluggable agents, manages secure audit logging.</p></td>
<td><p>PluginRegistry, Audit DB</p></td>
</tr>
<tr>
<td><p><strong>ReasoningLayer</strong></p></td>
<td><p>Orchestrator, Analysis Engine</p></td>
<td><p>Manages research execution, analysis, and citation
logic.</p></td>
<td><p>IOrchestratorPort, AnalysisAdapterPort</p></td>
</tr>
<tr>
<td><p><strong>MetacognitiveLayer</strong></p></td>
<td><p>Compliance Monitor</p></td>
<td><p>Monitors compliance, handles policy breaches, and error
tracking.</p></td>
<td><p>ComplianceDashboardService, PolicyMonitorPort</p></td>
</tr>
<tr>
<td><p><strong>AgencyLayer</strong></p></td>
<td><p>Multi-Agent Coordination Engine</p></td>
<td><p>Oversees role/task assignment, manages approval workflows and
reversion.</p></td>
<td><p>CoordinationPort, TaskDispatchAdapter</p></td>
</tr>
<tr>
<td><p><strong>BusinessApplications</strong></p></td>
<td><p>Dashboard Report Widget, REST API</p></td>
<td><p>Exposes endpoints for task execution, status polling, report
generation, and feedback collection.</p></td>
<td><p>OpenAPI Definitions, WidgetEnginePort</p></td>
</tr>
</tbody>
</table>

### Widget Definition

- **Widget ID & Name**: ResearchGenReportWidget

- **RBAC Roles**: Analyst, Research Admin

- **API/Tool Bindings**: ResearchGen API (launch/run workflow, fetch
  report, feedback)

- **Config Options**: Agent config (templates, sources), step/phase
  display, approval gating

- **Output**: Cited reports, agent rationale, approval status, feedback

- **Widget Metadata Reg Steps**: Register in WidgetRegistry on
  onboarding
