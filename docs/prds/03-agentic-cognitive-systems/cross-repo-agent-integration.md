# PRD: Cross-Repo Agent Integration Protocol

**Project:** Cross-Repo Agent Coordination
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Problem Statement

PhoenixVC operates 20+ repositories with three distinct agent configuration formats:

1. **Hand-crafted `.claude/commands/`** markdown (cognitive-mesh, houseofveritas, PhoenixRooivalk)
2. **AgentKit Forge YAML spec** generating multi-tool outputs (agentkit-forge, chaufher, pvc-costops-analytics)
3. **No agents** (azure-infrastructure, ai-gateway, codeflow-engine, Mystira, PhoenixVC-Website)

There is no protocol for cross-repo agent coordination: teams cannot share context across repo boundaries, there is no canonical team registry, and config drift between repos goes undetected.

---

## 2. Objective

Define the integration protocol that enables agent teams to operate across repository boundaries, normalize agent configurations across formats, and maintain a living ecosystem registry via arch-context-mcp.

---

## 3. Hub-and-Spoke Architecture

```
                    +---------------------------+
                    |     arch-context-mcp       |
                    |     (Knowledge Hub)        |
                    |  Graph + Vectors + Notion  |
                    |  + Agent Registry + MCP    |
                    +-------------+-------------+
                                  | MCP Protocol
     +----------+-----------------+----------------+-----------+
     |          |                 |                |           |
cognitive   agentkit          chaufher        azure-infra   ai-gateway
  mesh       forge           (30 agents)     (IaC hub)    (AI proxy)
(Definitions) (Distribution)  (.agentkit/)   (bootstrap)  (bootstrap)
```

### Role Assignments

| Component | Role | Responsibilities |
|-----------|------|-----------------|
| **arch-context-mcp** | Knowledge Hub | Index all repos, serve context, maintain agent registry, normalize formats |
| **cognitive-mesh** | Definition Hub | Canonical team specs (PRDs, issues), ecosystem-wide team definitions |
| **agentkit-forge** | Distribution Layer | Generate per-repo agent configs, sync across tools (Claude, Copilot, Cursor) |
| **chaufher** | Reference Implementation | Most mature setup (30 agents), pattern to follow for new repos |

---

## 4. Agent Config Format Normalization

### Canonical Format: AgentKit Forge YAML

```yaml
# .agentkit/spec/project.yaml
project:
  name: azure-infrastructure
  description: Azure IaC standards and templates
  stack: [powershell, python, bicep, terraform, csharp]
  domain: infrastructure

# .agentkit/spec/teams.yaml (standard 10 teams)
teams:
  - id: backend
  - id: frontend
  - id: data
  - id: infra
  - id: devops
  - id: testing
  - id: security
  - id: docs
  - id: product
  - id: quality

# .agentkit/spec/agents.yaml (domain-specific)
agents:
  - id: bicep-validator
    team: infra
    description: Validates Bicep module structure
```

### Generation Pipeline

```
.agentkit/spec/ (YAML)
    |
    v  agentkit sync
+-- .claude/commands/     (Claude Code)
+-- .claude/agents/       (Claude Code agents)
+-- .github/copilot/      (GitHub Copilot)
+-- .cursor/rules/        (Cursor)
+-- AGENTS.md             (Universal)
+-- [future] VS Code agent plugin manifest
```

### Hand-Crafted Compatibility

For repos that keep hand-crafted configs (cognitive-mesh):
- arch-context-mcp reads `.claude/commands/team-*.md` files directly
- Extracts team name, scope, agents from markdown structure
- Normalizes into the same schema as AgentKit YAML output
- No migration required

---

## 5. Ecosystem Team Registry

### Registry Location

The registry is constructed dynamically by arch-context-mcp from two sources:

1. **Per-repo agent configs** — indexed by Repository Indexer
2. **Ecosystem overlay** — `ecosystem-teams.yaml` in arch-context-mcp repo

### Ecosystem Overlay Schema

```yaml
# ecosystem-teams.yaml
version: "1.0"
ecosystem_teams:
  - id: debug-squad
    scope: cross-repo
    canonical_owner: cognitive-mesh
    repos: [cognitive-mesh, chaufher, codeflow-engine]
    capabilities: [crash-analysis, log-tracing, performance-profiling]

  - id: security-audit
    scope: cross-repo
    canonical_owner: cognitive-mesh
    repos: all
    capabilities: [dependency-scan, secret-detection, compliance-check]

  - id: iac-standards
    scope: cross-repo
    canonical_owner: azure-infrastructure
    repos: [azure-infrastructure, ai-gateway, chaufher, cognitive-mesh]
    capabilities: [bicep-validation, terraform-plan, naming-enforcement]
```

### MCP Tools for Registry

| Tool | Input | Output |
|------|-------|--------|
| `get_agent_config` | repo name/path | Normalized agent/team inventory (format-agnostic) |
| `find_ecosystem_agent` | capability query | Repos with matching agents, config paths |
| `get_team_registry` | (optional) team filter | Full ecosystem team registry |

---

## 6. Cross-Repo HandoverBridge Protocol

When HandoverBridge operates across repo boundaries:

1. **Source Agent** completes work in Repo A, generates handover artifact
2. **HandoverBridge** detects target is in Repo B (different `.claude/` context)
3. **Context Transfer**: Key decisions, file paths, and rationale are serialized to a handover manifest
4. **Target Resolution**: arch-context-mcp resolves Repo B's agent config and identifies the appropriate receiving team
5. **Receiving Agent** in Repo B loads handover manifest and continues work

### Handover Manifest Schema

```json
{
  "source_repo": "cognitive-mesh",
  "source_team": "team-agency",
  "target_repo": "ai-gateway",
  "target_team": "devops",
  "context": {
    "task_summary": "Update gateway model config for new embedding deployment",
    "decisions": ["Use text-embedding-3-large-v2", "Keep fallback to v1"],
    "files_modified": ["src/AgencyLayer/..."],
    "files_to_modify": ["infra/modules/aigateway_aca/..."]
  },
  "created_at": "2026-03-08T12:00:00Z"
}
```

---

## 7. Config Sync Protocol

### agentkit-forge as Single Source of Truth

```
agentkit-forge                    Target Repos
+-------------------+            +------------------+
| .agentkit/spec/   | --sync-->  | .agentkit/spec/  |
| templates/        |            | .claude/         |
| overlays/         |            | AGENTS.md        |
+-------------------+            +------------------+
         |
         v
   Drift Detection
   (CI check: compare generated vs committed)
```

### Drift Detection

GitHub Action in each repo:
1. Run `agentkit sync --dry-run`
2. Compare output with committed files
3. If diff exists, open PR or flag in CI

---

## 8. Repo Onboarding Standard

### Steps to Add Agents to a Bare Repo

1. `agentkit init` — generates `.agentkit/spec/project.yaml` from repo analysis
2. `agentkit sync` — generates all config files from YAML spec
3. Add domain-specific agents to `.agentkit/spec/agents.yaml` (if needed)
4. `agentkit sync` again — regenerates with domain agents
5. Commit and push
6. arch-context-mcp re-indexes (webhook or manual trigger)

---

## 9. Team Overlap Resolution

| Team Capability | Canonical Owner | Rationale |
|----------------|----------------|-----------|
| Security scanning | cognitive-mesh (SecurityAudit) | Cross-repo concern, needs full ecosystem context |
| Quality gates | agentkit-forge (standard `quality` team) | Generated consistently across repos |
| Infrastructure validation | azure-infrastructure (domain agents) | IaC expertise is repo-specific |
| Testing strategy | cognitive-mesh (TestForge) | Cross-repo concern |
| Brand consistency | chaufher (brand-guardian) | Domain-specific, not ecosystem-wide |

---

## 10. Success Metrics

| Metric | Target |
|--------|--------|
| Repos with agent configs | 11/11 (all explored repos) |
| Config format coverage | 100% repos parseable by arch-context-mcp |
| Cross-repo handover success rate | > 90% |
| Config drift detection | < 24h to detect |
| Team registry completeness | All teams across all repos indexed |

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
