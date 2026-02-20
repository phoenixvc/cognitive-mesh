// --- DTOs and Models for the Agency Router Port ---

namespace CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;

/// <summary>
/// Defines the degree of independent decision-making an agent can exercise.
/// This is a foundational concept from the Agent Framework.
/// </summary>
public enum AutonomyLevel
{
    RecommendOnly,
    ActWithConfirmation,
    FullyAutonomous,
    /// <summary>
    /// A special mode where human authorship is paramount, and AI assistance is minimal and explicit.
    /// </summary>
    SovereigntyFirst
}

/// <summary>
/// Contains provenance and consent metadata that must accompany every request,
/// ensuring compliance with the Global NFR Appendix.
/// </summary>
public class ProvenanceContext
{
    public string TenantId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ConsentId { get; set; } = string.Empty; // ID of the consent record for this action
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Represents the contextual information about a task that needs to be routed.
/// This is the primary input to the Agency Router.
/// </summary>
public class TaskContext
{
    public ProvenanceContext Provenance { get; set; } = default!;
    public string TaskId { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty; // e.g., "CreativeWriting", "DataAnalysis", "CodeGeneration"
    public string TaskDescription { get; set; } = string.Empty;

    /// <summary>
    /// Cognitive Impact Assessment (CIA) score. Measures the potential impact of the task on user cognition.
    /// Higher values suggest a greater need for human oversight.
    /// </summary>
    public double CognitiveImpactAssessmentScore { get; set; }

    /// <summary>
    /// Cognitive Sovereignty Index (CSI) score. Measures the current level of human authorship and control.
    /// Lower values may trigger a shift towards more sovereignty-preserving modes.
    /// </summary>
    public double CognitiveSovereigntyIndexScore { get; set; }

    /// <summary>
    /// The initial data or payload the task will operate on.
    /// </summary>
    public object InitialPayload { get; set; } = default!;
}

/// <summary>
/// Represents the router's decision on how an agentic task should proceed.
/// This is the primary output of the IAgencyRouterPort.
/// </summary>
public class AgencyModeDecision
{
    public string DecisionId { get; set; } = Guid.NewGuid().ToString();
    public string CorrelationId { get; set; } = string.Empty;
    public AutonomyLevel ChosenAutonomyLevel { get; set; }
    public object AppliedAuthorityScope { get; set; } = default!; // Could be a specific AuthorityScope object
    public string RecommendedEngine { get; set; } = string.Empty; // e.g., "FullyAutonomousAgent", "HumanInTheLoopWorkflow"
    public string Justification { get; set; } = string.Empty;
    public string PolicyVersionApplied { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a request from a user or admin to override the router's default decision.
/// </summary>
public class OverrideRequest
{
    public ProvenanceContext Provenance { get; set; } = default!;
    public string TaskId { get; set; } = string.Empty;
    public AutonomyLevel ForcedAutonomyLevel { get; set; }
    public string ReasonForOverride { get; set; } = string.Empty;
}

/// <summary>
/// Defines a single rule within a policy, consisting of a condition and a corresponding action.
/// </summary>
public class RoutingRule
{
    public string Condition { get; set; } = string.Empty; // e.g., "CIA > 0.8", "TaskType == 'CreativeWriting' && CSI < 0.5"
    public AutonomyLevel Action { get; set; } // The autonomy level to enforce if the condition is met.
}

/// <summary>
/// Represents a set of configurable policies that govern the router's decisions.
/// </summary>
public class PolicyConfiguration
{
    public string PolicyId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PolicyVersion { get; set; } = string.Empty;
    public List<RoutingRule> Rules { get; set; } = new List<RoutingRule>();
}


// --- Port Interface ---
/// <summary>
/// Defines the contract for the Contextual Adaptive Agency/Sovereignty Router Port.
/// This port is the primary entry point for mediating between autonomous agentic workflows
/// and human-centric, sovereignty-preserving interactions.
/// </summary>
public interface IAgencyRouterPort
{
    /// <summary>
    /// Dynamically routes a task to the appropriate agency mode based on its context,
    /// relevant policies, and cognitive impact scores.
    /// </summary>
    /// <param name="context">The context of the task to be routed.</param>
    /// <returns>A decision specifying the autonomy level and authority scope for the task.</returns>
    /// <remarks>
    /// **SLA:** This operation must return a decision in less than 200ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given a task context, When routed, Then an AgencyModeDecision is returned that complies with the active policy.
    /// </remarks>
    Task<AgencyModeDecision> RouteTaskAsync(TaskContext context);

    /// <summary>
    /// Applies a user or admin-initiated override to a task's agency mode.
    /// This action must be logged with full provenance.
    /// </summary>
    /// <param name="request">The request containing the override details.</param>
    /// <returns>True if the override was successfully applied and logged; otherwise, false.</returns>
    /// <remarks>
    /// **SLA:** This operation must process within 150ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given a valid override request, When applied, Then the system's agency mode for the task is updated and the action is logged immutably.
    /// </remarks>
    Task<bool> ApplyOverrideAsync(OverrideRequest request);

    /// <summary>
    /// Retrieves the current policy configuration for a given tenant.
    /// </summary>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <returns>The active policy configuration.</returns>
    Task<PolicyConfiguration> GetPolicyAsync(string tenantId);

    /// <summary>
    /// Updates the policy configuration for a given tenant.
    /// This is an administrative action and requires elevated permissions.
    /// </summary>
    /// <param name="policy">The new policy configuration to apply.</param>
    /// <returns>True if the policy was successfully updated; otherwise, false.</returns>
    Task<bool> UpdatePolicyAsync(PolicyConfiguration policy);

    /// <summary>
    /// Provides introspection into the scores and context for a given task,
    /// allowing downstream systems or UIs to display diagnostic information.
    /// </summary>
    /// <param name="taskId">The ID of the task to introspect.</param>
    /// <param name="tenantId">The ID of the tenant.</param>
    /// <returns>An object containing the CIA/CSI scores and other relevant context for the task.</returns>
    Task<TaskContext> GetIntrospectionDataAsync(string taskId, string tenantId);
}