using FluentAssertions;
using FoundationLayer.EnterpriseConnectors;
using MetacognitiveLayer.ContinuousLearning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.MetacognitiveLayer.ContinuousLearning;

/// <summary>
/// Unit tests for <see cref="LearningManager"/>, covering constructor validation,
/// base framework enablement, sub-feature prerequisite checks, feature flag gating,
/// idempotency, and multi-framework scenarios.
/// </summary>
public class LearningManagerTests
{
    private const string TestEndpoint = "https://test.openai.azure.com/";
    private const string TestApiKey = "test-api-key-00000000000000000000";
    private const string TestDeployment = "gpt-4-test";

    private readonly Mock<ILogger<LearningManager>> _loggerMock;

    /// <summary>
    /// Initializes shared test fixtures.
    /// </summary>
    public LearningManagerTests()
    {
        _loggerMock = new Mock<ILogger<LearningManager>>();
    }

    // ------------------------------------------------------------------
    // Helper: build a FeatureFlagManager backed by in-memory configuration
    // ------------------------------------------------------------------

    /// <summary>
    /// Creates a <see cref="FeatureFlagManager"/> whose feature flags are set
    /// according to the provided dictionary of configuration overrides.
    /// Keys should match the "FeatureFlags:*" config paths.
    /// </summary>
    private static FeatureFlagManager CreateFeatureFlagManager(
        Dictionary<string, string?>? overrides = null)
    {
        var defaults = new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "false",
            ["FeatureFlags:enable_ADK_WorkflowAgents"] = "false",
            ["FeatureFlags:enable_ADK_ToolIntegration"] = "false",
            ["FeatureFlags:enable_ADK_Guardrails"] = "false",
            ["FeatureFlags:enable_ADK_Multimodal"] = "false",
            ["FeatureFlags:enable_LangGraph"] = "false",
            ["FeatureFlags:enable_LangGraph_Stateful"] = "false",
            ["FeatureFlags:enable_LangGraph_Streaming"] = "false",
            ["FeatureFlags:enable_LangGraph_HITL"] = "false",
            ["FeatureFlags:enable_LangGraph_StatefulWorkflows"] = "false",
            ["FeatureFlags:enable_LangGraph_StreamingWorkflows"] = "false",
            ["FeatureFlags:enable_LangGraph_HITLWorkflows"] = "false",
            ["FeatureFlags:enable_CrewAI"] = "false",
            ["FeatureFlags:enable_CrewAI_Team"] = "false",
            ["FeatureFlags:enable_CrewAI_DynamicPlanning"] = "false",
            ["FeatureFlags:enable_CrewAI_AdaptiveExecution"] = "false",
            ["FeatureFlags:enable_CrewAI_MultiAgent"] = "false",
            ["FeatureFlags:enable_CrewAI_DynamicTaskRouting"] = "false",
            ["FeatureFlags:enable_CrewAI_StatefulWorkflows"] = "false",
            ["FeatureFlags:enable_SemanticKernel"] = "false",
            ["FeatureFlags:enable_SemanticKernel_Memory"] = "false",
            ["FeatureFlags:enable_SemanticKernel_Security"] = "false",
            ["FeatureFlags:enable_SemanticKernel_Automation"] = "false",
            ["FeatureFlags:enable_SemanticKernel_MultiAgent"] = "false",
            ["FeatureFlags:enable_SemanticKernel_DynamicTaskRouting"] = "false",
            ["FeatureFlags:enable_SemanticKernel_StatefulWorkflows"] = "false",
            ["FeatureFlags:enable_AutoGen"] = "false",
            ["FeatureFlags:enable_AutoGen_Conversations"] = "false",
            ["FeatureFlags:enable_AutoGen_Context"] = "false",
            ["FeatureFlags:enable_AutoGen_APIIntegration"] = "false",
            ["FeatureFlags:enable_AutoGen_MultiAgent"] = "false",
            ["FeatureFlags:enable_AutoGen_DynamicTaskRouting"] = "false",
            ["FeatureFlags:enable_AutoGen_StatefulWorkflows"] = "false",
            ["FeatureFlags:enable_Smolagents"] = "false",
            ["FeatureFlags:enable_Smolagents_Modular"] = "false",
            ["FeatureFlags:enable_Smolagents_Context"] = "false",
            ["FeatureFlags:enable_Smolagents_MultiAgent"] = "false",
            ["FeatureFlags:enable_Smolagents_DynamicTaskRouting"] = "false",
            ["FeatureFlags:enable_Smolagents_StatefulWorkflows"] = "false",
            ["FeatureFlags:enable_AutoGPT"] = "false",
            ["FeatureFlags:enable_AutoGPT_Autonomous"] = "false",
            ["FeatureFlags:enable_AutoGPT_Memory"] = "false",
            ["FeatureFlags:enable_AutoGPT_InternetAccess"] = "false",
            ["FeatureFlags:enable_AutoGPT_MultiAgent"] = "false",
            ["FeatureFlags:enable_AutoGPT_DynamicTaskRouting"] = "false",
            ["FeatureFlags:enable_AutoGPT_StatefulWorkflows"] = "false",
        };

        if (overrides is not null)
        {
            foreach (var kvp in overrides)
            {
                defaults[kvp.Key] = kvp.Value;
            }
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(defaults)
            .Build();

        return new FeatureFlagManager(configuration);
    }

    /// <summary>
    /// Creates a <see cref="LearningManager"/> with all feature flags disabled by default.
    /// </summary>
    private LearningManager CreateSut(
        Dictionary<string, string?>? flagOverrides = null,
        ILogger<LearningManager>? logger = null)
    {
        var ffm = CreateFeatureFlagManager(flagOverrides);
        return new LearningManager(
            TestEndpoint,
            TestApiKey,
            TestDeployment,
            ffm,
            logger);
    }

    // ==================================================================
    // Constructor Tests
    // ==================================================================

    /// <summary>Tests constructor null guard for featureFlagManager.</summary>
    [Fact]
    public void Constructor_NullFeatureFlagManager_ThrowsArgumentNullException()
    {
        var act = () => new LearningManager(
            TestEndpoint,
            TestApiKey,
            TestDeployment,
            featureFlagManager: null!,
            logger: null);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("featureFlagManager");
    }

    /// <summary>Tests that a valid constructor call succeeds.</summary>
    [Fact]
    public void Constructor_ValidArguments_CreatesInstance()
    {
        var sut = CreateSut();

        sut.Should().NotBeNull();
    }

    /// <summary>Tests that the logger parameter is optional (nullable).</summary>
    [Fact]
    public void Constructor_NullLogger_DoesNotThrow()
    {
        var sut = CreateSut(logger: null);

        sut.Should().NotBeNull();
    }

    /// <summary>Tests that passing a real logger also works.</summary>
    [Fact]
    public void Constructor_WithLogger_CreatesInstance()
    {
        var sut = CreateSut(logger: _loggerMock.Object);

        sut.Should().NotBeNull();
    }

    // ==================================================================
    // EnabledFrameworks Property Tests
    // ==================================================================

    /// <summary>Tests that EnabledFrameworks is initially empty.</summary>
    [Fact]
    public void EnabledFrameworks_Initially_IsEmpty()
    {
        var sut = CreateSut();

        sut.EnabledFrameworks.Should().BeEmpty();
    }

    /// <summary>Tests that EnabledFrameworks returns a snapshot (not a live reference).</summary>
    [Fact]
    public async Task EnabledFrameworks_AfterEnabling_ReturnsSnapshot()
    {
        var sut = CreateSut();

        await sut.EnableContinuousLearningAsync();

        var snapshot1 = sut.EnabledFrameworks;
        var snapshot2 = sut.EnabledFrameworks;

        // Both snapshots should have the same content
        snapshot1.Should().BeEquivalentTo(snapshot2);

        // But they should be distinct dictionary instances (snapshot behavior)
        snapshot1.Should().NotBeSameAs(snapshot2);
    }

    // ==================================================================
    // IsFrameworkEnabled Tests
    // ==================================================================

    /// <summary>Tests IsFrameworkEnabled returns false for unknown framework.</summary>
    [Fact]
    public void IsFrameworkEnabled_UnknownFramework_ReturnsFalse()
    {
        var sut = CreateSut();

        sut.IsFrameworkEnabled("NonExistent").Should().BeFalse();
    }

    /// <summary>Tests IsFrameworkEnabled returns true after framework is enabled.</summary>
    [Fact]
    public async Task IsFrameworkEnabled_AfterEnablement_ReturnsTrue()
    {
        var sut = CreateSut();

        await sut.EnableContinuousLearningAsync();

        sut.IsFrameworkEnabled("ContinuousLearning").Should().BeTrue();
    }

    // ==================================================================
    // Core Learning Operations
    // ==================================================================

    /// <summary>Tests EnableContinuousLearningAsync registers the ContinuousLearning framework.</summary>
    [Fact]
    public async Task EnableContinuousLearningAsync_Always_EnablesContinuousLearningFramework()
    {
        var sut = CreateSut();

        await sut.EnableContinuousLearningAsync();

        sut.IsFrameworkEnabled("ContinuousLearning").Should().BeTrue();
        sut.EnabledFrameworks.Should().ContainKey("ContinuousLearning");
    }

    /// <summary>Tests AdaptModelAsync registers the ModelAdaptation framework.</summary>
    [Fact]
    public async Task AdaptModelAsync_Always_EnablesModelAdaptationFramework()
    {
        var sut = CreateSut();

        await sut.AdaptModelAsync();

        sut.IsFrameworkEnabled("ModelAdaptation").Should().BeTrue();
        sut.EnabledFrameworks.Should().ContainKey("ModelAdaptation");
    }

    /// <summary>Tests that EnableContinuousLearningAsync is idempotent.</summary>
    [Fact]
    public async Task EnableContinuousLearningAsync_CalledTwice_OnlyRegistersOnce()
    {
        var sut = CreateSut();

        await sut.EnableContinuousLearningAsync();
        var firstTimestamp = sut.EnabledFrameworks["ContinuousLearning"];

        await sut.EnableContinuousLearningAsync();
        var secondTimestamp = sut.EnabledFrameworks["ContinuousLearning"];

        // Timestamp should remain the same (not re-registered)
        secondTimestamp.Should().Be(firstTimestamp);
        sut.EnabledFrameworks.Should().HaveCount(1);
    }

    // ==================================================================
    // Base Framework Enablement — Feature Flag Enabled
    // ==================================================================

    /// <summary>Tests that each base framework is enabled when its feature flag is true.</summary>
    [Theory]
    [InlineData("enable_ADK", "ADK")]
    [InlineData("enable_LangGraph", "LangGraph")]
    [InlineData("enable_CrewAI", "CrewAI")]
    [InlineData("enable_SemanticKernel", "SemanticKernel")]
    [InlineData("enable_AutoGen", "AutoGen")]
    [InlineData("enable_Smolagents", "Smolagents")]
    [InlineData("enable_AutoGPT", "AutoGPT")]
    public async Task EnableBaseFrameworkAsync_FlagEnabled_RegistersFramework(
        string flagKey, string frameworkId)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        // Call the corresponding EnableXxxAsync method via reflection
        // to avoid duplicating 7 nearly identical tests
        var methodName = $"Enable{frameworkId}Async";
        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull($"method '{methodName}' should exist");

        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    // ==================================================================
    // Base Framework Enablement — Feature Flag Disabled
    // ==================================================================

    /// <summary>Tests that base frameworks are NOT enabled when their feature flag is false.</summary>
    [Theory]
    [InlineData("ADK")]
    [InlineData("LangGraph")]
    [InlineData("CrewAI")]
    [InlineData("SemanticKernel")]
    [InlineData("AutoGen")]
    [InlineData("Smolagents")]
    [InlineData("AutoGPT")]
    public async Task EnableBaseFrameworkAsync_FlagDisabled_DoesNotRegisterFramework(
        string frameworkId)
    {
        // All flags disabled by default
        var sut = CreateSut();

        var methodName = $"Enable{frameworkId}Async";
        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();

        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
        sut.EnabledFrameworks.Should().BeEmpty();
    }

    // ==================================================================
    // Sub-Feature Enablement — Prerequisite Met
    // ==================================================================

    /// <summary>
    /// Tests ADK sub-features: when ADK base is enabled and sub-feature flag is on,
    /// the sub-feature is registered.
    /// </summary>
    [Theory]
    [InlineData("enable_ADK_WorkflowAgents", "ADK.WorkflowAgents", "EnableADKWorkflowAgentsAsync")]
    [InlineData("enable_ADK_ToolIntegration", "ADK.ToolIntegration", "EnableADKToolIntegrationAsync")]
    [InlineData("enable_ADK_Guardrails", "ADK.Guardrails", "EnableADKGuardrailsAsync")]
    [InlineData("enable_ADK_Multimodal", "ADK.Multimodal", "EnableADKMultimodalAsync")]
    public async Task EnableADKSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        // First enable the base framework
        await sut.EnableADKAsync();
        sut.IsFrameworkEnabled("ADK").Should().BeTrue();

        // Then enable the sub-feature
        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests LangGraph sub-features: when LangGraph base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_LangGraph_Stateful", "LangGraph.Stateful", "EnableLangGraphStatefulAsync")]
    [InlineData("enable_LangGraph_Streaming", "LangGraph.Streaming", "EnableLangGraphStreamingAsync")]
    [InlineData("enable_LangGraph_HITL", "LangGraph.HITL", "EnableLangGraphHITLAsync")]
    [InlineData("enable_LangGraph_StatefulWorkflows", "LangGraph.StatefulWorkflows", "EnableLangGraphStatefulWorkflowsAsync")]
    [InlineData("enable_LangGraph_StreamingWorkflows", "LangGraph.StreamingWorkflows", "EnableLangGraphStreamingWorkflowsAsync")]
    [InlineData("enable_LangGraph_HITLWorkflows", "LangGraph.HITLWorkflows", "EnableLangGraphHITLWorkflowsAsync")]
    public async Task EnableLangGraphSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_LangGraph"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableLangGraphAsync();
        sut.IsFrameworkEnabled("LangGraph").Should().BeTrue();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests CrewAI sub-features: when CrewAI base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_CrewAI_Team", "CrewAI.Team", "EnableCrewAITeamAsync")]
    [InlineData("enable_CrewAI_DynamicPlanning", "CrewAI.DynamicPlanning", "EnableCrewAIDynamicPlanningAsync")]
    [InlineData("enable_CrewAI_AdaptiveExecution", "CrewAI.AdaptiveExecution", "EnableCrewAIAdaptiveExecutionAsync")]
    [InlineData("enable_CrewAI_MultiAgent", "CrewAI.MultiAgent", "EnableCrewAIMultiAgentAsync")]
    [InlineData("enable_CrewAI_DynamicTaskRouting", "CrewAI.DynamicTaskRouting", "EnableCrewAIDynamicTaskRoutingAsync")]
    [InlineData("enable_CrewAI_StatefulWorkflows", "CrewAI.StatefulWorkflows", "EnableCrewAIStatefulWorkflowsAsync")]
    public async Task EnableCrewAISubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableCrewAIAsync();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests SemanticKernel sub-features: when SemanticKernel base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_SemanticKernel_Memory", "SemanticKernel.Memory", "EnableSemanticKernelMemoryAsync")]
    [InlineData("enable_SemanticKernel_Security", "SemanticKernel.Security", "EnableSemanticKernelSecurityAsync")]
    [InlineData("enable_SemanticKernel_Automation", "SemanticKernel.Automation", "EnableSemanticKernelAutomationAsync")]
    [InlineData("enable_SemanticKernel_MultiAgent", "SemanticKernel.MultiAgent", "EnableSemanticKernelMultiAgentAsync")]
    [InlineData("enable_SemanticKernel_DynamicTaskRouting", "SemanticKernel.DynamicTaskRouting", "EnableSemanticKernelDynamicTaskRoutingAsync")]
    [InlineData("enable_SemanticKernel_StatefulWorkflows", "SemanticKernel.StatefulWorkflows", "EnableSemanticKernelStatefulWorkflowsAsync")]
    public async Task EnableSemanticKernelSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_SemanticKernel"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableSemanticKernelAsync();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests AutoGen sub-features: when AutoGen base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_AutoGen_Conversations", "AutoGen.Conversations", "EnableAutoGenConversationsAsync")]
    [InlineData("enable_AutoGen_Context", "AutoGen.Context", "EnableAutoGenContextAsync")]
    [InlineData("enable_AutoGen_APIIntegration", "AutoGen.APIIntegration", "EnableAutoGenAPIIntegrationAsync")]
    [InlineData("enable_AutoGen_MultiAgent", "AutoGen.MultiAgent", "EnableAutoGenMultiAgentAsync")]
    [InlineData("enable_AutoGen_DynamicTaskRouting", "AutoGen.DynamicTaskRouting", "EnableAutoGenDynamicTaskRoutingAsync")]
    [InlineData("enable_AutoGen_StatefulWorkflows", "AutoGen.StatefulWorkflows", "EnableAutoGenStatefulWorkflowsAsync")]
    public async Task EnableAutoGenSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGen"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableAutoGenAsync();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests Smolagents sub-features: when Smolagents base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_Smolagents_Modular", "Smolagents.Modular", "EnableSmolagentsModularAsync")]
    [InlineData("enable_Smolagents_Context", "Smolagents.Context", "EnableSmolagentsContextAsync")]
    [InlineData("enable_Smolagents_MultiAgent", "Smolagents.MultiAgent", "EnableSmolagentsMultiAgentAsync")]
    [InlineData("enable_Smolagents_DynamicTaskRouting", "Smolagents.DynamicTaskRouting", "EnableSmolagentsDynamicTaskRoutingAsync")]
    [InlineData("enable_Smolagents_StatefulWorkflows", "Smolagents.StatefulWorkflows", "EnableSmolagentsStatefulWorkflowsAsync")]
    public async Task EnableSmolagentsSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_Smolagents"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableSmolagentsAsync();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    /// <summary>
    /// Tests AutoGPT sub-features: when AutoGPT base is enabled and sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_AutoGPT_Autonomous", "AutoGPT.Autonomous", "EnableAutoGPTAutonomousAsync")]
    [InlineData("enable_AutoGPT_Memory", "AutoGPT.Memory", "EnableAutoGPTMemoryAsync")]
    [InlineData("enable_AutoGPT_InternetAccess", "AutoGPT.InternetAccess", "EnableAutoGPTInternetAccessAsync")]
    [InlineData("enable_AutoGPT_MultiAgent", "AutoGPT.MultiAgent", "EnableAutoGPTMultiAgentAsync")]
    [InlineData("enable_AutoGPT_DynamicTaskRouting", "AutoGPT.DynamicTaskRouting", "EnableAutoGPTDynamicTaskRoutingAsync")]
    [InlineData("enable_AutoGPT_StatefulWorkflows", "AutoGPT.StatefulWorkflows", "EnableAutoGPTStatefulWorkflowsAsync")]
    public async Task EnableAutoGPTSubFeatureAsync_BaseEnabledAndFlagOn_RegistersSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_AutoGPT"] = "true",
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        await sut.EnableAutoGPTAsync();

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeTrue();
    }

    // ==================================================================
    // Sub-Feature Enablement — Prerequisite NOT Met
    // ==================================================================

    /// <summary>
    /// Tests that ADK sub-features are NOT registered when ADK base is not enabled,
    /// even though the sub-feature flag is on.
    /// </summary>
    [Theory]
    [InlineData("enable_ADK_WorkflowAgents", "ADK.WorkflowAgents", "EnableADKWorkflowAgentsAsync")]
    [InlineData("enable_ADK_ToolIntegration", "ADK.ToolIntegration", "EnableADKToolIntegrationAsync")]
    [InlineData("enable_ADK_Guardrails", "ADK.Guardrails", "EnableADKGuardrailsAsync")]
    [InlineData("enable_ADK_Multimodal", "ADK.Multimodal", "EnableADKMultimodalAsync")]
    public async Task EnableADKSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        // Sub-feature flag is on, but base ADK flag is off (or base not enabled)
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that LangGraph sub-features are NOT registered when LangGraph base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_LangGraph_Stateful", "LangGraph.Stateful", "EnableLangGraphStatefulAsync")]
    [InlineData("enable_LangGraph_Streaming", "LangGraph.Streaming", "EnableLangGraphStreamingAsync")]
    [InlineData("enable_LangGraph_HITL", "LangGraph.HITL", "EnableLangGraphHITLAsync")]
    public async Task EnableLangGraphSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that CrewAI sub-features are NOT registered when CrewAI base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_CrewAI_Team", "CrewAI.Team", "EnableCrewAITeamAsync")]
    [InlineData("enable_CrewAI_DynamicPlanning", "CrewAI.DynamicPlanning", "EnableCrewAIDynamicPlanningAsync")]
    [InlineData("enable_CrewAI_AdaptiveExecution", "CrewAI.AdaptiveExecution", "EnableCrewAIAdaptiveExecutionAsync")]
    public async Task EnableCrewAISubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that SemanticKernel sub-features are NOT registered when SemanticKernel base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_SemanticKernel_Memory", "SemanticKernel.Memory", "EnableSemanticKernelMemoryAsync")]
    [InlineData("enable_SemanticKernel_Security", "SemanticKernel.Security", "EnableSemanticKernelSecurityAsync")]
    public async Task EnableSemanticKernelSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that AutoGen sub-features are NOT registered when AutoGen base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_AutoGen_Conversations", "AutoGen.Conversations", "EnableAutoGenConversationsAsync")]
    [InlineData("enable_AutoGen_Context", "AutoGen.Context", "EnableAutoGenContextAsync")]
    public async Task EnableAutoGenSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that Smolagents sub-features are NOT registered when Smolagents base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_Smolagents_Modular", "Smolagents.Modular", "EnableSmolagentsModularAsync")]
    [InlineData("enable_Smolagents_Context", "Smolagents.Context", "EnableSmolagentsContextAsync")]
    public async Task EnableSmolagentsSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    /// <summary>
    /// Tests that AutoGPT sub-features are NOT registered when AutoGPT base is not enabled.
    /// </summary>
    [Theory]
    [InlineData("enable_AutoGPT_Autonomous", "AutoGPT.Autonomous", "EnableAutoGPTAutonomousAsync")]
    [InlineData("enable_AutoGPT_Memory", "AutoGPT.Memory", "EnableAutoGPTMemoryAsync")]
    public async Task EnableAutoGPTSubFeatureAsync_BaseNotEnabled_DoesNotRegisterSubFeature(
        string flagKey, string frameworkId, string methodName)
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            [$"FeatureFlags:{flagKey}"] = "true"
        });

        var method = typeof(LearningManager).GetMethod(methodName);
        method.Should().NotBeNull();
        var task = (Task)method!.Invoke(sut, Array.Empty<object>())!;
        await task;

        sut.IsFrameworkEnabled(frameworkId).Should().BeFalse();
    }

    // ==================================================================
    // Sub-Feature Enablement — Feature Flag Disabled
    // ==================================================================

    /// <summary>
    /// Tests that sub-features are NOT registered when the sub-feature flag is off,
    /// even if the base framework is enabled.
    /// </summary>
    [Fact]
    public async Task EnableADKWorkflowAgentsAsync_FlagDisabled_DoesNotRegisterEvenIfBaseEnabled()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_ADK_WorkflowAgents"] = "false"
        });

        await sut.EnableADKAsync();
        sut.IsFrameworkEnabled("ADK").Should().BeTrue();

        await sut.EnableADKWorkflowAgentsAsync();

        sut.IsFrameworkEnabled("ADK.WorkflowAgents").Should().BeFalse();
    }

    /// <summary>
    /// Tests that CrewAI.Team is not registered when its flag is off, even if CrewAI base is enabled.
    /// </summary>
    [Fact]
    public async Task EnableCrewAITeamAsync_FlagDisabled_DoesNotRegisterEvenIfBaseEnabled()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true",
            ["FeatureFlags:enable_CrewAI_Team"] = "false"
        });

        await sut.EnableCrewAIAsync();

        await sut.EnableCrewAITeamAsync();

        sut.IsFrameworkEnabled("CrewAI.Team").Should().BeFalse();
    }

    // ==================================================================
    // Idempotency Tests
    // ==================================================================

    /// <summary>Tests that enabling a base framework twice does not change state.</summary>
    [Fact]
    public async Task EnableADKAsync_CalledTwice_RemainsIdempotent()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        await sut.EnableADKAsync();
        var count1 = sut.EnabledFrameworks.Count;
        var ts1 = sut.EnabledFrameworks["ADK"];

        await sut.EnableADKAsync();
        var count2 = sut.EnabledFrameworks.Count;
        var ts2 = sut.EnabledFrameworks["ADK"];

        count2.Should().Be(count1);
        ts2.Should().Be(ts1);
    }

    /// <summary>Tests that enabling a sub-feature twice does not change state.</summary>
    [Fact]
    public async Task EnableADKGuardrailsAsync_CalledTwice_RemainsIdempotent()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_ADK_Guardrails"] = "true"
        });

        await sut.EnableADKAsync();
        await sut.EnableADKGuardrailsAsync();
        var count1 = sut.EnabledFrameworks.Count;

        await sut.EnableADKGuardrailsAsync();
        var count2 = sut.EnabledFrameworks.Count;

        count2.Should().Be(count1);
    }

    // ==================================================================
    // Multiple Frameworks — Enabling Several Simultaneously
    // ==================================================================

    /// <summary>
    /// Tests that multiple base frameworks can be enabled in a single LearningManager instance.
    /// </summary>
    [Fact]
    public async Task MultipleBaseFrameworks_AllFlagsEnabled_AllRegistered()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_LangGraph"] = "true",
            ["FeatureFlags:enable_CrewAI"] = "true",
            ["FeatureFlags:enable_SemanticKernel"] = "true",
            ["FeatureFlags:enable_AutoGen"] = "true",
            ["FeatureFlags:enable_Smolagents"] = "true",
            ["FeatureFlags:enable_AutoGPT"] = "true",
        });

        await sut.EnableADKAsync();
        await sut.EnableLangGraphAsync();
        await sut.EnableCrewAIAsync();
        await sut.EnableSemanticKernelAsync();
        await sut.EnableAutoGenAsync();
        await sut.EnableSmolagentsAsync();
        await sut.EnableAutoGPTAsync();

        sut.EnabledFrameworks.Should().HaveCount(7);
        sut.IsFrameworkEnabled("ADK").Should().BeTrue();
        sut.IsFrameworkEnabled("LangGraph").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI").Should().BeTrue();
        sut.IsFrameworkEnabled("SemanticKernel").Should().BeTrue();
        sut.IsFrameworkEnabled("AutoGen").Should().BeTrue();
        sut.IsFrameworkEnabled("Smolagents").Should().BeTrue();
        sut.IsFrameworkEnabled("AutoGPT").Should().BeTrue();
    }

    /// <summary>
    /// Tests that a base framework and its sub-features can all be enabled together.
    /// </summary>
    [Fact]
    public async Task ADKWithAllSubFeatures_AllFlagsEnabled_AllRegistered()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_ADK_WorkflowAgents"] = "true",
            ["FeatureFlags:enable_ADK_ToolIntegration"] = "true",
            ["FeatureFlags:enable_ADK_Guardrails"] = "true",
            ["FeatureFlags:enable_ADK_Multimodal"] = "true",
        });

        await sut.EnableADKAsync();
        await sut.EnableADKWorkflowAgentsAsync();
        await sut.EnableADKToolIntegrationAsync();
        await sut.EnableADKGuardrailsAsync();
        await sut.EnableADKMultimodalAsync();

        sut.EnabledFrameworks.Should().HaveCount(5);
        sut.IsFrameworkEnabled("ADK").Should().BeTrue();
        sut.IsFrameworkEnabled("ADK.WorkflowAgents").Should().BeTrue();
        sut.IsFrameworkEnabled("ADK.ToolIntegration").Should().BeTrue();
        sut.IsFrameworkEnabled("ADK.Guardrails").Should().BeTrue();
        sut.IsFrameworkEnabled("ADK.Multimodal").Should().BeTrue();
    }

    /// <summary>
    /// Tests that enabling a full framework hierarchy (base + all sub-features)
    /// for CrewAI works correctly.
    /// </summary>
    [Fact]
    public async Task CrewAIWithAllSubFeatures_AllFlagsEnabled_AllRegistered()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_CrewAI"] = "true",
            ["FeatureFlags:enable_CrewAI_Team"] = "true",
            ["FeatureFlags:enable_CrewAI_DynamicPlanning"] = "true",
            ["FeatureFlags:enable_CrewAI_AdaptiveExecution"] = "true",
            ["FeatureFlags:enable_CrewAI_MultiAgent"] = "true",
            ["FeatureFlags:enable_CrewAI_DynamicTaskRouting"] = "true",
            ["FeatureFlags:enable_CrewAI_StatefulWorkflows"] = "true",
        });

        await sut.EnableCrewAIAsync();
        await sut.EnableCrewAITeamAsync();
        await sut.EnableCrewAIDynamicPlanningAsync();
        await sut.EnableCrewAIAdaptiveExecutionAsync();
        await sut.EnableCrewAIMultiAgentAsync();
        await sut.EnableCrewAIDynamicTaskRoutingAsync();
        await sut.EnableCrewAIStatefulWorkflowsAsync();

        sut.EnabledFrameworks.Should().HaveCount(7);
        sut.IsFrameworkEnabled("CrewAI").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.Team").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.DynamicPlanning").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.AdaptiveExecution").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.MultiAgent").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.DynamicTaskRouting").Should().BeTrue();
        sut.IsFrameworkEnabled("CrewAI.StatefulWorkflows").Should().BeTrue();
    }

    // ==================================================================
    // Timestamp Tests
    // ==================================================================

    /// <summary>
    /// Tests that the enablement timestamp is set to a reasonable UTC time.
    /// </summary>
    [Fact]
    public async Task EnableFramework_SetsTimestamp_ToRecentUtcTime()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true"
        });

        var before = DateTimeOffset.UtcNow;
        await sut.EnableADKAsync();
        var after = DateTimeOffset.UtcNow;

        var timestamp = sut.EnabledFrameworks["ADK"];
        timestamp.Should().BeOnOrAfter(before);
        timestamp.Should().BeOnOrBefore(after);
    }

    // ==================================================================
    // Mixed Scenarios
    // ==================================================================

    /// <summary>
    /// Tests that enabling ContinuousLearning and then a base framework both exist together.
    /// </summary>
    [Fact]
    public async Task EnableContinuousLearningAndBaseFramework_BothRegistered()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_LangGraph"] = "true"
        });

        await sut.EnableContinuousLearningAsync();
        await sut.EnableLangGraphAsync();

        sut.EnabledFrameworks.Should().HaveCount(2);
        sut.IsFrameworkEnabled("ContinuousLearning").Should().BeTrue();
        sut.IsFrameworkEnabled("LangGraph").Should().BeTrue();
    }

    /// <summary>
    /// Tests that enabling ContinuousLearning and AdaptModel together works.
    /// </summary>
    [Fact]
    public async Task EnableContinuousLearningAndAdaptModel_BothRegistered()
    {
        var sut = CreateSut();

        await sut.EnableContinuousLearningAsync();
        await sut.AdaptModelAsync();

        sut.EnabledFrameworks.Should().HaveCount(2);
        sut.IsFrameworkEnabled("ContinuousLearning").Should().BeTrue();
        sut.IsFrameworkEnabled("ModelAdaptation").Should().BeTrue();
    }

    /// <summary>
    /// Tests that frameworks from different families can be enabled alongside their sub-features.
    /// </summary>
    [Fact]
    public async Task MixedFrameworkFamilies_AllEnabledCorrectly()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_ADK_Guardrails"] = "true",
            ["FeatureFlags:enable_LangGraph"] = "true",
            ["FeatureFlags:enable_LangGraph_Stateful"] = "true",
            ["FeatureFlags:enable_AutoGPT"] = "true",
            ["FeatureFlags:enable_AutoGPT_Autonomous"] = "true",
        });

        await sut.EnableADKAsync();
        await sut.EnableADKGuardrailsAsync();
        await sut.EnableLangGraphAsync();
        await sut.EnableLangGraphStatefulAsync();
        await sut.EnableAutoGPTAsync();
        await sut.EnableAutoGPTAutonomousAsync();

        sut.EnabledFrameworks.Should().HaveCount(6);
        sut.IsFrameworkEnabled("ADK").Should().BeTrue();
        sut.IsFrameworkEnabled("ADK.Guardrails").Should().BeTrue();
        sut.IsFrameworkEnabled("LangGraph").Should().BeTrue();
        sut.IsFrameworkEnabled("LangGraph.Stateful").Should().BeTrue();
        sut.IsFrameworkEnabled("AutoGPT").Should().BeTrue();
        sut.IsFrameworkEnabled("AutoGPT.Autonomous").Should().BeTrue();
    }

    // ==================================================================
    // Logging Verification
    // ==================================================================

    /// <summary>
    /// Verifies that the logger is invoked when a framework is enabled.
    /// </summary>
    [Fact]
    public async Task EnableFramework_WithLogger_LogsInformationOnEnablement()
    {
        var sut = CreateSut(
            new Dictionary<string, string?> { ["FeatureFlags:enable_ADK"] = "true" },
            _loggerMock.Object);

        await sut.EnableADKAsync();

        // Verify that at least one log call was made at Information level
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that the logger receives a Debug message when the feature flag is disabled.
    /// </summary>
    [Fact]
    public async Task EnableFramework_FlagDisabled_LogsDebugSkipMessage()
    {
        var sut = CreateSut(logger: _loggerMock.Object);

        // All flags are off by default
        await sut.EnableADKAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    /// <summary>
    /// Verifies that a Warning is logged when a sub-feature prerequisite is not met.
    /// </summary>
    [Fact]
    public async Task EnableSubFeature_PrerequisiteNotMet_LogsWarning()
    {
        var sut = CreateSut(
            new Dictionary<string, string?>
            {
                ["FeatureFlags:enable_ADK_Guardrails"] = "true"
            },
            _loggerMock.Object);

        // ADK base is not enabled, so enabling a sub-feature should log a warning
        await sut.EnableADKGuardrailsAsync();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => true),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    // ==================================================================
    // Concurrent Access Safety
    // ==================================================================

    /// <summary>
    /// Tests that enabling multiple base frameworks concurrently does not cause errors
    /// (validates ConcurrentDictionary thread safety).
    /// </summary>
    [Fact]
    public async Task EnableMultipleFrameworksConcurrently_NoExceptions()
    {
        var sut = CreateSut(new Dictionary<string, string?>
        {
            ["FeatureFlags:enable_ADK"] = "true",
            ["FeatureFlags:enable_LangGraph"] = "true",
            ["FeatureFlags:enable_CrewAI"] = "true",
            ["FeatureFlags:enable_SemanticKernel"] = "true",
            ["FeatureFlags:enable_AutoGen"] = "true",
            ["FeatureFlags:enable_Smolagents"] = "true",
            ["FeatureFlags:enable_AutoGPT"] = "true",
        });

        var tasks = new[]
        {
            sut.EnableADKAsync(),
            sut.EnableLangGraphAsync(),
            sut.EnableCrewAIAsync(),
            sut.EnableSemanticKernelAsync(),
            sut.EnableAutoGenAsync(),
            sut.EnableSmolagentsAsync(),
            sut.EnableAutoGPTAsync(),
        };

        var act = () => Task.WhenAll(tasks);

        await act.Should().NotThrowAsync();

        sut.EnabledFrameworks.Should().HaveCount(7);
    }

    // ==================================================================
    // All Flags Disabled — No Frameworks Registered
    // ==================================================================

    /// <summary>
    /// Tests that when all feature flags are disabled, enabling all base frameworks
    /// results in zero registered frameworks.
    /// </summary>
    [Fact]
    public async Task AllBaseFrameworks_AllFlagsDisabled_NoneRegistered()
    {
        var sut = CreateSut();

        await sut.EnableADKAsync();
        await sut.EnableLangGraphAsync();
        await sut.EnableCrewAIAsync();
        await sut.EnableSemanticKernelAsync();
        await sut.EnableAutoGenAsync();
        await sut.EnableSmolagentsAsync();
        await sut.EnableAutoGPTAsync();

        sut.EnabledFrameworks.Should().BeEmpty();
    }
}
