using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Models;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Ports
{
    /// <summary>
    /// Extends the base IConsentPort with agent-specific consent operations.
    /// This port handles consent flows specifically for agent operations, including
    /// authority escalations, high-risk actions, and sensitive data access.
    /// </summary>
    public interface IAgentConsentPort : IConsentPort
    {
        /// <summary>
        /// Records agent-specific consent with additional agent context.
        /// </summary>
        /// <param name="request">The consent request</param>
        /// <param name="agentId">The ID of the agent for which consent is being granted</param>
        /// <param name="agentAction">The specific action the agent is requesting consent for</param>
        /// <returns>The created consent record</returns>
        /// <remarks>
        /// **Acceptance Criteria:** Given a user action, when agent consent is requested,
        /// the consent flow is logged with agent context before the action proceeds.
        /// </remarks>
        Task<ConsentRecord> RecordAgentConsentAsync(ConsentRequest request, Guid agentId, string agentAction);

        /// <summary>
        /// Validates if a user has granted consent for a specific agent operation.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="consentType">The type of consent required (use AgentConsentTypes constants)</param>
        /// <returns>A response indicating if consent is granted</returns>
        Task<ValidateConsentResponse> ValidateAgentConsentAsync(string userId, string tenantId, Guid agentId, string consentType);

        /// <summary>
        /// Validates multiple consent requirements in a single batch operation.
        /// </summary>
        /// <param name="requests">The collection of consent validation requests</param>
        /// <returns>A dictionary mapping each request to its validation response</returns>
        Task<Dictionary<ValidateConsentRequest, ValidateConsentResponse>> ValidateConsentBatchAsync(IEnumerable<ValidateConsentRequest> requests);

        /// <summary>
        /// Gets the consent history for a specific agent.
        /// </summary>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="startDate">Optional start date for filtering</param>
        /// <param name="endDate">Optional end date for filtering</param>
        /// <returns>A collection of consent records related to the agent</returns>
        Task<IEnumerable<AgentConsentRecord>> GetAgentConsentHistoryAsync(
            Guid agentId, 
            string tenantId, 
            DateTimeOffset? startDate = null, 
            DateTimeOffset? endDate = null);

        /// <summary>
        /// Sets default consent preferences for agent operations.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="preferences">The consent preferences to set</param>
        /// <returns>True if the preferences were successfully set; otherwise, false</returns>
        Task<bool> SetAgentConsentPreferencesAsync(string userId, string tenantId, AgentConsentPreferences preferences);

        /// <summary>
        /// Gets the current consent preferences for agent operations.
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <returns>The user's agent consent preferences</returns>
        Task<AgentConsentPreferences> GetAgentConsentPreferencesAsync(string userId, string tenantId);

        /// <summary>
        /// Revokes all consents for a specific agent.
        /// </summary>
        /// <param name="userId">The ID of the user revoking consent</param>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="agentId">The ID of the agent</param>
        /// <returns>True if the revocation was successful; otherwise, false</returns>
        Task<bool> RevokeAllAgentConsentsAsync(string userId, string tenantId, Guid agentId);

        /// <summary>
        /// Checks if an emergency override of consent is active for critical operations.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="agentId">The ID of the agent</param>
        /// <returns>True if emergency override is active; otherwise, false</returns>
        Task<bool> IsEmergencyOverrideActiveAsync(string tenantId, Guid agentId);

        /// <summary>
        /// Activates an emergency override of consent for critical operations.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant</param>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="requestingUserId">The ID of the user requesting the override</param>
        /// <param name="reason">The reason for the emergency override</param>
        /// <param name="duration">The duration of the override</param>
        /// <returns>True if the override was successfully activated; otherwise, false</returns>
        Task<bool> ActivateEmergencyOverrideAsync(
            string tenantId, 
            Guid agentId, 
            string requestingUserId, 
            string reason, 
            TimeSpan duration);
    }

    /// <summary>
    /// Extends the base ConsentRecord with agent-specific context.
    /// </summary>
    public class AgentConsentRecord : ConsentRecord
    {
        /// <summary>
        /// The ID of the agent for which consent was granted or denied.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The specific action the agent was requesting consent for.
        /// </summary>
        public string AgentAction { get; set; }

        /// <summary>
        /// The authority level at the time consent was requested.
        /// </summary>
        public string AuthorityLevel { get; set; }

        /// <summary>
        /// Additional context about the agent operation.
        /// </summary>
        public Dictionary<string, object> OperationContext { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a user's preferences for agent consent.
    /// </summary>
    public class AgentConsentPreferences
    {
        /// <summary>
        /// The ID of the user.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The ID of the tenant.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Whether to automatically grant consent for low-risk operations.
        /// </summary>
        public bool AutoConsentLowRisk { get; set; }

        /// <summary>
        /// Whether to remember consent decisions for similar operations.
        /// </summary>
        public bool RememberDecisions { get; set; }

        /// <summary>
        /// The expiration period for remembered consent decisions.
        /// </summary>
        public TimeSpan? ConsentExpiration { get; set; }

        /// <summary>
        /// Agent-specific consent preferences.
        /// </summary>
        public Dictionary<Guid, AgentSpecificPreferences> AgentPreferences { get; set; } = new Dictionary<Guid, AgentSpecificPreferences>();

        /// <summary>
        /// Consent type-specific preferences.
        /// </summary>
        public Dictionary<string, bool> ConsentTypePreferences { get; set; } = new Dictionary<string, bool>();
    }

    /// <summary>
    /// Represents preferences for a specific agent.
    /// </summary>
    public class AgentSpecificPreferences
    {
        /// <summary>
        /// The ID of the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// Whether to trust this agent for all operations.
        /// </summary>
        public bool TrustAgent { get; set; }

        /// <summary>
        /// Whether to block this agent from all operations.
        /// </summary>
        public bool BlockAgent { get; set; }

        /// <summary>
        /// Specific consent types that are pre-approved for this agent.
        /// </summary>
        public List<string> PreApprovedConsentTypes { get; set; } = new List<string>();
    }
}
