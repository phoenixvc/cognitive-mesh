namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Step type in hybrid workflow.
/// </summary>
public enum HybridStepType
{
    /// <summary>LLM-based step.</summary>
    LLM,
    /// <summary>Code execution step.</summary>
    Code,
    /// <summary>Tool invocation step.</summary>
    Tool,
    /// <summary>Human review step.</summary>
    Human,
    /// <summary>Conditional branch.</summary>
    Conditional,
    /// <summary>Parallel execution.</summary>
    Parallel
}

/// <summary>
/// A step in a hybrid workflow.
/// </summary>
public class HybridWorkflowStep
{
    /// <summary>Step identifier.</summary>
    public required string StepId { get; init; }

    /// <summary>Step name.</summary>
    public required string Name { get; init; }

    /// <summary>Step type.</summary>
    public HybridStepType Type { get; init; }

    /// <summary>Configuration for the step.</summary>
    public required string Configuration { get; init; }

    /// <summary>Input mapping.</summary>
    public Dictionary<string, string> InputMapping { get; init; } = new();

    /// <summary>Output mapping.</summary>
    public Dictionary<string, string> OutputMapping { get; init; } = new();

    /// <summary>Next step(s).</summary>
    public IReadOnlyList<string> NextSteps { get; init; } = Array.Empty<string>();

    /// <summary>Condition for conditional steps.</summary>
    public string? Condition { get; init; }

    /// <summary>Timeout.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Retry configuration.</summary>
    public RetryConfiguration? Retry { get; init; }
}

/// <summary>
/// Retry configuration.
/// </summary>
public class RetryConfiguration
{
    /// <summary>Maximum retries.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Delay between retries.</summary>
    public TimeSpan Delay { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Whether to use exponential backoff.</summary>
    public bool ExponentialBackoff { get; init; } = true;
}

/// <summary>
/// A hybrid workflow definition.
/// </summary>
public class HybridWorkflowDefinition
{
    /// <summary>Workflow identifier.</summary>
    public string WorkflowId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Workflow name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Steps in the workflow.</summary>
    public IReadOnlyList<HybridWorkflowStep> Steps { get; init; } = Array.Empty<HybridWorkflowStep>();

    /// <summary>Entry step ID.</summary>
    public required string EntryStepId { get; init; }

    /// <summary>Input schema (JSON).</summary>
    public string? InputSchema { get; init; }

    /// <summary>Output schema (JSON).</summary>
    public string? OutputSchema { get; init; }

    /// <summary>Version.</summary>
    public string Version { get; init; } = "1.0.0";
}

/// <summary>
/// Result of hybrid workflow execution.
/// </summary>
public class HybridWorkflowResult
{
    /// <summary>Execution identifier.</summary>
    public required string ExecutionId { get; init; }

    /// <summary>Workflow identifier.</summary>
    public required string WorkflowId { get; init; }

    /// <summary>Final output.</summary>
    public Dictionary<string, object> Output { get; init; } = new();

    /// <summary>Step results.</summary>
    public IReadOnlyList<HybridStepResult> StepResults { get; init; } = Array.Empty<HybridStepResult>();

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// Result of a single step.
/// </summary>
public class HybridStepResult
{
    /// <summary>Step identifier.</summary>
    public required string StepId { get; init; }

    /// <summary>Step type.</summary>
    public HybridStepType Type { get; init; }

    /// <summary>Output.</summary>
    public object? Output { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Retry count.</summary>
    public int RetryCount { get; init; }
}

/// <summary>
/// Port for hybrid LLM/Code workflow coordination.
/// Implements the "Hybrid LLM/Code Workflow Coordinator" pattern.
/// </summary>
public interface IHybridWorkflowPort
{
    /// <summary>
    /// Registers a workflow definition.
    /// </summary>
    Task RegisterWorkflowAsync(
        HybridWorkflowDefinition workflow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a workflow definition.
    /// </summary>
    Task<HybridWorkflowDefinition?> GetWorkflowAsync(
        string workflowId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a workflow.
    /// </summary>
    Task<HybridWorkflowResult> ExecuteAsync(
        string workflowId,
        Dictionary<string, object> input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets execution status.
    /// </summary>
    Task<HybridWorkflowResult?> GetExecutionStatusAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an execution.
    /// </summary>
    Task CancelExecutionAsync(
        string executionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a workflow definition.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Issues)> ValidateWorkflowAsync(
        HybridWorkflowDefinition workflow,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists workflow definitions.
    /// </summary>
    Task<IReadOnlyList<HybridWorkflowDefinition>> ListWorkflowsAsync(
        CancellationToken cancellationToken = default);
}
