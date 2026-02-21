using AgencyLayer.CognitiveSovereignty.Models;

namespace AgencyLayer.CognitiveSovereignty.Ports;

/// <summary>
/// Defines the port for recording and retrieving authorship provenance trails
/// that track who authored what (human, agent, or hybrid) for each task.
/// </summary>
public interface IAuthorshipTrailPort
{
    /// <summary>
    /// Records an authorship entry for a task, adding it to the task's authorship trail.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="entry">The authorship entry to record.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated authorship trail for the task.</returns>
    Task<AuthorshipTrail> RecordAuthorshipAsync(string taskId, AuthorshipEntry entry, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the complete authorship trail for a task.
    /// </summary>
    /// <param name="taskId">The unique identifier of the task.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The authorship trail, or <c>null</c> if no trail exists for the task.</returns>
    Task<AuthorshipTrail?> GetTrailAsync(string taskId, CancellationToken ct = default);
}
