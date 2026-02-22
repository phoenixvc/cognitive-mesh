using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports;

namespace CognitiveMesh.ReasoningLayer.AgencyRouter.Engines;

/// <summary>
/// Defines the contract for policy persistence operations in the agency router.
/// </summary>
public interface IPolicyRepository
{
    /// <summary>Gets the policy configuration for the specified tenant.</summary>
    Task<PolicyConfiguration> GetPolicyForTenantAsync(string tenantId);
    /// <summary>Saves the policy configuration for a tenant.</summary>
    Task<bool> SavePolicyForTenantAsync(PolicyConfiguration policy);
}

/// <summary>
/// Defines the contract for task context persistence operations.
/// </summary>
public interface ITaskContextRepository
{
    /// <summary>Saves the task context.</summary>
    Task SaveTaskContextAsync(TaskContext context);
    /// <summary>Retrieves the task context by task and tenant identifiers.</summary>
    Task<TaskContext> GetTaskContextAsync(string taskId, string tenantId);
    /// <summary>Updates the autonomy level for the specified task.</summary>
    Task UpdateTaskAutonomyAsync(string taskId, string tenantId, AutonomyLevel newLevel, ProvenanceContext overrideProvenance);
}

/// <summary>
/// Defines the contract for audit logging in the agency router.
/// </summary>
public interface IAuditLoggingAdapter
{
    /// <summary>Logs an audit event with its associated provenance context.</summary>
    Task LogAuditEventAsync(string eventType, object eventData, ProvenanceContext provenance);
}


// --- Domain Engine Implementation ---
/// <summary>
/// A pure domain engine that implements the core business logic for the Contextual Adaptive Agency/Sovereignty Router.
/// As part of a Hexagonal Architecture, this engine is completely isolated from infrastructure concerns
/// and depends only on the contracts defined by its Port and outbound Adapters.
/// </summary>
public class ContextualAdaptiveAgencyEngine : IAgencyRouterPort
{
    private readonly ILogger<ContextualAdaptiveAgencyEngine> _logger;
    private readonly IPolicyRepository _policyRepository;
    private readonly ITaskContextRepository _taskContextRepository;
    private readonly IAuditLoggingAdapter _auditLoggingAdapter;

    // In-memory cache for tenant policies to improve performance, as per the PRD.
    private static readonly ConcurrentDictionary<string, PolicyConfiguration> _policyCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualAdaptiveAgencyEngine"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="policyRepository">The repository for persisting and retrieving policy configurations.</param>
    /// <param name="taskContextRepository">The repository for persisting and retrieving task contexts.</param>
    /// <param name="auditLoggingAdapter">The adapter for immutable audit logging.</param>
    public ContextualAdaptiveAgencyEngine(
        ILogger<ContextualAdaptiveAgencyEngine> logger,
        IPolicyRepository policyRepository,
        ITaskContextRepository taskContextRepository,
        IAuditLoggingAdapter auditLoggingAdapter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _policyRepository = policyRepository ?? throw new ArgumentNullException(nameof(policyRepository));
        _taskContextRepository = taskContextRepository ?? throw new ArgumentNullException(nameof(taskContextRepository));
        _auditLoggingAdapter = auditLoggingAdapter ?? throw new ArgumentNullException(nameof(auditLoggingAdapter));
    }

    /// <inheritdoc />
    public async Task<AgencyModeDecision> RouteTaskAsync(TaskContext context)
    {
        if (context?.Provenance == null || string.IsNullOrWhiteSpace(context.TaskId))
        {
            throw new ArgumentException("Task context and provenance information are required.", nameof(context));
        }

        var decision = new AgencyModeDecision
        {
            CorrelationId = context.Provenance.CorrelationId
        };

        try
        {
            var policy = await GetPolicyAsync(context.Provenance.TenantId);
            decision.PolicyVersionApplied = policy?.PolicyVersion ?? "N/A";

            // Evaluate policy rules to determine the autonomy level.
            var chosenLevel = EvaluatePolicy(context, policy!);
            decision.ChosenAutonomyLevel = chosenLevel;
            decision.Justification = $"Autonomy level set to '{chosenLevel}' based on policy '{decision.PolicyVersionApplied}'.";

            await _auditLoggingAdapter.LogAuditEventAsync("AgencyRoutingDecisionMade", decision, context.Provenance);
            await _taskContextRepository.SaveTaskContextAsync(context);

            return decision;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during task routing for CorrelationId '{CorrelationId}'.", context.Provenance.CorrelationId);
            decision.Justification = "An internal error occurred during routing.";
            decision.ChosenAutonomyLevel = AutonomyLevel.SovereigntyFirst; // Fail-safe to the most restrictive mode.
            await _auditLoggingAdapter.LogAuditEventAsync("AgencyRoutingFailed", new { Error = ex.Message, Decision = decision }, context.Provenance);
            return decision;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ApplyOverrideAsync(OverrideRequest request)
    {
        if (request?.Provenance == null || string.IsNullOrWhiteSpace(request.TaskId))
        {
            throw new ArgumentException("Override request and provenance information are required.", nameof(request));
        }

        try
        {
            // In a real system, we would fetch the task and update its state.
            // Here, we simulate this by calling the repository to update the record.
            await _taskContextRepository.UpdateTaskAutonomyAsync(request.TaskId, request.Provenance.TenantId, request.ForcedAutonomyLevel, request.Provenance);

            await _auditLoggingAdapter.LogAuditEventAsync("AgencyModeOverridden", request, request.Provenance);
            _logger.LogInformation("Successfully applied override for TaskId '{TaskId}' to level '{ForcedLevel}'.", request.TaskId, request.ForcedAutonomyLevel);
                
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply override for TaskId '{TaskId}'.", request.TaskId);
            await _auditLoggingAdapter.LogAuditEventAsync("AgencyOverrideFailed", new { Error = ex.Message, Request = request }, request.Provenance);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<PolicyConfiguration> GetPolicyAsync(string tenantId)
    {
        if (_policyCache.TryGetValue(tenantId, out var cachedPolicy))
        {
            _logger.LogDebug("Policy for Tenant '{TenantId}' served from cache.", tenantId);
            return cachedPolicy;
        }

        var policy = await _policyRepository.GetPolicyForTenantAsync(tenantId);
        if (policy != null)
        {
            _policyCache[tenantId] = policy;
        }

        return policy ?? new PolicyConfiguration { TenantId = tenantId, PolicyVersion = "default" };
    }

    /// <inheritdoc />
    public async Task<bool> UpdatePolicyAsync(PolicyConfiguration policy)
    {
        var success = await _policyRepository.SavePolicyForTenantAsync(policy);
        if (success)
        {
            // Invalidate the cache to ensure the new policy is loaded on next request.
            _policyCache.TryRemove(policy.TenantId, out _);
            await _auditLoggingAdapter.LogAuditEventAsync("PolicyUpdated", policy, new ProvenanceContext { TenantId = policy.TenantId, ActorId = "AdminSystem" });
        }
        return success;
    }

    /// <inheritdoc />
    public async Task<TaskContext> GetIntrospectionDataAsync(string taskId, string tenantId)
    {
        return await _taskContextRepository.GetTaskContextAsync(taskId, tenantId);
    }

    /// <summary>
    /// A simple rule evaluation engine that processes policy conditions against the task context.
    /// </summary>
    /// <param name="context">The task context containing CIA/CSI scores and other metadata.</param>
    /// <param name="policy">The policy configuration with rules to evaluate.</param>
    /// <returns>The determined AutonomyLevel based on the first matching rule.</returns>
    private AutonomyLevel EvaluatePolicy(TaskContext context, PolicyConfiguration policy)
    {
        if (policy?.Rules == null)
        {
            // Fail-safe default if no policy is defined.
            return AutonomyLevel.SovereigntyFirst;
        }

        foreach (var rule in policy.Rules.OrderBy(r => r.Condition)) // A simple order for deterministic evaluation
        {
            if (IsRuleConditionMet(rule.Condition, context))
            {
                _logger.LogInformation("Policy rule matched: '{Condition}'. Applying action: '{Action}'.", rule.Condition, rule.Action);
                return rule.Action;
            }
        }

        // Default action if no rules match.
        _logger.LogInformation("No specific policy rules matched. Applying default autonomy level.");
        return AutonomyLevel.ActWithConfirmation;
    }

    /// <summary>
    /// Parses and evaluates a single rule condition string.
    /// This is a basic implementation; a real system might use a more robust expression library.
    /// </summary>
    private bool IsRuleConditionMet(string condition, TaskContext context)
    {
        var parts = condition.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3) return false; // Expecting "Variable Operator Value" format

        var variableName = parts[0].Trim();
        var op = parts[1].Trim();
        var valueStr = parts[2].Trim();

        try
        {
            switch (variableName.ToUpperInvariant())
            {
                case "CIA":
                    return Compare(context.CognitiveImpactAssessmentScore, op, double.Parse(valueStr));
                case "CSI":
                    return Compare(context.CognitiveSovereigntyIndexScore, op, double.Parse(valueStr));
                case "TASKTYPE":
                    return Compare(context.TaskType, op, valueStr.Trim('\''));
                default:
                    return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse or evaluate rule condition: '{Condition}'.", condition);
            return false;
        }
    }

    private bool Compare<T>(T left, string op, T right) where T : IComparable<T>
    {
        return op switch
        {
            ">" => left.CompareTo(right) > 0,
            "<" => left.CompareTo(right) < 0,
            "==" => left.CompareTo(right) == 0,
            "!=" => left.CompareTo(right) != 0,
            ">=" => left.CompareTo(right) >= 0,
            "<=" => left.CompareTo(right) <= 0,
            _ => false
        };
    }
}