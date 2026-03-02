namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the response after applying a manual override to a spectrum dimension.
/// </summary>
public class OverrideResponse
{
    /// <summary>
    /// The unique identifier assigned to this override operation.
    /// </summary>
    public Guid OverrideId { get; set; }

    /// <summary>
    /// The name of the dimension that was overridden.
    /// </summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>
    /// The previous value of the dimension before the override.
    /// </summary>
    public double OldValue { get; set; }

    /// <summary>
    /// The new value of the dimension after the override.
    /// </summary>
    public double NewValue { get; set; }

    /// <summary>
    /// The timestamp when the override was applied.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// A human-readable message describing the override result.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
