using AgencyLayer.Agents.Ports;

namespace AgencyLayer.AgentTeamFramework.Configuration;

/// <summary>
/// Declarative definition of an agent within a team. Replaces per-agent registration
/// methods with a data-driven approach — each team defines its agents as a collection
/// of these records, and the base class registers them all.
/// </summary>
public record AgentDefinitionRecord
{
    /// <summary>Unique agent identifier (e.g., "roadmapcrew-vision-keeper").</summary>
    public required string AgentId { get; init; }

    /// <summary>Human-readable agent name.</summary>
    public required string Name { get; init; }

    /// <summary>Agent specialization type.</summary>
    public required SpecializedAgentType Type { get; init; }

    /// <summary>System prompt defining the agent's persona and instructions.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>Temperature for LLM generation (0.0–1.0).</summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>Knowledge domains the agent covers.</summary>
    public IReadOnlyList<string> Domains { get; init; } = [];

    /// <summary>Goals that guide the agent's behavior.</summary>
    public IReadOnlyList<string> Goals { get; init; } = [];

    /// <summary>Model to use (null for default).</summary>
    public string? Model { get; init; }

    /// <summary>Maximum tokens per response.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Available tools for the agent.</summary>
    public IReadOnlyList<string> Tools { get; init; } = [];

    /// <summary>Agent capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = [];

    /// <summary>Optional backstory for persona.</summary>
    public string? Backstory { get; init; }
}
