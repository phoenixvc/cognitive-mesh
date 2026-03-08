# PRD: Domain Agent Coverage — Project-to-Team Mapping

**Project:** Domain Agent Coverage
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

Map every PhoenixVC project/repository to the agent teams and domain agents that serve it, ensuring complete coverage and identifying gaps.

---

## 2. Repository Coverage Matrix

| Repo | Standard Teams | Domain Agents | Strategic Teams | Status |
|------|---------------|--------------|-----------------|--------|
| cognitive-mesh | 10 (hand-crafted) | — | All (definition hub) | Active |
| agentkit-forge | 10 (self-hosting) | — | TeamForge consumer | Active |
| houseofveritas | 11 (hand-crafted) | — | — | Active |
| chaufher | 10 + 20 domain | brand-guardian, growth-analyst, content-strategist, + 17 more | — | Active |
| pvc-costops-analytics | 10 (generated) | — | — | Active |
| azure-infrastructure | 10 (pending) | bicep-validator, module-publisher, naming-enforcer | — | Bootstrap P1 |
| ai-gateway | 10 (pending) | gateway-ops, model-registry | — | Bootstrap P1 |
| codeflow-engine | 10 (pending) | — | — | Bootstrap P2 |
| Mystira.workspace | 10 (pending) | (component-scoped) | — | Bootstrap P2 |
| PhoenixVC-Website | 10 (pending) | — | — | Bootstrap P2 |
| PhoenixRooivalk | 10 (pending) | detector, migrate, dev (existing) | ContractAuditor | Bootstrap P2 |

---

## 3. Project-to-Team Mapping

### VeritasVault (Financial/Investment Platform)
- **Repos**: cognitive-mesh (reasoning layer), ai-gateway (model access)
- **Teams**: WealthMind, ContractAuditor, SecurityAudit, DataArchitect
- **Key Agents**: PortfolioAnalyzer, RiskModeler, ComplianceMonitor

### ChaufHer (Ride-Hailing Platform)
- **Repos**: chaufher (monorepo)
- **Teams**: Standard 10 + domain agents (brand-guardian, growth-analyst, etc.)
- **Key Agents**: All 30 chaufher agents

### Mystira (AI Storytelling Platform)
- **Repos**: Mystira.workspace (9-component monorepo)
- **Teams**: Standard 10, DesignPanel, APIDesigner
- **Key Agents**: Per-component scoped agents (TBD after bootstrap)

### PhoenixRooivalk (Counter-Drone System)
- **Repos**: PhoenixRooivalk
- **Teams**: Standard 10, ContractAuditor (Solana), SecurityAudit
- **Key Agents**: detector, migrate, dev (existing domain agents)

### Cognitive Mesh (Enterprise AI Platform)
- **Repos**: cognitive-mesh, ai-gateway, azure-infrastructure
- **Teams**: All 19 ecosystem teams (definition hub)
- **Key Agents**: All strategic and engineering quality teams

### PhoenixVC (Venture Capital Operations)
- **Repos**: PhoenixVC-Website, pvc-costops-analytics, azure-infrastructure
- **Teams**: Standard 10, BrandForge, StartupSim, StakeholderSim
- **Key Agents**: InvestorSimulator, FinancialModeler, BrandStrategist

---

## 4. Coverage Gaps

| Gap | Impact | Resolution |
|-----|--------|-----------|
| Mystira domain agents unknown | Can't scope component agents | Bootstrap first, then identify domain needs |
| PhoenixRooivalk Solana agents | ContractAuditor team not built | Build after bootstrap |
| Cross-repo testing | No unified test runner across repos | TestForge team addresses this |
| Release coordination across repos | No unified release process | ReleasePilot team addresses this |

---

## 5. Priority Order

1. **Immediate**: azure-infrastructure, ai-gateway (foundation repos)
2. **Soon**: codeflow-engine (active development)
3. **Next**: Mystira.workspace (complex monorepo needs planning)
4. **Later**: PhoenixVC-Website, PhoenixRooivalk (lower priority)

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
