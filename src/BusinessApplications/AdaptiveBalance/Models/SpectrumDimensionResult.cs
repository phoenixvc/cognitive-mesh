namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the current position of a single spectrum dimension,
/// including confidence bounds and rationale.
/// </summary>
public class SpectrumDimensionResult
{
    /// <summary>
    /// The name of the spectrum dimension (e.g., "Profit", "Risk", "Agreeableness").
    /// </summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>
    /// The current value of this dimension on the spectrum (0.0 to 1.0).
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// The lower confidence bound for the dimension value (0.0 to 1.0).
    /// </summary>
    public double LowerBound { get; set; }

    /// <summary>
    /// The upper confidence bound for the dimension value (0.0 to 1.0).
    /// </summary>
    public double UpperBound { get; set; }

    /// <summary>
    /// An explanation of why the dimension is at its current position.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;
}
