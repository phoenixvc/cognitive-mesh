using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;

namespace CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Ports;

/// <summary>
/// Defines the contract for recording and retrieving temporal events in the TDC.
/// Events are the fundamental observation units that the dual-circuit gate
/// evaluates for potential temporal linking.
/// </summary>
public interface ITemporalEventPort
{
    /// <summary>
    /// Records a new temporal event into the CA1 eligibility gate buffer.
    /// </summary>
    /// <param name="temporalEvent">The temporal event to record.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The recorded event with any server-side enrichments applied.</returns>
    Task<TemporalEvent> RecordEventAsync(TemporalEvent temporalEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single temporal event by its unique identifier.
    /// </summary>
    /// <param name="eventId">The unique identifier of the event to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The temporal event if found; otherwise, null.</returns>
    Task<TemporalEvent?> GetEventAsync(string eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all temporal events within the specified time range.
    /// </summary>
    /// <param name="start">The inclusive start of the time range.</param>
    /// <param name="end">The inclusive end of the time range.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>A read-only list of temporal events within the range, ordered by timestamp.</returns>
    Task<IReadOnlyList<TemporalEvent>> GetEventsInRangeAsync(DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
}
