using System.Collections.Concurrent;
using AgencyLayer.CognitiveSandwich.Models;
using AgencyLayer.CognitiveSandwich.Ports;
using Microsoft.Extensions.Logging;

namespace AgencyLayer.CognitiveSandwich.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IAuditLoggingAdapter"/> using a
/// <see cref="ConcurrentBag{T}"/> for development and testing scenarios.
/// Stores audit trail entries in memory with thread-safe access.
/// </summary>
public class InMemoryAuditLoggingAdapter : IAuditLoggingAdapter
{
    private readonly ConcurrentBag<PhaseAuditEntry> _entries = new();
    private readonly ILogger<InMemoryAuditLoggingAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryAuditLoggingAdapter"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InMemoryAuditLoggingAdapter(ILogger<InMemoryAuditLoggingAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task LogAuditEntryAsync(PhaseAuditEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        _entries.Add(entry);

        _logger.LogDebug(
            "Logged audit entry {EntryId} for process {ProcessId}: {EventType}",
            entry.EntryId, entry.ProcessId, entry.EventType);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<PhaseAuditEntry>> GetAuditEntriesAsync(string processId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(processId);

        var entries = _entries
            .Where(e => e.ProcessId == processId)
            .OrderBy(e => e.Timestamp)
            .ToList();

        _logger.LogDebug(
            "Retrieved {Count} audit entries for process {ProcessId}",
            entries.Count, processId);

        return Task.FromResult<IReadOnlyList<PhaseAuditEntry>>(entries);
    }
}
