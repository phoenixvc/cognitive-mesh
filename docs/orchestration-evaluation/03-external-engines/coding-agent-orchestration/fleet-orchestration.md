# Coding-Agent Fleet Orchestration

## What It Is

Coding-agent fleet orchestration is the pattern of running multiple autonomous AI coding agents in parallel — each isolated in its own git worktree or sandbox — to produce code changes, PRs, and automated fixes across a codebase concurrently. The human's role shifts from implementer to supervisor/conductor. This is distinct from general workflow engines (which orchestrate arbitrary tasks) and agent runtimes (which coordinate LLM conversations).

## How It Differs from Other Categories

| Aspect | Workflow Engines | Agent Runtimes | Fleet Orchestration |
|--------|-----------------|----------------|---------------------|
| **Primary unit** | Task/activity | Agent conversation | Code change + PR |
| **Isolation** | Process/container | In-memory | Git worktree / Docker container |
| **Output** | Task result | Agent response | Commit + PR |
| **State management** | Event-sourced replay (Temporal) or HTTP (Inngest) | In-memory conversation | Git itself (branches, commits) |
| **Coordination** | DAG/state machine | Chat/graph | Task decomposition + merge |
| **Failure mode** | Retry task (deterministic replay) | Retry conversation | Discard worktree or re-inject CI failures |
| **Determinism** | Required (Temporal) or managed | LLM-dependent | Fundamentally non-deterministic |
| **Scale dimension** | Tasks/second | Agents in conversation | Parallel worktrees |
| **Merge/conflict** | No analogous concern | No analogous concern | Core challenge |

Temporal and Inngest solve *reliable execution of distributed workflows*. Fleet orchestration solves *parallel autonomous code generation with isolation and merge coordination*. They are complementary — you can run a coding-agent fleet on top of Temporal for durability, but the core problems (worktree isolation, conflict prevention, PR lifecycle, CI feedback loops) are domain-specific.

## Architecture Pattern

```text
┌────────────────────────────────────────────────────────┐
│                Fleet Orchestrator                     │
│  ┌─────────────────────────────────────────────┐     │
│  │  Task Decomposer                             │     │
│  │  (split issue → sub-tasks)                   │     │
│  │  Strategies: file-boundary, feature-slice,   │     │
│  │  layer-based, predictive conflict analysis   │     │
│  └─────────────────────────────────────────────┘     │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐  │
│  │Worktree │ │Worktree │ │Worktree │ │Worktree │  │
│  │ Agent 1 │ │ Agent 2 │ │ Agent 3 │ │ Agent N │  │
│  │(branch) │ │(branch) │ │(branch) │ │(branch) │  │
│  └────┬────┘ └────┬────┘ └────┬────┘ └────┬────┘  │
│       │  CI fail   │           │           │        │
│       │←──re-inject│           │           │        │
│       ↓           ↓           ↓           ↓        │
│  ┌─────────────────────────────────────────────┐    │
│  │  Merge Strategy / PR Automation             │    │
│  │  (sequential rebase, conflict detection,    │    │
│  │   review routing, quality gates)            │    │
│  └─────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────┐    │
│  │  Escalation: agent → orchestrator → human   │    │
│  └─────────────────────────────────────────────┘    │
└────────────────────────────────────────────────────────┘
```

### Key Components

1. **Task decomposer** (orchestrator capability): A logical capability of the fleet orchestrator that performs task decomposition, assignment, and routing decisions. Agent workers must execute only their assigned task slice and are prohibited from re-splitting or rerouting work; they may request re-decomposition only by escalating back through the orchestrator.
2. **Worktree manager**: Creates isolated git worktrees per agent, manages lifecycle and cleanup.
3. **Agent workers**: Autonomous coding agents (Claude Code, SWE-agent, Devin, etc.) operating in isolation. Workers execute their assigned scope only.
4. **CI feedback loop**: Failure logs re-injected into agent context for self-repair.
5. **Merge orchestrator**: Handles PR creation, conflict detection/resolution, sequential rebase, and review automation.
6. **Quality gates**: Automated testing, linting, and validation per worktree.
7. **Escalation chain**: Agent → orchestrator → human when automated resolution fails.

### Task Decomposition Strategies

| Strategy | Description | Best For |
|----------|-------------|----------|
| **File-boundary** | Assign tasks that touch disjoint file sets | Large-scale refactoring |
| **Feature-slice** | Each agent owns an entire feature (vertical) | Feature development |
| **Layer-based** | Frontend agent, backend agent, test agent | Layered architectures |
| **Predictive conflict analysis** | Analyze overlap before dispatch; warn on proximity | Reducing merge conflicts |

### Merge Strategies

1. **Sequential rebase**: Merge one branch, rebase others on updated main, repeat
2. **Conflict-aware scheduling**: Only parallelize tasks with disjoint file sets
3. **AI-assisted merge resolution**: Re-inject conflicts into agent sessions or use specialized merge tools
4. **Dedicated merge queue**: A coordinator agent (e.g., Gas Town's "Refinery") manages the merge queue

## Notable Implementations

| Implementation | Type | Key Feature | Scale |
|---------------|------|-------------|-------|
| **Claude Code (worktree mode)** | CLI agent | `isolation: "worktree"` for parallel subagents | 16 agents, 100K lines [^anthropic-c] |
| **ComposioHQ Agent Orchestrator** | Open-source | Agent-agnostic (Claude Code, Codex, Aider). State machine orchestration. 8 plugin slots. [^composio] | 30 concurrent agents, 40 worktrees |
| **OpenAI Symphony** | Open-source (Elixir) | OTP supervision trees for fault tolerance across hundreds of agents. ~258 lines. | Hundreds (BEAM VM) |
| **OpenHands (OpenDevin)** | Open-source | Event-sourced SDK. AgentDelegateAction for sub-task handoff. Docker isolation. | 72% SWE-Bench Verified |
| **SWE-agent** | Open-source (research) | Agent-Computer Interface. Minimal footprint. Princeton/Stanford. | Strong SWE-Bench performance |
| **Open SWE (LangChain)** | Open-source | Planner + Reviewer multi-agent. Cloud-native parallel execution. | Cloud-scale |
| **Devin** | Commercial | Compound AI: Planner + specialized models. Fleet mode for migrations. | 10-14x speedup over humans |
| **GitHub Copilot / Agent HQ** | Commercial | Mission Control for multi-agent. Run Claude, Codex, Copilot in parallel. | Multi-repo, multi-agent |
| **Cursor Agent** | IDE-integrated | 8 parallel agents with cloud VMs. | Cursor Bench (proprietary benchmark; not SWE-Bench) |
| **Windsurf** | IDE-integrated | Parallel multi-agent with git worktrees. Side-by-side Cascade panes. [^windsurf] | 78% SWE-Bench |
| **Google Antigravity** | Commercial | Multi-agent across editor, terminal, browser. Parallel mission dispatch. | Multi-agent |
| **Superset IDE** | Open-source | Terminal for 10+ parallel agents on a single machine using worktrees. | 10+ local agents |
| **Gas Town** | Open-source | Mayor (coordinator) + Polecats (workers) + Refinery (merge queue). "Molecules" = crash-resilient task chains. | Multi-agent |

### Ecosystem Tools

| Tool | Purpose |
|------|---------|
| **Claude Squad** | tmux multiplexer for parallel Claude Code sessions |
| **viwo-cli** | Docker + worktrees for isolated agent environments |
| **crystal** | Desktop GUI for fleet management |
| **Parallel Code** | Parallel Claude Code execution framework |

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.0 | 60.0% | Medium | Worktree creation takes seconds. Agent execution takes minutes. Not designed for low-latency. Merge sequencing and CI feedback loops add further delays. |
| Scalability | 3.8 | 76.0% | Medium | Parallel worktrees scale horizontally. Cloud runtimes (Docker/K8s) enable hundreds of agents. Bottlenecked by LLM API rate limits and human review capacity. Practical ceiling ~10-30 agents per machine locally. |
| Efficiency | 2.5 | 50.0% | Medium | Each worktree duplicates repo state. N agents = N× API cost. High PR rejection rates observed (LinearB data [^linearb]: 67.3% for AI-generated vs 15.6% manual). Wasted work from conflicts and rejections. |
| Fault Tolerance | 3.5 | 70.0% | Medium | Git provides excellent state durability (commits persist progress). Worktree isolation contains failures. Session persistence (tmux, OTP trees) handles crashes. CI re-injection enables self-repair. But LLM non-determinism means retries may produce different (possibly worse) results. |
| Throughput | 4.2 | 84.0% | Medium | Primary value proposition. 10-30 parallel agents producing PRs simultaneously. Demonstrated 10-14x speedup in migration scenarios (Devin). ComposioHQ: 40K lines in 8 days with 30 agents. |
| Maintainability | 2.8 | 56.0% | Low | Rapidly evolving ecosystem (most projects < 1 year old). No standard framework. Custom tooling required. Merge conflict resolution is complex. Orchestration configuration adds operational burden. |
| Determinism | 1.8 | 36.0% | High | **Fundamentally non-deterministic.** Same task given to same agent produces different code. No replay guarantees. Results vary by model, temperature, context window state. This is the weakest dimension by design — LLM outputs are inherently stochastic. |
| Integration Ease | 3.4 | 68.0% | Medium | Git-based workflow integrates naturally with existing CI/CD and PR review processes. Agent-agnostic orchestrators (Composio) support multiple backends. Requires worktree-aware tooling and often custom merge strategies. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.06 | 61.2% |
| Batch | 3.52 | 70.4% |
| Long-Running Durable | 2.72 | 54.4% |
| Event-Driven Serverless | 2.72 | 54.4% |
| Multi-Agent Reasoning | 2.96 | 59.2% |

## When to Use

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Large-scale codebase refactoring | **1st** | Split into independent files/modules; parallel agents. Proven in migration scenarios. |
| Multi-issue PR automation | **1st** | Each agent handles one issue independently. Natural parallelism. |
| Migration tasks (API version, framework) | **2nd** | Decompose by module; 10-14x speedup demonstrated. |
| Test generation at scale | **2nd** | Each agent generates tests for different modules independently. |
| Parallel feature development | 3rd | Works if features are independent; merge conflicts otherwise. |

## When NOT to Use

- Tightly coupled changes where files overlap (merge conflicts dominate)
- Real-time interactive coding (latency too high; minutes per agent)
- Simple single-file changes (overhead not justified)
- Tasks requiring cross-file coordination within a single agent context
- Teams without CI/CD infrastructure for automated validation
- Budget-constrained environments (N agents = N× API costs)

## Key Challenges

1. **Task decomposition quality**: Poorly decomposed tasks create merge conflicts. Predictive conflict analysis before dispatch is critical.
2. **Merge conflict resolution**: Overlapping changes across worktrees require sophisticated resolution. Sequential rebase is the most common strategy.
3. **Quality validation**: Each worktree needs independent build/test. CI pipeline must support parallel branch testing.
4. **Cost management**: N agents × LLM API costs scales linearly. Anthropic's C compiler demo: $20K for 16 agents.
5. **Determinism**: Re-running the same fleet produces different results. No replay guarantees.
6. **Review bottleneck**: Google DORA 2025 [^dora]: 91% increase in code review time, 154% increase in PR size with AI adoption. Parallel generation without parallel review creates queues.
7. **Rejection rates**: LinearB data [^linearb] shows 67.3% rejection rate for AI-generated PRs vs 15.6% for manual code.

## Fault Tolerance Patterns

| Pattern | Description | Used By |
|---------|-------------|---------|
| **Session persistence** | Survive terminal crashes, SSH disconnects, reboots | Composio, tmux tools |
| **CI failure re-injection** | Pipe failure logs back into agent context for self-repair | Most implementations |
| **OTP supervision trees** | Process supervision for concurrent agents | Symphony (Elixir/BEAM) |
| **Git as durable state** | Commits persist progress; agents resume from last commit | All git-based approaches |
| **Idempotent retries** | Standard distributed systems primitives for agent orchestration | Composio, Symphony |
| **Escalation chains** | Agent → orchestrator → human on unresolvable issues | Gas Town, Devin |

## Future Direction

- Standardized fleet management APIs (task assignment, status, merge)
- Intelligent task decomposition using code dependency analysis (AST-aware)
- Automated merge conflict resolution using LLM-assisted diffing
- Cost-aware scheduling (route simple tasks to cheaper models)
- Parallel review tooling (AI-assisted review to match generation throughput)
- Proof-of-work requirements (Symphony model: CI pass + tests + review + walkthrough before merge)

## Citation References

[^anthropic-c]: Anthropic C compiler demo — 16 parallel Claude Code agents producing 100K lines of code.
[^composio]: ComposioHQ Agent Orchestrator — open-source agent-agnostic orchestrator supporting 30 concurrent agents.
[^windsurf]: Windsurf SWE-Bench results — 78% on SWE-Bench Verified.
[^linearb]: LinearB "State of AI in Code" report — AI-generated PR rejection rate 67.3% vs 15.6% manual.
[^dora]: Google DORA 2025 report — 91% increase in code review time, 154% increase in PR size with AI adoption.
