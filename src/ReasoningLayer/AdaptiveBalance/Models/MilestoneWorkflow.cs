namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a milestone-based workflow consisting of ordered phases.
/// Tracks the current phase, overall status, and timing information.
/// </summary>
public sealed class MilestoneWorkflow
{
    /// <summary>Gets or sets the unique identifier for this workflow.</summary>
    public Guid WorkflowId { get; set; }

    /// <summary>Gets or sets the ordered list of phases in this workflow.</summary>
    public List<MilestonePhase> Phases { get; set; } = [];

    /// <summary>Gets or sets the index of the currently active phase.</summary>
    public int CurrentPhaseIndex { get; set; }

    /// <summary>Gets or sets the current status of the workflow.</summary>
    public WorkflowStatus Status { get; set; }

    /// <summary>Gets or sets the timestamp when the workflow was started.</summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>Gets or sets the timestamp when the workflow was completed; <c>null</c> if still in progress.</summary>
    public DateTimeOffset? CompletedAt { get; set; }
}
