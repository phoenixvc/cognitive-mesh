using System.Collections.Concurrent;
using CognitiveMesh.AgencyLayer.RealTime.Hubs;
using CognitiveMesh.AgencyLayer.RealTime.Models;
using CognitiveMesh.AgencyLayer.RealTime.Ports;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.AgencyLayer.RealTime.Adapters;

/// <summary>
/// SignalR-based implementation of <see cref="IRealTimeNotificationPort"/>.
/// Uses a typed <see cref="IHubContext{THub, T}"/> to send messages to connected clients
/// and tracks connected users in a thread-safe concurrent dictionary.
/// </summary>
public class SignalRNotificationAdapter : IRealTimeNotificationPort
{
    private readonly IHubContext<CognitiveMeshHub, ICognitiveMeshHubClient> _hubContext;
    private readonly ILogger<SignalRNotificationAdapter> _logger;
    private readonly ConcurrentDictionary<string, ConnectedUser> _connectedUsers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SignalRNotificationAdapter"/> class.
    /// </summary>
    /// <param name="hubContext">The typed SignalR hub context for sending messages to clients.</param>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public SignalRNotificationAdapter(
        IHubContext<CognitiveMeshHub, ICognitiveMeshHubClient> hubContext,
        ILogger<SignalRNotificationAdapter> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task BroadcastAsync(string eventName, object payload, CancellationToken ct)
    {
        _logger.LogDebug("Broadcasting event {EventName} to all clients", eventName);

        var notification = new RealTimeEvent(
            EventId: Guid.NewGuid().ToString(),
            EventType: eventName,
            Payload: payload,
            Timestamp: DateTimeOffset.UtcNow);

        await _hubContext.Clients.All.ReceiveNotification(notification);
    }

    /// <inheritdoc />
    public async Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct)
    {
        _logger.LogDebug("Sending event {EventName} to user {UserId}", eventName, userId);

        var notification = new RealTimeEvent(
            EventId: Guid.NewGuid().ToString(),
            EventType: eventName,
            Payload: payload,
            Timestamp: DateTimeOffset.UtcNow);

        await _hubContext.Clients.User(userId).ReceiveNotification(notification);
    }

    /// <inheritdoc />
    public async Task SendToGroupAsync(string groupName, string eventName, object payload, CancellationToken ct)
    {
        _logger.LogDebug("Sending event {EventName} to group {GroupName}", eventName, groupName);

        var notification = new RealTimeEvent(
            EventId: Guid.NewGuid().ToString(),
            EventType: eventName,
            Payload: payload,
            Timestamp: DateTimeOffset.UtcNow);

        await _hubContext.Clients.Group(groupName).ReceiveNotification(notification);
    }

    /// <inheritdoc />
    public async Task AddToGroupAsync(string connectionId, string groupName, CancellationToken ct)
    {
        _logger.LogDebug(
            "Adding connection {ConnectionId} to group {GroupName}",
            connectionId,
            groupName);

        await _hubContext.Groups.AddToGroupAsync(connectionId, groupName, ct);
    }

    /// <inheritdoc />
    public async Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken ct)
    {
        _logger.LogDebug(
            "Removing connection {ConnectionId} from group {GroupName}",
            connectionId,
            groupName);

        await _hubContext.Groups.RemoveFromGroupAsync(connectionId, groupName, ct);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ConnectedUser>> GetConnectedUsersAsync(CancellationToken ct)
    {
        IReadOnlyList<ConnectedUser> users = _connectedUsers.Values.ToList().AsReadOnly();
        return Task.FromResult(users);
    }

    /// <summary>
    /// Registers a connected user in the internal tracking dictionary.
    /// Called by hub lifecycle methods to maintain the connected user list.
    /// </summary>
    /// <param name="user">The connected user to track.</param>
    public void TrackConnection(ConnectedUser user)
    {
        _connectedUsers.TryAdd(user.ConnectionId, user);
        _logger.LogDebug(
            "Tracking connection {ConnectionId} for user {UserId}",
            user.ConnectionId,
            user.UserId);
    }

    /// <summary>
    /// Removes a connected user from the internal tracking dictionary.
    /// Called by hub lifecycle methods when a client disconnects.
    /// </summary>
    /// <param name="connectionId">The connection identifier to stop tracking.</param>
    public void UntrackConnection(string connectionId)
    {
        _connectedUsers.TryRemove(connectionId, out _);
        _logger.LogDebug("Untracked connection {ConnectionId}", connectionId);
    }
}
