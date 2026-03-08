namespace AgencyLayer.Agents.Ports;

/// <summary>
/// An agent specification.
/// </summary>
public class AgentSpecification
{
    /// <summary>Specification identifier.</summary>
    /// <summary>Spec identifier.</summary>
    public string SpecId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Agent name.</summary>
    public required string Name { get; init; }

    /// <summary>Agent description.</summary>
    public required string Description { get; init; }

    /// <summary>Agent role/persona.</summary>
    public required string Role { get; init; }

    /// <summary>Capabilities this agent should have.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Tools the agent can use.</summary>
    public IReadOnlyList<string> Tools { get; init; } = Array.Empty<string>();

    /// <summary>Constraints/limitations.</summary>
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();

    /// <summary>Example interactions.</summary>
    public IReadOnlyList<AgentInteractionExample> Examples { get; init; } = Array.Empty<AgentInteractionExample>();

    /// <summary>Success criteria.</summary>
    public IReadOnlyList<string> SuccessCriteria { get; init; } = Array.Empty<string>();

    /// <summary>Model preferences.</summary>
    public ModelPreferences? ModelPreferences { get; init; }

    /// <summary>Version.</summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// An example interaction for the agent.
/// </summary>
public class AgentInteractionExample
{
    /// <summary>Input/prompt.</summary>
    public required string Input { get; init; }

    /// <summary>Expected output.</summary>
    public required string ExpectedOutput { get; init; }

    /// <summary>Context for the example.</summary>
    public string? Context { get; init; }
}

/// <summary>
/// Model preferences for an agent.
/// </summary>
public class ModelPreferences
{
    /// <summary>Preferred model ID.</summary>
    public string? PreferredModel { get; init; }

    /// <summary>Fallback models.</summary>
    public IReadOnlyList<string> FallbackModels { get; init; } = Array.Empty<string>();

    /// <summary>Temperature.</summary>
    public double? Temperature { get; init; }

    /// <summary>Max tokens.</summary>
    public int? MaxTokens { get; init; }
}

/// <summary>
/// Result of agent generation from specification.
/// </summary>
public class GeneratedAgent
{
    /// <summary>Agent identifier.</summary>
    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Specification used.</summary>
    public required string SpecId { get; init; }

    /// <summary>System prompt generated.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>Tool configurations.</summary>
    public IReadOnlyList<ToolConfiguration> ToolConfigurations { get; init; } = Array.Empty<ToolConfiguration>();

    /// <summary>Generated at.</summary>
    public DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Whether agent is ready.</summary>
    public bool IsReady { get; init; }
}

/// <summary>
/// Tool configuration for agent.
/// </summary>
public class ToolConfiguration
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Configuration JSON.</summary>
    public string? Configuration { get; init; }

    /// <summary>Whether enabled.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>
/// Port for specification-driven agent development.
/// Implements the "Specification-Driven Agent Development" pattern.
/// </summary>
public interface ISpecDrivenAgentPort
{
    /// <summary>
    /// Creates an agent specification.
    /// </summary>
    Task<AgentSpecification> CreateSpecificationAsync(
        AgentSpecification specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an agent specification.
    /// </summary>
    Task<AgentSpecification?> GetSpecificationAsync(
        string specId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an agent specification.
    /// </summary>
    Task<AgentSpecification> UpdateSpecificationAsync(
        AgentSpecification specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an agent from specification.
    /// </summary>
    Task<GeneratedAgent> GenerateAgentAsync(
        string specId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a specification.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateSpecificationAsync(
        AgentSpecification specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests an agent against its specification.
    /// </summary>
    Task<SpecTestResult> TestAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists specifications.
    /// </summary>
    Task<IReadOnlyList<AgentSpecification>> ListSpecificationsAsync(
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specification.
    /// </summary>
    Task DeleteSpecificationAsync(
        string specId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of testing an agent against its specification.
/// </summary>
public class SpecTestResult
{
    public required string AgentId { get; init; }
    public required string SpecId { get; init; }
    /// <summary>Whether passed.</summary>
    public bool Passed { get; init; }
    /// <summary>Examples passed.</summary>
    public int ExamplesPassed { get; init; }
    /// <summary>Examples failed.</summary>
    public int ExamplesFailed { get; init; }
    /// <summary>Failure reasons.</summary>
    public IReadOnlyList<string> FailureReasons { get; init; } = Array.Empty<string>();
    /// <summary>Tested at.</summary>
    public DateTimeOffset TestedAt { get; init; }
}
