using MetacognitiveLayer.PerformanceMonitoring;
using MetacognitiveLayer.PerformanceMonitoring.Models;
using MetacognitiveLayer.PerformanceMonitoring.Ports;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.PerformanceMonitoring.Adapters;

/// <summary>
/// Adapter implementation of <see cref="IPerformanceMonitoringPort"/> that delegates to the
/// underlying <see cref="PerformanceMonitor"/> engine and <see cref="IMetricsStore"/> for
/// metric recording, threshold checking, history retrieval, and dashboard generation.
/// </summary>
public class PerformanceMonitoringAdapter : IPerformanceMonitoringPort
{
    private readonly PerformanceMonitor _monitor;
    private readonly IMetricsStore _store;
    private readonly ILogger<PerformanceMonitoringAdapter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMonitoringAdapter"/> class.
    /// </summary>
    /// <param name="monitor">The performance monitor engine that handles metric aggregation and threshold evaluation.</param>
    /// <param name="store">The metrics store for persisting and querying raw metric data.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    public PerformanceMonitoringAdapter(
        PerformanceMonitor monitor,
        IMetricsStore store,
        ILogger<PerformanceMonitoringAdapter> logger)
    {
        _monitor = monitor ?? throw new ArgumentNullException(nameof(monitor));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task RecordMetricAsync(string name, double value, IDictionary<string, string>? tags, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogDebug("Recording metric {MetricName} with value {MetricValue}", name, value);
        _monitor.RecordMetric(name, value, tags);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<MetricStatistics?> GetAggregatedStatsAsync(string metricName, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var stats = _monitor.GetAggregatedStats(metricName, TimeSpan.FromMinutes(5));
        return Task.FromResult<MetricStatistics?>(stats);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ThresholdViolation>> CheckThresholdsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var violations = await _monitor.CheckThresholdsAsync().ConfigureAwait(false);
        var result = violations.ToList().AsReadOnly();

        if (result.Count > 0)
        {
            _logger.LogWarning("Detected {ViolationCount} threshold violation(s)", result.Count);
        }

        return result;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<Metric>> GetMetricHistoryAsync(
        string metricName,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (_store is InMemoryMetricsStoreAdapter inMemoryStore)
        {
            var metrics = inMemoryStore.GetMetrics(metricName, from.UtcDateTime, to.UtcDateTime);
            return Task.FromResult(metrics);
        }

        // Fall back to QueryMetricsAsync for other store implementations
        return GetMetricHistoryViaQueryAsync(metricName, from, to);
    }

    /// <inheritdoc />
    public async Task<PerformanceDashboardSummary> GetDashboardSummaryAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogDebug("Generating performance dashboard summary");

        var violations = await CheckThresholdsAsync(ct).ConfigureAwait(false);
        var health = DetermineSystemHealth(violations.Count);

        var topMetrics = new List<MetricSummaryEntry>();
        long totalMetrics = 0;
        int activeThresholds = 0;

        if (_store is InMemoryMetricsStoreAdapter inMemoryStore)
        {
            var metricNames = inMemoryStore.GetAllMetricNames();
            totalMetrics = inMemoryStore.GetTotalMetricCount();

            foreach (var name in metricNames)
            {
                var stats = _monitor.GetAggregatedStats(name, TimeSpan.FromMinutes(5));
                if (stats == null)
                {
                    continue;
                }

                var allMetrics = inMemoryStore.GetMetrics(name, DateTime.MinValue, DateTime.MaxValue);
                var lastMetric = allMetrics.Count > 0 ? allMetrics[^1] : null;

                topMetrics.Add(new MetricSummaryEntry(
                    MetricName: name,
                    LastValue: lastMetric?.Value ?? 0,
                    Average: stats.Average,
                    Min: stats.Min,
                    Max: stats.Max,
                    SampleCount: stats.Count,
                    LastRecordedAt: lastMetric != null
                        ? new DateTimeOffset(lastMetric.Timestamp, TimeSpan.Zero)
                        : DateTimeOffset.UtcNow));
            }

            // Sort by sample count descending to show most active metrics first
            topMetrics = topMetrics
                .OrderByDescending(m => m.SampleCount)
                .ToList();
        }

        var summary = new PerformanceDashboardSummary(
            GeneratedAt: DateTimeOffset.UtcNow,
            TotalMetricsRecorded: totalMetrics,
            ActiveThresholds: activeThresholds,
            CurrentViolations: violations,
            TopMetrics: topMetrics.AsReadOnly(),
            SystemHealth: health);

        _logger.LogInformation(
            "Dashboard summary generated: {TotalMetrics} metrics, {ViolationCount} violations, health={Health}",
            totalMetrics, violations.Count, health);

        return summary;
    }

    /// <inheritdoc />
    public Task RegisterAlertAsync(string metricName, MetricThreshold threshold, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Registering alert for metric {MetricName} with condition {Condition} {Value}",
            metricName, threshold.Condition, threshold.Value);

        _monitor.RegisterThreshold(metricName, threshold);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Determines the system health status based on the number of active violations.
    /// </summary>
    /// <param name="violationCount">The number of current threshold violations.</param>
    /// <returns>The corresponding <see cref="SystemHealthStatus"/>.</returns>
    internal static SystemHealthStatus DetermineSystemHealth(int violationCount)
    {
        return violationCount switch
        {
            0 => SystemHealthStatus.Healthy,
            1 or 2 => SystemHealthStatus.Degraded,
            _ => SystemHealthStatus.Critical
        };
    }

    private async Task<IReadOnlyList<Metric>> GetMetricHistoryViaQueryAsync(
        string metricName,
        DateTimeOffset from,
        DateTimeOffset to)
    {
        var results = await _store.QueryMetricsAsync(
            metricName,
            from.UtcDateTime,
            to.UtcDateTime).ConfigureAwait(false);

        return results.ToList().AsReadOnly();
    }
}
