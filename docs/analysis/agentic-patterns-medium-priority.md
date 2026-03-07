# Agentic Patterns — Medium Priority

> **Source**: [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns)
> **Status**: Missing or partial patterns that improve capability but aren't critical
> **Related**: [Coverage](./agentic-pattern-coverage.md) | [High](./agentic-patterns-high-priority.md) | [Low](./agentic-patterns-low-priority.md) | [Antipatterns](./agentic-antipattern-analysis.md)
> **Date**: 2026-03-07

## Selection Criteria

Medium priority patterns are **missing or partial** and:
- Would improve agent capability or user experience
- Have clear implementation paths
- Are beneficial but not blocking for production
- Can be deferred without significant risk

---

## Orchestration & Control (10 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Oracle and Worker Multi-Model Approach** | 🟡 Partial | Formal hierarchy | Add oracle/worker protocol to `MultiAgentOrchestrationEngine` |
| **Multi-Model Orchestration for Complex Edits** | 🟡 Partial | Better edit quality | Orchestrate multiple models for complex editing tasks |
| **Swarm Migration Pattern** | 🟡 Partial | Topology flexibility | Add migration between swarm topologies |
| **Progressive Complexity Escalation** | 🟡 Partial | Graduated autonomy | Add escalation workflow based on task complexity |
| **Specification-Driven Agent Development** | 🟡 Partial | Better agent definition | Formalize spec-driven agent creation |
| **Hybrid LLM/Code Workflow Coordinator** | 🟡 Partial | Mixed execution | Add code execution paths to `DurableWorkflowEngine` |
| **Distributed Execution with Cloud Workers** | 🟡 Partial | Horizontal scaling | Add distributed worker pool |
| **Feature List as Immutable Contract** | 🟡 Partial | Feature stability | Make feature flags immutable once deployed |
| **LLM Map-Reduce Pattern** | ❌ Missing | Large-scale processing | Add map-reduce decomposition for LLM tasks |
| **Recursive Best-of-N Delegation** | ❌ Missing | Quality improvement | Add best-of-N sampling to agent responses |

---

## Context & Memory (6 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Agent-Powered Codebase Q&A / Onboarding** | ❌ Missing | Developer productivity | Add codebase Q&A agent using existing RAG |
| **Curated Code Context Window** | ❌ Missing | Better code context | Add code-specific context curation |
| **Curated File Context Window** | ❌ Missing | Better file context | Add file-specific context selection |
| **Self-Identity Accumulation** | ❌ Missing | Agent continuity | Add identity persistence across sessions |
| **Working Memory via TodoWrite** | ❌ Missing | Task tracking | Add TodoWrite-style working memory |
| **Progressive Disclosure for Large Files** | ❌ Missing | Better file handling | Add progressive disclosure for large documents |

---

## UX & Collaboration (6 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Chain-of-Thought Monitoring & Interruption** | 🟡 Partial | User control | Add runtime interruption to `TransparencyManager` |
| **Democratization of Tooling via Agents** | 🟡 Partial | Self-service tools | Add tool creation interface |
| **Seamless Background-to-Foreground Handoff** | 🟡 Partial | Better UX | Add explicit handoff transitions |
| **Team-Shared Agent Configuration as Code** | 🟡 Partial | GitOps for agents | Store agent configs in repo |
| **Codebase Optimization for Agents** | 🟡 Partial | Agent-friendly code | Add agent optimization guidelines |
| **Latent Demand Product Discovery** | ❌ Missing | Product insights | Extend `CustomerIntelligenceManager` for latent demand |

---

## Tool Use & Environment (8 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Dual-Use Tool Design** | 🟡 Partial | Human+agent tools | Add human-facing tool interfaces |
| **Multi-Platform Communication Aggregation** | 🟡 Partial | Unified comms | Add Slack/Teams/webhook integration |
| **Tool Use Steering via Prompting** | 🟡 Partial | Better tool selection | Add explicit steering prompts |
| **Progressive Tool Discovery** | ❌ Missing | Dynamic tools | Add allowlisted tool discovery (with guardrails) |
| **Patch Steering via Prompted Tool Selection** | ❌ Missing | Guided patching | Add prompt-based tool steering |
| **Visual AI Multimodal Integration** | ❌ Missing | Image/video support | Add multimodal processing |
| **AI Web Search Agent Loop** | ❌ Missing | Web research | Add web search agent |
| **Multi-Platform Webhook Triggers** | ❌ Missing | Event-driven agents | Add webhook trigger support |

---

## Feedback Loops (5 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Self-Discover: LLM Self-Composed Reasoning** | 🟡 Partial | Adaptive reasoning | Allow LLM to compose reasoning structures |
| **AI-Assisted Code Review / Verification** | ❌ Missing | Code quality | Add code review agent |
| **Background Agent with CI Feedback** | ❌ Missing | CI integration | Add CI feedback to agent loop |
| **Coding Agent CI Feedback Loop** | ❌ Missing | Code+CI integration | Add coding agent with CI |
| **Dogfooding with Rapid Iteration** | ❌ Missing | Self-improvement | Add self-improvement iteration |

---

## Reliability & Eval (4 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **CriticGPT-Style Code Review** | ❌ Missing | Code critique | Add critic-based review |
| **Asynchronous Coding Agent Pipeline** | ❌ Missing | Async code generation | Add async coding pipeline |
| **Adaptive Sandbox Fan-Out Controller** | ❌ Missing | Parallel sandboxes | Add sandbox fan-out |
| **Merged Code + Language Skill Model** | ❌ Missing | Code-specialized model | Integrate code-specialized model |

---

## Learning & Adaptation (3 patterns)

| Pattern | Status | Benefit | Implementation Path |
|---------|--------|---------|---------------------|
| **Compounding Engineering Pattern** | 🟡 Partial | Knowledge accumulation | Add insight compounding to `LearningInsight` |
| **Skill Library Evolution** | ❌ Missing | Dynamic skills | Add skill acquisition to `ToolDefinitions` |
| **Shipping as Research** | 🟡 Partial | Research loop | Formalize research-shipping loop |

---

## Summary

| Category | Total | Medium Priority |
|----------|-------|-----------------|
| Orchestration & Control | 44 | 10 |
| Context & Memory | 17 | 6 |
| UX & Collaboration | 16 | 6 |
| Tool Use & Environment | 23 | 8 |
| Feedback Loops | 14 | 5 |
| Reliability & Eval | 18 | 4 |
| Learning & Adaptation | 7 | 3 |
| **Total** | **139** | **42** |

---

## Implementation Notes

### Quick Wins (< 1 sprint each)
- Team-Shared Agent Configuration as Code
- Working Memory via TodoWrite
- Progressive Disclosure for Large Files
- Multi-Platform Webhook Triggers

### Medium Effort (1-2 sprints each)
- Agent-Powered Codebase Q&A / Onboarding
- AI-Assisted Code Review / Verification
- Background Agent with CI Feedback
- Chain-of-Thought Monitoring & Interruption

### Larger Initiatives (2+ sprints)
- Visual AI Multimodal Integration
- Merged Code + Language Skill Model
- Skill Library Evolution
- Distributed Execution with Cloud Workers
