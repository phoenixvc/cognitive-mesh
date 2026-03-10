namespace AgencyLayer.AgentTeamFramework.Prompts;

/// <summary>
/// Immutable prompt template loaded from an external source (e.g., YAML file).
/// Contains the prompt text along with metadata for versioning and configuration defaults.
/// </summary>
public sealed class PromptTemplate
{
    /// <summary>
    /// Unique identifier for this prompt (e.g., "roadmapcrew-vision-keeper").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Semantic version string for tracking prompt iterations (e.g., "1.0", "2.1").
    /// </summary>
    public string Version { get; init; } = "1.0";

    /// <summary>
    /// Human-readable name for display and logging purposes.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Brief description of what this prompt instructs the agent to do.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// The system prompt text sent to the LLM.
    /// </summary>
    public required string Prompt { get; init; }

    /// <summary>
    /// Categorization tags for filtering and discovery (e.g., ["roadmap", "vision"]).
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// Default configuration values that can be overridden by the agent definition.
    /// </summary>
    public PromptDefaults Defaults { get; init; } = new();
}

/// <summary>
/// Default configuration values associated with a prompt template.
/// These are used when the <see cref="AgencyLayer.AgentTeamFramework.Configuration.AgentDefinitionRecord"/>
/// does not specify explicit overrides.
/// </summary>
public sealed class PromptDefaults
{
    /// <summary>
    /// Default sampling temperature for this prompt (0.0–1.0).
    /// </summary>
    public double? Temperature { get; init; }

    /// <summary>
    /// Default maximum tokens for the response.
    /// </summary>
    public int? MaxTokens { get; init; }

    /// <summary>
    /// Default model identifier (e.g., "gpt-4o", "claude-3-opus").
    /// </summary>
    public string? Model { get; init; }
}
