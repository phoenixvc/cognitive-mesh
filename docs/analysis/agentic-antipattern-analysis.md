# Agentic Antipattern Analysis

> **Source catalog**: [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns) (147 patterns as of 2026-03-07)
> **Catalog version**: Main branch commit f3628c5 (3.5k stars)
> **Analysis scope**: Patterns that can become antipatterns in production systems
> **Related**: See also [Pattern Coverage Analysis](./agentic-pattern-coverage.md) for full coverage
> **Date**: 2026-03-07

## Overview

Not every pattern in the catalog is unconditionally beneficial. This document identifies 19 patterns (18% of the catalog) that carry significant risk and classifies them by severity, failure mode, and relevance to cognitive-mesh's architecture.

---

## Risk Classification

| Tier | Definition | Count |
|------|-----------|-------|
| 🔴 **High Risk** | Likely antipattern in most production contexts. Introduces systemic failure modes. | 4 |
| 🟠 **Medium Risk** | Antipattern in specific contexts. Requires strict guardrails to remain safe. | 9 |
| 🟡 **Low Risk** | Situational risk. Problematic under specific scaling, concurrency, or security conditions. | 6 |

---

## 🔴 HIGH RISK — Likely Antipatterns

### 1. No-Token-Limit Magic

**Category**: Reliability & Eval
**Failure mode**: Unbounded resource consumption

Removing or ignoring token limits creates:

- **Runaway API costs** — a single malformed loop can consume thousands of dollars in minutes
- **Latency explosion** — unbounded generation blocks downstream consumers
- **Resource starvation** — one agent's unbounded call starves the pool for others

The name itself ("Magic") signals the core problem: it replaces explicit engineering with wishful thinking.

**Why cognitive-mesh avoids it**: All operations are bounded through authority scopes (`AuthorityScopeViewModel.MaxResourceConsumption`) and circuit breaker patterns (`AgentCircuitBreakerPolicy`).

**Recommendation**: Never implement. Use explicit token budgets with monitoring and alerts.

---

### 2. Continuous Autonomous Task Loop

**Category**: Orchestration & Control
**Failure mode**: Drift, cascading failure, unmonitored execution

An agent running indefinitely without human checkpoints will:

- **Drift from intent** — small errors compound over iterations without correction
- **Cascade failures** — one bad decision feeds into the next iteration's input
- **Consume resources silently** — no natural stopping point means no natural cost boundary
- **Evade monitoring** — long-running loops are easy to forget about

**Why cognitive-mesh avoids it**: Task execution requires explicit invocation. The `ActWithConfirmation` autonomy level is the default, requiring human approval at decision points.

**Recommendation**: Use bounded iteration with explicit checkpoints. Every N iterations, require human review or automated evaluation gates.

---

### 3. Self-Rewriting Meta-Prompt Loop

**Category**: Orchestration & Control
**Failure mode**: Loss of reproducibility, adversarial drift

Self-modifying prompts are the agentic equivalent of self-modifying code:

- **Non-reproducible** — the same input produces different behavior depending on iteration history
- **Non-auditable** — you cannot review what the system will do because it changes itself
- **Adversarial drift** — over iterations, the prompt can evolve toward degenerate or adversarial behavior
- **Debugging nightmare** — when something fails, the prompt that caused the failure no longer exists

**Why cognitive-mesh avoids it**: Templates are static and resolved through `IContextTemplateResolver`. Prompt content is architect-defined, not agent-defined.

**Recommendation**: Never implement for production systems. If prompt evolution is needed, use versioned A/B testing with human approval gates.

---

### 4. Stop Hook Auto-Continue

**Category**: Orchestration & Control
**Failure mode**: Bypasses intentional safety stops

If a process was stopped, there was a reason. Auto-continuing:

- **Bypasses safety gates** — the stop might have been triggered by a guardrail
- **Masks errors** — transient failures that resolve on retry hide underlying issues
- **Creates infinite retry loops** — combined with other patterns, can loop indefinitely
- **Violates separation of concerns** — the decision to continue should belong to the caller, not the callee

**Why cognitive-mesh avoids it**: The `DurableWorkflowEngine` has explicit checkpoint and recovery mechanisms that require intentional resumption, not automatic continuation.

**Recommendation**: Use explicit retry policies with circuit breakers (as cognitive-mesh does with `AgentCircuitBreakerPolicy`). Never auto-continue without understanding why the stop occurred.

---

## 🟠 MEDIUM RISK — Context-Dependent Antipatterns

### 5. Inference-Time Scaling

**Category**: Orchestration & Control
**When problematic**: When used to compensate for poor prompts or wrong model selection

Scaling compute at inference time can mask fundamental issues:

- **Hides model inadequacy** — throwing more compute at a bad model delays fixing the real problem
- **Unpredictable costs** — scaling is often nonlinear; 2x compute rarely gives 2x quality
- **Latency unpredictability** — user-facing applications cannot tolerate variable response times

**Mitigation**: Only use for well-characterized workloads with cost caps and latency SLAs.

---

### 6. Tool Use Incentivization via Reward Shaping

**Category**: Feedback Loops
**When problematic**: When the reward signal is imperfect (it always is)

This is a direct application of Goodhart's Law: "When a measure becomes a target, it ceases to be a good measure."

- **Reward hacking** — agents learn to invoke tools for reward rather than for task completion
- **Overfitting to reward signal** — agent behavior optimizes for the metric, not the goal
- **Feedback loop instability** — small changes in reward function cause large behavioral shifts

**Mitigation**: Use reward shaping only with robust anti-hacking measures. Prefer direct outcome evaluation.

---

### 7. Inference-Healed Code Review Reward

**Category**: Feedback Loops
**When problematic**: When the healing model is imperfect

Creates a moral hazard:

- **Learned helplessness** — the model learns to produce broken code because fixes are "free"
- **Dependency on healer quality** — if the healer degrades, the entire pipeline degrades
- **False confidence** — healed code passes review but may have subtle bugs the healer introduced

**Mitigation**: Track healing frequency. If it increases over time, the pattern is degrading quality.

---

### 8. Progressive Tool Discovery

**Category**: Tool Use & Environment
**When problematic**: In production systems with security requirements

- **Expanded attack surface** — each discovered tool is a new potential exploit vector
- **Unpredictable capabilities** — you cannot audit what an agent might do if it discovers tools at runtime
- **Authorization challenges** — dynamic discovery conflicts with principle of least privilege

**Why cognitive-mesh avoids it**: `ToolDefinitions` is a static registry. Tools are architect-approved, not agent-discovered.

**Mitigation**: If needed, use allowlists for discoverable tools. Never allow unrestricted discovery.

---

### 9. Dynamic Code Injection (On-Demand File Fetch)

**Category**: Tool Use & Environment
**When problematic**: Always, unless heavily sandboxed

This is a remote code execution vulnerability by design:

- **Injection attacks** — fetched code can contain malicious payloads
- **Supply chain risk** — the remote source can be compromised
- **Non-deterministic behavior** — the same fetch can return different code at different times
- **Audit impossibility** — executed code may not match what was reviewed

**Why cognitive-mesh avoids it**: No dynamic code injection exists. All code paths are compile-time defined.

**Mitigation**: If absolutely necessary, use cryptographic verification, sandboxed execution, and immutable artifact references.

---

### 10. Swarm Migration Pattern

**Category**: Orchestration & Control
**When problematic**: When state consistency matters

- **Partial migration failures** — some agents migrate, others don't, creating split-brain
- **State synchronization** — maintaining consistent state across migrating agents is extremely hard
- **Observability loss** — during migration, agent state may be in neither old nor new topology

**Mitigation**: Use blue-green deployment patterns instead. Atomic topology switches are safer than gradual migration.

---

### 11. Context Window Anxiety Management

**Category**: Context & Memory
**When problematic**: When it treats symptoms instead of causes

"Anxiety management" implies the context window is already too full:

- **Architectural symptom** — if you need to manage anxiety about context, your context strategy is wrong
- **Complexity tax** — adding a management layer on top of a broken foundation adds complexity without fixing the root cause
- **False security** — the manager can fail silently, leaving you with the original problem plus a broken manager

**Mitigation**: Fix the context architecture (use RAG, summarization, or hierarchical memory) rather than managing the anxiety of having too much context.

---

### 12. Disposable Scaffolding Over Durable Features

**Category**: Orchestration & Control
**When problematic**: When "temporary" becomes permanent

- **Technical debt generator** — nothing is more permanent than a temporary solution
- **Maintenance burden** — scaffolding code still needs to be maintained while it exists
- **Testing gaps** — temporary code is rarely well-tested
- **Removal friction** — dependencies on scaffolding accumulate faster than the scaffolding can be removed

**Mitigation**: Set explicit expiration dates. Use feature flags with automatic cleanup. Track scaffolding age.

---

### 13. Agent Reinforcement Fine-Tuning (Agent RFT)

**Category**: Learning & Adaptation
**When problematic**: When training data quality is poor or reward signals are noisy

- **Behavior encoding** — fine-tuning on agent traces permanently encodes both good and bad behaviors
- **Reward signal quality** — if the reward is noisy, the model learns noise
- **Catastrophic forgetting** — fine-tuning on narrow agent traces can degrade general capabilities
- **Irreversibility** — unlike prompt changes, fine-tuning is expensive to undo

**Mitigation**: Curate training data aggressively. Use evaluation benchmarks before and after. Keep the base model accessible.

---

## 🟡 LOW RISK — Situational Antipatterns

### 14. Parallel Tool Call Learning

**Category**: Orchestration & Control
**When problematic**: When parallel calls interact or share state

- **Race conditions** — parallel calls to stateful tools can produce inconsistent results
- **Non-deterministic ordering** — debugging requires reproducing exact timing
- **Attribution difficulty** — when something fails in parallel, identifying the culprit is harder

**Mitigation**: Only parallelize truly independent, idempotent tool calls.

---

### 15. Filesystem-Based Agent State

**Category**: Context & Memory
**When problematic**: At scale or with concurrent agents

- **No atomicity** — filesystem operations are not transactional
- **No distribution** — files don't replicate across nodes
- **Locking issues** — concurrent file access requires explicit locking
- **No querying** — finding state requires scanning files

**Why cognitive-mesh chose differently**: Uses Redis (fast, distributed) + DuckDB (queryable, persistent) via `HybridMemoryStore`.

**Mitigation**: Acceptable for single-agent, single-machine development. Not for production.

---

### 16. Virtual Machine Operator Agent

**Category**: Tool Use & Environment
**When problematic**: When the blast radius of mistakes is large

- **Privilege escalation** — VM control is root-equivalent; a compromised agent has full system access
- **Irreversible actions** — deleting VMs, modifying networks, or changing configurations can be catastrophic
- **Lateral movement** — an agent controlling VMs can pivot to other systems on the network

**Mitigation**: Use narrowly scoped APIs instead of full VM access. Apply principle of least privilege.

---

### 17. Oracle and Worker Multi-Model Approach

**Category**: Orchestration & Control
**When problematic**: When the oracle becomes a bottleneck or single point of failure

- **Oracle SPOF** — if the oracle fails or hallucinates, all workers receive bad instructions
- **Cascading errors** — oracle mistakes propagate and amplify through workers
- **Bottleneck** — all planning flows through one model, limiting throughput

**Mitigation**: Add oracle redundancy or consensus. Workers should have the ability to flag nonsensical instructions.

---

### 18. Extended Coherence Work Sessions

**Category**: Reliability & Eval
**When problematic**: When sessions exceed the model's effective context window

- **Context rot** — accumulated context accumulates errors that compound
- **No natural reset** — without session boundaries, there's no opportunity to correct drift
- **Debugging difficulty** — reproducing a bug requires reproducing the entire session history

**Mitigation**: Use explicit checkpointing. Periodically summarize and refresh context.

---

### 19. Variance-Based RL Sample Selection

**Category**: Learning & Adaptation
**When problematic**: When variance is high due to noise rather than information

- **Noise amplification** — selecting high-variance samples can preferentially select noisy data
- **Exploration-exploitation imbalance** — may over-explore unstable regions of the solution space
- **Distribution shift** — selected samples may not represent the target distribution

**Mitigation**: Combine with confidence-based filtering. Use variance as one signal among many.

---

## Antipattern Taxonomy

### Category 1: Autonomy Without Oversight

**Patterns**: Continuous Autonomous Task Loop, Stop Hook Auto-Continue
**Root cause**: Removing human checkpoints from high-stakes processes
**Principle violated**: Defense in depth

In cognitive-mesh, this is addressed by the 4-level autonomy spectrum (SovereigntyFirst → FullyAutonomous) with `ActWithConfirmation` as the recommended default.

---

### Category 2: Self-Modification

**Patterns**: Self-Rewriting Meta-Prompt Loop, Agent RFT
**Root cause**: Systems that modify their own decision-making logic
**Principle violated**: Reproducibility, auditability

In cognitive-mesh, all templates are static and architect-defined. The system can learn insights (`LearningInsight`) but cannot modify its own reasoning structure.

---

### Category 3: Unbounded Resource Consumption

**Patterns**: No-Token-Limit Magic, Inference-Time Scaling (uncapped), Continuous loops
**Root cause**: Removing or ignoring resource boundaries
**Principle violated**: Resource accounting, cost predictability

In cognitive-mesh, authority scopes include `MaxResourceConsumption` and `MaxBudget` limits. The `AgentCircuitBreakerPolicy` provides automatic circuit-breaking with exponential backoff.

---

### Category 4: Reward/Incentive Hacking

**Patterns**: Tool Use Incentivization via Reward Shaping, Inference-Healed Code Review Reward
**Root cause**: Optimizing for proxy metrics instead of actual outcomes
**Principle violated**: Goodhart's Law — "When a measure becomes a target, it ceases to be a good measure"

Cognitive-mesh uses direct outcome evaluation through `SelfEvaluator` rather than reward-shaped incentives.

---

### Category 5: Security Surface Expansion

**Patterns**: Dynamic Code Injection, Progressive Tool Discovery, Virtual Machine Operator Agent
**Root cause**: Giving agents capabilities that expand their attack surface
**Principle violated**: Principle of least privilege

Cognitive-mesh uses a static tool registry, zero-trust security architecture (`SecurityPolicyEnforcementEngine`), and authority scopes that explicitly bound agent capabilities.

---

## Impact on cognitive-mesh

### Patterns Correctly Avoided

cognitive-mesh **intentionally avoids** the following high-risk patterns:

| Pattern | How cognitive-mesh avoids it |
|---------|------------------------------|
| No-Token-Limit Magic | `AuthorityScopeViewModel.MaxResourceConsumption` + `AgentCircuitBreakerPolicy` |
| Self-Rewriting Meta-Prompt | Static templates via `IContextTemplateResolver` |
| Continuous Autonomous Loop | Explicit invocation required; `ActWithConfirmation` default |
| Stop Hook Auto-Continue | `DurableWorkflowEngine` with intentional checkpoint/resume |
| Dynamic Code Injection | All code paths compile-time defined |
| Progressive Tool Discovery | Static `ToolDefinitions` registry |

### Patterns Worth Monitoring

These medium-risk patterns exist in partial form and need guardrails:

| Pattern | Current state | Recommended guardrail |
|---------|---------------|----------------------|
| Swarm execution | Collaborative swarm in orchestration engine | Add convergence timeout and max-iteration limit |
| Dual LLM usage | Multi-model factory exists | Add oracle-failure fallback |
| Extended sessions | Session context tracking | Add periodic context refresh/summarization |

---

## Recommendations

1. **Document intentional exclusions** — Add a section to CLAUDE.md explaining why certain patterns (Self-Rewriting Meta-Prompt, No-Token-Limit Magic, Dynamic Code Injection) are intentionally excluded.

2. **Add convergence bounds** — The collaborative swarm execution pattern should have explicit maximum iteration limits and convergence thresholds.

3. **Monitor partial implementations** — The 27 partially implemented patterns should each have explicit acceptance criteria for what "full" implementation means and whether full implementation is desirable.

4. **Consider catalog contributions** — cognitive-mesh's 12 extra patterns (cultural adaptation, ethical reasoning, compliance engines) are novel and could be contributed back to the awesome-agentic-patterns catalog.
