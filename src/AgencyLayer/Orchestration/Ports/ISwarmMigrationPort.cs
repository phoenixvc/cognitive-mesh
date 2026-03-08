namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Swarm topology type.
/// </summary>
public enum SwarmTopology
{
    /// <summary>Central orchestrator pattern.</summary>
    Centralized,
    /// <summary>Fully connected mesh.</summary>
    FullMesh,
    /// <summary>Hierarchical tree structure.</summary>
    Hierarchical,
    /// <summary>Ring topology.</summary>
    Ring,
    /// <summary>Star topology with specialists.</summary>
    Star,
    /// <summary>Dynamic topology.</summary>
    Dynamic
}

/// <summary>
/// A swarm configuration.
/// </summary>
public class SwarmConfiguration
{
    /// <summary>Swarm identifier.</summary>
    public string SwarmId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Swarm name.</summary>
    public required string Name { get; init; }

    /// <summary>Current topology.</summary>
    public SwarmTopology Topology { get; init; }

    /// <summary>Agent IDs in the swarm.</summary>
    public IReadOnlyList<string> AgentIds { get; init; } = Array.Empty<string>();

    /// <summary>Connection map (agent -> connected agents).</summary>
    public Dictionary<string, List<string>> Connections { get; init; } = new();

    /// <summary>Leader agent ID (if applicable).</summary>
    public string? LeaderId { get; init; }

    /// <summary>Maximum agents.</summary>
    public int MaxAgents { get; init; } = 10;

    /// <summary>Load balancing strategy.</summary>
    public string LoadBalancing { get; init; } = "round-robin";
}

/// <summary>
/// Migration plan between topologies.
/// </summary>
public class MigrationPlan
{
    /// <summary>Plan identifier.</summary>
    public string PlanId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Source topology.</summary>
    public required SwarmTopology SourceTopology { get; init; }

    /// <summary>Target topology.</summary>
    public required SwarmTopology TargetTopology { get; init; }

    /// <summary>Migration steps.</summary>
    public IReadOnlyList<MigrationStep> Steps { get; init; } = Array.Empty<MigrationStep>();

    /// <summary>Estimated duration.</summary>
    public TimeSpan EstimatedDuration { get; init; }

    /// <summary>Rollback plan.</summary>
    public string? RollbackPlanId { get; init; }
}

/// <summary>
/// A migration step.
/// </summary>
public class MigrationStep
{
    /// <summary>Step order.</summary>
    public int Order { get; init; }

    /// <summary>Step type.</summary>
    public required string StepType { get; init; }

    /// <summary>Affected agents.</summary>
    public IReadOnlyList<string> AffectedAgents { get; init; } = Array.Empty<string>();

    /// <summary>Step description.</summary>
    public required string Description { get; init; }

    /// <summary>Whether step is reversible.</summary>
    public bool Reversible { get; init; } = true;
}

/// <summary>
/// Migration status.
/// </summary>
public class MigrationStatus
{
    /// <summary>Plan ID.</summary>
    public required string PlanId { get; init; }

    /// <summary>Current state.</summary>
    public required string State { get; init; }

    /// <summary>Current step.</summary>
    public int CurrentStep { get; init; }

    /// <summary>Total steps.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }
}

/// <summary>
/// Port for swarm migration pattern.
/// Implements the "Swarm Migration Pattern" for topology flexibility.
/// </summary>
public interface ISwarmMigrationPort
{
    /// <summary>
    /// Gets current swarm configuration.
    /// </summary>
    Task<SwarmConfiguration> GetConfigurationAsync(
        string swarmId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Plans a migration to a new topology.
    /// </summary>
    Task<MigrationPlan> PlanMigrationAsync(
        string swarmId,
        SwarmTopology targetTopology,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a migration plan.
    /// </summary>
    Task<MigrationStatus> ExecuteMigrationAsync(
        string planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets migration status.
    /// </summary>
    Task<MigrationStatus> GetMigrationStatusAsync(
        string planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back a migration.
    /// </summary>
    Task<MigrationStatus> RollbackMigrationAsync(
        string planId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a topology for the swarm.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateTopologyAsync(
        string swarmId,
        SwarmTopology topology,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an agent to the swarm.
    /// </summary>
    Task AddAgentAsync(
        string swarmId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an agent from the swarm.
    /// </summary>
    Task RemoveAgentAsync(
        string swarmId,
        string agentId,
        CancellationToken cancellationToken = default);
}
