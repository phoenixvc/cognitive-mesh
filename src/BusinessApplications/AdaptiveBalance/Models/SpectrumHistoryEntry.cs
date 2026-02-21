namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents a single historical entry for a spectrum dimension change.
/// </summary>
public class SpectrumHistoryEntry
{
    /// <summary>
    /// The value of the dimension at this point in time (0.0 to 1.0).
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// The rationale for the value at this point in time.
    /// </summary>
    public string Rationale { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when this value was recorded.
    /// </summary>
    public DateTimeOffset RecordedAt { get; set; }
}
