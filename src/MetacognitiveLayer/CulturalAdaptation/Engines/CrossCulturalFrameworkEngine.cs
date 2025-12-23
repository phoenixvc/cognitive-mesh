using System.Collections.Concurrent;
using System.Globalization;
using MetacognitiveLayer.CulturalAdaptation.Ports;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.CulturalAdaptation.Engines
{
    /// <summary>
    /// Implements the core logic for adapting UI/UX and interaction patterns based on Geert Hofstede's
    /// 6-D model of cultural dimensions. This engine provides enterprise-grade cultural intelligence
    /// to ensure that the Cognitive Mesh is respectful, effective, and compliant across diverse global contexts.
    /// </summary>
    public class CrossCulturalFrameworkEngine : ICrossCulturalFrameworkPort
    {
        private readonly ILogger<CrossCulturalFrameworkEngine> _logger;

        // A static, in-memory database of Hofstede's 6-D model scores for various countries.
        // In a real-world enterprise system, this would be loaded from a configuration service or a dedicated database.
        // Source: Hofstede Insights - Country Comparison Tool (https://www.hofstede-insights.com/country-comparison/)
        private static readonly ConcurrentDictionary<string, CulturalProfile> _culturalDimensionsData = new()
        {
            ["US"] = new() { PowerDistance = 40, Individualism = 91, Masculinity = 62, UncertaintyAvoidance = 46, LongTermOrientation = 26, Indulgence = 68 },
            ["DE"] = new() { PowerDistance = 35, Individualism = 67, Masculinity = 66, UncertaintyAvoidance = 65, LongTermOrientation = 83, Indulgence = 40 },
            ["JP"] = new() { PowerDistance = 54, Individualism = 46, Masculinity = 95, UncertaintyAvoidance = 92, LongTermOrientation = 88, Indulgence = 42 },
            ["BR"] = new() { PowerDistance = 69, Individualism = 38, Masculinity = 49, UncertaintyAvoidance = 76, LongTermOrientation = 44, Indulgence = 59 },
            ["SE"] = new() { PowerDistance = 31, Individualism = 71, Masculinity = 5,  UncertaintyAvoidance = 29, LongTermOrientation = 53, Indulgence = 78 },
            ["CN"] = new() { PowerDistance = 80, Individualism = 20, Masculinity = 66, UncertaintyAvoidance = 30, LongTermOrientation = 87, Indulgence = 24 },
            // A default "global average" profile for fallbacks
            ["default"] = new() { PowerDistance = 55, Individualism = 50, Masculinity = 50, UncertaintyAvoidance = 60, LongTermOrientation = 50, Indulgence = 50 }
        };

        public CrossCulturalFrameworkEngine(ILogger<CrossCulturalFrameworkEngine> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task<CulturalProfileResponse> GetCulturalProfileAsync(CulturalProfileRequest request)
        {
            _logger.LogInformation("Retrieving cultural profile for locale '{Locale}'.", request.Locale);

            string countryCode = "default";
            string dataSource = "Cognitive Mesh Global Average";

            try
            {
                var regionInfo = new RegionInfo(request.Locale);
                string twoLetterISOCode = regionInfo.TwoLetterISORegionName.ToUpper();
                if (_culturalDimensionsData.ContainsKey(twoLetterISOCode))
                {
                    countryCode = twoLetterISOCode;
                    dataSource = "Hofstede Insights Country Data";
                    _logger.LogDebug("Found matching cultural profile for country code '{CountryCode}'.", countryCode);
                }
                else
                {
                    _logger.LogWarning("No specific cultural profile found for locale '{Locale}'. Falling back to default profile.", request.Locale);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse locale '{Locale}'. Falling back to default profile.", request.Locale);
            }

            var profile = _culturalDimensionsData[countryCode];
            var response = new CulturalProfileResponse
            {
                IsSuccess = true,
                Profile = profile,
                DataSource = dataSource
            };

            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<UIAdaptationResponse> GetUIAdaptationRecommendationsAsync(UIAdaptationRequest request)
        {
            _logger.LogInformation("Generating UI adaptation recommendations for component '{UIComponentContext}'.", request.UIComponentContext);
            var recommendations = new List<string>();
            var profile = request.Profile;

            // Power Distance (PDI)
            if (profile.PowerDistance > 65)
                recommendations.Add("Use formal language and clear hierarchical structures. Clearly label roles like 'Manager' or 'Admin'.");
            else if (profile.PowerDistance < 40)
                recommendations.Add("Emphasize collaboration and flat team structures. Use informal language and first names.");

            // Individualism (IDV)
            if (profile.Individualism > 70)
                recommendations.Add("Focus on personal achievement and benefits. Use 'You' and 'Your' pronouns. Highlight individual performance.");
            else if (profile.Individualism < 40)
                recommendations.Add("Focus on group harmony and collective goals. Use 'We' and 'Our' pronouns. Highlight team performance.");

            // Masculinity (MAS)
            if (profile.Masculinity > 65)
                recommendations.Add("Use competitive language (e.g., 'win', 'succeed', 'leader'). Emphasize success and achievement.");
            else if (profile.Masculinity < 40)
                recommendations.Add("Use cooperative language (e.g., 'support', 'well-being', 'quality'). Emphasize work-life balance and consensus.");

            // Uncertainty Avoidance (UAI)
            if (profile.UncertaintyAvoidance > 70)
                recommendations.Add("Provide detailed instructions, clear rules, and guarantees. Minimize ambiguity and offer structured choices.");
            else if (profile.UncertaintyAvoidance < 40)
                recommendations.Add("Offer flexibility and fewer rules. Emphasize emergent outcomes and adaptability.");

            // Long-Term Orientation (LTO)
            if (profile.LongTermOrientation > 65)
                recommendations.Add("Emphasize future benefits, long-term value, and patience. Frame decisions in the context of future growth.");
            else if (profile.LongTermOrientation < 40)
                recommendations.Add("Focus on immediate benefits, tradition, and quick results. Frame decisions in the context of short-term gains.");

            // Indulgence (IVR)
            if (profile.Indulgence > 60)
                recommendations.Add("Use optimistic, friendly, and informal language. Focus on enjoyment, personal happiness, and freedom.");
            else if (profile.Indulgence < 40)
                recommendations.Add("Use more formal, restrained language. Focus on duty, control, and social order.");

            _logger.LogInformation("Generated {Count} UI adaptation recommendations.", recommendations.Count);
            return Task.FromResult(new UIAdaptationResponse { Recommendations = recommendations });
        }

        /// <inheritdoc />
        public Task<ConsentFlowCustomizationResponse> GetConsentFlowCustomizationAsync(ConsentFlowCustomizationRequest request)
        {
            _logger.LogInformation("Generating consent flow customization based on cultural profile.");
            var profile = request.Profile;
            var response = new ConsentFlowCustomizationResponse();

            if (profile.UncertaintyAvoidance > 70)
            {
                response.RecommendedPattern = "DetailedExplicitOptIn";
                response.Rationale = $"High Uncertainty Avoidance ({profile.UncertaintyAvoidance}) suggests a preference for clarity and explicit rules. A detailed form with specific checkboxes for each data use case will build the most trust.";
            }
            else if (profile.Individualism < 40)
            {
                response.RecommendedPattern = "CommunityBenefitFocused";
                response.Rationale = $"Low Individualism ({profile.Individualism}) indicates a collectivist culture. Frame consent in terms of how data sharing benefits the user's group or community.";
            }
            else if (profile.PowerDistance < 40)
            {
                response.RecommendedPattern = "UserInControl";
                response.Rationale = $"Low Power Distance ({profile.PowerDistance}) suggests an emphasis on equality. The consent flow should highlight the user's control and ability to change settings at any time.";
            }
            else
            {
                response.RecommendedPattern = "StandardOptIn";
                response.Rationale = "The cultural profile does not strongly indicate a need for a highly specialized consent pattern. A standard, clear opt-in flow is recommended.";
            }

            _logger.LogInformation("Recommended consent flow pattern: '{Pattern}'. Rationale: {Rationale}", response.RecommendedPattern, response.Rationale);
            return Task.FromResult(response);
        }

        /// <inheritdoc />
        public Task<AuthorityModelAdjustmentResponse> GetAuthorityModelAdjustmentsAsync(AuthorityModelAdjustmentRequest request)
        {
            _logger.LogInformation("Generating authority model adjustments based on cultural profile.");
            var recommendations = new List<string>();
            var profile = request.Profile;

            if (profile.PowerDistance > 65)
            {
                recommendations.Add("Clearly display hierarchical relationships (e.g., organizational charts).");
                recommendations.Add("Use formal titles for roles (e.g., 'Project Manager', 'Lead Engineer').");
                recommendations.Add("Structure approval workflows with clear, top-down authority.");
            }
            else if (profile.PowerDistance < 40)
            {
                recommendations.Add("Emphasize collaborative team structures and flat hierarchies.");
                recommendations.Add("Use role descriptions that focus on contribution rather than status.");
                recommendations.Add("Design approval workflows that favor consensus and peer review.");
            }

            if (profile.Masculinity > 65)
            {
                recommendations.Add("Highlight decisive leadership and clear decision-makers.");
            }
            else if (profile.Masculinity < 40)
            {
                recommendations.Add("Showcase leaders who facilitate consensus and support the team.");
            }

            _logger.LogInformation("Generated {Count} authority model adjustment recommendations.", recommendations.Count);
            return Task.FromResult(new AuthorityModelAdjustmentResponse { Recommendations = recommendations });
        }
    }
}
