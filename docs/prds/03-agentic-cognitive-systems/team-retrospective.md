# PRD: Retrospective Agent Team

**Project:** Retrospective
**Owner:** Hans Jurgens Smit — PhoenixVC / Nexamesh AI
**Status:** Draft v0.1
**Date:** 8 March 2026
**Classification:** Internal — Cognitive Mesh Infrastructure

---

## 1. Purpose

Retrospective is a retrospective analysis team that gathers project and sprint metrics, identifies recurring issues and process bottlenecks, proposes evidence-based improvements, and tracks the implementation of improvement actions. The team drives continuous process improvement across the Cognitive Mesh development lifecycle.

---

## 2. Team Composition

| # | Agent | Role |
|---|-------|------|
| 1 | MetricsCollector | Gathers quantitative sprint and project metrics including velocity, cycle time, defect rates, PR review latency, and deployment frequency |
| 2 | PatternDetector | Identifies recurring issues, failure modes, and process anti-patterns by analyzing historical metrics, incident reports, and team feedback |
| 3 | ImprovementProposer | Suggests targeted process improvements based on detected patterns, drawing from industry best practices and organizational context |
| 4 | ActionTracker | Tracks improvement actions from proposal through implementation, monitoring adoption and measuring the impact of changes |

---

## 3. Workflow

1. **Collect**: MetricsCollector gathers data from GitHub (PR metrics, issue resolution times), CI/CD (build times, failure rates), and project management tools at the end of each sprint or milestone.
2. **Detect**: PatternDetector analyzes the collected metrics alongside historical data to identify trends, recurring bottlenecks, and systemic issues.
3. **Propose**: ImprovementProposer generates specific, actionable improvement proposals with expected outcomes, effort estimates, and success criteria.
4. **Prioritize**: The team ranks proposals by expected impact, implementation effort, and alignment with team goals.
5. **Track**: ActionTracker monitors the adoption of approved improvements, collecting follow-up metrics to measure their effectiveness.
6. **Report**: A retrospective report is produced summarizing findings, actions taken, and measured outcomes for team review.

---

## 4. Integration Points

- **RoadmapCrew**: Feeds improvement insights into roadmap planning to ensure process improvements are allocated time and resources.
- **All teams**: Collects metrics and feedback from every agent team to identify cross-cutting improvement opportunities.
- **CI/CD Pipelines**: Extracts build, test, and deployment metrics from GitHub Actions workflows.
- **GitHub**: Analyzes PR review patterns, issue lifecycle metrics, and contributor activity across all repositories.

---

*PRD Owner: Hans Jurgens Smit | PhoenixVC / Nexamesh AI | House of Veritas*
