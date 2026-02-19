# Cognitive Mesh

## Build & Test

```bash
dotnet build CognitiveMesh.sln
dotnet test CognitiveMesh.sln --no-build
dotnet test tests/AgencyLayer/Orchestration/Orchestration.Tests.csproj  # MAKER benchmark tests
```

Build treats warnings as errors (`TreatWarningsAsErrors=true` in Directory.Build.props). XML doc comments are required on public types (CS1591).

## Architecture

Five-layer hexagonal architecture. Each layer has its own sub-projects with separate `.csproj` files:

- **FoundationLayer** — Persistence (CosmosDB, Blob, DuckDB, Qdrant vector DB), security, notifications, circuit breaker
- **ReasoningLayer** — ConclAIve reasoning engine (Debate, Sequential, StrategicSimulation), ethical reasoning (Brandom + Floridi), threat intel, value generation
- **MetacognitiveLayer** — HybridMemoryStore (Redis + DuckDB), ReasoningTransparency, ContinuousLearning, SessionManager, performance monitoring
- **AgencyLayer** — MultiAgentOrchestrationEngine, DurableWorkflowEngine, checkpointing, MAKER benchmark, ActionPlanner, TaskRouter
- **BusinessApplications** — GDPR/EU AI Act compliance, decision support, customer intelligence

Dependency direction: Foundation ← Reasoning ← Metacognitive ← Agency ← Business. Never create circular references between layers.

## Key Patterns

- **Hexagonal architecture**: Ports (interfaces) in each layer, Adapters implement them. Engines contain pure business logic.
- **Port naming**: `I{Concept}Port` (e.g., `IMultiAgentOrchestrationPort`)
- **Adapter naming**: `{Implementation}Adapter` (e.g., `CosmosDbAdapter`, `InProcessAgentRuntimeAdapter`)
- **Engine naming**: `{Concept}Engine` (e.g., `DurableWorkflowEngine`, `DebateReasoningEngine`)
- **Event-driven**: Wolverine `IMessageBus` for pub/sub in ActionPlanning. MediatR in some sub-projects.
- **Async everywhere**: All public methods are async with `CancellationToken` parameter.

## Conventions

- .NET 9, C# preview, nullable reference types enabled
- PascalCase for public members, `_camelCase` for private fields
- Constructor injection with null guards (`?? throw new ArgumentNullException`)
- `ILogger<T>` for structured logging on all classes
- Test framework: xUnit + Moq + FluentAssertions
- Test naming: `MethodName_Scenario_ExpectedResult`

## Critical Files

- `src/AgencyLayer/Orchestration/Execution/DurableWorkflowEngine.cs` — Core workflow engine with checkpointing, retry, crash recovery
- `src/AgencyLayer/Orchestration/Benchmarks/MakerBenchmark.cs` — MAKER benchmark (Tower of Hanoi)
- `src/AgencyLayer/MultiAgentOrchestration/Engines/MultiAgentOrchestrationEngine.cs` — Multi-agent coordination with ethical checks
- `src/MetacognitiveLayer/Protocols/Common/Memory/HybridMemoryStore.cs` — Dual-write Redis+DuckDB memory
- `src/ReasoningLayer/StructuredReasoning/` — ConclAIve reasoning recipes

## Do Not

- Do not add `using` directives for namespaces already covered by `<ImplicitUsings>enable</ImplicitUsings>`
- Do not reference AgencyLayer from MetacognitiveLayer (circular dependency)
- Do not modify `Directory.Build.props` without explicit request
- Do not store secrets in code — use environment variables or Azure Key Vault references
- Do not suppress CS1591 warnings globally — add XML docs to public types instead
