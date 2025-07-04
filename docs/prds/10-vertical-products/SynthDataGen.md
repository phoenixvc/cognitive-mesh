---
Module: SynthDataGen
Primary Personas: QA / ML
Core Value Proposition: Schema-driven synthetic data pipelines
Priority: P3
License Tier: Community
Platform Layers: Reasoning, Business Apps
Main Integration Points: DataGen Engine, UI Widget
---

# Product Requirements Document: Synthetic Data Generator MCP (SynthDataGen)

### TL;DR

SynthDataGen is an MCP-powered toolchain that enables QA and ML teams to generate, validate, and visualize robust synthetic datasets within seconds. With deep schema configurability, one-click evaluation, and built-in compliance checks, it brings secure, repeatable data creation to every stage of AI development.

---

## Goals

### Business Goals
- Onboard ≥2 QA/ML teams within 3 weeks.
- Reduce dataset creation time from days to <1 minute.
- Keep operational costs <€150/month for 1,000 dataset generations.

### User Goals
- Generate synthetic data matching a provided JSON schema or sample set.
- Automate statistical evaluation and PII checks for high data quality.
- Provide instant visualizations (histograms, scatter plots) of generated data.

### Non-Goals
- Not for production-scale ML training pipelines initially.
- No live data ingestion from external DBs in v1.

---

## Stakeholders
- Product Owner: defines requirements and tracks adoption.
- QA Engineers: primary users for test data generation.
- ML Engineers: use for model testing and validation.
- Compliance Officers: ensure privacy and audit compliance.
- Developers: maintain APIs and integrations.
- Security & Compliance: ensure audit and regulatory compliance.
- QA/Test Automation: validate reliability and error handling.

---

## User Stories
| Persona                | Story                                                                                       |
| ---------------------- | ------------------------------------------------------------------------------------------- |
| **QA Engineer**        | As a QA engineer, I want large, schema-compliant datasets quickly to improve test coverage. |
|                        | As a QA engineer, I want automated quality evaluation so I can trust test results.          |
|                        | As a QA engineer, I want visual distributions to identify edge cases.                       |
| **ML Engineer**        | As an ML engineer, I want synthetic data matching real schemas to test models safely.       |
|                        | As an ML engineer, I want benchmark quality metrics to validate data integrity.             |
| **Compliance Officer** | As a compliance officer, I want automated PII detection on outputs to ensure privacy.       |
|                        | As a compliance officer, I want audit logs for every generation/evaluation cycle.           |

---

## Functional Requirements
| ID    | Requirement                                                                                          | Phase | Priority |
| ----- | ---------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1   | **Generate Data**: From JSON schema or sample input, adjustable record count.                        | 1     | P0       |
| FR2   | **Evaluate Data**: Compute distribution match, null rates, uniqueness, correlations; pass/warn/fail. | 1     | P1       |
| FR3   | **Visualize Data**: Inline histograms, scatter plots, value counts as images or base64.              | 1     | P1       |
| FR4   | **Pipeline API**: `/syntheticDataPipeline` REST endpoint chaining gen→eval→vis; accessible via MCP.  | 1     | P0       |
| FR5   | **Admin UI**: Web interface to configure default schemas, browse history, download datasets/reports. | 3     | P2       |
| FR-G1 | **Audit Logging**: Log every operation with schema fingerprint and metadata.                         | 1–3   | P0       |
| FR-G2 | **PII Scan**: Integrate PII detection on each output dataset; block on detection.                    | 1–3   | P0       |

---

## Non-Functional Requirements
| ID   | Category        | Requirement                              | Target                       |
| ---- | --------------- | ---------------------------------------- | ---------------------------- |
| NFR1 | Performance     | P95 gen+eval+vis latency for 1k rows     | <2s                          |
| NFR2 | Scalability     | Support ≥20 concurrent pipelines/min     | Continuous support           |
| NFR3 | Reliability     | Service uptime                           | ≥99.5%                       |
| NFR4 | Security        | Zero PII leakage; audit log completeness | Verified by compliance audit |
| NFR5 | Maintainability | CI test coverage ≥90% for pipeline code  | CI-green                     |

---

## User Experience
### Entry & Onboarding
- Install SynthDataGen VS Code extension or call REST API.
- Guided tutorial: upload schema, set record count, run generation.

### Core Flow
1. **Schema & Count**: User selects/uploads JSON schema and specifies row count.
   - Immediate schema validation feedback.
2. **Generate**: Click "Generate" triggers MCP pipeline: data gen → evaluation → visualization.
   - Progress spinner/status in IDE or API response.
3. **Download & Review**: Link to CSV/JSON download; inline report of quality metrics and viz images.
4. **History & Logs**: Access past runs, reports, and audit logs via Admin UI or commands.

### Advanced & Edge Cases
- Warn/suggest chunking for large datasets.
- Block downloads if PII detected; guide remediation.
- Support parameter presets and schema templates.

---

## Narrative
A QA engineer is tasked with testing a new AI model but lacks sufficient real-world data. Using SynthDataGen, she uploads a JSON schema, sets the desired record count, and generates a synthetic dataset in seconds. The tool automatically evaluates data quality, flags any PII, and provides instant visualizations. She downloads the dataset and report, confident in its compliance and coverage. When a compliance officer reviews the audit logs, every generation and evaluation is fully traceable, ensuring privacy and regulatory standards are met.

---

## Success Metrics
- Number of QA/ML teams onboarded within the first month.
- Percentage of datasets generated within 2 seconds for 1k rows.
- User satisfaction scores (CSAT/NPS) for data generation workflow.
- Audit/compliance pass rate for generation logs.
- Number of unique schemas and datasets generated per week.

---

## Tracking Plan
- Track data generation, evaluation, and visualization events.
- Log all audit and compliance events.
- Monitor user feedback and download/export actions.
- Track error and remediation events.

---

## Technical Architecture & Integrations
- **DataGen Engine:** Core data synthesis and schema validation.
- **Eval Engine:** Computes quality metrics and PII detection.
- **Vis Engine:** Generates visualizations (histograms, scatter plots, etc.).
- **Pipeline API:** Orchestrates gen→eval→vis workflow.
- **Admin UI:** Web interface for configuration, history, and downloads.
- **RBAC/AAA Service:** Enforces access control and credential management.
- **Audit Logging Service:** Stores immutable logs for all operations.
- **API Endpoints:**
  - /api/syntheticDataPipeline: Launches a synthetic data generation pipeline.
  - /api/evaluateSyntheticData: Evaluates data quality and compliance.
  - /api/visualizeSyntheticData: Generates visualizations for datasets.
  - /api/pipelineHistory: Retrieves past runs and reports.

---

## 1. Goals

### Business Goals

- Onboard ≥2 QA/ML teams within 3 weeks.
- Reduce dataset creation time from days to <1 minute.
- Keep operational costs <€150/month for 1,000 dataset generations.

### User Goals

- Generate synthetic data matching a provided JSON schema or sample set.
- Automate statistical evaluation and PII checks for high data quality.
- Provide instant visualizations (histograms, scatter plots) of generated data.

### Non-Goals

- Not for production-scale ML training pipelines initially.
- No live data ingestion from external DBs in v1.

---

## 2. User Stories

| Persona                | Story                                                                                       |
| ---------------------- | ------------------------------------------------------------------------------------------- |
| **QA Engineer**        | As a QA engineer, I want large, schema-compliant datasets quickly to improve test coverage. |
|                        | As a QA engineer, I want automated quality evaluation so I can trust test results.          |
|                        | As a QA engineer, I want visual distributions to identify edge cases.                       |
| **ML Engineer**        | As an ML engineer, I want synthetic data matching real schemas to test models safely.       |
|                        | As an ML engineer, I want benchmark quality metrics to validate data integrity.             |
| **Compliance Officer** | As a compliance officer, I want automated PII detection on outputs to ensure privacy.       |
|                        | As a compliance officer, I want audit logs for every generation/evaluation cycle.           |

---

## 3. Functional Requirements

| ID    | Requirement                                                                                          | Phase | Priority |
| ----- | ---------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1   | **Generate Data**: From JSON schema or sample input, adjustable record count.                        | 1     | P0       |
| FR2   | **Evaluate Data**: Compute distribution match, null rates, uniqueness, correlations; pass/warn/fail. | 1     | P1       |
| FR3   | **Visualize Data**: Inline histograms, scatter plots, value counts as images or base64.              | 1     | P1       |
| FR4   | **Pipeline API**: `/syntheticDataPipeline` REST endpoint chaining gen→eval→vis; accessible via MCP.  | 1     | P0       |
| FR5   | **Admin UI**: Web interface to configure default schemas, browse history, download datasets/reports. | 3     | P2       |
| FR-G1 | **Audit Logging**: Log every operation with schema fingerprint and metadata.                         | 1–3   | P0       |
| FR-G2 | **PII Scan**: Integrate PII detection on each output dataset; block on detection.                    | 1–3   | P0       |

---

## 4. User Experience

### Entry & Onboarding

- Install SynthDataGen VS Code extension or call REST API.
- Guided tutorial: upload schema, set record count, run generation.

### Core Flow

1. **Schema & Count**: User selects/uploads JSON schema and specifies row count.
   - Immediate schema validation feedback.
2. **Generate**: Click "Generate" triggers MCP pipeline: data gen → evaluation → visualization.
   - Progress spinner/status in IDE or API response.
3. **Download & Review**: Link to CSV/JSON download; inline report of quality metrics and viz images.
4. **History & Logs**: Access past runs, reports, and audit logs via Admin UI or commands.

### Advanced & Edge Cases

- Warn/suggest chunking for large datasets.
- Block downloads if PII detected; guide remediation.
- Support parameter presets and schema templates.

---

## 5. Non-Functional Requirements

| ID   | Category        | Requirement                              | Target                       |
| ---- | --------------- | ---------------------------------------- | ---------------------------- |
| NFR1 | Performance     | P95 gen+eval+vis latency for 1k rows     | <2s                          |
| NFR2 | Scalability     | Support ≥20 concurrent pipelines/min     | Continuous support           |
| NFR3 | Reliability     | Service uptime                           | ≥99.5%                       |
| NFR4 | Security        | Zero PII leakage; audit log completeness | Verified by compliance audit |
| NFR5 | Maintainability | CI test coverage ≥90% for pipeline code  | CI-green                     |

---

## 6. Main APIs & Schemas

**POST /api/syntheticDataPipeline**\
*Request:*

```json
{
  "schema": {/* JSON Schema */},
  "rowCount": 1000,
  "options": { "visualizations": ["histogram","scatter"] }
}
```

*Response:*

```json
{
  "dataUrl": "https://.../dataset.csv",
  "evaluation": { /* pass/warn/fail metrics */ },
  "visualizations": { /* img URLs/base64 */ }
}
```

**POST /api/evaluateSyntheticData** and **POST /api/visualizeSyntheticData** support modular calls.

---

## 7. Audit Log Event Taxonomy

| Event            | Description                    | Fields                         |
| ---------------- | ------------------------------ | ------------------------------ |
| DataGenStarted   | Pipeline initiated             | runId, user, schemaFingerprint |
| DataGenCompleted | Generation finished            | runId, rowCount, duration      |
| EvalCompleted    | Quality evaluation done        | runId, metrics, status         |
| VizCompleted     | Visualization images generated | runId, chartTypes              |
| PIIDetected      | PII scan flagged               | runId, fields                  |
| ExportTriggered  | Dataset/report downloaded      | runId, format                  |

All stored in secure append-only logs.

---

## 8. Mesh Layer Mapping

| Mesh Layer            | Component                              | Responsibility / Port                   |
| --------------------- | -------------------------------------- | --------------------------------------- |
| Foundation Layer      | DataGenEngine, AuditLog, RBAC          | Core data synthesis, logging, auth      |
| Reasoning Layer       | SchemaValidator, EvalEngine, VisEngine | Stats tests, PII scan, chart generation |
| Metacognitive Layer   | DataDriftMonitor, CoverageMonitor      | Monitor schema drift, usage metrics     |
| Agency Layer          | DataGenWorkflowManager                 | Orchestrate pipeline steps              |
| Business Applications | VS Code Extension, AdminWidget         | Expose commands and web UI              |

**Integration Instructions:** Register AdminWidget, enforce RBAC, emit audit events.

---

## 9. Widget Definition

**Widget ID:** SynthDataGenAdminWidget\
**RBAC Roles:** QA Engineer, ML Engineer, ComplianceAdmin\
**APIs:** All core endpoints + `GET /api/pipelineHistory`\
**Config:** Schema library, default options, PII block toggle\
**Output:** Links to downloads, evaluation report, visualization thumbnails, audit entries

---

## 10. Milestones & Exit Criteria

| Phase | Duration | Exit Criteria                                                                                             |
| ----- | -------- | --------------------------------------------------------------------------------------------------------- |
| P1    | 1 week   | VS Code snippet + API stub; 1 pilot team generates ≥5 datasets; basic audit logs                          |
| P2    | 2 weeks  | Full pipeline API; evaluation & viz modules; PII checks; latency <2s for 1k rows; cost baseline validated |
| P3    | 1 week   | Admin UI with history/download/export; compliance sign-off; scalability tested (≥20 pipelines/min)        |

---

## 11. Risks & Mitigations

| Risk                         | Mitigation                                                |
| ---------------------------- | --------------------------------------------------------- |
| Schema validation errors     | Pre-lint schemas, detailed error messages                 |
| PII leaks                    | Mandatory PII scan, block on detection                    |
| Memory/latency on large data | Chunked generation, job queue limits, resource monitoring |
| Compliance audit failures    | Detailed audit logs, automated compliance checks          |

---

## 12. Open Questions

1. Which data generation backend (SDV variant) should be default?
2. Should users be able to chain multiple schema templates?
3. How granular should PII scan rules be configurable?
4. Should history exports include raw datasets or only summaries?

---

> SynthDataGen delivers fast, compliant synthetic data pipelines directly in your IDE or scripts—empowering QA/ML teams with trusted, repeatable test data.

