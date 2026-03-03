namespace AgencyLayer.CognitiveSandwich.Models;

/// <summary>
/// Defines the configuration for a single phase when creating a new Cognitive Sandwich process.
/// Used within <see cref="SandwichProcessConfig"/> to specify the phase structure.
/// </summary>
public class PhaseDefinition
{
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
}
