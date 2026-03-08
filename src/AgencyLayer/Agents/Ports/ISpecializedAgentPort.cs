namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Type of specialized agent.
/// </summary>
public enum SpecializedAgentType
{
    /// <summary>Research and information gathering.</summary>
    Researcher,
    /// <summary>Code generation and editing.</summary>
    Coder,
    /// <summary>Documentation and writing.</summary>
    Writer,
    /// <summary>Testing and validation.</summary>
    Tester,
    /// <summary>Review and critique.</summary>
    Reviewer,
    /// <summary>Planning and orchestration.</summary>
    Planner,
    /// <summary>Data analysis.</summary>
    Analyst,
    /// <summary>Domain expert.</summary>
    Expert,
    /// <summary>Custom specialized agent.</summary>
    Custom
}

/// <summary>
/// Configuration for a specialized agent.
/// </summary>
public class SpecializedAgentConfiguration
{
    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Agent name.</summary>
    public required string Name { get; init; }

    /// <summary>Specialization type.</summary>
    public SpecializedAgentType Type { get; init; }

    /// <summary>System prompt/persona.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }

    /// <summary>Available tools.</summary>
    public IReadOnlyList<string> Tools { get; init; } = Array.Empty<string>();

    /// <summary>Capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();

    /// <summary>Maximum tokens per response.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Temperature for generation.</summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>Knowledge domains.</summary>
    public IReadOnlyList<string> Domains { get; init; } = Array.Empty<string>();

    /// <summary>Backstory for persona.</summary>
    public string? Backstory { get; init; }

    /// <summary>Goals for the agent.</summary>
    public IReadOnlyList<string> Goals { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A task for a specialized agent.
/// </summary>
public class SpecializedTask
{
    /// <summary>Task identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task description.</summary>
    public required string Description { get; init; }

    /// <summary>Expected output description.</summary>
    public string? ExpectedOutput { get; init; }

    /// <summary>Context from previous tasks.</summary>
    public string? Context { get; init; }

    /// <summary>Priority.</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Timeout.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result from a specialized agent.
/// </summary>
public class SpecializedTaskResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Agent that executed.</summary>
    public required string AgentId { get; init; }

    /// <summary>Whether successful.</summary>
    public required bool Success { get; init; }

    /// <summary>Output.</summary>
    public string? Output { get; init; }

    /// <summary>Artifacts produced.</summary>
    public IReadOnlyList<TaskArtifact> Artifacts { get; init; } = Array.Empty<TaskArtifact>();

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Tokens used.</summary>
    public int TokensUsed { get; init; }

    /// <summary>Tools invoked.</summary>
    public IReadOnlyList<string> ToolsUsed { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An artifact produced by a task.
/// </summary>
public class TaskArtifact
{
    /// <summary>Artifact name.</summary>
    public required string Name { get; init; }

    /// <summary>Artifact type (code, document, data, etc.).</summary>
    public required string Type { get; init; }

    /// <summary>Content.</summary>
    public required string Content { get; init; }

    /// <summary>File path if applicable.</summary>
    public string? FilePath { get; init; }
}

/// <summary>
/// Port for specialized agent management (CrewAI-style).
/// Implements the "Specialized Agent Personas" pattern.
/// </summary>
public interface ISpecializedAgentPort
{
    /// <summary>
    /// Registers a specialized agent.
    /// </summary>
    Task RegisterAgentAsync(
        SpecializedAgentConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a registered agent.
    /// </summary>
    Task<SpecializedAgentConfiguration?> GetAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists registered agents.
    /// </summary>
    Task<IReadOnlyList<SpecializedAgentConfiguration>> ListAgentsAsync(
        SpecializedAgentType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a task with a specific agent.
    /// </summary>
    Task<SpecializedTaskResult> ExecuteTaskAsync(
        string agentId,
        SpecializedTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds best agent for a task.
    /// </summary>
    Task<string?> FindBestAgentAsync(
        SpecializedTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delegates a task to the best available agent.
    /// </summary>
    Task<SpecializedTaskResult> DelegateTaskAsync(
        SpecializedTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an agent team for complex tasks.
    /// </summary>
    Task<string> CreateTeamAsync(
        IEnumerable<string> agentIds,
        string teamGoal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a task with a team (sequential or parallel).
    /// </summary>
    Task<IReadOnlyList<SpecializedTaskResult>> ExecuteWithTeamAsync(
        string teamId,
        IEnumerable<SpecializedTask> tasks,
        bool parallel = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters an agent.
    /// </summary>
    Task UnregisterAgentAsync(
        string agentId,
        CancellationToken cancellationToken = default);
}
