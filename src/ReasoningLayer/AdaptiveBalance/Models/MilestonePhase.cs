namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a single phase within a milestone-based workflow.
/// Each phase defines pre-conditions, post-conditions, and rollback behavior.
/// </summary>
/// <param name="PhaseId">Unique identifier for this phase.</param>
/// <param name="Name">Human-readable name of the phase.</param>
/// <param name="PreConditions">Conditions that must be met before entering this phase.</param>
/// <param name="PostConditions">Conditions that must be met before leaving this phase.</param>
/// <param name="FeedbackEnabled">Whether user feedback is collected during this phase.</param>
/// <param name="RollbackToPhaseId">The phase to roll back to on failure; <c>null</c> if rollback is not supported.</param>
public sealed record MilestonePhase(
    string PhaseId,
    string Name,
    List<string> PreConditions,
    List<string> PostConditions,
    bool FeedbackEnabled,
    string? RollbackToPhaseId);
