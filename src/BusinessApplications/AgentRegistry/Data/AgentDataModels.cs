using System;
using System.Collections.Generic;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Data
{
    /// <summary>
    /// Represents the definition of an agent type, including its capabilities and default settings.
    /// Used internally by the AgentRegistryService for persistence.
    /// </summary>
    public class AgentDefinition
    {
        /// <summary>
        /// Unique identifier for the agent definition.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The type of the agent (e.g., "ChampionNudger", "VelocityRecalibrator").
        /// </summary>
        public string AgentType { get; set; } = string.Empty;

        /// <summary>
        /// A description of the agent.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The capabilities of the agent.
        /// </summary>
        public List<string> Capabilities { get; set; } = new List<string>();

        /// <summary>
        /// The default autonomy level for the agent.
        /// </summary>
        public AutonomyLevel DefaultAutonomyLevel { get; set; } = AutonomyLevel.RecommendOnly;

        /// <summary>
        /// The default authority scope for the agent.
        /// </summary>
        public AuthorityScope DefaultAuthorityScope { get; set; } = new AuthorityScope();

        /// <summary>
        /// Lifecycle status of the agent.
        /// </summary>
        public AgentStatus Status { get; set; } = AgentStatus.Active;
    }

    /// <summary>
    /// Defines the degree of independent decision-making an agent can exercise.
    /// </summary>
    public enum AutonomyLevel
    {
        /// <summary>
        /// The agent can only analyze and provide recommendations.
        /// </summary>
        RecommendOnly,

        /// <summary>
        /// The agent can propose and prepare an action, but requires confirmation.
        /// </summary>
        ActWithConfirmation,

        /// <summary>
        /// The agent can act independently within its defined authority scope.
        /// </summary>
        FullyAutonomous
    }

    /// <summary>
    /// Represents the lifecycle status of an agent within the registry.
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>
        /// Agent is fully supported and available.
        /// </summary>
        Active,

        /// <summary>
        /// Agent is still usable but scheduled for removal.
        /// </summary>
        Deprecated,

        /// <summary>
        /// Agent is no longer available.
        /// </summary>
        Retired
    }

    /// <summary>
    /// Represents a version record for an agent definition.
    /// </summary>
    public class AgentVersionRecord
    {
        /// <summary>
        /// The ID of the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The version string (semantic versioning).
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// When this version was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// The status at this version.
        /// </summary>
        public AgentStatus Status { get; set; }

        /// <summary>
        /// The user who created this version.
        /// </summary>
        public string CreatedBy { get; set; } = string.Empty;

        /// <summary>
        /// Description of the changes in this version.
        /// </summary>
        public string ChangeDescription { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a deprecation notice for an agent.
    /// </summary>
    public class DeprecationNotice
    {
        /// <summary>
        /// The reason for deprecation.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The date when the agent will be retired.
        /// </summary>
        public DateTimeOffset SunsetDate { get; set; }

        /// <summary>
        /// The recommended replacement agent type, if any.
        /// </summary>
        public string? ReplacementAgentType { get; set; }

        /// <summary>
        /// Migration instructions for consumers.
        /// </summary>
        public string? MigrationInstructions { get; set; }
    }

    /// <summary>
    /// Criteria for searching agent definitions.
    /// Used internally by the AgentRegistryService.
    /// </summary>
    public class AgentSearchCriteria
    {
        /// <summary>
        /// Whether to return only active agents.
        /// </summary>
        public bool ActiveOnly { get; set; }

        /// <summary>
        /// Required capabilities for matching agents.
        /// </summary>
        public List<string>? RequiredCapabilities { get; set; }

        /// <summary>
        /// Agent types to filter by.
        /// </summary>
        public List<string>? AgentTypes { get; set; }

        /// <summary>
        /// Minimum autonomy level required.
        /// </summary>
        public AutonomyLevel? MinimumAutonomyLevel { get; set; }

        /// <summary>
        /// Free-text search term.
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Maximum number of results to return.
        /// </summary>
        public int MaxResults { get; set; } = 100;
    }

    /// <summary>
    /// Represents the result of an agent definition validation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the validation passed.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Validation error messages.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Validation warning messages.
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
