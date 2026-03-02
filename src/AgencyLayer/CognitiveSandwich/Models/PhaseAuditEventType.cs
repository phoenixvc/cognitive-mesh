namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Categorizes the type of event recorded in a phase audit trail entry.
/// </summary>
public enum PhaseAuditEventType
{
    /// <summary>
    /// A new Cognitive Sandwich process was created.
    /// </summary>
    ProcessCreated,

    /// <summary>
    /// A phase transition was initiated.
    /// </summary>
    PhaseTransitionStarted,

    /// <summary>
    /// A phase transition completed successfully.
    /// </summary>
    PhaseTransitionCompleted,

    /// <summary>
    /// A phase transition was blocked by failed preconditions.
    /// </summary>
    PhaseTransitionBlocked,

    /// <summary>
    /// A step-back operation was performed to revisit a prior phase.
    /// </summary>
    StepBackPerformed,

    /// <summary>
    /// Human validation was requested for a phase checkpoint.
    /// </summary>
    HumanValidationRequested,

    /// <summary>
    /// Human validation was completed.
    /// </summary>
    HumanValidationCompleted,

    /// <summary>
    /// Cognitive debt threshold was breached, blocking progression.
    /// </summary>
    CognitiveDebtBreached,

    /// <summary>
    /// The process completed all phases successfully.
    /// </summary>
    ProcessCompleted,

    /// <summary>
    /// The process failed due to an unrecoverable error.
    /// </summary>
    ProcessFailed
}
