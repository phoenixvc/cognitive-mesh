using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Ports;

/// <summary>
/// Defines the contract for the dual-circuit temporal association gate.
/// The gate uses CA1 promoter and L2 suppressor circuits to determine whether
/// two temporally separated events should be linked, preventing spurious
/// associations while preserving true causal chains.
/// </summary>
public interface ITemporalGatePort
{
    /// <summary>
    /// Evaluates whether a temporal edge should be created between a source and target event
    /// using the dual-circuit gate (CA1 promoter + L2 suppressor).
    /// </summary>
    /// <param name="sourceEvent">The earlier event in the potential temporal link.</param>
    /// <param name="targetEvent">The later event in the potential temporal link.</param>
    /// <param name="window">The current adaptive temporal window state.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A gating decision containing the scores, confidence, rationale, and link decision.</returns>
    Task<GatingDecision> EvaluateEdgeAsync(
        TemporalEvent sourceEvent,
        TemporalEvent targetEvent,
        TemporalWindow window,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adjusts the adaptive temporal window parameters based on threat level,
    /// cognitive load, and precision signals.
    /// </summary>
    /// <param name="window">The current window state to adjust.</param>
    /// <param name="threatLevel">Current threat level from 0.0 (none) to 1.0 (critical).</param>
    /// <param name="loadFactor">Current system load from 0.0 (idle) to 1.0 (saturated).</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The adjusted temporal window state.</returns>
    Task<TemporalWindow> AdjustWindowAsync(
        TemporalWindow window,
        double threatLevel,
        double loadFactor,
        CancellationToken cancellationToken = default);
}
