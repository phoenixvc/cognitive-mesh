namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents a request to submit learning evidence for the continuous improvement loop.
/// </summary>
public class LearningEvidenceRequest
{
    /// <summary>
    /// The type of pattern identified (e.g., "Bias", "Drift", "Success").
    /// </summary>
    public string PatternType { get; set; } = string.Empty;

    /// <summary>
    /// A description of the learning evidence.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The evidence supporting the learning, such as data or observations.
    /// </summary>
    public string Evidence { get; set; } = string.Empty;

    /// <summary>
    /// The observed outcome associated with this evidence.
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the agent that produced this learning evidence.
    /// </summary>
    public string SourceAgentId { get; set; } = string.Empty;
}
