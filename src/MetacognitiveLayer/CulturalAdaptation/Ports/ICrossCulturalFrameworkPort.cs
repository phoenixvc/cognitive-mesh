using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.CulturalAdaptation.Ports.Models
{
    /// <summary>
    /// Represents a cultural profile based on Geert Hofstede's 6-D model.
    /// Each dimension is scored, typically on a scale of 0 to 100.
    /// </summary>
    public class CulturalProfile
    {
        /// <summary>
        /// **Power Distance Index (PDI):** The degree to which less powerful members of a society accept and expect that power is distributed unequally.
        /// High PDI: Hierarchical order is accepted. Low PDI: People strive to equalize power distribution.
        /// </summary>
        public int PowerDistance { get; set; }

        /// <summary>
        /// **Individualism vs. Collectivism (IDV):** The degree to which people are integrated into groups.
        /// High IDV (Individualism): Loosely-knit social framework. Low IDV (Collectivism): Tightly-knit framework.
        /// </summary>
        public int Individualism { get; set; }

        /// <summary>
        /// **Masculinity vs. Femininity (MAS):** The distribution of emotional roles between the genders.
        /// High MAS (Masculinity): Preference for achievement, heroism, assertiveness, and material rewards.
        /// Low MAS (Femininity): Preference for cooperation, modesty, caring for the weak, and quality of life.
        /// </summary>
        public int Masculinity { get; set; }

        /// <summary>
        /// **Uncertainty Avoidance Index (UAI):** A society's tolerance for ambiguity and uncertainty.
        /// High UAI: Rigid codes of belief and behavior. Low UAI: More relaxed attitude, practice counts more than principles.
        /// </summary>
        public int UncertaintyAvoidance { get; set; }

        /// <summary>
        /// **Long-Term Orientation vs. Short-Term Orientation (LTO):** How every society has to maintain some links with its own past while dealing with the challenges of the present and future.
        /// High LTO: Pragmatic approach, encourages thrift and education as ways to prepare for the future.
        /// Low LTO: Prefers to maintain time-honored traditions and norms while viewing societal change with suspicion.
        /// </summary>
        public int LongTermOrientation { get; set; }

        /// <summary>
        /// **Indulgence vs. Restraint (IVR):** The extent to which people try to control their desires and impulses.
        /// High IVR (Indulgence): Allows relatively free gratification of basic and natural human drives related to enjoying life.
        /// Low IVR (Restraint): Suppresses gratification of needs and regulates it by means of strict social norms.
        /// </summary>
        public int Indulgence { get; set; }
    }

    /// <summary>
    /// A request to assess the cultural profile for a given context.
    /// </summary>
    public class CulturalProfileRequest
    {
        /// <summary>
        /// The user for whom the profile is being requested.
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// The locale (e.g., "en-US", "de-DE") to use for determining the cultural context.
        /// </summary>
        public string Locale { get; set; }
    }

    /// <summary>
    /// The response containing the assessed cultural profile.
    /// </summary>
    public class CulturalProfileResponse
    {
        public bool IsSuccess { get; set; }
        public CulturalProfile Profile { get; set; }
        public string DataSource { get; set; } // e.g., "Hofstede Insights - Country Comparison Tool"
    }

    /// <summary>
    /// A request for UI adaptation recommendations based on a cultural profile.
    /// </summary>
    public class UIAdaptationRequest
    {
        public CulturalProfile Profile { get; set; }
        /// <summary>
        /// The specific UI component being adapted (e.g., "Dashboard", "NotificationBanner", "ConsentForm").
        /// </summary>
        public string UIComponentContext { get; set; }
    }

    /// <summary>
    /// A response containing a set of UI adaptation recommendations.
    /// </summary>
    public class UIAdaptationResponse
    {
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// A request for consent flow customization recommendations.
    /// </summary>
    public class ConsentFlowCustomizationRequest
    {
        public CulturalProfile Profile { get; set; }
    }

    /// <summary>
    /// A response with recommendations for tailoring a consent flow.
    /// </summary>
    public class ConsentFlowCustomizationResponse
    {
        public string RecommendedPattern { get; set; } // e.g., "ExplicitOptIn", "DetailedExplanation", "Minimalist"
        public string Rationale { get; set; }
    }

    /// <summary>
    /// A request for authority model adjustments based on cultural context.
    /// </summary>
    public class AuthorityModelAdjustmentRequest
    {
        public CulturalProfile Profile { get; set; }
    }

    /// <summary>
    /// A response with recommendations for adjusting the presentation of authority.
    /// </summary>
    public class AuthorityModelAdjustmentResponse
    {
        public List<string> Recommendations { get; set; } = new();
    }
}

namespace CognitiveMesh.MetacognitiveLayer.CulturalAdaptation.Ports
{
    using CognitiveMesh.MetacognitiveLayer.CulturalAdaptation.Ports.Models;

    /// <summary>
    /// Defines the contract for the Cross-Cultural Framework Port. This port provides services
    /// for adapting the Cognitive Mesh's UI/UX and interaction patterns based on Geert Hofstede's
    /// cultural dimensions theory. It ensures that human-AI collaboration is not only ethical
    /// and legally compliant but also culturally aware and respectful.
    /// </summary>
    public interface ICrossCulturalFrameworkPort
    {
        /// <summary>
        /// Assesses and retrieves the cultural profile for a given user or locale based on Hofstede's 6-D model.
        /// </summary>
        /// <param name="request">The request containing the user or locale context.</param>
        /// <returns>A response containing the structured cultural profile.</returns>
        Task<CulturalProfileResponse> GetCulturalProfileAsync(CulturalProfileRequest request);

        /// <summary>
        /// Generates actionable recommendations for adapting UI elements based on a cultural profile.
        /// For example, it might recommend more direct language for low-context cultures or more visual,
        /// relationship-oriented designs for collectivist cultures.
        /// </summary>
        /// <param name="request">The request containing the cultural profile and UI component context.</param>
        /// <returns>A list of specific UI adaptation recommendations.</returns>
        Task<UIAdaptationResponse> GetUIAdaptationRecommendationsAsync(UIAdaptationRequest request);

        /// <summary>
        /// Recommends customizations for consent flows to align with cultural expectations.
        /// For example, it might recommend explicit, detailed opt-in forms for cultures with high
        /// uncertainty avoidance, versus simpler, trust-based flows for others.
        /// </summary>
        /// <param name="request">The request containing the cultural profile.</param>
        /// <returns>A response with a recommended consent pattern and the rationale behind it.</returns>
        Task<ConsentFlowCustomizationResponse> GetConsentFlowCustomizationAsync(ConsentFlowCustomizationRequest request);

        /// <summary>
        /// Recommends adjustments to how authority and hierarchy are presented in the UI.
        /// For example, in high power-distance cultures, it might recommend using formal titles and clear
        /// hierarchical displays, whereas in low power-distance cultures, it might suggest emphasizing
        /// collaboration and flat team structures.
        /// </summary>
        /// <param name="request">The request containing the cultural profile.</param>
        /// <returns>A list of recommendations for adjusting authority models.</returns>
        Task<AuthorityModelAdjustmentResponse> GetAuthorityModelAdjustmentsAsync(AuthorityModelAdjustmentRequest request);
    }
}
