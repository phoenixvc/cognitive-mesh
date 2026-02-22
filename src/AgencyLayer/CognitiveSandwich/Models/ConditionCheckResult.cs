namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the aggregated result of evaluating all pre- or postconditions for a phase.
/// </summary>
public class ConditionCheckResult
{
    /// <summary>
    /// Indicates whether all mandatory conditions were met.
    /// </summary>
    public bool AllMet { get; set; }

    /// <summary>
    /// Individual evaluation results for each condition.
    /// </summary>
    public IReadOnlyList<ConditionEvaluation> Results { get; set; } = [];
}
