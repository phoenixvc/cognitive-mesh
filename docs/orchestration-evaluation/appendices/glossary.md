# Glossary

Terms used throughout the orchestration evaluation documentation.

## Orchestration Concepts

**Activity** — A single unit of work in a workflow engine (Temporal/Cadence term). Equivalent to a "step" or "task" in other systems.

**Backpressure** — A flow-control mechanism where downstream components signal upstream to slow down when they're overwhelmed. Prevents resource exhaustion under load.

**Circuit Breaker** — A pattern that detects repeated failures and temporarily stops sending requests to a failing service, allowing it to recover. States: Closed (normal) → Open (blocking) → Half-Open (testing).

**Convergence** — In multi-agent systems, the point at which iterating agents reach a stable consensus or outcome. It can be assessed by heuristics (e.g., string matching), by meeting a scoring threshold, or by reaching a terminal state in a state machine.

**Coordination Pattern** — The strategy for how multiple agents or tasks interact. Common patterns: parallel, hierarchical, competitive, collaborative swarm.

**Correlation ID** — A unique identifier that links related events/logs across service boundaries, enabling end-to-end tracing.

**Durable Execution** — Execution model where workflow state is persisted after each step, allowing automatic recovery from crashes by replaying from the last checkpoint.

**Fan-Out / Fan-In** — Pattern where a single task spawns multiple parallel sub-tasks (fan-out) and then collects their results (fan-in).

**Idempotency** — The property that executing an operation multiple times produces the same result as executing it once. Critical for safe retries.

## Architecture Patterns

**Hexagonal Architecture** — (Ports and Adapters) An architectural pattern where business logic is at the center, connected to the outside world through ports (interfaces) and adapters (implementations). Used by cognitive-mesh.

**Event Sourcing** — Storing state as a sequence of events rather than current state snapshots. Enables replay and audit trails.

**Saga Pattern** — A sequence of local transactions where each step has a compensating action. If a step fails, compensating actions undo previous steps.

**State Machine** — A model of behavior with a finite number of states, transitions between states, and actions. Used by agentkit-forge (5-phase lifecycle).

**Step Function** — A serverless workflow primitive where each step's result is durably stored before the next step executes. Used by Inngest, AWS Step Functions.

## Metrics Terms

**p50 / p95 / p99** — Percentile latency measurements. p95 = 95% of requests complete within this time. p95 is the standard benchmark target.

**Throughput** — Number of tasks/operations completed per unit time (typically tasks/second).

**Wall-Clock Time** — Real elapsed time from start to finish, as opposed to CPU time.

**Saturation Point** — The workload level at which adding more work causes performance degradation rather than proportional throughput increase.

## Agent Orchestration Terms

**A2A (Agent-to-Agent)** — Protocol for inter-agent communication and task delegation. agentkit-forge implements an "A2A-lite" variant using JSON task files.

**Agent Runtime** — The execution environment that hosts and coordinates agents. Examples: AutoGen GroupChat, Semantic Kernel, LangGraph.

**Autonomy Level** — In cognitive-mesh: the degree of independence granted to an agent. Levels: RecommendOnly, ActWithConfirmation, FullyAutonomous.

**Authority Scope** — In cognitive-mesh: the boundaries within which an agent can operate — allowed endpoints, resource budgets, data policies.

**Fleet Orchestration** — Running multiple autonomous coding agents in parallel across isolated worktrees for automated code changes and PR generation.

**Governance Gate** — A checkpoint in the orchestration pipeline that enforces compliance rules (ethics, approval, authority) before allowing an action to proceed.

**GroupChat** — In AutoGen: a coordination mode where multiple agents converse in a shared chat, managed by a GroupChatManager with configurable max rounds.

**Human-in-the-Loop (HITL)** — An orchestration pattern where certain decisions require human approval before the workflow can proceed.

**Swarm** — A collaborative pattern where multiple agents iteratively work toward convergence on a shared task. cognitive-mesh implements this with `maxIterations=5`.

## Infrastructure Terms

**Checkpointing** — Periodically saving workflow state to durable storage, enabling resume-from-checkpoint after failures.

**Entry Point** — In Python packaging: a mechanism for registering plugins that can be discovered at runtime. Used by codeflow-engine.

**Lock TTL (Time-To-Live)** — The maximum duration a coordination lock is held before being considered stale and eligible for cleanup.

**Worker** — In workflow engines (Temporal/Cadence): a process that polls for and executes tasks. Workers can scale horizontally.

**Worktree** — In git: an additional working directory linked to the same repository. Used in fleet orchestration to isolate parallel coding agents.
