# Agentic Patterns — Low Priority

> **Source**: [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns)
> **Status**: Missing patterns that are nice-to-have, not architecturally aligned, or require RL infrastructure
> **Related**: [Coverage](./agentic-pattern-coverage.md) | [High](./agentic-patterns-high-priority.md) | [Medium](./agentic-patterns-medium-priority.md) | [Antipatterns](./agentic-antipattern-analysis.md)
> **Date**: 2026-03-07

## Selection Criteria

Low priority patterns are **missing** and:
- Nice-to-have but not impactful for current use cases
- Require significant infrastructure not currently planned (RL, CLI)
- Not aligned with cognitive-mesh's web API architecture
- Can be indefinitely deferred without impact

---

## CLI-Specific Patterns (Not Applicable)

These patterns are designed for CLI-native agent systems. cognitive-mesh is web API-centric.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **CLI-First Skill Design** | ❌ Missing | Architecture is web API-centric; no CLI tooling |
| **CLI-Native Agent Orchestration** | ❌ Missing | ASP.NET Core web API; CLI not planned |
| **Shell Command Contextualization** | ❌ Missing | No shell command execution in scope |
| **Intelligent Bash Tool Execution** | ❌ Missing | No bash execution in scope |
| **Code-Over-API Pattern** | ❌ Missing | API-centric design is intentional |

---

## RL/Fine-Tuning Patterns (Infrastructure Not Planned)

These patterns require reinforcement learning or fine-tuning infrastructure not currently in scope.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **Agent Reinforcement Fine-Tuning (Agent RFT)** | ❌ Missing | No RL training loop; fine-tuning not planned |
| **Variance-Based RL Sample Selection** | ❌ Missing | No RL components |
| **Memory Reinforcement Learning (MemRL)** | ❌ Missing | No memory-based RL |
| **RLAIF (RL from AI Feedback)** | ❌ Missing | No RLAIF pipeline |
| **Tool Use Incentivization via Reward Shaping** | ❌ Missing | No reward signals (also antipattern risk) |
| **Inference-Healed Code Review Reward** | ❌ Missing | No reward-based review (also antipattern risk) |
| **Anti-Reward-Hacking Grader Design** | ❌ Missing | No reward system to protect |

---

## VM/Infrastructure Patterns (Not Applicable)

These patterns require VM-level control or infrastructure not in scope.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **Isolated VM per RL Rollout** | ❌ Missing | No VM isolation or RL rollouts |
| **Virtual Machine Operator Agent** | ❌ Missing | No VM operations (also antipattern risk) |
| **Custom Sandboxed Background Agent** | ❌ Missing | No sandboxed background execution |

---

## Niche/Specialized Patterns (Low ROI)

These patterns address specific use cases with limited applicability.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **Context Window Anxiety Management** | ❌ Missing | Treating symptom not cause (see antipattern analysis) |
| **Filesystem-Based Agent State** | ❌ Missing | Redis+DuckDB is superior for production |
| **Prompt Caching via Exact Prefix Preservation** | ❌ Missing | Model-provider optimization; low control |
| **Explicit Posterior-Sampling Planner** | ❌ Missing | Probabilistic planning not required |
| **Three-Stage Perception Architecture** | ❌ Missing | No perception pipeline needed |
| **Agent Modes by Model Personality** | ❌ Missing | Single personality per agent is sufficient |
| **Burn the Boats** | ❌ Missing | Irreversible commitment pattern not applicable |
| **Subject Hygiene for Task Delegation** | ❌ Missing | Current delegation is sufficient |
| **Tool Selection Guide** | ❌ Missing | Agent tool selection is adequate |
| **Lane-Based Execution Queueing** | ❌ Missing | Current queueing is sufficient |

---

## UX Patterns (Limited Applicability)

These patterns address UX concerns not currently in scope.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **Abstracted Code Representation for Review** | ❌ Missing | No code review UI |
| **Agent-Assisted Scaffolding** | ❌ Missing | No scaffolding generation needed |
| **Proactive Trigger Vocabulary** | ❌ Missing | Current trigger mechanisms sufficient |
| **Dev Tooling Assumptions Reset** | ❌ Missing | Not applicable |
| **Milestone Escrow for Agent Resource Funding** | ❌ Missing | No funding model in scope |
| **Soulbound Identity Verification** | ❌ Missing | Current identity model sufficient |
| **Non-Custodial Spending Controls** | ❌ Missing | No spending model in scope |

---

## Workspace-Native Patterns (Different Architecture)

These patterns are for IDE-native or workspace-integrated systems.

| Pattern | Status | Why Low Priority |
|---------|--------|------------------|
| **Workspace-Native Multi-Agent Orchestration** | ❌ Missing | Server-side orchestration; not IDE-native |
| **Code-Then-Execute Pattern** | ❌ Missing | No code-gen-then-execute pipeline |
| **Frontier-Focused Development** | ❌ Missing | Using stable models intentionally |

---

## Summary

| Category | Count | Notes |
|----------|-------|-------|
| CLI-Specific | 5 | Not applicable to web API architecture |
| RL/Fine-Tuning | 7 | Infrastructure not planned |
| VM/Infrastructure | 3 | Not in scope |
| Niche/Specialized | 10 | Low ROI or antipattern-adjacent |
| UX Patterns | 7 | Limited applicability |
| Workspace-Native | 3 | Different architecture |
| **Total** | **35** | |

---

## Reconsideration Triggers

These patterns could be re-prioritized if:

### CLI-Specific Patterns
- cognitive-mesh adds CLI tooling
- Developer productivity use cases require shell integration

### RL/Fine-Tuning Patterns
- Organization builds RL training infrastructure
- Fine-tuning becomes cost-effective at scale
- Reward signals can be made robust

### VM/Infrastructure Patterns
- Multi-tenant isolation requirements emerge
- Untrusted workloads need sandboxing

### UX Patterns
- End-user developer tooling is built
- IDE integrations are prioritized

---

## Relationship to Antipatterns

Several low-priority patterns overlap with the [antipattern analysis](./agentic-antipattern-analysis.md):

| Pattern | Antipattern Classification |
|---------|---------------------------|
| Tool Use Incentivization via Reward Shaping | Medium Risk |
| Inference-Healed Code Review Reward | Medium Risk |
| Context Window Anxiety Management | Medium Risk |
| Virtual Machine Operator Agent | Low Risk |
| Filesystem-Based Agent State | Low Risk |

These patterns are low priority **because** they carry antipattern risk, not despite it.
