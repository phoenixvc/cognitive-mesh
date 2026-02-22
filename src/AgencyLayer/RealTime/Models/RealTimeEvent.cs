namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// Represents a real-time event dispatched through the SignalR hub.
/// </summary>
/// <param name="EventId">The unique identifier of the event.</param>
/// <param name="EventType">The type of event, typically one of the constants from <see cref="RealTimeEventTypes"/>.</param>
/// <param name="Payload">The event payload data.</param>
/// <param name="Timestamp">The timestamp when the event was created.</param>
/// <param name="SourceAgentId">The optional identifier of the agent that produced this event.</param>
public record RealTimeEvent(
    string EventId,
    string EventType,
    object Payload,
    DateTimeOffset Timestamp,
    string? SourceAgentId = null);
