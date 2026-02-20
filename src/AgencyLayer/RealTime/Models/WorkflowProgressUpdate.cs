namespace CognitiveMesh.AgencyLayer.RealTime.Models;

/// <summary>
/// DTO representing progress within a durable workflow, broadcast to subscribed clients.
/// </summary>
/// <param name="WorkflowId">The unique identifier of the workflow.</param>
/// <param name="StepName">The name of the current or completed step.</param>
/// <param name="StepIndex">The zero-based index of the current step.</param>
/// <param name="TotalSteps">The total number of steps in the workflow.</param>
/// <param name="Status">The status of the step (e.g., "Completed", "Failed", "InProgress").</param>
/// <param name="Timestamp">The timestamp when this progress update was generated.</param>
/// <param name="Details">Optional human-readable details about the step progress.</param>
public record WorkflowProgressUpdate(
    string WorkflowId,
    string StepName,
    int StepIndex,
    int TotalSteps,
    string Status,
    DateTimeOffset Timestamp,
    string? Details = null);
