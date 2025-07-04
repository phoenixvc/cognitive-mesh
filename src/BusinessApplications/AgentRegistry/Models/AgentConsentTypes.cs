using System;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Models
{
    /// <summary>
    /// Defines standardized consent types for agent operations in the Cognitive Mesh platform.
    /// These consent types are used with the IAgentConsentPort to enforce user approval for various
    /// agent operations based on their sensitivity and potential impact.
    /// </summary>
    public static class AgentConsentTypes
    {
        // --- Core Agent Operation Consent Types ---

        /// <summary>
        /// Consent for an agent to access and process user data for basic operations.
        /// This is the minimum consent level required for any agent to operate on user data.
        /// </summary>
        public const string BasicDataAccess = "Agent.BasicDataAccess";

        /// <summary>
        /// Consent for an agent to make decisions that may have business impact.
        /// Required for any agent that can make decisions affecting business processes.
        /// </summary>
        public const string BusinessDecisionMaking = "Agent.BusinessDecisionMaking";

        /// <summary>
        /// Consent for an agent to access sensitive or personally identifiable information.
        /// This consent should be explicitly requested before accessing PII or other sensitive data.
        /// </summary>
        public const string SensitiveDataAccess = "Agent.SensitiveDataAccess";

        /// <summary>
        /// Consent for an agent to perform actions with financial implications.
        /// Required before an agent can initiate transactions or make financial decisions.
        /// </summary>
        public const string FinancialOperations = "Agent.FinancialOperations";

        /// <summary>
        /// Consent for an agent to communicate externally on behalf of the user or organization.
        /// Required before sending emails, messages, or other communications to external parties.
        /// </summary>
        public const string ExternalCommunication = "Agent.ExternalCommunication";

        // --- Authority Escalation Consent Types ---

        /// <summary>
        /// Consent for an agent to temporarily elevate its authority level.
        /// This consent is required when an agent needs to perform actions beyond its default authority scope.
        /// </summary>
        public const string AuthorityEscalation = "Agent.AuthorityEscalation";

        /// <summary>
        /// Consent for an agent to operate in fully autonomous mode without further approvals.
        /// This is the highest level of consent and should be granted with caution.
        /// </summary>
        public const string FullAutonomy = "Agent.FullAutonomy";

        /// <summary>
        /// Consent for an agent to override safety constraints in emergency situations.
        /// This consent should only be requested and granted in exceptional circumstances.
        /// </summary>
        public const string EmergencyOverride = "Agent.EmergencyOverride";

        // --- Specialized Agent Consent Types ---

        /// <summary>
        /// Consent for an agent to execute code or scripts in the user's environment.
        /// Required before an agent can run potentially impactful code.
        /// </summary>
        public const string CodeExecution = "Agent.CodeExecution";

        /// <summary>
        /// Consent for an agent to modify system configurations or settings.
        /// Required before an agent can change system parameters that may affect stability.
        /// </summary>
        public const string SystemConfiguration = "Agent.SystemConfiguration";

        /// <summary>
        /// Consent for an agent to delegate tasks to other agents.
        /// Required before an agent can orchestrate multi-agent workflows.
        /// </summary>
        public const string AgentDelegation = "Agent.AgentDelegation";

        // --- Authority Level Constants ---

        /// <summary>
        /// Authority level where agents can only make recommendations but not take actions.
        /// </summary>
        public const string AuthorityLevel_RecommendOnly = "RecommendOnly";

        /// <summary>
        /// Authority level where agents can take actions but require confirmation for each step.
        /// </summary>
        public const string AuthorityLevel_ActWithConfirmation = "ActWithConfirmation";

        /// <summary>
        /// Authority level where agents can take actions within their defined scope without confirmation.
        /// </summary>
        public const string AuthorityLevel_ActWithinScope = "ActWithinScope";

        /// <summary>
        /// Authority level where agents can operate fully autonomously within their capability domain.
        /// </summary>
        public const string AuthorityLevel_FullyAutonomous = "FullyAutonomous";

        // --- Consent Scope Identifiers ---

        /// <summary>
        /// Scope identifier for session-only consent that expires when the user session ends.
        /// </summary>
        public const string Scope_SessionOnly = "session";

        /// <summary>
        /// Scope identifier for persistent consent that remains valid until explicitly revoked.
        /// </summary>
        public const string Scope_Persistent = "persistent";

        /// <summary>
        /// Scope identifier for time-limited consent that expires after a specified duration.
        /// </summary>
        public const string Scope_TimeLimited = "time-limited";

        /// <summary>
        /// Determines if the given consent type requires admin approval or can be granted by regular users.
        /// </summary>
        /// <param name="consentType">The consent type to check</param>
        /// <returns>True if admin approval is required; otherwise, false</returns>
        public static bool RequiresAdminApproval(string consentType)
        {
            return consentType switch
            {
                EmergencyOverride => true,
                SystemConfiguration => true,
                FullAutonomy => true,
                _ => false
            };
        }

        /// <summary>
        /// Gets the default expiration timespan for a given consent type.
        /// </summary>
        /// <param name="consentType">The consent type</param>
        /// <returns>The recommended expiration timespan, or null if the consent doesn't expire by default</returns>
        public static TimeSpan? GetDefaultExpiration(string consentType)
        {
            return consentType switch
            {
                BasicDataAccess => TimeSpan.FromDays(90),
                SensitiveDataAccess => TimeSpan.FromDays(30),
                AuthorityEscalation => TimeSpan.FromHours(24),
                EmergencyOverride => TimeSpan.FromHours(4),
                FullAutonomy => TimeSpan.FromHours(8),
                _ => null // No default expiration
            };
        }
    }
}
