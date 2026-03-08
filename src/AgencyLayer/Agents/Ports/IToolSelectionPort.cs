namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Selection strategy for tools.
/// </summary>
public enum ToolSelectionStrategy
{
    /// <summary>Select by semantic similarity.</summary>
    Semantic,
    /// <summary>Select by keyword matching.</summary>
    Keyword,
    /// <summary>Select using LLM reasoning.</summary>
    LLMReasoning,
    /// <summary>Hybrid approach.</summary>
    Hybrid
}

/// <summary>
/// A tool available for selection.
/// </summary>
public class SelectableTool
{
    /// <summary>Tool identifier.</summary>
    public required string ToolId { get; init; }

    /// <summary>Tool name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Input schema.</summary>
    public string? InputSchema { get; init; }

    /// <summary>Example usages.</summary>
    public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();

    /// <summary>Categories/tags.</summary>
    public IReadOnlyList<string> Categories { get; init; } = Array.Empty<string>();

    /// <summary>Embedding for semantic matching.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Usage count (for popularity weighting).</summary>
    public int UsageCount { get; init; }

    /// <summary>Success rate (0.0 - 1.0).</summary>
    public double SuccessRate { get; init; }

    /// <summary>Average execution time.</summary>
    public TimeSpan AverageExecutionTime { get; init; }

    /// <summary>Whether tool is enabled.</summary>
    public bool IsEnabled { get; init; } = true;
}

/// <summary>
/// Result of tool selection.
/// </summary>
public class ToolSelectionResult
{
    /// <summary>Selected tool.</summary>
    public SelectableTool? SelectedTool { get; init; }

    /// <summary>Confidence in selection (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Reasoning for selection.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Alternative tools considered.</summary>
    public IReadOnlyList<(SelectableTool Tool, double Score)> Alternatives { get; init; }
        = Array.Empty<(SelectableTool, double)>();

    /// <summary>Suggested parameters.</summary>
    public Dictionary<string, string> SuggestedParameters { get; init; } = new();

    /// <summary>Whether a tool was selected.</summary>
    public bool IsSelected => SelectedTool != null;
}

/// <summary>
/// Configuration for tool selection.
/// </summary>
public class ToolSelectionConfiguration
{
    /// <summary>Selection strategy.</summary>
    public ToolSelectionStrategy Strategy { get; init; } = ToolSelectionStrategy.Hybrid;

    /// <summary>Minimum confidence for selection.</summary>
    public double MinConfidence { get; init; } = 0.6;

    /// <summary>Maximum alternatives to return.</summary>
    public int MaxAlternatives { get; init; } = 3;

    /// <summary>Whether to consider tool performance.</summary>
    public bool ConsiderPerformance { get; init; } = true;

    /// <summary>Whether to suggest parameters.</summary>
    public bool SuggestParameters { get; init; } = true;

    /// <summary>Tool categories to include.</summary>
    public IReadOnlyList<string>? IncludeCategories { get; init; }

    /// <summary>Tool categories to exclude.</summary>
    public IReadOnlyList<string>? ExcludeCategories { get; init; }
}

/// <summary>
/// Port for intelligent tool selection.
/// Implements the "Tool Selection" / "Agentic Tool Selection" pattern.
/// </summary>
public interface IToolSelectionPort
{
    /// <summary>
    /// Selects the best tool for a task.
    /// </summary>
    Task<ToolSelectionResult> SelectToolAsync(
        string task,
        ToolSelectionConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects multiple tools for a complex task.
    /// </summary>
    Task<IReadOnlyList<ToolSelectionResult>> SelectToolsAsync(
        string task,
        int maxTools = 3,
        ToolSelectionConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a tool for selection.
    /// </summary>
    Task RegisterToolAsync(
        SelectableTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a tool's metadata.
    /// </summary>
    Task UpdateToolAsync(
        SelectableTool tool,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a tool.
    /// </summary>
    Task UnregisterToolAsync(
        string toolId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available tools.
    /// </summary>
    Task<IReadOnlyList<SelectableTool>> GetToolsAsync(
        IEnumerable<string>? categories = null,
        bool enabledOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records tool usage for learning.
    /// </summary>
    Task RecordUsageAsync(
        string toolId,
        bool success,
        TimeSpan executionTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates tool embeddings.
    /// </summary>
    Task UpdateEmbeddingsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tool selection statistics.
    /// </summary>
    Task<ToolSelectionStatistics> GetStatisticsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Tool selection statistics.
/// </summary>
public class ToolSelectionStatistics
{
    /// <summary>Total selections.</summary>
    public int TotalSelections { get; init; }
    /// <summary>Successful selections.</summary>
    public int SuccessfulSelections { get; init; }
    /// <summary>Average confidence.</summary>
    public double AverageConfidence { get; init; }
    /// <summary>Tool usage counts.</summary>
    public Dictionary<string, int> ToolUsageCounts { get; init; } = new();
    /// <summary>Tool success rates.</summary>
    public Dictionary<string, double> ToolSuccessRates { get; init; } = new();
}
