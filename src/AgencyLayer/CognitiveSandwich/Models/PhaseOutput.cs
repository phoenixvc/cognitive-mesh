namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the output produced by executing a phase in a Cognitive Sandwich process.
/// </summary>
public class PhaseOutput
{
    /// <summary>
    /// Identifier of the phase that produced this output.
    /// </summary>
    public string PhaseId { get; set; } = string.Empty;

    /// <summary>
    /// Key-value data produced by the phase execution.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Optional summary of the phase output for human review.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Timestamp when the output was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
