namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: Not applicable to current web API architecture
// Reconsideration: If IDE integrations are prioritized
// ============================================================================

/// <summary>
/// Tooling assumption.
/// </summary>
public class ToolingAssumption
{
    /// <summary>Assumption identifier.</summary>
    public required string AssumptionId { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Is valid.</summary>
    public bool IsValid { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for dev tooling assumptions reset.
/// Implements the "Dev Tooling Assumptions Reset" pattern.
///
/// This is a low-priority pattern because it's not applicable
/// to the current web API architecture.
/// </summary>
public interface IDevToolingResetPort
{
    /// <summary>Validates assumptions.</summary>
    Task<IReadOnlyList<ToolingAssumption>> ValidateAssumptionsAsync(CancellationToken cancellationToken = default);

    /// <summary>Resets assumptions.</summary>
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets current assumptions.</summary>
    Task<IReadOnlyList<ToolingAssumption>> GetAssumptionsAsync(CancellationToken cancellationToken = default);
}
