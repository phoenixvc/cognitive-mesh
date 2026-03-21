# cognitive-mesh

> **Version:** 0.0.1 &nbsp;|&nbsp; **Status:** Active &nbsp;|&nbsp; **Stack:** .NET 10 · ASP.NET Core · Next.js 15 · Azure · Kubernetes
>
> Enterprise-grade multi-layer AI platform — structured reasoning, multi-agent orchestration, zero-trust security, knowledge management (RAG), plugin-based UI, and full NIST RMF compliance in a single .NET 10 + Next.js solution.
>
> ---
>
> ## What it actually is
>
> cognitive-mesh is far more than an API or an agent runner. It is a **complete enterprise AI platform** structured as seven interdependent layers, each with its own domain logic, ports/adapters, and team of contributing agents. Think of it as the full-stack operating system for intelligent, governed AI workloads — from raw infrastructure (data stores, secrets, audit logs) all the way up to a user-facing plugin dashboard that lets end-users compose their own AI-powered views.
>
> It is the most complex and far-reaching repo in the phoenixvc org, and the platform that all other services ultimately serve.
>
> ---
>
> ## The Seven Layers
>
> ### 1. Foundation Layer — Infrastructure bedrock
>
> Everything the upper layers depend on, all behind ports so implementations are swappable.
>
> - **Zero-Trust Security**: JWT auth, RBAC/ABAC policy enforcement, secrets management (Azure Key Vault), audit logging
> - - **Data persistence**: Azure Cosmos DB adapter, Azure Blob Storage adapter, Vector Database abstraction (Azure AI Search / Redis)
>   - - **Knowledge & RAG**: Semantic search via `EnhancedRAGSystem`, Knowledge Graph, Document Processing / ingestion pipeline
>     - - **Enterprise integration**: Microsoft Fabric OneLake adapter, Feature Flag manager, Enterprise Connectors
>      
>       - ### 2. Reasoning Layer — The cognitive core
>      
>       - Modular reasoning engines, each exposed via a typed port interface. Higher layers call these engines without knowing their implementation.
>      
>       - - **StructuredReasoning (ConclAIve)**: Converts raw LLM outputs into auditable structured reasoning via Debate & Vote, Sequential Reasoning, and Strategic Simulation recipes
> - **AnalyticalReasoning**: Data-driven analysis, trend identification, structured insight generation
> - - **SecurityReasoning**: Threat intelligence engine, anomaly detection, dynamic risk scoring
>   - - **EthicalReasoning**: Normative agency (Brandom), information ethics (Floridi) — validates actions against ethical frameworks
>     - - **CreativeReasoning / CriticalReasoning**: Idea generation, logical consistency evaluation, bias detection
>       - - **DomainSpecificReasoning**: Pluggable industry-specific logic (finance, healthcare, etc.)
>         - - **SystemsReasoning**: Complex system analysis — feedback loops, interdependencies, leverage points
>          
>           - ### 3. Metacognitive Layer — Self-awareness and oversight
>          
>           - The layer that monitors the platform's own cognitive processes.
>          
>           - - **SecurityMonitoring**: Real-time security event aggregation, threat correlation, automated incident response
> - **ContinuousLearning**: Feedback loop — collects operational outcomes, generates learning insights, adapts system behaviour
> - - **PerformanceMonitoring**: KPI tracking (latency, throughput, resource utilisation) across all cognitive tasks
>   - - **SelfEvaluation**: `MetacognitiveOversightComponent` — quality, accuracy, and ethical alignment auditing of reasoning outputs
>     - - **ReasoningTransparency**: Generates explanations and justifications for AI decisions (explainability layer)
>       - - **CulturalAdaptation**: Applies Hofstede's cultural dimensions to tailor agent interactions for global deployments
>         - - **Protocols**: Manages ACP (AI Communication Protocol) and MCP (Metacognitive Protocol) for reliable inter-component communication
>          
>           - ### 4. Agency Layer — Autonomous execution
>          
>           - Where cognitive plans become real actions.
>          
>           - - **MultiAgentOrchestration**: Core engine for coordinating teams of specialised agents — task decomposition, agent selection, workflow execution, result synthesis
> - **ToolIntegration**: Extensible tool framework (web search, data analysis, code execution, classification) — new tools added as adapters without changing agent logic
> - - **SecurityAgents**: Automated incident response agents — immediate containment and remediation triggered by MetacognitiveLayer
>   - - **ActionPlanning**: Multi-step plan generation for complex goal achievement
>     - - **DecisionExecution**: Step-by-step plan execution with tool calls and state management
>       - - **HumanCollaboration**: Human-in-the-loop infrastructure — agents pause for human review, approval, or intervention
>         - - **ConvenerAgents**: Coordination agents that facilitate structured multi-party deliberation
>          
>           - ### 5. Business Applications Layer — The public API surface
>          
>           - The "front door" — all external clients (web, mobile, enterprise services) enter here.
>          
>           - | Application | What it exposes |
> |---|---|
> | Security | Zero-trust auth, risk scoring, compliance reporting |
> | Customer Intelligence | Inquiry handling, conversation management, troubleshooting |
> | Decision Support | Situation analysis, options generation, causal modelling |
> | Knowledge Management | Document ingestion, knowledge base querying |
> | Process Automation | Complex business process automation via agent workflows |
> | Research & Analysis | Automated research, document synthesis, content generation |
> | NIST Compliance | NIST RMF evidence collection, maturity assessment |
> | Value Generation | Business value tracking and reporting |
>
> ### 6. Metacognitive Layer integrations — AI Governance & Community
>
> - **AIGovernance**: Policy-as-code for AI operations, compliance dashboards
> - - **CommunityPulse**: Aggregates team/community signals for adaptive management
>   - - **LearningCatalyst**: Active learning triggers based on system performance patterns
>     - - **UncertaintyQuantification**: Surfaces confidence and uncertainty in AI outputs
>      
>       - ### 7. UI Layer — Plugin-based dashboard framework
>      
>       - A full-stack dashboard system (Next.js 15 frontend + .NET BFF), not a simple admin panel.
>      
>       - - **Widget marketplace**: All UI functionality is a self-contained widget submitted through a governed review process (security scan → code signing → admin approval → registration)
> - **Personalised dashboards**: Users compose their own layout from approved widgets; `DashboardLayoutService` persists per-user configurations
> - - **PluginOrchestrator**: "Sandwich pattern" gateway — every widget call is wrapped with auth, validation, and audit logging before reaching inner layers
>   - - **AgencyWidgets**: Pre-built widgets for core mesh capabilities (Adaptive Balance, NIST RMF, agent status, etc.)
>     - - **Next.js frontend** (`src/UILayer/web`): Next.js 15 + React 19, Tailwind CSS, shadcn/ui, Storybook 8, D3 visualisations, i18n (en-US, fr-FR, de-DE), WCAG 2.1 AA accessibility, offline service worker
>      
>       - ---
>
> ## Architecture
>
> ```
> External clients (browsers, mobile, enterprise services)
>          |
>          v
>    UI Layer (Next.js 15 + .NET BFF)
>          |
>          v
> Business Applications Layer   (REST API — the platform's public surface)
>     /          |          \
> AgencyLayer  MetacognitiveLayer  ReasoningLayer
>     \          |          /
>          FoundationLayer
>     (security, data, RAG, audit, secrets)
>          |
>          v
>    ai-flume gateway      (all LLM calls route through here)
>          |
>          v
>    Azure OpenAI / model backends
> ```
>
> All layers follow **Hexagonal (Ports and Adapters)** architecture: inner layers define typed ports (interfaces), outer layers provide adapters (implementations). This means every infrastructure dependency is swappable without touching business logic.
>
> ---
>
> ## Repository layout
>
> ```
> cognitive-mesh/
> ├── src/
> │   ├── FoundationLayer/       # Infra: security, data stores, RAG, audit
> │   ├── ReasoningLayer/        # Cognitive engines: analytical, ethical, security, creative
> │   ├── MetacognitiveLayer/    # Self-monitoring, learning, incident response
> │   ├── AgencyLayer/           # Multi-agent orchestration, tools, automation
> │   ├── BusinessApplications/  # REST API surface — the platform's front door
> │   ├── UILayer/               # Plugin dashboard (Next.js + .NET BFF)
> │   │   └── web/               # Next.js 15 frontend application
> │   ├── MeshSimRuntime/        # Simulation runtime for testing mesh behaviour
> │   └── ApiHost/               # ASP.NET Core hosting entry point
> ├── api/                       # OpenAPI specs (convener-api.yaml)
> ├── tests/                     # Unit + integration tests
> ├── cypress/                   # E2E tests (cypress.config.ts)
> ├── k8s/                       # Kubernetes manifests
> ├── infra/                     # Infrastructure as code
> ├── docs/                      # Architecture docs, runbooks, API versioning
> ├── examples/                  # Usage examples per layer
> ├── .claude/                   # Claude agent configuration (commands, hooks, rules)
> ├── AGENT_TEAMS.md             # Agent team structure and responsibilities
> ├── CognitiveMesh.sln          # .NET solution
> ├── Directory.Build.props      # Shared build properties (version, targets)
> └── docker-compose.yml         # Local development stack
> ```
>
> ---
>
> ## Prerequisites
>
> - .NET 10 SDK
> - - Node.js 20+ / npm (for UILayer/web)
>   - - Docker (for local containerised run)
>     - - Azure CLI (for cloud deployment)
>       - - Access to ai-flume gateway endpoint
>        
>         - ---
>
> ## Quick start
>
> ```bash
> # Build the .NET solution
> dotnet build CognitiveMesh.sln
>
> # Run tests
> dotnet test
>
> # Run the API host locally
> dotnet run --project src/ApiHost
>
> # Run the UI frontend
> cd src/UILayer/web
> npm install
> npm run dev          # http://localhost:3000
> npm run storybook    # http://localhost:6006 (component docs)
> ```
>
> ---
>
> ## Ecosystem
>
> cognitive-mesh is the intelligence and governance core of the phoenixvc platform. Every other service either feeds it data, routes work through it, or consumes its outputs.
>
> | Repo | Role |
> |---|---|
> | **ai-flume** | AI data plane — all LLM calls from cognitive-mesh route through ai-flume for routing, observability, and cost attribution |
> | **ai-gauge** | Cost observability — attributes cognitive-mesh model spend via ai-flume state service; OTEL spans join in ADX |
> | **ai-cadence** | Project tracker — proxies task-routing decisions to cognitive-mesh for AI-assisted triage and planning |
> | **cockpit** | Desktop ops tool — invokes cognitive-mesh agent teams for complex multi-step operations; surfaces agent status |
> | **retort** | Agent scaffold — retort-based projects register their agent configs into the mesh; retort generates the .agentkit/ overlay |
> | **org-meta** | Org intelligence — cognitive-mesh reads org-meta MCP context at session start; org-meta documents cognitive-mesh's cross-repo contracts |
>
> ### External inspiration
>
> - [mcowger/plexus](https://github.com/mcowger/plexus) — unified AI provider gateway (study reference for ai-flume integration patterns; **note: no licence file — study only, do not copy code**)
> - - [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel) — .NET AI orchestration patterns
>   - - [microsoft/autogen](https://github.com/microsoft/autogen) — multi-agent conversation patterns
>    
>     - ---
>
> ## Name
>
> **cognitive-mesh** — a mesh network has no single point of failure; every node connects to every other. A cognitive mesh applies the same topology to AI: no single agent is the bottleneck, reasoning is distributed across specialised nodes, and the network routes around failures. The name also deliberately avoids vendor lock-in — it describes the architecture, not the model or provider underneath it.
>
> ---
>
> ## Version
>
> This project has not yet had a public release. Current development version: `0.0.1`.
> 
