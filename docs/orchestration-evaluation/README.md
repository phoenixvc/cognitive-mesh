# Agent Orchestration Evaluation

Weighted evaluation of agent orchestration approaches across **4 internal repositories**, **19+ external engines/frameworks**, **3 cloud agent platforms**, and **2 interoperability protocols**, scored on 8 metrics with decimal precision (1.0–5.0 → 20%–100%).

## Key Findings

- **agentkit-forge** leads internal repos in both Interactive (78.6%) and Batch (73.8%) profiles due to deterministic lifecycle and strong auditability.
- **Temporal** is the top external engine for durable/batch workloads (91.4%); **Inngest** leads for event-driven serverless (82.0%).
- **Azure AI Foundry** leads managed agent platforms (76.6% Interactive, 77.8% Batch) — strongest .NET support and enterprise integration.
- **Custom implementations can compete on governance and determinism** but should not try to replicate Temporal's durable execution — see [Custom vs Established Analysis](09-custom-vs-established/custom-vs-established.md).
- Significant orchestration responsibility overlap exists across internal repos — see the [Deduplication Map](07-deduplication-map/consolidation-analysis.md) for consolidation paths.

## Documentation Map

| Section | Description |
|---------|-------------|
| [01 — Methodology](01-methodology/) | Scoring framework, weight profiles, maturity signals |
| [02 — Internal Repos](02-internal-repos/) | Analysis of agentkit-forge, codeflow-engine, cognitive-mesh, HouseOfVeritas |
| [03 — External Engines](03-external-engines/) | Workflow engines, agent runtimes, coding-agent fleet orchestration, managed agent platforms, protocols |
| [04 — Use-Case Rankings](04-use-case-rankings/) | Ranked lists by workload profile (Interactive, Batch, Durable, Event-driven, Multi-agent) |
| [05 — Config Defaults Matrix](05-config-defaults-matrix/) | Unified comparison of timeouts, retries, concurrency across all systems |
| [06 — Integration Playbooks](06-integration-playbooks/) | How to integrate internal repos with top-ranked external engines |
| [07 — Deduplication Map](07-deduplication-map/) | Overlap analysis and consolidation paths |
| [08 — Gaps & Future](08-gaps-and-future/) | Missing evidence, benchmark spec, operational runbooks |
| [09 — Custom vs Established](09-custom-vs-established/) | Gap analysis, niche opportunities, hybrid strategies, build vs buy framework |
| [10 — Agent Prompt Patterns](10-agent-prompt-patterns/) | Actual prompts, system instructions, and behavioral rules used by major platforms |
| [Appendices](appendices/) | Evidence index, glossary, decision tree, migration risk matrix |

## How to Read This Documentation

**Scoring**: All metrics use a 1.0–5.0 decimal scale. Percentage equivalents are `(score / 5) × 100`. Weighted totals apply profile-specific weights to each metric, then convert to percentage.

**Profiles**: Five workload profiles define different weight distributions. Choose the profile matching your deployment context — see [Weight Profiles](01-methodology/weight-profiles.md).

**Rankings**: Use-case documents in `04-use-case-rankings/` rank all evaluated systems (internal + external) with 1st/2nd/3rd designations per workload type.

## Quick Reference: Internal Repo Rankings

> **Note:** Rankings below are scoped to internal repositories only (4 repos). For global rankings including external engines, see [04 — Use-Case Rankings](04-use-case-rankings/).

| Profile | 1st | 2nd | 3rd | 4th |
|---------|-----|-----|-----|-----|
| Interactive | agentkit-forge (78.6%) | codeflow-engine (64.4%) | HouseOfVeritas (64.2%) | cognitive-mesh (62.0%) |
| Batch | agentkit-forge (73.8%) | HouseOfVeritas (70.6%) | codeflow-engine (68.8%) | cognitive-mesh (61.2%) |
