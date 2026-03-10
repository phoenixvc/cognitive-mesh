namespace CognitiveMesh.BusinessApplications.DecisionSupport.Models;

/// <summary>
/// Request payload for analyzing a situation from multiple perspectives.
/// </summary>
public class SituationAnalysisRequest
{
    /// <summary>
    /// Gets or sets the situation description to analyze.
    /// </summary>
    public string Situation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional list of perspectives to consider.
    /// When null, defaults to analytical, critical, creative, and practical.
    /// </summary>
    public List<string>? Perspectives { get; set; }

    /// <summary>
    /// Returns the effective perspectives, applying defaults if none were specified.
    /// </summary>
    public List<string> GetEffectivePerspectives() =>
        Perspectives is { Count: > 0 }
            ? Perspectives
            : new List<string> { "analytical", "critical", "creative", "practical" };
}
