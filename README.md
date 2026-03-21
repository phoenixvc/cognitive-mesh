# cognitive-mesh

![Version](https://img.shields.io/badge/version-0.0.1-blue) ![Status](https://img.shields.io/badge/status-active-green) ![Platform](https://img.shields.io/badge/platform-.NET%2010-purple)

> Multi-agent orchestration framework for phoenixvc — structured reasoning, role-based agent coordination, and cross-repo AI task routing.
>
> **cognitive-mesh** is the agent intelligence layer of the phoenixvc ecosystem. It provides a .NET 10 framework for coordinating teams of specialised AI agents, routing complex tasks through structured reasoning pipelines, and surfacing results to consuming services. It is the "thinking layer" that sits between raw model calls (via `ai-flume`) and the products that need intelligent, multi-step results.
>
> ---
>
> ## What it does
>
> - **Multi-agent coordination** — Role-based agent teams where each agent has a defined specialisation (planner, executor, reviewer, critic). Complex tasks are decomposed and routed to the right agent.
> - - **Structured reasoning** — Pipeline-based task execution with checkpointing, retry logic, and quality gates.
>   - - **Cross-repo routing** — `ai-cadence` backend proxies task-routing decisions here. `retort`-based projects can register their agents into the mesh.
>     - - **Observability** — All agent calls and reasoning steps are traced via OpenTelemetry, with spans pushed to `ai-flume`'s state service for attribution.
>       - - **Kubernetes-ready** — Containerised, with k8s manifests and Helm chart support.
>        
>         - ---
>
> ## Architecture
>
> ```
> consumers (ai-cadence / cockpit / retort projects)
>          |
>          v
> cognitive-mesh API  (.NET 10 / ASP.NET Core)
>          |
>     -----+------------------
>     |                      |
> agent teams            tools/
> (planner, executor,    (external APIs,
> reviewer, critic)       MCP clients)
>          |
>          v
>    ai-flume gateway  (model calls)
> ```
>
> ---
>
> ## Repository layout
>
> ```
> cognitive-mesh/
> ├── src/                    # .NET source (agents, pipelines, API)
> ├── api/                    # REST API layer
> ├── tests/                  # Unit + integration tests
> ├── cypress/                # E2E tests
> ├── k8s/                    # Kubernetes manifests
> ├── infra/                  # Infrastructure as code
> ├── docs/                   # Architecture and runbooks
> ├── examples/               # Usage examples
> ├── tools/                  # Supporting tools
> ├── scripts/                # Build and deploy scripts
> ├── CognitiveMesh.sln       # .NET solution file
> ├── Directory.Build.props   # Shared build properties
> └── README.md
> ```
>
> ---
>
> ## Prerequisites
>
> - .NET 10 SDK
> - - Docker (for local containerised run)
>   - - Azure CLI (for cloud deployment)
>     - - Access to `ai-flume` gateway endpoint
>      
>       - ---
>
> ## Quick start
>
> ```bash
> # Build
> dotnet build CognitiveMesh.sln
>
> # Test
> dotnet test
>
> # Run locally
> dotnet run --project src/CognitiveMesh.Api
> ```
>
> ---
>
> ## Ecosystem
>
> cognitive-mesh is the agent orchestration layer of the phoenixvc platform. It connects to:
>
> | Repo | Role |
> |---|---|
> | `ai-flume` | AI data plane — all model calls from cognitive-mesh route through ai-flume for observability and attribution |
> | `ai-cadence` | Project tracker — proxies task-routing decisions to cognitive-mesh for AI-assisted triage |
> | `cockpit` | Desktop ops tool — can invoke cognitive-mesh agent teams for complex multi-step operations |
> | `retort` | Agent scaffold — retort-based projects register their agents into cognitive-mesh |
> | `ai-gauge` | Cost observability — attributes cognitive-mesh model spend via ai-flume state service |
>
> ---
>
> ## Name
>
> **cognitive-mesh** — a mesh network has no single point of failure; every node connects to every other node. A cognitive mesh applies the same principle to AI agents: no single agent is the bottleneck, reasoning is distributed across specialised nodes, and the network routes around failures. The name captures both the distributed topology and the nature of the work — cognition, reasoning, intelligence — without being tied to any single framework or provider.
