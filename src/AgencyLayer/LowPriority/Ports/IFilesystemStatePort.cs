namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - Niche/Specialized Pattern
// Reason: Redis+DuckDB is superior for production state management
// Reconsideration: Not recommended; use HybridMemoryStore instead
// Related Antipattern: Filesystem-Based Agent State (Low Risk)
// ============================================================================

/// <summary>
/// Filesystem state entry.
/// </summary>
public class FilesystemStateEntry
{
    /// <summary>Key.</summary>
    public required string Key { get; init; }

    /// <summary>Value.</summary>
    public required string Value { get; init; }

    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Modified at.</summary>
    public DateTimeOffset ModifiedAt { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for filesystem-based agent state.
/// Implements the "Filesystem-Based Agent State" pattern.
///
/// This is a low-priority pattern because Redis+DuckDB hybrid
/// is superior for production state management.
/// </summary>
public interface IFilesystemStatePort
{
    /// <summary>Saves state to file.</summary>
    Task SaveAsync(string key, string value, CancellationToken cancellationToken = default);

    /// <summary>Loads state from file.</summary>
    Task<string?> LoadAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Deletes state file.</summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>Lists state files.</summary>
    Task<IReadOnlyList<FilesystemStateEntry>> ListAsync(string? prefix = null, CancellationToken cancellationToken = default);
}
