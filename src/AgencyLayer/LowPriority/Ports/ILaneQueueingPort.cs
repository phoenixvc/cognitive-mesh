namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Current queueing mechanisms are sufficient
// Reconsideration: If lane-based prioritization is needed
// ============================================================================

/// <summary>
/// Execution lane.
/// </summary>
public class ExecutionLane
{
    /// <summary>Lane identifier.</summary>
    public required string LaneId { get; init; }

    /// <summary>Lane name.</summary>
    public required string Name { get; init; }

    /// <summary>Priority.</summary>
    public int Priority { get; init; }

    /// <summary>Concurrency limit.</summary>
    public int ConcurrencyLimit { get; init; }

    /// <summary>Current queue size.</summary>
    public int QueueSize { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for lane-based execution queueing.
/// Implements the "Lane-Based Execution Queueing" pattern.
///
/// This is a low-priority pattern because current queueing
/// mechanisms are sufficient.
/// </summary>
public interface ILaneQueueingPort
{
    /// <summary>Creates a lane.</summary>
    Task<ExecutionLane> CreateLaneAsync(string name, int priority, int concurrencyLimit, CancellationToken cancellationToken = default);

    /// <summary>Enqueues task to lane.</summary>
    Task<string> EnqueueAsync(string laneId, string taskId, CancellationToken cancellationToken = default);

    /// <summary>Gets lane status.</summary>
    Task<ExecutionLane?> GetLaneAsync(string laneId, CancellationToken cancellationToken = default);

    /// <summary>Lists lanes.</summary>
    Task<IReadOnlyList<ExecutionLane>> ListLanesAsync(CancellationToken cancellationToken = default);
}
