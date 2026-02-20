namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents the current status of the reflexion (self-evaluation) system,
/// including recent results and aggregate metrics.
/// </summary>
public class ReflexionStatusResponse
{
    /// <summary>
    /// Recent reflexion evaluation results.
    /// </summary>
    public List<ReflexionStatusEntry> RecentResults { get; set; } = new();

    /// <summary>
    /// The overall hallucination rate as a proportion (0.0 to 1.0).
    /// </summary>
    public double HallucinationRate { get; set; }

    /// <summary>
    /// The average confidence level across recent evaluations (0.0 to 1.0).
    /// </summary>
    public double AverageConfidence { get; set; }
}
