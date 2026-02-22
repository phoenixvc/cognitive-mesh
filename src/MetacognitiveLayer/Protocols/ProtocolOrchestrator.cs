using MetacognitiveLayer.Protocols.ACP;
using MetacognitiveLayer.Protocols.Common;
using MetacognitiveLayer.Protocols.MCP;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols
{
    /// <summary>
    /// The ProtocolOrchestrator is the main entry point for the Hybrid MCP + ACP Protocol system.
    /// It coordinates interactions between the Model Context Protocol (MCP) and AI Communication Protocol (ACP)
    /// to enable seamless agent communication within the Cognitive Mesh.
    /// </summary>
    public class ProtocolOrchestrator
    {
        private readonly MCPHandler _mcpHandler;
        private readonly ACPHandler _acpHandler;
        private readonly ToolRegistry _toolRegistry;
        private readonly SessionManager _sessionManager;
        private readonly ILogger<ProtocolOrchestrator> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtocolOrchestrator"/> class.
        /// </summary>
        /// <param name="mcpHandler">The MCP handler for parsing and creating MCP messages.</param>
        /// <param name="acpHandler">The ACP handler for parsing and executing ACP templates.</param>
        /// <param name="toolRegistry">The tool registry for resolving required tools.</param>
        /// <param name="sessionManager">The session manager for managing agent sessions.</param>
        /// <param name="logger">The logger instance for diagnostic output.</param>
        public ProtocolOrchestrator(
            MCPHandler mcpHandler,
            ACPHandler acpHandler,
            ToolRegistry toolRegistry,
            SessionManager sessionManager,
            ILogger<ProtocolOrchestrator> logger)
        {
            _mcpHandler = mcpHandler ?? throw new ArgumentNullException(nameof(mcpHandler));
            _acpHandler = acpHandler ?? throw new ArgumentNullException(nameof(acpHandler));
            _toolRegistry = toolRegistry ?? throw new ArgumentNullException(nameof(toolRegistry));
            _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Processes an agent request using the hybrid protocol system.
        /// </summary>
        /// <param name="request">Raw JSON request in MCP format containing ACP XML template</param>
        /// <returns>Processed response in MCP format with embedded ACP response</returns>
        public async Task<string> ProcessAgentRequestAsync(string request)
        {
            try
            {
                _logger.LogInformation("Processing agent request using Hybrid MCP + ACP Protocol");
                
                // 1. Parse and validate MCP envelope
                var mcpContext = await _mcpHandler.ParseRequestAsync(request);
                
                // 2. Extract and validate ACP template
                var acpTemplate = _mcpHandler.ExtractACPTemplate(mcpContext);
                var acpTask = await _acpHandler.ParseTemplateAsync(acpTemplate);
                
                // 3. Create session or retrieve existing one
                var session = await _sessionManager.GetOrCreateSessionAsync(mcpContext.SessionId);
                
                // 4. Resolve tools required by the ACP template
                var tools = _toolRegistry.ResolveTools(acpTask.RequiredTools);
                
                // 5. Execute the task defined in the ACP template
                var taskResult = await _acpHandler.ExecuteTemplateAsync(acpTask, tools, session);
                
                // 6. Update session with new context
                await _sessionManager.UpdateSessionAsync(session);
                
                // 7. Wrap ACP result in MCP response
                var response = _mcpHandler.CreateResponse(mcpContext, taskResult);
                
                _logger.LogInformation("Successfully processed agent request");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing agent request");
                return _mcpHandler.CreateErrorResponse(ex);
            }
        }

        /// <summary>
        /// Registers a new tool with the protocol system.
        /// </summary>
        public Task<bool> RegisterToolAsync(string toolId, string toolDefinition)
        {
            return _toolRegistry.RegisterToolAsync(toolId, toolDefinition);
        }

        /// <summary>
        /// Loads and registers an ACP template in the template library.
        /// </summary>
        public Task<bool> RegisterTemplateAsync(string templateId, string templateXml)
        {
            return _acpHandler.RegisterTemplateAsync(templateId, templateXml);
        }
    }
}