namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

/// <summary>
/// Represents the adaptive temporal window state used by the dual-circuit gate.
/// The window dynamically adjusts its maximum gap (0-20000 ms) based on threat level,
/// cognitive load, and salience thresholds, as specified by the TDC PRD.
/// </summary>
public sealed class TemporalWindow
{
    /// <summary>
    /// The current maximum allowed gap in milliseconds between two events
    /// for them to be considered as potentially linked. Range: 0-20000 ms.
    /// </summary>
    public double CurrentMaxGapMs { get; set; } = 10000.0;

    /// <summary>
    /// The base salience threshold below which events are not considered
    /// for temporal linking.
    /// </summary>
    public double BaseSalienceThreshold { get; set; } = 0.3;

    /// <summary>
    /// Multiplier applied to the window size during elevated threat conditions.
    /// Higher threat multipliers expand the window to capture delayed causal chains.
    /// </summary>
    public double ThreatMultiplier { get; set; } = 1.0;

    /// <summary>
    /// Factor representing current cognitive/processing load (0.0 to 1.0).
    /// Higher load factors contract the window to reduce association noise.
    /// </summary>
    public double LoadFactor { get; set; } = 0.5;
}
