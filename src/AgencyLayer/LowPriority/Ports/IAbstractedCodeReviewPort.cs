namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: No code review UI in current architecture
// Reconsideration: If end-user developer tooling is built
// ============================================================================

/// <summary>
/// Abstracted code representation.
/// </summary>
public class AbstractedCode
{
    /// <summary>Original file.</summary>
    public required string FilePath { get; init; }

    /// <summary>Abstracted view.</summary>
    public required string AbstractedView { get; init; }

    /// <summary>Abstraction level.</summary>
    public required string Level { get; init; }

    /// <summary>Key elements.</summary>
    public IReadOnlyList<string> KeyElements { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for abstracted code representation for review.
/// Implements the "Abstracted Code Representation for Review" pattern.
///
/// This is a low-priority pattern because no code review UI
/// exists in the current architecture.
/// </summary>
public interface IAbstractedCodeReviewPort
{
    /// <summary>Creates abstracted view.</summary>
    Task<AbstractedCode> AbstractAsync(string code, string level, CancellationToken cancellationToken = default);

    /// <summary>Expands abstracted element.</summary>
    Task<string> ExpandAsync(string abstractedView, string element, CancellationToken cancellationToken = default);

    /// <summary>Gets abstraction levels.</summary>
    Task<IReadOnlyList<string>> GetLevelsAsync(CancellationToken cancellationToken = default);
}
