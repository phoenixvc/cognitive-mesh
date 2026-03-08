namespace AgencyLayer.Agents.Ports;

/// <summary>
/// An optimization recommendation.
/// </summary>
public class OptimizationRecommendation
{
    /// <summary>Recommendation identifier.</summary>
    public string RecommendationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Category.</summary>
    public OptimizationCategory Category { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Affected files/areas.</summary>
    public IReadOnlyList<string> AffectedAreas { get; init; } = Array.Empty<string>();

    /// <summary>Expected impact on agent performance.</summary>
    public ImpactLevel Impact { get; init; }

    /// <summary>Effort required.</summary>
    public EffortLevel Effort { get; init; }

    /// <summary>Priority score.</summary>
    public double Priority { get; init; }

    /// <summary>Steps to implement.</summary>
    public IReadOnlyList<string> Steps { get; init; } = Array.Empty<string>();

    /// <summary>Before example.</summary>
    public string? BeforeExample { get; init; }

    /// <summary>After example.</summary>
    public string? AfterExample { get; init; }
}

/// <summary>
/// Optimization category.
/// </summary>
public enum OptimizationCategory
{
    /// <summary>Code structure and organization.</summary>
    Structure,
    /// <summary>Naming and conventions.</summary>
    Naming,
    /// <summary>Documentation and comments.</summary>
    Documentation,
    /// <summary>Dependencies and modularity.</summary>
    Dependencies,
    /// <summary>Type safety and contracts.</summary>
    TypeSafety,
    /// <summary>Error handling patterns.</summary>
    ErrorHandling,
    /// <summary>Testing patterns.</summary>
    Testing,
    /// <summary>Configuration management.</summary>
    Configuration
}

/// <summary>
/// Impact level.
/// </summary>
public enum ImpactLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Effort level.
/// </summary>
public enum EffortLevel
{
    Trivial,
    Small,
    Medium,
    Large,
    Extensive
}

/// <summary>
/// Codebase health score.
/// </summary>
public class CodebaseHealthScore
{
    /// <summary>Overall score (0-100).</summary>
    public int OverallScore { get; init; }

    /// <summary>Scores by category.</summary>
    public Dictionary<OptimizationCategory, int> CategoryScores { get; init; } = new();

    /// <summary>Top issues.</summary>
    public IReadOnlyList<string> TopIssues { get; init; } = Array.Empty<string>();

    /// <summary>Strengths.</summary>
    public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();

    /// <summary>Trend compared to last assessment.</summary>
    public TrendDirection Trend { get; init; }

    /// <summary>Assessment date.</summary>
    public DateTimeOffset AssessedAt { get; init; }
}

/// <summary>
/// Trend direction.
/// </summary>
public enum TrendDirection
{
    Improving,
    Stable,
    Declining,
    Unknown
}

/// <summary>
/// Port for codebase optimization for agents.
/// Implements the "Codebase Optimization for Agents" pattern.
/// </summary>
public interface ICodebaseOptimizationPort
{
    /// <summary>
    /// Analyzes codebase for agent-friendliness.
    /// </summary>
    Task<CodebaseHealthScore> AnalyzeCodebaseAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets optimization recommendations.
    /// </summary>
    Task<IReadOnlyList<OptimizationRecommendation>> GetRecommendationsAsync(
        string repositoryPath,
        OptimizationCategory? category = null,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes a specific file.
    /// </summary>
    Task<IReadOnlyList<OptimizationRecommendation>> AnalyzeFileAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies an optimization.
    /// </summary>
    Task<bool> ApplyOptimizationAsync(
        string recommendationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates codebase against best practices.
    /// </summary>
    Task<IReadOnlyList<(string Rule, bool Passed, string? Violation)>> ValidateBestPracticesAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates agent-friendly documentation.
    /// </summary>
    Task<string> GenerateAgentDocsAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets health score history.
    /// </summary>
    Task<IReadOnlyList<CodebaseHealthScore>> GetHealthHistoryAsync(
        string repositoryPath,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests file structure improvements.
    /// </summary>
    Task<IReadOnlyList<string>> SuggestStructureImprovementsAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);
}
