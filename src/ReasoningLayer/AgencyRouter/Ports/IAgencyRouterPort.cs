// --- DTOs and Models for the Agency Router Port ---

namespace CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;

/// <summary>
/// Defines the degree of independent decision-making an agent can exercise.
/// This is a foundational concept from the Agent Framework.
/// </summary>
public enum AutonomyLevel
{
    /// <summary>
    /// The agent provides recommendations only; all actions require explicit human approval.
    /// </summary>
    RecommendOnly,
    /// <summary>
    /// The agent can act independently but must obtain confirmation before executing significant actions.
    /// </summary>
    ActWithConfirmation,
    /// <summary>
    /// The agent operates with full autonomy, executing actions without requiring human confirmation.
    /// </summary>
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
    /// <summary>Gets or sets the tenant identifier for multi-tenant isolation.</summary>
    public string TenantId { get; set; } = string.Empty;
    /// <summary>Gets or sets the identifier of the actor initiating the request.</summary>
    public string ActorId { get; set; } = string.Empty;
    /// <summary>Gets or sets the identifier of the consent record authorizing this action.</summary>
    public string ConsentId { get; set; } = string.Empty;
    /// <summary>Gets or sets the correlation identifier for distributed tracing.</summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Represents the contextual information about a task that needs to be routed.
/// This is the primary input to the Agency Router.
/// </summary>
public class TaskContext
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;
    /// <summary>Gets or sets the unique identifier of the task.</summary>
    public string TaskId { get; set; } = string.Empty;
    /// <summary>Gets or sets the type of the task (e.g., "CreativeWriting", "DataAnalysis", "CodeGeneration").</summary>
    public string TaskType { get; set; } = string.Empty;
    /// <summary>Gets or sets a human-readable description of the task.</summary>
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
    /// <summary>Gets or sets the unique identifier for this routing decision.</summary>
    public string DecisionId { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Gets or sets the correlation identifier linking this decision to the originating request.</summary>
    public string CorrelationId { get; set; } = string.Empty;
    /// <summary>Gets or sets the autonomy level determined by the router for the task.</summary>
    public AutonomyLevel ChosenAutonomyLevel { get; set; }
    /// <summary>Gets or sets the authority scope applied to the task based on policy evaluation.</summary>
    public object AppliedAuthorityScope { get; set; } = default!;
    /// <summary>Gets or sets the name of the recommended execution engine (e.g., "FullyAutonomousAgent", "HumanInTheLoopWorkflow").</summary>
    public string RecommendedEngine { get; set; } = string.Empty;
    /// <summary>Gets or sets the human-readable justification explaining why this autonomy level was chosen.</summary>
    public string Justification { get; set; } = string.Empty;
    /// <summary>Gets or sets the version of the policy that was applied to make this decision.</summary>
    public string PolicyVersionApplied { get; set; } = string.Empty;
    /// <summary>Gets or sets the UTC timestamp when this decision was made.</summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Represents a request from a user or admin to override the router's default decision.
/// </summary>
public class OverrideRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata for the override.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;
    /// <summary>Gets or sets the identifier of the task whose agency mode is being overridden.</summary>
    public string TaskId { get; set; } = string.Empty;
    /// <summary>Gets or sets the autonomy level to force on the task.</summary>
    public AutonomyLevel ForcedAutonomyLevel { get; set; }
    /// <summary>Gets or sets the justification provided by the requestor for the override.</summary>
    public string ReasonForOverride { get; set; } = string.Empty;
}

/// <summary>
/// Defines a single rule within a policy, consisting of a condition and a corresponding action.
/// </summary>
public class RoutingRule
{
    /// <summary>Gets or sets the condition expression to evaluate (e.g., "CIA > 0.8", "TaskType == 'CreativeWriting'").</summary>
    public string Condition { get; set; } = string.Empty;
    /// <summary>Gets or sets the autonomy level to enforce when the condition is met.</summary>
    public AutonomyLevel Action { get; set; }
}

/// <summary>
/// Represents a set of configurable policies that govern the router's decisions.
/// </summary>
public class PolicyConfiguration
{
    /// <summary>Gets or sets the unique identifier for this policy configuration.</summary>
    public string PolicyId { get; set; } = string.Empty;
    /// <summary>Gets or sets the tenant identifier that this policy applies to.</summary>
    public string TenantId { get; set; } = string.Empty;
    /// <summary>Gets or sets the version string of this policy for audit and traceability.</summary>
    public string PolicyVersion { get; set; } = string.Empty;
    /// <summary>Gets or sets the ordered list of routing rules that govern agency decisions.</summary>
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