# Product Requirement Documents (PRDs) – Overview

Welcome to the canonical home of all **Product Requirement Documents (PRDs)** for the Cognitive Mesh platform.  
Every feature, architecture layer, or widget added to the platform **must** have a PRD in this folder that:

1. Describes *what* will be built (and what will **not**).  
2. Specifies *why* it matters (business & user goals).  
3. Defines *how* success will be measured (functional / non-functional criteria, milestones).  
4. References and complies with the **Global Non-Functional Requirements Appendix** (`global-nfr.md`).

---

## Folder Structure

```
docs/prds/
├── 00-README.md            ← This file
├── global-nfr.md           ← Global Non-Functional Requirements Appendix
├── plugin-dashboard.md     ← Plugin-Based Dashboard Architecture PRD
├── convener-backend.md     ← Convener Backend Architecture PRD
└── convener-widget.md      ← Convener Widget/Plugin PRD
```

### File Roles

| File | Purpose |
|------|---------|
| **00-README.md** | Orientation & conventions for all PRDs. |
| **global-nfr.md** | Platform-wide security, privacy, SLA, compliance, and quality gates. All other PRDs inherit these requirements. |
| **plugin-dashboard.md** | Defines the core shell & extensible plugin/widget model. |
| **convener-backend.md** | Specifies APIs/services that power Convener features (champion discovery, pulse, etc.). |
| **convener-widget.md** | UI plugin that consumes Convener backend APIs & surfaces insights in the dashboard. |

---

## Naming & Ordering Conventions

* Files are prefixed with two digits (`00-`, `01-`, `02-` …) **only when necessary** to enforce reading order.  
* Use **kebab-case** for the descriptive slug (e.g., `plugin-dashboard`).

---

## Required PRD Sections

Every PRD **must** be written in Markdown and include the following headers (add extras as needed):

1. **TL;DR** – 3-5 sentence executive summary.  
2. **Goals** – Business & user goals.  
3. **Non-Goals / Out of Scope** – Explicitly clarify exclusions.  
4. **User Stories** – Written from the end-user or admin/developer viewpoint.  
5. **Functional Requirements** – What the system *must* do.  
6. **Non-Functional Requirements** – Performance, scalability, security, etc.  
   *Reference `global-nfr.md`; only record deltas or justified exceptions.*  
7. **UX Flows / Wireframes** – If applicable.  
8. **Risks & Mitigations** – Technical, operational, or adoption risks.  
9. **Milestones & Timeline** – Phased deliverables, owners, and acceptance criteria.  
10. **OpenAPI / Interface Links** – If the PRD defines or consumes an API, link to `../openapi.yaml` or a file in `docs/spec/`.

---

## Updating This Folder

1. **Add a new PRD**  
   * Copy the template from `docs/governance/nfr/templates/prd-template.md` (or existing PRDs).  
   * Name it `<nn>-<topic>.md` and place it here.

2. **Reference the Global NFR**  
   * Include a “NFR Compliance” table noting adherence or documented deviations.

3. **Amend an existing PRD**  
   * Update the file in-place.  
   * Add a “Change History” section or Git commit link for traceability.

4. **Update this README**  
   * Append the new filename & one-line description under **File Roles**.

---

## Quick-Reference Links

* **Global NFR Appendix:** [`global-nfr.md`](global-nfr.md)  
* **Bundled OpenAPI Spec:** [`../openapi.yaml`](../openapi.yaml)  
* **Service-level API specs:** `docs/spec/services/`  
* **Governance templates & checklists:** `docs/governance/nfr/`

---

*Last updated: 2025-07-01*  
*Maintainer: Platform Product Office*
