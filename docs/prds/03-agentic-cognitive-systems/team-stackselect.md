# PRD: StackSelect Agent Team

**Project:** StackSelect
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

Evaluate and recommend optimal technology stacks for new projects by systematically analyzing requirements, scanning available technologies, checking compatibility with existing repositories, estimating costs, and producing a defensible recommendation with full rationale.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | RequirementsAnalyzer | Parses project requirements into structured capability needs, non-functional requirements, and constraints |
| 2 | TechRadarScanner | Evaluates technology options against maturity, community support, ecosystem health, and trend trajectory |
| 3 | CompatibilityChecker | Checks candidate stack compatibility with existing repos, shared libraries, deployment infrastructure, and team skills |
| 4 | CostEstimator | Estimates infrastructure costs, licensing fees, migration effort, and total cost of ownership for each candidate stack |
| 5 | StackRecommender | Synthesizes all agent outputs into a final ranked recommendation with trade-off analysis and rationale |

---

## 3. Workflow

1. **RequirementsAnalyzer** ingests the project brief and produces a structured requirements manifest (functional needs, scale targets, compliance constraints, timeline).
2. **TechRadarScanner** takes the requirements manifest and generates a long-list of candidate technologies for each layer of the stack (language, framework, database, infrastructure, observability).
3. **CompatibilityChecker** evaluates each candidate against existing PhoenixVC/Nexamesh repositories, CI/CD pipelines, and team expertise — flagging conflicts and synergies.
4. **CostEstimator** produces cost projections for the top candidate stacks, covering cloud infrastructure, licensing, developer tooling, and migration from current state.
5. **StackRecommender** consolidates all inputs, applies weighted scoring, and produces a final recommendation document with primary recommendation, alternatives, and explicit trade-offs.

---

## 4. Integration Points

- **DesignPanel** — receives the selected stack as input for architecture design
- **RoadmapCrew** — stack decisions feed into roadmap planning as technical milestones
- **All new projects** — StackSelect is invoked at project inception before any code is written
- **arch-context-mcp** — queries existing repository metadata for compatibility analysis

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
