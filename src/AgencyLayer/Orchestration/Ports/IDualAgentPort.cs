namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Agent role in initializer-maintainer pattern.
/// </summary>
public enum AgentRole
{
    /// <summary>Initializer agent - sets up initial state.</summary>
    Initializer,
    /// <summary>Maintainer agent - handles ongoing operations.</summary>
    Maintainer
}

/// <summary>
/// Configuration for initializer-maintainer dual agent.
/// </summary>
public class DualAgentConfiguration
{
    /// <summary>Configuration identifier.</summary>
    public string ConfigurationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Name of this configuration.</summary>
    public required string Name { get; init; }

    /// <summary>Model for initializer agent.</summary>
    public required string InitializerModel { get; init; }

    /// <summary>Model for maintainer agent.</summary>
    public required string MaintainerModel { get; init; }

    /// <summary>Initialization timeout.</summary>
    public TimeSpan InitializationTimeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Handoff criteria (when to switch from initializer to maintainer).</summary>
    public HandoffCriteria HandoffCriteria { get; init; } = new();

    /// <summary>Whether maintainer can request initializer for complex tasks.</summary>
    public bool AllowMaintainerEscalation { get; init; } = true;

    /// <summary>Whether to validate handoff state.</summary>
    public bool ValidateHandoff { get; init; } = true;
}

/// <summary>
/// Criteria for when to hand off from initializer to maintainer.
/// </summary>
public class HandoffCriteria
{
    /// <summary>Hand off after initial setup is complete.</summary>
    public bool OnSetupComplete { get; init; } = true;

    /// <summary>Hand off after a specific number of steps.</summary>
    public int? AfterSteps { get; init; }

    /// <summary>Hand off after a time duration.</summary>
    public TimeSpan? AfterDuration { get; init; }

    /// <summary>Hand off when complexity drops below threshold.</summary>
    public double? ComplexityThreshold { get; init; }

    /// <summary>Custom handoff condition expression.</summary>
    public string? CustomCondition { get; init; }
}

/// <summary>
/// State passed during handoff.
/// </summary>
public class DualAgentHandoffContext
{
    /// <summary>State identifier.</summary>
    public string StateId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Consolidated context from initializer.</summary>
    public required string Context { get; init; }

    /// <summary>Active tasks to continue.</summary>
    public IReadOnlyList<string> ActiveTasks { get; init; } = Array.Empty<string>();

    /// <summary>Decisions made during initialization.</summary>
    public IReadOnlyList<string> Decisions { get; init; } = Array.Empty<string>();

    /// <summary>Resources provisioned.</summary>
    public Dictionary<string, string> Resources { get; init; } = new();

    /// <summary>Configuration established.</summary>
    public Dictionary<string, string> Configuration { get; init; } = new();

    /// <summary>When handoff occurred.</summary>
    public DateTimeOffset HandoffAt { get; init; }

    /// <summary>Whether handoff was validated.</summary>
    public bool IsValidated { get; init; }
}

/// <summary>
/// Request for initializer-maintainer operation.
/// </summary>
public class DualAgentRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The task to perform.</summary>
    public required string Task { get; init; }

    /// <summary>Configuration to use.</summary>
    public required DualAgentConfiguration Configuration { get; init; }

    /// <summary>Initial context.</summary>
    public string? InitialContext { get; init; }

    /// <summary>Existing handoff state (for resuming).</summary>
    public DualAgentHandoffContext? ExistingState { get; init; }
}

/// <summary>
/// Result of dual agent operation.
/// </summary>
public class DualAgentResult
{
    /// <summary>Request identifier.</summary>
    public required string RequestId { get; init; }

    /// <summary>Whether operation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Final output.</summary>
    public string? Output { get; init; }

    /// <summary>Which agent produced the final output.</summary>
    public AgentRole FinalAgent { get; init; }

    /// <summary>Handoff state (if operation is ongoing).</summary>
    public DualAgentHandoffContext? HandoffState { get; init; }

    /// <summary>Initializer steps taken.</summary>
    public int InitializerSteps { get; init; }

    /// <summary>Maintainer steps taken.</summary>
    public int MaintainerSteps { get; init; }

    /// <summary>Escalations to initializer.</summary>
    public int Escalations { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Port for initializer-maintainer dual agent pattern.
/// Implements the "Initializer-Maintainer Dual Agent" pattern.
/// </summary>
/// <remarks>
/// This port coordinates two specialized agents: an initializer for
/// complex setup tasks and a maintainer for ongoing operations,
/// with automatic handoff and escalation capabilities.
/// </remarks>
public interface IDualAgentPort
{
    /// <summary>
    /// Executes a task using the dual agent pattern.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<DualAgentResult> ExecuteAsync(
        DualAgentRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs only the initialization phase.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Handoff state for maintainer.</returns>
    Task<DualAgentHandoffContext> InitializeAsync(
        string task,
        DualAgentConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs only the maintainer phase.
    /// </summary>
    /// <param name="state">Handoff state from initializer.</param>
    /// <param name="task">Ongoing task.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<DualAgentResult> MaintainAsync(
        DualAgentHandoffContext state,
        string task,
        DualAgentConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Escalates from maintainer to initializer.
    /// </summary>
    /// <param name="sessionId">Session identifier.</param>
    /// <param name="reason">Reason for escalation.</param>
    /// <param name="context">Current context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Initializer response.</returns>
    Task<string> EscalateToInitializerAsync(
        string sessionId,
        string reason,
        string context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates handoff state.
    /// </summary>
    /// <param name="state">The handoff state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether state is valid and any issues.</returns>
    Task<(bool IsValid, IReadOnlyList<string> Issues)> ValidateHandoffAsync(
        DualAgentHandoffContext state,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets configuration.
    /// </summary>
    /// <param name="configuration">Configuration to set (null = get default).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<DualAgentConfiguration> ConfigureAsync(
        DualAgentConfiguration? configuration = null,
        CancellationToken cancellationToken = default);
}
