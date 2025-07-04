---
Module: DocRAG
Primary Personas: Devs, Analysts
Core Value Proposition: Cited Q&A over long docs
Priority: P0
License Tier: Professional
Platform Layers: Business Apps, Reasoning
Main Integration Points: Search, Vector, Plugin Registry
---

# MCP-Powered RAG Over Complex Documents (DocRAG)

### TL;DR

DocRAG empowers developers and analysts to query vast collections of
complex documents and instantly receive precise, cited answers. With a
single snippet or API call, users can automate ingestion, retrieval, and
LLM-powered Q&A—bridging the gap between tedious manual review and
efficient, validated insight.

------------------------------------------------------------------------

## Goals

### Business Goals

- Achieve adoption by at least 3 internal feature teams within 2 weeks
  of launch.

- Maintain monthly Azure and compute costs at or below €300 for 5,000
  queries.

- Reduce average time-to-answer for long-form technical docs from hours
  to under 1 minute.

### User Goals

- Instantly obtain accurate answers and citations from technical
  documents using a VS Code command or API call.

- View clear citations for every fact, indicating source file and
  precise page or line number.

- Query over diverse document types (PDF, DOCX, HTML, plain text),
  including custom, private data sources.

### Non-Goals

- Not designed for real-time document collaboration or co-editing.

- Not intended as an open-ended chatbot—DocRAG only retrieves knowledge
  from explicitly ingested documents.

------------------------------------------------------------------------

## Stakeholders

- Product Owner: defines requirements and tracks adoption.

- Developers: primary users for Q&A and document search.

- Data Analysts: use for compliance and technical research.

- QA Engineers: validate accuracy and audit logs.

- Product Managers: pilot users and feedback providers.

- Security & Compliance: ensure audit and regulatory compliance.

------------------------------------------------------------------------

## User Stories

**Primary Personas**

- Developer

- Data Analyst

- QA Engineer

- Product Manager (pilot user)

**Developer**

- As a developer, I want to run a code snippet to ask questions about a
  library's docs, so I can avoid manual searching and save time.

- As a developer, I want to see citations for every answer, so I can
  quickly verify the information.

**Data Analyst**

- As a data analyst, I want to upload and search regulatory PDFs, so I
  can instantly answer compliance questions.

- As a data analyst, I want to filter queries by date or tag, so I get
  the most relevant excerpts.

**QA Engineer**

- As a QA engineer, I want to QA the system's accuracy by querying known
  specs, so I can flag gaps or errors.

- As a QA engineer, I want access logs for queries, so I can audit usage
  and investigate issues.

**Product Manager**

- As a PM, I want to see high-level usage of DocRAG, so I can measure
  adoption and impact.

- As a PM, I want to participate in the pilot and give feedback, so my
  team's use cases are covered.

------------------------------------------------------------------------

## Functional Requirements

- **Document Ingestion:** Support PDF, DOCX, HTML, and plain text ingestion via UI or API.

- **Indexing:** Automated indexing and refresh of document collections.

- **Hybrid Retrieval:** Cognitive Search and Vector DB fallback for robust retrieval.

- **LLM Q&A:** Prompt LLM with top hits and return answer with citations.

- **Citation Display:** Inline citations with file, page/line, and clickable links in UI.

- **API Access:** /api/performRAG endpoint for programmatic Q&A.

- **Telemetry & Logging:** Log all queries, errors, and user feedback for audit and improvement.

- **Access Control:** RBAC for document collections and Q&A endpoints.

------------------------------------------------------------------------

## Non-Functional Requirements

- ≥95% of queries return answers within 1 minute.

- 100% audit trail coverage for all queries and user actions.

- 99.9% uptime for Q&A and ingestion endpoints.

- Automated test coverage of at least 80% for critical code paths.

- All data encrypted at rest and in transit.

------------------------------------------------------------------------

## User Experience / UX Flow

### Phase 1: VS Code Snippet Journey

- **Entry Point**

  - User launches VS Code, accesses Roo-powered snippet "Perform RAG."

  - Wizard prompts for the document collection ("sourceId") and natural
    language query.

  - User can optionally select retrieval mode (Cognitive Search or
    Vector DB fallback).

- **Core Flow**

  1.  System indexes/refreshes selected docs if needed, then retrieves
      top hits.

  2.  Backend assembles prompt, invokes LLM, and receives answer plus
      citations.

  3.  Result is displayed: clear answer plus inline citations (file,
      snippet, page/line), styled for readability.

  4.  Faults (model quota, stale index, rejected content) are surfaced
      in-line with actionable guidance.

- **Edge/Advanced Flows**

  - If Cognitive Search fails, fallback is auto-triggered via Vector DB.

  - If source is missing, user gets prompt to upload/ingest first.

  - For multi-hit citations, user can click to open file and scroll to
    passage in-editor.

  - All errors are logged and user is notified safely, with telemetry
    captured.

- **UI/UX Highlights**

  - Color-coded citations, accessible contrast.

  - Intuitive feedback widget (👍/👎) and optional comment on answer
    quality.

  - All steps keyboard navigable, a11y tested.

### Phase 2: API Journey

- **Entry Point**

  - User (or integration) POSTs to /api/performRAG with payload:

    - sourceId: document collection or drive

    - query: user's question

    - topN: number of top chunks/snippets to retrieve

- **Core Flow**

  1.  Mesh server checks/ingests docs, then runs hybrid search/vector
      retrieval.

  2.  LLM is prompted and generates answer, returning JSON with answer
      and citation array.

  3.  Result delivered to client; consumers can surface in custom UIs
      (e.g., clickable citations in web apps).

- **Advanced/Edge Cases**

  - Optional parameters for retrieval mode and metadata filtering.

  - Streaming or chunked response for very long answers.

------------------------------------------------------------------------

## Narrative

A software engineer, swamped with hundreds of pages of compliance, must
validate requirements before release. Instead of scrolling endlessly,
she opens VS Code and runs the "Perform RAG" snippet. Within seconds,
DocRAG indexes the latest docs, retrieves the most relevant snippets,
and invokes a state-of-the-art LLM. The tool returns a crystal-clear
answer—and every fact is cited with filename and page number. She clicks
a citation to jump right to source, validates instantly, and shares the
answer with her team. Instead of an afternoon lost in manual review,
she's done in minutes—improving delivery velocity, accuracy, and
compliance.

------------------------------------------------------------------------

## Success Metrics

- Number of feature teams using DocRAG within the first month.

- Percentage of queries completed within 1 minute.

- User satisfaction scores (CSAT/NPS) for Q&A workflow.

- Audit/compliance pass rate for query logs.

- Number of document types and sources ingested per week.

------------------------------------------------------------------------

## Tracking Plan

- Track document ingestion, query, and citation events.

- Log all audit and compliance events.

- Monitor user feedback and citation click actions.

- Track error and remediation events.

------------------------------------------------------------------------

## Technical Architecture & Integrations

- **Ingestion Service:** Handles document upload and parsing.

- **Indexing Engine:** Automates indexing and refresh of document collections.

- **Retrieval Engine:** Hybrid Cognitive Search and Vector DB fallback.

- **LLM Service:** Prompts LLM and returns answer with citations.

- **Citation Renderer:** Displays inline citations in UI and API responses.

- **RBAC/AAA Service:** Enforces access control and credential management.

- **Telemetry & Logging Service:** Stores immutable logs for all operations.

- **API Endpoints:**

  - /api/performRAG: Performs Q&A over ingested documents.

  - /api/ingestDocs: Uploads and indexes new documents.

  - /api/queryLogs: Retrieves audit and usage logs.

- **VS Code Snippet & Widget:** UI for Q&A, citation review, and feedback.

------------------------------------------------------------------------

## Milestones & Sequencing

### Project Estimate

- Medium: 4–5 weeks, 2–3 person team

### Team Size & Composition

- Small Team: 1 Product/Eng, 1 Backend, 1 DevOps/QA

### Suggested Phases

**Phase 1: VS Code Snippets for Search→Chat and Vector→Chat (1 week)**

- Key Deliverables: FR1, FR2, FR7, FR-Gov1, FR-Gov2

- Dependencies: Existing Cognitive Search/Vector DB, Azure OpenAI, pilot
  docs indexed

- Exit Criteria: 5 users complete >20 diverse queries, all citations
  validated, no critical bugs

**Phase 2: Mesh API & Multi-format Ingestion (2 weeks)**

- Key Deliverables: FR3, FR4, FR7, FR-Gov1, FR-Gov2, NFR1–4

- Dependencies: Container Apps/AKS deployed, audit/log pipeline live,
  API keys provisioned

- Exit Criteria: All integration tests pass, ≤500ms latency at 80 RPS,
  documented API

**Phase 3: Heuristics, Filtering, Advanced Compliance (1 week)**

- Key Deliverables: FR5, FR6, FR7, FR-Gov1, FR-Gov2, FR-Gov3

- Dependencies: Sufficient user base for feedback, RBAC groups set up,
  a11y tested

- Exit Criteria: Metadata filters work, heuristic path selection
  demonstrated, logs/audits reviewed

------------------------------------------------------------------------

## Risks & Mitigations

------------------------------------------------------------------------

## Open Questions

- Should we default to hybrid retrieval (BM25 + embeddings), or require
  user opt-in per query?

- What's the preferred way to handle very long answers: chunked output
  or paginated step-through?

- Is end-of-answer citation listing or inline footnotes more usable for
  most users?

- For access control: do we need per-file/source ACLs, or will
  resource-level RBAC suffice?

- Should we invest in real-time "index freshening" for fast-changing
  source docs?

- What is the threshold for requiring human review on flagged sensitive
  content?

------------------------------------------------------------------------

## Rollout Plan

- **Pilot**

  - Identify and onboard 5 power users (developers, analysts)

  - 20+ queries per participant, feedback session at week end

  - Success: all pilot users report satisfaction and can validate ≥90%
    of citations

- **Beta**

  - Expand to 3–5 teams, broad document coverage

  - QA benchmarks, collect success metric data, run regular office hours

  - Success: Citation accuracy & latency consistently within SMART
    targets

- **Full Launch**

  - Open to all internal/partner teams

  - Comprehensive documentation, "how-to" decks, and office hours

  - Formal feedback/review cadence, user support channel established

- **Feedback Channels**

  - In-product rating/comments per answer

  - Monthly usage/metric review sessions with target users

------------------------------------------------------------------------

## Component Mapping

<table style="min-width: 100px">
<tbody>
<tr>
<th><p>Mesh Layer</p></th>
<th><p>Component</p></th>
<th><p>Description &amp; Contract/Port</p></th>
<th><p>Integration Points</p></th>
</tr>
&#10;<tr>
<td><p>FoundationLayer</p></td>
<td><p>Ingestion Adapter, Audit Logger</p></td>
<td><p>Handles file ingestion, storage (Cognitive Search/Blob), logs all
RAG queries</p></td>
<td><p>PluginRegistry, Azure Storage, Audit Log</p></td>
</tr>
<tr>
<td><p>ReasoningLayer</p></td>
<td><p>Hybrid Retriever, VectorSearch Engine</p></td>
<td><p>Orchestrates hybrid BM25/vector retrieval, snippet extraction,
citation formatting</p></td>
<td><p>IRetrieverPort, VectorSearchPort, SnippetFormatterPort</p></td>
</tr>
<tr>
<td><p>MetacognitiveLayer</p></td>
<td><p>Compliance Monitor</p></td>
<td><p>Audits outputs for citation integrity, monitors latency and
usage/rate drift</p></td>
<td><p>ComplianceDashboardService, PolicyMonitor</p></td>
</tr>
<tr>
<td><p>AgencyLayer</p></td>
<td><p>RAG Orchestrator</p></td>
<td><p>Bundles end-to-end RAG (ingest→retrieve→LLM→cite), error handling
across layers</p></td>
<td><p>OrchestrationPort, Role-based Dispatch</p></td>
</tr>
<tr>
<td><p>BusinessApplications</p></td>
<td><p>DocRAG API Controller &amp; Widget</p></td>
<td><p>Exposes REST APIs (performRAG, ingestDocuments), renders
dashboard widget with citations and source jump</p></td>
<td><p>OpenAPI YAML (performRAG), WidgetDefinition, Mesh API
gateway</p></td>
</tr>
</tbody>
</table>

- All outputs/queries must use mesh error envelope and audit log.

- RBAC/compliance checks enforced in Foundation/Metacognitive layer.

- Widget registry/app onboarding on install.

- List example file/interface per component where helpful.

This mapping appendix ensures traceability from APIs/engines/adapters to
the correct mesh layer and supports cross-team ops and compliance.

------------------------------------------------------------------------
