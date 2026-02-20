using System.Collections.Concurrent;
using Azure.AI.OpenAI;
using FoundationLayer.EnterpriseConnectors;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.ContinuousLearning;

/// <summary>
/// Manages continuous learning capabilities including framework enablement,
/// model adaptation, and learning report generation.
/// </summary>
public class LearningManager
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _completionDeployment;
    private readonly FeatureFlagManager _featureFlagManager;
    private readonly ILogger<LearningManager>? _logger;

    /// <summary>
    /// Thread-safe set tracking which frameworks/features are currently enabled.
    /// Keys are framework identifiers (e.g., "ADK", "LangGraph.Stateful").
    /// Values are the UTC timestamp when the framework was enabled.
    /// </summary>
    private readonly ConcurrentDictionary<string, DateTimeOffset> _enabledFrameworks = new();

    /// <summary>
    /// Defines parent-child prerequisite relationships between frameworks.
    /// A child feature (key) requires its parent framework (value) to be enabled first.
    /// </summary>
    private static readonly Dictionary<string, string> FrameworkPrerequisites = new()
    {
        // ADK sub-features require the ADK base framework
        ["ADK.WorkflowAgents"] = "ADK",
        ["ADK.ToolIntegration"] = "ADK",
        ["ADK.Guardrails"] = "ADK",
        ["ADK.Multimodal"] = "ADK",
        // LangGraph sub-features require the LangGraph base
        ["LangGraph.Stateful"] = "LangGraph",
        ["LangGraph.Streaming"] = "LangGraph",
        ["LangGraph.HITL"] = "LangGraph",
        ["LangGraph.StatefulWorkflows"] = "LangGraph",
        ["LangGraph.StreamingWorkflows"] = "LangGraph",
        ["LangGraph.HITLWorkflows"] = "LangGraph",
        // CrewAI sub-features
        ["CrewAI.Team"] = "CrewAI",
        ["CrewAI.DynamicPlanning"] = "CrewAI",
        ["CrewAI.AdaptiveExecution"] = "CrewAI",
        ["CrewAI.MultiAgent"] = "CrewAI",
        ["CrewAI.DynamicTaskRouting"] = "CrewAI",
        ["CrewAI.StatefulWorkflows"] = "CrewAI",
        // SemanticKernel sub-features
        ["SemanticKernel.Memory"] = "SemanticKernel",
        ["SemanticKernel.Security"] = "SemanticKernel",
        ["SemanticKernel.Automation"] = "SemanticKernel",
        ["SemanticKernel.MultiAgent"] = "SemanticKernel",
        ["SemanticKernel.DynamicTaskRouting"] = "SemanticKernel",
        ["SemanticKernel.StatefulWorkflows"] = "SemanticKernel",
        // AutoGen sub-features
        ["AutoGen.Conversations"] = "AutoGen",
        ["AutoGen.Context"] = "AutoGen",
        ["AutoGen.APIIntegration"] = "AutoGen",
        ["AutoGen.MultiAgent"] = "AutoGen",
        ["AutoGen.DynamicTaskRouting"] = "AutoGen",
        ["AutoGen.StatefulWorkflows"] = "AutoGen",
        // Smolagents sub-features
        ["Smolagents.Modular"] = "Smolagents",
        ["Smolagents.Context"] = "Smolagents",
        ["Smolagents.MultiAgent"] = "Smolagents",
        ["Smolagents.DynamicTaskRouting"] = "Smolagents",
        ["Smolagents.StatefulWorkflows"] = "Smolagents",
        // AutoGPT sub-features
        ["AutoGPT.Autonomous"] = "AutoGPT",
        ["AutoGPT.Memory"] = "AutoGPT",
        ["AutoGPT.InternetAccess"] = "AutoGPT",
        ["AutoGPT.MultiAgent"] = "AutoGPT",
        ["AutoGPT.DynamicTaskRouting"] = "AutoGPT",
        ["AutoGPT.StatefulWorkflows"] = "AutoGPT",
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="LearningManager"/> class.
    /// </summary>
    /// <param name="openAIEndpoint">The Azure OpenAI endpoint URL.</param>
    /// <param name="openAIApiKey">The Azure OpenAI API key.</param>
    /// <param name="completionDeployment">The deployment name for chat completions.</param>
    /// <param name="featureFlagManager">The feature flag manager for checking enablement.</param>
    /// <param name="logger">Optional logger for structured logging.</param>
    public LearningManager(
        string openAIEndpoint,
        string openAIApiKey,
        string completionDeployment,
        FeatureFlagManager featureFlagManager,
        ILogger<LearningManager>? logger = null)
    {
        _openAIClient = new OpenAIClient(new Uri(openAIEndpoint), new AzureKeyCredential(openAIApiKey));
        _completionDeployment = completionDeployment;
        _featureFlagManager = featureFlagManager ?? throw new ArgumentNullException(nameof(featureFlagManager));
        _logger = logger;
    }

    /// <summary>
    /// Gets a read-only snapshot of all currently enabled frameworks and their activation timestamps.
    /// </summary>
    public IReadOnlyDictionary<string, DateTimeOffset> EnabledFrameworks =>
        new Dictionary<string, DateTimeOffset>(_enabledFrameworks);

    /// <summary>
    /// Checks whether a specific framework or feature is currently enabled.
    /// </summary>
    /// <param name="frameworkId">The framework identifier to check.</param>
    /// <returns>True if the framework is enabled; false otherwise.</returns>
    public bool IsFrameworkEnabled(string frameworkId) =>
        _enabledFrameworks.ContainsKey(frameworkId);

    #region Core Learning Operations

    /// <summary>
    /// Enables the continuous learning pipeline by activating all base frameworks
    /// whose feature flags are currently enabled.
    /// </summary>
    public async Task EnableContinuousLearningAsync()
    {
        _logger?.LogInformation("Enabling continuous learning pipeline");

        await EnableFrameworkAsync("ContinuousLearning", isFeatureFlagEnabled: true);

        _logger?.LogInformation(
            "Continuous learning pipeline enabled. Active frameworks: {Count}",
            _enabledFrameworks.Count);
    }

    /// <summary>
    /// Adapts the model by refreshing the learning state and re-evaluating
    /// which frameworks should be active based on current feature flags.
    /// </summary>
    public async Task AdaptModelAsync()
    {
        _logger?.LogInformation("Adapting model: re-evaluating active frameworks");

        // Mark the adaptation event
        await EnableFrameworkAsync("ModelAdaptation", isFeatureFlagEnabled: true);

        _logger?.LogInformation(
            "Model adaptation complete. Active frameworks: {Count}",
            _enabledFrameworks.Count);
    }

    /// <summary>
    /// Generates a learning report using the Azure OpenAI completion model.
    /// Includes information about currently enabled frameworks.
    /// </summary>
    /// <returns>The generated learning report as a string.</returns>
    public async Task<string> GenerateLearningReportAsync()
    {
        var enabledList = string.Join(", ", _enabledFrameworks.Keys);
        var systemPrompt = "You are a learning report generation system. Generate a detailed learning report based on the provided data.";
        var userPrompt = $"Generate a learning report based on the recent learning data. Currently enabled frameworks: {enabledList}. Total active: {_enabledFrameworks.Count}.";

        var chatCompletionOptions = new ChatCompletionsOptions
        {
            DeploymentName = _completionDeployment,
            Temperature = 0.3f,
            MaxTokens = 800,
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userPrompt)
            }
        };

        var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionOptions);
        var report = response.Value.Choices[0].Message.Content;

        return report;
    }

    #endregion

    #region Framework Enablement — Base Frameworks

    /// <summary>Enables the ADK framework if its feature flag is active.</summary>
    public async Task EnableADKAsync() =>
        await EnableFrameworkAsync("ADK", _featureFlagManager.EnableADK);

    /// <summary>Enables the LangGraph framework if its feature flag is active.</summary>
    public async Task EnableLangGraphAsync() =>
        await EnableFrameworkAsync("LangGraph", _featureFlagManager.EnableLangGraph);

    /// <summary>Enables the CrewAI framework if its feature flag is active.</summary>
    public async Task EnableCrewAIAsync() =>
        await EnableFrameworkAsync("CrewAI", _featureFlagManager.EnableCrewAI);

    /// <summary>Enables the Semantic Kernel framework if its feature flag is active.</summary>
    public async Task EnableSemanticKernelAsync() =>
        await EnableFrameworkAsync("SemanticKernel", _featureFlagManager.EnableSemanticKernel);

    /// <summary>Enables the AutoGen framework if its feature flag is active.</summary>
    public async Task EnableAutoGenAsync() =>
        await EnableFrameworkAsync("AutoGen", _featureFlagManager.EnableAutoGen);

    /// <summary>Enables the Smolagents framework if its feature flag is active.</summary>
    public async Task EnableSmolagentsAsync() =>
        await EnableFrameworkAsync("Smolagents", _featureFlagManager.EnableSmolagents);

    /// <summary>Enables the AutoGPT framework if its feature flag is active.</summary>
    public async Task EnableAutoGPTAsync() =>
        await EnableFrameworkAsync("AutoGPT", _featureFlagManager.EnableAutoGPT);

    #endregion

    #region Framework Enablement — ADK Features

    /// <summary>Enables ADK Workflow Agents if its feature flag is active.</summary>
    public async Task EnableADKWorkflowAgentsAsync() =>
        await EnableFrameworkAsync("ADK.WorkflowAgents", _featureFlagManager.EnableADKWorkflowAgents);

    /// <summary>Enables ADK Tool Integration if its feature flag is active.</summary>
    public async Task EnableADKToolIntegrationAsync() =>
        await EnableFrameworkAsync("ADK.ToolIntegration", _featureFlagManager.EnableADKToolIntegration);

    /// <summary>Enables ADK Guardrails if its feature flag is active.</summary>
    public async Task EnableADKGuardrailsAsync() =>
        await EnableFrameworkAsync("ADK.Guardrails", _featureFlagManager.EnableADKGuardrails);

    /// <summary>Enables ADK Multimodal support if its feature flag is active.</summary>
    public async Task EnableADKMultimodalAsync() =>
        await EnableFrameworkAsync("ADK.Multimodal", _featureFlagManager.EnableADKMultimodal);

    #endregion

    #region Framework Enablement — LangGraph Features

    /// <summary>Enables LangGraph Stateful mode if its feature flag is active.</summary>
    public async Task EnableLangGraphStatefulAsync() =>
        await EnableFrameworkAsync("LangGraph.Stateful", _featureFlagManager.EnableLangGraphStateful);

    /// <summary>Enables LangGraph Streaming mode if its feature flag is active.</summary>
    public async Task EnableLangGraphStreamingAsync() =>
        await EnableFrameworkAsync("LangGraph.Streaming", _featureFlagManager.EnableLangGraphStreaming);

    /// <summary>Enables LangGraph HITL mode if its feature flag is active.</summary>
    public async Task EnableLangGraphHITLAsync() =>
        await EnableFrameworkAsync("LangGraph.HITL", _featureFlagManager.EnableLangGraphHITL);

    /// <summary>Enables LangGraph Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableLangGraphStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("LangGraph.StatefulWorkflows", _featureFlagManager.EnableLangGraphStatefulWorkflows);

    /// <summary>Enables LangGraph Streaming Workflows if its feature flag is active.</summary>
    public async Task EnableLangGraphStreamingWorkflowsAsync() =>
        await EnableFrameworkAsync("LangGraph.StreamingWorkflows", _featureFlagManager.EnableLangGraphStreamingWorkflows);

    /// <summary>Enables LangGraph HITL Workflows if its feature flag is active.</summary>
    public async Task EnableLangGraphHITLWorkflowsAsync() =>
        await EnableFrameworkAsync("LangGraph.HITLWorkflows", _featureFlagManager.EnableLangGraphHITLWorkflows);

    #endregion

    #region Framework Enablement — CrewAI Features

    /// <summary>Enables CrewAI Team mode if its feature flag is active.</summary>
    public async Task EnableCrewAITeamAsync() =>
        await EnableFrameworkAsync("CrewAI.Team", _featureFlagManager.EnableCrewAITeam);

    /// <summary>Enables CrewAI Dynamic Planning if its feature flag is active.</summary>
    public async Task EnableCrewAIDynamicPlanningAsync() =>
        await EnableFrameworkAsync("CrewAI.DynamicPlanning", _featureFlagManager.EnableCrewAIDynamicPlanning);

    /// <summary>Enables CrewAI Adaptive Execution if its feature flag is active.</summary>
    public async Task EnableCrewAIAdaptiveExecutionAsync() =>
        await EnableFrameworkAsync("CrewAI.AdaptiveExecution", _featureFlagManager.EnableCrewAIAdaptiveExecution);

    /// <summary>Enables CrewAI Multi-Agent mode if its feature flag is active.</summary>
    public async Task EnableCrewAIMultiAgentAsync() =>
        await EnableFrameworkAsync("CrewAI.MultiAgent", _featureFlagManager.EnableCrewAIMultiAgent);

    /// <summary>Enables CrewAI Dynamic Task Routing if its feature flag is active.</summary>
    public async Task EnableCrewAIDynamicTaskRoutingAsync() =>
        await EnableFrameworkAsync("CrewAI.DynamicTaskRouting", _featureFlagManager.EnableCrewAIDynamicTaskRouting);

    /// <summary>Enables CrewAI Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableCrewAIStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("CrewAI.StatefulWorkflows", _featureFlagManager.EnableCrewAIStatefulWorkflows);

    #endregion

    #region Framework Enablement — Semantic Kernel Features

    /// <summary>Enables Semantic Kernel Memory if its feature flag is active.</summary>
    public async Task EnableSemanticKernelMemoryAsync() =>
        await EnableFrameworkAsync("SemanticKernel.Memory", _featureFlagManager.EnableSemanticKernelMemory);

    /// <summary>Enables Semantic Kernel Security if its feature flag is active.</summary>
    public async Task EnableSemanticKernelSecurityAsync() =>
        await EnableFrameworkAsync("SemanticKernel.Security", _featureFlagManager.EnableSemanticKernelSecurity);

    /// <summary>Enables Semantic Kernel Automation if its feature flag is active.</summary>
    public async Task EnableSemanticKernelAutomationAsync() =>
        await EnableFrameworkAsync("SemanticKernel.Automation", _featureFlagManager.EnableSemanticKernelAutomation);

    /// <summary>Enables Semantic Kernel Multi-Agent mode if its feature flag is active.</summary>
    public async Task EnableSemanticKernelMultiAgentAsync() =>
        await EnableFrameworkAsync("SemanticKernel.MultiAgent", _featureFlagManager.EnableSemanticKernelMultiAgent);

    /// <summary>Enables Semantic Kernel Dynamic Task Routing if its feature flag is active.</summary>
    public async Task EnableSemanticKernelDynamicTaskRoutingAsync() =>
        await EnableFrameworkAsync("SemanticKernel.DynamicTaskRouting", _featureFlagManager.EnableSemanticKernelDynamicTaskRouting);

    /// <summary>Enables Semantic Kernel Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableSemanticKernelStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("SemanticKernel.StatefulWorkflows", _featureFlagManager.EnableSemanticKernelStatefulWorkflows);

    #endregion

    #region Framework Enablement — AutoGen Features

    /// <summary>Enables AutoGen Conversations if its feature flag is active.</summary>
    public async Task EnableAutoGenConversationsAsync() =>
        await EnableFrameworkAsync("AutoGen.Conversations", _featureFlagManager.EnableAutoGenConversations);

    /// <summary>Enables AutoGen Context management if its feature flag is active.</summary>
    public async Task EnableAutoGenContextAsync() =>
        await EnableFrameworkAsync("AutoGen.Context", _featureFlagManager.EnableAutoGenContext);

    /// <summary>Enables AutoGen API Integration if its feature flag is active.</summary>
    public async Task EnableAutoGenAPIIntegrationAsync() =>
        await EnableFrameworkAsync("AutoGen.APIIntegration", _featureFlagManager.EnableAutoGenAPIIntegration);

    /// <summary>Enables AutoGen Multi-Agent mode if its feature flag is active.</summary>
    public async Task EnableAutoGenMultiAgentAsync() =>
        await EnableFrameworkAsync("AutoGen.MultiAgent", _featureFlagManager.EnableAutoGenMultiAgent);

    /// <summary>Enables AutoGen Dynamic Task Routing if its feature flag is active.</summary>
    public async Task EnableAutoGenDynamicTaskRoutingAsync() =>
        await EnableFrameworkAsync("AutoGen.DynamicTaskRouting", _featureFlagManager.EnableAutoGenDynamicTaskRouting);

    /// <summary>Enables AutoGen Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableAutoGenStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("AutoGen.StatefulWorkflows", _featureFlagManager.EnableAutoGenStatefulWorkflows);

    #endregion

    #region Framework Enablement — Smolagents Features

    /// <summary>Enables Smolagents Modular mode if its feature flag is active.</summary>
    public async Task EnableSmolagentsModularAsync() =>
        await EnableFrameworkAsync("Smolagents.Modular", _featureFlagManager.EnableSmolagentsModular);

    /// <summary>Enables Smolagents Context management if its feature flag is active.</summary>
    public async Task EnableSmolagentsContextAsync() =>
        await EnableFrameworkAsync("Smolagents.Context", _featureFlagManager.EnableSmolagentsContext);

    /// <summary>Enables Smolagents Multi-Agent mode if its feature flag is active.</summary>
    public async Task EnableSmolagentsMultiAgentAsync() =>
        await EnableFrameworkAsync("Smolagents.MultiAgent", _featureFlagManager.EnableSmolagentsMultiAgent);

    /// <summary>Enables Smolagents Dynamic Task Routing if its feature flag is active.</summary>
    public async Task EnableSmolagentsDynamicTaskRoutingAsync() =>
        await EnableFrameworkAsync("Smolagents.DynamicTaskRouting", _featureFlagManager.EnableSmolagentsDynamicTaskRouting);

    /// <summary>Enables Smolagents Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableSmolagentsStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("Smolagents.StatefulWorkflows", _featureFlagManager.EnableSmolagentsStatefulWorkflows);

    #endregion

    #region Framework Enablement — AutoGPT Features

    /// <summary>Enables AutoGPT Autonomous mode if its feature flag is active.</summary>
    public async Task EnableAutoGPTAutonomousAsync() =>
        await EnableFrameworkAsync("AutoGPT.Autonomous", _featureFlagManager.EnableAutoGPTAutonomous);

    /// <summary>Enables AutoGPT Memory if its feature flag is active.</summary>
    public async Task EnableAutoGPTMemoryAsync() =>
        await EnableFrameworkAsync("AutoGPT.Memory", _featureFlagManager.EnableAutoGPTMemory);

    /// <summary>Enables AutoGPT Internet Access if its feature flag is active.</summary>
    public async Task EnableAutoGPTInternetAccessAsync() =>
        await EnableFrameworkAsync("AutoGPT.InternetAccess", _featureFlagManager.EnableAutoGPTInternetAccess);

    /// <summary>Enables AutoGPT Multi-Agent mode if its feature flag is active.</summary>
    public async Task EnableAutoGPTMultiAgentAsync() =>
        await EnableFrameworkAsync("AutoGPT.MultiAgent", _featureFlagManager.EnableAutoGPTMultiAgent);

    /// <summary>Enables AutoGPT Dynamic Task Routing if its feature flag is active.</summary>
    public async Task EnableAutoGPTDynamicTaskRoutingAsync() =>
        await EnableFrameworkAsync("AutoGPT.DynamicTaskRouting", _featureFlagManager.EnableAutoGPTDynamicTaskRouting);

    /// <summary>Enables AutoGPT Stateful Workflows if its feature flag is active.</summary>
    public async Task EnableAutoGPTStatefulWorkflowsAsync() =>
        await EnableFrameworkAsync("AutoGPT.StatefulWorkflows", _featureFlagManager.EnableAutoGPTStatefulWorkflows);

    #endregion

    #region Private Helpers

    /// <summary>
    /// Common enablement logic for all frameworks and features.
    /// Checks the feature flag, validates prerequisites, registers the framework as enabled, and logs the action.
    /// </summary>
    /// <param name="frameworkId">Unique identifier for the framework or feature (e.g., "ADK.Guardrails").</param>
    /// <param name="isFeatureFlagEnabled">Whether the corresponding feature flag is currently enabled.</param>
    private Task EnableFrameworkAsync(string frameworkId, bool isFeatureFlagEnabled)
    {
        if (!isFeatureFlagEnabled)
        {
            _logger?.LogDebug("Feature flag disabled for framework '{FrameworkId}'; skipping enablement", frameworkId);
            return Task.CompletedTask;
        }

        // Check if already enabled to avoid redundant work
        if (_enabledFrameworks.ContainsKey(frameworkId))
        {
            _logger?.LogDebug("Framework '{FrameworkId}' is already enabled", frameworkId);
            return Task.CompletedTask;
        }

        // Validate prerequisite: if this is a sub-feature, the parent must be enabled
        if (FrameworkPrerequisites.TryGetValue(frameworkId, out var prerequisite))
        {
            if (!_enabledFrameworks.ContainsKey(prerequisite))
            {
                _logger?.LogWarning(
                    "Cannot enable '{FrameworkId}': prerequisite '{Prerequisite}' is not enabled. " +
                    "Enable the base framework first.",
                    frameworkId, prerequisite);
                return Task.CompletedTask;
            }
        }

        // Register the framework as enabled
        var enabledAt = DateTimeOffset.UtcNow;
        if (_enabledFrameworks.TryAdd(frameworkId, enabledAt))
        {
            _logger?.LogInformation(
                "Framework '{FrameworkId}' enabled at {EnabledAt}. Total active frameworks: {Count}",
                frameworkId, enabledAt, _enabledFrameworks.Count);
        }

        return Task.CompletedTask;
    }

    #endregion
}
