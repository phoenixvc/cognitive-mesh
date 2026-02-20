namespace AgencyLayer.CognitiveSovereignty.Models;

/// <summary>
/// Records an agent action submitted for approval within the cognitive sovereignty framework.
/// Tracks the agent, task, autonomy level, and approval status.
/// </summary>
public class AgentAction
{
    /// <summary>
    /// Unique identifier for this action.
    /// </summary>
    public string ActionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Identifier of the agent that produced or requests this action.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Identifier of the task this action relates to.
    /// </summary>
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of what the agent intends to do.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Autonomy level at which this action was generated (0.0 = fully manual, 1.0 = fully autonomous).
    /// </summary>
    public double AutonomyLevel { get; set; }

    /// <summary>
    /// Indicates whether this action requires explicit human approval before execution.
    /// </summary>
    public bool RequiresApproval { get; set; }

    /// <summary>
    /// Indicates whether this action has been approved by a human reviewer.
    /// <c>null</c> means pending approval.
    /// </summary>
    public bool? Approved { get; set; }

    /// <summary>
    /// Identifier of the user who approved or rejected this action.
    /// Empty when the action is still pending.
    /// </summary>
    public string ApprovedBy { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the action was created or submitted.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
