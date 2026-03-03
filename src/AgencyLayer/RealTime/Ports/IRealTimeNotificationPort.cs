using CognitiveMesh.AgencyLayer.RealTime.Models;

namespace CognitiveMesh.AgencyLayer.RealTime.Ports;

/// <summary>
/// Defines the contract for sending real-time notifications to connected clients.
/// This port follows the hexagonal architecture pattern and abstracts the underlying
/// transport mechanism (e.g., SignalR) from the business logic.
/// </summary>
public interface IRealTimeNotificationPort
{
    /// <summary>
    /// Broadcasts an event to all connected clients.
    /// </summary>
    /// <param name="eventName">The name of the event to broadcast.</param>
    /// <param name="payload">The event payload data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the broadcast has been dispatched.</returns>
    Task BroadcastAsync(string eventName, object payload, CancellationToken ct);

    /// <summary>
    /// Sends an event to a specific user identified by their user identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the target user.</param>
    /// <param name="eventName">The name of the event to send.</param>
    /// <param name="payload">The event payload data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been dispatched.</returns>
    Task SendToUserAsync(string userId, string eventName, object payload, CancellationToken ct);

    /// <summary>
    /// Sends an event to all clients in a specific group.
    /// </summary>
    /// <param name="groupName">The name of the target group.</param>
    /// <param name="eventName">The name of the event to send.</param>
    /// <param name="payload">The event payload data.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the message has been dispatched.</returns>
    Task SendToGroupAsync(string groupName, string eventName, object payload, CancellationToken ct);

    /// <summary>
    /// Adds a connection to a named group for targeted message delivery.
    /// </summary>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    /// <param name="groupName">The name of the group to join.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the connection has been added to the group.</returns>
    Task AddToGroupAsync(string connectionId, string groupName, CancellationToken ct);

    /// <summary>
    /// Removes a connection from a named group.
    /// </summary>
    /// <param name="connectionId">The SignalR connection identifier.</param>
    /// <param name="groupName">The name of the group to leave.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task that completes when the connection has been removed from the group.</returns>
    Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken ct);

    /// <summary>
    /// Retrieves a snapshot of all currently connected users.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A read-only list of currently connected users.</returns>
    Task<IReadOnlyList<ConnectedUser>> GetConnectedUsersAsync(CancellationToken ct);
}
