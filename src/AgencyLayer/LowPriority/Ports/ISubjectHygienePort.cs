namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Current delegation mechanisms are sufficient
// Reconsideration: If task delegation issues emerge
// ============================================================================

/// <summary>
/// Task subject.
/// </summary>
public class TaskSubject
{
    /// <summary>Subject identifier.</summary>
    public required string SubjectId { get; init; }

    /// <summary>Subject text.</summary>
    public required string Subject { get; init; }

    /// <summary>Is clean.</summary>
    public bool IsClean { get; init; }

    /// <summary>Issues.</summary>
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for subject hygiene for task delegation.
/// Implements the "Subject Hygiene for Task Delegation" pattern.
///
/// This is a low-priority pattern because current delegation
/// mechanisms are sufficient.
/// </summary>
public interface ISubjectHygienePort
{
    /// <summary>Validates subject hygiene.</summary>
    Task<TaskSubject> ValidateAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>Cleans subject.</summary>
    Task<string> CleanAsync(string subject, CancellationToken cancellationToken = default);

    /// <summary>Suggests improvements.</summary>
    Task<IReadOnlyList<string>> SuggestImprovementsAsync(string subject, CancellationToken cancellationToken = default);
}
