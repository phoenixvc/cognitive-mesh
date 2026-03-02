using AgencyLayer.CognitiveSandwich.Models;

namespace AgencyLayer.CognitiveSandwich.Ports;

/// <summary>
/// Adapter interface for persisting and retrieving audit trail entries
/// for Cognitive Sandwich processes. Implementations may write to CosmosDB,
/// event stores, or other durable storage backends.
/// </summary>
public interface IAuditLoggingAdapter
{
    /// <summary>
    /// Persists an audit trail entry for a Cognitive Sandwich process event.
    /// </summary>
    /// <param name="entry">The audit entry to persist.</param>
    /// <param name="ct">Cancellation token.</param>
    Task LogAuditEntryAsync(PhaseAuditEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all audit trail entries for the specified process, ordered chronologically.
    /// </summary>
    /// <param name="processId">The unique identifier of the process.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of audit entries.</returns>
    Task<IReadOnlyList<PhaseAuditEntry>> GetAuditEntriesAsync(string processId, CancellationToken ct = default);
}
