namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the lifecycle state of a Cognitive Sandwich process.
/// </summary>
public enum SandwichProcessState
{
    /// <summary>
    /// Process has been created but not yet started.
    /// </summary>
    Created,

    /// <summary>
    /// Process is actively executing phases.
    /// </summary>
    InProgress,

    /// <summary>
    /// Process is waiting for human review at a validation checkpoint.
    /// </summary>
    AwaitingHumanReview,

    /// <summary>
    /// Process has stepped back to a prior phase for rework.
    /// </summary>
    SteppedBack,

    /// <summary>
    /// All phases completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Process failed due to unrecoverable error or policy violation.
    /// </summary>
    Failed
}
