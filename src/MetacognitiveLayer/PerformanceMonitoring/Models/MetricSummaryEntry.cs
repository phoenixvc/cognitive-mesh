namespace MetacognitiveLayer.PerformanceMonitoring.Models;

/// <summary>
/// Represents a summary entry for a single metric, containing aggregated statistics
/// suitable for display on a performance dashboard.
/// </summary>
/// <param name="MetricName">The name of the metric.</param>
/// <param name="LastValue">The most recently recorded value for this metric.</param>
/// <param name="Average">The average value across all recorded samples.</param>
/// <param name="Min">The minimum recorded value.</param>
/// <param name="Max">The maximum recorded value.</param>
/// <param name="SampleCount">The total number of samples recorded for this metric.</param>
/// <param name="LastRecordedAt">The timestamp of the most recently recorded sample.</param>
public record MetricSummaryEntry(
    string MetricName,
    double LastValue,
    double Average,
    double Min,
    double Max,
    long SampleCount,
    DateTimeOffset LastRecordedAt);
