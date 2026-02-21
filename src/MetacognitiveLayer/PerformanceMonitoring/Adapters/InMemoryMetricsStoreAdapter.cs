using System.Collections.Concurrent;
using MetacognitiveLayer.PerformanceMonitoring;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.PerformanceMonitoring.Adapters;

/// <summary>
/// An in-memory implementation of <see cref="IMetricsStore"/> that stores metrics
/// in a <see cref="ConcurrentDictionary{TKey, TValue}"/> of <see cref="ConcurrentQueue{T}"/>.
/// Each metric name maintains up to <see cref="MaxEntriesPerMetric"/> data points,
/// with older entries trimmed automatically on write.
/// </summary>
/// <remarks>
/// This adapter is suitable for development, testing, and single-process deployments.
/// For production scenarios requiring persistence across restarts or distributed access,
/// use a durable metrics store implementation instead.
/// </remarks>
public class InMemoryMetricsStoreAdapter : IMetricsStore
{
    /// <summary>
    /// The maximum number of metric data points retained per metric name.
    /// </summary>
    public const int MaxEntriesPerMetric = 10_000;

    private readonly ConcurrentDictionary<string, ConcurrentQueue<Metric>> _store = new();
    private readonly ConcurrentDictionary<string, int> _counts = new();
    private readonly ILogger<InMemoryMetricsStoreAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryMetricsStoreAdapter"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    public InMemoryMetricsStoreAdapter(ILogger<InMemoryMetricsStoreAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Stores a single metric data point. If the metric name already has
    /// <see cref="MaxEntriesPerMetric"/> entries, the oldest entry is dequeued.
    /// </summary>
    /// <param name="metric">The metric to store.</param>
    public void StoreMetric(Metric metric)
    {
        ArgumentNullException.ThrowIfNull(metric);

        var queue = _store.GetOrAdd(metric.Name, _ => new ConcurrentQueue<Metric>());
        queue.Enqueue(metric);

        var count = _counts.AddOrUpdate(metric.Name, 1, (_, c) => c + 1);

        // Trim oldest entries if we exceed the limit
        while (count > MaxEntriesPerMetric && queue.TryDequeue(out _))
        {
            count = _counts.AddOrUpdate(metric.Name, 0, (_, c) => Math.Max(0, c - 1));
        }

        _logger.LogDebug("Stored metric {MetricName} with value {MetricValue}", metric.Name, metric.Value);
    }

    /// <summary>
    /// Queries metrics matching the specified name and time range, with optional tag filtering.
    /// </summary>
    /// <param name="name">The metric name to query.</param>
    /// <param name="since">The start of the query window.</param>
    /// <param name="until">The end of the query window. If null, uses current time.</param>
    /// <param name="tags">Optional tags to filter by. If provided, only metrics containing all specified tags are returned.</param>
    /// <returns>A collection of matching metrics.</returns>
    public Task<IEnumerable<Metric>> QueryMetricsAsync(
        string name,
        DateTime since,
        DateTime? until = null,
        IDictionary<string, string>? tags = null)
    {
        var end = until ?? DateTime.UtcNow;

        if (!_store.TryGetValue(name, out var queue))
        {
            return Task.FromResult<IEnumerable<Metric>>(Array.Empty<Metric>());
        }

        IEnumerable<Metric> results = queue
            .Where(m => m.Timestamp >= since && m.Timestamp <= end);

        if (tags is { Count: > 0 })
        {
            results = results.Where(m =>
                m.Tags != null && tags.All(t =>
                    m.Tags.TryGetValue(t.Key, out var value) && value == t.Value));
        }

        return Task.FromResult(results);
    }

    /// <summary>
    /// Gets metrics for the specified name filtered by a <see cref="DateTime"/> range.
    /// </summary>
    /// <param name="name">The metric name to retrieve.</param>
    /// <param name="from">The start of the time range (inclusive).</param>
    /// <param name="to">The end of the time range (inclusive).</param>
    /// <returns>A list of metrics within the specified time range.</returns>
    public IReadOnlyList<Metric> GetMetrics(string name, DateTime from, DateTime to)
    {
        if (!_store.TryGetValue(name, out var queue))
        {
            return Array.Empty<Metric>();
        }

        return queue
            .Where(m => m.Timestamp >= from && m.Timestamp <= to)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets all metric names that have been recorded.
    /// </summary>
    /// <returns>A read-only list of all tracked metric names.</returns>
    public IReadOnlyList<string> GetAllMetricNames()
    {
        return _store.Keys.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the total number of metric data points stored across all metric names.
    /// </summary>
    /// <returns>The total count of stored metrics.</returns>
    public long GetTotalMetricCount()
    {
        return _store.Values.Sum(q => (long)q.Count);
    }

    /// <summary>
    /// Gets the number of metric data points stored for a specific metric name.
    /// </summary>
    /// <param name="name">The metric name to query.</param>
    /// <returns>The count of stored metrics for the given name, or 0 if not found.</returns>
    public int GetMetricCount(string name)
    {
        return _store.TryGetValue(name, out var queue) ? queue.Count : 0;
    }
}
