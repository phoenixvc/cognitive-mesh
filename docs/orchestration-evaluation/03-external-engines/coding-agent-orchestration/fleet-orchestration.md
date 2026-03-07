# Coding-Agent Fleet Orchestration

## What It Is

Coding-agent fleet orchestration is the pattern of running multiple autonomous AI coding agents in parallel across isolated git worktrees for automated code changes, PR generation, and large-scale codebase modifications. This is distinct from general workflow engines (which orchestrate arbitrary tasks) and agent runtimes (which coordinate LLM conversations).

## How It Differs from Other Categories

| Aspect | Workflow Engines | Agent Runtimes | Fleet Orchestration |
|--------|-----------------|----------------|---------------------|
| **Primary unit** | Task/activity | Agent conversation | Code change + PR |
| **Isolation** | Process/container | In-memory | Git worktree |
| **Output** | Task result | Agent response | Commit + PR |
| **Coordination** | DAG/state machine | Chat/graph | Task decomposition + merge |
| **Failure mode** | Retry task | Retry conversation | Discard worktree |
| **Scale dimension** | Tasks/second | Agents in conversation | Parallel worktrees |

## Architecture Pattern

```
┌────────────────────────────────────────────┐
│           Fleet Orchestrator              │
│  ┌──────────────────────────────────┐    │
│  │  Task Decomposer                 │    │
│  │  (split issue → sub-tasks)       │    │
│  └──────────────────────────────────┘    │
│  ┌─────────┐ ┌─────────┐ ┌─────────┐   │
│  │Worktree │ │Worktree │ │Worktree │   │
│  │ Agent 1 │ │ Agent 2 │ │ Agent 3 │   │
│  │(branch) │ │(branch) │ │(branch) │   │
│  └────┬────┘ └────┬────┘ └────┬────┘   │
│       ↓           ↓           ↓         │
│  ┌──────────────────────────────────┐    │
│  │  Merge Strategy / PR Automation  │    │
│  │  (conflict resolution, review)   │    │
│  └──────────────────────────────────┘    │
└────────────────────────────────────────────┘
```

### Key Components

1. **Task decomposer**: Splits a high-level issue/request into independent sub-tasks
2. **Worktree manager**: Creates isolated git worktrees per agent, manages lifecycle
3. **Agent workers**: Autonomous coding agents (Claude Code, SWE-agent, Devin, etc.) operating in isolation
4. **Merge orchestrator**: Handles PR creation, conflict detection/resolution, and review automation
5. **Quality gates**: Automated testing, linting, and validation per worktree

## Notable Implementations

| Implementation | Type | Key Feature |
|---------------|------|-------------|
| **Claude Code (worktree mode)** | CLI agent | `isolation: "worktree"` parameter for parallel agent runs |
| **SWE-agent** | Open-source | Autonomous coding agent with environment isolation |
| **OpenHands (OpenDevin)** | Open-source | Sandboxed agent execution with Docker containers |
| **Devin** | Commercial | Full autonomous coding agent with IDE and browser |
| **GitHub Copilot Workspace** | Commercial | Issue-to-PR automation with workspace isolation |
| **Cursor Agent / Windsurf** | IDE-integrated | In-editor agent with multi-file changes |
| **Codegen platforms** | Various | Parallel agent execution for large-scale refactoring |

## Per-Metric Scores

| Metric | Score | % | Confidence | Justification |
|--------|:-----:|:-:|:----------:|---------------|
| Latency | 3.0 | 60.0% | Medium | Worktree creation takes seconds. Agent execution takes minutes. Not designed for low-latency. |
| Scalability | 3.8 | 76.0% | Medium | Parallel worktrees scale horizontally (limited by disk/compute). Task decomposition enables parallelism. |
| Efficiency | 3.0 | 60.0% | Medium | Each worktree duplicates repo state. LLM API costs per agent. Disk I/O for large repos. |
| Fault Tolerance | 3.5 | 70.0% | Medium | Worktree isolation means failures are contained. Failed agents can be discarded without affecting others. No merge side effects until PR. |
| Throughput | 3.5 | 70.0% | Medium | Parallel agent execution. But task decomposition quality limits effective parallelism. |
| Maintainability | 2.8 | 56.0% | Low | Emerging pattern; no standard framework. Custom tooling required. Merge conflict resolution is complex. |
| Determinism | 3.0 | 60.0% | Medium | Git provides full change tracking. But LLM non-determinism means re-runs produce different code. |
| Integration Ease | 3.2 | 64.0% | Low | Git-native (any repo works). But no standard APIs or protocols for fleet management. |

### Weighted Totals

| Profile | Score | Percentage |
|---------|:-----:|:----------:|
| Interactive | 3.20 | 64.0% |
| Batch | 3.50 | 70.0% |
| Long-Running Durable | 2.90 | 58.0% |
| Event-Driven Serverless | 2.80 | 56.0% |
| Multi-Agent Reasoning | 3.40 | 68.0% |

## When to Use

| Use Case | Rank | Fit |
|----------|:----:|-----|
| Large-scale codebase refactoring | **1st** | Split into independent files/modules; parallel agents. |
| Multi-issue PR automation | **1st** | Each agent handles one issue independently. |
| Migration tasks (API version, framework) | **2nd** | Decompose by module; parallel execution. |
| Parallel feature development | 3rd | Works if features are independent; conflicts otherwise. |

## When NOT to Use

- Tightly coupled changes where files overlap (merge conflicts dominate)
- Real-time interactive coding (latency too high)
- Simple single-file changes (overhead not justified)
- Tasks requiring cross-file coordination within a single agent context

## Key Challenges

1. **Task decomposition quality**: Poorly decomposed tasks create merge conflicts
2. **Merge conflict resolution**: Overlapping changes across worktrees require sophisticated resolution
3. **Quality validation**: Each worktree needs independent build/test validation
4. **Cost management**: N agents × LLM API costs scales linearly
5. **Determinism**: Re-running the same fleet produces different results

## Future Direction

- Standardized fleet management APIs (task assignment, status, merge)
- Intelligent task decomposition using code dependency analysis
- Automated merge conflict resolution using LLM-assisted diffing
- Cost-aware scheduling (route simple tasks to cheaper models)
