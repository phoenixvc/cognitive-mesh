namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Type of MCP tool capability.
/// </summary>
public enum MCPCapability
{
    /// <summary>Tool can read resources.</summary>
    Read,
    /// <summary>Tool can write/create resources.</summary>
    Write,
    /// <summary>Tool can execute commands.</summary>
    Execute,
    /// <summary>Tool can search/query.</summary>
    Search,
    /// <summary>Tool can stream responses.</summary>
    Stream,
    /// <summary>Tool supports transactions.</summary>
    Transaction
}

/// <summary>
/// An MCP tool definition.
/// </summary>
public class MCPToolDefinition
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Tool description.</summary>
    public required string Description { get; init; }

    /// <summary>JSON Schema for input parameters.</summary>
    public required string InputSchema { get; init; }

    /// <summary>JSON Schema for output.</summary>
    public string? OutputSchema { get; init; }

    /// <summary>Capabilities this tool provides.</summary>
    public IReadOnlyList<MCPCapability> Capabilities { get; init; } = Array.Empty<MCPCapability>();

    /// <summary>Whether this tool requires confirmation.</summary>
    public bool RequiresConfirmation { get; init; }

    /// <summary>Estimated execution time.</summary>
    public TimeSpan? EstimatedDuration { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Server that provides this tool.</summary>
    public string? ServerName { get; init; }
}

/// <summary>
/// An MCP server connection.
/// </summary>
public class MCPServerConnection
{
    /// <summary>Server name.</summary>
    public required string Name { get; init; }

    /// <summary>Server URL or transport.</summary>
    public required string Endpoint { get; init; }

    /// <summary>Whether connected.</summary>
    public bool IsConnected { get; init; }

    /// <summary>Tools provided by this server.</summary>
    public IReadOnlyList<string> ToolIds { get; init; } = Array.Empty<string>();

    /// <summary>Server version.</summary>
    public string? Version { get; init; }

    /// <summary>Last health check.</summary>
    public DateTimeOffset? LastHealthCheck { get; init; }
}

/// <summary>
/// Request to invoke an MCP tool.
/// </summary>
public class MCPToolInvocation
{
    /// <summary>Invocation identifier.</summary>
    public string InvocationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Tool to invoke.</summary>
    public required string ToolId { get; init; }

    /// <summary>Input parameters (JSON).</summary>
    public required string Input { get; init; }

    /// <summary>Timeout for the invocation.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Context for the invocation.</summary>
    public string? Context { get; init; }

    /// <summary>Whether to stream the response.</summary>
    public bool StreamResponse { get; init; }
}

/// <summary>
/// Result of an MCP tool invocation.
/// </summary>
public class MCPToolResult
{
    /// <summary>Invocation identifier.</summary>
    public required string InvocationId { get; init; }

    /// <summary>Tool that was invoked.</summary>
    public required string ToolId { get; init; }

    /// <summary>Whether invocation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Output from the tool (JSON).</summary>
    public string? Output { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Error code if failed.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>Duration of the invocation.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Whether output was truncated.</summary>
    public bool WasTruncated { get; init; }

    /// <summary>Metadata from the tool.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Configuration for MCP tool interface.
/// </summary>
public class MCPConfiguration
{
    /// <summary>Default timeout for tool invocations.</summary>
    public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>Maximum concurrent invocations.</summary>
    public int MaxConcurrentInvocations { get; init; } = 10;

    /// <summary>Whether to enable caching.</summary>
    public bool EnableCaching { get; init; } = true;

    /// <summary>Cache TTL.</summary>
    public TimeSpan CacheTTL { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Whether to auto-retry on transient failures.</summary>
    public bool EnableAutoRetry { get; init; } = true;

    /// <summary>Maximum retry attempts.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Tools that require explicit confirmation.</summary>
    public IReadOnlyList<string> ConfirmationRequiredTools { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for MCP (Model Context Protocol) tool interface.
/// Implements the "Code Mode MCP Tool Interface Improvement" pattern.
/// </summary>
/// <remarks>
/// This port provides a standardized interface for MCP tools,
/// enabling discovery, invocation, and management of tools
/// across multiple MCP servers.
/// </remarks>
public interface IMCPToolPort
{
    /// <summary>
    /// Discovers available tools.
    /// </summary>
    /// <param name="capabilities">Filter by capabilities (null = all).</param>
    /// <param name="tags">Filter by tags (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Available tools.</returns>
    Task<IReadOnlyList<MCPToolDefinition>> DiscoverToolsAsync(
        IEnumerable<MCPCapability>? capabilities = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific tool definition.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The tool definition.</returns>
    Task<MCPToolDefinition?> GetToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a tool.
    /// </summary>
    /// <param name="invocation">The invocation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The invocation result.</returns>
    Task<MCPToolResult> InvokeAsync(
        MCPToolInvocation invocation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes a tool with streaming response.
    /// </summary>
    /// <param name="invocation">The invocation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of output chunks.</returns>
    IAsyncEnumerable<string> InvokeStreamingAsync(
        MCPToolInvocation invocation,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects to an MCP server.
    /// </summary>
    /// <param name="name">Server name.</param>
    /// <param name="endpoint">Server endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The connection.</returns>
    Task<MCPServerConnection> ConnectServerAsync(
        string name,
        string endpoint,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects from an MCP server.
    /// </summary>
    /// <param name="name">Server name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DisconnectServerAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets connected servers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Connected servers.</returns>
    Task<IReadOnlyList<MCPServerConnection>> GetServersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates tool input against schema.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="input">Input to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether valid and any errors.</returns>
    Task<(bool IsValid, IReadOnlyList<string> Errors)> ValidateInputAsync(
        string toolId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets configuration.
    /// </summary>
    /// <param name="configuration">Configuration to set (null = get).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<MCPConfiguration> ConfigureAsync(
        MCPConfiguration? configuration = null,
        CancellationToken cancellationToken = default);
}
