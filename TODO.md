# TODO: Next Steps

The `DecisionExecutor` has been refactored to use `IDecisionReasoningEngine` for decision logic and `IMediator` for persistence. However, there are pending tasks to fully complete the integration and testing.

## Immediate Tasks
1. **Fix Build Errors in Shared Project:**
   - There are missing XML comments in `CognitiveMesh.Shared` (NodeLabels.cs) causing build failures (warnings treated as errors).
   - Resolve these to ensure a clean build.

2. **Verify Tests:**
   - Once the build is fixed, run `dotnet test tests/AgencyLayer/DecisionExecution/DecisionExecution.Tests.csproj`.
   - Ensure `DecisionExecutorTests` and `ConclAIveReasoningAdapterTests` pass.

3. **Persistence Implementation:**
   - The `DecisionPersistenceHandler` stores the result in the Knowledge Graph.
   - Verify the graph schema/structure aligns with other components (e.g., `NodeLabels`).

4. **Integration with ActionPlanner:**
   - Currently, `DecisionExecutor` just returns a result and persists it.
   - Consider if it should also trigger actions via `ActionPlanner` directly or if another handler should listen to `DecisionMadeNotification` to trigger actions.

5. **Reasoning Recipe Selection:**
   - The `ConclAIveReasoningAdapter` currently defaults to `null` (auto) for recipe type unless specified in metadata.
   - Refine the logic to select recipes based on `DecisionType`.

## Context
- **Modified Files:**
  - `src/AgencyLayer/DecisionExecution/DecisionExecutor.cs`
  - `src/AgencyLayer/DecisionExecution/DecisionExecution.csproj`
  - `src/AgencyLayer/DecisionExecution/IDecisionReasoningEngine.cs` (New)
  - `src/AgencyLayer/DecisionExecution/Adapters/ConclAIveReasoningAdapter.cs` (New)
  - `src/AgencyLayer/DecisionExecution/Events/DecisionMadeNotification.cs` (New)
  - `src/AgencyLayer/DecisionExecution/Handlers/DecisionPersistenceHandler.cs` (New)
  - `tests/AgencyLayer/DecisionExecution/DecisionExecution.Tests.csproj` (New)
  - `tests/AgencyLayer/DecisionExecution/DecisionExecutorTests.cs` (New)
