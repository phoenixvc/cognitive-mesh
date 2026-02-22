namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// Well-known event type constants used across the Cognitive Mesh real-time notification system.
/// </summary>
public static class RealTimeEventTypes
{
    /// <summary>
    /// Raised when an agent's operational status changes (e.g., idle to active, healthy to degraded).
    /// </summary>
    public const string AgentStatusChanged = "AgentStatusChanged";

    /// <summary>
    /// Raised when a single step within a durable workflow completes.
    /// </summary>
    public const string WorkflowStepCompleted = "WorkflowStepCompleted";

    /// <summary>
    /// Raised when an entire workflow finishes execution.
    /// </summary>
    public const string WorkflowCompleted = "WorkflowCompleted";

    /// <summary>
    /// Raised when a monitored metric exceeds its configured threshold.
    /// </summary>
    public const string MetricThresholdViolation = "MetricThresholdViolation";

    /// <summary>
    /// Raised when a general notification is received for a user or group.
    /// </summary>
    public const string NotificationReceived = "NotificationReceived";

    /// <summary>
    /// Raised when dashboard data has been refreshed and clients should update their displays.
    /// </summary>
    public const string DashboardDataUpdated = "DashboardDataUpdated";

    /// <summary>
    /// Raised to report incremental progress during a reasoning operation.
    /// </summary>
    public const string ReasoningProgressUpdate = "ReasoningProgressUpdate";
}
