using System;
using System.Collections.Generic;

namespace CognitiveMesh.Shared.Models
{
    /// <summary>
    /// Represents a policy configuration for routing decisions.
    /// </summary>
    public class PolicyConfiguration
    {
        /// <summary>
        /// Gets or sets the unique identifier for the policy.
        /// </summary>
        public required string PolicyId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the policy.
        /// </summary>
        public required string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the version of the policy.
        /// </summary>
        public required string PolicyVersion { get; set; }

        /// <summary>
        /// Gets or sets whether the policy is currently active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets when the policy was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the policy was last updated.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the list of routing rules for the policy.
        /// </summary>
        public List<RoutingRule> Rules { get; set; } = new();
    }

    /// <summary>
    /// Represents a routing rule within a policy.
    /// </summary>
    public class RoutingRule
    {
        /// <summary>
        /// Gets or sets the rule ID.
        /// </summary>
        public required string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the priority of the rule (lower numbers have higher priority).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the condition that must be met for this rule to apply.
        /// </summary>
        public required string Condition { get; set; }

        /// <summary>
        /// Gets or sets the action to take when this rule matches.
        /// </summary>
        public required string Action { get; set; }

        /// <summary>
        /// Gets or sets the target of the action (e.g., a specific service or endpoint).
        /// </summary>
        public required string Target { get; set; }
    }
}
