namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Provides context for a phase transition request, including the output from the
/// current phase, the requesting user, and optional human feedback.
/// </summary>
public class PhaseTransitionContext
{
    /// <summary>
    /// Identifier of the user initiating the transition.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Output produced by the current phase, used for postcondition evaluation.
    /// </summary>
    public PhaseOutput? PhaseOutput { get; set; }

    /// <summary>
    /// Optional human feedback provided during a human-in-the-loop validation checkpoint.
    /// </summary>
    public string? HumanFeedback { get; set; }

    /// <summary>
    /// Reason or justification for the transition.
    /// </summary>
    public string TransitionReason { get; set; } = string.Empty;
}
