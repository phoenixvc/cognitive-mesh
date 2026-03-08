namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Type of tool error.
/// </summary>
public enum ToolErrorType
{
    /// <summary>Transient error (retry may succeed).</summary>
    Transient,
    /// <summary>Invalid input.</summary>
    InvalidInput,
    /// <summary>Authentication error.</summary>
    Authentication,
    /// <summary>Authorization error.</summary>
    Authorization,
    /// <summary>Rate limiting.</summary>
    RateLimited,
    /// <summary>Resource not found.</summary>
    NotFound,
    /// <summary>Timeout.</summary>
    Timeout,
    /// <summary>Server error.</summary>
    ServerError,
    /// <summary>Unknown error.</summary>
    Unknown
}

/// <summary>
/// Retry strategy.
/// </summary>
public enum RetryStrategy
{
    /// <summary>Exponential backoff.</summary>
    ExponentialBackoff,
    /// <summary>Linear backoff.</summary>
    LinearBackoff,
    /// <summary>Fixed delay.</summary>
    FixedDelay,
    /// <summary>Immediate retry.</summary>
    Immediate,
    /// <summary>No retry.</summary>
    NoRetry
}

/// <summary>
/// A tool error with classification.
/// </summary>
public class ToolError
{
    /// <summary>Error type.</summary>
    public required ToolErrorType Type { get; init; }

    /// <summary>Error message.</summary>
    public required string Message { get; init; }

    /// <summary>Error code.</summary>
    public string? Code { get; init; }

    /// <summary>Whether this error is retryable.</summary>
    public bool IsRetryable { get; init; }

    /// <summary>Suggested wait time before retry.</summary>
    public TimeSpan? SuggestedRetryAfter { get; init; }

    /// <summary>Stack trace if available.</summary>
    public string? StackTrace { get; init; }

    /// <summary>Inner error details.</summary>
    public string? InnerDetails { get; init; }
}

/// <summary>
/// Configuration for tool retry.
/// </summary>
public class ToolRetryConfiguration
{
    /// <summary>Maximum retry attempts.</summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>Retry strategy.</summary>
    public RetryStrategy Strategy { get; init; } = RetryStrategy.ExponentialBackoff;

    /// <summary>Initial delay in milliseconds.</summary>
    public int InitialDelayMs { get; init; } = 100;

    /// <summary>Maximum delay in milliseconds.</summary>
    public int MaxDelayMs { get; init; } = 30000;

    /// <summary>Backoff multiplier (for exponential).</summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>Whether to add jitter to delays.</summary>
    public bool AddJitter { get; init; } = true;

    /// <summary>Jitter percentage (0.0 - 1.0).</summary>
    public double JitterPercent { get; init; } = 0.1;

    /// <summary>Error types to retry.</summary>
    public IReadOnlyList<ToolErrorType> RetryableErrors { get; init; } = new[]
    {
        ToolErrorType.Transient,
        ToolErrorType.RateLimited,
        ToolErrorType.Timeout,
        ToolErrorType.ServerError
    };

    /// <summary>Whether to use adaptive retry based on error patterns.</summary>
    public bool EnableAdaptiveRetry { get; init; } = true;
}

/// <summary>
/// Record of a retry attempt.
/// </summary>
public class RetryAttempt
{
    /// <summary>Attempt number.</summary>
    public int AttemptNumber { get; init; }

    /// <summary>Error encountered.</summary>
    public required ToolError Error { get; init; }

    /// <summary>Delay before this attempt.</summary>
    public TimeSpan DelayBefore { get; init; }

    /// <summary>When the attempt was made.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Whether another retry will be attempted.</summary>
    public bool WillRetry { get; init; }
}

/// <summary>
/// Result of a tool invocation with retry.
/// </summary>
public class RetryResult
{
    /// <summary>Whether the operation succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Output if successful.</summary>
    public string? Output { get; init; }

    /// <summary>Final error if failed.</summary>
    public ToolError? FinalError { get; init; }

    /// <summary>Total attempts made.</summary>
    public int TotalAttempts { get; init; }

    /// <summary>Retry attempts.</summary>
    public IReadOnlyList<RetryAttempt> Attempts { get; init; } = Array.Empty<RetryAttempt>();

    /// <summary>Total duration including retries.</summary>
    public TimeSpan TotalDuration { get; init; }
}

/// <summary>
/// Port for tool retry with backoff.
/// Implements the "Retry with Exponential Backoff" pattern.
/// </summary>
/// <remarks>
/// This port provides sophisticated retry logic for tool invocations
/// with configurable backoff strategies, error classification,
/// and adaptive retry based on error patterns.
/// </remarks>
public interface IToolRetryPort
{
    /// <summary>
    /// Invokes a tool with retry.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="input">Tool input.</param>
    /// <param name="configuration">Retry configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The retry result.</returns>
    Task<RetryResult> InvokeWithRetryAsync(
        string toolId,
        string input,
        ToolRetryConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Classifies an error.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Classified error.</returns>
    Task<ToolError> ClassifyErrorAsync(
        Exception exception,
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates delay before next retry.
    /// </summary>
    /// <param name="attemptNumber">Current attempt number.</param>
    /// <param name="error">The error encountered.</param>
    /// <param name="configuration">Retry configuration.</param>
    /// <returns>Delay before next retry.</returns>
    TimeSpan CalculateDelay(
        int attemptNumber,
        ToolError error,
        ToolRetryConfiguration configuration);

    /// <summary>
    /// Determines if an error should be retried.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <param name="attemptNumber">Current attempt number.</param>
    /// <param name="configuration">Retry configuration.</param>
    /// <returns>Whether to retry.</returns>
    bool ShouldRetry(
        ToolError error,
        int attemptNumber,
        ToolRetryConfiguration configuration);

    /// <summary>
    /// Gets retry statistics for a tool.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="since">Start time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Retry statistics.</returns>
    Task<ToolRetryStatistics> GetStatisticsAsync(
        string toolId,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets adaptive retry configuration based on tool history.
    /// </summary>
    /// <param name="toolId">The tool ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Adaptive configuration.</returns>
    Task<ToolRetryConfiguration> GetAdaptiveConfigurationAsync(
        string toolId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Retry statistics for a tool.
/// </summary>
public class ToolRetryStatistics
{
    /// <summary>Total invocations.</summary>
    public int TotalInvocations { get; init; }
    /// <summary>Successful first attempt.</summary>
    public int SuccessfulFirstAttempt { get; init; }
    /// <summary>Successful after retry.</summary>
    public int SuccessfulAfterRetry { get; init; }
    /// <summary>Failed after retries.</summary>
    public int FailedAfterRetries { get; init; }
    /// <summary>First attempt success rate.</summary>
    public double FirstAttemptSuccessRate { get; init; }
    /// <summary>Overall success rate.</summary>
    public double OverallSuccessRate { get; init; }
    /// <summary>Average retries per success.</summary>
    public double AverageRetriesPerSuccess { get; init; }
    /// <summary>Errors by type.</summary>
    public Dictionary<ToolErrorType, int> ErrorsByType { get; init; } = new();
}
