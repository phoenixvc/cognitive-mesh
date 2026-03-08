namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: No funding model in scope for current architecture
// Reconsideration: If agent resource funding becomes a requirement
// ============================================================================

/// <summary>
/// Milestone escrow.
/// </summary>
public class MilestoneEscrow
{
    /// <summary>Escrow identifier.</summary>
    public required string EscrowId { get; init; }

    /// <summary>Amount.</summary>
    public decimal Amount { get; init; }

    /// <summary>Milestone description.</summary>
    public required string Milestone { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for milestone escrow for agent resource funding.
/// Implements the "Milestone Escrow for Agent Resource Funding" pattern.
///
/// This is a low-priority pattern because no funding model
/// is in scope for the current architecture.
/// </summary>
public interface IMilestoneEscrowPort
{
    /// <summary>Creates escrow.</summary>
    Task<MilestoneEscrow> CreateEscrowAsync(string milestone, decimal amount, CancellationToken cancellationToken = default);

    /// <summary>Releases escrow.</summary>
    Task ReleaseAsync(string escrowId, CancellationToken cancellationToken = default);

    /// <summary>Gets escrow status.</summary>
    Task<MilestoneEscrow?> GetEscrowAsync(string escrowId, CancellationToken cancellationToken = default);
}
