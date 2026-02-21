using Microsoft.Extensions.Configuration;

namespace FoundationLayer.EnterpriseConnectors;

/// <summary>
/// Provides strongly-typed access to feature flags from the application configuration.
/// Each property maps to a "FeatureFlags:*" configuration key.
/// </summary>
public class FeatureFlagManager
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureFlagManager"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration containing feature flag settings.</param>
    public FeatureFlagManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>Gets whether OneLake integration is enabled.</summary>
    public bool UseOneLake => _configuration.GetValue<bool>("FeatureFlags:UseOneLake");
    /// <summary>Gets whether multi-agent orchestration is enabled.</summary>
    public bool EnableMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_multi_agent");
    /// <summary>Gets whether dynamic task routing is enabled.</summary>
    public bool EnableDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_dynamic_task_routing");
    /// <summary>Gets whether stateful workflows are enabled.</summary>
    public bool EnableStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_stateful_workflows");
    /// <summary>Gets whether human-in-the-loop workflows are enabled.</summary>
    public bool EnableHumanInTheLoop => _configuration.GetValue<bool>("FeatureFlags:enable_human_in_the_loop");
    /// <summary>Gets whether tool integration is enabled.</summary>
    public bool EnableToolIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_tool_integration");
    /// <summary>Gets whether memory management is enabled.</summary>
    public bool EnableMemoryManagement => _configuration.GetValue<bool>("FeatureFlags:enable_memory_management");
    /// <summary>Gets whether streaming is enabled.</summary>
    public bool EnableStreaming => _configuration.GetValue<bool>("FeatureFlags:enable_streaming");
    /// <summary>Gets whether code execution is enabled.</summary>
    public bool EnableCodeExecution => _configuration.GetValue<bool>("FeatureFlags:enable_code_execution");
    /// <summary>Gets whether guardrails are enabled.</summary>
    public bool EnableGuardrails => _configuration.GetValue<bool>("FeatureFlags:enable_guardrails");
    /// <summary>Gets whether enterprise integration is enabled.</summary>
    public bool EnableEnterpriseIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_enterprise_integration");
    /// <summary>Gets whether modular skills are enabled.</summary>
    public bool EnableModularSkills => _configuration.GetValue<bool>("FeatureFlags:enable_modular_skills");

    /// <summary>Gets whether ADK (Agent Development Kit) is enabled.</summary>
    public bool EnableADK => _configuration.GetValue<bool>("FeatureFlags:enable_ADK");
    /// <summary>Gets whether ADK workflow agents are enabled.</summary>
    public bool EnableADKWorkflowAgents => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_WorkflowAgents");
    /// <summary>Gets whether ADK tool integration is enabled.</summary>
    public bool EnableADKToolIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_ToolIntegration");
    /// <summary>Gets whether ADK guardrails are enabled.</summary>
    public bool EnableADKGuardrails => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_Guardrails");
    /// <summary>Gets whether ADK multimodal support is enabled.</summary>
    public bool EnableADKMultimodal => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_Multimodal");

    /// <summary>Gets whether LangGraph integration is enabled.</summary>
    public bool EnableLangGraph => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph");
    /// <summary>Gets whether LangGraph stateful mode is enabled.</summary>
    public bool EnableLangGraphStateful => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_Stateful");
    /// <summary>Gets whether LangGraph streaming mode is enabled.</summary>
    public bool EnableLangGraphStreaming => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_Streaming");
    /// <summary>Gets whether LangGraph human-in-the-loop is enabled.</summary>
    public bool EnableLangGraphHITL => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_HITL");

    /// <summary>Gets whether CrewAI integration is enabled.</summary>
    public bool EnableCrewAI => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI");
    /// <summary>Gets whether CrewAI team mode is enabled.</summary>
    public bool EnableCrewAITeam => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_Team");
    /// <summary>Gets whether CrewAI dynamic planning is enabled.</summary>
    public bool EnableCrewAIDynamicPlanning => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_DynamicPlanning");
    /// <summary>Gets whether CrewAI adaptive execution is enabled.</summary>
    public bool EnableCrewAIAdaptiveExecution => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_AdaptiveExecution");

    /// <summary>Gets whether Semantic Kernel integration is enabled.</summary>
    public bool EnableSemanticKernel => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel");
    /// <summary>Gets whether Semantic Kernel memory features are enabled.</summary>
    public bool EnableSemanticKernelMemory => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Memory");
    /// <summary>Gets whether Semantic Kernel security features are enabled.</summary>
    public bool EnableSemanticKernelSecurity => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Security");
    /// <summary>Gets whether Semantic Kernel automation features are enabled.</summary>
    public bool EnableSemanticKernelAutomation => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Automation");

    /// <summary>Gets whether AutoGen integration is enabled.</summary>
    public bool EnableAutoGen => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen");
    /// <summary>Gets whether AutoGen conversations are enabled.</summary>
    public bool EnableAutoGenConversations => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_Conversations");
    /// <summary>Gets whether AutoGen context management is enabled.</summary>
    public bool EnableAutoGenContext => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_Context");
    /// <summary>Gets whether AutoGen API integration is enabled.</summary>
    public bool EnableAutoGenAPIIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_APIIntegration");

    /// <summary>Gets whether Smolagents integration is enabled.</summary>
    public bool EnableSmolagents => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents");
    /// <summary>Gets whether Smolagents modular mode is enabled.</summary>
    public bool EnableSmolagentsModular => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_Modular");
    /// <summary>Gets whether Smolagents context management is enabled.</summary>
    public bool EnableSmolagentsContext => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_Context");

    /// <summary>Gets whether AutoGPT integration is enabled.</summary>
    public bool EnableAutoGPT => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT");
    /// <summary>Gets whether AutoGPT autonomous mode is enabled.</summary>
    public bool EnableAutoGPTAutonomous => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_Autonomous");
    /// <summary>Gets whether AutoGPT memory features are enabled.</summary>
    public bool EnableAutoGPTMemory => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_Memory");
    /// <summary>Gets whether AutoGPT internet access is enabled.</summary>
    public bool EnableAutoGPTInternetAccess => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_InternetAccess");

    /// <summary>Gets whether LangGraph stateful workflows are enabled.</summary>
    public bool EnableLangGraphStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_StatefulWorkflows");
    /// <summary>Gets whether LangGraph streaming workflows are enabled.</summary>
    public bool EnableLangGraphStreamingWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_StreamingWorkflows");
    /// <summary>Gets whether LangGraph HITL workflows are enabled.</summary>
    public bool EnableLangGraphHITLWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_HITLWorkflows");

    /// <summary>Gets whether CrewAI multi-agent mode is enabled.</summary>
    public bool EnableCrewAIMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_MultiAgent");
    /// <summary>Gets whether CrewAI dynamic task routing is enabled.</summary>
    public bool EnableCrewAIDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_DynamicTaskRouting");
    /// <summary>Gets whether CrewAI stateful workflows are enabled.</summary>
    public bool EnableCrewAIStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_StatefulWorkflows");

    /// <summary>Gets whether Semantic Kernel multi-agent mode is enabled.</summary>
    public bool EnableSemanticKernelMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_MultiAgent");
    /// <summary>Gets whether Semantic Kernel dynamic task routing is enabled.</summary>
    public bool EnableSemanticKernelDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_DynamicTaskRouting");
    /// <summary>Gets whether Semantic Kernel stateful workflows are enabled.</summary>
    public bool EnableSemanticKernelStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_StatefulWorkflows");

    /// <summary>Gets whether AutoGen multi-agent mode is enabled.</summary>
    public bool EnableAutoGenMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_MultiAgent");
    /// <summary>Gets whether AutoGen dynamic task routing is enabled.</summary>
    public bool EnableAutoGenDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_DynamicTaskRouting");
    /// <summary>Gets whether AutoGen stateful workflows are enabled.</summary>
    public bool EnableAutoGenStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_StatefulWorkflows");

    /// <summary>Gets whether Smolagents multi-agent mode is enabled.</summary>
    public bool EnableSmolagentsMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_MultiAgent");
    /// <summary>Gets whether Smolagents dynamic task routing is enabled.</summary>
    public bool EnableSmolagentsDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_DynamicTaskRouting");
    /// <summary>Gets whether Smolagents stateful workflows are enabled.</summary>
    public bool EnableSmolagentsStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_StatefulWorkflows");

    /// <summary>Gets whether AutoGPT multi-agent mode is enabled.</summary>
    public bool EnableAutoGPTMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_MultiAgent");
    /// <summary>Gets whether AutoGPT dynamic task routing is enabled.</summary>
    public bool EnableAutoGPTDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_DynamicTaskRouting");
    /// <summary>Gets whether AutoGPT stateful workflows are enabled.</summary>
    public bool EnableAutoGPTStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_StatefulWorkflows");
}
