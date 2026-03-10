namespace AgencyLayer.AgentTeamFramework.Ports;

/// <summary>
/// Standard lifecycle port for agent teams. All agent teams expose
/// initialization and metadata through this interface. Domain-specific
/// ports (e.g., <c>IRoadmapCrewPort</c>) extend this with team-specific operations.
/// </summary>
public interface IAgentTeamPort
{
    /// <summary>Team identifier (e.g., "roadmap-crew", "debug-squad").</summary>
    string TeamId { get; }

    /// <summary>Number of agents in this team.</summary>
    int AgentCount { get; }

    /// <summary>Whether the team has been initialized (agents registered).</summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Initializes the team by registering all agents with the specialized agent port.
    /// Thread-safe and idempotent — safe to call multiple times or concurrently.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
