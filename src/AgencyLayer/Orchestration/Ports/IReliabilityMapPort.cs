namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Category of reliability problem.
/// </summary>
public enum ReliabilityCategory
{
    /// <summary>Instruction following issues.</summary>
    InstructionFollowing,
    /// <summary>Tool usage problems.</summary>
    ToolUsage,
    /// <summary>Context management issues.</summary>
    ContextManagement,
    /// <summary>Error handling problems.</summary>
    ErrorHandling,
    /// <summary>Output quality issues.</summary>
    OutputQuality,
    /// <summary>Consistency problems.</summary>
    Consistency,
    /// <summary>Performance issues.</summary>
    Performance,
    /// <summary>Safety/boundary issues.</summary>
    Safety
}

/// <summary>
/// Severity of a reliability problem.
/// </summary>
public enum ProblemSeverity
{
    /// <summary>Minor annoyance.</summary>
    Low,
    /// <summary>Noticeable issue.</summary>
    Medium,
    /// <summary>Significant problem.</summary>
    High,
    /// <summary>Critical reliability issue.</summary>
    Critical
}

/// <summary>
/// A known reliability problem.
/// </summary>
public class ReliabilityProblem
{
    /// <summary>Problem identifier.</summary>
    public required string ProblemId { get; init; }

    /// <summary>Problem name.</summary>
    public required string Name { get; init; }

    /// <summary>Detailed description.</summary>
    public required string Description { get; init; }

    /// <summary>Category.</summary>
    public ReliabilityCategory Category { get; init; }

    /// <summary>Severity.</summary>
    public ProblemSeverity Severity { get; init; }

    /// <summary>Symptoms to look for.</summary>
    public IReadOnlyList<string> Symptoms { get; init; } = Array.Empty<string>();

    /// <summary>Root causes.</summary>
    public IReadOnlyList<string> RootCauses { get; init; } = Array.Empty<string>();

    /// <summary>Mitigations.</summary>
    public IReadOnlyList<ReliabilityMitigation> Mitigations { get; init; } = Array.Empty<ReliabilityMitigation>();

    /// <summary>Detection patterns.</summary>
    public IReadOnlyList<string> DetectionPatterns { get; init; } = Array.Empty<string>();

    /// <summary>Models affected.</summary>
    public IReadOnlyList<string> AffectedModels { get; init; } = Array.Empty<string>();

    /// <summary>Reference links.</summary>
    public IReadOnlyList<string> References { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A mitigation for a reliability problem.
/// </summary>
public class ReliabilityMitigation
{
    /// <summary>Mitigation name.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Implementation details.</summary>
    public string? Implementation { get; init; }

    /// <summary>Effectiveness (0.0 - 1.0).</summary>
    public double Effectiveness { get; init; }

    /// <summary>Cost/complexity (0.0 - 1.0).</summary>
    public double Complexity { get; init; }

    /// <summary>Whether this is automated or manual.</summary>
    public bool IsAutomated { get; init; }
}

/// <summary>
/// A checklist item for reliability assessment.
/// </summary>
public class ReliabilityChecklistItem
{
    /// <summary>Item identifier.</summary>
    public required string ItemId { get; init; }

    /// <summary>Category.</summary>
    public ReliabilityCategory Category { get; init; }

    /// <summary>Question or check.</summary>
    public required string Question { get; init; }

    /// <summary>Guidance for answering.</summary>
    public string? Guidance { get; init; }

    /// <summary>Related problems if this fails.</summary>
    public IReadOnlyList<string> RelatedProblemIds { get; init; } = Array.Empty<string>();

    /// <summary>Whether this is critical.</summary>
    public bool IsCritical { get; init; }

    /// <summary>Order in the checklist.</summary>
    public int Order { get; init; }
}

/// <summary>
/// Result of a checklist assessment.
/// </summary>
public class ChecklistAssessmentResult
{
    /// <summary>Assessment identifier.</summary>
    public string AssessmentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Agent or workflow assessed.</summary>
    public required string TargetId { get; init; }

    /// <summary>Overall score (0.0 - 1.0).</summary>
    public double OverallScore { get; init; }

    /// <summary>Items that passed.</summary>
    public IReadOnlyList<string> PassedItems { get; init; } = Array.Empty<string>();

    /// <summary>Items that failed.</summary>
    public IReadOnlyList<string> FailedItems { get; init; } = Array.Empty<string>();

    /// <summary>Items not applicable.</summary>
    public IReadOnlyList<string> NotApplicableItems { get; init; } = Array.Empty<string>();

    /// <summary>Identified problems.</summary>
    public IReadOnlyList<ReliabilityProblem> IdentifiedProblems { get; init; } = Array.Empty<ReliabilityProblem>();

    /// <summary>Recommended mitigations.</summary>
    public IReadOnlyList<ReliabilityMitigation> RecommendedMitigations { get; init; } = Array.Empty<ReliabilityMitigation>();

    /// <summary>Scores by category.</summary>
    public Dictionary<ReliabilityCategory, double> CategoryScores { get; init; } = new();

    /// <summary>When the assessment was performed.</summary>
    public DateTimeOffset AssessedAt { get; init; }
}

/// <summary>
/// Port for reliability problem tracking and assessment.
/// Implements the "Reliability Problem Map Checklist" pattern.
/// </summary>
/// <remarks>
/// This port provides a structured approach to identifying, tracking,
/// and mitigating reliability problems in agentic systems, based on
/// known failure modes and best practices.
/// </remarks>
public interface IReliabilityMapPort
{
    /// <summary>
    /// Gets all known reliability problems.
    /// </summary>
    /// <param name="category">Filter by category (null = all).</param>
    /// <param name="severity">Minimum severity (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reliability problems.</returns>
    Task<IReadOnlyList<ReliabilityProblem>> GetProblemsAsync(
        ReliabilityCategory? category = null,
        ProblemSeverity? severity = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific problem.
    /// </summary>
    /// <param name="problemId">The problem ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The problem.</returns>
    Task<ReliabilityProblem?> GetProblemAsync(
        string problemId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the reliability checklist.
    /// </summary>
    /// <param name="category">Filter by category (null = all).</param>
    /// <param name="criticalOnly">Only critical items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Checklist items.</returns>
    Task<IReadOnlyList<ReliabilityChecklistItem>> GetChecklistAsync(
        ReliabilityCategory? category = null,
        bool criticalOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a checklist assessment.
    /// </summary>
    /// <param name="targetId">Agent or workflow to assess.</param>
    /// <param name="answers">Answers to checklist items (itemId → passed).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Assessment result.</returns>
    Task<ChecklistAssessmentResult> RunAssessmentAsync(
        string targetId,
        IReadOnlyDictionary<string, bool> answers,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-detects reliability problems from execution logs.
    /// </summary>
    /// <param name="targetId">Agent or workflow to analyze.</param>
    /// <param name="since">Start time for analysis.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Detected problems.</returns>
    Task<IReadOnlyList<ReliabilityProblem>> DetectProblemsAsync(
        string targetId,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets mitigations for identified problems.
    /// </summary>
    /// <param name="problemIds">Problems to get mitigations for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mitigations grouped by problem.</returns>
    Task<IReadOnlyDictionary<string, IReadOnlyList<ReliabilityMitigation>>> GetMitigationsAsync(
        IEnumerable<string> problemIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records application of a mitigation.
    /// </summary>
    /// <param name="targetId">Target that was mitigated.</param>
    /// <param name="problemId">Problem being addressed.</param>
    /// <param name="mitigationName">Mitigation applied.</param>
    /// <param name="appliedBy">Who applied it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordMitigationAsync(
        string targetId,
        string problemId,
        string mitigationName,
        string appliedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets assessment history.
    /// </summary>
    /// <param name="targetId">Filter by target (null = all).</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Assessment results.</returns>
    Task<IReadOnlyList<ChecklistAssessmentResult>> GetAssessmentHistoryAsync(
        string? targetId = null,
        int limit = 50,
        CancellationToken cancellationToken = default);
}
