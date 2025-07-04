---
Module: PrivateMesh
Primary Personas: Security, Compliance
Core Value Proposition: 100% air-gapped MCP client
Priority: P2
License Tier: Enterprise
Platform Layers: Foundation, Reasoning
Main Integration Points: Local Models, Admin UI
---

# Product Requirements Document: Local & Private MCP Client (PrivateMesh)

### TL;DR

PrivateMesh is a fully on-premises MCP server that provides local model inference, context memory, embedding/vector search, and tool integration—all with zero external network calls. It ensures data never leaves the local machine or subnet, enabling privacy-first, compliance-driven AI workflows.

---

## Goals

### Business Goals
- **Security & Compliance:** Guarantee 100% data privacy by isolating all processing on local infrastructure.
- **Cost Predictability:** Eliminate cloud costs; operate within fixed local resource budgets.
- **Ease of Adoption:** Deployable in under 1 hour by internal teams.

### User Goals
- **Offline Availability:** Access MCP tools (chatCompletion, createEmbedding, queryMemory, searchEmbeddings) without internet.
- **Consistent Performance:** P95 inference latency <800 ms for standard prompts.
- **Persistent Context:** Maintain conversation history and memory across sessions.

### Non-Goals
- Not intended to replace large-scale cloud LLM services for huge context or high-accuracy tasks.
- No real-time web search or external data access.

---

## Stakeholders
- Product Owner: J
- Engineering Lead: Infrastructure & MCP server team
- Security & Compliance: Local deployment audit and validation
- DevOps: Installer packaging, VM/K8s templates
- Users: Privacy-sensitive teams (Legal, HR, R&D)

---

## User Stories
- As a privacy-sensitive user, I want to run all AI workflows locally so that no data leaves my network.
- As a compliance officer, I want to audit all tool usage and memory operations so that I can ensure regulatory compliance.
- As a DevOps engineer, I want a one-click installer and easy updates so that deployment is fast and reliable.
- As an admin, I want to monitor resource usage and firewall status so that I can maintain security and performance.

---

## Functional Requirements
| ID      | Requirement                                                                                                                                          | Phase | Priority |
| ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- | ----- | -------- |
| FR1     | **Installer**: `private-mesh-setup.sh` / `setup.ps1` deploys MCP server, local models (llama-2, Mistral, via Ollama/llama.cpp), SQLite DB, firewall. | 1     | P0       |
| FR2     | **Core Tools**: Expose `chatCompletion`, `createEmbedding`, `saveMemory` via STDIO/HTTP transport.                                                   | 1     | P0       |
| FR3     | **Memory Query**: `queryMemory({sessionId, query, topK})` backed by SQLite+FTS for prior chat recall.                                                | 2     | P1       |
| FR4     | **Vector Search**: `searchEmbeddings({query, topK})` using local SQLite+FAISS embedding index over user docs.                                        | 2     | P1       |
| FR5     | **Model Management**: Runtime switch between models (`llama-2-7b-chat`, `mistral-small`, etc.) via `switchModel(modelId)`.                           | 2     | P2       |
| FR6     | **Admin UI**: Dashboard for request logs, memory store health, resource usage, model loads, and firewall status.                                     | 3     | P2       |
| FR7     | **Data Isolation**: No outbound calls; validate via automated firewall/egress tests.                                                                 | 1–3   | P0       |
| FR-Gov1 | **Audit Logging**: Log all tool invocations, memory ops, model switches to encrypted local logs with rotation.                                       | 1–3   | P0       |

---

## Non-Functional Requirements
| ID   | Category        | Requirement                                        | Target                    |
| ---- | --------------- | -------------------------------------------------- | ------------------------- |
| NFR1 | Performance     | P95 chatCompletion latency (7B model)              | ≤ 800 ms                  |
| NFR2 | Scalability     | Support ≥ 5 concurrent user sessions               | ≥ 5 users                 |
| NFR3 | Reliability     | Local service uptime                               | ≥ 99.5%                   |
| NFR4 | Security        | Zero external egress; pass firewall tests          | 100% outbound blocked     |
| NFR5 | Maintainability | One-click updates via package manager (apt/winget) | 1-click installer/upgrade |

---

## User Experience
### Phase 1: Installation & Snippet Usage
1. **Install**: Run `curl https://repo.company.com/install-private-mesh.sh | bash` or PowerShell equivalent.
2. **Configure**: Edit `~/.privatemesh/config.json` for memory DB path, model defaults.
3. **Chat**: Use VS Code snippet or CLI:
   ```yaml
   provider: private-mesh
   tool: chatCompletion
   messages:
     - role: user
       content: "Explain the Q3 roadmap changes."
   ```
4. **Memory Save**: `saveMemory({sessionId})` auto-saves session to SQLite.

### Phase 2: Memory Recall & Vector Search
- **Recall**: `queryMemory({sessionId, query, topK})` injects top-K snippets.
- **Search**: `searchEmbeddings({query, topK})` returns semantically relevant docs/snippets.

### Phase 3: Admin Dashboard
- View live memory store size, model process health, firewall egress status.
- Trigger manual log export, clear cache, and rotate logs.

---

## Narrative
A legal team at a global enterprise needs to ensure that all sensitive data and AI interactions remain strictly on-premises. With PrivateMesh, they deploy the MCP server locally, configure their preferred models, and begin using chat and embedding tools with full confidence that no data ever leaves their network. The admin dashboard provides real-time visibility into memory usage and firewall status, while compliance officers can audit every operation through encrypted local logs. The result: rapid AI adoption with zero privacy compromise.

---

## Technical Architecture & Integrations
- **MCP Server**: Go/Node.js process communications over STDIO and HTTP; spawns local model processes.
- **Models**: Managed via Ollama or llama.cpp; stored in local model cache.
- **Memory DB**: SQLite with FTS5 extension for text search.
- **Vector Store**: SQLite + FAISS for embeddings.
- **Installer**: Shell/PowerShell scripts configure service, firewall, dependencies.
- **Logging**: Encrypted local log files with rotation, no network transmission.

---

## Success Metrics
- **Deployment Time:** Fresh install → first chat in <10 min.
- **Latency:** P95 chat <800 ms; memory recall <200 ms.
- **Isolation:** 100% runs validated by firewall tests.
- **Adoption:** ≥10 daily sessions by target teams.

---

## Tracking Plan
- Track installation, configuration, and chat/embedding tool usage events.
- Log all audit and compliance events.
- Monitor admin dashboard activity and log exports.
- Track error and remediation events.

---

## 10. Risks & Mitigations

| Risk                 | Mitigation                                                                   |
| -------------------- | ---------------------------------------------------------------------------- |
| Hardware limits      | Provide low/high resource presets; shipping CPU-only & GPU-optimized configs |
| Model license issues | Only whitelisted OSS models; version lock & license audit                    |
| SQLite corruption    | Use WAL journaling; automated backup and restore                             |
| Installer failures   | Test across Windows, Linux, Mac; provide Docker fallback                     |
| Egress bypass        | Pre- and post-install firewall/egress tests; periodic automated checks       |

---

## 11. Open Questions

1. Should memory DB use field-level encryption?
2. GPU recommended vs. CPU support only?
3. Default retention policy for memory?
4. Patch distribution: auto vs. manual opt-in?
5. Admin UI remote access vs. local-only?

---

## 12. Mesh Layer Mapping

| Mesh Layer              | Component                      | Responsibility / Port                                                 |
| ----------------------- | ------------------------------ | --------------------------------------------------------------------- |
| **Foundation Layer**    | LocalMCPServer, AuditLog, RBAC | Hosts core tools; enforces auth; logs to encrypted local files        |
| **Reasoning Layer**     | ChatEngine, EmbeddingEngine    | Performs inference and embedding generation via local models          |
| **Metacognitive Layer** | HealthMonitor, PrivacyMonitor  | Monitors resource health, detects egress attempts, ensures compliance |
| **Agency Layer**        | SessionManager, MemoryOps      | Orchestrates memory save/query and model-switch workflows             |
| **BusinessApps**        | CLI/VSCode Extension, Admin UI | Exposes user-facing commands, snippets, and dashboard widgets         |

---

## 13. Main APIs & Schemas

### chatCompletion (STDIO/HTTP)

**Request:**

```json
{ "tool": "chatCompletion", "messages": [{"role":"user","content":"..."}] }
```

**Response:**

```json
{ "id":"<respId>", "choices":[{"message":{"role":"assistant","content":"..."}}], "usage":{...} }
```

### createEmbedding

**Request:**

```json
{ "tool": "createEmbedding", "input": "..." }
```

**Response:**

```json
{ "embeddingId":"<id>", "vector":[...], "usage":{...} }
```

### saveMemory

**Request:**

```json
{ "tool":"saveMemory", "sessionId":"<id>", "messages":[{...}] }
```

**Response:**

```json
{ "status":"success", "savedCount":10 }
```

### queryMemory

**Request:**

```json
{ "tool":"queryMemory", "sessionId":"<id>", "query":"...", "topK":5 }
```

**Response:**

```json
{ "results":[{"entryId":"...","text":"...","score":0.87}], "usage":{...} }
```

### searchEmbeddings

**Request:**

```json
{ "tool":"searchEmbeddings", "query":"...", "topK":5 }
```

**Response:**

```json
{ "results":[{"id":"...","score":0.92}], "usage":{...} }
```

### switchModel

**Request:**

```json
{ "tool":"switchModel", "modelId":"llama-2-7b-chat" }
```

**Response:**

```json
{ "status":"switched", "modelId":"llama-2-7b-chat" }
```

### adminStatus

**Request:**

```json
{ "tool":"adminStatus" }
```

**Response:**

```json
{ "memoryDBSize":12345, "modelProcesses":[...], "egressBlocked":true }
```

---

## 14. Widget Definition

**Widget ID:** PrivateMeshLocalChatWidget\
**RBAC Roles:** User, PrivacyAdmin, SystemAdmin\
**API/Tool Bindings:** chatCompletion, createEmbedding, saveMemory, queryMemory, searchEmbeddings, switchModel, adminStatus\
**Config/UI:** Model selector, session filter, log view/export, firewall status indicator\
**Output:** LLM responses, embedding vectors, memory snippets with provenance and tags, resource health metrics\
**Registration:** Automatically registered in WidgetRegistry at install; RBAC enforces feature visibility.

---

## 15. Audit Log Event Taxonomy

| Event Type         | Description                            | Key Fields                                     |
| ------------------ | -------------------------------------- | ---------------------------------------------- |
| ModelLoaded        | Local model binary loaded into process | modelId, user, time, version                   |
| ChatGenerated      | Assistant response generated           | sessionId, user, prompt, modelId, time, tokens |
| EmbeddingDone      | Embedding vector created               | sessionId, inputRef, vectorId, time            |
| MemorySaved        | Conversation saved to memory DB        | sessionId, entryId, timestamp                  |
| MemoryQueried      | QueryMemory invocation                 | sessionId, query, topK, resultsCount, time     |
| EmbeddingsSearched | searchEmbeddings invocation            | query, topK, resultsCount, time                |
| ModelSwitched      | Active inference model changed         | oldModelId, newModelId, user, time             |
| AdminAction        | Dashboard operation performed          | actionType, user, success, time                |

All audit logs are encrypted, local-only, and rotate per retention policy.

---

> PrivateMesh: your fully private, on-prem MCP client—air-gapped, audited, and always under your control.

