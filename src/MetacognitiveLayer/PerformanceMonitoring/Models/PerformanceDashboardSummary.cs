using MetacognitiveLayer.PerformanceMonitoring;

namespace MetacognitiveLayer.PerformanceMonitoring.Models;

/// <summary>
/// Represents a comprehensive dashboard summary of the performance monitoring system,
/// including metric counts, violations, top metrics, and overall system health.
/// </summary>
/// <param name="GeneratedAt">The timestamp when this summary was generated.</param>
/// <param name="TotalMetricsRecorded">The total number of individual metric data points recorded.</param>
/// <param name="ActiveThresholds">The number of currently registered alert thresholds.</param>
/// <param name="CurrentViolations">The list of current threshold violations.</param>
/// <param name="TopMetrics">Summary entries for the most active metrics.</param>
/// <param name="SystemHealth">The overall health status of the system.</param>
public record PerformanceDashboardSummary(
    DateTimeOffset GeneratedAt,
    long TotalMetricsRecorded,
    int ActiveThresholds,
    IReadOnlyList<ThresholdViolation> CurrentViolations,
    IReadOnlyList<MetricSummaryEntry> TopMetrics,
    SystemHealthStatus SystemHealth);

/// <summary>
/// Represents the overall health status of the monitored system, derived from
/// the number of active threshold violations.
/// </summary>
public enum SystemHealthStatus
{
    /// <summary>No threshold violations detected. All metrics are within acceptable ranges.</summary>
    Healthy,

    /// <summary>A small number of threshold violations detected (1-2). Investigation recommended.</summary>
    Degraded,

    /// <summary>Multiple threshold violations detected (3 or more). Immediate attention required.</summary>
    Critical
}
