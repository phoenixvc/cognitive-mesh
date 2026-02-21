namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// DTO representing an update to dashboard data, broadcast to clients in a specific dashboard group.
/// </summary>
/// <param name="DashboardId">The unique identifier of the dashboard that was updated.</param>
/// <param name="SectionId">The identifier of the specific dashboard section that changed.</param>
/// <param name="Data">The updated data payload for the dashboard section.</param>
/// <param name="Timestamp">The timestamp when the dashboard data was refreshed.</param>
/// <param name="Details">Optional human-readable details about the update.</param>
public record DashboardDataUpdate(
    string DashboardId,
    string SectionId,
    object Data,
    DateTimeOffset Timestamp,
    string? Details = null);
