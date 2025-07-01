using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- DTOs and Models for the Multi-Agent Orchestration Port ---
namespace CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models
{
    /// <summary>
    /// Defines the degree of independent decision-making an agent can exercise.
    /// This directly implements the 'Autonomy' dimension of the Agent Framework.
    /// </summary>
    public enum AutonomyLevel
    {
        /// <summary>
        /// The agent can only analyze and provide recommendations. A human or another agent must approve action.
        /// </summary>
        RecommendOnly,

        /// <summary>
        /// The agent can propose and prepare an action, but requires explicit confirmation before execution.
        /// </summary>
        ActWithConfirmation,

        /// <summary>
        /// The agent can act independently within its defined authority scope without requiring confirmation for each action.
        /// </summary>
        FullyAutonomous
    }

    /// <summary>
    /// Defines the collaboration strategy for a group of agents working on a single task.
    /// This enables the coordination of emergent behaviors.
    /// </summary>
    public enum CoordinationPattern
    {
        /// <summary>
        /// Agents work independently and their results are aggregated.
        /// </summary>
        Parallel,

        /// <summary>
        /// A lead agent directs the work of other agents in a structured workflow.
        /// </summary>
        Hierarchical,

        /// <summary>
        /// Agents compete to produce the best solution; the orchestrator selects the winner.
        /// </summary>
        Competitive,

        /// <summary>
        /// Agents work together, sharing information in real-time to achieve a collective goal.
        /// </summary>
        CollaborativeSwarm
    }

    /// <summary>
    /// Defines the specific scope and limitations of an agent's actions.
    /// This implements the 'Authority' dimension of the Agent Framework.
    /// </summary>
    public class AuthorityScope
    {
        /// <summary>
        /// A list of API endpoints or ports the agent is allowed to call.
        /// </summary>
        public List<string> AllowedApiEndpoints { get; set; } = new();

        /// <summary>
        /// Maximum computational resources (e.g., CPU seconds, memory GB) the agent can consume per task.
        /// </summary>
        public double MaxResourceConsumption { get; set; }

        /// <summary>
        /// Maximum budget the agent can expend per task, if applicable.
        /// </summary>
        public decimal MaxBudget { get; set; }

        /// <summary>
        /// The data access policies that apply to this agent (e.g., "read:pii", "write:operational-data").
        /// </summary>
        public List<string> DataAccessPolicies { get; set; } = new();
    }

    /// <summary>
    /// Represents the definition of an agent type, including its capabilities and default settings.
    /// </summary>
    public class AgentDefinition
    {
        public string AgentType { get; set; } // e.g., "ChampionNudger", "VelocityRecalibrator"
        public string Description { get; set; }
        public List<string> Capabilities { get; set; } = new();
        public AutonomyLevel DefaultAutonomyLevel { get; set; } = AutonomyLevel.RecommendOnly;
        public AuthorityScope DefaultAuthorityScope { get; set; } = new();
    }

    /// <summary>
    /// Represents a task assigned to a team of agents.
    /// </summary>
    public class AgentTask
    {
        public string TaskId { get; set; } = Guid.NewGuid().ToString();
        public string Goal { get; set; }
        public Dictionary<string, object> Context { get; set; } = new();
        public List<string> Constraints { get; set; } = new();
        public CoordinationPattern CoordinationPattern { get; set; } = CoordinationPattern.CollaborativeSwarm;
        public List<string> RequiredAgentTypes { get; set; } = new();
    }

    /// <summary>
    /// Represents a request to execute a task using a team of agents.
    /// </summary>
    public class AgentExecutionRequest
    {
        public AgentTask Task { get; set; }
        public string TenantId { get; set; }
        public string RequestingUserId { get; set; }
    }

    /// <summary>
    /// Represents the result of an executed agent task.
    /// </summary>
    public class AgentExecutionResponse
    {
        public string TaskId { get; set; }
        public bool IsSuccess { get; set; }
        public object Result { get; set; }
        public string Summary { get; set; }
        public List<string> AgentIdsInvolved { get; set; } = new();
        public string AuditTrailId { get; set; }
    }

    /// <summary>
    /// Represents a piece of knowledge or a learned model that can be shared between agents.
    /// </summary>
    public class AgentLearningInsight
    {
        public string InsightId { get; set; } = Guid.NewGuid().ToString();
        public string GeneratingAgentType { get; set; }
        public string InsightType { get; set; } // e.g., "OptimizedWorkflow", "NewRiskFactor"
        public object InsightData { get; set; }
        public double ConfidenceScore { get; set; }
    }

    /// <summary>
    /// Represents a request to dynamically spawn a new agent instance.
    /// </summary>
    public class DynamicAgentSpawnRequest
    {
        public string AgentType { get; set; }
        public string TenantId { get; set; }
        public string ParentTaskId { get; set; } // The task that requires this new agent.
        public AutonomyLevel? CustomAutonomy { get; set; }
        public AuthorityScope CustomAuthority { get; set; }
    }
}


// --- Port Interface ---
namespace CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports
{
    using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models;

    /// <summary>
    /// Defines the contract for the Multi-Agent Orchestration Port in the Agency Layer.
    /// This port is the primary entry point for coordinating complex tasks across multiple
    /// autonomous agents, adhering to the Hexagonal Architecture pattern.
    /// </summary>
    public interface IMultiAgentOrchestrationPort
    {
        /// <summary>
        /// Registers a new type of agent with the orchestration system.
        /// </summary>
        /// <param name="definition">The definition of the agent type.</param>
        Task RegisterAgentAsync(AgentDefinition definition);

        /// <summary>
        /// Executes a complex task by assembling and coordinating a team of agents.
        /// </summary>
        /// <param name="request">The request detailing the task to be executed.</param>
        /// <returns>The result of the multi-agent task execution.</returns>
        /// <remarks>
        /// **Acceptance Criteria:** Given an agent workflow, when triggered, the orchestrator makes a decision and dispatches tasks to the appropriate agents.
        /// </remarks>
        Task<AgentExecutionResponse> ExecuteTaskAsync(AgentExecutionRequest request);

        /// <summary>
        /// Dynamically sets the autonomy level for a specific agent instance or type.
        /// </summary>
        /// <param name="agentIdOrType">The identifier for the agent or agent type.</param>
        /// <param name="level">The new autonomy level to set.</param>
        /// <param name="tenantId">The tenant scope for this configuration change.</param>
        Task SetAgentAutonomyAsync(string agentIdOrType, AutonomyLevel level, string tenantId);

        /// <summary>
        /// Configures the authority scope for a specific agent instance or type.
        /// </summary>
        /// <param name="agentIdOrType">The identifier for the agent or agent type.</param>
        /// <param name="scope">The new authority scope to apply.</param>
        /// <param name="tenantId">The tenant scope for this configuration change.</param>
        Task ConfigureAgentAuthorityAsync(string agentIdOrType, AuthorityScope scope, string tenantId);

        /// <summary>
        /// Shares a learning insight with the multi-agent system, allowing other agents to benefit from it.
        /// </summary>
        /// <param name="insight">The learning insight to be shared.</param>
        Task ShareLearningInsightAsync(AgentLearningInsight insight);

        /// <summary>
        /// Dynamically creates and deploys a new agent instance to handle a specific task.
        /// </summary>
        /// <param name="request">The request to spawn a new agent.</param>
        /// <returns>The unique ID of the newly spawned agent instance.</returns>
        Task<string> SpawnAgentAsync(DynamicAgentSpawnRequest request);

        /// <summary>
        /// Retrieves the current status of an ongoing agent task.
        /// </summary>
        /// <param name="taskId">The ID of the task to query.</param>
        /// <param name="tenantId">The tenant scope for the query.</param>
        /// <returns>The current status and progress of the task.</returns>
        Task<AgentTask> GetAgentTaskStatusAsync(string taskId, string tenantId);
    }
}
