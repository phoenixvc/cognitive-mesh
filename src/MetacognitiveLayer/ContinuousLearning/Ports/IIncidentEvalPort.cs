namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// Severity of an incident.
/// </summary>
public enum IncidentSeverity
{
    /// <summary>Minor issue with no user impact.</summary>
    Low,
    /// <summary>Noticeable issue with limited impact.</summary>
    Medium,
    /// <summary>Significant issue affecting functionality.</summary>
    High,
    /// <summary>Critical issue causing major failure.</summary>
    Critical
}

/// <summary>
/// Status of an incident in the eval pipeline.
/// </summary>
public enum IncidentEvalStatus
{
    /// <summary>Incident reported but not analyzed.</summary>
    Reported,
    /// <summary>Incident is being analyzed.</summary>
    Analyzing,
    /// <summary>Eval has been generated.</summary>
    EvalGenerated,
    /// <summary>Eval is being reviewed.</summary>
    UnderReview,
    /// <summary>Eval has been approved and added to suite.</summary>
    Approved,
    /// <summary>Eval was rejected.</summary>
    Rejected
}

/// <summary>
/// Represents an incident that occurred.
/// </summary>
public class AgentIncident
{
    /// <summary>Unique identifier.</summary>
    public string IncidentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Title of the incident.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description.</summary>
    public required string Description { get; init; }

    /// <summary>Severity level.</summary>
    public IncidentSeverity Severity { get; init; }

    /// <summary>Agent involved.</summary>
    public required string AgentId { get; init; }

    /// <summary>Session where incident occurred.</summary>
    public string? SessionId { get; init; }

    /// <summary>Relevant execution logs.</summary>
    public IReadOnlyList<string> ExecutionLogs { get; init; } = Array.Empty<string>();

    /// <summary>Expected behavior.</summary>
    public string? ExpectedBehavior { get; init; }

    /// <summary>Actual behavior observed.</summary>
    public string? ActualBehavior { get; init; }

    /// <summary>Root cause if determined.</summary>
    public string? RootCause { get; init; }

    /// <summary>When the incident occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>When the incident was reported.</summary>
    public DateTimeOffset ReportedAt { get; init; }

    /// <summary>Who reported the incident.</summary>
    public string? ReportedBy { get; init; }

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An evaluation case generated from an incident.
/// </summary>
public class GeneratedEval
{
    /// <summary>Unique identifier.</summary>
    public string EvalId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Source incident ID.</summary>
    public required string IncidentId { get; init; }

    /// <summary>Name of the eval case.</summary>
    public required string Name { get; init; }

    /// <summary>Description of what this eval tests.</summary>
    public required string Description { get; init; }

    /// <summary>Input for the eval (prompt/context).</summary>
    public required string Input { get; init; }

    /// <summary>Expected output or behavior.</summary>
    public required string ExpectedOutput { get; init; }

    /// <summary>Scoring criteria.</summary>
    public IReadOnlyList<EvalCriterion> Criteria { get; init; } = Array.Empty<EvalCriterion>();

    /// <summary>Agent capabilities being tested.</summary>
    public IReadOnlyList<string> TestedCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>Priority of this eval.</summary>
    public int Priority { get; init; } = 50;

    /// <summary>Whether this is a regression test.</summary>
    public bool IsRegressionTest { get; init; } = true;

    /// <summary>Current status in the pipeline.</summary>
    public IncidentEvalStatus Status { get; init; }

    /// <summary>When the eval was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }

    /// <summary>Confidence in the eval quality (0.0 - 1.0).</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// A criterion for evaluating an eval case.
/// </summary>
public class EvalCriterion
{
    /// <summary>Name of the criterion.</summary>
    public required string Name { get; init; }

    /// <summary>Description of what it measures.</summary>
    public required string Description { get; init; }

    /// <summary>Weight in overall score (0.0 - 1.0).</summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>Type of evaluation (Exact, Contains, Semantic, Custom).</summary>
    public required string EvaluationType { get; init; }

    /// <summary>Expected value or pattern.</summary>
    public string? ExpectedValue { get; init; }

    /// <summary>Pass threshold (0.0 - 1.0).</summary>
    public double PassThreshold { get; init; } = 0.8;
}

/// <summary>
/// Result of running an eval.
/// </summary>
public class EvalResult
{
    /// <summary>The eval ID.</summary>
    public required string EvalId { get; init; }

    /// <summary>Whether the eval passed.</summary>
    public required bool Passed { get; init; }

    /// <summary>Overall score (0.0 - 1.0).</summary>
    public double Score { get; init; }

    /// <summary>Scores by criterion.</summary>
    public Dictionary<string, double> CriterionScores { get; init; } = new();

    /// <summary>Actual output from the agent.</summary>
    public string? ActualOutput { get; init; }

    /// <summary>Explanation of the result.</summary>
    public string? Explanation { get; init; }

    /// <summary>When the eval was run.</summary>
    public DateTimeOffset RunAt { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// Port for incident-to-evaluation synthesis.
/// Implements the "Incident-to-Eval Synthesis" pattern.
/// </summary>
/// <remarks>
/// This port transforms production incidents into evaluation cases,
/// creating a feedback loop that prevents regression and captures
/// lessons learned from real-world failures.
/// </remarks>
public interface IIncidentEvalPort
{
    /// <summary>
    /// Reports a new incident.
    /// </summary>
    /// <param name="incident">The incident to report.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The incident ID.</returns>
    Task<string> ReportIncidentAsync(
        AgentIncident incident,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an eval case from an incident.
    /// </summary>
    /// <param name="incidentId">The incident ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated eval.</returns>
    Task<GeneratedEval> GenerateEvalAsync(
        string incidentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reviews and approves an eval.
    /// </summary>
    /// <param name="evalId">The eval ID.</param>
    /// <param name="approved">Whether to approve.</param>
    /// <param name="comments">Review comments.</param>
    /// <param name="reviewedBy">Who reviewed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ReviewEvalAsync(
        string evalId,
        bool approved,
        string? comments,
        string reviewedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs an eval case.
    /// </summary>
    /// <param name="evalId">The eval to run.</param>
    /// <param name="agentId">The agent to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The eval result.</returns>
    Task<EvalResult> RunEvalAsync(
        string evalId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs all approved evals against an agent.
    /// </summary>
    /// <param name="agentId">The agent to test.</param>
    /// <param name="tags">Optional tags to filter evals.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All eval results.</returns>
    Task<IReadOnlyList<EvalResult>> RunEvalSuiteAsync(
        string agentId,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets incidents.
    /// </summary>
    /// <param name="agentId">Filter by agent (null = all).</param>
    /// <param name="severity">Filter by severity (null = all).</param>
    /// <param name="since">Filter by time.</param>
    /// <param name="limit">Maximum to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of incidents.</returns>
    Task<IReadOnlyList<AgentIncident>> GetIncidentsAsync(
        string? agentId = null,
        IncidentSeverity? severity = null,
        DateTimeOffset? since = null,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets evals.
    /// </summary>
    /// <param name="status">Filter by status (null = all).</param>
    /// <param name="limit">Maximum to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of evals.</returns>
    Task<IReadOnlyList<GeneratedEval>> GetEvalsAsync(
        IncidentEvalStatus? status = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets eval results history for an agent.
    /// </summary>
    /// <param name="agentId">The agent ID.</param>
    /// <param name="evalId">Specific eval (null = all).</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Eval results.</returns>
    Task<IReadOnlyList<EvalResult>> GetEvalHistoryAsync(
        string agentId,
        string? evalId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
