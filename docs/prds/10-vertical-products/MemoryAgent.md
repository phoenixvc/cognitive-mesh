---
Module: MemoryAgent
Primary Personas: Developers
Core Value Proposition: Local, encrypted chat memory in IDE
Priority: P2
License Tier: Community
Platform Layers: Foundation, Business Apps
Main Integration Points: VS Code / Cursor plug-in
---

# Product Requirements Document: Cursor+Claude Desktop Memory Integration (MemoryAgent)

### TL;DR

MemoryAgent integrates Roo/MCP with Cursor and the Claude desktop memory service to deliver persistent, privacy-first chat context within IDEs. It automatically captures and recalls conversational history locally, empowering developers to access prior chats, tag or omit sensitive content, and manage memory without leaving their IDE—ensuring offline continuity, compliance, and seamless user experience.

---

## Goals

### Business Goals
- Ensure all memory storage and retrieval occur locally, guaranteeing desktop privacy and no cloud data leakage.
- Achieve daily active usage by ≥5 developers within 3 weeks of launch.
- Maintain operating costs with zero increase in Azure or cloud spend by leveraging local or hosted Claude Lite.

### User Goals
- Enable persistent recall of relevant past chat sessions to streamline ongoing development tasks.
- Provide a frictionless user experience: memory is captured and injected with minimal manual steps.
- Allow users to tag or omit sensitive memory snippets, maintaining control over retained and recalled content.

### Non-Goals
- Not intended for large-scale document retrieval or replacing full-document RAG.
- Will not offer cloud-synced or multi-device chat memory in initial version.

---

## Stakeholders
- Product Owner: defines requirements and tracks adoption.
- Developers: primary users for chat memory and recall.
- Security-Focused IT Leads: ensure privacy and compliance.
- QA Testers: validate reliability and performance.
- Privacy Admins: oversee audit and tagging compliance.

---

## User Stories
| Persona                      | Story                                                                                                   |
| ---------------------------- | ------------------------------------------------------------------------------------------------------- |
| **Software Developer**       | As a Developer, I want my AI chats remembered across sessions so I can revisit past solutions.          |
|                              | As a Developer, I want a single command to recall memory in VS Code or Cursor, avoiding complex setups. |
|                              | As a Developer, I want to mark chat messages as private so sensitive project info is excluded.          |
|                              | As a Developer, I want to search chat history for topics within the IDE for quick reference.            |
| **Security-Focused IT Lead** | As an IT Lead, I want conversation memory encrypted and stored locally so policies are never violated.  |
| **QA Tester**                | As a QA, I want to verify accurate and fast memory retrieval to ensure reliability in critical tasks.   |

---

## Functional Requirements
| ID    | Requirement                                                                                               | Phase | Priority |
| ----- | --------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1   | Integrate Roo/MCP with Cursor to persist chat via `saveMemory({sessionId,messages})`.                     | 1     | P0       |
| FR2   | Implement `queryMemory({sessionId,query,topK})` to return top-K relevant snippets from local storage.     | 1     | P0       |
| FR3   | Provide VS Code snippet `Roo: Recall Memory` to inject top-K snippets into prompt context.                | 1     | P1       |
| FR4   | Integrate with Claude desktop memory API via MCP HTTP tool `claudeMemory` for advanced semantic recall.   | 2     | P1       |
| FR5   | Enable tagging of memory entries (public/private) and filter recall by tags.                              | 2     | P2       |
| FR6   | Surface a VS Code Webview UI for users to review, edit, delete, or backup memory entries.                 | 3     | P2       |
| FR7   | Encrypt local memory at rest with AES-256 using user passphrase; ensure no data leaves device by default. | 1–3   | P0       |
| FR-G1 | Log all memory operations (save, query, delete, tag) in a tamper-resistant local audit log.               | 1–3   | P0       |

---

## Non-Functional Requirements
| ID   | Category           | Requirement                                                  | Target                          |
| ---- | ------------------ | ------------------------------------------------------------ | ------------------------------- |
| NFR1 | Performance        | Memory recall/injection latency                              | P95 < 200ms                     |
| NFR2 | Reliability        | Local memory uptime                                          | ≥ 99.5%                         |
| NFR3 | Security & Privacy | 100% at-rest AES-256 encryption; zero outbound data flow     | Verified by security audit      |
| NFR4 | Scalability        | Support ≥3 concurrent sessions per device                    | Continuous reliable performance |
| NFR5 | Maintainability    | Code coverage ≥80% for memory module tests; CLI docs updated | CI-passing                      |

---

## User Experience
### Entry & Onboarding
- Install Roo/MCP extension in VS Code or Cursor.
- On first use, prompt for passphrase to encrypt local memory.
- Tooltips highlight `Recall Memory`, tagging, and privacy features.

### Core Flows
**Automatic Capture**
1. User chats via Roo; all messages auto-saved grouped by `sessionId`.
2. Success/failure feedback shown; actionable errors if disk/permissions issues.

**Instant Recall**
1. User runs `Roo: Recall Memory` or HTTP `queryMemory`.
2. Top-K snippets retrieved via semantic search (or fallback), previewed for discard/injection.
3. Snippets injected into active prompt context.

**Privacy & Tagging**
- Inline commands/UI toggles to tag entries public/private.
- Recall filters respect tag exclusion.

**Management UI**
- Phase 3 webview: list all memory entries, edit/delete/tag, backup/export, and view audit log.
- Bulk delete for private content, one-click backup.

### Advanced & Edge Cases
- Fallback to SQLite FTS5 if Claude API unavailable.
- LRU eviction on large DB to maintain <200ms recall latency.
- Clear alerts for encryption key errors or DB corruption; link to restore.

---

## Narrative
A developer is working on a complex project and frequently consults Roo for code suggestions and debugging help. With MemoryAgent, every chat is automatically saved and encrypted locally. When the developer returns to a similar problem weeks later, she uses the `Recall Memory` command to instantly retrieve relevant past conversations, filtering out sensitive content with privacy tags. The seamless recall and management UI save hours of searching, and the IT lead is confident that all data remains private and compliant.

---

## Success Metrics
- Number of developers using MemoryAgent daily within the first month.
- Percentage of memory recalls completed within 200ms.
- User satisfaction scores (CSAT/NPS) for memory workflow.
- Audit/compliance pass rate for memory logs.
- Number of unique sessions and memory entries managed per week.

---

## Tracking Plan
- Track memory save, query, tag, and delete events.
- Log all audit and compliance events.
- Monitor user feedback and management UI actions.
- Track error and remediation events.

---

## Technical Architecture & Integrations
- **MemoryStore (SQLite):** Local encrypted memory storage.
- **RecallEngine:** Semantic ranking and tag-filtered queries.
- **ClaudeSimilarity:** Advanced semantic recall via Claude API.
- **Webview UI:** Management interface for memory review and tagging.
- **RBAC/AAA Service:** Enforces access control and credential management.
- **Audit Logging Service:** Stores immutable logs for all operations.
- **API Endpoints:**
  - /api/saveMemory: Persists chat memory.
  - /api/queryMemory: Retrieves relevant memory snippets.
  - /api/tagMemory: Tags memory entries.
  - /api/deleteMemory: Deletes memory entries.
  - /api/auditLog: Retrieves audit events.

---

## 1. Goals

### Business Goals

- Ensure all memory storage and retrieval occur locally, guaranteeing desktop privacy and no cloud data leakage.
- Achieve daily active usage by ≥5 developers within 3 weeks of launch.
- Maintain operating costs with zero increase in Azure or cloud spend by leveraging local or hosted Claude Lite.

### User Goals

- Enable persistent recall of relevant past chat sessions to streamline ongoing development tasks.
- Provide a frictionless user experience: memory is captured and injected with minimal manual steps.
- Allow users to tag or omit sensitive memory snippets, maintaining control over retained and recalled content.

### Non-Goals

- Not intended for large-scale document retrieval or replacing full-document RAG.
- Will not offer cloud-synced or multi-device chat memory in initial version.

---

## 2. User Stories

| Persona                      | Story                                                                                                   |
| ---------------------------- | ------------------------------------------------------------------------------------------------------- |
| **Software Developer**       | As a Developer, I want my AI chats remembered across sessions so I can revisit past solutions.          |
|                              | As a Developer, I want a single command to recall memory in VS Code or Cursor, avoiding complex setups. |
|                              | As a Developer, I want to mark chat messages as private so sensitive project info is excluded.          |
|                              | As a Developer, I want to search chat history for topics within the IDE for quick reference.            |
| **Security-Focused IT Lead** | As an IT Lead, I want conversation memory encrypted and stored locally so policies are never violated.  |
| **QA Tester**                | As a QA, I want to verify accurate and fast memory retrieval to ensure reliability in critical tasks.   |

---

## 3. Functional Requirements

| ID    | Requirement                                                                                               | Phase | Priority |
| ----- | --------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1   | Integrate Roo/MCP with Cursor to persist chat via `saveMemory({sessionId,messages})`.                     | 1     | P0       |
| FR2   | Implement `queryMemory({sessionId,query,topK})` to return top-K relevant snippets from local storage.     | 1     | P0       |
| FR3   | Provide VS Code snippet `Roo: Recall Memory` to inject top-K snippets into prompt context.                | 1     | P1       |
| FR4   | Integrate with Claude desktop memory API via MCP HTTP tool `claudeMemory` for advanced semantic recall.   | 2     | P1       |
| FR5   | Enable tagging of memory entries (public/private) and filter recall by tags.                              | 2     | P2       |
| FR6   | Surface a VS Code Webview UI for users to review, edit, delete, or backup memory entries.                 | 3     | P2       |
| FR7   | Encrypt local memory at rest with AES-256 using user passphrase; ensure no data leaves device by default. | 1–3   | P0       |
| FR-G1 | Log all memory operations (save, query, delete, tag) in a tamper-resistant local audit log.               | 1–3   | P0       |

---

## 4. User Experience

### Entry & Onboarding

- Install Roo/MCP extension in VS Code or Cursor.
- On first use, prompt for passphrase to encrypt local memory.
- Tooltips highlight `Recall Memory`, tagging, and privacy features.

### Core Flows

**Automatic Capture**

1. User chats via Roo; all messages auto-saved grouped by `sessionId`.
2. Success/failure feedback shown; actionable errors if disk/permissions issues.

**Instant Recall**

1. User runs `Roo: Recall Memory` or HTTP `queryMemory`.
2. Top-K snippets retrieved via semantic search (or fallback), previewed for discard/injection.
3. Snippets injected into active prompt context.

**Privacy & Tagging**

- Inline commands/UI toggles to tag entries public/private.
- Recall filters respect tag exclusion.

**Management UI**

- Phase 3 webview: list all memory entries, edit/delete/tag, backup/export, and view audit log.
- Bulk delete for private content, one-click backup.

### Advanced & Edge Cases

- Fallback to SQLite FTS5 if Claude API unavailable.
- LRU eviction on large DB to maintain <200ms recall latency.
- Clear alerts for encryption key errors or DB corruption; link to restore.

---

## 5. Non-Functional Requirements

| ID   | Category           | Requirement                                                  | Target                          |
| ---- | ------------------ | ------------------------------------------------------------ | ------------------------------- |
| NFR1 | Performance        | Memory recall/injection latency                              | P95 < 200ms                     |
| NFR2 | Reliability        | Local memory uptime                                          | ≥ 99.5%                         |
| NFR3 | Security & Privacy | 100% at-rest AES-256 encryption; zero outbound data flow     | Verified by security audit      |
| NFR4 | Scalability        | Support ≥3 concurrent sessions per device                    | Continuous reliable performance |
| NFR5 | Maintainability    | Code coverage ≥80% for memory module tests; CLI docs updated | CI-passing                      |

---

## 6. Main APIs & Schemas

**saveMemory**\
*Request:*

```json
{ "sessionId": "string", "messages": [{"role":"user","text":"..."}, ...] }
```

*Response:* `{ "status":"success" }`

**queryMemory**\
*Request:*

```json
{ "sessionId": "string", "query": "string", "topK": 5 }
```

*Response:*

```json
{ "snippets": [ {"entryId":"string","text":"...","tags":["public"]}, ... ] }
```

**tagMemory** / **deleteMemory** / **auditLog** follow similar request/response patterns with appropriate IDs and statuses.

---

## 7. Audit Log Event Taxonomy

| Event Type    | Description             | Key Fields                     |
| ------------- | ----------------------- | ------------------------------ |
| MemorySaved   | Chat messages persisted | sessionId, entryId, user, tags |
| MemoryQueried | Recall executed         | sessionId, query, topK, user   |
| MemoryTagged  | Tag assignment          | entryId, oldTag, newTag, user  |
| MemoryDeleted | Entry deletion          | entryId, user, timestamp       |
| AuditAccessed | Audit log viewed        | user, timestamp, scope         |

All events stored in an encrypted, append-only local audit log.

---

## 8. Mesh Layer Mapping

| Mesh Layer            | Component                            | Responsibility / Port                                    |
| --------------------- | ------------------------------------ | -------------------------------------------------------- |
| Foundation Layer      | MemoryStore (SQLite), AuditLog, RBAC | Local encrypted memory storage, secure logging, AAA      |
| Reasoning Layer       | RecallEngine, ClaudeSimilarity       | Semantic ranking, vector recall, tag-filtered queries    |
| Metacognitive Layer   | PrivacyMonitor, SessionMonitor       | PII detection, key errors, session health monitoring     |
| Agency Layer          | MemoryRecallOrchestrator             | Orchestrates save→query→inject→UI workflows              |
| Business Applications | VS Code/Cursor Extension, Webview UI | Exposes commands: saveMemory, queryMemory, management UI |

**Integration Instructions:**

- Register `MemoryAgentRecallWidget` with WidgetRegistry.
- Enforce RBAC via Foundation Layer on all operations.
- Emit audit events per taxonomy for compliance.

---

## 9. Widget Definition

**Widget ID:** MemoryAgentRecallWidget\
**RBAC Roles:** Developer, PrivacyAdmin\
**Bindings:** saveMemory, queryMemory, tagMemory, deleteMemory, auditLog\
**Config:** Session selector, max snippets, tag filters, passphrase management\
**Output:** Snippet list with preview, source jump links, privacy tags, delete/backup controls\
**Registration:** Standard mesh widget onboarding; role-bound visibility.

---

## 10. Milestones & Exit Criteria

| Phase | Duration | Exit Criteria                                                                                 |
| ----- | -------- | --------------------------------------------------------------------------------------------- |
| P1    | 1 week   | Core save/query/inject flows; 5 pilot devs run ≥10 recall cycles with P95 <200ms; audit logs. |
| P2    | 2 weeks  | Claude memory API integration; tagging filter UI; fallback to SQLite tested.                  |
| P3    | 1 week   | Full Webview management UI; backup/restore; encryption key flow; security sign-off.           |

---

## 11. Risks & Mitigations

| Risk                      | Mitigation                                                    |
| ------------------------- | ------------------------------------------------------------- |
| Database corruption       | Use WAL-mode, integrity checks, automated backups, restore UI |
| Claude API unavailability | Fallback to SQLite FTS5, clear user guidance                  |
| Performance degradation   | Implement LRU eviction, index tuning, size alerts             |
| Key loss or mismatch      | Prompt re-key flows, backup/export key utility                |
| Sensitive leak via logs   | Encrypt audit log, restrict access to local only              |

---

## 12. Open Questions

1. Should sessionId default to project path or user-defined?
2. Cross-session/global search: opt-in or always on?
3. Retention policy: max entries per session? automatic purge schedule?
4. Exportable audit logs: strictly local or optional admin upload?

---

> MemoryAgent ensures developers retain contextual chat intelligence securely and privately—integrated seamlessly into their IDE workflow without cloud dependency.

