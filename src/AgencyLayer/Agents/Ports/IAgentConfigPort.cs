namespace AgencyLayer.Agents.Ports;

/// <summary>
/// An agent configuration.
/// </summary>
public class AgentConfiguration
{
    /// <summary>Configuration identifier.</summary>
    public required string ConfigId { get; init; }

    /// <summary>Agent name.</summary>
    public required string AgentName { get; init; }

    /// <summary>Agent type.</summary>
    public required string AgentType { get; init; }

    /// <summary>Version.</summary>
    public required string Version { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>System prompt.</summary>
    public required string SystemPrompt { get; init; }

    /// <summary>Model configuration.</summary>
    public ModelConfiguration Model { get; init; } = new();

    /// <summary>Tool configurations.</summary>
    public IReadOnlyList<AgentToolConfig> Tools { get; init; } = Array.Empty<AgentToolConfig>();

    /// <summary>Environment variables.</summary>
    public Dictionary<string, string> Environment { get; init; } = new();

    /// <summary>Feature flags.</summary>
    public Dictionary<string, bool> Features { get; init; } = new();

    /// <summary>Constraints.</summary>
    public AgentConstraints Constraints { get; init; } = new();

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Updated at.</summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Created by.</summary>
    public required string CreatedBy { get; init; }
}

/// <summary>
/// Model configuration.
/// </summary>
public class ModelConfiguration
{
    /// <summary>Model identifier.</summary>
    public string ModelId { get; init; } = "default";

    /// <summary>Temperature.</summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>Max tokens.</summary>
    public int MaxTokens { get; init; } = 4096;

    /// <summary>Top P.</summary>
    public double TopP { get; init; } = 1.0;

    /// <summary>Fallback models.</summary>
    public IReadOnlyList<string> FallbackModels { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Tool configuration for an agent.
/// </summary>
public class AgentToolConfig
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Whether enabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Tool-specific settings.</summary>
    public Dictionary<string, string> Settings { get; init; } = new();

    /// <summary>Rate limits.</summary>
    public ToolRateLimit? RateLimit { get; init; }
}

/// <summary>
/// Tool rate limit.
/// </summary>
public class ToolRateLimit
{
    /// <summary>Max calls per minute.</summary>
    public int MaxCallsPerMinute { get; init; } = 60;

    /// <summary>Max calls per hour.</summary>
    public int MaxCallsPerHour { get; init; } = 1000;
}

/// <summary>
/// Agent constraints.
/// </summary>
public class AgentConstraints
{
    /// <summary>Maximum response length.</summary>
    public int MaxResponseLength { get; init; } = 10000;

    /// <summary>Maximum tool calls per turn.</summary>
    public int MaxToolCallsPerTurn { get; init; } = 10;

    /// <summary>Timeout per request.</summary>
    public TimeSpan RequestTimeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Allowed domains.</summary>
    public IReadOnlyList<string> AllowedDomains { get; init; } = Array.Empty<string>();

    /// <summary>Blocked actions.</summary>
    public IReadOnlyList<string> BlockedActions { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Configuration change event.
/// </summary>
public class ConfigChangeEvent
{
    /// <summary>Config identifier.</summary>
    public required string ConfigId { get; init; }

    /// <summary>Change type.</summary>
    public required string ChangeType { get; init; }

    /// <summary>Changed by.</summary>
    public required string ChangedBy { get; init; }

    /// <summary>Previous version.</summary>
    public string? PreviousVersion { get; init; }

    /// <summary>New version.</summary>
    public required string NewVersion { get; init; }

    /// <summary>Diff summary.</summary>
    public string? DiffSummary { get; init; }

    /// <summary>Changed at.</summary>
    public DateTimeOffset ChangedAt { get; init; }
}

/// <summary>
/// Port for team-shared agent configuration as code.
/// Implements the "Team-Shared Agent Configuration as Code" pattern.
/// </summary>
public interface IAgentConfigPort
{
    /// <summary>
    /// Gets an agent configuration.
    /// </summary>
    Task<AgentConfiguration?> GetConfigurationAsync(
        string agentName,
        string? version = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves an agent configuration.
    /// </summary>
    Task<AgentConfiguration> SaveConfigurationAsync(
        AgentConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an agent configuration.
    /// </summary>
    Task<AgentConfiguration> UpdateConfigurationAsync(
        string agentName,
        Action<AgentConfiguration> update,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all configurations.
    /// </summary>
    Task<IReadOnlyList<AgentConfiguration>> ListConfigurationsAsync(
        string? agentType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configuration history.
    /// </summary>
    Task<IReadOnlyList<ConfigChangeEvent>> GetHistoryAsync(
        string agentName,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two versions.
    /// </summary>
    Task<string> CompareVersionsAsync(
        string agentName,
        string version1,
        string version2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back to a previous version.
    /// </summary>
    Task<AgentConfiguration> RollbackAsync(
        string agentName,
        string targetVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a configuration.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateConfigurationAsync(
        AgentConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports configuration as YAML/JSON.
    /// </summary>
    Task<string> ExportConfigurationAsync(
        string agentName,
        string format = "yaml",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports configuration from YAML/JSON.
    /// </summary>
    Task<AgentConfiguration> ImportConfigurationAsync(
        string content,
        string format = "yaml",
        CancellationToken cancellationToken = default);
}
