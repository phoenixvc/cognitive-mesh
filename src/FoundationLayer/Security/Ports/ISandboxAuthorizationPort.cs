namespace FoundationLayer.Security.Ports;

/// <summary>
/// Types of sandbox environments.
/// </summary>
public enum SandboxType
{
    /// <summary>Process-level isolation.</summary>
    Process,
    /// <summary>Container-based isolation.</summary>
    Container,
    /// <summary>Virtual machine isolation.</summary>
    VirtualMachine,
    /// <summary>WebAssembly sandbox.</summary>
    WebAssembly,
    /// <summary>Interpreter sandbox (e.g., restricted Python).</summary>
    Interpreter
}

/// <summary>
/// Permissions that can be granted to a sandboxed tool.
/// </summary>
[Flags]
public enum SandboxPermission
{
    None = 0,
    FileSystemRead = 1,
    FileSystemWrite = 2,
    NetworkOutbound = 4,
    NetworkInbound = 8,
    ProcessSpawn = 16,
    EnvironmentVariables = 32,
    SystemCalls = 64,
    MemoryUnlimited = 128,
    CpuUnlimited = 256
}

/// <summary>
/// Resource limits for a sandbox.
/// </summary>
public class SandboxResourceLimits
{
    /// <summary>Maximum memory in bytes (null = default).</summary>
    public long? MaxMemoryBytes { get; init; }

    /// <summary>Maximum CPU time in milliseconds (null = default).</summary>
    public int? MaxCpuTimeMs { get; init; }

    /// <summary>Maximum wall-clock time in milliseconds (null = default).</summary>
    public int? MaxWallTimeMs { get; init; }

    /// <summary>Maximum file size in bytes (null = default).</summary>
    public long? MaxFileSizeBytes { get; init; }

    /// <summary>Maximum number of open file descriptors (null = default).</summary>
    public int? MaxOpenFiles { get; init; }

    /// <summary>Maximum number of processes/threads (null = default).</summary>
    public int? MaxProcesses { get; init; }

    /// <summary>Maximum network bandwidth in bytes/second (null = unlimited).</summary>
    public long? MaxNetworkBandwidth { get; init; }
}

/// <summary>
/// Configuration for a sandbox environment.
/// </summary>
public class SandboxConfiguration
{
    /// <summary>Unique identifier for this sandbox configuration.</summary>
    public string ConfigurationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable name.</summary>
    public required string Name { get; init; }

    /// <summary>Type of sandbox to create.</summary>
    public required SandboxType Type { get; init; }

    /// <summary>Permissions granted to the sandbox.</summary>
    public SandboxPermission Permissions { get; init; } = SandboxPermission.None;

    /// <summary>Resource limits.</summary>
    public SandboxResourceLimits? ResourceLimits { get; init; }

    /// <summary>Allowed filesystem paths (when FileSystemRead/Write is enabled).</summary>
    public IReadOnlyList<string> AllowedPaths { get; init; } = Array.Empty<string>();

    /// <summary>Allowed network destinations (when NetworkOutbound is enabled).</summary>
    public IReadOnlyList<string> AllowedNetworkDestinations { get; init; } = Array.Empty<string>();

    /// <summary>Environment variables to inject.</summary>
    public Dictionary<string, string> EnvironmentVariables { get; init; } = new();

    /// <summary>Whether to capture stdout/stderr.</summary>
    public bool CaptureOutput { get; init; } = true;
}

/// <summary>
/// Request to authorize a tool for sandboxed execution.
/// </summary>
public class SandboxAuthorizationRequest
{
    /// <summary>The tool identifier requesting sandbox access.</summary>
    public required string ToolId { get; init; }

    /// <summary>The agent requesting on behalf of.</summary>
    public required string AgentId { get; init; }

    /// <summary>Requested sandbox configuration.</summary>
    public required SandboxConfiguration RequestedConfiguration { get; init; }

    /// <summary>Justification for the requested permissions.</summary>
    public string? Justification { get; init; }

    /// <summary>Maximum duration for the sandbox session.</summary>
    public TimeSpan? MaxDuration { get; init; }
}

/// <summary>
/// Result of sandbox authorization.
/// </summary>
public class SandboxAuthorizationResult
{
    /// <summary>Whether authorization was granted.</summary>
    public required bool IsAuthorized { get; init; }

    /// <summary>The approved sandbox configuration (may differ from requested).</summary>
    public SandboxConfiguration? ApprovedConfiguration { get; init; }

    /// <summary>Session token for the sandbox.</summary>
    public string? SessionToken { get; init; }

    /// <summary>When the authorization expires.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Reason if authorization was denied.</summary>
    public string? DenialReason { get; init; }

    /// <summary>Permissions that were denied from the request.</summary>
    public SandboxPermission DeniedPermissions { get; init; }
}

/// <summary>
/// Result of sandbox execution.
/// </summary>
public class SandboxExecutionResult
{
    /// <summary>Whether execution completed successfully.</summary>
    public required bool Success { get; init; }

    /// <summary>Exit code of the sandboxed process.</summary>
    public int? ExitCode { get; init; }

    /// <summary>Captured stdout.</summary>
    public string? StandardOutput { get; init; }

    /// <summary>Captured stderr.</summary>
    public string? StandardError { get; init; }

    /// <summary>Execution duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Resource usage statistics.</summary>
    public SandboxResourceUsage? ResourceUsage { get; init; }

    /// <summary>Error message if execution failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Security violations detected during execution.</summary>
    public IReadOnlyList<string> SecurityViolations { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Resource usage during sandbox execution.
/// </summary>
public class SandboxResourceUsage
{
    public long PeakMemoryBytes { get; init; }
    public long CpuTimeMs { get; init; }
    public long NetworkBytesSent { get; init; }
    public long NetworkBytesReceived { get; init; }
    public int FilesCreated { get; init; }
    public int ProcessesSpawned { get; init; }
}

/// <summary>
/// Port for sandboxed tool authorization and execution.
/// Implements the "Sandboxed Tool Authorization" pattern.
/// </summary>
/// <remarks>
/// This port enforces the principle of least privilege by requiring explicit
/// authorization for tool execution and providing resource-limited sandbox
/// environments. All tool executions are isolated and monitored.
/// </remarks>
public interface ISandboxAuthorizationPort
{
    /// <summary>
    /// Authorizes a tool for sandboxed execution.
    /// </summary>
    /// <param name="request">The authorization request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization result.</returns>
    Task<SandboxAuthorizationResult> AuthorizeAsync(
        SandboxAuthorizationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a command in an authorized sandbox.
    /// </summary>
    /// <param name="sessionToken">The session token from authorization.</param>
    /// <param name="command">The command to execute.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <param name="workingDirectory">Working directory (must be in allowed paths).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The execution result.</returns>
    Task<SandboxExecutionResult> ExecuteInSandboxAsync(
        string sessionToken,
        string command,
        IEnumerable<string>? arguments = null,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates an active sandbox session.
    /// </summary>
    /// <param name="sessionToken">The session token.</param>
    /// <param name="reason">Reason for termination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task TerminateSessionAsync(
        string sessionToken,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the default sandbox configuration for a tool type.
    /// </summary>
    /// <param name="toolType">The tool type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The default configuration.</returns>
    Task<SandboxConfiguration> GetDefaultConfigurationAsync(
        string toolType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active sandbox sessions.
    /// </summary>
    /// <param name="agentId">Filter by agent (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of active session tokens with metadata.</returns>
    Task<IReadOnlyList<(string SessionToken, string ToolId, DateTimeOffset CreatedAt)>> ListActiveSessionsAsync(
        string? agentId = null,
        CancellationToken cancellationToken = default);
}
