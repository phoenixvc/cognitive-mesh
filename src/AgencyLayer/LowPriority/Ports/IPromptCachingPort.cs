namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Model-provider optimization with low control
// Reconsideration: If prompt caching becomes critical for cost
// ============================================================================

/// <summary>
/// Prompt cache entry.
/// </summary>
public class PromptCacheEntry
{
    /// <summary>Cache key.</summary>
    public required string CacheKey { get; init; }

    /// <summary>Prompt prefix.</summary>
    public required string Prefix { get; init; }

    /// <summary>Token count.</summary>
    public int TokenCount { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Expires at.</summary>
    public DateTimeOffset ExpiresAt { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for prompt caching via exact prefix preservation.
/// Implements the "Prompt Caching via Exact Prefix Preservation" pattern.
///
/// This is a low-priority pattern because it's a model-provider
/// optimization with low control from the application side.
/// </summary>
public interface IPromptCachingPort
{
    /// <summary>Caches prompt prefix.</summary>
    Task<string> CachePrefixAsync(string prefix, CancellationToken cancellationToken = default);

    /// <summary>Gets cached prefix.</summary>
    Task<PromptCacheEntry?> GetCacheAsync(string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>Validates cache hit.</summary>
    Task<bool> ValidateCacheAsync(string prompt, string cacheKey, CancellationToken cancellationToken = default);

    /// <summary>Invalidates cache.</summary>
    Task InvalidateAsync(string cacheKey, CancellationToken cancellationToken = default);
}
