using System.Collections.Concurrent;
using CognitiveMesh.FoundationLayer.NISTEvidence.Models;
using CognitiveMesh.FoundationLayer.NISTEvidence.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.NISTEvidence.Adapters;

/// <summary>
/// In-memory implementation of <see cref="INISTEvidenceRepositoryPort"/> for development,
/// testing, and scenarios where persistent storage is not required.
/// Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> for thread-safe operations.
/// </summary>
public class InMemoryNISTEvidenceAdapter : INISTEvidenceRepositoryPort
{
    private readonly ConcurrentDictionary<Guid, NISTEvidenceRecord> _store = new();
    private readonly ILogger<InMemoryNISTEvidenceAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryNISTEvidenceAdapter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public InMemoryNISTEvidenceAdapter(ILogger<InMemoryNISTEvidenceAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<NISTEvidenceRecord> StoreAsync(NISTEvidenceRecord record, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (record.EvidenceId == Guid.Empty)
        {
            record.EvidenceId = Guid.NewGuid();
        }

        if (record.SubmittedAt == default)
        {
            record.SubmittedAt = DateTimeOffset.UtcNow;
        }

        record.AuditTrail.Add(new EvidenceAuditEntry(
            EntryId: Guid.NewGuid(),
            Action: "Created",
            PerformedBy: record.SubmittedBy,
            PerformedAt: DateTimeOffset.UtcNow,
            Details: $"Evidence record created with artifact type '{record.ArtifactType}'."));

        _store[record.EvidenceId] = record;

        _logger.LogInformation(
            "Stored NIST evidence record {EvidenceId} for statement {StatementId} in pillar {PillarId}",
            record.EvidenceId, record.StatementId, record.PillarId);

        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task<NISTEvidenceRecord?> GetByIdAsync(Guid evidenceId, CancellationToken cancellationToken)
    {
        _store.TryGetValue(evidenceId, out var record);

        _logger.LogDebug(
            "GetByIdAsync for {EvidenceId}: {Found}",
            evidenceId, record is not null ? "found" : "not found");

        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NISTEvidenceRecord>> GetByStatementAsync(string statementId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(statementId);

        var results = _store.Values
            .Where(r => string.Equals(r.StatementId, statementId, StringComparison.OrdinalIgnoreCase))
            .ToList();

        _logger.LogDebug(
            "GetByStatementAsync for {StatementId}: found {Count} records",
            statementId, results.Count);

        return Task.FromResult<IReadOnlyList<NISTEvidenceRecord>>(results);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<NISTEvidenceRecord>> QueryAsync(EvidenceQueryFilter filter, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(filter);

        IEnumerable<NISTEvidenceRecord> query = _store.Values;

        if (!string.IsNullOrEmpty(filter.StatementId))
        {
            query = query.Where(r => string.Equals(r.StatementId, filter.StatementId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(filter.PillarId))
        {
            query = query.Where(r => string.Equals(r.PillarId, filter.PillarId, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(filter.TopicId))
        {
            query = query.Where(r => string.Equals(r.TopicId, filter.TopicId, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.ReviewStatus.HasValue)
        {
            query = query.Where(r => r.ReviewStatus == filter.ReviewStatus.Value);
        }

        if (filter.SubmittedAfter.HasValue)
        {
            query = query.Where(r => r.SubmittedAt >= filter.SubmittedAfter.Value);
        }

        if (filter.SubmittedBefore.HasValue)
        {
            query = query.Where(r => r.SubmittedAt <= filter.SubmittedBefore.Value);
        }

        var results = query.Take(filter.MaxResults).ToList();

        _logger.LogDebug("QueryAsync returned {Count} results (max {MaxResults})", results.Count, filter.MaxResults);

        return Task.FromResult<IReadOnlyList<NISTEvidenceRecord>>(results);
    }

    /// <inheritdoc />
    public Task<NISTEvidenceRecord?> UpdateReviewStatusAsync(
        Guid evidenceId,
        EvidenceReviewStatus status,
        string reviewerId,
        string? notes,
        CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(evidenceId, out var record))
        {
            _logger.LogWarning("UpdateReviewStatusAsync: evidence record {EvidenceId} not found", evidenceId);
            return Task.FromResult<NISTEvidenceRecord?>(null);
        }

        var previousStatus = record.ReviewStatus;
        record.ReviewStatus = status;
        record.ReviewerId = reviewerId;
        record.ReviewerNotes = notes;

        record.AuditTrail.Add(new EvidenceAuditEntry(
            EntryId: Guid.NewGuid(),
            Action: "StatusChanged",
            PerformedBy: reviewerId,
            PerformedAt: DateTimeOffset.UtcNow,
            Details: $"Review status changed from '{previousStatus}' to '{status}'."));

        _logger.LogInformation(
            "Updated review status for {EvidenceId} from {PreviousStatus} to {NewStatus} by {ReviewerId}",
            evidenceId, previousStatus, status, reviewerId);

        return Task.FromResult<NISTEvidenceRecord?>(record);
    }

    /// <inheritdoc />
    public Task<bool> ArchiveAsync(Guid evidenceId, CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(evidenceId, out var record))
        {
            _logger.LogWarning("ArchiveAsync: evidence record {EvidenceId} not found", evidenceId);
            return Task.FromResult(false);
        }

        record.IsArchived = true;

        record.AuditTrail.Add(new EvidenceAuditEntry(
            EntryId: Guid.NewGuid(),
            Action: "Archived",
            PerformedBy: "system",
            PerformedAt: DateTimeOffset.UtcNow,
            Details: "Evidence record archived."));

        _logger.LogInformation("Archived evidence record {EvidenceId}", evidenceId);

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<EvidenceStatistics> GetStatisticsAsync(CancellationToken cancellationToken)
    {
        var records = _store.Values.ToList();
        var totalRecords = records.Count;

        var pendingReviews = records.Count(r => r.ReviewStatus == EvidenceReviewStatus.Pending);
        var approvedCount = records.Count(r => r.ReviewStatus == EvidenceReviewStatus.Approved);
        var rejectedCount = records.Count(r => r.ReviewStatus == EvidenceReviewStatus.Rejected);
        var averageFileSizeBytes = totalRecords > 0
            ? (long)records.Average(r => r.FileSizeBytes)
            : 0L;

        var byPillar = records
            .GroupBy(r => r.PillarId)
            .ToDictionary(g => g.Key, g => g.Count());

        var statistics = new EvidenceStatistics(
            TotalRecords: totalRecords,
            PendingReviews: pendingReviews,
            ApprovedCount: approvedCount,
            RejectedCount: rejectedCount,
            AverageFileSizeBytes: averageFileSizeBytes,
            ByPillar: byPillar);

        _logger.LogDebug(
            "GetStatisticsAsync: Total={Total}, Pending={Pending}, Approved={Approved}, Rejected={Rejected}",
            totalRecords, pendingReviews, approvedCount, rejectedCount);

        return Task.FromResult(statistics);
    }
}
