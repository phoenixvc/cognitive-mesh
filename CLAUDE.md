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

## Operational Practices

### 1. Separate Architecture from Implementation

Never mix strategic design decisions and code implementation in the same conversation. Use the orchestrator agent (`/orchestrate`) for architecture and planning, then delegate to team agents (`/team-foundation`, `/team-agency`, etc.) for implementation. Architectural decisions made during coding get lost when context shortens.

### 2. Verify What Actually Ran

The interface can show one thing while something else happened underneath. After any significant operation:

- Check `git log` to confirm commits actually landed
- Check `git diff` to confirm the change matches intent
- Check build output for silent warnings that got swallowed
- If using multiple models, verify which model produced which output

### 3. Debug by Asking the System to Explain Itself

When something doesn't work, don't ask "why doesn't this work?" Instead:

- Ask the agent to trace execution step by step
- Ask it to explain what each part of the code is doing
- Ask it to list its assumptions
- Unnecessary complexity becomes visible when forced to narrate

### 4. No Irreversible Actions Without Confirmation

The `protect-sensitive.sh` hook blocks writes to secrets and credentials. Additionally:

- The following destructive commands are blocked by `guard-destructive-bash.sh` (hook) and `settings.json` (deny list):
  - `git push --force` / `git push -f` (force-push)
  - `git reset --hard`
  - `git clean -f` / `git clean -df` / `git clean -fd`
  - `git checkout -- .` / `git checkout .`
  - `git restore .`
  - `git branch -D` (force-delete branch)
- `rm -rf` beyond the project root is blocked by both `guard-destructive-bash.sh` and `settings.json`
- Package publishing (`dotnet nuget push`) is denied in `settings.json`
- If an operation cannot be undone, stop and ask for confirmation

### 5. Integrations Fail Quietly — Validate Them

"Connected" does not mean "working." For every integration point, verify:

- **What can the agent see?** (file access, API access, tool availability)
- **What can't it see?** (secrets, external services, runtime state)
- **What is it allowed to do?** (check `settings.json` permissions)
- **How is it tested?** (unit tests, integration tests, manual verification)
- **What counts as failure?** (silent errors are the most dangerous)

### 6. Plan Before Coding

Never jump straight to implementation. The sequence is:

1. Understand the project context (`/discover` for new codebases)
2. Define the plan and identify risks (`/orchestrate` or TodoWrite)
3. Document architectural decisions in files (not just in chat)
4. Only then start building

Speeding up the wrong decision just gets you to the problem faster.

### 7. Persist Decisions to Files, Not Chat

Chat context gets summarized and shortened. Important decisions must be saved to durable storage:

- Architectural decisions → CLAUDE.md or dedicated ADR files
- Task state → TodoWrite tool or `.claude/state/` files
- Integration results → commit to the repo
- One orchestrator chat delegates; results return as files, not conversation fragments

### 8. Commit Early and Often

The model's context is not version control. If a working solution exists only in chat, it can be lost on the next context window shift.

- Commit after each meaningful change, not at the end of a session
- Don't keep important code only in conversation
- Use descriptive commit messages that capture the "why"
- The `stop-build-check.sh` hook verifies the build on every response — treat build failures as blockers
