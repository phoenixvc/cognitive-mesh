namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Single personality per agent is sufficient
// Reconsideration: If multiple agent personalities are needed
// ============================================================================

/// <summary>
/// Agent personality mode.
/// </summary>
public class PersonalityMode
{
    /// <summary>Mode identifier.</summary>
    public required string ModeId { get; init; }

    /// <summary>Mode name.</summary>
    public required string Name { get; init; }

    /// <summary>Traits.</summary>
    public IReadOnlyList<string> Traits { get; init; } = Array.Empty<string>();

    /// <summary>Communication style.</summary>
    public required string Style { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for agent modes by model personality.
/// Implements the "Agent Modes by Model Personality" pattern.
///
/// This is a low-priority pattern because single personality per
/// agent is sufficient for current use cases.
/// </summary>
public interface IAgentPersonalityPort
{
    /// <summary>Sets personality mode.</summary>
    Task SetModeAsync(string agentId, string modeId, CancellationToken cancellationToken = default);

    /// <summary>Gets current mode.</summary>
    Task<PersonalityMode?> GetModeAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>Lists available modes.</summary>
    Task<IReadOnlyList<PersonalityMode>> ListModesAsync(CancellationToken cancellationToken = default);

    /// <summary>Registers a mode.</summary>
    Task RegisterModeAsync(PersonalityMode mode, CancellationToken cancellationToken = default);
}
