using CognitiveMesh.AgencyLayer.RealTime.Ports;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.AgencyLayer.RealTime.Hubs;

/// <summary>
/// SignalR hub that provides real-time communication for the Cognitive Mesh platform.
/// Clients can subscribe to agent updates, join dashboard groups, and receive
/// workflow progress notifications through this hub.
/// </summary>
public class CognitiveMeshHub : Hub<ICognitiveMeshHubClient>
{
    private readonly ILogger<CognitiveMeshHub> _logger;
    private readonly IRealTimeNotificationPort _notificationPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="CognitiveMeshHub"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="notificationPort">The notification port for managing real-time connections.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public CognitiveMeshHub(
        ILogger<CognitiveMeshHub> logger,
        IRealTimeNotificationPort notificationPort)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _notificationPort = notificationPort ?? throw new ArgumentNullException(nameof(notificationPort));
    }

    /// <summary>
    /// Called when a new client connects to the hub. Extracts user information
    /// from the connection context and tracks the connection.
    /// </summary>
    /// <returns>A task that completes when the connection has been registered.</returns>
    public override async Task OnConnectedAsync()
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier ?? "anonymous";

        _logger.LogInformation(
            "Client connected. ConnectionId: {ConnectionId}, UserId: {UserId}",
            connectionId,
            userId);

        await _notificationPort.AddToGroupAsync(connectionId, "all-users", CancellationToken.None);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub. Removes the connection from tracking.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect, if any.</param>
    /// <returns>A task that completes when the disconnection has been processed.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        var userId = Context.UserIdentifier ?? "anonymous";

        if (exception is not null)
        {
            _logger.LogWarning(
                exception,
                "Client disconnected with error. ConnectionId: {ConnectionId}, UserId: {UserId}",
                connectionId,
                userId);
        }
        else
        {
            _logger.LogInformation(
                "Client disconnected. ConnectionId: {ConnectionId}, UserId: {UserId}",
                connectionId,
                userId);
        }

        await _notificationPort.RemoveFromGroupAsync(connectionId, "all-users", CancellationToken.None);
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Adds the calling client to a dashboard group to receive targeted dashboard updates.
    /// </summary>
    /// <param name="dashboardId">The unique identifier of the dashboard to join.</param>
    /// <returns>A task that completes when the client has joined the dashboard group.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dashboardId"/> is null or whitespace.</exception>
    public async Task JoinDashboardGroup(string dashboardId)
    {
        if (string.IsNullOrWhiteSpace(dashboardId))
        {
            throw new ArgumentException("Dashboard ID must not be null or whitespace.", nameof(dashboardId));
        }

        var groupName = $"dashboard-{dashboardId}";
        await _notificationPort.AddToGroupAsync(Context.ConnectionId, groupName, CancellationToken.None);

        _logger.LogInformation(
            "Client {ConnectionId} joined dashboard group {GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Removes the calling client from a dashboard group.
    /// </summary>
    /// <param name="dashboardId">The unique identifier of the dashboard to leave.</param>
    /// <returns>A task that completes when the client has left the dashboard group.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dashboardId"/> is null or whitespace.</exception>
    public async Task LeaveDashboardGroup(string dashboardId)
    {
        if (string.IsNullOrWhiteSpace(dashboardId))
        {
            throw new ArgumentException("Dashboard ID must not be null or whitespace.", nameof(dashboardId));
        }

        var groupName = $"dashboard-{dashboardId}";
        await _notificationPort.RemoveFromGroupAsync(Context.ConnectionId, groupName, CancellationToken.None);

        _logger.LogInformation(
            "Client {ConnectionId} left dashboard group {GroupName}",
            Context.ConnectionId,
            groupName);
    }

    /// <summary>
    /// Subscribes the calling client to receive updates for a specific agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent to subscribe to.</param>
    /// <returns>A task that completes when the subscription has been registered.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="agentId"/> is null or whitespace.</exception>
    public async Task SubscribeToAgent(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent ID must not be null or whitespace.", nameof(agentId));
        }

        var groupName = $"agent-{agentId}";
        await _notificationPort.AddToGroupAsync(Context.ConnectionId, groupName, CancellationToken.None);

        _logger.LogInformation(
            "Client {ConnectionId} subscribed to agent {AgentId}",
            Context.ConnectionId,
            agentId);
    }

    /// <summary>
    /// Unsubscribes the calling client from updates for a specific agent.
    /// </summary>
    /// <param name="agentId">The unique identifier of the agent to unsubscribe from.</param>
    /// <returns>A task that completes when the subscription has been removed.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="agentId"/> is null or whitespace.</exception>
    public async Task UnsubscribeFromAgent(string agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            throw new ArgumentException("Agent ID must not be null or whitespace.", nameof(agentId));
        }

        var groupName = $"agent-{agentId}";
        await _notificationPort.RemoveFromGroupAsync(Context.ConnectionId, groupName, CancellationToken.None);

        _logger.LogInformation(
            "Client {ConnectionId} unsubscribed from agent {AgentId}",
            Context.ConnectionId,
            agentId);
    }
}
