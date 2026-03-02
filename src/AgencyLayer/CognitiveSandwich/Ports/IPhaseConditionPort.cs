using AgencyLayer.CognitiveSandwich.Models;

namespace AgencyLayer.CognitiveSandwich.Ports;

/// <summary>
/// Defines the port for evaluating pre- and postconditions on phases
/// within a Cognitive Sandwich process. Conditions act as quality gates
/// that must be satisfied before entering or after completing a phase.
/// </summary>
public interface IPhaseConditionPort
{
    /// <summary>
    /// Evaluates all preconditions for the specified phase, determining
    /// whether the phase is ready to begin execution.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="phaseId">The unique identifier of the phase whose preconditions to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregated result with individual condition evaluations.</returns>
    Task<ConditionCheckResult> CheckPreconditionsAsync(string processId, string phaseId, CancellationToken ct = default);

    /// <summary>
    /// Evaluates all postconditions for the specified phase given the phase output,
    /// determining whether the phase can be considered complete.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="phaseId">The unique identifier of the phase whose postconditions to check.</param>
    /// <param name="output">The output produced by the phase execution.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Aggregated result with individual condition evaluations.</returns>
    Task<ConditionCheckResult> CheckPostconditionsAsync(string processId, string phaseId, PhaseOutput output, CancellationToken ct = default);
}
