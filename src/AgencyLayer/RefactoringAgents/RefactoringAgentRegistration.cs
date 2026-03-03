using AgencyLayer.MultiAgentOrchestration.Adapters;
using AgencyLayer.MultiAgentOrchestration.Ports;
using AgencyLayer.RefactoringAgents.Engines;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.RefactoringAgents;

/// <summary>
/// Provides methods to register the SOLID/DRY refactoring agent with the
/// multi-agent orchestration system.
/// </summary>
public static class RefactoringAgentRegistration
{
    /// <summary>
    /// Registers the SOLID/DRY refactoring agent with the orchestrator and runtime adapter.
    /// This method creates the agent definition, registers it with the orchestration port,
    /// and wires up the runtime handler for task execution.
    /// </summary>
    /// <param name="orchestrationPort">The orchestration port to register the agent definition with.</param>
    /// <param name="runtimeAdapter">The runtime adapter to register the agent handler with.</param>
    /// <param name="loggerFactory">Logger factory for creating typed loggers.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public static async Task RegisterSolidDryAgentAsync(
        IMultiAgentOrchestrationPort orchestrationPort,
        InProcessAgentRuntimeAdapter runtimeAdapter,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(orchestrationPort);
        ArgumentNullException.ThrowIfNull(runtimeAdapter);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        // 1. Register the agent definition with the orchestrator
        var definition = new AgentDefinition
        {
            AgentId = Guid.NewGuid(),
            AgentType = SolidDryRefactoringAgent.AgentType,
            Description = "Analyzes source code for SOLID and DRY principle violations, providing actionable refactoring suggestions with severity ratings and code quality scores.",
            Capabilities = new List<string>
            {
                "analyze-solid-principles",
                "analyze-dry-violations",
                "suggest-refactorings",
                "code-quality-scoring",
                "extract-class",
                "extract-method",
                "extract-interface",
                "inject-dependency",
                "replace-conditional-with-polymorphism"
            },
            DefaultAutonomyLevel = AutonomyLevel.RecommendOnly,
            DefaultAuthorityScope = new AuthorityScope
            {
                AllowedApiEndpoints = new List<string> { "/api/code/analyze", "/api/code/refactor" },
                MaxResourceConsumption = 50.0,
                MaxBudget = 0m,
                DataAccessPolicies = new List<string> { "read:source-code" }
            },
            Status = AgentStatus.Active
        };

        await orchestrationPort.RegisterAgentAsync(definition);

        // 2. Create the agent and register its handler with the runtime adapter
        var engineLogger = loggerFactory.CreateLogger<SolidDryRefactoringEngine>();
        var agentLogger = loggerFactory.CreateLogger<SolidDryRefactoringAgent>();
        var engine = new SolidDryRefactoringEngine(engineLogger);
        var agent = new SolidDryRefactoringAgent(agentLogger, engine);

        runtimeAdapter.RegisterHandler(SolidDryRefactoringAgent.AgentType, async task =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await agent.HandleTaskAsync(task, cancellationToken);
        });
    }
}
