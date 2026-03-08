namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Irreversible commitment pattern not applicable
// Reconsideration: If hard commitment decisions are required
// ============================================================================

/// <summary>
/// Commitment decision.
/// </summary>
public class CommitmentDecision
{
    /// <summary>Decision identifier.</summary>
    public required string DecisionId { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Is irreversible.</summary>
    public bool IsIrreversible { get; init; }

    /// <summary>Committed at.</summary>
    public DateTimeOffset? CommittedAt { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for burn the boats pattern.
/// Implements the "Burn the Boats" pattern for irreversible commitment.
///
/// This is a low-priority pattern because irreversible commitment
/// is not applicable to current use cases.
/// </summary>
public interface IBurnTheBoatsPort
{
    /// <summary>Makes irreversible commitment.</summary>
    Task<CommitmentDecision> CommitAsync(string description, CancellationToken cancellationToken = default);

    /// <summary>Checks if committed.</summary>
    Task<bool> IsCommittedAsync(string decisionId, CancellationToken cancellationToken = default);

    /// <summary>Lists commitments.</summary>
    Task<IReadOnlyList<CommitmentDecision>> ListCommitmentsAsync(CancellationToken cancellationToken = default);
}
