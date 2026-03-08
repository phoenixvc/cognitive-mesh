namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A discovered tool.
/// </summary>
public class DiscoveredTool
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Source (MCP server, API, etc.).</summary>
    public required string Source { get; init; }

    /// <summary>Category.</summary>
    public string? Category { get; init; }

    /// <summary>Input schema (JSON).</summary>
    public string? InputSchema { get; init; }

    /// <summary>Output schema (JSON).</summary>
    public string? OutputSchema { get; init; }

    /// <summary>Whether on allowlist.</summary>
    public bool IsAllowlisted { get; init; }

    /// <summary>Whether verified.</summary>
    public bool IsVerified { get; init; }

    /// <summary>Trust level.</summary>
    public ToolTrustLevel TrustLevel { get; init; }

    /// <summary>Discovered at.</summary>
    public DateTimeOffset DiscoveredAt { get; init; }

    /// <summary>Last seen.</summary>
    public DateTimeOffset LastSeen { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Tool trust level.
/// </summary>
public enum ToolTrustLevel
{
    /// <summary>Unknown/untrusted.</summary>
    Unknown = 0,
    /// <summary>Discovered but not verified.</summary>
    Discovered = 1,
    /// <summary>Verified safe.</summary>
    Verified = 2,
    /// <summary>Trusted/allowlisted.</summary>
    Trusted = 3,
    /// <summary>Core system tool.</summary>
    Core = 4
}

/// <summary>
/// Tool discovery configuration.
/// </summary>
public class DiscoveryConfiguration
{
    /// <summary>Sources to scan.</summary>
    public IReadOnlyList<string> Sources { get; init; } = Array.Empty<string>();

    /// <summary>Categories to discover.</summary>
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    /// <summary>Whether to auto-allowlist.</summary>
    public bool AutoAllowlist { get; init; }

    /// <summary>Minimum trust level to enable.</summary>
    public ToolTrustLevel MinTrustLevel { get; init; } = ToolTrustLevel.Verified;

    /// <summary>Whether to verify discovered tools.</summary>
    public bool VerifyOnDiscovery { get; init; } = true;

    /// <summary>Discovery interval.</summary>
    public TimeSpan DiscoveryInterval { get; init; } = TimeSpan.FromHours(1);
}

/// <summary>
/// Discovery result.
/// </summary>
public class DiscoveryResult
{
    /// <summary>Newly discovered tools.</summary>
    public IReadOnlyList<DiscoveredTool> NewTools { get; init; } = Array.Empty<DiscoveredTool>();

    /// <summary>Updated tools.</summary>
    public IReadOnlyList<DiscoveredTool> UpdatedTools { get; init; } = Array.Empty<DiscoveredTool>();

    /// <summary>Removed tools.</summary>
    public IReadOnlyList<string> RemovedToolIds { get; init; } = Array.Empty<string>();

    /// <summary>Errors during discovery.</summary>
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    /// <summary>Discovery duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>When discovered.</summary>
    public DateTimeOffset DiscoveredAt { get; init; }
}

/// <summary>
/// Port for progressive tool discovery.
/// Implements the "Progressive Tool Discovery" pattern.
/// </summary>
public interface IToolDiscoveryPort
{
    /// <summary>
    /// Discovers tools from configured sources.
    /// </summary>
    Task<DiscoveryResult> DiscoverAsync(
        DiscoveryConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers tools from a specific source.
    /// </summary>
    Task<DiscoveryResult> DiscoverFromSourceAsync(
        string source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets discovered tools.
    /// </summary>
    Task<IReadOnlyList<DiscoveredTool>> GetDiscoveredToolsAsync(
        ToolTrustLevel? minTrustLevel = null,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a discovered tool.
    /// </summary>
    Task<(bool Verified, IReadOnlyList<string> Issues)> VerifyToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a tool to the allowlist.
    /// </summary>
    Task AllowlistToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tool from the allowlist.
    /// </summary>
    Task RemoveFromAllowlistAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables a discovered tool for use.
    /// </summary>
    Task EnableToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables a discovered tool.
    /// </summary>
    Task DisableToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts continuous discovery.
    /// </summary>
    Task StartContinuousDiscoveryAsync(
        DiscoveryConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops continuous discovery.
    /// </summary>
    Task StopContinuousDiscoveryAsync(
        CancellationToken cancellationToken = default);
}
