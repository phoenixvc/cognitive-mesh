# PRD: Architecture Context MCP Server
**Project:** `arch-context-mcp`
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Problem Statement

PhoenixVC operates 20+ production repositories across a polyglot stack (Python, C#/.NET, Rust, TypeScript). As the system architect, maintaining deep cross-repo context during design, development, and agent-assisted work is increasingly expensive. Existing AI coding tools (Copilot, Cursor) operate with shallow, session-scoped context — they don't understand:

- How repositories relate to each other
- Architectural boundaries and ownership
- Data flows across service boundaries
- Dependency graphs at the system level
- Domain-specific patterns and conventions per project

Third-party tools (Roam-Code, Sourcegraph Cody) solve parts of this but introduce vendor dependency, limited customisation, and no integration with the Cognitive Mesh agent ecosystem.

---

## 2. Objective

Build a self-hosted MCP server that indexes all PhoenixVC repositories, constructs a living architecture knowledge graph, and exposes structured context to any MCP-compatible agent or LLM interface — including Lobe, Claude, and Cognitive Mesh agents.

---

## 3. Goals & Non-Goals

### Goals
- Index all PhoenixVC repos (local + GitHub) into a queryable architecture graph
- Expose architecture context via MCP protocol to any connected agent
- Support cross-repo dependency tracing, data flow queries, and pattern detection
- Remain stack-agnostic (Python, Rust, C#, TypeScript all supported)
- Integrate natively with Cognitive Mesh agent orchestration
- Self-hosted, zero vendor dependency

### Non-Goals
- Not a code editor or IDE plugin
- Not a replacement for git or source control
- Not a real-time code execution environment
- Not a general-purpose search engine

---

## 4. Users & Stakeholders

| User | Context |
|------|---------|
| **Hans (Architect)** | Primary — architecture queries, cross-repo tracing, design decisions |
| **Cognitive Mesh Agents** | Consumers — use context for autonomous task execution |
| **Lobe / Claude** | MCP clients — query server during assisted development sessions |
| **Future engineering team** | Onboarding, codebase navigation |

---

## 5. Core Features

### 5.1 Repository Indexer
- Scan local paths and GitHub orgs (`phoenixvc`, `justaghost`)
- Parse source files across: `.py`, `.cs`, `.rs`, `.ts`, `.tsx`, `.json`, `.yaml`, `.toml`
- Extract: files, modules, classes, functions, interfaces, exports, imports, dependencies
- Detect: service boundaries, API contracts, shared libraries, config patterns
- Store in: graph database (Neo4j or lightweight alternative)
- Trigger: on-demand, scheduled (cron), or git webhook

### 5.2 Architecture Knowledge Graph
- Nodes: Repository · Module · Service · Function · Interface · Schema · Config
- Edges: depends_on · imports · calls · exposes · implements · shares_schema_with
- Metadata: language, project (VeritasVault/ChaufHer/etc.), domain, last_modified, owner
- Query interface: Cypher (Neo4j) or GraphQL

### 5.3 MCP Tool Endpoints

| Tool Name | Description |
|-----------|-------------|
| `get_repo_overview` | Summary of a repo — structure, language, dependencies, purpose |
| `trace_dependency` | Forward/reverse dependency trace across repos |
| `find_pattern` | Locate architectural patterns (e.g. "all event handlers", "all API gateways") |
| `get_data_flow` | Trace data from source to sink across service boundaries |
| `list_interfaces` | All public contracts/interfaces for a given service |
| `get_project_context` | Full architecture context for a named project (e.g. VeritasVault) |
| `search_codebase` | Semantic search across indexed content |
| `get_change_impact` | Given a file/module change, list affected downstream components |

### 5.4 Semantic Search Layer
- Embed indexed content using a local embedding model (e.g. `nomic-embed-text` via Ollama)
- Store vectors in pgvector (PostgreSQL) or Qdrant
- Enable natural language queries: *"Where is the risk scoring logic in VeritasVault?"*

### 5.5 Project Metadata Registry
- Named project definitions: VeritasVault · Cognitive Mesh · PhoenixRooivalk · ChaufHer · Mystira
- Each project maps to: repos, domains, tech stack, architectural style, key interfaces
- Manually curated + auto-updated from index

---

## 6. Technical Architecture

```
┌─────────────────────────────────────────────────────┐
│                  MCP Clients                        │
│         Lobe · Claude · Cognitive Mesh Agents       │
└──────────────────────┬──────────────────────────────┘
                       │ MCP Protocol (stdio / SSE)
┌──────────────────────▼──────────────────────────────┐
│            arch-context-mcp (MCP Server)            │
│                                                     │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────┐  │
│  │ Tool Router │  │ Query Engine │  │  Indexer  │  │
│  └──────┬──────┘  └──────┬───────┘  └─────┬─────┘  │
│         └────────────────┼────────────────┘        │
│                          │                         │
│  ┌───────────────────────▼─────────────────────┐   │
│  │           Knowledge Store                   │   │
│  │  Graph DB (Neo4j/Kuzu) + pgvector/Qdrant    │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────┐
│              Source Repositories                    │
│   GitHub (phoenixvc · justaghost) · Local Paths     │
└─────────────────────────────────────────────────────┘
```

### Stack
| Component | Technology |
|-----------|-----------|
| MCP Server runtime | Python (`mcp` SDK) or TypeScript (`@modelcontextprotocol/sdk`) |
| Graph database | **Kuzu** (embedded, no server) or Neo4j (if scale needed) |
| Vector store | **pgvector** (reuse existing PostgreSQL) or Qdrant |
| Embedding model | `nomic-embed-text` via Ollama (local, no API cost) |
| Repo access | `gitpython` + GitHub API (`PyGithub`) |
| Code parsing | Tree-sitter (multi-language AST parsing) |
| Scheduler | APScheduler or cron |
| Deployment | Docker container, runs alongside Cognitive Mesh stack |

---

## 7. Data Model (Simplified)

```python
# Core node types
Repository(id, name, org, language[], path, project, last_indexed)
Module(id, repo_id, path, language, exports[], imports[])
Service(id, repo_id, name, type, port, protocol)
Interface(id, module_id, name, methods[], schema)
Function(id, module_id, name, signature, calls[], complexity)

# Core edge types
DEPENDS_ON(from: Module, to: Module, type: import|package)
CALLS(from: Function, to: Function, async: bool)
EXPOSES(from: Service, to: Interface)
SHARES_SCHEMA(from: Service, to: Service, schema: str)
BELONGS_TO(from: Repository, to: Project)
```

---

## 8. Integration Points

| System | Integration |
|--------|------------|
| **Cognitive Mesh** | Native MCP tool registration — agents call arch tools directly |
| **Lobe** | Add as custom MCP server in Skill Store |
| **Claude Desktop** | Add to `claude_desktop_config.json` |
| **GitHub Webhooks** | Trigger re-index on push to any tracked repo |
| **VS Code / Cursor** | Via Continue.dev MCP plugin (optional) |

---

## 9. Phased Delivery

### Phase 1 — Foundation (Week 1–2)
- [ ] Repo scanner: file tree + language detection
- [ ] Basic graph schema in Kuzu (embedded)
- [ ] MCP server skeleton with 3 tools: `get_repo_overview`, `search_codebase`, `get_project_context`
- [ ] Manual project metadata registry (JSON config)
- [ ] Docker container + local deployment

### Phase 2 — Intelligence (Week 3–4)
- [ ] Tree-sitter AST parsing for Python, TypeScript, Rust, C#
- [ ] Dependency graph construction
- [ ] Semantic embedding + vector search via pgvector
- [ ] Tools: `trace_dependency`, `find_pattern`, `list_interfaces`
- [ ] GitHub API integration for remote repo indexing

### Phase 3 — Integration (Week 5–6)
- [ ] Cognitive Mesh agent registration
- [ ] GitHub webhook for auto re-indexing
- [ ] Tools: `get_data_flow`, `get_change_impact`
- [ ] Query performance optimisation
- [ ] Lobe Skill Store packaging

### Phase 4 — Production (Week 7–8)
- [ ] Full PhoenixVC repo index (20+ repos)
- [ ] Incremental indexing (diff-based, not full re-scan)
- [ ] Access control (project-level scoping)
- [ ] Monitoring + index health dashboard
- [ ] Documentation + agent prompt templates

---

## 10. Success Metrics

| Metric | Target |
|--------|--------|
| Repos indexed | 20+ (all PhoenixVC) |
| Query response time | < 500ms for graph queries |
| Semantic search relevance | > 85% top-3 accuracy (manual eval) |
| MCP tool coverage | 8 core tools live |
| Agent adoption | Used in ≥ 3 Cognitive Mesh agent workflows |
| Re-index time (full) | < 10 minutes for all repos |
| Re-index time (incremental) | < 60 seconds per repo |

---

## 11. Risks & Mitigations

| Risk | Likelihood | Mitigation |
|------|-----------|------------|
| Tree-sitter C# support gaps | Medium | Fallback to Roslyn analyzer via subprocess |
| Graph query performance at scale | Low | Kuzu is embedded + columnar — fast for this scale |
| Embedding model quality | Medium | Evaluate nomic vs. code-specific models (CodeBERT) |
| Repo access auth complexity | Low | Use GitHub PAT scoped to org read |
| Index staleness | Medium | Webhook + scheduled fallback |

---

## 12. Open Questions

- [ ] Kuzu vs Neo4j — embedded simplicity vs query power at 20+ repo scale?
- [ ] Should `get_change_impact` do static analysis or rely on graph traversal only?
- [ ] Multi-org support needed? (`phoenixvc` + `justaghost` + future orgs)
- [ ] Should project metadata registry be code-defined (YAML) or UI-managed?
- [ ] Expose read-only HTTP API alongside MCP for non-agent consumers?

---

## 13. Repo & Naming

```
Repo:       phoenixvc/arch-context-mcp
MCP Name:   arch-context
Docker:     ghcr.io/phoenixvc/arch-context-mcp:latest
Config:     arch-context.config.yaml
```

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
*"Built on Truth, Forged in Work, Governed by Principle."*
