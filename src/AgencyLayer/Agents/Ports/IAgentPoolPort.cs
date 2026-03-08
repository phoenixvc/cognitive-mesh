namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Status of a pooled agent.
/// </summary>
public enum PooledAgentStatus
{
    /// <summary>Agent is available.</summary>
    Available,
    /// <summary>Agent is busy.</summary>
    Busy,
    /// <summary>Agent is warming up.</summary>
    WarmingUp,
    /// <summary>Agent is cooling down.</summary>
    CoolingDown,
    /// <summary>Agent is unhealthy.</summary>
    Unhealthy
}

/// <summary>
/// A pooled agent instance.
/// </summary>
public class PooledAgent
{
    /// <summary>Instance identifier.</summary>
    public required string InstanceId { get; init; }

    /// <summary>Agent type.</summary>
    public required string AgentType { get; init; }

    /// <summary>Current status.</summary>
    public PooledAgentStatus Status { get; init; }

    /// <summary>Current task if busy.</summary>
    public string? CurrentTaskId { get; init; }

    /// <summary>Tasks completed.</summary>
    public int TasksCompleted { get; init; }

    /// <summary>Average task duration.</summary>
    public TimeSpan AverageTaskDuration { get; init; }

    /// <summary>Last activity time.</summary>
    public DateTimeOffset LastActivityAt { get; init; }

    /// <summary>When the instance was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Memory usage if tracked.</summary>
    public long? MemoryUsageBytes { get; init; }
}

/// <summary>
/// Configuration for an agent pool.
/// </summary>
public class AgentPoolConfiguration
{
    /// <summary>Agent type for this pool.</summary>
    public required string AgentType { get; init; }

    /// <summary>Minimum instances.</summary>
    public int MinInstances { get; init; } = 1;

    /// <summary>Maximum instances.</summary>
    public int MaxInstances { get; init; } = 10;

    /// <summary>Target utilization (0.0 - 1.0).</summary>
    public double TargetUtilization { get; init; } = 0.7;

    /// <summary>Scale up threshold.</summary>
    public double ScaleUpThreshold { get; init; } = 0.8;

    /// <summary>Scale down threshold.</summary>
    public double ScaleDownThreshold { get; init; } = 0.3;

    /// <summary>Scale up cooldown.</summary>
    public TimeSpan ScaleUpCooldown { get; init; } = TimeSpan.FromMinutes(2);

    /// <summary>Scale down cooldown.</summary>
    public TimeSpan ScaleDownCooldown { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Idle timeout before recycling.</summary>
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(15);

    /// <summary>Maximum tasks before recycling.</summary>
    public int? MaxTasksBeforeRecycle { get; init; }

    /// <summary>Health check interval.</summary>
    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(30);
}

/// <summary>
/// Request to acquire an agent from the pool.
/// </summary>
public class AcquireAgentRequest
{
    /// <summary>Agent type needed.</summary>
    public required string AgentType { get; init; }

    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Maximum wait time.</summary>
    public TimeSpan? MaxWaitTime { get; init; }

    /// <summary>Priority (lower = higher priority).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Preferred instance (for affinity).</summary>
    public string? PreferredInstanceId { get; init; }
}

/// <summary>
/// Result of acquiring an agent.
/// </summary>
public class AcquireAgentResult
{
    /// <summary>Whether an agent was acquired.</summary>
    public required bool Success { get; init; }

    /// <summary>The acquired agent.</summary>
    public PooledAgent? Agent { get; init; }

    /// <summary>Wait time before acquisition.</summary>
    public TimeSpan WaitTime { get; init; }

    /// <summary>Reason if failed.</summary>
    public string? FailureReason { get; init; }
}

/// <summary>
/// Pool statistics.
/// </summary>
public class AgentPoolStatistics
{
    /// <summary>Pool agent type.</summary>
    public required string AgentType { get; init; }

    /// <summary>Total instances.</summary>
    public int TotalInstances { get; init; }

    /// <summary>Available instances.</summary>
    public int AvailableInstances { get; init; }

    /// <summary>Busy instances.</summary>
    public int BusyInstances { get; init; }

    /// <summary>Current utilization.</summary>
    public double Utilization { get; init; }

    /// <summary>Tasks in queue.</summary>
    public int QueuedTasks { get; init; }

    /// <summary>Average wait time.</summary>
    public TimeSpan AverageWaitTime { get; init; }

    /// <summary>Tasks completed in last hour.</summary>
    public int TasksCompletedLastHour { get; init; }
}

/// <summary>
/// Port for agent pooling and lifecycle management.
/// Implements the "Agent Pool Lifecycle Management" pattern.
/// </summary>
public interface IAgentPoolPort
{
    /// <summary>
    /// Creates or updates a pool.
    /// </summary>
    Task CreatePoolAsync(
        AgentPoolConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acquires an agent from the pool.
    /// </summary>
    Task<AcquireAgentResult> AcquireAsync(
        AcquireAgentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases an agent back to the pool.
    /// </summary>
    Task ReleaseAsync(
        string instanceId,
        bool recycle = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pool statistics.
    /// </summary>
    Task<AgentPoolStatistics> GetStatisticsAsync(
        string agentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales a pool.
    /// </summary>
    Task ScaleAsync(
        string agentType,
        int targetInstances,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pool instances.
    /// </summary>
    Task<IReadOnlyList<PooledAgent>> GetInstancesAsync(
        string agentType,
        PooledAgentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs health check on pool.
    /// </summary>
    Task<IReadOnlyList<(string InstanceId, bool Healthy, string? Issue)>> HealthCheckAsync(
        string agentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recycles unhealthy instances.
    /// </summary>
    Task<int> RecycleUnhealthyAsync(
        string agentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a pool.
    /// </summary>
    Task DeletePoolAsync(
        string agentType,
        CancellationToken cancellationToken = default);
}
