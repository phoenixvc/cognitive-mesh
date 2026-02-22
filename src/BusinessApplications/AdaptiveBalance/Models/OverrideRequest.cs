namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents a request to manually override the position of a spectrum dimension.
/// </summary>
public class OverrideRequest
{
    /// <summary>
    /// The name of the spectrum dimension to override.
    /// </summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>
    /// The new value for the dimension (must be between 0.0 and 1.0).
    /// </summary>
    public double NewValue { get; set; }

    /// <summary>
    /// The rationale for the override.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>
    /// The identifier of the person performing the override.
    /// </summary>
    public string OverriddenBy { get; set; } = string.Empty;
}
