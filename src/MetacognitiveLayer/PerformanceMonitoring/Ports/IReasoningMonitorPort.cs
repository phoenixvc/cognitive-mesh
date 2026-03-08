namespace MetacognitiveLayer.PerformanceMonitoring.Ports;

/// <summary>
/// A reasoning step being monitored.
/// </summary>
public class MonitoredReasoningStep
{
    /// <summary>Step identifier.</summary>
    public required string StepId { get; init; }

    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Step type.</summary>
    public required string StepType { get; init; }

    /// <summary>Step content.</summary>
    public required string Content { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Duration so far.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Tokens used.</summary>
    public int TokensUsed { get; init; }

    /// <summary>Status.</summary>
    public ReasoningStepStatus Status { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// Reasoning step status.
/// </summary>
public enum ReasoningStepStatus
{
    /// <summary>Step is pending.</summary>
    Pending,
    /// <summary>Step is in progress.</summary>
    InProgress,
    /// <summary>Step completed.</summary>
    Completed,
    /// <summary>Step was interrupted.</summary>
    Interrupted,
    /// <summary>Step failed.</summary>
    Failed
}

/// <summary>
/// A reasoning session summary.
/// </summary>
public class ReasoningSessionSummary
{
    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Total steps.</summary>
    public int TotalSteps { get; init; }

    /// <summary>Completed steps.</summary>
    public int CompletedSteps { get; init; }

    /// <summary>Current step.</summary>
    public MonitoredReasoningStep? CurrentStep { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>Total tokens.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Average confidence.</summary>
    public double AverageConfidence { get; init; }

    /// <summary>Status.</summary>
    public ReasoningSessionStatus Status { get; init; }

    /// <summary>Can be interrupted.</summary>
    public bool CanInterrupt { get; init; }
}

/// <summary>
/// Reasoning session status.
/// </summary>
public enum ReasoningSessionStatus
{
    Active,
    Paused,
    Completed,
    Interrupted,
    Failed
}

/// <summary>
/// Interruption request.
/// </summary>
public class InterruptionRequest
{
    /// <summary>Session to interrupt.</summary>
    public required string SessionId { get; init; }

    /// <summary>Interruption type.</summary>
    public InterruptionType Type { get; init; }

    /// <summary>Reason for interruption.</summary>
    public string? Reason { get; init; }

    /// <summary>Whether to save state.</summary>
    public bool SaveState { get; init; } = true;
}

/// <summary>
/// Interruption type.
/// </summary>
public enum InterruptionType
{
    /// <summary>Pause execution.</summary>
    Pause,
    /// <summary>Stop and discard.</summary>
    Stop,
    /// <summary>Stop and save.</summary>
    StopAndSave,
    /// <summary>Redirect to different path.</summary>
    Redirect
}

/// <summary>
/// Port for chain-of-thought monitoring and interruption.
/// Implements the "Chain-of-Thought Monitoring and Interruption" pattern.
/// </summary>
public interface IReasoningMonitorPort
{
    /// <summary>
    /// Starts monitoring a reasoning session.
    /// </summary>
    Task<string> StartMonitoringAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a reasoning step.
    /// </summary>
    Task RecordStepAsync(
        MonitoredReasoningStep step,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current session summary.
    /// </summary>
    Task<ReasoningSessionSummary?> GetSessionSummaryAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all steps for a session.
    /// </summary>
    Task<IReadOnlyList<MonitoredReasoningStep>> GetStepsAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Streams reasoning steps as they occur.
    /// </summary>
    IAsyncEnumerable<MonitoredReasoningStep> StreamStepsAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests interruption.
    /// </summary>
    Task<bool> RequestInterruptionAsync(
        InterruptionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused session.
    /// </summary>
    Task<bool> ResumeSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if interruption was requested.
    /// </summary>
    Task<bool> IsInterruptionRequestedAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes monitoring.
    /// </summary>
    Task CompleteMonitoringAsync(
        string sessionId,
        CancellationToken cancellationToken = default);
}
