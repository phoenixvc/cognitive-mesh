namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Represents a single phase within a Cognitive Sandwich process.
/// Each phase has pre/postconditions, an execution status, and optional human validation.
/// </summary>
public class Phase
{
    /// <summary>
    /// Unique identifier for this phase.
    /// </summary>
    public string PhaseId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Human-readable name of the phase.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this phase accomplishes.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Zero-based order of this phase within the process.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Conditions that must be satisfied before this phase can begin.
    /// </summary>
    public IReadOnlyList<PhaseCondition> Preconditions { get; set; } = [];

    /// <summary>
    /// Conditions that must be satisfied after this phase completes.
    /// </summary>
    public IReadOnlyList<PhaseCondition> Postconditions { get; set; } = [];

    /// <summary>
    /// When <c>true</c>, the process pauses after this phase for human-in-the-loop validation.
    /// </summary>
    public bool RequiresHumanValidation { get; set; }

    /// <summary>
    /// Current execution status of this phase.
    /// </summary>
    public PhaseStatus Status { get; set; } = PhaseStatus.Pending;
}
