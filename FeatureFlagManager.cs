using Microsoft.Extensions.Configuration;

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
}
