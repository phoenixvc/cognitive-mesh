using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models
{
    /// <summary>
    /// Represents a request to register a new agent in the system.
    /// </summary>
    public class AgentRegistrationRequest
    {
        /// <summary>
        /// The unique identifier for the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The name of the agent.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the agent (e.g., "Analyst", "Researcher", "Assistant").
        /// </summary>
        public string AgentType { get; set; }

        /// <summary>
        /// A description of the agent's purpose and capabilities.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The capabilities this agent has (e.g., "DataAnalysis", "ContentGeneration").
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();

        /// <summary>
        /// The version of the agent.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The ID of the tenant that owns this agent.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The ID of the user who is registering the agent.
        /// </summary>
        public string RegisteredBy { get; set; }

        /// <summary>
        /// The default authority scope for this agent.
        /// </summary>
        public string DefaultAuthorityScope { get; set; }

        /// <summary>
        /// The default autonomy level for this agent.
        /// </summary>
        public string DefaultAutonomyLevel { get; set; }

        /// <summary>
        /// The regulatory frameworks this agent claims compliance with (e.g., "GDPR", "EU AI Act").
        /// </summary>
        public List<string> ComplianceFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// Additional metadata about the agent, including compliance-related information.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents an agent entity with its full details.
    /// </summary>
    public class Agent
    {
        /// <summary>
        /// The unique identifier for the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The name of the agent.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of the agent (e.g., "Analyst", "Researcher", "Assistant").
        /// </summary>
        public string AgentType { get; set; }

        /// <summary>
        /// A description of the agent's purpose and capabilities.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The capabilities this agent has (e.g., "DataAnalysis", "ContentGeneration").
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();

        /// <summary>
        /// The version of the agent.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The ID of the tenant that owns this agent.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The ID of the user who registered the agent.
        /// </summary>
        public string RegisteredBy { get; set; }

        /// <summary>
        /// The timestamp when the agent was registered.
        /// </summary>
        public DateTimeOffset RegisteredAt { get; set; }

        /// <summary>
        /// The ID of the user who last updated the agent.
        /// </summary>
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// The timestamp when the agent was last updated.
        /// </summary>
        public DateTimeOffset? LastUpdatedAt { get; set; }

        /// <summary>
        /// The default authority scope for this agent.
        /// </summary>
        public string DefaultAuthorityScope { get; set; }

        /// <summary>
        /// The default autonomy level for this agent.
        /// </summary>
        public string DefaultAutonomyLevel { get; set; }

        /// <summary>
        /// Indicates whether the agent is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// The regulatory frameworks this agent claims compliance with (e.g., "GDPR", "EU AI Act").
        /// </summary>
        public List<string> ComplianceFrameworks { get; set; } = new List<string>();

        /// <summary>
        /// The current compliance status of the agent.
        /// </summary>
        public AgentComplianceStatus ComplianceStatus { get; set; }

        /// <summary>
        /// Additional metadata about the agent, including compliance-related information.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents the compliance status of an agent.
    /// </summary>
    public class AgentComplianceStatus
    {
        /// <summary>
        /// Indicates whether the agent is compliant with all applicable regulations.
        /// </summary>
        public bool IsCompliant { get; set; }

        /// <summary>
        /// The timestamp when the compliance status was last verified.
        /// </summary>
        public DateTimeOffset LastVerifiedAt { get; set; }

        /// <summary>
        /// The ID of the user or system that performed the last verification.
        /// </summary>
        public string VerifiedBy { get; set; }

        /// <summary>
        /// A list of compliance issues that need to be addressed.
        /// </summary>
        public List<ComplianceIssue> ComplianceIssues { get; set; } = new List<ComplianceIssue>();

        /// <summary>
        /// A dictionary of compliance certifications, keyed by framework name.
        /// </summary>
        public Dictionary<string, ComplianceCertification> Certifications { get; set; } = new Dictionary<string, ComplianceCertification>();
    }

    /// <summary>
    /// Represents a compliance issue that needs to be addressed.
    /// </summary>
    public class ComplianceIssue
    {
        /// <summary>
        /// The unique identifier for the compliance issue.
        /// </summary>
        public string IssueId { get; set; }

        /// <summary>
        /// The regulatory framework related to this issue (e.g., "GDPR", "EU AI Act").
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// The specific article or section of the regulation.
        /// </summary>
        public string RegulatoryReference { get; set; }

        /// <summary>
        /// A description of the compliance issue.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The severity of the issue (e.g., "Low", "Medium", "High", "Critical").
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// The timestamp when the issue was identified.
        /// </summary>
        public DateTimeOffset IdentifiedAt { get; set; }

        /// <summary>
        /// Suggested remediation steps to address the issue.
        /// </summary>
        public string RemediationSteps { get; set; }
    }

    /// <summary>
    /// Represents a compliance certification for a specific regulatory framework.
    /// </summary>
    public class ComplianceCertification
    {
        /// <summary>
        /// The unique identifier for the certification.
        /// </summary>
        public string CertificationId { get; set; }

        /// <summary>
        /// The regulatory framework this certification applies to.
        /// </summary>
        public string Framework { get; set; }

        /// <summary>
        /// The status of the certification (e.g., "Pending", "Approved", "Rejected").
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The timestamp when the certification was issued.
        /// </summary>
        public DateTimeOffset IssuedAt { get; set; }

        /// <summary>
        /// The timestamp when the certification expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// The ID of the authority that issued the certification.
        /// </summary>
        public string IssuedBy { get; set; }

        /// <summary>
        /// A link or reference to the certification evidence.
        /// </summary>
        public string Evidence { get; set; }
    }

    /// <summary>
    /// Represents a request to verify an agent's compliance with specific regulations.
    /// </summary>
    public class ComplianceVerificationRequest
    {
        /// <summary>
        /// The ID of the agent to verify.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The regulatory frameworks to verify against (e.g., "GDPR", "EU AI Act").
        /// If null or empty, all applicable frameworks will be checked.
        /// </summary>
        public List<string> Frameworks { get; set; } = new List<string>();

        /// <summary>
        /// The ID of the user requesting the verification.
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// Additional context for the verification.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Criteria for searching agents.
    /// </summary>
    public class AgentSearchCriteria
    {
        /// <summary>
        /// Filter by agent type.
        /// </summary>
        public string AgentType { get; set; }

        /// <summary>
        /// Filter by tenant ID.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Filter by compliance framework.
        /// </summary>
        public string ComplianceFramework { get; set; }

        /// <summary>
        /// Filter by compliance status.
        /// </summary>
        public bool? IsCompliant { get; set; }

        /// <summary>
        /// Filter by capability.
        /// </summary>
        public string Capability { get; set; }

        /// <summary>
        /// Include inactive agents in the results.
        /// </summary>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; } = 100;

        /// <summary>
        /// Number of results to skip (for pagination).
        /// </summary>
        public int Skip { get; set; }
    }
}

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Ports
{
    using CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models;

    /// <summary>
    /// Defines the contract for the Agent Registry Port in the BusinessApplications Layer.
    /// This port is the primary entry point for registering, querying, and managing agent 
    /// definitions, adhering to the Hexagonal Architecture pattern.
    ///
    /// In the context of the Ethical & Legal Compliance Framework (P0), this interface
    /// is extended to include methods for verifying agent compliance with various
    /// regulatory and ethical standards, ensuring that agents operate within defined
    /// legal and ethical boundaries.
    ///
    /// The Agent Registry serves as a central repository for all agent definitions and
    /// their compliance statuses, enabling comprehensive governance, audit trails, and
    /// regulatory reporting across the Cognitive Mesh platform.
    /// </summary>
    public interface IAgentRegistryPort
    {
        /// <summary>
        /// Registers a new agent in the system with compliance metadata.
        /// </summary>
        /// <param name="request">The registration request containing agent details and compliance claims.</param>
        /// <returns>The registered agent with its assigned ID and initial compliance status.</returns>
        /// <remarks>
        /// This method performs initial validation of the agent's compliance claims against
        /// the specified regulatory frameworks. For a full compliance verification, use
        /// <see cref="VerifyAgentComplianceAsync"/> after registration.
        /// </remarks>
        Task<Agent> RegisterAgentAsync(AgentRegistrationRequest request);

        /// <summary>
        /// Retrieves an agent by its ID.
        /// </summary>
        /// <param name="agentId">The ID of the agent to retrieve.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agent.</param>
        /// <returns>The agent if found; otherwise, null.</returns>
        Task<Agent> GetAgentByIdAsync(Guid agentId, string tenantId);

        /// <summary>
        /// Updates an existing agent's definition and metadata.
        /// </summary>
        /// <param name="agent">The updated agent definition.</param>
        /// <param name="updatedBy">The ID of the user performing the update.</param>
        /// <returns>The updated agent with refreshed compliance status.</returns>
        /// <remarks>
        /// Updates that affect compliance-related attributes will trigger an automatic
        /// re-evaluation of the agent's compliance status.
        /// </remarks>
        Task<Agent> UpdateAgentAsync(Agent agent, string updatedBy);

        /// <summary>
        /// Deactivates an agent, preventing it from being used while preserving its history.
        /// </summary>
        /// <param name="agentId">The ID of the agent to deactivate.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agent.</param>
        /// <param name="deactivatedBy">The ID of the user performing the deactivation.</param>
        /// <param name="reason">The reason for deactivation.</param>
        /// <returns>True if the agent was successfully deactivated; otherwise, false.</returns>
        Task<bool> DeactivateAgentAsync(Guid agentId, string tenantId, string deactivatedBy, string reason);

        /// <summary>
        /// Retrieves agents of a specific type.
        /// </summary>
        /// <param name="agentType">The type of agents to retrieve.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agents.</param>
        /// <param name="includeInactive">Whether to include inactive agents in the results.</param>
        /// <returns>A collection of agents of the specified type.</returns>
        Task<IEnumerable<Agent>> GetAgentsByTypeAsync(string agentType, string tenantId, bool includeInactive = false);

        /// <summary>
        /// Retrieves agents based on their compliance status with a specific framework.
        /// </summary>
        /// <param name="framework">The regulatory framework to check compliance with (e.g., "GDPR", "EU AI Act").</param>
        /// <param name="isCompliant">If true, returns compliant agents; if false, returns non-compliant agents.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agents.</param>
        /// <returns>A collection of agents with the specified compliance status.</returns>
        Task<IEnumerable<Agent>> GetAgentsByComplianceStatusAsync(string framework, bool isCompliant, string tenantId);

        /// <summary>
        /// Searches for agents based on various criteria.
        /// </summary>
        /// <param name="criteria">The search criteria.</param>
        /// <returns>A collection of agents matching the criteria.</returns>
        Task<IEnumerable<Agent>> SearchAgentsAsync(AgentSearchCriteria criteria);

        /// <summary>
        /// Verifies an agent's compliance with specific regulatory frameworks.
        /// </summary>
        /// <param name="request">The verification request specifying the agent and frameworks to verify.</param>
        /// <returns>The updated agent with its refreshed compliance status.</returns>
        /// <remarks>
        /// This method performs a comprehensive compliance check against the specified
        /// frameworks, identifying any issues that need to be addressed. The verification
        /// results are stored in the agent's compliance history for audit purposes.
        /// </remarks>
        Task<Agent> VerifyAgentComplianceAsync(ComplianceVerificationRequest request);

        /// <summary>
        /// Retrieves the compliance history for an agent.
        /// </summary>
        /// <param name="agentId">The ID of the agent.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agent.</param>
        /// <param name="framework">Optional framework to filter the history by.</param>
        /// <param name="startTime">Optional start time for the history range.</param>
        /// <param name="endTime">Optional end time for the history range.</param>
        /// <returns>A chronological history of the agent's compliance status changes.</returns>
        Task<IEnumerable<AgentComplianceStatus>> GetAgentComplianceHistoryAsync(
            Guid agentId,
            string tenantId,
            string framework = null,
            DateTimeOffset? startTime = null,
            DateTimeOffset? endTime = null);

        /// <summary>
        /// Requests a formal certification of an agent's compliance with a specific framework.
        /// </summary>
        /// <param name="agentId">The ID of the agent to certify.</param>
        /// <param name="tenantId">The ID of the tenant that owns the agent.</param>
        /// <param name="framework">The regulatory framework for which to request certification.</param>
        /// <param name="requestedBy">The ID of the user requesting the certification.</param>
        /// <returns>The certification request details, including its status and next steps.</returns>
        /// <remarks>
        /// Certification is a formal process that may involve third-party validation or
        /// regulatory authority approval. The process may take time to complete, and the
        /// initial status will typically be "Pending".
        /// </remarks>
        Task<ComplianceCertification> RequestComplianceCertificationAsync(
            Guid agentId,
            string tenantId,
            string framework,
            string requestedBy);
    }
}
