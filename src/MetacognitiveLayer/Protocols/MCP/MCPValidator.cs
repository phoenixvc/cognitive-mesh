using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MetacognitiveLayer.Protocols.MCP
{
    /// <summary>
    /// Validates MCP (Model Context Protocol) JSON messages against the schema.
    /// </summary>
    public class MCPValidator
    {
        private readonly ILogger<MCPValidator> _logger;
        private JsonDocument? _mcpSchema;

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
                    ""$schema"": ""http://json-schema.org/draft-07/schema#"",
                    ""type"": ""object"",
                    ""required"": [""sessionId"", ""taskTemplate"", ""protocolVersion""],
                    ""properties"": {
                        ""sessionId"": {
                            ""type"": ""string"",
                            ""minLength"": 1
                        },
                        ""conversationId"": {
                            ""type"": ""string""
                        },
                        ""userId"": {
                            ""type"": ""string""
                        },
                        ""userRole"": {
                            ""type"": ""string""
                        },
                        ""timestamp"": {
                            ""type"": ""string"",
                            ""format"": ""date-time""
                        },
                        ""taskTemplate"": {
                            ""type"": ""string"",
                            ""minLength"": 1
                        },
                        ""memory"": {
                            ""type"": ""object""
                        },
                        ""securityToken"": {
                            ""type"": ""string""
                        },
                        ""parameters"": {
                            ""type"": ""object""
                        },
                        ""protocolVersion"": {
                            ""type"": ""string"",
                            ""enum"": [""1.0""]
                        }
                    }
                }";

                // Parse the schema as a JsonDocument for validation reference
                _mcpSchema = JsonDocument.Parse(schemaJson);
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

                var isValid = ValidateDocument(jsonDocument);

                return Task.FromResult(isValid);
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

                var isValid = ValidateDocument(jsonDocument);

                return Task.FromResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating MCP JsonDocument");
                throw;
            }
        }

        /// <summary>
        /// Performs basic structural validation of a JSON document against the MCP schema.
        /// </summary>
        private bool ValidateDocument(JsonDocument jsonDocument)
        {
            var isValid = true;
            var root = jsonDocument.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
            {
                _logger.LogError("MCP validation error: root element must be an object");
                return false;
            }

            // Validate required fields
            var requiredFields = new[] { "sessionId", "taskTemplate", "protocolVersion" };
            foreach (var field in requiredFields)
            {
                if (!root.TryGetProperty(field, out var prop) || prop.ValueKind == JsonValueKind.Null)
                {
                    _logger.LogError("MCP validation error: missing required field '{Field}'", field);
                    isValid = false;
                }
            }

            // Validate protocolVersion enum
            if (root.TryGetProperty("protocolVersion", out var version) &&
                version.ValueKind == JsonValueKind.String &&
                version.GetString() != "1.0")
            {
                _logger.LogError("MCP validation error: protocolVersion must be '1.0', got '{Version}'",
                    version.GetString());
                isValid = false;
            }

            return isValid;
        }
    }
}
