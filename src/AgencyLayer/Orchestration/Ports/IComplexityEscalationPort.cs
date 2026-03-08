namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Complexity level.
/// </summary>
public enum ComplexityLevel
{
    /// <summary>Simple, routine task.</summary>
    Simple = 1,
    /// <summary>Moderate complexity.</summary>
    Moderate = 2,
    /// <summary>Complex, requires planning.</summary>
    Complex = 3,
    /// <summary>Highly complex, multi-step.</summary>
    High = 4,
    /// <summary>Expert level, requires supervision.</summary>
    Expert = 5
}

/// <summary>
/// Complexity assessment for a task.
/// </summary>
public class ComplexityAssessment
{
    /// <summary>Assessment identifier.</summary>
    public string AssessmentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Assessed complexity level.</summary>
    public ComplexityLevel Level { get; init; }

    /// <summary>Confidence in assessment.</summary>
    public double Confidence { get; init; }

    /// <summary>Factors contributing to complexity.</summary>
    public IReadOnlyList<ComplexityFactor> Factors { get; init; } = Array.Empty<ComplexityFactor>();

    /// <summary>Recommended handling approach.</summary>
    public required string RecommendedApproach { get; init; }

    /// <summary>Suggested model/agent tier.</summary>
    public required string SuggestedTier { get; init; }

    /// <summary>Whether escalation is recommended.</summary>
    public bool EscalationRecommended { get; init; }

    /// <summary>Reasoning.</summary>
    public string? Reasoning { get; init; }
}

/// <summary>
/// A factor contributing to complexity.
/// </summary>
public class ComplexityFactor
{
    /// <summary>Factor name.</summary>
    public required string Name { get; init; }

    /// <summary>Weight (0.0 - 1.0).</summary>
    public double Weight { get; init; }

    /// <summary>Score for this factor.</summary>
    public double Score { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }
}

/// <summary>
/// Escalation action taken.
/// </summary>
public class EscalationAction
{
    /// <summary>Action identifier.</summary>
    public string ActionId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>From level.</summary>
    public ComplexityLevel FromLevel { get; init; }

    /// <summary>To level.</summary>
    public ComplexityLevel ToLevel { get; init; }

    /// <summary>Reason for escalation.</summary>
    public required string Reason { get; init; }

    /// <summary>Action taken (route to model, human, etc.).</summary>
    public required string ActionTaken { get; init; }

    /// <summary>When escalated.</summary>
    public DateTimeOffset EscalatedAt { get; init; }

    /// <summary>Outcome.</summary>
    public string? Outcome { get; init; }
}

/// <summary>
/// Escalation policy.
/// </summary>
public class EscalationPolicy
{
    /// <summary>Policy identifier.</summary>
    public required string PolicyId { get; init; }

    /// <summary>Level thresholds for escalation.</summary>
    public Dictionary<ComplexityLevel, string> LevelTargets { get; init; } = new();

    /// <summary>Auto-escalation triggers.</summary>
    public IReadOnlyList<string> AutoEscalationTriggers { get; init; } = Array.Empty<string>();

    /// <summary>Whether to require approval for escalation.</summary>
    public bool RequireApproval { get; init; }

    /// <summary>Maximum retries before escalation.</summary>
    public int MaxRetriesBeforeEscalation { get; init; } = 3;
}

/// <summary>
/// Port for progressive complexity escalation.
/// Implements the "Progressive Complexity Escalation" pattern.
/// </summary>
public interface IComplexityEscalationPort
{
    /// <summary>
    /// Assesses the complexity of a task.
    /// </summary>
    Task<ComplexityAssessment> AssessComplexityAsync(
        string taskId,
        string taskDescription,
        Dictionary<string, string>? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Escalates a task to a higher tier.
    /// </summary>
    Task<EscalationAction> EscalateAsync(
        string taskId,
        ComplexityLevel targetLevel,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// De-escalates a task to a lower tier.
    /// </summary>
    Task<EscalationAction> DeescalateAsync(
        string taskId,
        ComplexityLevel targetLevel,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets escalation policy.
    /// </summary>
    Task<EscalationPolicy> GetPolicyAsync(
        string policyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets escalation policy.
    /// </summary>
    Task SetPolicyAsync(
        EscalationPolicy policy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets escalation history for a task.
    /// </summary>
    Task<IReadOnlyList<EscalationAction>> GetHistoryAsync(
        string taskId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suggests appropriate tier for complexity level.
    /// </summary>
    Task<string> SuggestTierAsync(
        ComplexityLevel level,
        CancellationToken cancellationToken = default);
}
