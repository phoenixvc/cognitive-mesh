namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Request payload for exploring future scenarios for a given situation.
/// </summary>
public class ScenarioExplorationRequest
{
    /// <summary>
    /// Gets or sets the situation description to explore scenarios for.
    /// </summary>
    public string Situation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the desired number of scenarios to generate. Defaults to 3.
    /// </summary>
    public int NumberOfScenarios { get; set; } = 3;

    /// <summary>
    /// Gets or sets the key uncertainties to consider when generating scenarios.
    /// </summary>
    public List<string> KeyUncertainties { get; set; } = new();
}
