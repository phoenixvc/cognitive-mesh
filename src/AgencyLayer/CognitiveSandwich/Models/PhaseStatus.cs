namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the status of a single phase within a Cognitive Sandwich process.
/// </summary>
public enum PhaseStatus
{
    /// <summary>
    /// Phase has not started yet.
    /// </summary>
    Pending,

    /// <summary>
    /// Phase is currently being executed.
    /// </summary>
    InProgress,

    /// <summary>
    /// Phase output is awaiting human review before the process can advance.
    /// </summary>
    AwaitingReview,

    /// <summary>
    /// Phase completed successfully and its postconditions are satisfied.
    /// </summary>
    Completed,

    /// <summary>
    /// Phase was rolled back due to a step-back operation.
    /// </summary>
    RolledBack
}
