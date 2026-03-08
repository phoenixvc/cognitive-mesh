namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Agent tool selection via IToolSelectionPort is adequate
// Reconsideration: If tool selection becomes problematic
// ============================================================================

/// <summary>
/// Tool selection guide.
/// </summary>
public class ToolGuide
{
    /// <summary>Guide identifier.</summary>
    public required string GuideId { get; init; }

    /// <summary>Task pattern.</summary>
    public required string TaskPattern { get; init; }

    /// <summary>Recommended tools.</summary>
    public IReadOnlyList<string> RecommendedTools { get; init; } = Array.Empty<string>();

    /// <summary>Rationale.</summary>
    public string? Rationale { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for tool selection guide.
/// Implements the "Tool Selection Guide" pattern.
///
/// This is a low-priority pattern because IToolSelectionPort
/// provides adequate tool selection capabilities.
/// </summary>
public interface IToolSelectionGuidePort
{
    /// <summary>Gets guide for task.</summary>
    Task<ToolGuide?> GetGuideAsync(string task, CancellationToken cancellationToken = default);

    /// <summary>Registers a guide.</summary>
    Task RegisterGuideAsync(ToolGuide guide, CancellationToken cancellationToken = default);

    /// <summary>Lists guides.</summary>
    Task<IReadOnlyList<ToolGuide>> ListGuidesAsync(CancellationToken cancellationToken = default);
}
