using CognitiveMesh.AgencyLayer.RealTime.Models;

namespace CognitiveMesh.AgencyLayer.RealTime.Hubs;

/// <summary>
/// Defines the strongly-typed client interface for the Cognitive Mesh SignalR hub.
/// Methods on this interface correspond to client-side handlers that receive real-time notifications.
/// </summary>
public interface ICognitiveMeshHubClient
{
    /// <summary>
    /// Receives an agent status change notification.
    /// </summary>
    /// <param name="update">The agent status update details.</param>
    /// <returns>A task that completes when the client has acknowledged the message.</returns>
    Task ReceiveAgentStatus(AgentStatusUpdate update);

    /// <summary>
    /// Receives a workflow progress notification.
    /// </summary>
    /// <param name="update">The workflow progress update details.</param>
    /// <returns>A task that completes when the client has acknowledged the message.</returns>
    Task ReceiveWorkflowProgress(WorkflowProgressUpdate update);

    /// <summary>
    /// Receives a metric threshold violation alert.
    /// </summary>
    /// <param name="alert">The metric alert notification details.</param>
    /// <returns>A task that completes when the client has acknowledged the message.</returns>
    Task ReceiveMetricAlert(MetricAlertNotification alert);

    /// <summary>
    /// Receives a generic real-time event notification.
    /// </summary>
    /// <param name="notification">The real-time event details.</param>
    /// <returns>A task that completes when the client has acknowledged the message.</returns>
    Task ReceiveNotification(RealTimeEvent notification);

    /// <summary>
    /// Receives a dashboard data update notification.
    /// </summary>
    /// <param name="update">The dashboard data update details.</param>
    /// <returns>A task that completes when the client has acknowledged the message.</returns>
    Task ReceiveDashboardUpdate(DashboardDataUpdate update);
}
