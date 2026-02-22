using System.Diagnostics;
using System.Text.Json;
using MetacognitiveLayer.Protocols.Integration;
using MetacognitiveLayer.Protocols.LLM;
using Microsoft.Extensions.Logging;
using IContextTemplateResolver = MetacognitiveLayer.Protocols.Common.Templates.IContextTemplateResolver;
using IMeshMemoryStore = MetacognitiveLayer.Protocols.Common.Memory.IMeshMemoryStore;
using IToolRunner = MetacognitiveLayer.Protocols.Common.Tools.IToolRunner;
using ToolContext = MetacognitiveLayer.Protocols.Common.Tools.ToolContext;

namespace MetacognitiveLayer.Protocols.Common.Orchestration
{
    /// <summary>
    /// Orchestrates the execution of agents using templates, LLM providers, and tools.
    /// </summary>
    public class AgentOrchestrator : IAgentOrchestrator
    {
        private readonly IContextTemplateResolver _templateResolver;
        private readonly IMeshMemoryStore _memoryStore;
        private readonly ILLMProvider _llmProvider;
        private readonly IToolRunner _toolRunner;
        private readonly ILogger<AgentOrchestrator> _logger;
        private readonly Dictionary<string, AgentRegistration> _agents = new Dictionary<string, AgentRegistration>();

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentOrchestrator"/> class.
        /// </summary>
        /// <param name="templateResolver">The context template resolver for resolving agent prompts.</param>
        /// <param name="memoryStore">The memory store for persisting agent state.</param>
        /// <param name="llmProvider">The LLM provider for generating agent responses.</param>
        /// <param name="toolRunner">The tool runner for executing agent tools.</param>
        /// <param name="logger">The logger instance.</param>
        public AgentOrchestrator(
            IContextTemplateResolver templateResolver,
            IMeshMemoryStore memoryStore,
            ILLMProvider llmProvider,
            IToolRunner toolRunner,
            ILogger<AgentOrchestrator> logger)
        {
            _templateResolver = templateResolver ?? throw new ArgumentNullException(nameof(templateResolver));
            _memoryStore = memoryStore ?? throw new ArgumentNullException(nameof(memoryStore));
            _llmProvider = llmProvider ?? throw new ArgumentNullException(nameof(llmProvider));
            _toolRunner = toolRunner ?? throw new ArgumentNullException(nameof(toolRunner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Executes an agent task and returns the result.
        /// </summary>
        public async Task<AgentResult> ExecuteAgentAsync(ACPRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new AgentResult
            {
                AgentId = request.AgentId,
                TaskName = request.TaskName
            };

            try
            {
                _logger.LogInformation("Executing agent {AgentId} for task {TaskName}", request.AgentId, request.TaskName);

                // Validate agent registration
                if (!_agents.TryGetValue(request.AgentId, out var agent))
                {
                    _logger.LogWarning("Agent {AgentId} not registered", request.AgentId);
                    result.Success = false;
                    result.ErrorMessage = $"Agent {request.AgentId} is not registered";
                    return result;
                }

                // Get session context
                var sessionId = request.Parameters.ContainsKey("session_id") 
                    ? request.Parameters["session_id"].ToString() 
                    : Guid.NewGuid().ToString();

                // Store request in memory
                await _memoryStore.SaveContextAsync(
                    sessionId!,
                    $"request:{DateTime.UtcNow.Ticks}",
                    JsonSerializer.Serialize(request)
                );

                // Resolve prompt from template
                var prompt = _templateResolver.ResolvePrompt(request);

                // Save prompt to memory
                await _memoryStore.SaveContextAsync(
                    sessionId!,
                    $"prompt:{DateTime.UtcNow.Ticks}",
                    prompt
                );

                // Execute LLM call
                var llmResponse = await _llmProvider.CompletePromptAsync(
                    prompt,
                    new LLMOptions
                    {
                        MaxTokens = agent.Configuration.ContainsKey("max_tokens") 
                            ? Convert.ToInt32(agent.Configuration["max_tokens"]) 
                            : 2000,
                        Temperature = agent.Configuration.ContainsKey("temperature") 
                            ? Convert.ToSingle(agent.Configuration["temperature"]) 
                            : 0.7f,
                        Model = agent.Configuration.ContainsKey("model")
                            ? agent.Configuration["model"].ToString()!
                            : "default"
                    }
                );

                // Save LLM response to memory
                await _memoryStore.SaveContextAsync(
                    sessionId!,
                    $"llm_response:{DateTime.UtcNow.Ticks}",
                    llmResponse
                );

                // Process tool invocations if any
                var processedResponse = await ProcessToolInvocations(llmResponse, sessionId!, request.Parameters);

                // Set result
                result.Success = true;
                result.Output = processedResponse;
                result.Data["raw_llm_response"] = llmResponse;
                result.Data["session_id"] = sessionId ?? string.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing agent {AgentId}", request.AgentId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                stopwatch.Stop();
                result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                result.Timestamp = DateTimeOffset.UtcNow;
            }

            return result;
        }

        /// <summary>
        /// Registers an agent with the orchestrator.
        /// </summary>
        public async Task<bool> RegisterAgentAsync(string agentId, string agentType, Dictionary<string, object> configuration)
        {
            try
            {
                _logger.LogInformation("Registering agent {AgentId} of type {AgentType}", agentId, agentType);

                // Create agent registration
                var agent = new AgentRegistration
                {
                    AgentId = agentId,
                    AgentType = agentType,
                    Configuration = configuration ?? new Dictionary<string, object>(),
                    RegisteredAt = DateTimeOffset.UtcNow
                };

                // Add to registry
                _agents[agentId] = agent;

                // Store in memory for persistence
                await _memoryStore.SaveContextAsync(
                    "system",
                    $"agent:{agentId}",
                    JsonSerializer.Serialize(agent)
                );

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent {AgentId}", agentId);
                return false;
            }
        }

        /// <summary>
        /// Gets a list of available agents.
        /// </summary>
        public async Task<Dictionary<string, string>> GetAvailableAgentsAsync()
        {
            await Task.CompletedTask;
            var agents = new Dictionary<string, string>();

            foreach (var agent in _agents)
            {
                agents[agent.Key] = agent.Value.AgentType;
            }

            return agents;
        }

        /// <summary>
        /// Processes any tool invocations in the LLM response.
        /// </summary>
        private async Task<string> ProcessToolInvocations(string response, string sessionId, Dictionary<string, object> parameters)
        {
            try
            {
                // Check if response contains tool invocation syntax
                if (!response.Contains("<tool:") || !response.Contains("</tool>"))
                {
                    return response;
                }

                // Simple regex-based tool invocation parser
                var pattern = @"<tool:([^>]+)>(.*?)</tool>";
                var matches = System.Text.RegularExpressions.Regex.Matches(response, pattern);

                if (matches.Count == 0)
                {
                    return response;
                }

                var result = response;
                
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    var toolId = match.Groups[1].Value.Trim();
                    var toolInputJson = match.Groups[2].Value.Trim();
                    
                    try
                    {
                        // Parse tool input
                        var toolInput = JsonSerializer.Deserialize<Dictionary<string, object>>(toolInputJson)!;
                        
                        // Execute tool
                        var toolContext = new ToolContext
                        {
                            SessionId = sessionId,
                            UserId = parameters.ContainsKey("user_id") ? parameters["user_id"].ToString()! : "system",
                            AdditionalContext = parameters
                        };

                        var toolResult = await _toolRunner.Execute(toolId, toolInput!, toolContext);
                        
                        // Replace tool invocation with result
                        var toolResultJson = JsonSerializer.Serialize(toolResult);
                        result = result.Replace(match.Value, $"<tool-result:{toolId}>\n{toolResultJson}\n</tool-result>");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing tool {ToolId}", toolId);
                        result = result.Replace(
                            match.Value, 
                            $"<tool-error:{toolId}>\n{{\n  \"error\": \"{ex.Message}\"\n}}\n</tool-error>"
                        );
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing tool invocations");
                return response;
            }
        }

        /// <summary>
        /// Agent registration information.
        /// </summary>
        private class AgentRegistration
        {
            public string AgentId { get; set; } = string.Empty;
            public string AgentType { get; set; } = string.Empty;
            public Dictionary<string, object> Configuration { get; set; } = new();
            public DateTimeOffset RegisteredAt { get; set; }
        }
    }
}