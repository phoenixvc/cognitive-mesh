namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents the current status of a milestone-based workflow.
/// Tracks the lifecycle from creation through completion or failure.
/// </summary>
public enum WorkflowStatus
{
    /// <summary>Workflow has been created but not yet started.</summary>
    NotStarted,

    /// <summary>Workflow is actively progressing through phases.</summary>
    InProgress,

    /// <summary>All phases have been completed successfully.</summary>
    Completed,

    /// <summary>Workflow has been rolled back to a previous phase.</summary>
    RolledBack,

    /// <summary>Workflow has encountered a fatal error and cannot proceed.</summary>
    Failed
}
