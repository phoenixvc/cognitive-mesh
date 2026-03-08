# Agentic Pattern Coverage Analysis

> **Source catalog**: [awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns) (147 patterns across 8 categories)
> **Catalog version**: Main branch as of 2026-03-07 (commit f3628c5, 3.5k stars)
> **Evaluated codebase**: cognitive-mesh (216 C# files, 5-layer hexagonal architecture)
> **Date**: 2026-03-07
>
> **Related Documents**:
> - [Index](./agentic-patterns-index.md) — Overview and roadmap
> - [High Priority](./agentic-patterns-high-priority.md) — 30 critical patterns for production
> - [Medium Priority](./agentic-patterns-medium-priority.md) — 42 beneficial patterns
> - [Low Priority](./agentic-patterns-low-priority.md) — 35 nice-to-have or not aligned
> - [Antipatterns](./agentic-antipattern-analysis.md) — 19 patterns classified by risk

## Executive Summary

| Metric | Count | % of Catalog |
|--------|-------|-------------|
| Fully implemented | 31 | 21% |
| Partially implemented | 27 | 18% |
| Missing | 89 | 61% |
| Extra (cognitive-mesh only) | 12 | — |

**Catalog growth note**: The awesome-agentic-patterns catalog has grown from ~108 to 147 patterns since May 2025. Many patterns are marked "NEW" or "UPDATED" including new categories for cost control, safety guardrails, and workspace-native orchestration.

Cognitive-mesh is strongest in **Orchestration & Control** (13 full) and **UX & Collaboration** (5 full). The biggest gaps are in **Learning & Adaptation** (0 full) and **Security & Safety** (0 full).

---

## 1. Context & Memory

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Agent-Powered Codebase Q&A / Onboarding | ❌ Missing | No codebase-specific Q&A agent |
| Context Window Anxiety Management | ❌ Missing | No explicit token-budget management or context-window monitoring |
| Context-Minimization Pattern | 🟡 Partial | `HybridMemoryStore` handles selective retrieval but does not actively minimize context before injection |
| Curated Code Context Window | ❌ Missing | No code-specific context curation; tools are data-oriented |
| Curated File Context Window | ❌ Missing | No file-specific context selection prior to LLM calls |
| Dynamic Context Injection | ✅ Full | `IContextTemplateResolver` resolves templates with variables; `SessionContext` carries parameters into agent execution |
| Episodic Memory Retrieval & Injection | ✅ Full | `HybridMemoryStore.QuerySimilarAsync()` performs vector-similarity search over Redis/DuckDB for episodic recall |
| Filesystem-Based Agent State | ❌ Missing | State persisted via Redis + DuckDB, not filesystem |
| Layered Configuration Context | ✅ Full | `FeatureFlagManager` enables layered feature toggles; `SessionContext` supports hierarchical config |
| Memory Synthesis from Execution Logs | 🟡 Partial | `AuditLoggingAdapter` captures 30+ event types but no synthesis step extracts patterns from logs |
| Proactive Agent State Externalization | ✅ Full | `HybridMemoryStore` dual-writes to Redis (hot) + DuckDB (cold) with automatic fallback |
| Context Window Auto-Compaction | ❌ Missing | No automatic compaction; manual summarization only |
| Progressive Disclosure for Large Files | ❌ Missing | Files are read in full or by line range; no progressive disclosure |
| Prompt Caching via Exact Prefix Preservation | ❌ Missing | No prefix caching optimization |
| Self-Identity Accumulation | ❌ Missing | No agent identity persistence across sessions |
| Semantic Context Filtering Pattern | 🟡 Partial | Vector search filters semantically but no explicit semantic filtering layer |
| Working Memory via TodoWrite | ❌ Missing | No TodoWrite-style working memory (task tracking is session-bound) |

**Subtotal: 4 Full, 3 Partial, 10 Missing** (17 patterns)

---

## 2. Feedback Loops

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| AI-Assisted Code Review / Verification | ❌ Missing | No code-review agent or automated review pipeline |
| Background Agent with CI Feedback | ❌ Missing | No CI integration for agent feedback |
| Coding Agent CI Feedback Loop | ❌ Missing | No coding-agent loop |
| Dogfooding with Rapid Iteration | ❌ Missing | No self-improvement iteration mechanism |
| Graph of Thoughts (GoT) | 🟡 Partial | `KnowledgeGraphManager` provides graph storage but reasoning engines do not traverse it as a thought graph |
| Inference-Healed Code Review Reward | ❌ Missing | No reward-based code review system |
| Reflection Loop | ✅ Full | `SelfEvaluator` evaluates performance across components; `ContinuousLearningComponent` integrates feedback |
| Rich Feedback Loops > Perfect Prompts | ✅ Full | `LearningManager` aggregates feedback from multiple agentic frameworks (LangGraph, CrewAI, etc.) |
| Self-Critique Evaluator Loop | ✅ Full | `SelfEvaluator.EvaluatePerformance()` runs critique cycles with insight generation |
| Self-Discover: LLM Self-Composed Reasoning | 🟡 Partial | Multiple reasoning engines exist but structures are architect-defined, not self-composed |
| Spec-As-Test Feedback Loop | ❌ Missing | No specification-as-test pattern |
| Tool Use Incentivization via Reward Shaping | ❌ Missing | No reward signals for tool selection |
| Incident-to-Eval Synthesis | ❌ Missing | No incident-to-evaluation feedback loop (NEW pattern) |
| Iterative Prompt & Skill Refinement | 🟡 Partial | Skill definitions exist but no iterative refinement loop |

**Subtotal: 3 Full, 3 Partial, 8 Missing** (14 patterns)

---

## 3. Learning & Adaptation

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Agent Reinforcement Fine-Tuning (Agent RFT) | ❌ Missing | No RL training loop or fine-tuning pipeline |
| Compounding Engineering Pattern | 🟡 Partial | `LearningInsight` captures individual insights with confidence scores but no compounding/accumulation mechanism |
| Skill Library Evolution | ❌ Missing | `ToolDefinitions` is a static registry of 13 tools; no dynamic evolution or skill acquisition |
| Variance-Based RL Sample Selection | ❌ Missing | No RL components whatsoever |
| Frontier-Focused Development | ❌ Missing | No explicit frontier model tracking |
| Memory Reinforcement Learning (MemRL) | ❌ Missing | No memory-based RL |
| Shipping as Research | 🟡 Partial | PRDs document experimental features but no formal research-shipping loop |

**Subtotal: 0 Full, 2 Partial, 5 Missing** (7 patterns)

---

## 4. Orchestration & Control

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Action-Selector Pattern | ✅ Full | `ActionPlanner` decomposes tasks; `DecisionExecutor` selects and executes actions |
| Agent-Driven Research | ✅ Full | `ResearchAnalyst` processes research questions with multi-source analysis |
| Autonomous Workflow Agent Architecture | ✅ Full | `MultiAgentOrchestrationEngine` with 4 coordination patterns (parallel, hierarchical, competitive, swarm) |
| Conditional Parallel Tool Execution | ✅ Full | Parallel execution coordination pattern in `MultiAgentOrchestrationEngine` |
| Continuous Autonomous Task Loop | 🟡 Partial | Task execution pipeline exists but no continuous autonomous loop; requires explicit invocation |
| Discrete Phase Separation | ✅ Full | 5-layer hexagonal architecture enforces strict phase boundaries (Foundation → Reasoning → Metacognitive → Agency → Business) |
| Disposable Scaffolding Over Durable Features | ❌ Missing | No scaffolding-then-remove pattern |
| Distributed Execution with Cloud Workers | 🟡 Partial | Azure Blob/CosmosDB/OneLake integration exists but no distributed worker pool |
| Dual LLM Pattern | 🟡 Partial | `LLMClientFactory` supports multiple models but no explicit dual-LLM orchestration (planner vs. executor) |
| Explicit Posterior-Sampling Planner | ❌ Missing | No probabilistic planning or posterior sampling |
| Feature List as Immutable Contract | 🟡 Partial | `FeatureFlagManager` manages features but flags are mutable at runtime |
| Inference-Time Scaling | ❌ Missing | No compute scaling at inference time |
| Initializer-Maintainer Dual Agent | ❌ Missing | No dual-lifecycle agent architecture |
| Inversion of Control | ✅ Full | Hexagonal Ports & Adapters throughout; constructor injection everywhere |
| Iterative Multi-Agent Brainstorming | ✅ Full | `DebateReasoningEngine` implements ideology-collider brainstorming with cross-examination |
| Language Agent Tree Search (LATS) | ❌ Missing | No tree-search agent implementation |
| LLM Map-Reduce Pattern | ❌ Missing | No map-reduce decomposition for LLM tasks |
| Multi-Model Orchestration for Complex Edits | 🟡 Partial | Multi-model support via factory but not orchestrated for complex editing tasks |
| Opponent Processor / Multi-Agent Debate | ✅ Full | `DebateReasoningEngine` with argument generation, cross-examination, synthesis, and voting |
| Oracle and Worker Multi-Model Approach | 🟡 Partial | Agent roles and hierarchy exist but no formal oracle/worker separation |
| Parallel Tool Call Learning | ❌ Missing | No learning from parallel tool call outcomes |
| Plan-Then-Execute Pattern | ✅ Full | `ActionPlanner` (plan) + `DecisionExecutor` (execute) with clear phase separation |
| Progressive Autonomy with Model Evolution | ✅ Full | 4 autonomy levels (SovereigntyFirst → FullyAutonomous) with `AuthorityService` enforcement |
| Progressive Complexity Escalation | 🟡 Partial | Risk classification (EU AI Act) and authority scopes exist but no progressive escalation workflow |
| Self-Rewriting Meta-Prompt Loop | ❌ Missing | Templates are static; no self-modification (intentionally avoided — see Antipattern Analysis) |
| Specification-Driven Agent Development | 🟡 Partial | Authority scopes define agent boundaries but agents are not fully spec-driven |
| Stop Hook Auto-Continue Pattern | ❌ Missing | No stop-hook mechanism (intentionally avoided — see Antipattern Analysis) |
| Sub-Agent Spawning | ✅ Full | `MultiAgentOrchestrationEngine.AssembleTeamAsync()` dynamically spawns agent teams |
| Swarm Migration Pattern | 🟡 Partial | Collaborative swarm execution exists but no migration between swarm topologies |
| Three-Stage Perception Architecture | ❌ Missing | No perception pipeline |
| Tool Capability Compartmentalization | ✅ Full | 13 tool types in `ToolDefinitions` with distinct capabilities; `BaseTool` enforces boundaries |
| Tree-of-Thought Reasoning | 🟡 Partial | `SequentialReasoningEngine` provides step-by-step reasoning but no branching/backtracking |
| Agent Modes by Model Personality | ❌ Missing | Single model personality per agent |
| Budget-Aware Model Routing with Hard Cost Caps | 🟡 Partial | `MaxBudget` in authority scopes but no routing (NEW pattern) |
| Burn the Boats | ❌ Missing | No irreversible commitment pattern |
| Custom Sandboxed Background Agent | ❌ Missing | No sandboxed background execution |
| Factory over Assistant | ✅ Full | Factory pattern throughout (`LLMClientFactory`, `ServiceFactory`) |
| Hybrid LLM/Code Workflow Coordinator | 🟡 Partial | `DurableWorkflowEngine` coordinates but no hybrid LLM/code |
| Lane-Based Execution Queueing | ❌ Missing | No lane-based queuing |
| Planner-Worker Separation for Long-Running Agents | 🟡 Partial | `ActionPlanner` + `DecisionExecutor` but not optimized for long-running |
| Recursive Best-of-N Delegation | ❌ Missing | No best-of-N sampling |
| Subject Hygiene for Task Delegation | ❌ Missing | No explicit subject hygiene |
| Tool Selection Guide | ❌ Missing | Tools selected by agent; no guide |
| Workspace-Native Multi-Agent Orchestration | ❌ Missing | Not workspace-native (NEW pattern) |

**Subtotal: 14 Full, 14 Partial, 16 Missing** (44 patterns)

---

## 5. Reliability & Eval

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Anti-Reward-Hacking Grader Design | ❌ Missing | No reward system to protect |
| Asynchronous Coding Agent Pipeline | ❌ Missing | No async coding pipeline |
| CriticGPT-Style Code Review | ❌ Missing | No critic-based review |
| Extended Coherence Work Sessions | 🟡 Partial | `SessionContext` tracks sessions but no extended-coherence mechanisms (context refresh, checkpointing) |
| Lethal Trifecta Threat Model | ❌ Missing | Security exists but no formal trifecta threat model |
| Merged Code + Language Skill Model | ❌ Missing | Uses generic LLM clients; no code-specialized model |
| No-Token-Limit Magic | ❌ Missing | No token-limit management (intentionally avoided — see Antipattern Analysis) |
| RLAIF (RL from AI Feedback) | ❌ Missing | No RLAIF pipeline |
| Structured Output Specification | ✅ Full | JSON schema validation, typed request/response models throughout |
| Versioned Constitution Governance | ✅ Full | `AIGovernanceMonitor` with versioned policy evaluation and enforcement |
| Action Caching & Replay Pattern | ❌ Missing | No action caching |
| Adaptive Sandbox Fan-Out Controller | ❌ Missing | No sandbox fan-out |
| Canary Rollout and Automatic Rollback | ❌ Missing | No canary deployments (NEW pattern) |
| Failover-Aware Model Fallback | 🟡 Partial | Circuit breaker exists but no model fallback |
| LLM Observability | ✅ Full | `AuditLoggingAdapter`, `TransparencyManager`, OpenTelemetry integration |
| Reliability Problem Map Checklist | ❌ Missing | No formal reliability checklist (NEW pattern) |
| Schema Validation Retry with Cross-Step Learning | ❌ Missing | Schema validation exists but no cross-step learning |
| Workflow Evals with Mocked Tools | ❌ Missing | Unit tests use mocks but no workflow-level evals |

**Subtotal: 3 Full, 2 Partial, 13 Missing** (18 patterns)

---

## 6. Security & Safety

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Deterministic Security Scanning Build Loop | ❌ Missing | No build-integrated security scanning |
| Isolated VM per RL Rollout | ❌ Missing | No VM isolation or RL rollouts |
| PII Tokenization | 🟡 Partial | GDPR compliance adapter handles data subject rights but no tokenization/de-identification of PII in transit |
| External Credential Sync | ❌ Missing | No external credential synchronization |
| Hook-Based Safety Guard Rails | ✅ Full | Pre/post hooks in Claude Code integration; `guard-destructive-bash.sh` |
| Non-Custodial Spending Controls | ❌ Missing | No spending controls (NEW pattern) |
| Sandboxed Tool Authorization | 🟡 Partial | Authority scopes limit tools but no sandbox |
| Soulbound Identity Verification | ❌ Missing | No soulbound identity (NEW pattern) |
| Zero-Trust Agent Mesh | ✅ Full | `SecurityPolicyEnforcementEngine` with zero-trust architecture (NEW pattern) |

**Subtotal: 2 Full, 2 Partial, 5 Missing** (9 patterns)

---

## 7. Tool Use & Environment

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Agent SDK for Programmatic Control | ✅ Full | Full SDK via MCP/ACP protocols with typed handlers and validators |
| Agent-First Tooling and Logging | ✅ Full | `AuditLoggingAdapter` with 30+ event types, correlation IDs, circuit breaker |
| Agentic Search Over Vector Embeddings | ✅ Full | `VectorDatabaseManager` (Qdrant), `EnhancedRAGSystem` with embedding generation and reranking |
| CLI-First Skill Design | ❌ Missing | Web API-centric; no CLI tooling |
| CLI-Native Agent Orchestration | ❌ Missing | ASP.NET Core web API; no CLI orchestration |
| Code Mode MCP Tool Interface Improvement | 🟡 Partial | `MCPHandler` exists but limited to basic request/response; no code-mode enhancements |
| Code-Over-API Pattern | ❌ Missing | API-centric design throughout |
| Code-Then-Execute Pattern | ❌ Missing | No code-generation-then-execution pipeline |
| Dual-Use Tool Design | 🟡 Partial | Tools are agent-only; no human-facing tool interfaces |
| Dynamic Code Injection (On-Demand File Fetch) | ❌ Missing | No dynamic code injection (intentionally avoided — see Antipattern Analysis) |
| Egress Lockdown (No-Exfiltration Channel) | 🟡 Partial | Authority scopes limit actions but no explicit egress/network lockdown |
| LLM-Friendly API Design | ✅ Full | Structured models, JSON schemas, typed interfaces throughout |
| Multi-Platform Communication Aggregation | 🟡 Partial | SendGrid email only; no Slack/Teams/webhook aggregation |
| Patch Steering via Prompted Tool Selection | ❌ Missing | No prompt-based tool steering |
| Progressive Tool Discovery | ❌ Missing | Static tool registry; no runtime discovery |
| Shell Command Contextualization | ❌ Missing | No shell command execution |
| Subagent Compilation Checker | ❌ Missing | No compilation validation for subagent outputs |
| Tool Use Steering via Prompting | 🟡 Partial | Tool definitions include descriptions but no explicit steering prompts |
| Virtual Machine Operator Agent | ❌ Missing | No VM operations |
| Visual AI Multimodal Integration | ❌ Missing | Text-only; no image/video processing |
| AI Web Search Agent Loop | ❌ Missing | No web search agent |
| Intelligent Bash Tool Execution | ❌ Missing | No bash execution |
| Multi-Platform Webhook Triggers | ❌ Missing | No webhook triggers |

**Subtotal: 4 Full, 5 Partial, 14 Missing** (23 patterns)

---

## 8. UX & Collaboration

| Pattern | Status | Evidence / Gap |
|---------|--------|----------------|
| Abstracted Code Representation for Review | ❌ Missing | No code abstraction for review |
| Agent-Assisted Scaffolding | ❌ Missing | No scaffolding generation |
| Agent-Friendly Workflow Design | ✅ Full | Hexagonal architecture with clean port/adapter interfaces; all methods async with CancellationToken |
| AI-Accelerated Learning and Skill Development | ✅ Full | `LearningCatalyst` for skill gaps; `LearningManager` for framework-specific growth |
| Chain-of-Thought Monitoring & Interruption | 🟡 Partial | `TransparencyManager` logs reasoning traces but no runtime interruption capability |
| Democratization of Tooling via Agents | 🟡 Partial | Tools accessible through orchestration but no self-service tool creation |
| Human-in-the-Loop Approval Framework | ✅ Full | `AgentConsentService`, `OverrideRequest`, `AuthorityService` with 4 autonomy levels |
| Latent Demand Product Discovery | ❌ Missing | `CustomerIntelligenceManager` tracks preferences but no latent-demand discovery |
| Proactive Trigger Vocabulary | ❌ Missing | No proactive trigger vocabulary |
| Seamless Background-to-Foreground Handoff | 🟡 Partial | Session management exists but no explicit background-to-foreground transition |
| Spectrum of Control / Blended Initiative | ✅ Full | SovereigntyFirst → RecommendOnly → ActWithConfirmation → FullyAutonomous spectrum |
| Team-Shared Agent Configuration as Code | 🟡 Partial | `AgentRegistryService` manages agent configs but configs stored in DB, not as code |
| Verbose Reasoning Transparency | ✅ Full | `TransparencyManager` with JSON/Markdown report strategies, confidence scoring, step timestamps |
| Codebase Optimization for Agents | 🟡 Partial | Clean architecture but not agent-optimized |
| Dev Tooling Assumptions Reset | ❌ Missing | No assumptions reset |
| Milestone Escrow for Agent Resource Funding | ❌ Missing | No milestone escrow (NEW pattern) |

**Subtotal: 5 Full, 5 Partial, 6 Missing** (16 patterns)

---

## 9. Extra Patterns in cognitive-mesh

These patterns exist in cognitive-mesh but have no counterpart in the awesome-agentic-patterns catalog:

| Pattern | Key File(s) | Description |
|---------|-------------|-------------|
| **Cross-Cultural Adaptation (Hofstede 6-D)** | `CrossCulturalFrameworkEngine.cs` | Culture-aware UI/consent/authority adaptation using Hofstede's 6 cultural dimensions; 6 built-in locale profiles (US, DE, JP, BR, SE, CN) |
| **EU AI Act Compliance Engine** | `EUAIActComplianceAdapter.cs` | Full risk classification (Unacceptable/High/Limited/Minimal), conformity assessment, transparency obligations, EU database registration |
| **GDPR Compliance Engine** | `GDPRComplianceAdapter.cs` | Data subject rights, consent management, data minimization, right to be forgotten |
| **Ethical Reasoning (Brandom + Floridi)** | `EthicalReasoner.cs`, `INormativeAgencyPort.cs` | Normative agency validation (Brandom framework), informational dignity assessment (Floridi framework) |
| **Psychological Safety Metrics** | `PsychologicalSafetyMetricsEngine.cs` | Team dynamics monitoring, engagement metrics, collaboration pattern analysis |
| **Impact-First Measurement** | `ImpactFirstMeasurementEngine.cs` | Social/innovation/learning/workflow-depth impact scoring with bulk batch capability |
| **Organizational Blindness Detection** | `OrganizationalValueBlindnessEngine.cs` | Blind spot identification and risk assessment for overlooked organizational opportunities |
| **Champion Nudger Agent** | `ChampionNudgerAgent.cs` | Identifies high-impact contributors and provides behavioral nudges/recommendations |
| **Strategic Simulation Engine** | `StrategicSimulationEngine.cs` | Scenario modeling and outcome prediction for strategic decision-making |
| **Competitive Execution Pattern** | `MultiAgentOrchestrationEngine.cs` | Conflict resolution through agent competition; selects best result |
| **Collaborative Swarm Convergence** | `MultiAgentOrchestrationEngine.cs` | Iterative convergence through swarm collaboration |
| **Value Generation Engine** | `ValueGenerationEngine.cs` | User/team value assessment, profile-based scoring, employability prediction |

---

## References

1. **Primary source**: [nibzard/awesome-agentic-patterns](https://github.com/nibzard/awesome-agentic-patterns) — Apache 2.0 licensed catalog of 147 production-ready agentic AI patterns
2. **Origin article**: [What Sourcegraph learned building AI coding agents](https://sourcegraph.com/blog/what-sourcegraph-learned-building-ai-coding-agents) (May 2025)
3. **Related analysis**: [Antipattern Analysis](./agentic-antipattern-analysis.md) — 19 patterns classified by risk
4. **cognitive-mesh architecture**: See [CLAUDE.md](/CLAUDE.md) for hexagonal architecture and conventions

### Catalog Updates Tracked

The awesome-agentic-patterns catalog is actively maintained. Key additions since initial analysis:

| Pattern | Category | Notes |
|---------|----------|-------|
| Budget-Aware Model Routing | Orchestration | Hard cost caps (NEW) |
| Canary Rollout and Automatic Rollback | Reliability | Agent policy changes (NEW) |
| Incident-to-Eval Synthesis | Feedback Loops | Incident → evaluation (NEW) |
| Non-Custodial Spending Controls | Security | Spending limits (NEW) |
| Reliability Problem Map Checklist | Reliability | RAG/Agent checklist (NEW) |
| Soulbound Identity Verification | Security | Agent identity (NEW) |
| Workspace-Native Multi-Agent Orchestration | Orchestration | IDE-native (NEW) |
| Zero-Trust Agent Mesh | Security | Zero-trust architecture (NEW) |
| Milestone Escrow for Agent Resource Funding | UX | Resource escrow (NEW) |

---

## Methodology

1. **Source enumeration**: All 147 patterns from awesome-agentic-patterns catalog enumerated by category
2. **Codebase search**: Each pattern matched against cognitive-mesh via:
   - Glob patterns for relevant files (`*Engine.cs`, `*Adapter.cs`, `*Port.cs`)
   - Grep for pattern-specific keywords
   - Manual code review of critical files
3. **Status classification**:
   - ✅ **Full**: Pattern implemented with all core capabilities
   - 🟡 **Partial**: Pattern partially implemented or infrastructure present without full capability
   - ❌ **Missing**: Pattern not implemented (may be intentionally avoided)
4. **Extra patterns**: cognitive-mesh features not present in the catalog documented separately
