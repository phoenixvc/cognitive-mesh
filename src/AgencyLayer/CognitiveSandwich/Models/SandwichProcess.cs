namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents the core entity of a Cognitive Sandwich workflow: a multi-phase process
/// with human-in-the-loop validation, step-back capability, and cognitive debt monitoring.
/// </summary>
public class SandwichProcess
{
    /// <summary>
    /// Unique identifier for this process instance.
    /// </summary>
    public string ProcessId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Tenant that owns this process, for multi-tenancy isolation.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable name of the process.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the process was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Zero-based index of the currently active phase.
    /// </summary>
    public int CurrentPhaseIndex { get; set; }

    /// <summary>
    /// Ordered list of phases that compose this process.
    /// </summary>
    public IReadOnlyList<Phase> Phases { get; set; } = [];

    /// <summary>
    /// Current lifecycle state of the process.
    /// </summary>
    public SandwichProcessState State { get; set; } = SandwichProcessState.Created;

    /// <summary>
    /// Maximum number of step-back operations allowed before the process is blocked.
    /// </summary>
    public int MaxStepBacks { get; set; } = 3;

    /// <summary>
    /// Number of step-back operations that have been performed so far.
    /// </summary>
    public int StepBackCount { get; set; }

    /// <summary>
    /// Cognitive debt threshold (0-100). When exceeded, phase transitions are blocked.
    /// </summary>
    public double CognitiveDebtThreshold { get; set; } = 70.0;
}
