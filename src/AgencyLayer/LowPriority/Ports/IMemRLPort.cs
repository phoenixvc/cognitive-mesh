namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No memory-based RL infrastructure
// Reconsideration: If RL training infrastructure is built
// ============================================================================

/// <summary>
/// Memory entry for RL.
/// </summary>
public class MemoryEntry
{
    /// <summary>Entry identifier.</summary>
    public required string EntryId { get; init; }

    /// <summary>Content.</summary>
    public required string Content { get; init; }

    /// <summary>Importance score.</summary>
    public double Importance { get; init; }

    /// <summary>Reward signal.</summary>
    public double Reward { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for Memory Reinforcement Learning.
/// Implements the "Memory Reinforcement Learning (MemRL)" pattern.
///
/// This is a low-priority pattern because no memory-based RL
/// infrastructure exists.
/// </summary>
public interface IMemRLPort
{
    /// <summary>Stores memory with reward.</summary>
    Task StoreAsync(MemoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Retrieves memories by importance.</summary>
    Task<IReadOnlyList<MemoryEntry>> RetrieveByImportanceAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>Updates memory rewards.</summary>
    Task UpdateRewardAsync(string entryId, double reward, CancellationToken cancellationToken = default);

    /// <summary>Trains memory selection policy.</summary>
    Task TrainPolicyAsync(CancellationToken cancellationToken = default);
}
