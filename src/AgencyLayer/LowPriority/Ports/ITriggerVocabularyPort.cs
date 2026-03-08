namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - UX Pattern
// Reason: Current trigger mechanisms are sufficient
// Reconsideration: If proactive agent triggers become important
// ============================================================================

/// <summary>
/// Trigger vocabulary entry.
/// </summary>
public class TriggerEntry
{
    /// <summary>Trigger phrase.</summary>
    public required string Phrase { get; init; }

    /// <summary>Action to trigger.</summary>
    public required string Action { get; init; }

    /// <summary>Context required.</summary>
    public IReadOnlyList<string> Context { get; init; } = Array.Empty<string>();
}

/// <summary>
/// [LOW PRIORITY] Port for proactive trigger vocabulary.
/// Implements the "Proactive Trigger Vocabulary" pattern.
///
/// This is a low-priority pattern because current trigger
/// mechanisms are sufficient.
/// </summary>
public interface ITriggerVocabularyPort
{
    /// <summary>Matches triggers in text.</summary>
    Task<IReadOnlyList<TriggerEntry>> MatchTriggersAsync(string text, CancellationToken cancellationToken = default);

    /// <summary>Registers trigger.</summary>
    Task RegisterTriggerAsync(TriggerEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Lists triggers.</summary>
    Task<IReadOnlyList<TriggerEntry>> ListTriggersAsync(CancellationToken cancellationToken = default);
}
