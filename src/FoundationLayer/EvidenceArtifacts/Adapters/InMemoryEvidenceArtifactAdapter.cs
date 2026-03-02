using System.Collections.Concurrent;
using CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;
using CognitiveMesh.FoundationLayer.EvidenceArtifacts.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IEvidenceArtifactRepositoryPort"/> for development,
/// testing, and scenarios where persistent storage is not required.
/// Uses a <see cref="ConcurrentDictionary{TKey, TValue}"/> for thread-safe operations.
/// </summary>
public class InMemoryEvidenceArtifactAdapter : IEvidenceArtifactRepositoryPort
{
    private readonly ConcurrentDictionary<Guid, EvidenceArtifact> _store = new();
    private readonly ILogger<InMemoryEvidenceArtifactAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryEvidenceArtifactAdapter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public InMemoryEvidenceArtifactAdapter(ILogger<InMemoryEvidenceArtifactAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<EvidenceArtifact> StoreArtifactAsync(EvidenceArtifact artifact, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        if (artifact.ArtifactId == Guid.Empty)
        {
            artifact.ArtifactId = Guid.NewGuid();
        }

        if (artifact.SubmittedAt == default)
        {
            artifact.SubmittedAt = DateTimeOffset.UtcNow;
        }

        _store[artifact.ArtifactId] = artifact;

        _logger.LogInformation(
            "Stored evidence artifact {ArtifactId} of source type '{SourceType}' with correlation {CorrelationId}",
            artifact.ArtifactId, artifact.SourceType, artifact.CorrelationId);

        return Task.FromResult(artifact);
    }

    /// <inheritdoc />
    public Task<EvidenceArtifact?> GetArtifactAsync(Guid artifactId, CancellationToken cancellationToken)
    {
        _store.TryGetValue(artifactId, out var artifact);

        _logger.LogDebug(
            "GetArtifactAsync for {ArtifactId}: {Found}",
            artifactId, artifact is not null ? "found" : "not found");

        return Task.FromResult(artifact);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EvidenceArtifact>> SearchAsync(ArtifactSearchCriteria criteria, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(criteria);

        IEnumerable<EvidenceArtifact> query = _store.Values;

        if (!string.IsNullOrEmpty(criteria.SourceType))
        {
            query = query.Where(a => string.Equals(a.SourceType, criteria.SourceType, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(criteria.CorrelationId))
        {
            query = query.Where(a => string.Equals(a.CorrelationId, criteria.CorrelationId, StringComparison.OrdinalIgnoreCase));
        }

        if (criteria.Tags is { Count: > 0 })
        {
            query = query.Where(a => criteria.Tags.All(tag =>
                a.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)));
        }

        if (criteria.SubmittedAfter.HasValue)
        {
            query = query.Where(a => a.SubmittedAt >= criteria.SubmittedAfter.Value);
        }

        var results = query.Take(criteria.MaxResults).ToList();

        _logger.LogDebug("SearchAsync returned {Count} results (max {MaxResults})", results.Count, criteria.MaxResults);

        return Task.FromResult<IReadOnlyList<EvidenceArtifact>>(results);
    }

    /// <inheritdoc />
    public Task<int> DeleteExpiredAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var expiredIds = _store.Values
            .Where(a => a.ExpiresAt.HasValue && a.ExpiresAt.Value <= now)
            .Select(a => a.ArtifactId)
            .ToList();

        var deletedCount = 0;
        foreach (var id in expiredIds)
        {
            if (_store.TryRemove(id, out _))
            {
                deletedCount++;
            }
        }

        _logger.LogInformation(
            "DeleteExpiredAsync: removed {DeletedCount} expired artifacts out of {ExpiredCount} identified",
            deletedCount, expiredIds.Count);

        return Task.FromResult(deletedCount);
    }

    /// <inheritdoc />
    public Task<int> GetArtifactCountAsync(CancellationToken cancellationToken)
    {
        var count = _store.Count;
        _logger.LogDebug("GetArtifactCountAsync: {Count} artifacts in store", count);
        return Task.FromResult(count);
    }
}
