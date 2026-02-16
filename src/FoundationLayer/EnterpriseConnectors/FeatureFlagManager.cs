using Microsoft.Extensions.Configuration;

namespace FoundationLayer.EnterpriseConnectors;

public class FeatureFlagManager
{
    private readonly IConfiguration _configuration;

    public FeatureFlagManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool UseOneLake => _configuration.GetValue<bool>("FeatureFlags:UseOneLake");
    public bool EnableMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_multi_agent");
    public bool EnableDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_dynamic_task_routing");
    public bool EnableStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_stateful_workflows");
    public bool EnableHumanInTheLoop => _configuration.GetValue<bool>("FeatureFlags:enable_human_in_the_loop");
    public bool EnableToolIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_tool_integration");
    public bool EnableMemoryManagement => _configuration.GetValue<bool>("FeatureFlags:enable_memory_management");
    public bool EnableStreaming => _configuration.GetValue<bool>("FeatureFlags:enable_streaming");
    public bool EnableCodeExecution => _configuration.GetValue<bool>("FeatureFlags:enable_code_execution");
    public bool EnableGuardrails => _configuration.GetValue<bool>("FeatureFlags:enable_guardrails");
    public bool EnableEnterpriseIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_enterprise_integration");
    public bool EnableModularSkills => _configuration.GetValue<bool>("FeatureFlags:enable_modular_skills");

    public bool EnableADK => _configuration.GetValue<bool>("FeatureFlags:enable_ADK");
    public bool EnableADKWorkflowAgents => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_WorkflowAgents");
    public bool EnableADKToolIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_ToolIntegration");
    public bool EnableADKGuardrails => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_Guardrails");
    public bool EnableADKMultimodal => _configuration.GetValue<bool>("FeatureFlags:enable_ADK_Multimodal");

    public bool EnableLangGraph => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph");
    public bool EnableLangGraphStateful => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_Stateful");
    public bool EnableLangGraphStreaming => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_Streaming");
    public bool EnableLangGraphHITL => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_HITL");

    public bool EnableCrewAI => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI");
    public bool EnableCrewAITeam => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_Team");
    public bool EnableCrewAIDynamicPlanning => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_DynamicPlanning");
    public bool EnableCrewAIAdaptiveExecution => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_AdaptiveExecution");

    public bool EnableSemanticKernel => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel");
    public bool EnableSemanticKernelMemory => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Memory");
    public bool EnableSemanticKernelSecurity => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Security");
    public bool EnableSemanticKernelAutomation => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_Automation");

    public bool EnableAutoGen => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen");
    public bool EnableAutoGenConversations => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_Conversations");
    public bool EnableAutoGenContext => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_Context");
    public bool EnableAutoGenAPIIntegration => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_APIIntegration");

    public bool EnableSmolagents => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents");
    public bool EnableSmolagentsModular => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_Modular");
    public bool EnableSmolagentsContext => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_Context");

    public bool EnableAutoGPT => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT");
    public bool EnableAutoGPTAutonomous => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_Autonomous");
    public bool EnableAutoGPTMemory => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_Memory");
    public bool EnableAutoGPTInternetAccess => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_InternetAccess");

    public bool EnableLangGraphStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_StatefulWorkflows");
    public bool EnableLangGraphStreamingWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_StreamingWorkflows");
    public bool EnableLangGraphHITLWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph_HITLWorkflows");

    public bool EnableCrewAIMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_MultiAgent");
    public bool EnableCrewAIDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_DynamicTaskRouting");
    public bool EnableCrewAIStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI_StatefulWorkflows");

    public bool EnableSemanticKernelMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_MultiAgent");
    public bool EnableSemanticKernelDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_DynamicTaskRouting");
    public bool EnableSemanticKernelStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel_StatefulWorkflows");

    public bool EnableAutoGenMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_MultiAgent");
    public bool EnableAutoGenDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_DynamicTaskRouting");
    public bool EnableAutoGenStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen_StatefulWorkflows");

    public bool EnableSmolagentsMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_MultiAgent");
    public bool EnableSmolagentsDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_DynamicTaskRouting");
    public bool EnableSmolagentsStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents_StatefulWorkflows");

    public bool EnableAutoGPTMultiAgent => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_MultiAgent");
    public bool EnableAutoGPTDynamicTaskRouting => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_DynamicTaskRouting");
    public bool EnableAutoGPTStatefulWorkflows => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT_StatefulWorkflows");
}
