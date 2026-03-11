namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Request payload for generating multiple decision options for a given situation.
/// </summary>
public class OptionsGenerationRequest
{
    /// <summary>
    /// Gets or sets the situation description for which to generate options.
    /// </summary>
    public string Situation { get; set; } = string.Empty;

    /// <summary>
    /// Desired number of options to generate. Defaults to 3.
    /// </summary>
    public int NumberOfOptions { get; set; } = 3;
}
