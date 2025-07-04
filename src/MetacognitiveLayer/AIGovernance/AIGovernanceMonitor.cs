using CognitiveMesh.BusinessApplications.Compliance.Ports;
using CognitiveMesh.FoundationLayer.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CognitiveMesh.MetacognitiveLayer.AIGovernance.Models
{
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
        public string ActionType { get; set; }

        /// <summary>
        /// The ID of the agent or service performing the action.
        /// </summary>
        public string ActorId { get; set; }

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
        public string CorrelationId { get; set; }

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
        public string PolicyId { get; set; }

        /// <summary>
        /// The name of the violated policy.
        /// </summary>
        public string PolicyName { get; set; }

        /// <summary>
        /// The version of the policy that was active at the time of violation.
        /// </summary>
        public int PolicyVersion { get; set; }

        /// <summary>
        /// A detailed message explaining the nature of the violation.
        /// </summary>
        public string ViolationMessage { get; set; }
    }
}

namespace CognitiveMesh.MetacognitiveLayer.AIGovernance.Ports
{
    using CognitiveMesh.MetacognitiveLayer.AIGovernance.Models;

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
}

namespace CognitiveMesh.MetacognitiveLayer.AIGovernance
{
    using CognitiveMesh.MetacognitiveLayer.AIGovernance.Models;
    using CognitiveMesh.MetacognitiveLayer.AIGovernance.Ports;

    /// <summary>
    /// Implements the AI Governance Monitor, a critical metacognitive service that ensures all AI actions
    /// comply with active governance policies. It provides real-time policy evaluation, violation detection,
    /// and automated escalation, forming a key part of the Ethical & Legal Compliance Framework.
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
                        SubjectId = request.ActorId,
                        Timestamp = DateTimeOffset.UtcNow,
                        Details = $"Policy '{violation.PolicyName}' (v{violation.PolicyVersion}) violated. Reason: {violation.ViolationMessage}",
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
            return null;
        }
    }
}
