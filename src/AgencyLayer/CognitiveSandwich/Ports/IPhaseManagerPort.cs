using AgencyLayer.CognitiveSandwich.Models;

namespace AgencyLayer.CognitiveSandwich.Ports;

/// <summary>
/// Defines the primary port for managing Cognitive Sandwich processes.
/// This port handles process creation, phase transitions, step-back operations,
/// and audit trail retrieval following hexagonal architecture conventions.
/// </summary>
public interface IPhaseManagerPort
{
    /// <summary>
    /// Creates a new Cognitive Sandwich process from the given configuration.
    /// </summary>
    /// <param name="config">Configuration specifying phases, step-back limits, and thresholds.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The newly created process with all phases initialized.</returns>
    Task<SandwichProcess> CreateProcessAsync(SandwichProcessConfig config, CancellationToken ct = default);

    /// <summary>
    /// Retrieves an existing Cognitive Sandwich process by its unique identifier.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The process, or throws if not found.</returns>
    Task<SandwichProcess> GetProcessAsync(string processId, CancellationToken ct = default);

    /// <summary>
    /// Attempts to transition the process to its next phase after validating
    /// postconditions of the current phase, preconditions of the next phase,
    /// and cognitive debt thresholds.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="context">Context for the transition including user, output, and feedback.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the transition attempt.</returns>
    Task<PhaseResult> TransitionToNextPhaseAsync(string processId, PhaseTransitionContext context, CancellationToken ct = default);

    /// <summary>
    /// Steps the process back to a prior phase, rolling back intermediate phases
    /// and incrementing the step-back counter.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="targetPhaseId">The identifier of the phase to step back to.</param>
    /// <param name="reason">The reason for the step-back operation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the step-back attempt.</returns>
    Task<PhaseResult> StepBackAsync(string processId, string targetPhaseId, StepBackReason reason, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the complete audit trail for a process, ordered chronologically.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of audit entries for the process.</returns>
    Task<IReadOnlyList<PhaseAuditEntry>> GetAuditTrailAsync(string processId, CancellationToken ct = default);
}
