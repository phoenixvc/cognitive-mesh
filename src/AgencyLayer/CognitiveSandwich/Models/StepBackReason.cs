namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Captures the reason, initiator, and timestamp for a step-back operation
/// that rewinds a Cognitive Sandwich process to a prior phase.
/// </summary>
public class StepBackReason
{
    /// <summary>
    /// Human-readable explanation of why the step-back was initiated.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the user or system component that initiated the step-back.
    /// </summary>
    public string InitiatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the step-back was requested.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
