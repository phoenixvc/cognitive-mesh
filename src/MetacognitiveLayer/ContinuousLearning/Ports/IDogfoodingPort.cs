namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// A dogfooding session.
/// </summary>
public class DogfoodingSession
{
    /// <summary>Session identifier.</summary>
    public string SessionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Feature being tested.</summary>
    public required string FeatureId { get; init; }

    /// <summary>Session type.</summary>
    public DogfoodingType Type { get; init; }

    /// <summary>Participants.</summary>
    public IReadOnlyList<string> Participants { get; init; } = Array.Empty<string>();

    /// <summary>Goals.</summary>
    public IReadOnlyList<string> Goals { get; init; } = Array.Empty<string>();

    /// <summary>Status.</summary>
    public DogfoodingStatus Status { get; init; }

    /// <summary>Started at.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>Ended at.</summary>
    public DateTimeOffset? EndedAt { get; init; }

    /// <summary>Iteration number.</summary>
    public int Iteration { get; init; } = 1;
}

/// <summary>
/// Dogfooding type.
/// </summary>
public enum DogfoodingType
{
    /// <summary>Internal team testing.</summary>
    InternalTeam,
    /// <summary>Automated testing.</summary>
    Automated,
    /// <summary>User simulation.</summary>
    Simulation,
    /// <summary>A/B testing.</summary>
    ABTest
}

/// <summary>
/// Dogfooding status.
/// </summary>
public enum DogfoodingStatus
{
    /// <summary>Planned.</summary>
    Planned,

    /// <summary>Active.</summary>
    Active,

    /// <summary>Paused.</summary>
    Paused,

    /// <summary>Completed.</summary>
    Completed,

    /// <summary>Cancelled.</summary>
    Cancelled
}

/// <summary>
/// Dogfooding finding.
/// </summary>
public class DogfoodingFinding
{
    /// <summary>Finding identifier.</summary>
    public string FindingId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Finding type.</summary>
    public FindingType Type { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Severity.</summary>
    public FindingSeverity Severity { get; init; }

    /// <summary>Steps to reproduce.</summary>
    public string? StepsToReproduce { get; init; }

    /// <summary>Impact description.</summary>
    public string? Impact { get; init; }

    /// <summary>Suggested fix.</summary>
    public string? SuggestedFix { get; init; }

    /// <summary>Reported by.</summary>
    public required string ReportedBy { get; init; }

    /// <summary>Reported at.</summary>
    public DateTimeOffset ReportedAt { get; init; }

    /// <summary>Status.</summary>
    public FindingStatus Status { get; init; }
}

/// <summary>
/// Finding type.
/// </summary>
public enum FindingType
{
    /// <summary>Bug.</summary>
    Bug,

    /// <summary>UX issue.</summary>
    UXIssue,

    /// <summary>Performance.</summary>
    Performance,

    /// <summary>Feature request.</summary>
    FeatureRequest,

    /// <summary>Documentation.</summary>
    Documentation,

    /// <summary>Security.</summary>
    Security
}

/// <summary>
/// Finding severity.
/// </summary>
public enum FindingSeverity
{
    /// <summary>Low.</summary>
    Low,

    /// <summary>Medium.</summary>
    Medium,

    /// <summary>High.</summary>
    High,

    /// <summary>Critical.</summary>
    Critical
}

/// <summary>
/// Finding status.
/// </summary>
public enum FindingStatus
{
    /// <summary>New.</summary>
    New,

    /// <summary>Acknowledged.</summary>
    Acknowledged,

    /// <summary>In progress.</summary>
    InProgress,

    /// <summary>Resolved.</summary>
    Resolved,

    /// <summary>Won't fix.</summary>
    WontFix,

    /// <summary>Duplicate.</summary>
    Duplicate
}

/// <summary>
/// Iteration report.
/// </summary>
public class IterationReport
{
    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Iteration number.</summary>
    public int Iteration { get; init; }

    /// <summary>Summary.</summary>
    public required string Summary { get; init; }

    /// <summary>Findings count.</summary>
    public int TotalFindings { get; init; }

    /// <summary>Critical findings.</summary>
    public int CriticalFindings { get; init; }

    /// <summary>Resolved findings.</summary>
    public int ResolvedFindings { get; init; }

    /// <summary>Key improvements.</summary>
    public IReadOnlyList<string> KeyImprovements { get; init; } = Array.Empty<string>();

    /// <summary>Remaining issues.</summary>
    public IReadOnlyList<string> RemainingIssues { get; init; } = Array.Empty<string>();

    /// <summary>Ready for next iteration.</summary>
    public bool ReadyForNextIteration { get; init; }

    /// <summary>Ready for release.</summary>
    public bool ReadyForRelease { get; init; }
}

/// <summary>
/// Port for dogfooding with rapid iteration.
/// Implements the "Dogfooding with Rapid Iteration" pattern.
/// </summary>
public interface IDogfoodingPort
{
    /// <summary>
    /// Starts a dogfooding session.
    /// </summary>
    Task<DogfoodingSession> StartSessionAsync(
        string featureId,
        DogfoodingType type,
        IEnumerable<string> goals,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session details.
    /// </summary>
    Task<DogfoodingSession?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports a finding.
    /// </summary>
    Task<DogfoodingFinding> ReportFindingAsync(
        DogfoodingFinding finding,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates finding status.
    /// </summary>
    Task<DogfoodingFinding> UpdateFindingStatusAsync(
        string findingId,
        FindingStatus status,
        string? resolution = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets findings for a session.
    /// </summary>
    Task<IReadOnlyList<DogfoodingFinding>> GetFindingsAsync(
        string sessionId,
        FindingStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates iteration report.
    /// </summary>
    Task<IterationReport> GenerateReportAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts next iteration.
    /// </summary>
    Task<DogfoodingSession> StartNextIterationAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a session.
    /// </summary>
    Task<DogfoodingSession> EndSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active sessions.
    /// </summary>
    Task<IReadOnlyList<DogfoodingSession>> GetActiveSessionsAsync(
        CancellationToken cancellationToken = default);
}
