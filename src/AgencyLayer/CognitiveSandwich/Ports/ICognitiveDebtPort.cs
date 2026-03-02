using AgencyLayer.CognitiveSandwich.Models;

namespace AgencyLayer.CognitiveSandwich.Ports;

/// <summary>
/// Defines the port for assessing cognitive debt within a Cognitive Sandwich process.
/// Cognitive debt measures the degree of over-reliance on AI, enabling the system
/// to enforce human oversight at appropriate intervals.
/// </summary>
public interface ICognitiveDebtPort
{
    /// <summary>
    /// Assesses the cognitive debt for a specific phase within a process.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="phaseId">The unique identifier of the phase to assess.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A debt assessment including score, breach status, and recommendations.</returns>
    Task<CognitiveDebtAssessment> AssessDebtAsync(string processId, string phaseId, CancellationToken ct = default);

    /// <summary>
    /// Checks whether the cognitive debt threshold has been breached for the process,
    /// indicating that further AI-driven phase transitions should be blocked.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the threshold is breached; otherwise <c>false</c>.</returns>
    Task<bool> IsThresholdBreachedAsync(string processId, CancellationToken ct = default);
}
