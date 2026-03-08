namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Critic review severity.
/// </summary>
public enum CriticSeverity
{
    /// <summary>Informational suggestion.</summary>
    Info,
    /// <summary>Minor issue.</summary>
    Minor,
    /// <summary>Moderate issue.</summary>
    Moderate,
    /// <summary>Major issue.</summary>
    Major,
    /// <summary>Critical issue.</summary>
    Critical
}

/// <summary>
/// A critic finding.
/// </summary>
public class CriticFinding
{
    /// <summary>Finding identifier.</summary>
    public string FindingId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Severity.</summary>
    public CriticSeverity Severity { get; init; }

    /// <summary>Category.</summary>
    public required string Category { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>File path.</summary>
    public string? FilePath { get; init; }

    /// <summary>Line number.</summary>
    public int? LineNumber { get; init; }

    /// <summary>Code snippet.</summary>
    public string? CodeSnippet { get; init; }

    /// <summary>Suggestion.</summary>
    public string? Suggestion { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>References.</summary>
    public IReadOnlyList<string> References { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Critic review result.
/// </summary>
public class CriticReviewResult
{
    /// <summary>Review identifier.</summary>
    public required string ReviewId { get; init; }

    /// <summary>Overall assessment.</summary>
    public required string OverallAssessment { get; init; }

    /// <summary>Overall score (0-100).</summary>
    public int OverallScore { get; init; }

    /// <summary>Findings.</summary>
    public IReadOnlyList<CriticFinding> Findings { get; init; } = Array.Empty<CriticFinding>();

    /// <summary>Summary by category.</summary>
    public Dictionary<string, int> FindingsByCategory { get; init; } = new();

    /// <summary>Summary by severity.</summary>
    public Dictionary<CriticSeverity, int> FindingsBySeverity { get; init; } = new();

    /// <summary>Strengths identified.</summary>
    public IReadOnlyList<string> Strengths { get; init; } = Array.Empty<string>();

    /// <summary>Improvement areas.</summary>
    public IReadOnlyList<string> ImprovementAreas { get; init; } = Array.Empty<string>();

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Reviewed at.</summary>
    public DateTimeOffset ReviewedAt { get; init; }
}

/// <summary>
/// Critic review configuration.
/// </summary>
public class CriticReviewConfiguration
{
    /// <summary>Categories to focus on.</summary>
    public IReadOnlyList<string> FocusCategories { get; init; } = Array.Empty<string>();

    /// <summary>Minimum severity to report.</summary>
    public CriticSeverity MinSeverity { get; init; } = CriticSeverity.Info;

    /// <summary>Whether to include suggestions.</summary>
    public bool IncludeSuggestions { get; init; } = true;

    /// <summary>Maximum findings.</summary>
    public int MaxFindings { get; init; } = 50;

    /// <summary>Review depth.</summary>
    public ReviewDepth Depth { get; init; } = ReviewDepth.Standard;

    /// <summary>Custom review prompt.</summary>
    public string? CustomPrompt { get; init; }
}

/// <summary>
/// Review depth.
/// </summary>
public enum ReviewDepth
{
    /// <summary>Quick.</summary>
    Quick,
    /// <summary>Standard.</summary>
    Standard,
    /// <summary>Thorough.</summary>
    Thorough,
    /// <summary>Exhaustive.</summary>
    Exhaustive
}

/// <summary>
/// Port for CriticGPT-style code review.
/// Implements the "CriticGPT-Style Code Review" pattern.
/// </summary>
public interface ICriticReviewPort
{
    /// <summary>
    /// Reviews code critically.
    /// </summary>
    Task<CriticReviewResult> ReviewCodeAsync(
        string code,
        string? language = null,
        CriticReviewConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews a file.
    /// </summary>
    Task<CriticReviewResult> ReviewFileAsync(
        string filePath,
        CriticReviewConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews a diff/PR.
    /// </summary>
    Task<CriticReviewResult> ReviewDiffAsync(
        string diff,
        CriticReviewConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews multiple files.
    /// </summary>
    Task<IReadOnlyList<CriticReviewResult>> ReviewFilesAsync(
        IEnumerable<string> filePaths,
        CriticReviewConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets review history.
    /// </summary>
    Task<IReadOnlyList<CriticReviewResult>> GetReviewHistoryAsync(
        string? filePath = null,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates improvement suggestions.
    /// </summary>
    Task<IReadOnlyList<string>> GenerateImprovementsAsync(
        CriticReviewResult review,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records feedback on a finding.
    /// </summary>
    Task RecordFindingFeedbackAsync(
        string findingId,
        bool wasHelpful,
        string? comment = null,
        CancellationToken cancellationToken = default);
}
