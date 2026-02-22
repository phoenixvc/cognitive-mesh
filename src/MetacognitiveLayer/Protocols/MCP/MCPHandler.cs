using System.Text.Json;
using MetacognitiveLayer.Protocols.MCP.Models;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.MCP
{
    /// <summary>
    /// Handles the Model Context Protocol (MCP) operations, including parsing,
    /// validation, and creation of MCP-compliant JSON messages.
    /// </summary>
    public class MCPHandler
    {
        private readonly MCPValidator _validator;
        private readonly ILogger<MCPHandler> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MCPHandler"/> class.
        /// </summary>
        /// <param name="validator">The MCP validator used to validate incoming requests.</param>
        /// <param name="logger">The logger instance for diagnostic output.</param>
        public MCPHandler(MCPValidator validator, ILogger<MCPHandler> logger)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Parses and validates an MCP JSON request.
        /// </summary>
        public async Task<MCPContext> ParseRequestAsync(string request)
        {
            try
            {
                _logger.LogDebug("Parsing MCP request");
                
                // Parse JSON
                using var jsonDocument = JsonDocument.Parse(request);

                // Validate against MCP schema
                var isValid = await _validator.ValidateAsync(jsonDocument);
                if (!isValid)
                {
                    _logger.LogError("Invalid MCP request format");
                    throw new InvalidOperationException("The MCP request does not conform to the required schema");
                }

                // Extract MCP context
                var mcpContext = JsonSerializer.Deserialize<MCPContext>(request);
                
                _logger.LogDebug("Successfully parsed MCP request for session {SessionId}", mcpContext?.SessionId);
                return mcpContext!;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error parsing MCP JSON request");
                throw new InvalidOperationException("Invalid MCP JSON format", ex);
            }
        }

        /// <summary>
        /// Extracts the ACP XML template from an MCP context.
        /// </summary>
        public string ExtractACPTemplate(MCPContext context)
        {
            try
            {
                _logger.LogDebug("Extracting ACP template from MCP context");
                
                // The ACP template is expected to be in the taskTemplate field
                if (string.IsNullOrEmpty(context.TaskTemplate))
                {
                    throw new InvalidOperationException("No ACP template found in MCP context");
                }
                
                return context.TaskTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting ACP template from MCP context");
                throw;
            }
        }

        /// <summary>
        /// Creates an MCP-compliant response containing ACP task results.
        /// </summary>
        public string CreateResponse(MCPContext requestContext, object taskResult)
        {
            try
            {
                _logger.LogDebug("Creating MCP response for session {SessionId}", requestContext.SessionId);
                
                var response = new MCPResponse
                {
                    SessionId = requestContext.SessionId,
                    ConversationId = requestContext.ConversationId,
                    UserId = requestContext.UserId,
                    Timestamp = DateTime.UtcNow,
                    Status = "success",
                    Result = taskResult
                };
                
                return JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating MCP response");
                throw;
            }
        }

        /// <summary>
        /// Creates an MCP-compliant error response.
        /// </summary>
        public string CreateErrorResponse(Exception exception)
        {
            _logger.LogDebug("Creating MCP error response");
            
            var errorResponse = new MCPResponse
            {
                SessionId = "error",
                Timestamp = DateTime.UtcNow,
                Status = "error",
                Error = new MCPError
                {
                    Message = exception.Message,
                    Type = exception.GetType().Name,
                    Details = exception.ToString()
                }
            };
            
            return JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}