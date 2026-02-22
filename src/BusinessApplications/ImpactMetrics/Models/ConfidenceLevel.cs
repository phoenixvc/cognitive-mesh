namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Indicates the statistical confidence level of a calculated metric
/// based on the volume of underlying data.
/// </summary>
public enum ConfidenceLevel
{
    /// <summary>
    /// Low confidence — fewer than 10 survey responses or behavioral signals.
    /// </summary>
    Low,

    /// <summary>
    /// Medium confidence — between 10 and 50 survey responses or behavioral signals.
    /// </summary>
    Medium,

    /// <summary>
    /// High confidence — more than 50 survey responses or behavioral signals.
    /// </summary>
    High
}
