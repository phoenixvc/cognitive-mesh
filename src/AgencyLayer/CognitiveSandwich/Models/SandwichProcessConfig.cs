namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Configuration used to create a new Cognitive Sandwich process,
/// defining the phases, step-back limits, and cognitive debt thresholds.
/// </summary>
public class SandwichProcessConfig
{
    /// <summary>
    /// Tenant that will own the created process.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name for the process.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of phase definitions that compose the process.
    /// Must contain between 3 and 7 phases.
    /// </summary>
    public IReadOnlyList<PhaseDefinition> Phases { get; set; } = [];

    /// <summary>
    /// Maximum number of step-back operations allowed before the process is blocked.
    /// </summary>
    public int MaxStepBacks { get; set; } = 3;

    /// <summary>
    /// Cognitive debt score threshold (0-100). When exceeded, phase transitions are blocked.
    /// </summary>
    public double CognitiveDebtThreshold { get; set; } = 70.0;
}
