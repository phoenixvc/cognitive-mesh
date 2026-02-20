namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents a single reflexion (self-evaluation) result entry.
/// </summary>
public class ReflexionStatusEntry
{
    /// <summary>
    /// The unique identifier for this reflexion result.
    /// </summary>
    public Guid ResultId { get; set; }

    /// <summary>
    /// Whether this result was identified as a hallucination.
    /// </summary>
    public bool IsHallucination { get; set; }

    /// <summary>
    /// The confidence level of the reflexion evaluation (0.0 to 1.0).
    /// </summary>
    public double Confidence { get; set; }

    /// <summary>
    /// The timestamp when this evaluation was performed.
    /// </summary>
    public DateTimeOffset EvaluatedAt { get; set; }
}
