namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Response payload containing explored scenarios and strategic recommendations.
/// </summary>
public class ScenarioExplorationResponse
{
    /// <summary>
    /// Gets or sets the original situation description.
    /// </summary>
    public string Situation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the analyzed scenarios.
    /// </summary>
    public List<ScenarioAnalysis> Scenarios { get; set; } = new();

    /// <summary>
    /// Gets or sets the strategic recommendations based on the scenario analysis.
    /// </summary>
    public string StrategicRecommendations { get; set; } = string.Empty;
}

/// <summary>
/// Represents the analysis of a single future scenario.
/// </summary>
public class ScenarioAnalysis
{
    /// <summary>
    /// Gets or sets the scenario description.
    /// </summary>
    public string Scenario { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the implications of the scenario.
    /// </summary>
    public string Implications { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the early indicators that would suggest this scenario is becoming likely.
    /// </summary>
    public string EarlyIndicators { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the recommended preparations for this scenario.
    /// </summary>
    public string RecommendedPreparations { get; set; } = string.Empty;
}
