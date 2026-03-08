# Agentic Patterns — Index

> **Source**: [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns) (147 patterns)
> **Date**: 2026-03-07

## Document Suite

| Document | Purpose | Pattern Count |
|----------|---------|---------------|
| [Coverage Analysis](./agentic-pattern-coverage.md) | Full mapping of 147 patterns to cognitive-mesh | 147 + 12 extra |
| [High Priority](./agentic-patterns-high-priority.md) | Critical missing patterns for production | 30 |
| [Medium Priority](./agentic-patterns-medium-priority.md) | Beneficial patterns to improve capability | 42 |
| [Low Priority](./agentic-patterns-low-priority.md) | Nice-to-have or not architecturally aligned | 35 |
| [Antipatterns](./agentic-antipattern-analysis.md) | 19 patterns classified by risk | 19 |

---

## Pattern Distribution

```
                        Total: 147 patterns
    ┌────────────────────────────────────────────────────┐
    │ ✅ Fully Implemented (31)                    21%   │
    ├────────────────────────────────────────────────────┤
    │ 🟡 Partially Implemented (27)                18%   │
    ├────────────────────────────────────────────────────┤
    │ ❌ Missing - High Priority (30)              20%   │
    ├────────────────────────────────────────────────────┤
    │ ❌ Missing - Medium Priority (24)            16%   │
    ├────────────────────────────────────────────────────┤
    │ ❌ Missing - Low Priority (35)               24%   │
    └────────────────────────────────────────────────────┘

    + 12 Extra patterns unique to cognitive-mesh
    + 19 Antipattern classifications (overlaps with above)
```

---

## By Category

| Category | Full | Partial | High | Medium | Low | Total |
|----------|------|---------|------|--------|-----|-------|
| Context & Memory | 4 | 3 | 4 | 6 | 0 | 17 |
| Feedback Loops | 3 | 3 | 3 | 5 | 0 | 14 |
| Learning & Adaptation | 0 | 2 | 0 | 3 | 2 | 7 |
| Orchestration & Control | 14 | 14 | 8 | 10 | 8 | 44 |
| Reliability & Eval | 3 | 2 | 7 | 4 | 2 | 18 |
| Security & Safety | 2 | 2 | 5 | 0 | 0 | 9 |
| Tool Use & Environment | 4 | 5 | 3 | 8 | 3 | 23 |
| UX & Collaboration | 5 | 5 | 0 | 6 | 0 | 16 |
| **Total** | **35** | **36** | **30** | **42** | **15** | **148** |

*Note: Some partial patterns appear in priority lists for completion.*

---

## Implementation Roadmap Summary

### Phase 1: Security Foundation (Sprint 1-2)
From [High Priority](./agentic-patterns-high-priority.md#phase-1-security-foundation-sprint-1-2):
- Deterministic Security Scanning Build Loop
- External Credential Sync
- PII Tokenization
- Egress Lockdown

### Phase 2: Reliability Infrastructure (Sprint 3-4)
From [High Priority](./agentic-patterns-high-priority.md#phase-2-reliability-infrastructure-sprint-3-4):
- Canary Rollout and Automatic Rollback
- Failover-Aware Model Fallback
- Action Caching & Replay
- Workflow Evals with Mocked Tools

### Phase 3: Orchestration Enhancement (Sprint 5-6)
From [High Priority](./agentic-patterns-high-priority.md#phase-3-orchestration-enhancement-sprint-5-6):
- Budget-Aware Model Routing
- Dual LLM Pattern formalization
- Tree-of-Thought Reasoning
- Graph of Thoughts integration

### Phase 4: Context & Feedback (Sprint 7-8)
From [High Priority](./agentic-patterns-high-priority.md#phase-4-context--feedback-sprint-7-8):
- Context Window Auto-Compaction
- Memory Synthesis from Execution Logs
- Incident-to-Eval Synthesis
- Spec-As-Test Feedback Loop

---

## Quick Reference

### Patterns to Avoid
See [Antipattern Analysis](./agentic-antipattern-analysis.md) for 19 patterns with risk classifications:
- 4 High Risk (never implement)
- 9 Medium Risk (requires guardrails)
- 6 Low Risk (situational)

### Extra Patterns (Contribute to Catalog?)
See [Coverage Analysis](./agentic-pattern-coverage.md#9-extra-patterns-in-cognitive-mesh) for 12 patterns unique to cognitive-mesh:
- Cross-Cultural Adaptation (Hofstede 6-D)
- EU AI Act Compliance Engine
- GDPR Compliance Engine
- Ethical Reasoning (Brandom + Floridi)
- Psychological Safety Metrics
- Impact-First Measurement
- Organizational Blindness Detection
- Champion Nudger Agent
- Strategic Simulation Engine
- Competitive Execution Pattern
- Collaborative Swarm Convergence
- Value Generation Engine

---

## Source Updates

The [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns) catalog is actively maintained:

- **Current count**: 147 patterns (was 108 in May 2025)
- **Commit tracked**: f3628c5
- **Stars**: 3.5k
- **License**: Apache 2.0
- **Origin**: Sourcegraph AI coding agents writeup (May 2025)

New patterns added to catalog should be evaluated and added to appropriate priority file.
