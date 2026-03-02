namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents a pre- or postcondition attached to a phase in a Cognitive Sandwich process.
/// Conditions are evaluated before entering or after completing a phase.
/// </summary>
public class PhaseCondition
{
    /// <summary>
    /// Unique identifier for this condition.
    /// </summary>
    public string ConditionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Human-readable name of the condition.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The type of evaluation this condition performs.
    /// </summary>
    public PhaseConditionType ConditionType { get; set; }

    /// <summary>
    /// The expression or rule to evaluate (interpretation depends on <see cref="ConditionType"/>).
    /// </summary>
    public string Expression { get; set; } = string.Empty;

    /// <summary>
    /// When <c>true</c>, failure of this condition blocks phase transition.
    /// When <c>false</c>, failure is logged as a warning but does not block.
    /// </summary>
    public bool IsMandatory { get; set; } = true;
}
