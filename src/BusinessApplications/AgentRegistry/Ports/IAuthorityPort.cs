using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models
{
    /// <summary>
    /// Represents the scope of authority granted to an agent, defining what actions
    /// it can perform and under what conditions.
    /// </summary>
    public class AuthorityScope
    {
        /// <summary>
        /// The unique identifier for this authority scope.
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// A human-readable name for this authority scope.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description of what this authority scope allows.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The list of actions permitted within this scope.
        /// </summary>
        public List<string> AllowedActions { get; set; } = new List<string>();

        /// <summary>
        /// The list of resources that can be accessed within this scope.
        /// </summary>
        public List<string> AllowedResources { get; set; } = new List<string>();

        /// <summary>
        /// The list of actions explicitly prohibited within this scope.
        /// </summary>
        public List<string> DeniedActions { get; set; } = new List<string>();

        /// <summary>
        /// Conditions that must be met for this scope to be valid (e.g., time restrictions, user presence).
        /// </summary>
        public Dictionary<string, object> Conditions { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The regulatory frameworks this scope complies with (e.g., "GDPR", "EU AI Act").
        /// </summary>
        public List<string> ComplianceFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// The tenant ID to which this scope belongs.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The timestamp when this scope was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The ID of the user who created this scope.
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// The timestamp when this scope was last updated.
        /// </summary>
        public DateTimeOffset? LastUpdatedAt { get; set; }

        /// <summary>
        /// The ID of the user who last updated this scope.
        /// </summary>
        public string LastUpdatedBy { get; set; }
    }

    /// <summary>
    /// Represents a request to validate an agent's authority to perform a specific action.
    /// </summary>
    public class AuthorityValidationRequest
    {
        /// <summary>
        /// The ID of the agent whose authority is being validated.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The action the agent is attempting to perform.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The resource the agent is attempting to access.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The tenant ID in which the action is being performed.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The ID of the user on whose behalf the agent is acting (if applicable).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Additional context for the validation.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the result of an authority validation.
    /// </summary>
    public class AuthorityValidationResult
    {
        /// <summary>
        /// Indicates whether the agent has the authority to perform the requested action.
        /// </summary>
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// The reason for the authorization decision.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The ID of the authority scope that was used for validation.
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// The regulatory frameworks that were considered during validation.
        /// </summary>
        public List<string> ApplicableFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// Any compliance issues identified during validation.
        /// </summary>
        public List<ComplianceIssue> ComplianceIssues { get; set; } = new List<ComplianceIssue>();

        /// <summary>
        /// The timestamp when the validation was performed.
        /// </summary>
        public DateTimeOffset ValidatedAt { get; set; }

        /// <summary>
        /// A unique identifier for this validation result, useful for audit trails.
        /// </summary>
        public string ValidationId { get; set; }
    }

    /// <summary>
    /// Represents a request to override an agent's authority constraints.
    /// </summary>
    public class AuthorityOverrideRequest
    {
        /// <summary>
        /// The ID of the agent whose authority is being overridden.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The action for which authority is being overridden.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The resource for which authority is being overridden.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The ID of the user requesting the override.
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// The reason for the override.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The tenant ID in which the override is being requested.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The duration for which the override should be valid.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Additional context for the override.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents an audit record of an authority-related event.
    /// </summary>
    public class AuthorityAuditRecord
    {
        /// <summary>
        /// The unique identifier for this audit record.
        /// </summary>
        public string AuditId { get; set; }

        /// <summary>
        /// The type of event (e.g., "Validation", "Override", "ScopeUpdate").
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// The ID of the agent involved.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The ID of the user involved (if applicable).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The action that was being performed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The resource that was being accessed.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// The result of the event (e.g., "Authorized", "Denied", "Overridden").
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// The reason for the result.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The ID of the authority scope that was applied.
        /// </summary>
        public string ScopeId { get; set; }

        /// <summary>
        /// The tenant ID in which the event occurred.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The timestamp when the event occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Additional details about the event.
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a user's permission to override agent authority.
    /// </summary>
    public class OverridePermission
    {
        /// <summary>
        /// The unique identifier for this permission.
        /// </summary>
        public string PermissionId { get; set; }

        /// <summary>
        /// The ID of the user who has the override permission.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The scope of the override permission (e.g., specific agents, actions, or resources).
        /// </summary>
        public Dictionary<string, object> Scope { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The tenant ID to which this permission applies.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The timestamp when this permission was granted.
        /// </summary>
        public DateTimeOffset GrantedAt { get; set; }

        /// <summary>
        /// The ID of the user who granted this permission.
        /// </summary>
        public string GrantedBy { get; set; }

        /// <summary>
        /// The timestamp when this permission expires.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// The reason this permission was granted.
        /// </summary>
        public string Reason { get; set; }
    }
}

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Ports
{
    using CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models;

    /// <summary>
    /// Defines the contract for the Authority Port in the BusinessApplications Layer.
    /// This port is responsible for managing agent authority, including querying and 
    /// overriding agent permissions and scope, adhering to the Hexagonal Architecture pattern.
    ///
    /// In the context of the Ethical & Legal Compliance Framework, this interface ensures
    /// that agent actions are validated against defined authority scopes and that all
    /// authority-related events are auditable.
    /// </summary>
    public interface IAuthorityPort
    {
        /// <summary>
        /// Configures the authority scope for an agent.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="scope">The authority scope to configure.</param>
        /// <param name="configuredBy">The ID of the user configuring the scope.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The configured authority scope.</returns>
        Task<AuthorityScope> ConfigureAgentAuthorityAsync(Guid agentId, AuthorityScope scope, string configuredBy, string tenantId);

        /// <summary>
        /// Retrieves the current authority scope for an agent.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The agent's current authority scope.</returns>
        Task<AuthorityScope> GetAgentAuthorityScopeAsync(Guid agentId, string tenantId);

        /// <summary>
        /// Creates or updates an authority scope template that can be applied to multiple agents.
        /// </summary>
        /// <param name="scope">The authority scope template to create or update.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The created or updated authority scope template.</returns>
        Task<AuthorityScope> CreateAuthorityScopeTemplateAsync(AuthorityScope scope, string tenantId);

        /// <summary>
        /// Retrieves an authority scope template by its ID.
        /// </summary>
        /// <param name="scopeId">The ID of the authority scope template.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The authority scope template.</returns>
        Task<AuthorityScope> GetAuthorityScopeTemplateAsync(string scopeId, string tenantId);

        /// <summary>
        /// Lists all authority scope templates for a tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A collection of authority scope templates.</returns>
        Task<IEnumerable<AuthorityScope>> ListAuthorityScopeTemplatesAsync(string tenantId);

        /// <summary>
        /// Applies an authority scope template to an agent.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="scopeId">The ID of the authority scope template to apply.</param>
        /// <param name="appliedBy">The ID of the user applying the template.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The applied authority scope.</returns>
        Task<AuthorityScope> ApplyAuthorityScopeTemplateAsync(Guid agentId, string scopeId, string appliedBy, string tenantId);

        /// <summary>
        /// Validates whether an agent has the authority to perform a specific action.
        /// </summary>
        /// <param name="request">The validation request.</param>
        /// <returns>The validation result.</returns>
        Task<AuthorityValidationResult> ValidateAuthorityAsync(AuthorityValidationRequest request);

        /// <summary>
        /// Validates whether an action falls within an agent's authority scope.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="action">The action to validate.</param>
        /// <param name="resource">The resource being accessed.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The validation result.</returns>
        Task<AuthorityValidationResult> ValidateActionWithinScopeAsync(Guid agentId, string action, string resource, string tenantId);

        /// <summary>
        /// Overrides an agent's authority constraints for a specific action.
        /// </summary>
        /// <param name="request">The override request.</param>
        /// <returns>True if the override was successful; otherwise, false.</returns>
        Task<bool> OverrideAuthorityAsync(AuthorityOverrideRequest request);

        /// <summary>
        /// Revokes an active authority override.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="action">The action for which the override should be revoked.</param>
        /// <param name="revokedBy">The ID of the user revoking the override.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>True if the override was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeAuthorityOverrideAsync(Guid agentId, string action, string revokedBy, string tenantId);

        /// <summary>
        /// Grants override permission to a user.
        /// </summary>
        /// <param name="permission">The override permission to grant.</param>
        /// <returns>The granted override permission.</returns>
        Task<OverridePermission> GrantOverridePermissionAsync(OverridePermission permission);

        /// <summary>
        /// Revokes override permission from a user.
        /// </summary>
        /// <param name="permissionId">The ID of the permission to revoke.</param>
        /// <param name="revokedBy">The ID of the user revoking the permission.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>True if the permission was successfully revoked; otherwise, false.</returns>
        Task<bool> RevokeOverridePermissionAsync(string permissionId, string revokedBy, string tenantId);

        /// <summary>
        /// Checks if a user has permission to override agent authority.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="action">The action for which override permission is being checked.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>True if the user has override permission; otherwise, false.</returns>
        Task<bool> HasOverridePermissionAsync(string userId, Guid agentId, string action, string tenantId);

        /// <summary>
        /// Lists all override permissions for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A collection of the user's override permissions.</returns>
        Task<IEnumerable<OverridePermission>> ListUserOverridePermissionsAsync(string userId, string tenantId);

        /// <summary>
        /// Retrieves authority audit records based on various criteria.
        /// </summary>
        /// <param name="agentId">Optional agent ID to filter by.</param>
        /// <param name="userId">Optional user ID to filter by.</param>
        /// <param name="eventType">Optional event type to filter by.</param>
        /// <param name="startTime">Optional start time for the time range.</param>
        /// <param name="endTime">Optional end time for the time range.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="maxResults">The maximum number of results to return.</param>
        /// <param name="skip">The number of results to skip (for pagination).</param>
        /// <returns>A collection of authority audit records matching the criteria.</returns>
        Task<IEnumerable<AuthorityAuditRecord>> GetAuthorityAuditRecordsAsync(
            Guid? agentId = null,
            string userId = null,
            string eventType = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null,
            string tenantId = null,
            int maxResults = 100,
            int skip = 0);

        /// <summary>
        /// Retrieves an authority audit record by its ID.
        /// </summary>
        /// <param name="auditId">The ID of the audit record.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The authority audit record.</returns>
        Task<AuthorityAuditRecord> GetAuthorityAuditRecordByIdAsync(string auditId, string tenantId);

        /// <summary>
        /// Validates the compliance of an authority scope with specific regulatory frameworks.
        /// </summary>
        /// <param name="scope">The authority scope to validate.</param>
        /// <param name="frameworks">The regulatory frameworks to validate against.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>A list of compliance issues, if any.</returns>
        Task<List<ComplianceIssue>> ValidateAuthorityScopeComplianceAsync(
            AuthorityScope scope,
            List<string> frameworks,
            string tenantId);
    }
}
