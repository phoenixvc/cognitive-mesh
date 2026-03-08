namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Types of cacheable actions.
/// </summary>
public enum CacheableActionType
{
    /// <summary>LLM completion request.</summary>
    LLMCompletion,
    /// <summary>Tool invocation.</summary>
    ToolInvocation,
    /// <summary>Agent decision.</summary>
    AgentDecision,
    /// <summary>Workflow step.</summary>
    WorkflowStep,
    /// <summary>Search or retrieval operation.</summary>
    SearchOperation,
    /// <summary>External API call.</summary>
    ExternalApiCall
}

/// <summary>
/// Represents a cached action entry.
/// </summary>
public class CachedAction
{
    /// <summary>Unique identifier for this cache entry.</summary>
    public required string CacheKey { get; init; }

    /// <summary>Type of action cached.</summary>
    public required CacheableActionType ActionType { get; init; }

    /// <summary>Hash of the input for cache lookup.</summary>
    public required string InputHash { get; init; }

    /// <summary>The cached output (JSON serialized).</summary>
    public required string OutputPayload { get; init; }

    /// <summary>Type of the output for deserialization.</summary>
    public required string OutputType { get; init; }

    /// <summary>When the action was originally executed.</summary>
    public DateTimeOffset ExecutedAt { get; init; }

    /// <summary>When the cache entry was created.</summary>
    public DateTimeOffset CachedAt { get; init; }

    /// <summary>When the cache entry expires.</summary>
    public DateTimeOffset? ExpiresAt { get; init; }

    /// <summary>Number of times this entry has been replayed.</summary>
    public int ReplayCount { get; init; }

    /// <summary>Last time this entry was replayed.</summary>
    public DateTimeOffset? LastReplayedAt { get; init; }

    /// <summary>Original execution duration in milliseconds.</summary>
    public double ExecutionDurationMs { get; init; }

    /// <summary>Agent that executed the action.</summary>
    public string? AgentId { get; init; }

    /// <summary>Session context at execution time.</summary>
    public string? SessionId { get; init; }

    /// <summary>Tags for categorization.</summary>
    public Dictionary<string, string> Tags { get; init; } = new();

    /// <summary>Whether the action was successful.</summary>
    public bool WasSuccessful { get; init; }
}

/// <summary>
/// Request to cache an action.
/// </summary>
public class CacheActionRequest
{
    /// <summary>Type of action to cache.</summary>
    public required CacheableActionType ActionType { get; init; }

    /// <summary>The input to the action (for hashing).</summary>
    public required string InputPayload { get; init; }

    /// <summary>The output of the action.</summary>
    public required string OutputPayload { get; init; }

    /// <summary>Type of the output.</summary>
    public required string OutputType { get; init; }

    /// <summary>Execution duration in milliseconds.</summary>
    public double ExecutionDurationMs { get; init; }

    /// <summary>Whether the action was successful.</summary>
    public bool WasSuccessful { get; init; } = true;

    /// <summary>Time-to-live for the cache entry.</summary>
    public TimeSpan? TimeToLive { get; init; }

    /// <summary>Agent that executed the action.</summary>
    public string? AgentId { get; init; }

    /// <summary>Session context.</summary>
    public string? SessionId { get; init; }

    /// <summary>Tags for categorization.</summary>
    public Dictionary<string, string> Tags { get; init; } = new();
}

/// <summary>
/// Result of a cache lookup.
/// </summary>
public class CacheLookupResult
{
    /// <summary>Whether a cache hit was found.</summary>
    public required bool IsHit { get; init; }

    /// <summary>The cached action if found.</summary>
    public CachedAction? CachedAction { get; init; }

    /// <summary>Cache key used for lookup.</summary>
    public required string CacheKey { get; init; }

    /// <summary>Time saved by cache hit (original execution duration).</summary>
    public double? TimeSavedMs { get; init; }
}

/// <summary>
/// Replay session for debugging or testing.
/// </summary>
public class ReplaySession
{
    /// <summary>Unique identifier for the replay session.</summary>
    public string SessionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>When the replay session started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Cache entries to replay in order.</summary>
    public IReadOnlyList<string> CacheKeys { get; init; } = Array.Empty<string>();

    /// <summary>Current position in the replay.</summary>
    public int CurrentPosition { get; init; }

    /// <summary>Whether the replay is complete.</summary>
    public bool IsComplete { get; init; }
}

/// <summary>
/// Port for action caching and replay.
/// Implements the "Action Caching and Replay Pattern".
/// </summary>
/// <remarks>
/// This port provides deterministic caching and replay of agent actions
/// for debugging, testing, and performance optimization. Cached actions
/// can be replayed to reproduce exact behavior without re-execution.
/// </remarks>
public interface IActionCachingPort
{
    /// <summary>
    /// Looks up a cached action by input.
    /// </summary>
    /// <param name="actionType">The type of action.</param>
    /// <param name="inputPayload">The input to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cache lookup result.</returns>
    Task<CacheLookupResult> LookupAsync(
        CacheableActionType actionType,
        string inputPayload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches an action result.
    /// </summary>
    /// <param name="request">The cache request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached action entry.</returns>
    Task<CachedAction> CacheAsync(
        CacheActionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cached action by cache key.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached action if found.</returns>
    Task<CachedAction?> GetAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a cached action.
    /// </summary>
    /// <param name="cacheKey">The cache key to invalidate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateAsync(
        string cacheKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cached actions matching criteria.
    /// </summary>
    /// <param name="actionType">Action type to invalidate (null = all).</param>
    /// <param name="agentId">Agent ID to invalidate (null = all).</param>
    /// <param name="olderThan">Invalidate entries older than this (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of entries invalidated.</returns>
    Task<int> InvalidateBulkAsync(
        CacheableActionType? actionType = null,
        string? agentId = null,
        DateTimeOffset? olderThan = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a replay session from cached actions.
    /// </summary>
    /// <param name="sessionId">The original session ID to replay.</param>
    /// <param name="startFrom">Optional cache key to start replay from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The replay session.</returns>
    Task<ReplaySession> CreateReplaySessionAsync(
        string sessionId,
        string? startFrom = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next action in a replay session.
    /// </summary>
    /// <param name="replaySessionId">The replay session ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next cached action, or null if complete.</returns>
    Task<CachedAction?> GetNextReplayActionAsync(
        string replaySessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cache statistics.
    /// </summary>
    /// <param name="since">Start time for statistics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cache statistics.</returns>
    Task<CacheStatistics> GetStatisticsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists cached actions matching criteria.
    /// </summary>
    /// <param name="actionType">Filter by action type (null = all).</param>
    /// <param name="agentId">Filter by agent (null = all).</param>
    /// <param name="limit">Maximum entries to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of cached actions.</returns>
    Task<IReadOnlyList<CachedAction>> ListAsync(
        CacheableActionType? actionType = null,
        string? agentId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics.
/// </summary>
public class CacheStatistics
{
    public int TotalEntries { get; init; }
    public int Hits { get; init; }
    public int Misses { get; init; }
    public double HitRate { get; init; }
    public double TotalTimeSavedMs { get; init; }
    public long TotalSizeBytes { get; init; }
    public Dictionary<CacheableActionType, int> EntriesByType { get; init; } = new();
}
