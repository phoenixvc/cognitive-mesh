namespace ReasoningLayer.StructuredReasoning.Ports;

/// <summary>
/// Type of ReAct step.
/// </summary>
public enum ReActStepType
{
    /// <summary>Thought/reasoning step.</summary>
    Thought,
    /// <summary>Action step (tool use).</summary>
    Action,
    /// <summary>Observation step (action result).</summary>
    Observation,
    /// <summary>Final answer.</summary>
    Answer
}

/// <summary>
/// A step in the ReAct loop.
/// </summary>
public class ReActStep
{
    /// <summary>Step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Step type.</summary>
    public required ReActStepType Type { get; init; }

    /// <summary>Content of the step.</summary>
    public required string Content { get; init; }

    /// <summary>Tool invoked (for Action steps).</summary>
    public string? ToolId { get; init; }

    /// <summary>Tool input (for Action steps).</summary>
    public string? ToolInput { get; init; }

    /// <summary>When the step occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// Configuration for ReAct reasoning.
/// </summary>
public class ReActConfiguration
{
    /// <summary>Maximum iterations.</summary>
    public int MaxIterations { get; init; } = 10;

    /// <summary>Available tools.</summary>
    public IReadOnlyList<string> AvailableTools { get; init; } = Array.Empty<string>();

    /// <summary>Whether to require thought before action.</summary>
    public bool RequireThoughtBeforeAction { get; init; } = true;

    /// <summary>Whether to allow multiple actions per iteration.</summary>
    public bool AllowMultipleActions { get; init; } = false;

    /// <summary>Timeout for the entire reasoning chain.</summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }
}

/// <summary>
/// Result of ReAct reasoning.
/// </summary>
public class ReActResult
{
    /// <summary>Whether reasoning was successful.</summary>
    public required bool Success { get; init; }

    /// <summary>Final answer.</summary>
    public string? Answer { get; init; }

    /// <summary>All steps in the reasoning chain.</summary>
    public IReadOnlyList<ReActStep> Steps { get; init; } = Array.Empty<ReActStep>();

    /// <summary>Total iterations used.</summary>
    public int IterationsUsed { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Confidence in the answer (0.0 - 1.0).</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Port for ReAct (Reasoning + Acting) pattern.
/// Implements the "ReAct (Reasoning + Acting)" pattern.
/// </summary>
/// <remarks>
/// This port implements the ReAct paradigm where the agent alternates
/// between thinking (reasoning) and acting (tool use), with observations
/// feeding back into the reasoning process.
/// </remarks>
public interface IReActPort
{
    /// <summary>
    /// Runs ReAct reasoning on a query.
    /// </summary>
    /// <param name="query">The query to answer.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reasoning result.</returns>
    Task<ReActResult> ReasonAsync(
        string query,
        ReActConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs one iteration of the ReAct loop.
    /// </summary>
    /// <param name="query">The original query.</param>
    /// <param name="previousSteps">Steps so far.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>New steps from this iteration.</returns>
    Task<IReadOnlyList<ReActStep>> RunIterationAsync(
        string query,
        IReadOnlyList<ReActStep> previousSteps,
        ReActConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a thought step.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="context">Current context (previous steps).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The thought.</returns>
    Task<string> GenerateThoughtAsync(
        string query,
        string context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an action from a thought.
    /// </summary>
    /// <param name="thought">The thought.</param>
    /// <param name="availableTools">Available tools.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tool ID and input.</returns>
    Task<(string ToolId, string Input)> GenerateActionAsync(
        string thought,
        IReadOnlyList<string> availableTools,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an action and returns observation.
    /// </summary>
    /// <param name="toolId">Tool to invoke.</param>
    /// <param name="input">Tool input.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Observation.</returns>
    Task<string> ExecuteActionAsync(
        string toolId,
        string input,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if reasoning should terminate.
    /// </summary>
    /// <param name="steps">Steps so far.</param>
    /// <param name="maxIterations">Maximum iterations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether to terminate and reason.</returns>
    Task<(bool ShouldTerminate, string? Reason)> ShouldTerminateAsync(
        IReadOnlyList<ReActStep> steps,
        int maxIterations,
        CancellationToken cancellationToken = default);
}
