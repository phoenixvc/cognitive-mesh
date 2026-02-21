namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the evaluation result of a single phase condition.
/// </summary>
public class ConditionEvaluation
{
    /// <summary>
    /// Identifier of the condition that was evaluated.
    /// </summary>
    public string ConditionId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the condition was satisfied.
    /// </summary>
    public bool Met { get; set; }

    /// <summary>
    /// Human-readable reason for the evaluation result.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
