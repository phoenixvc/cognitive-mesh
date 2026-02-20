namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Tracks the authorship provenance for a task, recording who authored what
/// (human, agent, or hybrid) with content hashes for auditability.
/// </summary>
public class AuthorshipTrail
{
    /// <summary>
    /// Unique identifier for this authorship trail.
    /// </summary>
    public string TrailId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the task this trail documents.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of authorship entries recording each contribution to the task.
    /// </summary>
    public List<AuthorshipEntry> Entries { get; set; } = [];
}

/// <summary>
/// Represents a single authorship contribution within an <see cref="AuthorshipTrail"/>.
/// </summary>
public class AuthorshipEntry
{
    /// <summary>
    /// Unique identifier for this entry.
    /// </summary>
    public string EntryId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of author that produced this contribution.
    /// </summary>
    public AuthorType AuthorType { get; set; }

    /// <summary>
    /// Identifier of the specific author (user ID or agent ID).
    /// </summary>
    public string AuthorId { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 hash of the content for integrity verification and deduplication.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when this contribution was recorded.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Identifies the type of author for an authorship trail entry.
/// </summary>
public enum AuthorType
{
    /// <summary>
    /// Content authored entirely by a human.
    /// </summary>
    Human,

    /// <summary>
    /// Content authored entirely by an AI agent.
    /// </summary>
    Agent,

    /// <summary>
    /// Content co-authored by both human and agent.
    /// </summary>
    Hybrid
}
