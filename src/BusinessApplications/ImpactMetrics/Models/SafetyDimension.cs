namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Enumerates the dimensions measured as part of a psychological safety assessment
/// for AI adoption within a team.
/// </summary>
public enum SafetyDimension
{
    /// <summary>
    /// Measures the degree of trust team members place in AI systems.
    /// </summary>
    TrustInAI,

    /// <summary>
    /// Measures the degree to which team members fear being replaced by AI.
    /// </summary>
    FearOfReplacement,

    /// <summary>
    /// Measures how comfortable team members are with automated processes.
    /// </summary>
    ComfortWithAutomation,

    /// <summary>
    /// Measures team members' willingness to experiment with new AI tools and workflows.
    /// </summary>
    WillingnessToExperiment,

    /// <summary>
    /// Measures how transparent team members perceive AI decision-making to be.
    /// </summary>
    TransparencyPerception,

    /// <summary>
    /// Measures the tolerance for errors made by AI systems.
    /// </summary>
    ErrorTolerance
}
