using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.MetacognitiveLayer.Protocols.Integration;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Common.Orchestration
{
    /// <summary>
    /// Interface for orchestrating agent execution and managing agent lifecycles.
    /// </summary>
    public interface IAgentOrchestrator
    {
        /// <summary>
        /// Executes an agent task and returns the result.
        /// </summary>
        /// <param name="request">The agent execution request</param>
        /// <returns>The result of the agent execution</returns>
        Task<AgentResult> ExecuteAgentAsync(ACPRequest request);

        /// <summary>
        /// Registers an agent with the orchestrator.
        /// </summary>
        /// <param name="agentId">The unique identifier for the agent</param>
        /// <param name="agentType">The type of agent to register</param>
        /// <param name="configuration">Configuration for the agent</param>
        /// <returns>Success indicator</returns>
        Task<bool> RegisterAgentAsync(string agentId, string agentType, Dictionary<string, object> configuration);

        /// <summary>
        /// Gets a list of available agents.
        /// </summary>
        /// <returns>Collection of available agent IDs and their types</returns>
        Task<Dictionary<string, string>> GetAvailableAgentsAsync();
    }

    /// <summary>
    /// Represents the result of an agent execution.
    /// </summary>
    public class AgentResult
    {
        /// <summary>
        /// Unique identifier for the result
        /// </summary>
        public string ResultId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Agent ID that produced this result
        /// </summary>
        public string AgentId { get; set; }

        /// <summary>
        /// Task name that was executed
        /// </summary>
        public string TaskName { get; set; }

        /// <summary>
        /// Whether the execution was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Primary output of the agent (could be text, JSON, etc.)
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// Additional structured output data
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Error message if execution failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Timestamp when execution completed
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Execution duration in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; set; }
    }
}