namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: No spending model in scope for current architecture
// Reconsideration: If agent spending controls become a requirement
// ============================================================================

/// <summary>
/// Spending limit.
/// </summary>
public class SpendingLimit
{
    /// <summary>Limit identifier.</summary>
    public required string LimitId { get; init; }

    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Maximum amount.</summary>
    public decimal MaxAmount { get; init; }

    /// <summary>Period.</summary>
    public required string Period { get; init; }

    /// <summary>Current spent.</summary>
    public decimal CurrentSpent { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for non-custodial spending controls.
/// Implements the "Non-Custodial Spending Controls" pattern.
///
/// This is a low-priority pattern because no spending model
/// is in scope for the current architecture.
/// </summary>
public interface INonCustodialSpendingPort
{
    /// <summary>Sets spending limit.</summary>
    Task SetLimitAsync(string agentId, decimal maxAmount, string period, CancellationToken cancellationToken = default);

    /// <summary>Checks spending.</summary>
    Task<(bool Allowed, decimal Remaining)> CheckSpendingAsync(string agentId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Records spending.</summary>
    Task RecordSpendingAsync(string agentId, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Gets limit.</summary>
    Task<SpendingLimit?> GetLimitAsync(string agentId, CancellationToken cancellationToken = default);
}
