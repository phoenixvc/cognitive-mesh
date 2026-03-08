namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// CI/CD pipeline result.
/// </summary>
public class CIResult
{
    /// <summary>Result identifier.</summary>
    public required string ResultId { get; init; }

    /// <summary>Pipeline identifier.</summary>
    public required string PipelineId { get; init; }

    /// <summary>Run identifier.</summary>
    public required string RunId { get; init; }

    /// <summary>Commit SHA.</summary>
    public required string CommitSha { get; init; }

    /// <summary>Branch.</summary>
    public required string Branch { get; init; }

    /// <summary>Overall status.</summary>
    public CIStatus Status { get; init; }

    /// <summary>Stage results.</summary>
    public IReadOnlyList<CIStageResult> Stages { get; init; } = Array.Empty<CIStageResult>();

    /// <summary>Test results.</summary>
    public TestSummary? TestSummary { get; init; }

    /// <summary>Lint/style results.</summary>
    public LintSummary? LintSummary { get; init; }

    /// <summary>Build logs URL.</summary>
    public string? LogsUrl { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// CI status.
/// </summary>
public enum CIStatus
{
    /// <summary>Pending.</summary>
    Pending,
    /// <summary>Running.</summary>
    Running,
    /// <summary>Passed.</summary>
    Passed,
    /// <summary>Failed.</summary>
    Failed,
    /// <summary>Cancelled.</summary>
    Cancelled,
    /// <summary>Skipped.</summary>
    Skipped
}

/// <summary>
/// CI stage result.
/// </summary>
public class CIStageResult
{
    /// <summary>Stage name.</summary>
    public required string Name { get; init; }

    /// <summary>Status.</summary>
    public CIStatus Status { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Test summary.
/// </summary>
public class TestSummary
{
    /// <summary>Total tests.</summary>
    public int Total { get; init; }

    /// <summary>Passed tests.</summary>
    public int Passed { get; init; }

    /// <summary>Failed tests.</summary>
    public int Failed { get; init; }

    /// <summary>Skipped tests.</summary>
    public int Skipped { get; init; }

    /// <summary>Failed test details.</summary>
    public IReadOnlyList<FailedTest> FailedTests { get; init; } = Array.Empty<FailedTest>();
}

/// <summary>
/// Failed test details.
/// </summary>
public class FailedTest
{
    /// <summary>Test name.</summary>
    public required string Name { get; init; }

    /// <summary>Error message.</summary>
    public required string Error { get; init; }

    /// <summary>Stack trace.</summary>
    public string? StackTrace { get; init; }

    /// <summary>File path.</summary>
    public string? FilePath { get; init; }

    /// <summary>Line number.</summary>
    public int? LineNumber { get; init; }
}

/// <summary>
/// Lint summary.
/// </summary>
public class LintSummary
{
    /// <summary>Total issues.</summary>
    public int TotalIssues { get; init; }

    /// <summary>Errors.</summary>
    public int Errors { get; init; }

    /// <summary>Warnings.</summary>
    public int Warnings { get; init; }

    /// <summary>Issue details.</summary>
    public IReadOnlyList<LintIssue> Issues { get; init; } = Array.Empty<LintIssue>();
}

/// <summary>
/// Lint issue.
/// </summary>
public class LintIssue
{
    /// <summary>Rule.</summary>
    public required string Rule { get; init; }

    /// <summary>Message.</summary>
    public required string Message { get; init; }

    /// <summary>Severity.</summary>
    public required string Severity { get; init; }

    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Line number.</summary>
    public int LineNumber { get; init; }
}

/// <summary>
/// CI feedback analysis.
/// </summary>
public class CIFeedbackAnalysis
{
    /// <summary>Result identifier.</summary>
    public required string ResultId { get; init; }

    /// <summary>Summary.</summary>
    public required string Summary { get; init; }

    /// <summary>Root causes identified.</summary>
    public IReadOnlyList<string> RootCauses { get; init; } = Array.Empty<string>();

    /// <summary>Suggested fixes.</summary>
    public IReadOnlyList<SuggestedFix> SuggestedFixes { get; init; } = Array.Empty<SuggestedFix>();

    /// <summary>Patterns detected.</summary>
    public IReadOnlyList<string> Patterns { get; init; } = Array.Empty<string>();

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// A suggested fix.
/// </summary>
public class SuggestedFix
{
    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>File path.</summary>
    public string? FilePath { get; init; }

    /// <summary>Code change.</summary>
    public string? CodeChange { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// Port for background agent with CI feedback.
/// Implements the "Background Agent with CI Feedback" pattern.
/// </summary>
public interface ICIFeedbackPort
{
    /// <summary>
    /// Processes CI results and provides feedback.
    /// </summary>
    Task<CIFeedbackAnalysis> AnalyzeResultAsync(
        CIResult result,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CI results for a commit.
    /// </summary>
    Task<IReadOnlyList<CIResult>> GetResultsForCommitAsync(
        string commitSha,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent CI results.
    /// </summary>
    Task<IReadOnlyList<CIResult>> GetRecentResultsAsync(
        string? branch = null,
        int limit = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to CI results.
    /// </summary>
    IAsyncEnumerable<CIResult> SubscribeToResultsAsync(
        string? branch = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates fix suggestions for failed tests.
    /// </summary>
    Task<IReadOnlyList<SuggestedFix>> GenerateFixesAsync(
        IEnumerable<FailedTest> failedTests,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a suggested fix.
    /// </summary>
    Task<bool> ApplyFixAsync(
        SuggestedFix fix,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records feedback on fix effectiveness.
    /// </summary>
    Task RecordFixFeedbackAsync(
        string fixDescription,
        bool wasEffective,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets CI health metrics.
    /// </summary>
    Task<CIHealthMetrics> GetHealthMetricsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// CI health metrics.
/// </summary>
public class CIHealthMetrics
{
    /// <summary>Total runs.</summary>
    public int TotalRuns { get; init; }

    /// <summary>Passed runs.</summary>
    public int PassedRuns { get; init; }

    /// <summary>Failed runs.</summary>
    public int FailedRuns { get; init; }

    /// <summary>Pass rate.</summary>
    public double PassRate { get; init; }

    /// <summary>Average duration.</summary>
    public TimeSpan AverageDuration { get; init; }

    /// <summary>Common failures.</summary>
    public IReadOnlyList<string> CommonFailures { get; init; } = Array.Empty<string>();
}
