namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the response containing the current adaptive balance positions
/// for all spectrum dimensions with confidence metrics.
/// </summary>
public class BalanceResponse
{
    /// <summary>
    /// The current position for each spectrum dimension.
    /// </summary>
    public List<SpectrumDimensionResult> Dimensions { get; set; } = new();

    /// <summary>
    /// The overall confidence level of the balance calculation (0.0 to 1.0).
    /// </summary>
    public double OverallConfidence { get; set; }

    /// <summary>
    /// The timestamp when this balance response was generated.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; set; }
}
