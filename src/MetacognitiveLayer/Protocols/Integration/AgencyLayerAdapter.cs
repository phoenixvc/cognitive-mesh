using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.ToolIntegration;
using CognitiveMesh.MetacognitiveLayer.Protocols.ACP.Models;
using CognitiveMesh.MetacognitiveLayer.Protocols.Common;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.Integration
{
    /// <summary>
    /// Adapter that integrates the Hybrid Protocol system with the AgencyLayer.
    /// Handles tool execution, task planning and coordination with the AgencyLayer components.
    /// </summary>
    public class AgencyLayerAdapter
    {
        private readonly ILogger<AgencyLayerAdapter> _logger;
        private readonly ToolRegistry _toolRegistry;
        private readonly IMeshMemoryStore _memoryStore;
        private readonly IToolRunner _toolRunner;
        private readonly IAgentOrchestrator _agentOrchestrator;
        private readonly IContextTemplateResolver _templateResolver;

        public AgencyLayerAdapter(
            ToolRegistry toolRegistry,
            IMeshMemoryStore memoryStore,
            IToolRunner toolRunner,
            IAgentOrchestrator agentOrchestrator,
            IContextTemplateResolver templateResolver,
            ILogger<AgencyLayerAdapter> logger)
        {
            _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
            _toolRunner = toolRunner ?? throw new ArgumentNullException(nameof(toolRunner));
            _agentOrchestrator = agentOrchestrator ?? throw new ArgumentNullException(nameof(agentOrchestrator));
            _templateResolver = templateResolver ?? throw new ArgumentNullException(nameof(templateResolver));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes a tool from the AgencyLayer based on the ACP task definition.
        /// </summary>
        /// <param name="task">The ACP task containing the tool execution details</param>
        /// <param name="sessionContext">Current session context</param>
        /// <returns>Result of the tool execution</returns>
        public async Task<object> ExecuteToolAsync(ACPTask task, SessionContext sessionContext)
        {
            try
            {
                _logger.LogInformation("Executing tool for task: {TaskName}, tool: {ToolName}", task.Name, task.PrimaryTool);
                
                if (string.IsNullOrEmpty(task.PrimaryTool))
                {
                    throw new InvalidOperationException("No primary tool specified in the ACP task");
                }
                
                // Create parameter dictionary from ACP parameters
                var parameters = new Dictionary<string, object>();
                foreach (var param in task.Parameters)
                {
                    object value = param.Value;
                    
                    // Resolve from context if needed
                    if (param.IsContextual && param.Value is string contextKey)
                    {
                        // Use IMeshMemoryStore for context retrieval
                        var contextValue = await _memoryStore.GetContextAsync(sessionContext.SessionId, contextKey);
                        value = contextValue ?? sessionContext.GetContextValue(contextKey);
                    }
                    
                    parameters[param.Name] = value;
                }
                
                // Create tool context
                var toolContext = new ToolContext
                {
                    SessionId = sessionContext.SessionId,
                    UserId = sessionContext.UserId
                };
                
                // Use IToolRunner to execute the tool
                var toolResult = await _toolRunner.Execute(task.PrimaryTool, parameters, toolContext);
                
                // Store result in both session context and persistent memory store
                if (toolResult != null)
                {
                    sessionContext.SetContextValue("lastToolResult", toolResult);
                    await _memoryStore.SaveContextAsync(
                        sessionContext.SessionId, 
                        $"tool_result_{task.PrimaryTool}", 
                        JsonConvert.SerializeObject(toolResult)
                    );
            }
                
                _logger.LogInformation("Tool execution completed for task: {TaskName}", task.Name);
                return toolResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing tool for task: {TaskName}", task.Name);
                
                // Handle fallback if specified
                if (task.Fallback != null)
                {
                    return HandleToolExecutionFallback(task, ex);
                }
                
                throw;
            }
        }

        /// <summary>
        /// Runs an agent based on the ACP request.
        /// </summary>
        /// <param name="agentId">The ID of the agent to run</param>
        /// <param name="task">The ACP task defining the agent request</param>
        /// <param name="sessionContext">Current session context</param>
        /// <returns>Result of the agent execution</returns>
        public async Task<object> RunAgentAsync(string agentId, ACPTask task, SessionContext sessionContext)
        {
            try
            {
                _logger.LogInformation("Running agent: {AgentId} for task: {TaskName}", agentId, task.Name);
                
                // Convert ACP task to an ACPRequest
                var acpRequest = ConvertTaskToAcpRequest(task, sessionContext);
                
                // Resolve the template using the context template resolver
                var resolvedPrompt = _templateResolver.ResolvePrompt(acpRequest);
                acpRequest.ResolvedPrompt = resolvedPrompt;
                
                // Run the agent using the orchestrator
                var agentResponse = await _agentOrchestrator.RunAgent(agentId, acpRequest);
                
                // Store agent result in session context
                sessionContext.SetContextValue($"agent_result_{agentId}", agentResponse);
                
                // Store in persistent memory store
                await _memoryStore.SaveContextAsync(
                    sessionContext.SessionId,
                    $"agent_result_{agentId}",
                    JsonConvert.SerializeObject(agentResponse)
                );
                
                _logger.LogInformation("Agent execution completed for agent: {AgentId}", agentId);
                return agentResponse;
    }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running agent: {AgentId}", agentId);
                throw;
}
        }

        /// <summary>
        /// Registers tools from the AgencyLayer with the protocol's tool registry.
        /// </summary>
        /// <returns>Number of tools registered</returns>
        public async Task<int> RegisterAgencyLayerToolsAsync()
        {
            try
            {
                _logger.LogInformation("Registering AgencyLayer tools with protocol tool registry");
                
                // Get available tools from IToolRunner
                var availableTools = await GetAvailableToolsAsync();
                
                int registered = 0;
                foreach (var tool in availableTools)
                {
                    var success = await _toolRegistry.RegisterToolAsync(
                        tool.Key, 
                        JsonConvert.SerializeObject(tool.Value)
                    );
                    
                    if (success)
                    {
                        registered++;
                    }
                }
                
                _logger.LogInformation("Registered {RegisteredCount} AgencyLayer tools with protocol tool registry", registered);
                return registered;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering AgencyLayer tools");
                throw;
            }
        }

        /// <summary>
        /// Maps the MCP context to AgencyLayer execution context.
        /// </summary>
        public Dictionary<string, object> CreateAgencyLayerContext(SessionContext sessionContext)
        {
            // Create a context object that the AgencyLayer components can understand
            var agencyContext = new Dictionary<string, object>
            {
                { "sessionId", sessionContext.SessionId },
                { "conversationId", sessionContext.ConversationId },
                { "userId", sessionContext.UserId },
                { "timestamp", DateTime.UtcNow }
            };
            
            // Add memory entries
            var memory = sessionContext.GetAllMemory();
            if (memory != null && memory.Count > 0)
            {
                agencyContext["memory"] = memory;
            }
            
            return agencyContext;
        }

        private object HandleToolExecutionFallback(ACPTask task, Exception exception)
        {
            _logger.LogWarning("Executing fallback for task: {TaskName}, fallback type: {FallbackType}", 
                task.Name, task.Fallback.Type);
            
            switch (task.Fallback.Type)
            {
                case "default":
                    return task.Fallback.DefaultResponse ?? "Task execution failed with a default fallback response.";
                
                case "error":
                    throw new InvalidOperationException($"Task execution failed: {exception.Message}", exception);
                
                default:
                    _logger.LogWarning("Unknown fallback type: {FallbackType}. Using default response.", task.Fallback.Type);
                    return task.Fallback.DefaultResponse ?? "Task execution failed.";
            }
        }

        private async Task<Dictionary<string, object>> GetAvailableToolsAsync()
        {
            // This would typically call into the IToolRunner to get available tools
            // For now, returning a sample set of tools
            return new Dictionary<string, object>
            {
                { "DataAnalysisTool", new { Type = "Analysis", Capabilities = new[] { "Statistics", "Visualization" } } },
                { "ClassificationTool", new { Type = "ML", Capabilities = new[] { "Categorization", "Labeling" } } },
                { "TextGenerationTool", new { Type = "NLP", Capabilities = new[] { "Content", "Summarization" } } },
                { "svg_generator", new { Type = "Graphics", Capabilities = new[] { "SVG", "Marketing", "Branding" } } }
            };
        }

        private ACPRequest ConvertTaskToAcpRequest(ACPTask task, SessionContext sessionContext)
        {
            // Convert the ACP task to an ACPRequest object for the agent orchestrator
            // This is a placeholder for the actual implementation
            var acpRequest = new ACPRequest
            {
                AgentId = task.PrimaryTool,
                TaskName = task.Name,
                Parameters = new Dictionary<string, object>()
            };

            // Copy parameters
            foreach (var param in task.Parameters)
            {
                object value = param.Value;
                if (param.IsContextual && param.Value is string contextKey)
                {
                    value = sessionContext.GetContextValue(contextKey);
                }
                acpRequest.Parameters[param.Name] = value;
            }

            return acpRequest;
        }
    }

    // Helper classes to match the interfaces mentioned in the requirements
    public class ToolContext
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    }

    public class ACPRequest
    {
        public string AgentId { get; set; }
        public string TaskName { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        public string ResolvedPrompt { get; set; }
    }

    public class AgentResponse
    {
        public string Content { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    // Interface definitions for the new abstractions
    public interface IMeshMemoryStore
    {
        Task SaveContextAsync(string sessionId, string key, string value);
        Task<string> GetContextAsync(string sessionId, string key);
        Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold);
    }

    public interface IToolRunner
    {
        Task<object> Execute(string toolId, Dictionary<string, object> input, ToolContext context);
    }

    public interface IAgentOrchestrator
    {
        Task<AgentResponse> RunAgent(string agentId, ACPRequest request);
    }

    public interface IContextTemplateResolver
    {
        string ResolvePrompt(ACPRequest acpRequest);
        string ApplyDSL(string acpDslTemplate, Dictionary<string, object> variables);
    }
}