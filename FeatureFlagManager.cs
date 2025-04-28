using Microsoft.Extensions.Configuration;

public class FeatureFlagManager
{
    private readonly IConfiguration _configuration;

    public FeatureFlagManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool UseOneLake => _configuration.GetValue<bool>("FeatureFlags:UseOneLake");
    public bool EnableADK => _configuration.GetValue<bool>("FeatureFlags:enable_ADK");
    public bool EnableLangGraph => _configuration.GetValue<bool>("FeatureFlags:enable_LangGraph");
    public bool EnableCrewAI => _configuration.GetValue<bool>("FeatureFlags:enable_CrewAI");
    public bool EnableSemanticKernel => _configuration.GetValue<bool>("FeatureFlags:enable_SemanticKernel");
    public bool EnableAutoGen => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGen");
    public bool EnableSmolagents => _configuration.GetValue<bool>("FeatureFlags:enable_Smolagents");
    public bool EnableAutoGPT => _configuration.GetValue<bool>("FeatureFlags:enable_AutoGPT");
}
