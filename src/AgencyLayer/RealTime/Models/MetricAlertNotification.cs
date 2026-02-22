namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// DTO representing a metric threshold violation alert, broadcast to subscribed clients.
/// </summary>
/// <param name="AlertId">The unique identifier of the alert.</param>
/// <param name="MetricName">The name of the metric that violated its threshold.</param>
/// <param name="CurrentValue">The current value of the metric that triggered the alert.</param>
/// <param name="ThresholdValue">The threshold value that was exceeded.</param>
/// <param name="Severity">The severity level of the alert (e.g., "Warning", "Critical").</param>
/// <param name="Timestamp">The timestamp when the alert was generated.</param>
/// <param name="Details">Optional human-readable details about the alert.</param>
public record MetricAlertNotification(
    string AlertId,
    string MetricName,
    double CurrentValue,
    double ThresholdValue,
    string Severity,
    DateTimeOffset Timestamp,
    string? Details = null);
