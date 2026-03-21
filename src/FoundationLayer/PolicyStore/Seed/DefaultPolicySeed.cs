using CognitiveMesh.FoundationLayer.PolicyStore.Models;

namespace CognitiveMesh.FoundationLayer.PolicyStore.Seed;

/// <summary>
/// Provides the default set of remediation policies used to seed the policy store.
/// </summary>
public static class DefaultPolicySeed
{
    /// <summary>
    /// Returns the full set of default remediation policies covering all
    /// incident categories (infrastructure, application, data, security) at
    /// every severity level (low, medium, high, critical).
    /// </summary>
    /// <returns>A read-only list of 16 default <see cref="RemediationPolicy"/> instances.</returns>
    public static IReadOnlyList<RemediationPolicy> GetDefaultPolicies()
    {
        var now = DateTimeOffset.UtcNow;

        return
        [
            // ── Infrastructure ──────────────────────────────────────────
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "infrastructure", Severity = "low",
                AllowedActions = RemediationAction.Retry,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 1.0 },
                MaxRetries = 5, RepeatedFailureEscalationThreshold = 10,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "infrastructure", Severity = "medium",
                AllowedActions = RemediationAction.Retry | RemediationAction.Restart,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.6, ["Restart"] = 0.4 },
                MaxRetries = 3, RepeatedFailureEscalationThreshold = 7,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "infrastructure", Severity = "high",
                AllowedActions = RemediationAction.Retry | RemediationAction.Restart | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.3, ["Restart"] = 0.4, ["Escalate"] = 0.3 },
                MaxRetries = 2, RepeatedFailureEscalationThreshold = 5,
                HumanInLoopRequired = true, ApproverRoles = ["SRE", "PlatformAdmin"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "infrastructure", Severity = "critical",
                AllowedActions = RemediationAction.Retry | RemediationAction.Rollback | RemediationAction.Restart | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.1, ["Rollback"] = 0.3, ["Restart"] = 0.2, ["Escalate"] = 0.4 },
                MaxRetries = 1, RepeatedFailureEscalationThreshold = 3,
                HumanInLoopRequired = true, ApproverRoles = ["SRE", "PlatformAdmin", "IncidentCommander"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },

            // ── Application ────────────────────────────────────────────
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "application", Severity = "low",
                AllowedActions = RemediationAction.Retry,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 1.0 },
                MaxRetries = 5, RepeatedFailureEscalationThreshold = 10,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "application", Severity = "medium",
                AllowedActions = RemediationAction.Retry | RemediationAction.Reassign,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.6, ["Reassign"] = 0.4 },
                MaxRetries = 3, RepeatedFailureEscalationThreshold = 7,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "application", Severity = "high",
                AllowedActions = RemediationAction.Retry | RemediationAction.Rollback | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.3, ["Rollback"] = 0.4, ["Escalate"] = 0.3 },
                MaxRetries = 2, RepeatedFailureEscalationThreshold = 5,
                HumanInLoopRequired = true, ApproverRoles = ["AppOwner", "TechLead"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "application", Severity = "critical",
                AllowedActions = RemediationAction.Retry | RemediationAction.Rollback | RemediationAction.Reassign | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.1, ["Rollback"] = 0.3, ["Reassign"] = 0.2, ["Escalate"] = 0.4 },
                MaxRetries = 1, RepeatedFailureEscalationThreshold = 3,
                HumanInLoopRequired = true, ApproverRoles = ["AppOwner", "TechLead", "IncidentCommander"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },

            // ── Data ───────────────────────────────────────────────────
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "data", Severity = "low",
                AllowedActions = RemediationAction.Retry,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 1.0 },
                MaxRetries = 5, RepeatedFailureEscalationThreshold = 10,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "data", Severity = "medium",
                AllowedActions = RemediationAction.Retry | RemediationAction.Rollback,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.5, ["Rollback"] = 0.5 },
                MaxRetries = 3, RepeatedFailureEscalationThreshold = 7,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "data", Severity = "high",
                AllowedActions = RemediationAction.Retry | RemediationAction.Rollback | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.2, ["Rollback"] = 0.5, ["Escalate"] = 0.3 },
                MaxRetries = 2, RepeatedFailureEscalationThreshold = 5,
                HumanInLoopRequired = true, ApproverRoles = ["DataEngineer", "DBA"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "data", Severity = "critical",
                AllowedActions = RemediationAction.Rollback | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Rollback"] = 0.5, ["Escalate"] = 0.5 },
                MaxRetries = 1, RepeatedFailureEscalationThreshold = 2,
                HumanInLoopRequired = true, ApproverRoles = ["DataEngineer", "DBA", "IncidentCommander"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },

            // ── Security ───────────────────────────────────────────────
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "security", Severity = "low",
                AllowedActions = RemediationAction.Retry | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.7, ["Escalate"] = 0.3 },
                MaxRetries = 3, RepeatedFailureEscalationThreshold = 5,
                HumanInLoopRequired = false, ApproverRoles = [],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "security", Severity = "medium",
                AllowedActions = RemediationAction.Retry | RemediationAction.Reassign | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Retry"] = 0.3, ["Reassign"] = 0.3, ["Escalate"] = 0.4 },
                MaxRetries = 2, RepeatedFailureEscalationThreshold = 4,
                HumanInLoopRequired = true, ApproverRoles = ["SecurityAnalyst"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "security", Severity = "high",
                AllowedActions = RemediationAction.Rollback | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Rollback"] = 0.4, ["Escalate"] = 0.6 },
                MaxRetries = 1, RepeatedFailureEscalationThreshold = 3,
                HumanInLoopRequired = true, ApproverRoles = ["SecurityAnalyst", "CISO"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            },
            new RemediationPolicy
            {
                Id = Guid.NewGuid(), IncidentCategory = "security", Severity = "critical",
                AllowedActions = RemediationAction.Rollback | RemediationAction.Escalate,
                RankingWeights = new Dictionary<string, double> { ["Rollback"] = 0.3, ["Escalate"] = 0.7 },
                MaxRetries = 0, RepeatedFailureEscalationThreshold = 1,
                HumanInLoopRequired = true, ApproverRoles = ["SecurityAnalyst", "CISO", "IncidentCommander"],
                CreatedAt = now, UpdatedAt = now, CreatedBy = "system"
            }
        ];
    }
}
