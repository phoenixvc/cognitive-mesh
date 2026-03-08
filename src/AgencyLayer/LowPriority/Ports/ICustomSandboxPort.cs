namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - VM/Infrastructure Pattern
// Reason: No sandboxed background execution in scope
// Reconsideration: If isolated background agents are required
// ============================================================================

/// <summary>
/// Custom sandbox configuration.
/// </summary>
public class CustomSandboxConfig
{
    /// <summary>Sandbox name.</summary>
    public required string Name { get; init; }

    /// <summary>Base image.</summary>
    public required string BaseImage { get; init; }

    /// <summary>Resource limits.</summary>
    public Dictionary<string, string> Limits { get; init; } = new();

    /// <summary>Allowed capabilities.</summary>
    public IReadOnlyList<string> Capabilities { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for custom sandboxed background agent.
/// Implements the "Custom Sandboxed Background Agent" pattern.
///
/// This is a low-priority pattern because sandboxed background
/// execution is not in scope for current architecture.
/// </summary>
public interface ICustomSandboxPort
{
    /// <summary>Creates custom sandbox.</summary>
    Task<string> CreateSandboxAsync(CustomSandboxConfig config, CancellationToken cancellationToken = default);

    /// <summary>Runs agent in sandbox.</summary>
    Task<string> RunAgentAsync(string sandboxId, string agentId, CancellationToken cancellationToken = default);

    /// <summary>Gets sandbox output.</summary>
    Task<string> GetOutputAsync(string sandboxId, CancellationToken cancellationToken = default);

    /// <summary>Destroys sandbox.</summary>
    Task DestroySandboxAsync(string sandboxId, CancellationToken cancellationToken = default);
}
