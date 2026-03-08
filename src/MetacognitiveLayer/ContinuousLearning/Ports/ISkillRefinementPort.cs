namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// Type of refinement applied to a skill.
/// </summary>
public enum RefinementType
{
    /// <summary>Prompt improvement.</summary>
    PromptRefinement,
    /// <summary>Parameter tuning.</summary>
    ParameterTuning,
    /// <summary>Tool selection optimization.</summary>
    ToolOptimization,
    /// <summary>Error handling improvement.</summary>
    ErrorHandling,
    /// <summary>Output format improvement.</summary>
    OutputFormat,
    /// <summary>Context usage optimization.</summary>
    ContextOptimization
}

/// <summary>
/// A skill definition that can be refined.
/// </summary>
public class RefinableSkill
{
    /// <summary>Unique identifier.</summary>
    public required string SkillId { get; init; }

    /// <summary>Name of the skill.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>The current prompt template.</summary>
    public required string PromptTemplate { get; init; }

    /// <summary>Parameters and their current values.</summary>
    public Dictionary<string, object> Parameters { get; init; } = new();

    /// <summary>Tools this skill uses.</summary>
    public IReadOnlyList<string> Tools { get; init; } = Array.Empty<string>();

    /// <summary>Version number.</summary>
    public int Version { get; init; } = 1;

    /// <summary>Performance metrics.</summary>
    public SkillMetrics Metrics { get; init; } = new();

    /// <summary>When last refined.</summary>
    public DateTimeOffset? LastRefinedAt { get; init; }
}

/// <summary>
/// Performance metrics for a skill.
/// </summary>
public class SkillMetrics
{
    /// <summary>Success rate (0.0 - 1.0).</summary>
    public double SuccessRate { get; init; }

    /// <summary>Average execution time in milliseconds.</summary>
    public double AverageExecutionTimeMs { get; init; }

    /// <summary>Total executions.</summary>
    public int TotalExecutions { get; init; }

    /// <summary>Successful executions.</summary>
    public int SuccessfulExecutions { get; init; }

    /// <summary>Failed executions.</summary>
    public int FailedExecutions { get; init; }

    /// <summary>Average user satisfaction (if tracked).</summary>
    public double? AverageSatisfaction { get; init; }

    /// <summary>Common error types.</summary>
    public IReadOnlyList<string> CommonErrors { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An execution outcome for learning.
/// </summary>
public class SkillExecutionOutcome
{
    /// <summary>The skill ID.</summary>
    public required string SkillId { get; init; }

    /// <summary>Unique execution ID.</summary>
    public string ExecutionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Input provided.</summary>
    public required string Input { get; init; }

    /// <summary>Output generated.</summary>
    public required string Output { get; init; }

    /// <summary>Whether the execution succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Execution duration in milliseconds.</summary>
    public double DurationMs { get; init; }

    /// <summary>User feedback (if provided).</summary>
    public string? UserFeedback { get; init; }

    /// <summary>Satisfaction rating (1-5).</summary>
    public int? SatisfactionRating { get; init; }

    /// <summary>When the execution occurred.</summary>
    public DateTimeOffset ExecutedAt { get; init; }
}

/// <summary>
/// A proposed refinement.
/// </summary>
public class ProposedRefinement
{
    /// <summary>Unique identifier.</summary>
    public string RefinementId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The skill to refine.</summary>
    public required string SkillId { get; init; }

    /// <summary>Type of refinement.</summary>
    public required RefinementType Type { get; init; }

    /// <summary>Description of the refinement.</summary>
    public required string Description { get; init; }

    /// <summary>Current value.</summary>
    public required string CurrentValue { get; init; }

    /// <summary>Proposed new value.</summary>
    public required string ProposedValue { get; init; }

    /// <summary>Rationale for the change.</summary>
    public required string Rationale { get; init; }

    /// <summary>Expected improvement.</summary>
    public string? ExpectedImprovement { get; init; }

    /// <summary>Confidence in the refinement (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>When the refinement was proposed.</summary>
    public DateTimeOffset ProposedAt { get; init; }
}

/// <summary>
/// Result of applying a refinement.
/// </summary>
public class RefinementResult
{
    /// <summary>The refinement ID.</summary>
    public required string RefinementId { get; init; }

    /// <summary>Whether the refinement was applied.</summary>
    public required bool Applied { get; init; }

    /// <summary>New version of the skill.</summary>
    public int? NewVersion { get; init; }

    /// <summary>Reason if not applied.</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Port for iterative skill refinement.
/// Implements the "Iterative Prompt and Skill Refinement" pattern.
/// </summary>
/// <remarks>
/// This port enables continuous improvement of agent skills based on
/// execution outcomes and user feedback, automatically proposing
/// refinements to prompts, parameters, and tool usage.
/// </remarks>
public interface ISkillRefinementPort
{
    /// <summary>
    /// Records an execution outcome for learning.
    /// </summary>
    /// <param name="outcome">The outcome.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordOutcomeAsync(
        SkillExecutionOutcome outcome,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes outcomes and proposes refinements.
    /// </summary>
    /// <param name="skillId">The skill to analyze.</param>
    /// <param name="minOutcomes">Minimum outcomes required.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Proposed refinements.</returns>
    Task<IReadOnlyList<ProposedRefinement>> ProposeRefinementsAsync(
        string skillId,
        int minOutcomes = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a refinement.
    /// </summary>
    /// <param name="refinementId">The refinement to apply.</param>
    /// <param name="appliedBy">Who applied it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<RefinementResult> ApplyRefinementAsync(
        string refinementId,
        string appliedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a refinement.
    /// </summary>
    /// <param name="refinementId">The refinement to reject.</param>
    /// <param name="reason">Reason for rejection.</param>
    /// <param name="rejectedBy">Who rejected it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RejectRefinementAsync(
        string refinementId,
        string reason,
        string rejectedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a skill.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The skill.</returns>
    Task<RefinableSkill?> GetSkillAsync(
        string skillId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets skill metrics.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <param name="since">Start time.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Metrics.</returns>
    Task<SkillMetrics> GetMetricsAsync(
        string skillId,
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending refinements.
    /// </summary>
    /// <param name="skillId">Filter by skill (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pending refinements.</returns>
    Task<IReadOnlyList<ProposedRefinement>> GetPendingRefinementsAsync(
        string? skillId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets refinement history.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Applied refinements.</returns>
    Task<IReadOnlyList<ProposedRefinement>> GetRefinementHistoryAsync(
        string skillId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back to a previous version.
    /// </summary>
    /// <param name="skillId">The skill ID.</param>
    /// <param name="targetVersion">Version to roll back to.</param>
    /// <param name="reason">Reason for rollback.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RollbackAsync(
        string skillId,
        int targetVersion,
        string reason,
        CancellationToken cancellationToken = default);
}
