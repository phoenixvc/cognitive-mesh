namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Status of a model in the fallback chain.
/// </summary>
public enum ModelStatus
{
    /// <summary>Model is healthy and available.</summary>
    Healthy,
    /// <summary>Model is degraded but functional.</summary>
    Degraded,
    /// <summary>Model is unavailable.</summary>
    Unavailable,
    /// <summary>Model is in circuit breaker open state.</summary>
    CircuitOpen,
    /// <summary>Model is being rate limited.</summary>
    RateLimited,
    /// <summary>Model status is unknown.</summary>
    Unknown
}

/// <summary>
/// Configuration for a model in the fallback chain.
/// </summary>
public class FallbackModelConfiguration
{
    /// <summary>Unique identifier for this model configuration.</summary>
    public required string ModelId { get; init; }

    /// <summary>The model name/identifier for the LLM provider.</summary>
    public required string ModelName { get; init; }

    /// <summary>The LLM provider (OpenAI, Azure, Anthropic, etc.).</summary>
    public required string Provider { get; init; }

    /// <summary>Priority in the fallback chain (lower = higher priority).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Maximum tokens per minute for rate limiting.</summary>
    public int? MaxTokensPerMinute { get; init; }

    /// <summary>Maximum requests per minute for rate limiting.</summary>
    public int? MaxRequestsPerMinute { get; init; }

    /// <summary>Timeout for requests in milliseconds.</summary>
    public int TimeoutMs { get; init; } = 30000;

    /// <summary>Number of failures before circuit breaker opens.</summary>
    public int CircuitBreakerThreshold { get; init; } = 3;

    /// <summary>Duration to keep circuit breaker open.</summary>
    public TimeSpan CircuitBreakerDuration { get; init; } = TimeSpan.FromMinutes(1);

    /// <summary>Whether this model supports function/tool calling.</summary>
    public bool SupportsFunctionCalling { get; init; } = true;

    /// <summary>Whether this model supports vision/multimodal.</summary>
    public bool SupportsVision { get; init; } = false;

    /// <summary>Cost per 1K input tokens (for routing decisions).</summary>
    public decimal? CostPer1KInputTokens { get; init; }

    /// <summary>Cost per 1K output tokens.</summary>
    public decimal? CostPer1KOutputTokens { get; init; }

    /// <summary>Custom capabilities or limitations.</summary>
    public Dictionary<string, string> Capabilities { get; init; } = new();
}

/// <summary>
/// Health check result for a model.
/// </summary>
public class ModelHealthCheck
{
    /// <summary>The model ID.</summary>
    public required string ModelId { get; init; }

    /// <summary>Current status.</summary>
    public required ModelStatus Status { get; init; }

    /// <summary>Response latency in milliseconds.</summary>
    public double? LatencyMs { get; init; }

    /// <summary>Error rate over recent window (0.0 - 1.0).</summary>
    public double ErrorRate { get; init; }

    /// <summary>Remaining rate limit quota.</summary>
    public int? RemainingQuota { get; init; }

    /// <summary>Time until rate limit resets.</summary>
    public TimeSpan? QuotaResetsIn { get; init; }

    /// <summary>When the health check was performed.</summary>
    public DateTimeOffset CheckedAt { get; init; }

    /// <summary>Error message if unhealthy.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Result of a model selection decision.
/// </summary>
public class ModelSelectionResult
{
    /// <summary>The selected model.</summary>
    public required FallbackModelConfiguration SelectedModel { get; init; }

    /// <summary>Whether this is a fallback selection.</summary>
    public bool IsFallback { get; init; }

    /// <summary>Models that were skipped and why.</summary>
    public Dictionary<string, string> SkippedModels { get; init; } = new();

    /// <summary>Reason for the selection.</summary>
    public string? SelectionReason { get; init; }
}

/// <summary>
/// Request requirements for model selection.
/// </summary>
public class ModelRequirements
{
    /// <summary>Requires function/tool calling support.</summary>
    public bool RequiresFunctionCalling { get; init; }

    /// <summary>Requires vision/multimodal support.</summary>
    public bool RequiresVision { get; init; }

    /// <summary>Maximum acceptable latency in milliseconds.</summary>
    public int? MaxLatencyMs { get; init; }

    /// <summary>Maximum acceptable cost per request.</summary>
    public decimal? MaxCostPerRequest { get; init; }

    /// <summary>Estimated input tokens.</summary>
    public int? EstimatedInputTokens { get; init; }

    /// <summary>Estimated output tokens.</summary>
    public int? EstimatedOutputTokens { get; init; }

    /// <summary>Preferred providers (in order).</summary>
    public IReadOnlyList<string>? PreferredProviders { get; init; }

    /// <summary>Custom capability requirements.</summary>
    public Dictionary<string, string> RequiredCapabilities { get; init; } = new();
}

/// <summary>
/// Port for failover-aware model fallback.
/// Implements the "Failover-Aware Model Fallback" pattern.
/// </summary>
/// <remarks>
/// This port provides intelligent model selection with automatic fallback
/// when the primary model is unavailable or degraded. It maintains health
/// state for all models and routes requests to the best available option.
/// </remarks>
public interface IModelFallbackPort
{
    /// <summary>
    /// Selects the best available model based on requirements and health.
    /// </summary>
    /// <param name="requirements">The requirements for the request.</param>
    /// <param name="excludeModels">Models to exclude from consideration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The model selection result.</returns>
    Task<ModelSelectionResult> SelectModelAsync(
        ModelRequirements? requirements = null,
        IEnumerable<string>? excludeModels = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of all configured models.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Health status for each model.</returns>
    Task<IReadOnlyList<ModelHealthCheck>> GetAllModelHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the health status of a specific model.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The health check result.</returns>
    Task<ModelHealthCheck> GetModelHealthAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a successful request to a model.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="latencyMs">Request latency in milliseconds.</param>
    /// <param name="inputTokens">Input tokens used.</param>
    /// <param name="outputTokens">Output tokens generated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordSuccessAsync(
        string modelId,
        double latencyMs,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a failed request to a model.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="errorType">Type of error (Timeout, RateLimit, ServerError, etc.).</param>
    /// <param name="errorMessage">Error message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordFailureAsync(
        string modelId,
        string errorType,
        string errorMessage,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds or updates a model configuration.
    /// </summary>
    /// <param name="configuration">The model configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertModelConfigurationAsync(
        FallbackModelConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a model from the fallback chain.
    /// </summary>
    /// <param name="modelId">The model ID to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveModelAsync(
        string modelId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all configured models in priority order.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of model configurations.</returns>
    Task<IReadOnlyList<FallbackModelConfiguration>> ListModelsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually opens the circuit breaker for a model.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="duration">How long to keep it open.</param>
    /// <param name="reason">Reason for opening.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task OpenCircuitBreakerAsync(
        string modelId,
        TimeSpan duration,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually resets the circuit breaker for a model.
    /// </summary>
    /// <param name="modelId">The model ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResetCircuitBreakerAsync(
        string modelId,
        CancellationToken cancellationToken = default);
}
