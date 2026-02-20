using MetacognitiveLayer.PerformanceMonitoring;

namespace MetacognitiveLayer.PerformanceMonitoring.Ports;

/// <summary>
/// Defines the contract for the Performance Monitoring Port in the Metacognitive Layer.
/// This port is the primary entry point for recording metrics, checking thresholds,
/// querying metric history, and generating dashboard summaries. Follows the Hexagonal
/// Architecture pattern.
/// </summary>
public interface IPerformanceMonitoringPort
{
    /// <summary>
    /// Records a metric value with the given name and optional tags.
    /// </summary>
    /// <param name="name">The name of the metric to record.</param>
    /// <param name="value">The numeric value of the metric.</param>
    /// <param name="tags">Optional tags to associate with the metric for filtering.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RecordMetricAsync(string name, double value, IDictionary<string, string>? tags, CancellationToken ct);

    /// <summary>
    /// Gets aggregated statistics for a metric over the default evaluation window.
    /// </summary>
    /// <param name="metricName">The name of the metric to aggregate.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// aggregated <see cref="MetricStatistics"/>, or <c>null</c> if no data is available.
    /// </returns>
    Task<MetricStatistics?> GetAggregatedStatsAsync(string metricName, CancellationToken ct);

    /// <summary>
    /// Checks all registered thresholds against the current aggregated metric values
    /// and returns any violations found.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// read-only list of <see cref="ThresholdViolation"/> instances.
    /// </returns>
    Task<IReadOnlyList<ThresholdViolation>> CheckThresholdsAsync(CancellationToken ct);

    /// <summary>
    /// Queries raw metric history for a specific metric within a time range.
    /// </summary>
    /// <param name="metricName">The name of the metric to query.</param>
    /// <param name="from">The start of the query window (inclusive).</param>
    /// <param name="to">The end of the query window (inclusive).</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a
    /// read-only list of <see cref="Metric"/> data points within the specified range.
    /// </returns>
    Task<IReadOnlyList<Metric>> GetMetricHistoryAsync(string metricName, DateTimeOffset from, DateTimeOffset to, CancellationToken ct);

    /// <summary>
    /// Generates a comprehensive dashboard summary including top metrics,
    /// current violations, and overall system health status.
    /// </summary>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the
    /// <see cref="Models.PerformanceDashboardSummary"/> with current system state.
    /// </returns>
    Task<Models.PerformanceDashboardSummary> GetDashboardSummaryAsync(CancellationToken ct);

    /// <summary>
    /// Registers an alert threshold for a specific metric. When the metric violates this
    /// threshold, subsequent calls to <see cref="CheckThresholdsAsync"/> will report the violation.
    /// </summary>
    /// <param name="metricName">The name of the metric to monitor.</param>
    /// <param name="threshold">The threshold configuration defining the alert condition.</param>
    /// <param name="ct">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task RegisterAlertAsync(string metricName, MetricThreshold threshold, CancellationToken ct);
}
