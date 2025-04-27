using Microsoft.Extensions.Configuration;

public class FeatureFlagManager
{
    private readonly IConfiguration _configuration;

    public FeatureFlagManager(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool UseOneLake => _configuration.GetValue<bool>("FeatureFlags:UseOneLake");
}
