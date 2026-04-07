# PhoenixVC Stack (Cognitive Mesh + Retort + Codeflow-Engine) vs. Coding-Agent Harnesses (Claw Code, oh-my-agent, and friends)

> Companion comparison to [`custom-vs-established.md`](custom-vs-established.md). This doc answers the
> narrower question: **what does the combined PhoenixVC stack give you that a popular coding-agent
> harness doesn't, and vice versa?**

The PhoenixVC stack and the coding-agent-harness ecosystem are often discussed as if they competed.
They don't. They sit at different layers of the stack and solve different problems. This doc lines
them up so the overlap and the gaps are obvious.

## TL;DR

| Question | Answer |
|----------|--------|
| Are these substitutes? | **No.** Cognitive Mesh runs production cognition; harnesses orchestrate the developer's editor. |
| Where does the overlap actually live? | **Multi-agent orchestration semantics, task delegation, quality gates, worktree isolation.** |
| Should we pick one over the other? | **Combine.** Use Retort to govern the harness layer, Cognitive Mesh + Codeflow-Engine to govern the runtime layer. |
| What's the biggest mistake? | Trying to use Cognitive Mesh's `MultiAgentOrchestrationEngine` as an *editor* harness, or using Claw Code as a *production* agent runtime. They are not the same workload. |

## The four layers

```
┌─────────────────────────────────────────────────────────────┐
│  L4 — Runtime cognition          (production traffic)       │
│       ▸ Cognitive Mesh AgencyLayer                          │
│       ▸ MultiAgentOrchestrationEngine, DurableWorkflowEngine│
│       ▸ Ethical reasoning, MAKER benchmark, checkpointing   │
├─────────────────────────────────────────────────────────────┤
│  L3 — DevOps automation          (CI / PR / review bots)    │
│       ▸ Codeflow-Engine (Python)                            │
│       ▸ AutoGen multi-agent, GitHub/Linear/Slack/Axolo      │
│       ▸ Workflow retries, event fan-out                     │
├─────────────────────────────────────────────────────────────┤
│  L2 — Developer harness          (the IDE / CLI agent)      │
│       ▸ Claw Code, Claude Code, oh-my-openagent, oh-my-pi   │
│       ▸ Subagents, hooks, skills, MCP, worktree isolation   │
│       ▸ Hash-anchored edits, LSP, tool-call loops           │
├─────────────────────────────────────────────────────────────┤
│  L1 — Spec & governance          (config sync, team rules)  │
│       ▸ Retort (`.agentkit/spec/*.yaml`)                    │
│       ▸ Renders configs for 16 harnesses from one source    │
│       ▸ Quality gates, drift checks, GitHub Actions         │
└─────────────────────────────────────────────────────────────┘
```

The PhoenixVC stack covers **L1 + L3 + L4**. The coding-agent-harness ecosystem covers **L2** —
and most of them ship with their own bundled L1 (per-tool config, hooks, agents folder). Retort
exists precisely to push that L1 out of each individual harness and into a shared spec.

## The contenders

### PhoenixVC stack

| Repo | Layer | Language | Primary unit | Differentiator |
|------|-------|----------|--------------|----------------|
| [`phoenixvc/cognitive-mesh`](https://github.com/phoenixvc/cognitive-mesh) | L4 | .NET 9 / C# | Agent task / workflow checkpoint | Five-layer hexagonal architecture, ethical reasoning (Brandom + Floridi), durable workflow engine with crash recovery, MAKER benchmark, ConclAIve recipes |
| [`phoenixvc/retort`](https://github.com/phoenixvc/retort) | L1 | Node.js / TypeScript (Ink + React TUI) | YAML spec → tool-specific config | One YAML, 16 tool outputs, file-based task delegation protocol, Handlebars templates, drift detection |
| [`phoenixvc/codeflow-engine`](https://github.com/phoenixvc/codeflow-engine) | L3 | Python (84%) | Workflow / event handler | AutoGen-backed multi-agent path, GitHub/Linear/Slack/Axolo integrations, event fan-out, exponential-backoff retries |

### Coding-agent harnesses (L2)

| Harness | Origin | Language | Notable architecture |
|---------|--------|----------|----------------------|
| **Claw Code** ([`instructkr/claw-code`](https://github.com/instructkr/claw-code)) | Clean-room rewrite of Claude Code after the npm source-map leak (March 2026) | Rust 95.9%, Python 3.6% reference workspace | Built on `oh-my-codex`, CLI subcommands (`prompt`, `login`, `doctor`), session management, OAuth/API-key auth, mock-parity test harness |
| **Claude Code** | Anthropic | TypeScript / Node | Subagents, hooks (PreToolUse/PostToolUse), skills with progressive disclosure, MCP, worktree isolation, plain-text `CLAUDE.md` memory |
| **oh-my-agent** ([`first-fluke/oh-my-agent`](https://github.com/first-fluke/oh-my-agent)) | Community | `.agents/`-based | 15 specialized agents (frontend/backend/db/mobile/qa/tf-infra…), 5-phase workflow with 11 review gates, portable across Antigravity, Claude Code, Cursor, Gemini CLI, Codex CLI, OpenCode |
| **oh-my-openagent / OMO** ([`code-yeongyu/oh-my-openagent`](https://github.com/code-yeongyu/oh-my-openagent)) | Community | TypeScript + Bun | OpenCode plugin, hash-anchored edits (`LINE#ID`), LSP + ast-grep across 25 languages, multi-provider routing by task category, Sisyphus/Hephaestus/Prometheus/Oracle agents |
| **oh-my-pi** ([`can1357/oh-my-pi`](https://github.com/can1357/oh-my-pi)) | Community | Mixed | Parallel execution framework, 6 bundled agents, isolation backends |
| **oh-my-codex (OMX)** | Community | — | Hooks + agent teams + HUD on top of Codex CLI |
| **Antigravity / Cursor / Windsurf / Gemini CLI / Codex CLI / Junie / Cline / Roo Code** | Various vendors | Various | Each has its own native config dialect — Retort generates all of them from one spec |

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
| Audit & determinism | Hexagonal ports, structured logging, MAKER scoring | Workflow history retention | Drift detection across 16 tool outputs | Mock parity harness | Observable dashboards |

The pattern is clear: **the coding-agent harnesses optimize for one developer at a keyboard**;
**Cognitive Mesh + Codeflow-Engine optimize for production traffic**; **Retort sits between them
and makes the L1 governance consistent**. Treat the overlap as "common vocabulary, different
production targets" rather than as competition.

## The combined PhoenixVC stack — what you get

Running all three PhoenixVC repos together gives you a developer-loop-to-production-runtime path
that none of the individual harnesses provide:

```
   Developer (IDE / CLI)
        │
        │  ↑ Retort renders .claude/, .cursor/, .windsurf/, etc.
        │  ↑ Retort defines team scopes & quality gates
        │
        ▼
   L2 harness (Claude Code / Claw Code / oh-my-agent)
        │
        │  ─→ commits, PRs
        │
        ▼
   Codeflow-Engine
        │  workflow validation, retries, event fan-out
        │  GitHub/Linear/Slack/Axolo integration
        │  AutoGen review agents
        ▼
   Cognitive Mesh AgencyLayer
        │  MultiAgentOrchestrationEngine (ethics-checked)
        │  DurableWorkflowEngine (checkpoint/replay)
        │  MAKER benchmark, ConclAIve reasoning recipes
        ▼
   Production cognition serving end users
```

What this combination uniquely provides:

1. **A single spec language across the dev loop and the runtime.** Retort YAML defines team
   scopes; Cognitive Mesh ports/adapters define the production agent contracts. The same
   "team-quality" or "team-security" concept can exist in both.
2. **Hand-off without rewrite.** A workflow drafted in Codeflow-Engine for PR review can graduate
   to a `DurableWorkflowEngine` job in Cognitive Mesh once it needs replay, ethical checks, or
   crash recovery.
3. **Governance you can audit.** Retort drift detection at L1, AutoGen workflow history at L3,
   MAKER scores + `ReasoningTransparency` at L4 — three independent audit surfaces, not one.
4. **Polyglot reach.** TypeScript at L1, Python at L3, .NET at L4. None of the harness ecosystems
   span all three; most are TS- or Rust-only.

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
   They have no durable workflow story, no ethical-reasoning gate, no five-layer separation.
3. **Hand-maintaining `.claude/`, `.cursor/`, `.windsurf/` in parallel.** This is exactly what
   Retort exists to eliminate. If you have more than two harnesses, the manual cost compounds
   fast.
4. **Forking Codeflow-Engine logic into Cognitive Mesh prematurely.** Keep prototypes in Python
   until they earn the .NET rewrite via the Shape C graduation path.
5. **Letting harness slash commands and Retort team definitions drift.** If you adopt Retort,
   make `retort sync --diff` a required check, not a nice-to-have.

## SWOT — PhoenixVC Ecosystem (Cognitive Mesh + Retort + Codeflow-Engine, combined)

### Strengths

- **Full-stack coverage** from L1 (spec/governance) to L4 (production cognition). No single
  competing project spans all four layers; the closest competitors are L2-only.
- **Polyglot reach by design.** TypeScript at L1 (Retort), Python at L3 (Codeflow-Engine), .NET at
  L4 (Cognitive Mesh). Each language is the *right* one for its layer rather than a convenience
  pick.
- **Audit-grade runtime semantics.** `DurableWorkflowEngine` checkpointing, ethical-reasoning
  gates (Brandom + Floridi), MAKER benchmark, and `ReasoningTransparency` give three independent
  audit surfaces — none of the harness-only stacks can produce equivalent forensic artifacts.
- **Hexagonal architecture enforcement.** Layer dependencies are codified
  (Foundation ← Reasoning ← Metacognitive ← Agency ← Business) and `TreatWarningsAsErrors=true`
  forces interface hygiene.
- **Graduation path.** Workflows can be prototyped in Codeflow-Engine and graduated to
  Cognitive Mesh once they earn durability/ethics requirements — without rewriting the spec layer.
- **Spec governance for free.** Once Retort is adopted, drift across `.claude/`, `.cursor/`,
  `.windsurf/`, `.github/copilot-instructions.md`, etc. becomes a CI failure rather than a
  manual chore.
- **First-class team metaphor.** The team-skill files in this repo (`team-foundation`,
  `team-agency`, `team-quality`, etc.) already match Retort's `teams.yaml` shape — the migration
  cost is near-zero.

### Weaknesses

- **High onboarding cost.** Three repos, three runtimes, five layers, hexagonal architecture,
  ports/adapters, MAKER benchmark — a new contributor must absorb a lot before being productive.
  Compared to "install Claw Code, type a prompt," the activation energy is an order of magnitude
  higher.
- **No L2 story of its own.** PhoenixVC has no native developer harness; it relies on
  Claude Code / Claw Code / oh-my-* to provide the IDE surface. Any harness improvement happens
  outside the org's control.
- **Spec/runtime alignment is not yet automatic.** Retort spec teams and Cognitive Mesh ports
  are conceptually parallel but not generated from each other. Drift between them is possible.
- **Codeflow-Engine maturity gaps.** AutoGen is imported behind `try/except` and isn't declared
  in base dependencies; max-concurrent enforcement is unverified; history retention is in-process
  only. (See [`codeflow-engine.md`](../02-internal-repos/codeflow-engine.md).)
- **No hash-anchored editing, no LSP integration, no in-editor multi-provider routing.** OMO and
  similar L2 harnesses meaningfully out-execute the PhoenixVC stack on raw code-modification
  precision.
- **Build pipeline is strict.** `TreatWarningsAsErrors`, CS1591 enforcement, MAKER tests — great
  for governance, painful for fast experiments. Codeflow-Engine in Python is the relief valve,
  but the boundary must be explicit.
- **Documentation surface is large but uneven.** This `docs/orchestration-evaluation/` tree alone
  has 10+ subfolders; finding the "right" doc as a newcomer is non-trivial.

### Opportunities

- **Codify the spec ↔ runtime contract.** Generate Cognitive Mesh agent ports directly from
  `teams.yaml` (or the inverse), so adding a team in one place propagates everywhere. This is
  the "missing middle" between Retort and Cognitive Mesh.
- **Package the MAKER benchmark as a Retort quality gate.** Harness configs that degrade
  reasoning scores fail spec drift. Turns L1 governance into a runtime quality control loop.
- **Expose `DurableWorkflowEngine` checkpoints as a Codeflow-Engine integration target.** Python
  workflows would gain the same checkpoint store as .NET workflows — closing the L3↔L4 boundary.
- **Adopt OMO-style hash-anchored edits in any future internal coding harness.** Borrow what
  the L2 ecosystem has solved well rather than reinventing it.
- **Productize the graduation path.** "Prototype in Codeflow-Engine → graduate to Cognitive Mesh"
  is a real and rare capability. Document it as a first-class workflow, not a footnote.
- **Federation across PhoenixVC-style teams.** Retort already supports task delegation across
  agents; extending that across orgs/repos via A2A/MCP would expand the spec layer without
  changing the runtime.
- **Reference deployment.** Ship a one-command bootstrap (`./scripts/bootstrap-phoenixvc.sh`)
  that wires Retort + a chosen harness + Codeflow-Engine + Cognitive Mesh together. Lowers the
  onboarding cost dramatically.

### Threats

- **L2 harness ecosystem moves fast.** Claw Code reached 175k stars in days. New harness
  primitives (hash-anchored edits, LSP integration, parallel subagents) become baseline
  expectations, and the PhoenixVC stack inherits them only via whichever L2 it integrates with.
- **Vendor harnesses (Claude Code, Cursor, Windsurf, Antigravity) consolidate L1.** If a single
  vendor harness wins enough share, Retort's "16 outputs from one spec" value proposition
  shrinks toward "one output from one spec."
- **Standards displacement.** If A2A or MCP solidify a cross-tool team/skill schema, Retort's
  bespoke YAML may need to migrate or risk becoming a parallel dialect.
- **Talent fragmentation.** Maintaining a TypeScript + Python + .NET stack requires three
  skill sets. Smaller teams will struggle; this favors monoglot competitors.
- **Determinism vs. capability tradeoff.** Strict ethical-reasoning and durable-workflow gates
  add friction. Competing systems (especially L2 harnesses) ship faster *because* they don't
  have those gates. In domains where regulators don't require them, the PhoenixVC stack looks
  expensive.
- **Codeflow-Engine drift risk.** With AutoGen as an optional, undeclared dependency, a future
  AutoGen breaking change could leave Codeflow-Engine in a half-broken state.
- **Documentation drift.** Five-layer architectures with 10+ docs subfolders are notorious for
  silent doc rot. This very file will be obsolete the moment a fourth PhoenixVC repo joins.

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
| **PhoenixVC stack (combined)** | 4.5 | 3.5 | 4.5 | 4.0 | 3.0 | 4.0 | 4.0 | 3.5 |
| Cognitive Mesh (alone) | 4.5 | 4.0 | 4.5 | 3.5 | 3.0 | 4.0 | 4.0 | 3.5 |
| Codeflow-Engine (alone) | 3.0 | 3.0 | 4.0 | 3.0 | 3.0 | 4.0 | 3.0 | 3.0 |
| Retort (alone, L1 only) | 4.0 | 4.0 | 3.0 | 5.0 | n/a | n/a | n/a | n/a |
| Claw Code | 3.5 | 4.0 | 3.0 | 4.5 | 4.5 | 3.0 | 4.0 | 4.0 |
| Claude Code | 3.0 | 4.5 | 3.5 | 5.0 | 4.5 | 3.0 | 4.0 | 4.0 |
| oh-my-agent | 3.0 | 3.5 | 3.0 | 4.5 | 4.0 | 3.0 | 4.0 | 4.0 |
| oh-my-openagent (OMO) | 3.5 | 3.5 | 3.0 | 4.5 | 4.5 | 3.5 | 4.5 | 4.0 |

### Weighted totals (Profile 5: Multi-Agent Reasoning)

| System | Weighted score (1–5) | Percentage |
|--------|:--------------------:|:----------:|
| **Cognitive Mesh (alone)** | **4.00** | **79.9%** |
| **PhoenixVC stack (combined)** | **3.98** | **79.5%** |
| Claude Code | 3.90 | 78.0% |
| Claw Code | 3.77 | 75.4% |
| oh-my-openagent (OMO) | 3.76 | 75.1% |
| oh-my-agent | 3.52 | 70.4% |
| Codeflow-Engine (alone) | 3.24 | 64.8% |
| Retort (alone) | n/a (L1 only — not directly comparable on runtime metrics) | — |

Calculations use the Profile 5 weight vector applied to each row in the per-system table. Cells
marked `n/a` for Retort reflect that L1 spec governance has no runtime latency, scalability,
throughput, or efficiency to score.

### What the weights say

- **Cognitive Mesh alone narrowly outscores the combined PhoenixVC stack on Profile 5** — and
  this is the most informative result in the table. The combined stack has slightly *worse*
  Maintainability (3.5 vs 4.0) because three runtimes are harder to maintain than one, and
  Profile 5 weights Maintainability at 0.18. Combining Retort and Codeflow-Engine doesn't help
  the metrics this profile cares about; their value lives on Integration Ease (only 0.14 here)
  and on the cross-cutting "spec governance" axis Profile 5 doesn't model. **If your workload
  is purely Profile 5, run Cognitive Mesh alone — the combined stack pays for itself only on
  Profile 1 (Interactive) and Profile 4 (Event-Driven), where Retort's 5.0 Integration Ease
  and Codeflow-Engine's event fan-out dominate.**
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
  16 tool outputs from one spec is unmatched.

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

- Should `cognitive-mesh` ship a `.agentkit/spec/` directory natively? See Shape A. The team-skill
  files in this repo (`team-foundation`, `team-reasoning`, etc.) already imply the schema.
- Is there a clean way to expose `DurableWorkflowEngine` checkpoints as a Codeflow-Engine
  integration target, so Python workflows can checkpoint into the same store as .NET workflows?
- Could the MAKER benchmark be packaged as a Retort quality gate, so harnesses fail the spec
  drift check when their generated configs degrade reasoning scores?

## Sources

- [Cognitive Mesh — `src/AgencyLayer/README.md`](../../../src/AgencyLayer/README.md)
- [Cognitive Mesh — `CLAUDE.md`](../../../CLAUDE.md)
- [Codeflow-Engine internal repo writeup](../02-internal-repos/codeflow-engine.md)
- [`docs/orchestration-evaluation/03-external-engines/coding-agent-orchestration/fleet-orchestration.md`](../03-external-engines/coding-agent-orchestration/fleet-orchestration.md)
- [`phoenixvc/retort` README](https://github.com/phoenixvc/retort) — config-sync framework, 16 supported tools
- [`phoenixvc/codeflow-engine`](https://github.com/phoenixvc/codeflow-engine) — Python multi-agent PR/DevOps automation
- [`instructkr/claw-code`](https://github.com/instructkr/claw-code) — Rust clean-room rewrite of Claude Code harness
- [Claw Code launch coverage (24-7 Press Release)](https://www.24-7pressrelease.com/press-release/533389/claw-code-launches-open-source-ai-coding-agent-framework-with-72000-github-stars-in-first-days)
- [`first-fluke/oh-my-agent`](https://github.com/first-fluke/oh-my-agent) — `.agents/`-based portable multi-agent harness
- [`code-yeongyu/oh-my-openagent`](https://github.com/code-yeongyu/oh-my-openagent) — OpenCode plugin, hash-anchored edits, LSP integration
- [`can1357/oh-my-pi`](https://github.com/can1357/oh-my-pi) — terminal AI agent with parallel execution
- [Inside Claude Code architecture deep-dive](https://www.penligent.ai/hackinglabs/inside-claude-code-the-architecture-behind-tools-memory-hooks-and-mcp/)
