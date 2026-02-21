using FoundationLayer.AuditLogging;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.AIGovernance;

/// <summary>
/// Represents a request to evaluate a specific AI action against all active governance policies.
/// </summary>
public class GovernanceEvaluationRequest
{
    /// <summary>
    /// A unique identifier for the transaction or action being evaluated.
    /// </summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of action being performed (e.g., "DataAnalysis", "CustomerInteraction", "FinancialTransaction").
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the agent or service performing the action.
    /// </summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>
    /// A dictionary containing the contextual data for the action, which will be evaluated against policy rules.
    /// </summary>
    public Dictionary<string, object> ActionContext { get; set; } = new();
}

/// <summary>
/// Represents the outcome of a governance policy evaluation.
/// </summary>
public class GovernanceEvaluationResponse
{
    /// <summary>
    /// The correlation ID linking this response to its originating request.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the action is compliant with all active governance policies.
    /// </summary>
    public bool IsCompliant { get; set; }

    /// <summary>
    /// A list of policy violations that were detected. This will be empty if the action is compliant.
    /// </summary>
    public List<PolicyViolation> Violations { get; set; } = new();
}

/// <summary>
/// Describes a single detected policy violation.
/// </summary>
public class PolicyViolation
{
    /// <summary>
    /// The ID of the policy that was violated.
    /// </summary>
    public string PolicyId { get; set; } = string.Empty;

    /// <summary>
    /// The name of the violated policy.
    /// </summary>
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>
    /// The version of the policy that was active at the time of violation.
    /// </summary>
    public int PolicyVersion { get; set; }

    /// <summary>
    /// A detailed message explaining the nature of the violation.
    /// </summary>
    public string ViolationMessage { get; set; } = string.Empty;
}

/// <summary>
/// Defines the contract for the AI Governance Monitor Port. This port provides the core capability
/// for real-time evaluation of AI actions against the mesh's active governance policies.
/// </summary>
public interface IAIGovernanceMonitorPort
{
    /// <summary>
    /// Evaluates a given AI action against all active governance policies and enforces them by
    /// logging violations and triggering escalation workflows.
    /// </summary>
    /// <param name="request">The request detailing the action to be evaluated.</param>
    /// <returns>A response indicating whether the action is compliant and detailing any violations.</returns>
    Task<GovernanceEvaluationResponse> EvaluateAndEnforcePolicyAsync(GovernanceEvaluationRequest request);
}

/// <summary>
/// Defines the contract for accessing governance policy data, including listing and retrieving active policies.
/// </summary>
public interface IGovernancePort
{
    /// <summary>
    /// Lists all governance policies currently stored in the system.
    /// </summary>
    /// <returns>An enumerable of <see cref="PolicyRecord"/> representing available policies.</returns>
    Task<IEnumerable<PolicyRecord>> ListPoliciesAsync();
}

/// <summary>
/// Represents a single governance policy record, including its metadata and content.
/// </summary>
public class PolicyRecord
{
    /// <summary>
    /// The unique identifier of the policy.
    /// </summary>
    public string PolicyId { get; set; } = string.Empty;

    /// <summary>
    /// The human-readable name of the policy.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The full content or rule definition of the policy.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The version number of the policy.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// The lifecycle status of the policy (e.g., "Active", "Draft", "Retired").
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the policy was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}

/// <summary>
/// Defines the contract for audit logging operations, enabling structured event recording.
/// </summary>
public interface IAuditLoggingPort
{
    /// <summary>
    /// Logs an audit event for compliance and traceability purposes.
    /// </summary>
    /// <param name="auditEvent">The audit event to log.</param>
    Task LogEventAsync(AuditEvent auditEvent);
}

/// <summary>
/// Defines the contract for sending notifications through various channels.
/// </summary>
public interface INotificationPort
{
    /// <summary>
    /// Sends a notification to specified recipients through configured channels.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    Task SendNotificationAsync(Notification notification);
}

/// <summary>
/// Represents a notification message to be delivered through one or more channels.
/// </summary>
public class Notification
{
    /// <summary>
    /// The subject line of the notification.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// The body content of the notification message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The delivery channels for this notification (e.g., "Email", "Slack").
    /// </summary>
    public List<string> Channels { get; set; } = new();

    /// <summary>
    /// The intended recipients of the notification.
    /// </summary>
    public List<string> Recipients { get; set; } = new();

    /// <summary>
    /// The timestamp when the notification was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// Implements the AI Governance Monitor, a critical metacognitive service that ensures all AI actions
/// comply with active governance policies. It provides real-time policy evaluation, violation detection,
/// and automated escalation, forming a key part of the Ethical &amp; Legal Compliance Framework.
/// </summary>
public class AIGovernanceMonitor : IAIGovernanceMonitorPort
{
    private readonly ILogger<AIGovernanceMonitor> _logger;
    private readonly IGovernancePort _governancePort;
    private readonly IAuditLoggingPort _auditLoggingPort;
    private readonly INotificationPort _notificationPort;

    public AIGovernanceMonitor(
        ILogger<AIGovernanceMonitor> logger,
        IGovernancePort governancePort,
        IAuditLoggingPort auditLoggingPort,
        INotificationPort notificationPort)
    {
        _logger = logger;
        _governancePort = governancePort;
        _auditLoggingPort = auditLoggingPort;
        _notificationPort = notificationPort;
    }

    /// <inheritdoc />
    public async Task<GovernanceEvaluationResponse> EvaluateAndEnforcePolicyAsync(GovernanceEvaluationRequest request)
    {
        _logger.LogInformation("Evaluating governance policies for action '{ActionType}' by actor '{ActorId}' (Correlation ID: {CorrelationId}).",
            request.ActionType, request.ActorId, request.CorrelationId);

        var response = new GovernanceEvaluationResponse
        {
            CorrelationId = request.CorrelationId,
            IsCompliant = true
        };

        // 1. Fetch all currently active policies from the governance port.
        // This inherently handles versioning and lifecycle, as the port only returns active policies.
        var activePolicies = await _governancePort.ListPoliciesAsync();
        var policiesToEvaluate = activePolicies.Where(p => p.Status == "Active").ToList();

        _logger.LogDebug("Evaluating against {Count} active policies.", policiesToEvaluate.Count);

        // 2. Evaluate the action against each active policy.
        foreach (var policy in policiesToEvaluate)
        {
            var violation = EvaluatePolicy(policy, request);
            if (violation != null)
            {
                response.IsCompliant = false;
                response.Violations.Add(violation);
            }
        }

        // 3. If any violations were found, enforce the policy by logging and escalating.
        if (!response.IsCompliant)
        {
            _logger.LogWarning("Governance violation detected for action '{ActionType}' (Correlation ID: {CorrelationId}). Violations: {ViolationCount}",
                request.ActionType, request.CorrelationId, response.Violations.Count);

            foreach (var violation in response.Violations)
            {
                // Log the violation for audit purposes.
                await _auditLoggingPort.LogEventAsync(new AuditEvent
                {
                    EventType = "GovernanceViolationDetected",
                    UserId = request.ActorId,
                    Timestamp = DateTimeOffset.UtcNow,
                    EventData = $"Policy '{violation.PolicyName}' (v{violation.PolicyVersion}) violated. Reason: {violation.ViolationMessage}",
                    CorrelationId = request.CorrelationId
                });

                // Trigger an escalation workflow (e.g., notify compliance officers).
                await _notificationPort.SendNotificationAsync(new Notification
                {
                    Subject = $"[Alert] Governance Policy Violation: {violation.PolicyName}",
                    Message = $"Action by '{request.ActorId}' violated policy '{violation.PolicyName}'. Details: {violation.ViolationMessage}",
                    Channels = new List<string> { "Email", "Slack" },
                    Recipients = new List<string> { "compliance-team@cognitivemesh.com" },
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
        }
        else
        {
            _logger.LogInformation("Governance evaluation passed for action '{ActionType}' (Correlation ID: {CorrelationId}).",
                request.ActionType, request.CorrelationId);
        }

        return response;
    }

    /// <summary>
    /// A simulated policy evaluation engine. In a real system, this would use a proper rules engine
    /// like OPA (Open Policy Agent) to evaluate policies written in Rego or a similar language.
    /// </summary>
    private PolicyViolation EvaluatePolicy(PolicyRecord policy, GovernanceEvaluationRequest request)
    {
        // This simulation checks for a few hardcoded policy rules based on the policy name.
        switch (policy.Name)
        {
            case "High-Risk Transaction Approval Policy":
                if (request.ActionType == "FinancialTransaction" &&
                    request.ActionContext.TryGetValue("amount", out var amountObj) &&
                    Convert.ToDecimal(amountObj) > 10000 &&
                    (!request.ActionContext.TryGetValue("humanApproval", out var approvalObj) || !(bool)approvalObj))
                {
                    return new PolicyViolation
                    {
                        PolicyId = policy.PolicyId,
                        PolicyName = policy.Name,
                        PolicyVersion = policy.Version,
                        ViolationMessage = "Financial transactions over $10,000 require explicit human approval, which was not provided."
                    };
                }
                break;

            case "PII Data Handling Policy":
                if (request.ActionType == "DataSharing" &&
                    request.ActionContext.TryGetValue("dataType", out var dataTypeObj) &&
                    dataTypeObj.ToString() == "PII" &&
                    (!request.ActionContext.TryGetValue("isAnonymized", out var anonObj) || !(bool)anonObj))
                {
                    return new PolicyViolation
                    {
                        PolicyId = policy.PolicyId,
                        PolicyName = policy.Name,
                        PolicyVersion = policy.Version,
                        ViolationMessage = "Sharing of Personally Identifiable Information (PII) is only permitted if it is properly anonymized."
                    };
                }
                break;
        }

        // No violation found for this policy.
        return null!;
    }
}