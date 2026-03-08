namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A dual-use tool definition.
/// </summary>
public class DualUseTool
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Human-facing interface definition.</summary>
    public required HumanInterface HumanInterface { get; init; }

    /// <summary>Agent-facing interface definition.</summary>
    public required AgentInterface AgentInterface { get; init; }

    /// <summary>Shared implementation reference.</summary>
    public required string ImplementationRef { get; init; }

    /// <summary>Category.</summary>
    public string? Category { get; init; }

    /// <summary>Whether enabled.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Human-facing interface.
/// </summary>
public class HumanInterface
{
    /// <summary>Display name.</summary>
    public required string DisplayName { get; init; }

    /// <summary>User-friendly description.</summary>
    public required string Description { get; init; }

    /// <summary>UI components.</summary>
    public IReadOnlyList<UIComponent> Components { get; init; } = Array.Empty<UIComponent>();

    /// <summary>Help text.</summary>
    public string? HelpText { get; init; }

    /// <summary>Example usage.</summary>
    public string? ExampleUsage { get; init; }
}

/// <summary>
/// Agent-facing interface.
/// </summary>
public class AgentInterface
{
    /// <summary>Tool schema (JSON).</summary>
    public required string Schema { get; init; }

    /// <summary>Agent description.</summary>
    public required string Description { get; init; }

    /// <summary>Example invocations.</summary>
    public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();

    /// <summary>Required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();
}

/// <summary>
/// UI component for human interface.
/// </summary>
public class UIComponent
{
    /// <summary>Component type.</summary>
    public required string Type { get; init; }

    /// <summary>Property name.</summary>
    public required string Property { get; init; }

    /// <summary>Label.</summary>
    public required string Label { get; init; }

    /// <summary>Placeholder.</summary>
    public string? Placeholder { get; init; }

    /// <summary>Whether required.</summary>
    public bool Required { get; init; }

    /// <summary>Validation rules.</summary>
    public Dictionary<string, string> Validation { get; init; } = new();
}

/// <summary>
/// Tool execution context.
/// </summary>
public class ToolExecutionContext
{
    /// <summary>Whether executed by agent.</summary>
    public bool IsAgentExecution { get; init; }

    /// <summary>User identifier.</summary>
    public string? UserId { get; init; }

    /// <summary>Agent identifier.</summary>
    public string? AgentId { get; init; }

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }

    /// <summary>Additional context.</summary>
    public Dictionary<string, string> Context { get; init; } = new();
}

/// <summary>
/// Tool execution result.
/// </summary>
public class ToolExecutionResult
{
    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Result data.</summary>
    public object? Data { get; init; }

    /// <summary>Human-readable result.</summary>
    public string? HumanReadable { get; init; }

    /// <summary>Agent-consumable result.</summary>
    public string? AgentReadable { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Execution time.</summary>
    public TimeSpan ExecutionTime { get; init; }
}

/// <summary>
/// Port for dual-use tool design.
/// Implements the "Dual-Use Tool Design" pattern for human+agent tools.
/// </summary>
public interface IDualUseToolPort
{
    /// <summary>
    /// Registers a dual-use tool.
    /// </summary>
    Task RegisterToolAsync(
        DualUseTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tool.
    /// </summary>
    Task<DualUseTool?> GetToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists available tools.
    /// </summary>
    Task<IReadOnlyList<DualUseTool>> ListToolsAsync(
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool for human use.
    /// </summary>
    Task<ToolExecutionResult> ExecuteForHumanAsync(
        string toolId,
        Dictionary<string, object> parameters,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool for agent use.
    /// </summary>
    Task<ToolExecutionResult> ExecuteForAgentAsync(
        string toolId,
        string parametersJson,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets human interface for a tool.
    /// </summary>
    Task<HumanInterface?> GetHumanInterfaceAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets agent interface for a tool.
    /// </summary>
    Task<AgentInterface?> GetAgentInterfaceAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tool.
    /// </summary>
    Task UpdateToolAsync(
        DualUseTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a tool.
    /// </summary>
    Task UnregisterToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);
}
