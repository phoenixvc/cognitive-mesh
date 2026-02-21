using System.Text.Json;
using MetacognitiveLayer.Protocols.ACP.Models;
using MetacognitiveLayer.Protocols.Common;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.Integration
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AgencyLayerAdapter"/> class.
        /// </summary>
        /// <param name="toolRegistry">The tool registry for managing available tools.</param>
        /// <param name="memoryStore">The memory store for persisting and retrieving context.</param>
        /// <param name="toolRunner">The tool runner for executing tools.</param>
        /// <param name="agentOrchestrator">The agent orchestrator for running agents.</param>
        /// <param name="templateResolver">The template resolver for resolving context prompts and DSL templates.</param>
        /// <param name="logger">The logger instance.</param>
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
                        JsonSerializer.Serialize(toolResult)
                    );
            }
                
                _logger.LogInformation("Tool execution completed for task: {TaskName}", task.Name);
                return toolResult!;
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
                    JsonSerializer.Serialize(agentResponse)
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
                        JsonSerializer.Serialize(tool.Value)
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
            await Task.CompletedTask;
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

    /// <summary>
    /// Represents the context in which a tool is executed, including session and user information.
    /// </summary>
    public class ToolContext
    {
        /// <summary>Gets or sets the session identifier.</summary>
        public string SessionId { get; set; } = string.Empty;
        /// <summary>Gets or sets the user identifier.</summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>Gets or sets additional context data for the tool execution.</summary>
        public Dictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a request to execute an agent task via the Agent Communication Protocol.
    /// </summary>
    public class ACPRequest
    {
        /// <summary>Gets or sets the identifier of the agent to execute.</summary>
        public string AgentId { get; set; } = string.Empty;
        /// <summary>Gets or sets the name of the task to execute.</summary>
        public string TaskName { get; set; } = string.Empty;
        /// <summary>Gets or sets the parameters for the task execution.</summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
        /// <summary>Gets or sets the resolved prompt after template resolution.</summary>
        public string ResolvedPrompt { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the response returned by an agent after task execution.
    /// </summary>
    public class AgentResponse
    {
        /// <summary>Gets or sets the content of the agent response.</summary>
        public string Content { get; set; } = string.Empty;
        /// <summary>Gets or sets the metadata associated with the agent response.</summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Defines the contract for a memory store that persists and retrieves session context and supports similarity queries.
    /// </summary>
    public interface IMeshMemoryStore
    {
        /// <summary>Saves a context value associated with a session and key.</summary>
        Task SaveContextAsync(string sessionId, string key, string value);
        /// <summary>Retrieves a context value by session identifier and key.</summary>
        Task<string> GetContextAsync(string sessionId, string key);
        /// <summary>Queries the memory store for entries similar to the given embedding within the specified threshold.</summary>
        Task<IEnumerable<string>> QuerySimilarAsync(string embedding, float threshold);
    }

    /// <summary>
    /// Defines the contract for executing tools by identifier with given inputs and context.
    /// </summary>
    public interface IToolRunner
    {
        /// <summary>Executes the specified tool with the provided input parameters and context.</summary>
        Task<object> Execute(string toolId, Dictionary<string, object> input, ToolContext context);
    }

    /// <summary>
    /// Defines the contract for orchestrating agent execution based on ACP requests.
    /// </summary>
    public interface IAgentOrchestrator
    {
        /// <summary>Runs the specified agent with the given ACP request and returns the agent response.</summary>
        Task<AgentResponse> RunAgent(string agentId, ACPRequest request);
    }

    /// <summary>
    /// Defines the contract for resolving context templates and applying DSL transformations.
    /// </summary>
    public interface IContextTemplateResolver
    {
        /// <summary>Resolves a prompt from the given ACP request using template resolution.</summary>
        string ResolvePrompt(ACPRequest acpRequest);
        /// <summary>Applies a DSL template with the provided variables and returns the resolved string.</summary>
        string ApplyDSL(string acpDslTemplate, Dictionary<string, object> variables);
    }
}