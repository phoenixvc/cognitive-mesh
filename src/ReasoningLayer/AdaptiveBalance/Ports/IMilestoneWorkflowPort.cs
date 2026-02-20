using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

/// <summary>
/// Defines the contract for the milestone-based workflow engine.
/// Manages workflows consisting of ordered phases with pre-conditions,
/// post-conditions, and rollback capabilities.
/// </summary>
public interface IMilestoneWorkflowPort
{
    /// <summary>
    /// Creates a new workflow from an ordered list of phases.
    /// </summary>
    /// <param name="phases">The ordered list of phases for the workflow.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The created workflow in NotStarted status.</returns>
    Task<MilestoneWorkflow> CreateWorkflowAsync(List<MilestonePhase> phases, CancellationToken cancellationToken);

    /// <summary>
    /// Advances the workflow to the next phase.
    /// Verifies post-conditions of the current phase are met before advancing.
    /// </summary>
    /// <param name="workflowId">The identifier of the workflow to advance.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated workflow after advancing to the next phase.</returns>
    Task<MilestoneWorkflow> AdvancePhaseAsync(Guid workflowId, CancellationToken cancellationToken);

    /// <summary>
    /// Rolls back the workflow to a previously defined rollback target phase.
    /// Uses the current phase's RollbackToPhaseId to determine the target.
    /// </summary>
    /// <param name="workflowId">The identifier of the workflow to roll back.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The updated workflow after rollback.</returns>
    Task<MilestoneWorkflow> RollbackPhaseAsync(Guid workflowId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a workflow by its identifier.
    /// </summary>
    /// <param name="workflowId">The identifier of the workflow to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The workflow if found; otherwise, <c>null</c>.</returns>
    Task<MilestoneWorkflow?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken);
}
