using System;
using System.Collections.Generic;
using System.Linq;

// --- Value Objects and Enums for the Agent Definition ---
namespace CognitiveMesh.FoundationLayer.ConvenerData.ValueObjects
{
    /// <summary>
    /// Defines the degree of independent decision-making an agent can exercise without outside direction.
    /// This directly implements the 'Autonomy' dimension of the Agent Framework.
    /// </summary>
    public enum AutonomyLevel
    {
        /// <summary>
        /// The agent can only analyze and provide recommendations. A human or another agent must approve action.
        /// </summary>
        RecommendOnly,

        /// <summary>
        /// The agent can propose and prepare an action but requires explicit confirmation before execution.
        /// </summary>
        ActWithConfirmation,

        /// <summary>
        /// The agent can act independently within its defined authority scope without requiring confirmation for each action.
        /// </summary>
        FullyAutonomous
    }

    /// <summary>
    /// Represents the lifecycle status of an agent in the registry.
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>
        /// The agent is active and available for orchestration.
        /// </summary>
        Active,

        /// <summary>
        /// The agent is supported but will be retired in a future version.
        /// </summary>
        Deprecated,

        /// <summary>
        /// The agent is no longer supported and cannot be used.
        /// </summary>
        Retired
    }

    /// <summary>
    /// Defines the specific scope and limitations of an agent's actions.
    /// This is an immutable value object that implements the 'Authority' dimension of the Agent Framework.
    /// </summary>
    public record AuthorityScope
    {
        /// <summary>
        /// A list of API endpoints or ports the agent is allowed to call.
        /// </summary>
        public IReadOnlyCollection<string> AllowedApiEndpoints { get; init; } = new List<string>();

        /// <summary>
        /// Maximum computational resources (e.g., CPU seconds, memory GB) the agent can consume per task.
        /// </summary>
        public double MaxResourceConsumption { get; init; }

        /// <summary>
        /// Maximum budget the agent can expend per task, if applicable.
        /// </summary>
        public decimal MaxBudget { get; init; }

        /// <summary>
        /// The data access policies that apply to this agent (e.g., "read:pii", "write:operational-data").
        /// </summary>
        public IReadOnlyCollection<string> DataAccessPolicies { get; init; } = new List<string>();
    }
}


// --- Entity ---
namespace CognitiveMesh.FoundationLayer.ConvenerData.Entities
{
    using CognitiveMesh.FoundationLayer.ConvenerData.ValueObjects;

    /// <summary>
    /// Represents the core definition of an agent type within the Cognitive Mesh Agent Registry.
    /// This is a root aggregate entity that encapsulates the identity, capabilities, and governance
    /// boundaries for a class of agents, directly implementing the Agent Framework.
    /// </summary>
    public class AgentDefinition
    {
        /// <summary>
        /// The unique identifier for this agent definition.
        /// </summary>
        public Guid AgentId { get; private set; }

        /// <summary>
        /// A unique, machine-readable name for the agent type (e.g., "ChampionNudger", "VelocityRecalibrator").
        /// </summary>
        public string AgentType { get; private set; }

        /// <summary>
        /// The semantic version of this agent definition (e.g., "1.0.0").
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// A human-readable description of the agent's purpose and functionality.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The current lifecycle status of the agent (Active, Deprecated, Retired).
        /// </summary>
        public AgentStatus Status { get; private set; }

        /// <summary>
        /// A list of capabilities that define the agent's capacity to act on decisions.
        /// This implements the 'Agency' dimension of the Agent Framework.
        /// </summary>
        public IReadOnlyCollection<string> Capabilities { get; private set; }

        /// <summary>
        /// The default level of autonomy for agents of this type. Can be overridden by policies.
        /// </summary>
        public AutonomyLevel DefaultAutonomyLevel { get; private set; }

        /// <summary>
        /// The default authority scope for agents of this type. Can be overridden by policies.
        /// </summary>
        public AuthorityScope DefaultAuthorityScope { get; private set; }

        /// <summary>
        /// The contact person or team responsible for this agent.
        /// </summary>
        public string ContactOwner { get; private set; }

        /// <summary>
        /// The timestamp when this agent definition was first created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; }

        /// <summary>
        /// The timestamp of the last update to this agent definition.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; private set; }

        /// <summary>
        /// Private constructor for EF Core and other persistence frameworks.
        /// </summary>
        private AgentDefinition() 
        {
            // Initialize all properties to prevent null reference exceptions
            AgentType = string.Empty;
            Version = "1.0.0";
            Description = string.Empty;
            Capabilities = new List<string>();
            DefaultAuthorityScope = new AuthorityScope
            {
                AllowedApiEndpoints = new List<string>(),
                DataAccessPolicies = new List<string>(),
                MaxBudget = 0,
                MaxResourceConsumption = 0
            };
            ContactOwner = string.Empty;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Creates a new instance of an AgentDefinition.
        /// </summary>
        public AgentDefinition(
            string agentType,
            string version,
            string description,
            IEnumerable<string> capabilities,
            AutonomyLevel defaultAutonomy,
            AuthorityScope defaultAuthority,
            string contactOwner)
        {
            if (string.IsNullOrWhiteSpace(agentType)) throw new ArgumentException("AgentType is required.", nameof(agentType));
            if (string.IsNullOrWhiteSpace(version) || !System.Version.TryParse(version, out _)) throw new ArgumentException("A valid semantic version is required.", nameof(version));

            AgentId = Guid.NewGuid();
            AgentType = agentType;
            Version = version;
            Description = description;
            Capabilities = new List<string>(capabilities ?? Enumerable.Empty<string>());
            DefaultAutonomyLevel = defaultAutonomy;
            DefaultAuthorityScope = defaultAuthority ?? throw new ArgumentNullException(nameof(defaultAuthority));
            ContactOwner = contactOwner;
            Status = AgentStatus.Active;
            CreatedAt = DateTimeOffset.UtcNow;
            UpdatedAt = CreatedAt;
        }

        /// <summary>
        /// Updates the agent's definition. This method should be used for non-version-breaking changes.
        /// </summary>
        public void Update(string description, IEnumerable<string> capabilities, string contactOwner)
        {
            Description = description ?? Description;
            Capabilities = new List<string>(capabilities ?? this.Capabilities);
            ContactOwner = contactOwner ?? ContactOwner;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Marks the agent definition as deprecated, signaling it will be retired in the future.
        /// </summary>
        public void Deprecate()
        {
            if (Status == AgentStatus.Retired)
            {
                throw new InvalidOperationException("A retired agent cannot be deprecated.");
            }
            Status = AgentStatus.Deprecated;
            UpdatedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Retires the agent definition, making it unavailable for new orchestrations.
        /// </summary>
        public void Retire()
        {
            Status = AgentStatus.Retired;
            UpdatedAt = DateTimeOffset.UtcNow;
        }
    }
}
