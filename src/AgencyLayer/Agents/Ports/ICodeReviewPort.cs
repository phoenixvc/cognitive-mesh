namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Severity of a review finding.
/// </summary>
public enum ReviewSeverity
{
    /// <summary>Informational or style suggestion.</summary>
    Info,
    /// <summary>Minor issue or improvement suggestion.</summary>
    Minor,
    /// <summary>Significant issue that should be addressed.</summary>
    Major,
    /// <summary>Critical issue that must be fixed.</summary>
    Critical,
    /// <summary>Blocking issue that prevents merge.</summary>
    Blocker
}

/// <summary>
/// Category of review finding.
/// </summary>
public enum ReviewCategory
{
    /// <summary>Bug or logic error.</summary>
    Bug,
    /// <summary>Security vulnerability.</summary>
    Security,
    /// <summary>Performance issue.</summary>
    Performance,
    /// <summary>Code style or formatting.</summary>
    Style,
    /// <summary>Naming convention violation.</summary>
    Naming,
    /// <summary>Missing or incorrect documentation.</summary>
    Documentation,
    /// <summary>Test coverage issue.</summary>
    Testing,
    /// <summary>Architecture or design concern.</summary>
    Architecture,
    /// <summary>Maintainability concern.</summary>
    Maintainability,
    /// <summary>Accessibility issue.</summary>
    Accessibility
}

/// <summary>
/// Request to review code changes.
/// </summary>
public class CodeReviewRequest
{
    /// <summary>Unique identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The diff or patch to review.</summary>
    public required string Diff { get; init; }

    /// <summary>Files changed (for context).</summary>
    public IReadOnlyList<string> ChangedFiles { get; init; } = Array.Empty<string>();

    /// <summary>Pull request or change description.</summary>
    public string? Description { get; init; }

    /// <summary>Focus areas for review.</summary>
    public IReadOnlyList<ReviewCategory> FocusAreas { get; init; } = Array.Empty<ReviewCategory>();

    /// <summary>Minimum severity to report.</summary>
    public ReviewSeverity MinSeverity { get; init; } = ReviewSeverity.Minor;

    /// <summary>Whether to include suggestions.</summary>
    public bool IncludeSuggestions { get; init; } = true;

    /// <summary>Whether to include praise for good code.</summary>
    public bool IncludePraise { get; init; } = false;

    /// <summary>Language or framework hints.</summary>
    public IReadOnlyList<string> LanguageHints { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A finding from code review.
/// </summary>
public class ReviewFinding
{
    /// <summary>Unique identifier.</summary>
    public string FindingId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Line number (in the new file).</summary>
    public int LineNumber { get; init; }

    /// <summary>End line number (if spans multiple lines).</summary>
    public int? EndLineNumber { get; init; }

    /// <summary>Severity.</summary>
    public required ReviewSeverity Severity { get; init; }

    /// <summary>Category.</summary>
    public required ReviewCategory Category { get; init; }

    /// <summary>Short title.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description.</summary>
    public required string Description { get; init; }

    /// <summary>Code snippet with the issue.</summary>
    public string? CodeSnippet { get; init; }

    /// <summary>Suggested fix.</summary>
    public string? SuggestedFix { get; init; }

    /// <summary>Suggested code replacement.</summary>
    public string? SuggestedCode { get; init; }

    /// <summary>Link to relevant documentation.</summary>
    public string? DocumentationLink { get; init; }

    /// <summary>Confidence in the finding (0.0 - 1.0).</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Result of a code review.
/// </summary>
public class CodeReviewResult
{
    /// <summary>The request ID.</summary>
    public required string RequestId { get; init; }

    /// <summary>Overall assessment (Approve, RequestChanges, Comment).</summary>
    public required string Assessment { get; init; }

    /// <summary>Summary of the review.</summary>
    public required string Summary { get; init; }

    /// <summary>All findings.</summary>
    public IReadOnlyList<ReviewFinding> Findings { get; init; } = Array.Empty<ReviewFinding>();

    /// <summary>Count by severity.</summary>
    public Dictionary<ReviewSeverity, int> SeverityCounts { get; init; } = new();

    /// <summary>Count by category.</summary>
    public Dictionary<ReviewCategory, int> CategoryCounts { get; init; } = new();

    /// <summary>Positive observations (if enabled).</summary>
    public IReadOnlyList<string> Praise { get; init; } = Array.Empty<string>();

    /// <summary>Files reviewed.</summary>
    public int FilesReviewed { get; init; }

    /// <summary>Lines reviewed.</summary>
    public int LinesReviewed { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// Configuration for the review agent.
/// </summary>
public class ReviewConfiguration
{
    /// <summary>Rules to enable.</summary>
    public IReadOnlyList<string> EnabledRules { get; init; } = Array.Empty<string>();

    /// <summary>Rules to disable.</summary>
    public IReadOnlyList<string> DisabledRules { get; init; } = Array.Empty<string>();

    /// <summary>Custom rules (name → description).</summary>
    public Dictionary<string, string> CustomRules { get; init; } = new();

    /// <summary>File patterns to exclude.</summary>
    public IReadOnlyList<string> ExcludePatterns { get; init; } = Array.Empty<string>();

    /// <summary>Language-specific configurations.</summary>
    public Dictionary<string, Dictionary<string, string>> LanguageConfig { get; init; } = new();
}

/// <summary>
/// Port for AI-assisted code review.
/// Implements the "AI-Assisted Code Review / Verification" pattern.
/// </summary>
/// <remarks>
/// This port provides automated code review capabilities that analyze
/// code changes for bugs, security issues, style violations, and
/// other concerns, providing actionable feedback.
/// </remarks>
public interface ICodeReviewPort
{
    /// <summary>
    /// Reviews code changes.
    /// </summary>
    /// <param name="request">The review request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The review result.</returns>
    Task<CodeReviewResult> ReviewAsync(
        CodeReviewRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews a specific file.
    /// </summary>
    /// <param name="filePath">Path to the file.</param>
    /// <param name="content">File content.</param>
    /// <param name="focusAreas">Focus areas.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Findings for the file.</returns>
    Task<IReadOnlyList<ReviewFinding>> ReviewFileAsync(
        string filePath,
        string content,
        IEnumerable<ReviewCategory>? focusAreas = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggestions for fixing a finding.
    /// </summary>
    /// <param name="finding">The finding.</param>
    /// <param name="context">Surrounding code context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suggested fix as code.</returns>
    Task<string> GetFixSuggestionAsync(
        ReviewFinding finding,
        string context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets review configuration.
    /// </summary>
    /// <param name="configuration">Configuration to set (null = get).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<ReviewConfiguration> ConfigureAsync(
        ReviewConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies that suggested fixes are correct.
    /// </summary>
    /// <param name="originalCode">Original code.</param>
    /// <param name="fixedCode">Fixed code.</param>
    /// <param name="finding">The finding being fixed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the fix is valid.</returns>
    Task<(bool IsValid, string? Issue)> VerifyFixAsync(
        string originalCode,
        string fixedCode,
        ReviewFinding finding,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets review statistics.
    /// </summary>
    /// <param name="since">Start time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Statistics.</returns>
    Task<ReviewStatistics> GetStatisticsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Review statistics.
/// </summary>
public class ReviewStatistics
{
    /// <summary>Total reviews.</summary>
    public int TotalReviews { get; init; }
    /// <summary>Total findings.</summary>
    public int TotalFindings { get; init; }
    /// <summary>Average findings per review.</summary>
    public double AverageFindingsPerReview { get; init; }
    /// <summary>Findings by severity.</summary>
    public Dictionary<ReviewSeverity, int> FindingsBySeverity { get; init; } = new();
    /// <summary>Findings by category.</summary>
    public Dictionary<ReviewCategory, int> FindingsByCategory { get; init; } = new();
    /// <summary>Most common issues.</summary>
    public IReadOnlyList<string> MostCommonIssues { get; init; } = Array.Empty<string>();
}
