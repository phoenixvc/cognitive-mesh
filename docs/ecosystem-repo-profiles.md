# Ecosystem Repository Profiles

> Last updated: 2026-03-09

Detailed per-repo profiles for all phoenixvc and JustAGhosT repositories. Each profile covers tech stack, current AI patterns, Cognitive Mesh layer opportunities, AI orchestration assessment, and agent team mapping.

For priority scoring and decision matrix, see [ecosystem-repos-and-issues.md](ecosystem-repos-and-issues.md).

---

## phoenixvc Repos

### cognitive-mesh
**Marketing Name**: Cognitive Mesh | **Score**: N/A (Core Platform) | **Tier**: N/A
**Tech Stack**: .NET 9, C#, Azure OpenAI, CosmosDB, Redis, DuckDB, Qdrant

Enterprise agent/LLM platform with five-layer hexagonal architecture (Foundation, Reasoning, Metacognitive, Agency, BusinessApplications). RBAC, audit, policy-as-code governance. Azure OpenAI and RAG ready.

**Current AI**: ConclAIve reasoning engine (Debate, Sequential, StrategicSimulation), ethical reasoning (Brandom + Floridi), multi-agent orchestration, durable workflows, hybrid memory (Redis + DuckDB), MAKER benchmark.

**Role in Ecosystem**: Central platform that all other repos integrate with for AI reasoning, orchestration, and governance.

---

### ai-gateway
**Marketing Name**: AI Gateway | **Score**: 4.8 | **Tier**: 1a (Infrastructure)
**Tech Stack**: .NET, Azure OpenAI, OpenAI-compatible endpoints
**CM Ticket**: #165 | **Distributed Issues**: ai-gateway #56-#61

Normalizes AI model access to OpenAI-compatible endpoints. Handles model routing, rate limiting, and provider failover.

**Current AI**: Model routing, provider abstraction, rate limiting.

**CM Layer Opportunities**: Foundation (circuit breaker health monitoring), Reasoning (route selection based on prompt), Metacognitive (usage pattern learning).

**AI Orchestration**: Keep in-repo — ai-gateway IS the shared routing layer. All repos should route through it.

---

### pvc-costops-analytics
**Marketing Name**: CostOps | **Score**: 4.1 | **Tier**: 1a (Infrastructure)
**Tech Stack**: Azure, cost analysis

PhoenixVC cost analysis and operations. Tracks Azure spending across all projects.

**Current AI**: None — high potential.

**CM Layer Opportunities**: Reasoning (cost optimization, anomaly detection), Metacognitive (spending pattern learning), Agency (automated cost alerts and remediation).

**AI Orchestration**: Build against CM directly.

---

### phoenixvc-actions-runner
**Marketing Name**: Actions Runner | **Score**: 2.8 | **Tier**: 1a (Infrastructure)
**Tech Stack**: Azure VMSS, GitHub Actions, Key Vault

Manages ephemeral GitHub Actions runners for phoenixvc org. Scales VMSS based on workflow demand.

**Current AI**: None.

**CM Layer Opportunities**: Agency (runner scaling optimization), Metacognitive (workflow demand pattern learning).

**AI Orchestration**: Low priority. Infrastructure enabler for CI/CD.

---

### azure-infrastructure
**Score**: 2.9 | **Tier**: 1a (Infrastructure)
**Tech Stack**: Terraform, Azure, IaC modules
**CM Ticket**: #164 | **Distributed Issues**: azure-infrastructure #13-#18

Unified Azure infrastructure standards, modules, and tooling for nl, pvc, tws, mys.

**Current AI**: None.

**CM Layer Opportunities**: Reasoning (infrastructure planning, drift analysis), Agency (automated remediation).

---

### Mystira.workspace
**Marketing Name**: Mystira Workspace | **Score**: 1.5 | **Tier**: 1b (Traction)
**Tech Stack**: VS Code workspace, docs, tooling, story generation (merged StoryGenerator)
**CM Ticket**: #167 | **Distributed Issues**: Mystira.workspace #718-#719

Unified workspace for Mystira multi-repo development. Includes merged StoryGenerator (AI narrative generation).

**Current AI**: Story generation AI (from merged StoryGenerator).

**CM Layer Opportunities**: Reasoning (narrative quality via debate engine), Metacognitive (story pattern memory), Agency (multi-step generation workflows).

**AI Orchestration**: Route story generation through ai-gateway. Use CM ReasoningLayer for narrative quality.

---

### PhoenixRooivalk
**Marketing Name**: SkySnare / AeroNet | **Score**: 4.0 | **Tier**: 1b (Traction)
**Tech Stack**: Hardware/software, edge AI, sensors, communications
**CM Ticket**: #297 | **Distributed Issues**: PhoenixRooivalk #688-#690, #698

Counter-UAS (drone defense) system. 685+ issues spanning phased development. 27 defined agents. Edge AI for real-time processing.

**Current AI**: 27 agents across detection, tracking, classification, response. Edge AI for sensors.

**CM Layer Opportunities**:
- Reasoning: Threat classification via DebateReasoningEngine (multi-sensor consensus), StrategicSimulation for threat scenarios
- Agency: MultiAgentOrchestrationEngine for 27-agent coordination, DurableWorkflowEngine for response
- Metacognitive: Threat pattern memory, false positive learning
- BusinessApplications: Ethical reasoning (Brandom + Floridi) for use-of-force decisions

**AI Orchestration**: Keep edge AI in-repo (latency critical). Migrate strategic reasoning and multi-agent coordination to CM. Route model calls through ai-gateway.

---

### chaufher
**Marketing Name**: ChaufHER | **Score**: 3.5 | **Tier**: 1b (Traction)
**Tech Stack**: Ride-hailing platform
**CM Ticket**: #301 | **Distributed Issues**: chaufher #102, #103

Ride-hailing platform with safety-focused features. 30 defined agents (not yet implemented).

**Current AI**: 30 agents defined but not implemented.

**CM Layer Opportunities**:
- Reasoning: Route optimization, safety assessment
- Agency: 30-agent orchestration, emergency response workflows via DurableWorkflowEngine
- BusinessApplications: Ethical reasoning for passenger safety (Brandom + Floridi)

**AI Orchestration**: Build against CM directly. Safety-critical reasoning requires CM ethical framework.

---

### PhoenixVC-Website
**Marketing Name**: PhoenixVC | **Score**: 3.2 | **Tier**: 3 (Backlog)
**CM Ticket**: #168 | **Distributed Issues**: PhoenixVC-Website #193-#194

Modernized company website. Low AI priority.

---

### Phoenix.MarketDataPlatform
**Marketing Name**: MarketPulse | **Score**: 3.1 | **Tier**: 3 (Backlog)

Market data platform. Stale development. Could share financial reasoning with VeritasVault.

---

## JustAGhosT Repos

### agentkit-forge
**Marketing Name**: AgentKit Forge | **Score**: 4.9 | **Tier**: 1a (Infrastructure)
**Tech Stack**: Windows-first polyglot AI orchestration, governance templates
**CM Ticket**: #289 | **Distributed Issues**: agentkit-forge #339-#341

Governance/template engine consumed by other repos. Drift detection, hook generation, CI/CD templates.

**Current AI**: AI orchestration templates, agent config distribution.

**CM Layer Opportunities**: Agency (agent config distribution), Metacognitive (drift detection learning).

**AI Orchestration**: Complementary to CM — agentkit-forge distributes, CM reasons.

---

### codeflow-engine
**Marketing Name**: CodeFlow | **Score**: 4.7 | **Tier**: 1a (Infrastructure)
**Tech Stack**: Workflow AI engine
**CM Ticket**: #290 | **Distributed Issues**: codeflow-engine #16-#17

Workflow AI for code generation and transformation.

**CM Layer Opportunities**: Agency (assess overlap with DurableWorkflowEngine), Reasoning (code analysis via ConclAIve).

---

### whatssummarize
**Marketing Name**: ConvoLens | **Score**: 4.6 | **Tier**: 1c (Promising)
**Tech Stack**: Azure OpenAI, OpenAI, Anthropic
**CM Ticket**: #291

Multi-provider conversation summarization. Active multi-provider LLM integration.

**Current AI**: Direct Azure OpenAI + OpenAI + Anthropic calls for summarization.

**CM Layer Opportunities**: Foundation (provider routing via ai-gateway), Reasoning (summarization quality), Metacognitive (caching, pattern learning).

**AI Orchestration**: Migrate provider routing to ai-gateway. Keep prompts in-repo. Shares summarization patterns with OmniPost.

---

### FlairForge
**Marketing Name**: FlairForge | **Score**: 4.5 | **Tier**: 1d (AI Gap Fill)
**CM Ticket**: #292 | **Distributed Issue**: FlairForge #12

AI flyer/content generator. References CM in README but not implemented.

**AI Orchestration**: Build against CM directly. Greenfield integration candidate.

---

### AllieDigital
**Marketing Name**: AllieDigital | **Score**: 4.4 | **Tier**: 3 (Backlog — deferred)
**CM Ticket**: #293 | **Distributed Issue**: AllieDigital #4

Educational platform for neurodivergent learners with neural visualizations and adaptive UI.

**CM Layer Opportunities**: MetacognitiveLayer (HybridMemoryStore for learner profiles, ContinuousLearning), Reasoning (difficulty adjustment).

---

### GeoResourceAIExplorer
**Marketing Name**: ProspectAI | **Score**: 4.3 | **Tier**: 1d (AI Gap Fill)
**CM Ticket**: #294 | **Distributed Issue**: GeoResourceAIExplorer #4

AI resource exploration platform. AI chat claimed but not built.

**CM Layer Opportunities**: Reasoning (StrategicSimulation for geological scenarios), Foundation (Qdrant for geological embeddings), Agency (ActionPlanner for query decomposition).

---

### content_creation
**Marketing Name**: OmniPost | **Score**: 4.2 | **Tier**: 1c (Promising)
**Tech Stack**: OpenAI, Azure OpenAI, multi-platform publishing
**CM Ticket**: #295 | **Distributed Issue**: content_creation #109

AI-powered multi-platform content publisher. Active AI for summarization, parsing, image generation.

**Current AI**: Direct OpenAI + Azure OpenAI calls.

**AI Orchestration**: Migrate provider routing to ai-gateway. Shares summarization patterns with ConvoLens — evaluate shared service.

---

### HouseOfVeritas
**Marketing Name**: House of Veritas | **Score**: 4.1 | **Tier**: 1c (Promising)
**Tech Stack**: Azure Document Intelligence, Inngest, 11 agents
**CM Ticket**: #296 (also #276) | **Distributed Issues**: HouseOfVeritas #42-#43, #44

Operational SaaS for estate/asset management. Deployed with 30+ PRs.

**Current AI**: Azure Document Intelligence, 11 agents, Inngest workflows.

**CM Layer Opportunities**:
- Agency: Migrate Inngest → CM DurableWorkflowEngine, 11-agent coordination
- Reasoning: Document analysis, property valuation
- Foundation: Route Azure Doc Intel through ai-gateway

**Note**: Separate from HouseFix (FixForge). Different domains: estate mgmt SaaS vs DIY guides.

---

### zeeplan
**Marketing Name**: ZeePlan | **Score**: 3.8 | **Tier**: 2 (Next Quarter)
**CM Ticket**: #298 | **Distributed Issue**: zeeplan #54

Farm operations planning for Zeerust Farm. No current AI.

**CM Layer Opportunities**: Reasoning (StrategicSimulation for farm scenarios), Metacognitive (seasonal pattern learning), Agency (automated planning workflows).

---

### crisis-unleashed-app
**Marketing Name**: Crisis Unleashed | **Score**: 3.7 | **Tier**: 2 (Next Quarter)
**Tech Stack**: React + FastAPI
**CM Ticket**: #299 | **Distributed Issue**: crisis-unleashed-app #30

Full-stack blockchain game. No current AI.

**CM Layer Opportunities**: Agency (multi-agent game orchestration), Reasoning (NPC decisions via DebateReasoningEngine), Metacognitive (player behavior learning).

---

### vv
**Marketing Name**: VeritasVault | **Score**: 3.6 | **Tier**: 2 (Next Quarter)
**Tech Stack**: .NET 9, Clean Architecture
**CM Ticket**: #300 | **Distributed Issue**: vv #23

Financial platform. Base for monorepo consolidation (absorbing vv-landing and vv-docs per CM #306).

**CM Layer Opportunities**: Reasoning (market analysis, risk assessment), BusinessApplications (regulatory compliance), Metacognitive (market pattern memory).

---

### movie-list-by-mood
**Marketing Name**: MoodReel | **Score**: 3.5 | **Tier**: 2 (Next Quarter)

Mood-based movie recommendations. Medium AI potential.

---

### home-lab-setup
**Marketing Name**: InfraLab | **Score**: 3.0 | **Tier**: 3 (Backlog)
**Tech Stack**: Azure, PowerShell
**CM Ticket**: #302 | **Distributed Issue**: home-lab-setup #45

Azure homelab automation. AI claimed but not built. Shares cost optimization patterns with CostOps.

---

### HouseFix
**Marketing Name**: FixForge | **Score**: 2.8 | **Tier**: 3 (Backlog)

DIY home repair/modernization guides. Scaffold only (1 commit). Separate from HouseOfVeritas.

**Future synergy**: HouseOfVeritas marketplace could surface FixForge guides as content.

---

### twinesandstraps
**Marketing Name**: Twines & Straps | **Score**: 2.5 | **Tier**: 3 (Backlog)
**CM Ticket**: #303 | **Distributed Issue**: twinesandstraps #98

E-commerce platform (97 PRs, WhatsApp integration, Azure deployment, Claude code analysis branch).

**CM Layer Opportunities**: Reasoning (product recommendations), Agency (WhatsApp chatbot workflows), Metacognitive (customer behavior learning).

---

### vectorforge
**Marketing Name**: VectorForge | **Score**: 3.2 | **Tier**: 3 (Backlog)

Vector processing tools. May share vector DB patterns with CM FoundationLayer Qdrant adapter.

---

### PuffWise
**Marketing Name**: PuffWise | **Score**: 1.4 | **Tier**: Excluded

No description. Low activity. Not a candidate for CM integration.

---

## Archived / Excluded

| Repo | Reason |
|------|--------|
| **codeflow-desktop** | Archived (CM #304) |
| **vv-landing** | Merging into vv monorepo (CM #306) |
| **vv-docs** | Merging into vv monorepo (CM #306) |
| **Mystira.StoryGenerator** | Merged into Mystira.workspace |

---

## AI Consolidation Summary

Based on the AI Consolidation Analysis (CM #309):

| Layer | Responsibility | Repos Affected |
|-------|---------------|----------------|
| **ai-gateway** | Provider routing, rate limiting, cost tracking, provider failover | ConvoLens, OmniPost, vv-landing, all future AI repos |
| **cognitive-mesh** | Reasoning engines, multi-agent orchestration, memory, ethics | All Tier 1-2 repos |
| **Individual repos** | Domain-specific prompts, UX, data handling | All repos |

### Shared Patterns Identified

- **Summarization**: ConvoLens + OmniPost share summarization patterns — evaluate shared service via ai-gateway
- **Agent configs**: PhoenixRooivalk (27) + ChaufHER (30) + HouseOfVeritas (11) — standardize via agentkit-forge
- **Financial reasoning**: VeritasVault + MarketPulse — shared CM ReasoningLayer recipes
- **Cost optimization**: CostOps + InfraLab — shared cost reasoning patterns
