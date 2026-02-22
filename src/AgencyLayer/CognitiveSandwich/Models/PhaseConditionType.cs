namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Defines the type of a phase condition used for pre/postcondition evaluation.
/// </summary>
public enum PhaseConditionType
{
    /// <summary>
    /// Condition requires a specific data field to be present and non-empty.
    /// </summary>
    DataPresence,

    /// <summary>
    /// Condition requires a quality score to meet a minimum threshold.
    /// </summary>
    QualityThreshold,

    /// <summary>
    /// Condition requires explicit human approval.
    /// </summary>
    HumanApproval,

    /// <summary>
    /// Condition evaluates a custom expression or rule.
    /// </summary>
    CustomExpression,

    /// <summary>
    /// Condition checks that cognitive debt is within acceptable limits.
    /// </summary>
    CognitiveDebtCheck
}
