# PhoenixVC / JustAGhosT Ecosystem - Repos & Open Issues

> Snapshot taken 2026-03-09

For detailed per-repo profiles, see [ecosystem-repo-profiles.md](ecosystem-repo-profiles.md).

---

## Decision Matrix Methodology

AI integration potential is scored using a weighted decision matrix:

- **AI Usage (30%)**: Current AI/LLM usage in the repo (0-5 scale)
- **Business Impact (25%)**: Potential business value from Cognitive Mesh integration (0-5 scale)
- **Integration Complexity (20%)**: Technical difficulty of adding Cognitive Mesh (0-5 scale, inverted - lower complexity = higher score)
- **CM Usage (15%)**: How well the repo aligns with Cognitive Mesh capabilities (0-5 scale)
- **Activity (10%)**: Recent development activity level (0-5 scale)

**Total Score Range**: 0-5 (higher = higher priority for integration)

---

## phoenixvc (GitHub Org) - Active Repos

| Repo | Marketing Name | Description | Last Pushed | AI Integration Potential | Score | CM Ticket |
|------|---------------|-------------|-------------|--------------------------|-------|-----------|
| **cognitive-mesh** | Cognitive Mesh | Enterprise agent/LLM platform with layered governance (RBAC, audit, policy-as-code); Azure OpenAI and RAG ready. | 2026-03-08 | N/A (Core Platform) | N/A | N/A |
| **ai-gateway** | AI Gateway | Normalizes AI model access to OpenAI-compatible endpoints. Model routing, rate limiting, provider failover. | 2026-03-03 | High | 4.8 | #165 |
| **pvc-costops-analytics** | CostOps | PhoenixVC cost analysis and operations. Azure spending tracking across all projects. | 2026-03-08 | High | 4.1 | — |
| **PhoenixRooivalk** | SkySnare / AeroNet | Counter-UAS (drone defense) system. 27 defined agents, edge AI, phased hardware/software development. 685+ issues. | 2026-03-08 | High | 4.0 | #297 |
| **chaufher** | ChaufHER | Ride-hailing platform with safety-focused features. 30 defined agents for ride matching, safety, routing, pricing. | 2026-03-04 | High | 3.5 | #301 |
| **PhoenixVC-Website** | PhoenixVC | Modernized PhoenixVC company website. | 2026-03-07 | Medium | 3.2 | #168 |
| **Phoenix.MarketDataPlatform** | MarketPulse | Market data platform for financial data processing and analysis. | 2025-05-23 | Medium | 3.1 | — |
| **azure-infrastructure** | — | Unified Azure infrastructure standards, modules, and tooling for nl, pvc, tws, mys. | 2025-12-08 | Medium | 2.9 | #164 |
| **phoenixvc-actions-runner** | Actions Runner | Manages ephemeral GitHub Actions runners for phoenixvc org. Scales VMSS based on workflow demand. | 2026-03-08 | Medium | 2.8 | — |
| **azure-project-template** | — | Template repository for new Azure projects using phoenixvc standards. | 2025-12-07 | Low | 1.8 | — |
| **Mystira.workspace** | Mystira Workspace | Unified workspace for Mystira multi-repo development. Includes merged StoryGenerator (AI story generation). | 2026-03-08 | Medium | 1.5 | #167 |
| **Mystira.Devhub** | — | Mystira developer hub. | 2026-02-20 | Low | 1.3 | — |

### Archived / Merged (phoenixvc)

| Repo | Status | Notes |
|------|--------|-------|
| **Mystira.StoryGenerator** | Merged | Merged into Mystira.workspace |

---

## JustAGhosT (GitHub Org) - Active Repos

| Repo | Marketing Name | Description | Last Pushed | AI Integration Potential | Score | CM Ticket |
|------|---------------|-------------|-------------|--------------------------|-------|-----------|
| **agentkit-forge** | AgentKit Forge | Windows-first, polyglot AI-orchestration template. Governance/template engine for ecosystem repos. | 2026-03-08 | High | 4.9 | #289 |
| **codeflow-engine** | CodeFlow | Workflow AI engine for code generation and transformation. | 2026-03-07 | High | 4.7 | #290 |
| **whatssummarize** | ConvoLens | Multi-provider conversation summarization (Azure OpenAI, OpenAI, Anthropic). | 2026-03-05 | High | 4.6 | #291 |
| **FlairForge** | FlairForge | AI flyer/content generator. References CM in README but not yet implemented. | 2026-01-23 | High | 4.5 | #292 |
| **AllieDigital** | AllieDigital | Educational platform for neurodivergent learners with neural visualizations and adaptive UI. | 2026-02-21 | High | 4.4 | #293 |
| **GeoResourceAIExplorer** | ProspectAI | AI-integrated resource exploration platform. AI chat claimed but not built. | 2025-09-28 | High | 4.3 | #294 |
| **content_creation** | OmniPost | AI-powered multi-platform content publisher. OpenAI + Azure OpenAI for summarization, parsing, image gen. | 2026-01-27 | High | 4.2 | #295 |
| **HouseOfVeritas** | House of Veritas | Operational SaaS for estate/asset management. Azure Doc Intel, 11 agents, Inngest workflows. 30+ PRs. | 2026-03-02 | High | 4.1 | #296 |
| **zeeplan** | ZeePlan | Farm operations planning tool for Zeerust Farm. No current AI. | 2025-12-03 | Medium | 3.8 | #298 |
| **crisis-unleashed-app** | Crisis Unleashed | Modular full-stack app (React + FastAPI) — blockchain game. No current AI. | 2026-02-20 | Medium | 3.7 | #299 |
| **vv** | VeritasVault | .NET 9 Clean Architecture financial platform. Base for monorepo consolidation (absorbing vv-landing, vv-docs). | 2025-12-04 | Medium | 3.6 | #300 |
| **movie-list-by-mood** | MoodReel | Mood-based movie recommendation platform. | 2026-03-02 | Medium | 3.5 | — |
| **vectorforge** | VectorForge | Vector processing tools. | 2026-01-17 | Medium | 3.2 | — |
| **home-lab-setup** | InfraLab | Azure homelab automation (PowerShell). VPN, NAT, DNS, certificates, monitoring, cost guardrails. AI claimed but not built. | 2026-03-02 | Medium | 3.0 | #302 |
| **HouseFix** | FixForge | DIY home repair/modernization guides. Scaffold only (1 commit). Separate from HouseOfVeritas. | 2026-02-24 | Low | 2.8 | — |
| **twinesandstraps** | Twines & Straps | E-commerce platform. 97 PRs, WhatsApp integration, Azure deployment, Claude code analysis branch. | 2025-12-13 | Low | 2.5 | #303 |
| **PuffWise** | PuffWise | *(no description)* | 2026-03-03 | Low | 1.4 | — |
| **ShareX** | — | ShareX fork - free screen capture/recording with upload support. | 2026-02-27 | Low | 1.1 | — |

### Archived / Consolidating (JustAGhosT)

| Repo | Status | Notes |
|------|--------|-------|
| **vv-landing** | Consolidating | Merging into vv monorepo (CM #306) |
| **vv-docs** | Consolidating | Merging into vv monorepo (CM #306) |
| **codeflow-desktop** | Archived | CM #304 |

### Not Scored (Low activity / no AI fit)

pigpro, musicformom, stackcompare, xtox, sygtemp, phoenixvc-dev-api-fastapi, phoenixvc-template-test, .legal, syg, ImageStitch, cheesypork, BelaPlan, direct-impact-platform, farm-business-plan, goat-farming-guide, JustAGhosT (profile), CloneDriverTenThousand, VelvetBridge, velvetv2, v0-startup-tracker, PoultryClub, dfordiamond

---

## Open Issues on Key Repos

### phoenixvc/PhoenixRooivalk (685+ issues, showing top 10)

| # | Title |
|---|-------|
| 685 | feat: Hugging Face integration for model training with MCP |
| 684 | docs: add Phase 5 C-UAS site operational procedures |
| 683 | docs: add Phase 5 security architecture deep-dive |
| 682 | docs: add Phase 5 coalition interoperability spec |
| 681 | docs: add Phase 4 OTA firmware update system spec |
| 680 | docs: add Phase 4 certification and compliance plan |
| 679 | docs: add Phase 4 manufacturing process and quality control spec |
| 678 | docs: add Phase 3 enclosure and mounting design spec |
| 677 | docs: add Phase 3 power systems spec (PoE + Solar) |
| 676 | docs: add Phase 3 communications architecture spec |

### phoenixvc/cognitive-mesh (310+ issues, showing top 10)

| # | Title |
|---|-------|
| 310 | Distribute ecosystem integration issues to ALL target repos |
| 309 | AI Consolidation Analysis — shared AI services extraction |
| 308 | AI orchestration pattern assessment — current state across ecosystem |
| 307 | Map JustAGhosT + phoenixvc projects to agent teams — domain coverage analysis |
| 306 | Consolidate VeritasVault repos (vv, vv-landing, vv-docs) into monorepo |
| 305 | [Epic] Ecosystem Integration — Comprehensive sequenced rollout across all repos |
| 304 | Mark codeflow-desktop as archived (low priority) |
| 303 | Integrate Cognitive Mesh AI capabilities into twinesandstraps (Twines & Straps) |
| 302 | Integrate Cognitive Mesh AI capabilities into home-lab-setup (InfraLab) |
| 301 | Integrate Cognitive Mesh AI capabilities into chaufher (ChaufHER) |

### justaghost/agentkit-forge (337+ issues, showing top 10)

| # | Title |
|---|-------|
| 337 | [FEATURE] Maintain list of consuming repositories (Notion, labels, Linear, etc.) |
| 336 | [FEATURE] Evaluate scripts from other repos (e.g. actions runner) for inclusion |
| 335 | docs(staging): add optional guidance for docs-staging / draft-docs workflow |
| 334 | docs(backlog): include documentation backlog guidance for all repos |
| 333 | chore(governance): audit governance pipeline adoption in downstream repos |
| 332 | chore(drift): test drift detection in adopter repos |
| 331 | chore(branch-protection): audit branch protection patterns in adopter repos |
| 330 | chore(hooks): audit hook generation in adopter repos |
| 329 | chore(templates): analyze implemented repos for CI/CD template generation opportunities |
| 328 | fix(budget-guard): verify and address budget-guard workflow logic |

### justaghost/HouseOfVeritas

| # | Title |
|---|-------|
| 44 | Extend House of Veritas with CM reasoning and orchestration |
| 42 | Cognitive Mesh ecosystem integration |
| 2 | Infrastructure Drift Detected - 12 Failed Check(s) |

---

## Notes

- **arch-context-mcp** was not found in either org listing — may be a planned/private repo or different name.
- **agentkit-forge** is the governance/template engine that other repos consume (drift detection, hook generation, CI/CD templates).
- **PhoenixRooivalk** has the highest issue count (685+) and 27 agents across detection, tracking, classification, and response phases.
- **chaufher** has 30 defined agents for ride-hailing operations.
- **HouseOfVeritas** has 11 agents and uses Inngest for workflow orchestration.
- **cognitive-mesh** issues now span evaluating AI tools and comprehensive ecosystem integration (#288-#310).
- **content_creation** (OmniPost) is an active AI content publisher — NOT archived. Shares summarization patterns with ConvoLens.
- **Mystira.StoryGenerator** has been merged into **Mystira.workspace**.
- Repos with "Dependency Dashboard" issues use Renovate/Dependabot for automated dependency updates.

---

## AI Integration Priority Tiers

Based on the decision matrix scoring and ecosystem epic (#305), repos are organized into priority tiers:

### Tier 1a — Infrastructure (enabling platform)

- **agentkit-forge** / AgentKit Forge (4.9) — Distribution layer, highest strategic value | CM #289
- **pvc-costops-analytics** / CostOps (4.1) — Cost optimization AI | —
- **ai-gateway** / AI Gateway (4.8) — Model routing infrastructure | CM #165
- **codeflow-engine** / CodeFlow (4.7) — Workflow AI engine | CM #290
- **phoenixvc-actions-runner** / Actions Runner (2.8) — Ephemeral GitHub runners, VMSS scaling | —
- **azure-infrastructure** (2.9) — Shared IaC modules | CM #164

### Tier 1b — Traction (projects with momentum and agent teams)

- **Mystira.workspace** (1.5, includes merged StoryGenerator) — Monorepo workspace | CM #167
- **PhoenixRooivalk** / SkySnare-AeroNet (4.0) — 27 agents, edge AI, counter-UAS | CM #297
- **chaufher** / ChaufHER (3.5) — 30 agents, ride-hailing, safety reasoning | CM #301

### Tier 1c — Promising (active AI to consolidate)

- **whatssummarize** / ConvoLens (4.6) — Active multi-provider AI (Azure OpenAI, OpenAI, Anthropic) | CM #291
- **content_creation** / OmniPost (4.2) — Active AI (OpenAI + Azure OpenAI, image gen) | CM #295
- **HouseOfVeritas** / House of Veritas (4.1) — Azure Doc Intel, 11 agents, Inngest | CM #296

### Tier 1d — AI Gap Fill (AI planned but not built)

- **FlairForge** (4.5) — AI flyer generator, refs CM in README but not implemented | CM #292
- **GeoResourceAIExplorer** / ProspectAI (4.3) — AI chat claimed, not built | CM #294

### Tier 2 — Next Quarter

- **zeeplan** / ZeePlan (3.8) — Farm scenario simulation | CM #298
- **crisis-unleashed-app** / Crisis Unleashed (3.7) — Game AI agents | CM #299
- **vv** / VeritasVault (3.6) — Consolidated VV monorepo | CM #300
- **movie-list-by-mood** / MoodReel (3.5) — Mood recommendations | —

### Tier 3 — Backlog

- **PhoenixVC-Website** / PhoenixVC (3.2) | CM #168
- **vectorforge** / VectorForge (3.2) | —
- **Phoenix.MarketDataPlatform** / MarketPulse (3.1) | —
- **home-lab-setup** / InfraLab (3.0) | CM #302
- **HouseFix** / FixForge (2.8) | —
- **twinesandstraps** / Twines & Straps (2.5) | CM #303
- **AllieDigital** (4.4) — Educational AI (deferred by user) | CM #293

### Excluded

- **codeflow-desktop** — Archived (CM #304)
- **vv-landing** — Merging into vv monorepo (CM #306)
- **vv-docs** — Merging into vv monorepo (CM #306)
- **Mystira.StoryGenerator** — Merged into Mystira.workspace

---

## Implementation Plan

1. ~~Create overarching GitHub issue in phoenixvc/cognitive-mesh for ecosystem integration planning~~ → Done: #288, #305 (epic)
2. ~~Create individual implementation tickets for high-priority repos~~ → Done: #289-#303
3. ~~Distribute tailored issues to all target repos~~ → Done: #310
4. Plan VV monorepo consolidation (vv as base, absorb vv-landing + vv-docs) → #306
5. AI consolidation analysis — shared services extraction → #309
6. Agent team mapping across all repos → #307
7. AI orchestration pattern assessment → #308
8. Monitor and update low-score repos for future opportunities

---

## Cross-References

- **Epic**: [#305 - Ecosystem Integration — Comprehensive sequenced rollout](https://github.com/phoenixvc/cognitive-mesh/issues/305)
- **Planning**: [#288 - Ecosystem integration planning](https://github.com/phoenixvc/cognitive-mesh/issues/288)
- **Registry**: [#275 - Integration registry](https://github.com/phoenixvc/cognitive-mesh/issues/275)
- **Master Epic**: [#265 - Master epic](https://github.com/phoenixvc/cognitive-mesh/issues/265)
- **Distribution**: [#310 - Distribute to all repos](https://github.com/phoenixvc/cognitive-mesh/issues/310)
- **Profiles**: [ecosystem-repo-profiles.md](ecosystem-repo-profiles.md)
