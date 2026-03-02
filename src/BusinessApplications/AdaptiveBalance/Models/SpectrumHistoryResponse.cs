namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the history of changes for a specific spectrum dimension.
/// </summary>
public class SpectrumHistoryResponse
{
    /// <summary>
    /// The name of the spectrum dimension.
    /// </summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>
    /// The historical entries for this dimension, ordered by time.
    /// </summary>
    public List<SpectrumHistoryEntry> History { get; set; } = new();
}
