# PhoenixVC Stack vs. Coding-Agent Harnesses (Claw Code, oh-my-openagent, and friends)

> Companion comparison to [`custom-vs-established.md`](custom-vs-established.md). This doc answers the
> narrower question: **what does the combined PhoenixVC stack give you that a popular coding-agent
> harness doesn't, and vice versa?** Earlier revisions framed this as a four- or six-layer comparison
> of three repos. The PhoenixVC org has thirteen — see [The seven layers](#the-seven-layers).

The PhoenixVC stack and the coding-agent-harness ecosystem are often discussed as if they competed.
They don't. They sit at different layers of the stack and solve different problems. This doc lines
them up so the overlap and the gaps are obvious.

## TL;DR

| Question | Answer |
|----------|--------|
| Are these substitutes? | **No.** Cognitive Mesh runs production cognition; harnesses orchestrate the developer's editor. |
| The single most useful axis? | **Build-time vs runtime.** Retort, retort-plugins, codeflow-plugins, and mcp-org are build-time. Sluice, docket, codeflow-engine, phoenix-flow, cognitive-mesh, and deck are runtime. The harnesses (Claude Code, Claw Code, oh-my-openagent) live entirely at runtime. |
| Where does the overlap actually live? | **Multi-agent orchestration semantics, task delegation, quality gates, worktree isolation.** |
| Should we pick one over the other? | **Combine.** Retort governs; sluice carries traffic; cognitive-mesh runs durable cognition; deck is the operator's pane of glass. |
| What's the biggest mistake? | Trying to use Cognitive Mesh's `MultiAgentOrchestrationEngine` as an *editor* harness, or using Claw Code as a *production* agent runtime. They are not the same workload. |

## The seven layers

Earlier revisions of this doc described five (and then six) layers covering three PhoenixVC repos.
The actual PhoenixVC ecosystem is **thirteen repos across seven layers**, plus a horizontal axis
(build-time vs runtime) that cuts across all of them. This is the diagram that matches the codebase:

```
┌────────────────────────────────────────────────────────────────┐
│  L6 — Operator control plane     (desktop ops, tray, dashboards)│
│       ▸ deck — Tauri 2.0 + React/TS + Rust + .NET sidecar       │
│         "VS Code-style shell" — surfaces sluice, docket,        │
│         cognitive-mesh, phoenix-flow, retort, mcp-org           │
├────────────────────────────────────────────────────────────────┤
│  L5 — End-user UI                (dashboards, widgets)          │
│       ▸ Cognitive Mesh UILayer (Next.js + Tailwind)             │
│       ▸ Widget plugins via IWidgetRegistry (PluginAPI)          │
│       ▸ phoenix-flow Kanban UI for human+agent task graph       │
├────────────────────────────────────────────────────────────────┤
│  L4 — Runtime cognition          (production traffic)           │
│       ▸ Cognitive Mesh AgencyLayer + Reasoning + Foundation     │
│       ▸ MultiAgentOrchestrationEngine, DurableWorkflowEngine    │
│       ▸ Ethical reasoning, MAKER benchmark, checkpointing       │
├────────────────────────────────────────────────────────────────┤
│  L3 — DevOps & task automation   (CI / PR / shared task graph)  │
│       ▸ codeflow-engine (Python) — AutoGen, GitHub/Linear/Slack │
│       ▸ phoenix-flow — bidirectional YAML ↔ Kanban sync, MCP    │
│         server for human+agent shared task graph                │
├────────────────────────────────────────────────────────────────┤
│  L2 — Coding-agent harness       (the IDE / CLI agent itself)   │
│       ▸ Claw Code, Claude Code, oh-my-openagent, oh-my-pi       │
│       ▸ Subagents, hooks, skills, MCP, worktree isolation       │
│  L2.5 — IDE bridges              (extending L1/L3 into editors) │
│       ▸ retort-plugins: VS Code (TS), JetBrains (Kotlin), Zed   │
│       ▸ codeflow-plugins: VS Code AutoPR + multi-agent assist   │
├────────────────────────────────────────────────────────────────┤
│  L1 — Spec & governance          (config sync, team rules)      │
│       ▸ retort (`.agentkit/spec/*.yaml`) — CLI + Ink TUI        │
│         9+ harness outputs from one source, drift checks,       │
│         5-phase lifecycle, hookify runtime guards               │
│       ▸ mcp-org — org-level MCP server, cross-repo roadmap      │
├────────────────────────────────────────────────────────────────┤
│  L0 — AI data plane              (LLM traffic, cost, telemetry) │
│       ▸ sluice — OpenAI-compatible gateway on Azure Container   │
│         Apps; LiteLLM proxy, semantic cache, rate limit, auth   │
│       ▸ docket — LLM spend tracking, cost analytics             │
└────────────────────────────────────────────────────────────────┘
```

Crossing horizontally:

```
        BUILD-TIME                       RUNTIME
┌──────────────────────────┐  ┌──────────────────────────────┐
│ retort                   │  │ sluice  (L0)                 │
│ retort-plugins           │  │ docket  (L0)                 │
│ codeflow-plugins         │  │ codeflow-engine  (L3)        │
│ mcp-org                  │  │ phoenix-flow  (L3 + L5)      │
│ ── all of L1 & L2.5 ──   │  │ cognitive-mesh  (L4 + L5)    │
│                          │  │ deck  (L6)                   │
│                          │  │ ── L2 harnesses ──           │
│                          │  │ Claw Code, Claude Code, OMO  │
└──────────────────────────┘  └──────────────────────────────┘
```

The PhoenixVC stack covers **L0 + L1 + L2.5 + L3 + L4 + L5 + L6** — every layer except the
coding-agent harness itself (L2 proper). Retort and retort-plugins exist precisely so any L2
harness can plug in cleanly. The PhoenixVC stack brackets the L2 harness ecosystem on every
side: data plane (sluice/docket) and governance (retort/mcp-org) below; DevOps automation,
runtime cognition, end-user UI, and operator control plane above.

> **Credit where due:** the build-time vs runtime axis is the central reframe and comes from a
> peer analysis. Earlier versions of this doc placed retort as L1 alongside the runtime systems,
> which obscured the most important property — **retort has no runtime; it's a compiler.**
> Treating it as a peer of cognitive-mesh on the same axis was a category error. The same peer
> review prompted several factual corrections in the contender table below (retort's actual
> harness count, oh-my-openagent's six archetypes, claw-code's parity-harness story).

## The contenders

### PhoenixVC stack — full ecosystem (13 repos)

| Repo | Layer | Build/Run | Language | Differentiator |
|------|-------|-----------|----------|----------------|
| [`cognitive-mesh`](https://github.com/phoenixvc/cognitive-mesh) | L4 + L5 | Runtime | .NET 9 / C# (backend) + Next.js / TS (UILayer) | **Six-layer** hexagonal architecture (Foundation → Reasoning → Metacognitive → Agency → Business → UI). 9 code teams + 5 workflow agents partitioned by layer. `DurableWorkflowEngine` checkpointing, `HybridMemoryStore` (Redis + DuckDB), MAKER benchmark, ethical reasoning (Brandom + Floridi), `UILayer/PluginAPI/IWidgetRegistry` widget contract. |
| [`retort`](https://github.com/phoenixvc/retort) | L1 | **Build-time** | Node.js / TypeScript (CLI + Ink + React TUI) | **Spec-driven config compiler** for 9+ harnesses (Claude, Cursor, Copilot, Windsurf, Codex, Gemini, Cline, Roo, Warp). 13 team commands. CLI + interactive TUI. 5-phase lifecycle with `/check` gates, hookify runtime guards (file-event + bash-event regex), drift detection, file-based task delegation in `.claude/state/tasks/*.json`. **No runtime — it's a compiler.** |
| [`retort-plugins`](https://github.com/phoenixvc/retort-plugins) | L2.5 | Build-time | TS 65% (VS Code) + Kotlin 18% (JetBrains) + Rust 5% (Zed) | Three IDE extensions sharing one Retort backend; activates on `.agentkit/` presence; `@retort` Copilot Chat participant; Junie context injection; Zed slash commands. |
| [`codeflow-engine`](https://github.com/phoenixvc/codeflow-engine) | L3 | Runtime | Python 84% | AutoGen-backed multi-agent path, GitHub/Linear/Slack/Axolo integrations, event fan-out, exponential-backoff retries. |
| [`codeflow-plugins`](https://github.com/phoenixvc/codeflow-plugins) | L2.5 | Build-time | TS / IDE-extension stack | VS Code extension surfacing AutoPR and multi-agent coding assistance from codeflow-engine — the L2.5 bridge for L3 (parallel to retort-plugins for L1). |
| [`sluice`](https://github.com/phoenixvc/sluice) | **L0** | Runtime | HCL 34% + Python 23% + JS 14% + Shell + PowerShell | **OpenAI-compatible AI gateway.** LiteLLM proxy on Azure Container Apps; FastAPI + Redis state service; Terraform-managed; centralized auth, rate limiting, semantic caching, telemetry. Receives traffic from cognitive-mesh, mystira-workspace, phoenix-flow, codeflow-engine; retort recommends sluice as the standard gateway. |
| [`docket`](https://github.com/phoenixvc/docket) | **L0** | Runtime | — | **LLM spend tracking & cost analytics.** Consumes telemetry from sluice; surfaces cost attribution and optimisation. Cost-control sibling to sluice. |
| [`phoenix-flow`](https://github.com/phoenixvc/phoenix-flow) | L3 + L5 | Runtime | TypeScript / React + MCP server | **Human + agent shared task graph.** React Kanban UI, MCP server, bidirectional YAML ↔ board sync. Closes the loop where retort's file-based task delegation needs human visibility. |
| [`mcp-org`](https://github.com/phoenixvc/mcp-org) | L1 | Mixed | — | Org-level MCP server for cross-repo tasks and roadmap management. Provides organizational context to any MCP-compatible harness. |
| [`deck`](https://github.com/phoenixvc/deck) | **L6** | Runtime | TS 73% (React 18 + Vite + Tailwind) + Rust 18% (Tauri 2.0) + C# 5% (.NET 9 sidecar) | **Operator control plane.** Tauri desktop app with VS Code-style shell. Service management, infrastructure deploy (Bicep), dashboards, test execution. Integrates retort → cognitive-mesh → sluice → phoenix-flow → docket → mcp-org. The single pane of glass for L0–L5. |
| [`mystira-workspace`](https://github.com/phoenixvc/mystira-workspace) | (consumer) | Runtime | .NET + TS + Rust monorepo | Out-of-scope for this comparison — it's a *consumer* of the stack ("AI-powered interactive storytelling for children"), not a platform component. Listed for completeness. |
| [`phoenix-website`](https://github.com/phoenixvc/phoenix-website) | (org site) | n/a | — | Out-of-scope (public web presence). |
| [`.github`](https://github.com/phoenixvc/.github) | (org meta) | n/a | — | Out-of-scope (org metadata). |

### Coding-agent harnesses (L2 proper)

| Harness | Origin | Language | Notable architecture |
|---------|--------|----------|----------------------|
| **Claw Code** ([`instructkr/claw-code`](https://github.com/instructkr/claw-code)) | Clean-room rewrite of Claude Code after the npm source-map leak (March 2026) | **Rust 72% + Python** (peer analysis correction; my earlier 95.9% was the canonical `/rust` workspace only) | **Rebuilds harness plumbing** (CLI binary, tool dispatch, context window) rather than agent choreography. CLI subcommands (`prompt`, `login`, `doctor`), session management, OAuth/API-key auth, **parity-harness against reference Claude Code behavior** (`claw doctor`). |
| **Claude Code** | Anthropic | TypeScript / Node | Subagents, hooks (PreToolUse/PostToolUse), skills with progressive disclosure, MCP, worktree isolation, plain-text `CLAUDE.md` memory. |
| **oh-my-openagent / OMO** ([`code-yeongyu/oh-my-openagent`](https://github.com/code-yeongyu/oh-my-openagent)) | Community | TypeScript + Bun | **Prescriptive role hierarchy:** Prometheus / Metis / Momus → Atlas → Sisyphus-Junior. **Six named archetypes with cognitive specialization** (planner, gap-analyzer, reviewer, conductor, executor, specialists). `boulder.json` (active plan + session history) + `.sisyphus/notepads/{plan}/` (wisdom accumulation across sessions). Hash-anchored edits (`LINE#ID`), LSP + ast-grep across 25 languages, multi-provider routing by **semantic categories** (visual-engineering, ultrabrain, artistry, quick, deep, writing) — decoupling intent from model choice. |
| **oh-my-agent** ([`first-fluke/oh-my-agent`](https://github.com/first-fluke/oh-my-agent)) | Community | `.agents/`-based | 15 specialized agents, 5-phase workflow with 11 review gates, portable across Antigravity / Claude Code / Cursor / Gemini CLI / Codex CLI / OpenCode. |
| **oh-my-pi** ([`can1357/oh-my-pi`](https://github.com/can1357/oh-my-pi)) | Community | Mixed | Parallel execution framework, 6 bundled agents, isolation backends. |
| **oh-my-codex (OMX)** | Community | — | Hooks + agent teams + HUD on top of Codex CLI. |
| **Antigravity / Cursor / Windsurf / Gemini CLI / Codex CLI / Junie / Cline / Roo Code** | Various vendors | Various | Each has its own native config dialect — Retort generates all of them from one spec. |

## Two partition philosophies (the insight that reframes everything)

A peer review surfaced a distinction that the rest of this doc had glossed over. The systems
above don't just differ on layer — they differ on **how they cut the agent problem into named
agents in the first place.**

| Partition by… | Examples | What "an agent" means |
|---------------|----------|------------------------|
| **Domain** (what the code touches) | retort (`/team-frontend`, `/team-backend`, `/team-data`, `/team-infra`, …), cognitive-mesh (Foundation/Reasoning/Metacognitive/Agency/Business teams), oh-my-agent (frontend/backend/db/mobile/qa) | "A frontend specialist" — knows React, Tailwind, accessibility |
| **Cognitive function** (what the agent does mentally) | oh-my-openagent (Prometheus plans, Metis analyzes gaps, Momus reviews, Atlas conducts, Sisyphus-Junior executes) | "A planner" — never executes; "A reviewer" — never plans |

This is **not a stylistic difference**, it's a different theory of why multi-agent systems
work. Domain partitioning is "give each specialist their lane and route work by topic."
Cognitive-function partitioning is "no single agent should both plan and execute, because the
plan-and-execute-in-one-breath failure mode is the dominant source of bad agent behavior."

retort enforces the planner/executor split *informally* via convention (`/orchestrate` →
`/plan` → `/team-*`). oh-my-openagent enforces it *prescriptively* — Atlas literally has no
execute verb, Sisyphus-Junior literally has no plan verb. Both approaches work, but the
prescriptive split is harder to violate.

**For the PhoenixVC stack the practical implication is:** retort's team commands are domain-
partitioned, but the underlying lifecycle (`/orchestrate` → `/plan` → `/team-*` → `/check`)
already implements a soft cognitive-function partition. Making that split *hard* (so a team
agent literally cannot call planning verbs and vice versa) would adopt one of OMO's most
defensible design choices without abandoning retort's domain teams.

## Five capabilities the PhoenixVC stack should consider stealing

The peer analysis listed five concrete cross-pollination opportunities. I'm reproducing them
here because they're the most actionable part of the comparison.

1. **Wisdom accumulation / notepads** (from oh-my-openagent). OMO's `.sisyphus/notepads/{plan}/`
   automatically extracts learnings after each task and feeds them to subsequent subagents —
   prompt-level continuous learning. retort's `docs/history/` and `docs/handoffs/` are
   human-authored after the fact; they don't propagate forward automatically. The closest
   PhoenixVC analogue is cognitive-mesh's `MetacognitiveLayer/ContinuousLearning` — but at
   runtime, not at the prompt level. **Action:** add `.claude/state/wisdom/{plan}/` to retort's
   spec; have hookify post-task hooks extract learnings into it; have `/orchestrate` and `/plan`
   inject the relevant wisdom file at session start.
2. **Semantic categories for model routing** (from oh-my-openagent). OMO's category system
   (`visual-engineering`, `ultrabrain`, `artistry`, `quick`, `deep`, `writing`) decouples intent
   from model choice — `quick` can route to Haiku, `ultrabrain` to Opus, without hardcoding
   model IDs in team specs. retort currently has team commands but no model-routing layer.
   **Action:** add `category:` field to team specs; let the harness pick the model. This pairs
   beautifully with sluice (L0), which is already an OpenAI-compatible router — a category
   could become a sluice routing rule.
3. **Hard plan/execute separation** (from oh-my-openagent). See [partition philosophies](#two-partition-philosophies-the-insight-that-reframes-everything)
   above. **Action:** have retort's `/orchestrate` lifecycle prevent team agents from invoking
   planning verbs, and prevent the planner from invoking execution verbs.
4. **Durable-workflow semantics for retort task files** (from cognitive-mesh). retort's
   `.claude/state/tasks/*.json` lifecycle is a state machine, but it lacks `DurableWorkflowEngine`'s
   checkpoint/resume model. A mid-task crash leaves the JSON in `working` with no way to safely
   resume. **Action:** adopt the cognitive-mesh checkpoint pattern at the JSON level — write
   intermediate state on each tool call; on resume, replay from the last checkpoint.
5. **Parity harness for retort sync output** (from claw-code). claw-code's parity-harness tests
   that its CLI behavior matches reference Claude Code behavior. retort's `sync` produces 9+
   tool-specific outputs and currently relies on drift detection at write time. **Action:** add
   golden-reference snapshots for each generated config; CI fails if `retort sync` regresses.

And the converse — five capabilities the L2 harness ecosystem could steal *from* the PhoenixVC
stack:

1. **Spec-driven multi-harness rendering** (from retort). retort is the only L1 tool that
   targets multiple agent frontends from a single source. claw-code is harness-specific; OMO is
   prompt-specific; cognitive-mesh is product-specific.
2. **Generated-file discipline** (`<!-- GENERATED by Retort -->` headers + drift detection).
   None of the L2 harnesses have an equivalent "spec is source of truth, output is derived"
   story — they treat their config files as authoritative and editable.
3. **Hookify runtime guards** (from retort). File-event and bash-event regex guards enforce
   rules at tool-call time, not at review time. Most L2 harnesses have hooks but don't ship a
   guard library on top of them.
4. **5-phase lifecycle with explicit quality gates per transition** (from retort). cognitive-mesh
   has the ingredients (MAKER benchmark, build hooks) but no explicit phase machinery. OMO has
   Momus validation but no shipping phase.
5. **AI data-plane separation** (from sluice). L2 harnesses talk to providers directly. sluice
   teaches the rest of the ecosystem that LLM traffic is infrastructure that deserves its own
   gateway, telemetry, and cost attribution. Pairs naturally with docket. Most L2 harnesses
   have no equivalent — every install is its own siloed billing event.

## State and continuity — the one axis I missed

Persistence is the axis where the systems are most different and where I previously had nothing
to say. Filling that gap:

| System | Persistence mechanism | Lives where |
|--------|----------------------|-------------|
| **retort** | `.claude/state/tasks/*.json` lifecycle (submitted → accepted → working → completed), `AGENT_BACKLOG.md`, `docs/handoffs/`, `docs/history/` | Filesystem; human-authored after the fact for the longer-form docs |
| **cognitive-mesh** | `DurableWorkflowEngine` checkpointing + `HybridMemoryStore` (Redis + DuckDB) | Runtime state, not session state |
| **oh-my-openagent** | `boulder.json` (active plan + session history) + `.sisyphus/notepads/{plan}/` (wisdom accumulation across sessions) | **Strongest session-continuity story** — automatic, not human-authored |
| **claw-code** | Inherits Claude Code's context model | In-session |
| **deck** | Tauri local state + sidecar services | Per-operator desktop, not shared |
| **phoenix-flow** | Bidirectional YAML ↔ Kanban sync | Shared between humans and agents — that's its entire purpose |
| **sluice** | Redis state service | Per-request, not per-task |
| **docket** | LLM spend telemetry | Per-call, not per-task |

The takeaway: **OMO has the strongest prompt-level continuity story; cognitive-mesh has the
strongest runtime checkpoint story; phoenix-flow has the strongest human-agent shared-state
story; retort has the strongest spec-time state story.** None of them has all four. The combined
PhoenixVC stack is the only ecosystem that ships three of them in one place — and could
plausibly steal OMO's wisdom-notepad pattern to ship the fourth.

## Where they actually overlap

If you ignore the marketing and look at the verbs each system implements, the overlap collapses
to a small set of concepts. Each row says "all of these systems implement *some* version of this,
but with very different blast radius."

| Concept | Cognitive Mesh | Codeflow-Engine | Retort | Claw Code | oh-my-agent / OMO |
|---------|---------------|----------------|--------|-----------|-------------------|
| Multi-agent orchestration | `MultiAgentOrchestrationEngine.cs` (production runtime, ethics-checked) | AutoGen `GroupChatManager` (PR/CI bots) | Defines team scopes in `teams.yaml` | Subagent invocation in CLI session | 15+ named agents w/ slash commands |
| Task delegation | `IAgentOrchestrationPort.ExecuteTaskAsync` | Workflow event fan-out | File-based state machine in `.claude/state/tasks/`, lifecycle: submitted → working → completed | In-session subagent dispatch | `/orchestrate`, `/work`, `/ultrawork` |
| Durable execution / checkpointing | `DurableWorkflowEngine` with crash recovery | Retries (`attempts=3`, `delay=5`), timeout via `asyncio.wait_for` | Out of scope (build-time, not runtime) | Session state, not durable | Out of scope |
| Worktree isolation | Out of scope (server runtime) | Out of scope | `feat/agent-<name>/<slug>` branch convention | Yes (inherited from Claude Code patterns) | Yes |
| Quality gates | MAKER benchmark, ethical-reasoning checks | Lint/test/coverage in workflows | Lint, typecheck, unit tests, coverage ≥80%, drift detection — enforced at sync time | Mock-parity tests | 5-phase / 11-gate pipeline |
| Memory | `HybridMemoryStore` (Redis + DuckDB), `ReasoningTransparency` | Workflow history (1000 entries) | None — stateless config tool | `CLAUDE.md`-style plain text | Plain text + skill folders |
| Tool integration | `ToolIntegration/BaseTool.cs`, `ToolRegistry` | Entry-point registries for actions/integrations/providers | MCP/A2A config emission (`.mcp/servers.json`) | MCP-compatible | MCP, Exa, Context7, Grep.app, tmux |
| Audit & determinism | Hexagonal ports, structured logging, MAKER scoring | Workflow history retention | Drift detection across 9+ tool outputs | Parity harness against reference Claude Code (`claw doctor`) | Observable dashboards |
| **AI data plane (LLM gateway, cost)** | Calls providers directly today; in the PhoenixVC stack flows through **sluice** | Calls providers directly | Recommends **sluice** as standard gateway in spec output | Provider-agnostic harness; user wires in keys | Multi-provider routing client-side, no shared gateway |
| **Operator control plane** | Operator UI lives in **deck** (Tauri) which surfaces all PhoenixVC services | Out of scope | Out of scope | Out of scope | Out of scope |

The pattern is clear: **the coding-agent harnesses optimize for one developer at a keyboard**;
**cognitive-mesh + codeflow-engine + phoenix-flow optimize for production traffic**; **sluice +
docket optimize for the LLM data plane underneath all of it**; **retort + retort-plugins +
codeflow-plugins + mcp-org sit at build time and make all of it governable**; **deck wraps the
whole thing in an operator pane of glass.** Treat the overlap as "common vocabulary, different
production targets" rather than as competition.

## Extension surfaces — the parallel that makes the stack consistent

A subtle but important symmetry: **the PhoenixVC stack has two formal plugin systems, one at
each end**. They are *not* the same plugin system, but they look alike on purpose.

| Property | `retort-plugins` (L2.5) | `UILayer/PluginAPI/IWidgetRegistry` (L5) |
|----------|-------------------------|------------------------------------------|
| Host | An IDE (VS Code, JetBrains, Zed) | The Cognitive Mesh dashboard (Next.js) |
| Plugin unit | An IDE extension calling Retort's CLI/HTTP surface | A `WidgetDefinition` registered with the dashboard |
| Activation | `.agentkit/` directory present in workspace root | `RegisterWidgetAsync(WidgetDefinition)` |
| Languages allowed | TS (VS Code), Kotlin (JetBrains), Rust (Zed) | Whatever Next.js can render — typed via C# `WidgetDefinition` model |
| Primary verbs | `sync`, `start`, `quality-gate`, `orchestration-status` | `Register`, `Get`, `GetAll`, `Remove` |
| Audit story | Drift detection across generated configs | Versioned API, validation, stable contract |
| What it extends | The L1 spec layer's reach into the editor | The L4 runtime's reach into the operator UI |

This is the stack's hidden through-line: **the PhoenixVC org treats "extensibility" as a
first-class architectural concern at *both* boundaries** — where developers meet the spec layer,
and where operators meet the runtime. The L2 harness ecosystem (Claude Code, Claw Code, OMO,
oh-my-agent) has nothing analogous to `IWidgetRegistry` — they extend the *editor*, not the
*operator dashboard*. Conversely, no L2 harness ships an IDE-extension family for governance
the way retort-plugins does; their plugins all aim at editor productivity, not at sync/drift/
quality-gate enforcement.

The practical implication: a team adopting the PhoenixVC stack gets the *same* extension model
twice — write a small plugin against a stable contract, get drop-in support across multiple
hosts. Once at L2.5 for IDE integration, once at L5 for operator dashboards. That's a unique
architectural property worth defending.

## The combined PhoenixVC stack — what you get

Running the full PhoenixVC ecosystem together gives you a path that no L2 harness can match
because the path goes from operator desktop down to LLM gateway and back up to durable cognition:

```
                    ┌────────────────────┐
                    │  deck (L6)         │  ← operator pane of glass
                    │  Tauri desktop op  │
                    └────────┬───────────┘
                             │
   ┌─────────────────────────┼──────────────────────────┐
   │                         │                          │
   ▼                         ▼                          ▼
┌──────────┐           ┌──────────┐              ┌────────────┐
│ retort   │           │ phoenix- │              │ cognitive- │
│ +plugins │           │ flow     │              │ mesh       │  ← L4+L5
│ (L1+L2.5)│           │ (L3+L5)  │              │            │
└────┬─────┘           └────┬─────┘              └─────┬──────┘
     │ generates             │ task graph              │
     │ configs for           │ shared with             │
     │ L2 harness            │ humans                  │
     ▼                       ▼                         │
┌────────────────────────────────────────────┐         │
│  L2 harness                                │         │
│  (Claude Code / Claw Code / oh-my-openagent)│        │
└────────────────┬───────────────────────────┘         │
                 │ commits, PRs                        │
                 ▼                                     │
            ┌──────────────┐                           │
            │ codeflow-    │                           │
            │ engine (L3)  │                           │
            │ +plugins     │                           │
            └──────┬───────┘                           │
                   │                                   │
                   └────────┐                          │
                            ▼                          ▼
                ┌──────────────────────────────────────┐
                │  L0 — AI data plane                  │
                │  ┌─────────┐  ┌─────────┐            │
                │  │ sluice  │→ │ docket  │            │
                │  │ gateway │  │ cost    │            │
                │  └─────────┘  └─────────┘            │
                │  All LLM traffic from above flows    │
                │  through here; cost flows back up    │
                └──────────────────────────────────────┘
```

What this combination uniquely provides:

1. **A single spec language across the dev loop and the runtime.** Retort YAML defines team
   scopes; cognitive-mesh ports/adapters define the production agent contracts. The same
   "team-quality" or "team-security" concept can exist in both.
2. **Hand-off without rewrite.** A workflow drafted in codeflow-engine for PR review can graduate
   to a `DurableWorkflowEngine` job in cognitive-mesh once it needs replay, ethical checks, or
   crash recovery.
3. **Governance you can audit at four layers.** Retort drift detection at L1, codeflow-engine
   workflow history at L3, MAKER scores + `ReasoningTransparency` at L4, and **sluice + docket
   per-call cost telemetry at L0** — four independent audit surfaces, not one.
4. **Per-call cost attribution all the way down.** sluice intercepts every LLM call from every
   PhoenixVC service and feeds docket. When a cognitive-mesh workflow runs hot, you can trace
   spend back to the originating workflow without instrumenting cognitive-mesh.
5. **Operator pane of glass.** deck surfaces L0–L5 from one Tauri desktop app, so operators
   don't context-switch between terraform, kubectl, the cognitive-mesh dashboard, the docket
   UI, and the phoenix-flow Kanban.
6. **Polyglot reach across the whole stack.** TS/Node at L1, Python at L3 + L0 (sluice), .NET
   at L4, Rust + TS + .NET at L6 (deck). HCL + Terraform at L0. No harness ecosystem spans
   that breadth.
7. **Two formal extension surfaces.** retort-plugins at L2.5 (IDE bridge) and
   `UILayer/PluginAPI/IWidgetRegistry` at L5 (dashboard widgets) — see [Extension surfaces](#extension-surfaces--the-parallel-that-makes-the-stack-consistent).

## What the combined stack doesn't do (and what the harnesses do better)

Be honest about the gaps. If the only thing you need is "a really good coding agent in my
terminal," you don't need the PhoenixVC stack at all. Use Claw Code or oh-my-openagent and stop
there. Specifically:

| Capability | PhoenixVC stack | Best-in-class harness |
|------------|-----------------|----------------------|
| Hash-anchored / surgical file edits | Not present | **OMO** (`LINE#ID` content hash) |
| LSP-aware refactor (workspace rename, ast-grep) | Not present | **OMO** (25 languages) |
| Parallel sub-agent fan-out for codebase exploration | Server-side only | **Claude Code / Claw Code** (up to 7 parallel) |
| Plain-text memory the model reads automatically | `HybridMemoryStore` is for runtime, not the IDE | **Claude Code** (`CLAUDE.md`) |
| Per-task multi-provider model routing in the editor | Out of scope | **OMO** (provider routing by task category) |
| Multi-IDE config from one spec | Not without Retort | **Retort** itself solves this — and works with all of the above |
| Clean-room reproducible CLI harness | Not the goal | **Claw Code** (audit-grade Rust rewrite) |

Conversely, things the harness ecosystem will not give you:

- Durable workflow replay with checkpoint files (`DurableWorkflowEngine`)
- Brandom/Floridi-style ethical reasoning gates wired into orchestration
- A MAKER-style benchmark suite for measuring agent reasoning quality across releases
- Five-layer hexagonal architecture with enforced layer dependencies (Foundation ← Reasoning
  ← Metacognitive ← Agency ← Business)
- A `Wolverine`/`MediatR` event bus carrying production agent traffic

## How to combine them in practice

Three concrete integration shapes, in order of effort.

### Shape A — Retort governs your harness layer (low effort, high payoff)

1. Add a `.agentkit/spec/` directory to `cognitive-mesh`.
2. Define `teams.yaml` mirroring the existing team agents (`team-foundation`, `team-agency`,
   `team-reasoning`, `team-business`, `team-metacognitive`, `team-quality`, etc. — they already
   exist as Claude Code skills in this repo).
3. Run `retort sync` to regenerate `.claude/`, `.cursor/`, `.windsurf/`, `.github/copilot-instructions.md`,
   `AGENTS.md`, `WARP.md`, etc. from one source.
4. Wire `retort sync --diff` into CI to catch drift the same way the existing
   `stop-build-check.sh` hook catches build drift.

This is non-invasive: nothing in the .NET solution changes. The win is consistency across every
developer's harness of choice.

### Shape B — Codeflow-Engine handles PR / review automation outside the runtime

1. Stand up Codeflow-Engine alongside the `.NET` solution (separate process / container).
2. Wire its GitHub integration to the repo so it handles PR triage, label routing, review-bot
   spawning, and CI failure re-injection — work that doesn't belong on the production cognition
   path.
3. Have Codeflow-Engine workflows publish their results onto a queue Cognitive Mesh consumes via
   its existing `IMessageBus`. The boundary is one queue — Codeflow-Engine sees no .NET types,
   Cognitive Mesh sees no Python.

### Shape C — Promote workflows from L3 to L4

For any Codeflow-Engine workflow that becomes business-critical (touches customer data, must be
replayable for compliance, must clear ethical-reasoning checks):

1. Re-implement it as a `DurableWorkflowEngine` workflow in `src/AgencyLayer/Orchestration/`.
2. Add MAKER-benchmark coverage in `tests/AgencyLayer/Orchestration/`.
3. Decommission the Codeflow-Engine version, keeping only its event-trigger surface as the entry
   point.

This is the "graduation path" — Python prototype, .NET production. The two layers stay distinct
on purpose.

## Decision matrix

| If your need is… | Reach for… |
|------------------|-----------|
| One developer wants a fast terminal AI | Claw Code, Claude Code, or oh-my-openagent |
| A team wants consistent agent configs across mixed IDEs | **Retort** (alone is fine) |
| Automate PR review / CI failure triage with multi-agent chat | **Codeflow-Engine** |
| Run ethically-checked, checkpointed agent workflows in production traffic | **Cognitive Mesh AgencyLayer** |
| Hash-anchored surgical file edits | OMO (oh-my-openagent) |
| Audit-grade open-source CLI harness | Claw Code |
| Multi-agent orchestration *without* worktree mechanics (server-side jobs) | Cognitive Mesh, not a harness |
| Multi-agent orchestration *with* worktree mechanics (parallel coding) | oh-my-agent or oh-my-pi, not Cognitive Mesh |
| A combined stack that goes from "developer typing in the IDE" to "audited production cognition" | **Retort + harness + Codeflow-Engine + Cognitive Mesh** — all four layers |

## Anti-patterns to avoid

1. **Using `MultiAgentOrchestrationEngine.cs` to drive an editor session.** It's a server-side
   orchestrator, not a CLI harness. The latency profile, memory model, and tool registry are
   wrong for interactive use.
2. **Treating Claw Code or OMO as a production runtime.** They're audit-grade *developer* tools.
   They have no durable workflow story, no ethical-reasoning gate, no L0 data plane separation.
3. **Hand-maintaining `.claude/`, `.cursor/`, `.windsurf/` in parallel.** This is exactly what
   Retort exists to eliminate. If you have more than two harnesses, the manual cost compounds
   fast.
4. **Forking Codeflow-Engine logic into Cognitive Mesh prematurely.** Keep prototypes in Python
   until they earn the .NET rewrite via the Shape C graduation path.
5. **Letting harness slash commands and Retort team definitions drift.** If you adopt Retort,
   make `retort sync --diff` a required check, not a nice-to-have.

## SWOT — PhoenixVC Ecosystem (full 13-repo stack)

### Strengths

- **Full-stack coverage** from L0 (AI data plane: sluice + docket) through L1 (retort, mcp-org)
  and L2.5 (retort-plugins, codeflow-plugins) to L3 (codeflow-engine, phoenix-flow), L4
  (cognitive-mesh), L5 (Cognitive Mesh UILayer + phoenix-flow Kanban) and L6 (deck operator
  control plane). **Seven layers.** No single competing project spans more than two.
- **Per-call cost attribution all the way down.** sluice intercepts every LLM call from every
  PhoenixVC service; docket attributes spend back to originating workflows. No L2 harness
  ecosystem has anything analogous.
- **Two symmetric extension surfaces.** `retort-plugins` extends the spec layer into the IDE
  via TS/Kotlin/Rust extensions; `UILayer/PluginAPI/IWidgetRegistry` extends the runtime layer
  into the dashboard via versioned widget contracts. Same architectural pattern, both ends of
  the stack — see [Extension surfaces](#extension-surfaces--the-parallel-that-makes-the-stack-consistent).
- **Operator pane of glass.** deck (Tauri) gives operators a single desktop app that surfaces
  L0–L5 — service management, infrastructure deploy, dashboards, test execution. No need to
  context-switch between terraform, kubectl, the cognitive-mesh dashboard, the docket UI, and
  the phoenix-flow Kanban.
- **Build-time vs runtime separation done right.** retort + mcp-org + retort-plugins +
  codeflow-plugins are pure compilers. sluice + docket + cognitive-mesh + codeflow-engine +
  phoenix-flow + deck are pure runtimes. The boundary is explicit in the architecture, not
  implicit in tooling.
- **Polyglot reach across the whole stack.** TS/Node at L1 + L2.5 + L5, Python at L3 + L0,
  .NET at L4 (and as the deck sidecar), Rust at L0 (sluice infra) + L6 (deck shell), Kotlin at
  L2.5 (retort-plugins JetBrains), HCL/Terraform at L0 infra. Each layer uses the right
  language; nothing is shoehorned.
- **Audit-grade runtime semantics.** `DurableWorkflowEngine` checkpointing, ethical-reasoning
  gates (Brandom + Floridi), MAKER benchmark, and `ReasoningTransparency` give independent
  audit surfaces — and now sluice/docket add per-call telemetry on top.
- **Hexagonal architecture enforcement.** Layer dependencies in cognitive-mesh are codified
  (Foundation ← Reasoning ← Metacognitive ← Agency ← Business ← UI) and
  `TreatWarningsAsErrors=true` forces interface hygiene.
- **Graduation path.** Workflows can be prototyped in codeflow-engine, surfaced to humans via
  phoenix-flow, and graduated to a `DurableWorkflowEngine` job in cognitive-mesh once they
  earn durability/ethics requirements — without rewriting the spec layer.
- **First-class team metaphor.** The team-skill files in this repo (`team-foundation`,
  `team-agency`, `team-quality`, etc.) already match Retort's `teams.yaml` shape — the migration
  cost is near-zero.

### Weaknesses

- **Onboarding cost is now extreme.** *Thirteen repos.* Six runtimes (.NET / Node / Python /
  Rust / multi-language IDE plugins / Tauri shell). Seven layers. Two plugin systems. Four
  audit surfaces. A new contributor must absorb a lot before being productive. Compared to
  "install Claw Code, type a prompt," the activation energy is two orders of magnitude higher.
- **No L2 (coding-agent) story of its own.** PhoenixVC has L2.5 (retort-plugins, codeflow-plugins)
  but not L2 proper — it doesn't ship a coding-agent harness. It bridges *into* whatever
  harness you already use, but if Claude Code, Claw Code, or OMO change their tool-call
  protocol, the L2.5 plugins must follow. The runtime is still someone else's.
- **Partition philosophy is implicit, not enforced.** retort, cognitive-mesh, and oh-my-agent
  all partition by domain. OMO partitions by cognitive function (planner/reviewer/executor)
  and *enforces* the split. retort's `/orchestrate` lifecycle implies a soft cognitive-function
  split but doesn't prevent a team agent from also planning. See
  [Two partition philosophies](#two-partition-philosophies-the-insight-that-reframes-everything).
- **Spec/runtime alignment is not yet automatic.** Retort spec teams and cognitive-mesh ports
  are conceptually parallel but not generated from each other. Drift between them is possible.
- **No prompt-level wisdom accumulation.** OMO's `.sisyphus/notepads/{plan}/` automatic learning
  extraction has no PhoenixVC analogue — `docs/history/` is human-authored after the fact, and
  cognitive-mesh's `MetacognitiveLayer/ContinuousLearning` runs at runtime, not at the prompt
  level. This is the single most copyable idea from OMO.
- **No semantic-category model routing.** OMO's `visual-engineering` / `ultrabrain` / `quick`
  category system decouples intent from model choice. retort has team commands but no routing
  layer; sluice could *become* the routing implementation but doesn't have a category contract
  yet.
- **codeflow-engine maturity gaps.** AutoGen is imported behind `try/except` and isn't declared
  in base dependencies; max-concurrent enforcement is unverified; history retention is in-process
  only. (See [`codeflow-engine.md`](../02-internal-repos/codeflow-engine.md).)
- **No hash-anchored editing, no LSP integration, no in-editor multi-provider routing.** OMO and
  similar L2 harnesses meaningfully out-execute the PhoenixVC stack on raw code-modification
  precision. None of L0–L6 fixes that — only an L2 harness can.
- **Build pipeline is strict.** `TreatWarningsAsErrors`, CS1591 enforcement, MAKER tests — great
  for governance, painful for fast experiments. codeflow-engine in Python is the relief valve,
  but the boundary must be explicit.
- **Documentation surface is large and uneven.** This `docs/orchestration-evaluation/` tree alone
  has 10+ subfolders; finding the "right" doc as a newcomer is non-trivial. This very file is
  now ~700 lines and is the third revision in one session.

### Opportunities

- **Adopt OMO's wisdom-notepad pattern.** Add `.claude/state/wisdom/{plan}/` to retort's spec;
  have hookify post-task hooks extract learnings; have `/orchestrate` and `/plan` inject the
  relevant wisdom file at session start. Closes the single biggest gap vs OMO.
- **Adopt OMO's semantic-category routing — and make sluice the implementation.** Add a
  `category:` field to team specs; have sluice route by category to the cheapest model that
  can handle the workload. This makes the L0 ↔ L1 boundary explicit and gives the whole
  PhoenixVC stack OMO-grade routing without leaving the gateway.
- **Make plan/execute separation prescriptive in retort.** Have `/orchestrate` lifecycle prevent
  team agents from invoking planning verbs and prevent the planner from invoking execution
  verbs. Adopts OMO's defensible Atlas/Sisyphus split without abandoning retort's domain teams.
- **Codify the spec ↔ runtime contract.** Generate cognitive-mesh agent ports directly from
  `teams.yaml` (or the inverse), so adding a team in one place propagates everywhere. The
  "missing middle" between Retort and Cognitive Mesh.
- **Package the MAKER benchmark as a Retort quality gate.** Harness configs that degrade
  reasoning scores fail spec drift. Turns L1 governance into a runtime quality control loop.
- **Add a parity-harness for `retort sync` output.** Borrow claw-code's pattern: golden-reference
  snapshots for each generated config; CI fails if `retort sync` regresses.
- **Adopt durable-workflow checkpointing for retort task files.** retort's
  `.claude/state/tasks/*.json` lifecycle is a state machine but lacks `DurableWorkflowEngine`'s
  checkpoint/resume model. Adopt cognitive-mesh's checkpoint pattern at the JSON level.
- **Productize the graduation path.** "Prototype in codeflow-engine → surface in phoenix-flow
  → graduate to cognitive-mesh" is a rare capability. Document it as a first-class workflow.
- **Reference deployment.** Ship a one-command bootstrap (`./scripts/bootstrap-phoenixvc.sh`)
  that wires retort + a chosen harness + sluice + docket + codeflow-engine + cognitive-mesh +
  deck together. Lowers the onboarding cost dramatically — probably the single highest-leverage
  weakness fix.

### Threats

- **L2 harness ecosystem moves fast.** Claw Code reached 175k stars in days. New harness
  primitives (hash-anchored edits, LSP integration, parallel subagents) become baseline
  expectations, and the PhoenixVC stack inherits them only via whichever L2 it integrates with.
- **Vendor harnesses (Claude Code, Cursor, Windsurf, Antigravity) consolidate L1.** If a single
  vendor harness wins enough share, retort's "9+ outputs from one spec" value proposition
  shrinks toward "one output from one spec."
- **Standards displacement.** If A2A or MCP solidify a cross-tool team/skill schema, retort's
  bespoke YAML may need to migrate or risk becoming a parallel dialect. mcp-org partly hedges
  against this but doesn't eliminate it.
- **Talent fragmentation.** Maintaining TS + Python + .NET + Rust + Kotlin + HCL requires six
  skill sets. Smaller teams will struggle; this favors monoglot competitors.
- **Determinism vs. capability tradeoff.** Strict ethical-reasoning and durable-workflow gates
  add friction. Competing systems (especially L2 harnesses) ship faster *because* they don't
  have those gates. In domains where regulators don't require them, the PhoenixVC stack looks
  expensive.
- **codeflow-engine drift risk.** With AutoGen as an optional, undeclared dependency, a future
  AutoGen breaking change could leave codeflow-engine in a half-broken state.
- **sluice as single point of failure.** Once you route all org LLM traffic through one
  gateway, the gateway is a high-value target and a hard outage. The benefits (cost
  attribution, auth, semantic cache) are real, but the failure mode is now "no LLM traffic
  anywhere" instead of "one service is down."
- **Documentation drift.** Seven-layer architectures across thirteen repos with 10+ docs
  subfolders are notorious for silent doc rot. This file will be obsolete the moment a
  fourteenth PhoenixVC repo joins.

## Weighted comparison on key capabilities

Scoring uses the project's existing 1.0–5.0 decimal scale (see
[`01-methodology/scoring-framework.md`](../01-methodology/scoring-framework.md)). Weights use
**Profile 5: Multi-Agent Reasoning** from
[`weight-profiles.md`](../01-methodology/weight-profiles.md), since that's the workload class
where these systems compete. Higher is better.

**Weights** (Profile 5): Determinism 0.22 · Maintainability 0.18 · Fault Tolerance 0.16 ·
Integration Ease 0.14 · Latency 0.10 · Scalability 0.08 · Throughput 0.07 · Efficiency 0.05.

### Per-system scores

| System | Det. | Maint. | F.T. | Int. | Lat. | Scale | Thru. | Eff. |
|--------|:----:|:------:|:----:|:----:|:----:|:-----:|:-----:|:----:|
| **PhoenixVC stack (combined: Cognitive Mesh + Retort + retort-plugins + Codeflow-Engine)** | 4.5 | 3.5 | 4.5 | 4.5 | 3.0 | 4.0 | 4.0 | 3.5 |
| Cognitive Mesh (alone, all 6 layers) | 4.5 | 4.0 | 4.5 | 3.5 | 3.0 | 4.0 | 4.0 | 3.5 |
| Codeflow-Engine (alone) | 3.0 | 3.0 | 4.0 | 3.0 | 3.0 | 4.0 | 3.0 | 3.0 |
| Retort + retort-plugins (L1 + L2.5) | 4.0 | 4.0 | 3.0 | 5.0 | n/a | n/a | n/a | n/a |
| Claw Code | 3.5 | 4.0 | 3.0 | 4.5 | 4.5 | 3.0 | 4.0 | 4.0 |
| Claude Code | 3.0 | 4.5 | 3.5 | 5.0 | 4.5 | 3.0 | 4.0 | 4.0 |
| oh-my-agent | 3.0 | 3.5 | 3.0 | 4.5 | 4.0 | 3.0 | 4.0 | 4.0 |
| oh-my-openagent (OMO) | 3.5 | 3.5 | 3.0 | 4.5 | 4.5 | 3.5 | 4.5 | 4.0 |

> **Why retort-plugins lifts the combined Integration Ease score from 4.0 to 4.5.** Retort
> alone gets a 5.0 on Integration Ease (one YAML → 9+ harness outputs). Adding the IDE bridge means
> developers don't have to leave their editor to invoke any of it — `@retort` Copilot Chat,
> JetBrains tool window, Zed slash commands. The combined-stack Integration Ease was previously
> dragged down by Cognitive Mesh's .NET ports/adapters surface (3.5); retort-plugins partly
> recovers that by giving the same developers an in-editor handle on the spec layer.

### Weighted totals (Profile 5: Multi-Agent Reasoning)

| System | Weighted score (1–5) | Percentage |
|--------|:--------------------:|:----------:|
| **PhoenixVC stack (combined)** | **4.05** | **80.9%** |
| Cognitive Mesh (alone) | 4.00 | 79.9% |
| Claude Code | 3.90 | 78.0% |
| Claw Code | 3.77 | 75.4% |
| oh-my-openagent (OMO) | 3.76 | 75.1% |
| oh-my-agent | 3.52 | 70.4% |
| Codeflow-Engine (alone) | 3.24 | 64.8% |
| Retort + retort-plugins (alone) | n/a (L1 + L2.5 only — not directly comparable on runtime metrics) | — |

Calculations use the Profile 5 weight vector applied to each row in the per-system table. Cells
marked `n/a` for Retort reflect that L1 spec governance has no runtime latency, scalability,
throughput, or efficiency to score.

### What the weights say

- **The combined PhoenixVC stack narrowly outscores Cognitive Mesh alone on Profile 5** — and
  this flipped relative to the previous version of this doc once retort-plugins entered the
  picture. The combined stack still pays a Maintainability tax (3.5 vs 4.0), but Integration
  Ease rises from 3.5 (Cognitive Mesh ports/adapters alone) to 4.5 (Retort + retort-plugins
  in-editor) — the +1.0 on Integration Ease at weight 0.14 outweighs the −0.5 on
  Maintainability at weight 0.18 by about 0.05 points. **The takeaway: until retort-plugins
  existed, "run Cognitive Mesh alone" was the right Profile-5 answer; now the combined stack
  is.** It's a small margin, but it's the right direction.
- **Claude Code edges out Claw Code despite Claw Code being a clean-room rewrite** because the
  rewrite is younger (lower Maintainability evidence) and has a less complete integration
  surface (4.5 vs 5.0). The gap is small and will close as Claw Code matures.
- **OMO is the strongest L2 harness on this profile** when you exclude Claude Code, driven by
  hash-anchored edits and LSP integration improving its Determinism (3.5) and Throughput (4.5)
  scores above the rest of the L2 field.
- **The L2 harnesses cluster within ~8 percentage points of Cognitive Mesh** even on
  Profile 5, which is the profile *most* hostile to them. They lose Determinism (no replay)
  and Fault Tolerance (no durable workflows) but recover ground on Latency, Maintainability,
  and Integration Ease. The right reading isn't "Cognitive Mesh wins" — it's "they're solving
  different problems and Profile 5 happens to favor the runtime side."
- **Codeflow-Engine alone ranks last among production-grade systems** because the
  AutoGen-behind-`try/except` integration gap, unverified `max_concurrent` enforcement, and
  in-process history retention all knock down its production-runtime metrics. It earns its
  place in the *combined* stack as the L3 graduation lane, not as a standalone.
- **Retort doesn't fit the runtime scoring model at all** — it's a spec-time tool. Scoring
  it on Latency or Throughput would be a category error. Its 5.0 Integration Ease is real:
  9+ harness outputs from one spec is unmatched.

> **Caveat: this scoring table now lags the rest of the doc.** It scores four PhoenixVC
> components against the L2 harness ecosystem, but the rest of this doc covers all 13 PhoenixVC
> repos plus the build-time/runtime axis. A complete scoring would add rows for sluice (L0
> data plane), docket (L0 cost telemetry), phoenix-flow (L3 + L5 shared task graph), deck (L6
> control plane), mcp-org (L1 org context) — but most of those don't fit Profile 5's metric
> shape (sluice/docket are infrastructure, deck is desktop UX, mcp-org is build-time). The
> scoring above remains valid for the *runtime* subset; treat it as a partial measurement
> rather than a complete one.

### How the score changes by profile

Recomputing the same per-system scores under other profiles flips the leaderboard. Approximate
deltas:

| Profile | Top system | Why |
|---------|-----------|-----|
| **Profile 1 — Interactive** | OMO / Claude Code | Latency 0.22 + Integration Ease 0.18 swamps Cognitive Mesh's determinism advantage |
| **Profile 2 — Batch** | Cognitive Mesh / PhoenixVC stack | Scalability 0.22 + Fault Tolerance 0.22 favors the durable runtime |
| **Profile 3 — Long-Running Durable** | Cognitive Mesh / PhoenixVC stack | Fault Tolerance 0.28 + Determinism 0.20 — this is `DurableWorkflowEngine`'s home turf |
| **Profile 4 — Event-Driven** | OMO / Codeflow-Engine | Latency 0.20 + Integration Ease 0.20 + Efficiency 0.15 — event fan-out is Codeflow-Engine's design center |
| **Profile 5 — Multi-Agent Reasoning** | **PhoenixVC stack / Cognitive Mesh** (this table) | Determinism + Maintainability + Fault Tolerance |

The honest summary: **the PhoenixVC stack wins on profiles where you'd be regulated**; the L2
harnesses win on profiles where you'd be shipping a product to developers. Plan accordingly.

### Caveats on these scores

1. Scores marked here are best-effort based on README evidence and the `02-internal-repos/`
   writeups already in this tree. They are *not* benchmark-derived. Treat them as a structured
   prior, not a measurement.
2. The L2 harnesses are scored on their *production runtime behavior*, which is not their
   intended workload. Scoring them on developer-experience metrics (which Profile 5 doesn't
   include) would yield very different numbers.
3. Cognitive Mesh's Latency score (3.0) reflects server-side runtime behavior, not interactive
   chat. It would be lower if scored as an editor tool — but it isn't one.
4. The "combined PhoenixVC stack" row is *not* a simple average of the three component rows;
   it reflects what each component contributes at its own layer (e.g., Retort's 5.0 Integration
   Ease lifts the combined Integration Ease score).

## Open questions (PRs welcome)

- Should `cognitive-mesh` ship a `.agentkit/spec/` directory natively? The team-skill files in
  this repo (`team-foundation`, `team-reasoning`, etc.) already imply the schema.
- Is there a clean way to expose `DurableWorkflowEngine` checkpoints as a codeflow-engine
  integration target, so Python workflows can checkpoint into the same store as .NET workflows?
- Could the MAKER benchmark be packaged as a Retort quality gate, so harnesses fail the spec
  drift check when their generated configs degrade reasoning scores?
- Should sluice grow a `category:` routing contract that OMO-style semantic categories can
  target? See [Five capabilities the PhoenixVC stack should consider stealing](#five-capabilities-the-phoenixvc-stack-should-consider-stealing)
  item 2.
- Should `.claude/state/wisdom/{plan}/` become a first-class part of retort's spec, mirroring
  OMO's `.sisyphus/notepads/{plan}/` pattern? Item 1 in the same list.
- Should phoenix-flow's bidirectional YAML ↔ Kanban sync become the canonical surface for
  retort task-file lifecycle, so humans see what agents are doing without poking at JSON files
  in `.claude/state/tasks/`?
- Can mcp-org provide cross-repo wisdom propagation, so a learning extracted in one PhoenixVC
  repo becomes available to agents in another?

## Sources

### In-repo

- [Cognitive Mesh — `src/AgencyLayer/README.md`](../../../src/AgencyLayer/README.md)
- [Cognitive Mesh — `src/UILayer/PluginAPI/IWidgetRegistry.cs`](../../../src/UILayer/PluginAPI/IWidgetRegistry.cs)
- [Cognitive Mesh — `.claude/rules/architecture.md`](../../../.claude/rules/architecture.md) (six-layer dependency rules)
- [Cognitive Mesh — `CLAUDE.md`](../../../CLAUDE.md)
- [codeflow-engine internal repo writeup](../02-internal-repos/codeflow-engine.md)
- [`docs/orchestration-evaluation/03-external-engines/coding-agent-orchestration/fleet-orchestration.md`](../03-external-engines/coding-agent-orchestration/fleet-orchestration.md)

### PhoenixVC ecosystem (13 repos)

- [`phoenixvc/cognitive-mesh`](https://github.com/phoenixvc/cognitive-mesh) — enterprise agent/LLM platform (this repo)
- [`phoenixvc/retort`](https://github.com/phoenixvc/retort) — Windows-first polyglot AI-orchestration framework, CLI + Ink TUI, 9+ harness outputs from one spec
- [`phoenixvc/retort-plugins`](https://github.com/phoenixvc/retort-plugins) — IDE extensions for VS Code (TS), JetBrains (Kotlin), Zed (Rust)
- [`phoenixvc/codeflow-engine`](https://github.com/phoenixvc/codeflow-engine) — Python multi-agent PR/DevOps automation
- [`phoenixvc/codeflow-plugins`](https://github.com/phoenixvc/codeflow-plugins) — VS Code AutoPR + multi-agent assist (L2.5 bridge for L3)
- [`phoenixvc/sluice`](https://github.com/phoenixvc/sluice) — OpenAI-compatible AI gateway on Azure Container Apps; LiteLLM, semantic cache, rate limit, telemetry
- [`phoenixvc/docket`](https://github.com/phoenixvc/docket) — LLM spend tracking & cost analytics, sluice telemetry consumer
- [`phoenixvc/phoenix-flow`](https://github.com/phoenixvc/phoenix-flow) — Human + agent shared task graph, React Kanban UI, MCP server, bidirectional YAML sync
- [`phoenixvc/mcp-org`](https://github.com/phoenixvc/mcp-org) — Org-level MCP server for cross-repo tasks and roadmap management
- [`phoenixvc/deck`](https://github.com/phoenixvc/deck) — Tauri operator control plane; surfaces sluice, docket, cognitive-mesh, phoenix-flow, retort, mcp-org
- [`phoenixvc/mystira-workspace`](https://github.com/phoenixvc/mystira-workspace) — consumer of the stack ("AI-powered interactive storytelling for children")
- [`phoenixvc/phoenix-website`](https://github.com/phoenixvc/phoenix-website) — public web presence
- [`phoenixvc/.github`](https://github.com/phoenixvc/.github) — org metadata

### L2 coding-agent harnesses

- [`instructkr/claw-code`](https://github.com/instructkr/claw-code) — Rust clean-room rewrite of Claude Code harness; `claw doctor` parity-harness against reference Claude Code
- [Claw Code launch coverage (24-7 Press Release)](https://www.24-7pressrelease.com/press-release/533389/claw-code-launches-open-source-ai-coding-agent-framework-with-72000-github-stars-in-first-days)
- [`first-fluke/oh-my-agent`](https://github.com/first-fluke/oh-my-agent) — `.agents/`-based portable multi-agent harness
- [`code-yeongyu/oh-my-openagent`](https://github.com/code-yeongyu/oh-my-openagent) — OpenCode plugin; Prometheus/Metis/Momus → Atlas → Sisyphus-Junior cognitive role hierarchy; `boulder.json` + `.sisyphus/notepads/{plan}/` wisdom accumulation; semantic-category model routing; hash-anchored edits + LSP
- [`can1357/oh-my-pi`](https://github.com/can1357/oh-my-pi) — terminal AI agent with parallel execution
- [Inside Claude Code architecture deep-dive](https://www.penligent.ai/hackinglabs/inside-claude-code-the-architecture-behind-tools-memory-hooks-and-mcp/)

### External analyses incorporated

- Peer AI comparative analysis of retort vs cognitive-mesh vs oh-my-openagent vs claw-code
  (provided by user, April 2026) — supplied the build-time/runtime axis reframe, the partition-
  philosophy (domain vs cognitive function) distinction, the wisdom-notepad cross-pollination
  idea, the semantic-category routing idea, several factual corrections (claw-code's
  72%-Rust split, OMO's six archetypes, retort's 9+ harness count, claw-code's parity-harness
  story).
