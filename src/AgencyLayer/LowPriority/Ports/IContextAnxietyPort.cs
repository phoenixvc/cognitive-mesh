namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Treats symptom not cause; see antipattern analysis
// Reconsideration: Not recommended; address root cause instead
// Related Antipattern: Context Window Anxiety Management (Medium Risk)
// ============================================================================

/// <summary>
/// Context anxiety metrics.
/// </summary>
public class ContextAnxietyMetrics
{
    /// <summary>Current token usage.</summary>
    public int CurrentTokens { get; init; }

    /// <summary>Maximum tokens.</summary>
    public int MaxTokens { get; init; }

    /// <summary>Usage percentage.</summary>
    public double UsagePercent { get; init; }

    /// <summary>Anxiety level.</summary>
    public string? AnxietyLevel { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for context window anxiety management.
/// Implements the "Context Window Anxiety Management" pattern.
///
/// This is a low-priority pattern because it treats symptoms rather
/// than root causes. Better to use context compaction patterns.
/// </summary>
public interface IContextAnxietyPort
{
    /// <summary>Gets anxiety metrics.</summary>
    Task<ContextAnxietyMetrics> GetMetricsAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Suggests anxiety reduction.</summary>
    Task<IReadOnlyList<string>> SuggestReductionAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>Applies anxiety management.</summary>
    Task ApplyManagementAsync(string sessionId, CancellationToken cancellationToken = default);
}
