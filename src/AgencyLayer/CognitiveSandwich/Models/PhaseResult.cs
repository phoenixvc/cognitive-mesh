namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the result of a phase transition or step-back operation,
/// including success status, validation errors, and the associated audit entry.
/// </summary>
public class PhaseResult
{
    /// <summary>
    /// Indicates whether the transition or step-back completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Identifier of the phase that was transitioned from.
    /// </summary>
    public string PhaseId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the phase that was transitioned to, or <c>null</c> if the transition was blocked.
    /// </summary>
    public string? NextPhaseId { get; set; }

    /// <summary>
    /// Validation errors that prevented the transition, if any.
    /// </summary>
    public IReadOnlyList<string> ValidationErrors { get; set; } = [];

    /// <summary>
    /// Audit trail entry recording this transition attempt.
    /// </summary>
    public PhaseAuditEntry? AuditEntry { get; set; }
}
