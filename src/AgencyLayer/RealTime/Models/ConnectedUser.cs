namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// Represents a user currently connected to the real-time hub.
/// </summary>
/// <param name="UserId">The unique identifier of the connected user.</param>
/// <param name="ConnectionId">The SignalR connection identifier for this session.</param>
/// <param name="ConnectedAt">The timestamp when the user connected.</param>
/// <param name="TenantId">The optional tenant identifier for multi-tenant scenarios.</param>
public record ConnectedUser(
    string UserId,
    string ConnectionId,
    DateTimeOffset ConnectedAt,
    string? TenantId = null);
