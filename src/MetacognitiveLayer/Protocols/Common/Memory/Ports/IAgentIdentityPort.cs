namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// An agent's accumulated identity.
/// </summary>
public class AgentIdentity
{
    /// <summary>Identity identifier.</summary>
    public required string IdentityId { get; init; }

    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Core persona traits.</summary>
    public IReadOnlyList<string> CoreTraits { get; init; } = Array.Empty<string>();

    /// <summary>Learned preferences.</summary>
    public Dictionary<string, string> Preferences { get; init; } = new();

    /// <summary>Interaction style.</summary>
    public InteractionStyle Style { get; init; } = new();

    /// <summary>Knowledge domains.</summary>
    public IReadOnlyList<KnowledgeDomain> Domains { get; init; } = Array.Empty<KnowledgeDomain>();

    /// <summary>Behavioral patterns observed.</summary>
    public IReadOnlyList<BehaviorPattern> Patterns { get; init; } = Array.Empty<BehaviorPattern>();

    /// <summary>Total interactions.</summary>
    public int TotalInteractions { get; init; }

    /// <summary>Identity version.</summary>
    public int Version { get; init; }

    /// <summary>Last updated.</summary>
    public DateTimeOffset LastUpdated { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Interaction style preferences.
/// </summary>
public class InteractionStyle
{
    /// <summary>Communication tone.</summary>
    public string Tone { get; init; } = "professional";

    /// <summary>Verbosity level (1-5).</summary>
    public int Verbosity { get; init; } = 3;

    /// <summary>Preferred response format.</summary>
    public string PreferredFormat { get; init; } = "structured";

    /// <summary>Use of examples.</summary>
    public bool IncludeExamples { get; init; } = true;

    /// <summary>Technical depth.</summary>
    public string TechnicalDepth { get; init; } = "moderate";
}

/// <summary>
/// A knowledge domain.
/// </summary>
public class KnowledgeDomain
{
    /// <summary>Domain name.</summary>
    public required string Name { get; init; }

    /// <summary>Proficiency level (0.0 - 1.0).</summary>
    public double Proficiency { get; init; }

    /// <summary>Times applied.</summary>
    public int ApplicationCount { get; init; }

    /// <summary>Last applied.</summary>
    public DateTimeOffset? LastApplied { get; init; }
}

/// <summary>
/// A behavioral pattern.
/// </summary>
public class BehaviorPattern
{
    /// <summary>Pattern name.</summary>
    public required string Name { get; init; }

    /// <summary>Pattern description.</summary>
    public required string Description { get; init; }

    /// <summary>Frequency (0.0 - 1.0).</summary>
    public double Frequency { get; init; }

    /// <summary>Success rate when applied.</summary>
    public double SuccessRate { get; init; }

    /// <summary>Context triggers.</summary>
    public IReadOnlyList<string> Triggers { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Identity update event.
/// </summary>
public class IdentityUpdate
{
    /// <summary>Update type.</summary>
    public required string UpdateType { get; init; }

    /// <summary>Field updated.</summary>
    public required string Field { get; init; }

    /// <summary>Previous value.</summary>
    public string? PreviousValue { get; init; }

    /// <summary>New value.</summary>
    public string? NewValue { get; init; }

    /// <summary>Reason for update.</summary>
    public string? Reason { get; init; }

    /// <summary>When updated.</summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Port for self-identity accumulation.
/// Implements the "Self-Identity Accumulation" pattern for agent continuity.
/// </summary>
public interface IAgentIdentityPort
{
    /// <summary>
    /// Gets an agent's identity.
    /// </summary>
    Task<AgentIdentity?> GetIdentityAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates initial identity for an agent.
    /// </summary>
    Task<AgentIdentity> CreateIdentityAsync(
        string agentId,
        IEnumerable<string> coreTraits,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates identity from an interaction.
    /// </summary>
    Task<AgentIdentity> UpdateFromInteractionAsync(
        string agentId,
        string interactionContext,
        string interactionOutcome,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a learned preference.
    /// </summary>
    Task AddPreferenceAsync(
        string agentId,
        string key,
        string value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates interaction style.
    /// </summary>
    Task UpdateStyleAsync(
        string agentId,
        InteractionStyle style,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records domain knowledge.
    /// </summary>
    Task RecordDomainKnowledgeAsync(
        string agentId,
        string domain,
        double proficiencyDelta,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a behavioral pattern.
    /// </summary>
    Task RecordPatternAsync(
        string agentId,
        BehaviorPattern pattern,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets identity update history.
    /// </summary>
    Task<IReadOnlyList<IdentityUpdate>> GetUpdateHistoryAsync(
        string agentId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates identity summary for context.
    /// </summary>
    Task<string> GenerateSummaryAsync(
        string agentId,
        CancellationToken cancellationToken = default);
}
