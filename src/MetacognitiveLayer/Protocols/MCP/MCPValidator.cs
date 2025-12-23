using System.Text.Json;
using Microsoft.Extensions.Logging;

// Using JsonSchema.Net library for validation
namespace MetacognitiveLayer.Protocols.MCP
{
    /// <summary>
    /// Validates MCP (Model Context Protocol) JSON messages against the schema.
    /// </summary>
    public class MCPValidator
    {
        private readonly ILogger<MCPValidator> _logger;
        private JsonSchema _mcpSchema;

        public MCPValidator(ILogger<MCPValidator> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeSchema();
        }

        /// <summary>
        /// Initializes the MCP JSON schema for validation.
        /// </summary>
        private void InitializeSchema()
        {
            try
            {
                _logger.LogInformation("Initializing MCP schema");
                
                // Define the MCP schema
                var schemaJson = @"{
                    '$schema': 'http://json-schema.org/draft-07/schema#',
                    'type': 'object',
                    'required': ['sessionId', 'taskTemplate', 'protocolVersion'],
                    'properties': {
                        'sessionId': {
                            'type': 'string',
                            'minLength': 1
                        },
                        'conversationId': {
                            'type': 'string'
                        },
                        'userId': {
                            'type': 'string'
                        },
                        'userRole': {
                            'type': 'string'
                        },
                        'timestamp': {
                            'type': 'string',
                            'format': 'date-time'
                        },
                        'taskTemplate': {
                            'type': 'string',
                            'minLength': 1
                        },
                        'memory': {
                            'type': 'object'
                        },
                        'securityToken': {
                            'type': 'string'
                        },
                        'parameters': {
                            'type': 'object'
                        },
                        'protocolVersion': {
                            'type': 'string',
                            'enum': ['1.0']
                        }
                    }
                }";
                
                // Parse the schema using JsonSchema.Net
                _mcpSchema = JsonSchema.FromText(schemaJson);
                _logger.LogInformation("MCP schema initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing MCP schema");
                throw;
            }
        }

        /// <summary>
        /// Validates an MCP JSON string against the schema.
        /// </summary>
        /// <param name="jsonString">The MCP JSON string to validate</param>
        /// <returns>True if valid, otherwise false</returns>
        public Task<bool> ValidateAsync(string jsonString)
        {
            try
            {
                _logger.LogDebug("Validating MCP JSON against schema");
                
                if (string.IsNullOrEmpty(jsonString))
                {
                    _logger.LogError("JSON string is null or empty");
                    return Task.FromResult(false);
                }
                
                using JsonDocument jsonDocument = JsonDocument.Parse(jsonString);
                
                // Validate using JsonSchema.Net
                var validationResult = _mcpSchema.Evaluate(jsonDocument.RootElement);
                
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Details)
            {
                        _logger.LogError("MCP validation error: {Error} at {Path}", 
                            error.Message, error.InstanceLocation);
            }
        }
                
                return Task.FromResult(validationResult.IsValid);
    }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error parsing JSON");
                return Task.FromResult(false);
}
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MCP JSON");
                throw;
            }
        }

        /// <summary>
        /// Validates an MCP JsonDocument against the schema.
        /// </summary>
        /// <param name="jsonDocument">The MCP JsonDocument to validate</param>
        /// <returns>True if valid, otherwise false</returns>
        public Task<bool> ValidateAsync(JsonDocument jsonDocument)
        {
            try
            {
                _logger.LogDebug("Validating MCP JsonDocument against schema");
                
                if (jsonDocument == null)
                {
                    _logger.LogError("JsonDocument is null");
                    return Task.FromResult(false);
                }
                
                // Validate using JsonSchema.Net
                var validationResult = _mcpSchema.Evaluate(jsonDocument.RootElement);
                
                if (!validationResult.IsValid)
                {
                    foreach (var error in validationResult.Details)
                    {
                        _logger.LogError("MCP validation error: {Error} at {Path}", 
                            error.Message, error.InstanceLocation);
                    }
                }
                
                return Task.FromResult(validationResult.IsValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MCP JsonDocument");
                throw;
            }
        }
    }
}