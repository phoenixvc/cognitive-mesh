using AgencyLayer.CognitiveSovereignty.Models;

namespace AgencyLayer.CognitiveSovereignty.Ports;

/// <summary>
/// Defines the port for managing agent action approval workflows
/// within the cognitive sovereignty framework. Enables human-in-the-loop
/// approval or rejection of agent-proposed actions.
/// </summary>
public interface IAgentActionApprovalPort
{
    /// <summary>
    /// Submits an agent action for human approval.
    /// </summary>
    /// <param name="action">The agent action to submit for approval.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The submitted action with its assigned identifier.</returns>
    Task<AgentAction> SubmitActionForApprovalAsync(AgentAction action, CancellationToken ct = default);

    /// <summary>
    /// Approves a pending agent action, allowing it to proceed.
    /// </summary>
    /// <param name="actionId">The unique identifier of the action to approve.</param>
    /// <param name="approvedBy">The identifier of the user who approved the action.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The approved action.</returns>
    Task<AgentAction> ApproveActionAsync(string actionId, string approvedBy, CancellationToken ct = default);

    /// <summary>
    /// Rejects a pending agent action, preventing it from proceeding.
    /// </summary>
    /// <param name="actionId">The unique identifier of the action to reject.</param>
    /// <param name="rejectedBy">The identifier of the user who rejected the action.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The rejected action.</returns>
    Task<AgentAction> RejectActionAsync(string actionId, string rejectedBy, CancellationToken ct = default);

    /// <summary>
    /// Retrieves all pending (unapproved, unrejected) actions for review.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A list of pending agent actions.</returns>
    Task<IReadOnlyList<AgentAction>> GetPendingActionsAsync(CancellationToken ct = default);
}
