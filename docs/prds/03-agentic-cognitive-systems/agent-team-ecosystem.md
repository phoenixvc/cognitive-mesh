# PRD: Agent Team Ecosystem — Master Overview

**Project:** PhoenixVC Agent Team Ecosystem
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

Define the complete agent team ecosystem spanning all PhoenixVC repositories. This document serves as the master index for all teams, their relationships, and the build order for bringing the ecosystem to full capability.

---

## 2. Ecosystem Architecture

```
                    arch-context-mcp
                    (Knowledge Hub)
                          |
                     MCP Protocol
          +---------+----+----+---------+
          |         |         |         |
     cognitive  agentkit  chaufher  azure-infra
       mesh      forge   (reference)  ai-gateway
    (Definitions) (Distribution)     + 4 more repos
```

### Roles

| Component | Role |
|-----------|------|
| arch-context-mcp | Knowledge layer — indexes repos, agent registry, format normalization |
| cognitive-mesh | Definition hub — canonical team specs, PRDs, issues |
| agentkit-forge | Distribution layer — generates per-repo configs (Claude, Copilot, Cursor) |
| chaufher | Reference implementation — 30 agents, most mature setup |

---

## 3. Complete Team Registry

### Tier 1: Core Development Teams (10 standard agentkit-forge teams)
Available in every bootstrapped repo via `agentkit sync`.

| Team | Scope | Description |
|------|-------|-------------|
| backend | Per-repo | Backend API and services development |
| frontend | Per-repo | UI and frontend development |
| data | Per-repo | Database, migrations, data modeling |
| infra | Per-repo | Infrastructure and IaC |
| devops | Per-repo | CI/CD, deployments, monitoring |
| testing | Per-repo | Test strategy and execution |
| security | Per-repo | Security scanning and compliance |
| docs | Per-repo | Documentation |
| product | Per-repo | Product requirements and specs |
| quality | Per-repo | Code quality and standards |

### Tier 2: Strategic Teams (ecosystem-wide)

| Team | Canonical Owner | Agents | Status |
|------|----------------|--------|--------|
| TeamForge | cognitive-mesh | 7 + 3 supporting | Epic #142 |
| RoadmapCrew | cognitive-mesh | 6 | PRD ready |
| HandoverBridge | cognitive-mesh | 5 | PRD ready |
| Planning Pipeline | cognitive-mesh | 8 (2 phases) | PRD ready |
| StackSelect | cognitive-mesh | 5 | Outlined |
| DesignPanel | cognitive-mesh | 5 | Outlined |

### Tier 3: Domain Teams

| Team | Canonical Owner | Agents | Status |
|------|----------------|--------|--------|
| StartupSim | cognitive-mesh | 6 | Outlined |
| BrandForge | cognitive-mesh | 5 | Outlined |
| WealthMind | cognitive-mesh | 5 | Outlined |
| ContractAuditor | cognitive-mesh | 4 | Outlined |
| StakeholderSim | cognitive-mesh | 5 | Outlined |

### Tier 4: Engineering Quality Teams (cross-repo)

| Team | Canonical Owner | Agents | Status |
|------|----------------|--------|--------|
| DebugSquad | cognitive-mesh | 5 | Outlined |
| TestForge | cognitive-mesh | 5 | Outlined |
| SecurityAudit | cognitive-mesh | 5 | Outlined |
| DocsCrew | cognitive-mesh | 5 | Outlined |
| APIDesigner | cognitive-mesh | 5 | Outlined |
| ResearchDesk | cognitive-mesh | 4 | Outlined |
| ReleasePilot | cognitive-mesh | 5 | Outlined |
| RetroSpective | cognitive-mesh | 4 | Outlined |
| DataArchitect | cognitive-mesh | 5 | Outlined |

### Domain-Specific Agents (repo-level, not promoted to ecosystem)

| Repo | Agent | Team |
|------|-------|------|
| azure-infrastructure | bicep-validator | infra |
| azure-infrastructure | module-publisher | devops |
| azure-infrastructure | naming-enforcer | quality |
| ai-gateway | gateway-ops | devops |
| ai-gateway | model-registry | backend |
| chaufher | brand-guardian | (chaufher-only) |
| chaufher | growth-analyst | (chaufher-only) |
| chaufher | content-strategist | (chaufher-only) |

---

## 4. Config Formats

| Format | Repos | Status |
|--------|-------|--------|
| AgentKit Forge YAML | agentkit-forge, chaufher, pvc-costops-analytics, + 6 bootstrapping | Canonical |
| Hand-crafted `.claude/commands/` | cognitive-mesh, houseofveritas | Compatible (read by arch-context-mcp) |
| VS Code Agent Plugins | (evaluation pending) | Future distribution channel |

---

## 5. Build Order

| Phase | Work | Dependencies |
|-------|------|-------------|
| 0 | Bootstrap azure-infrastructure + ai-gateway | None |
| 1 | Domain agents + bootstrap codeflow-engine | Phase 0 |
| 2 | Ecosystem registry + arch-context-mcp agent indexing | Phase 1 + arch-context-mcp core |
| 3 | Bootstrap Mystira, PhoenixRooivalk, PhoenixVC-Website | Phase 2 |
| 4 | Cross-repo teams (DebugSquad, SecurityAudit) | Phase 2 |
| 5 | Strategic teams (RoadmapCrew, HandoverBridge, etc.) | Phase 2 |
| 6 | Domain teams (StartupSim, BrandForge, etc.) | Phase 5 |

---

## 6. Success Metrics

| Metric | Target |
|--------|--------|
| Repos with agent configs | 11/11 |
| Teams operational | 19 ecosystem + 10 standard per-repo |
| Config format coverage | 100% parseable by arch-context-mcp |
| Cross-repo handover | Functional between any 2 repos |
| VS Code agent plugin eval | Complete |

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
