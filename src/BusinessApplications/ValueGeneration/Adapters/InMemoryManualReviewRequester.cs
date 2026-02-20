using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.ValueGeneration.Engines;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.ValueGeneration.Adapters;

/// <summary>
/// In-memory implementation of <see cref="IManualReviewRequester"/>
/// for development, testing, and local environments.  Review requests
/// are stored in a <see cref="ConcurrentDictionary{TKey,TValue}"/> for
/// later inspection.
/// </summary>
public class InMemoryManualReviewRequester : IManualReviewRequester
{
    private readonly ILogger<InMemoryManualReviewRequester> _logger;
    private readonly ConcurrentDictionary<string, Dictionary<string, object>> _reviews = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryManualReviewRequester"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public InMemoryManualReviewRequester(ILogger<InMemoryManualReviewRequester> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task RequestManualReviewAsync(string userId, string tenantId, string reviewType, Dictionary<string, object> context)
    {
        var reviewId = Guid.NewGuid().ToString();
        _reviews[reviewId] = new Dictionary<string, object>
        {
            { "userId", userId },
            { "tenantId", tenantId },
            { "reviewType", reviewType },
            { "context", context },
            { "requestedAt", DateTimeOffset.UtcNow }
        };

        _logger.LogInformation(
            "Manual review requested for user '{UserId}', type '{ReviewType}', reviewId '{ReviewId}'.",
            userId, reviewType, reviewId);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all submitted review requests. Useful for testing and diagnostics.
    /// </summary>
    /// <returns>A read-only dictionary of review requests keyed by review ID.</returns>
    public IReadOnlyDictionary<string, Dictionary<string, object>> GetAllReviews()
    {
        return _reviews;
    }
}
