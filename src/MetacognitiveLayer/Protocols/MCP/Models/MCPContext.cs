using System.Text.Json.Serialization;

namespace MetacognitiveLayer.Protocols.MCP.Models
{
    /// <summary>
    /// Represents the context for a Model Context Protocol (MCP) request.
    /// Contains session identification, user context, memory references, and the ACP task template.
    /// </summary>
    public class MCPContext
    {
        /// <summary>
        /// Unique identifier for the current session
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;
        
        /// <summary>
        /// Identifier for the ongoing conversation
        /// </summary>
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = string.Empty;
        
        /// <summary>
        /// User identifier for authentication and authorization
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Role/permissions of the requesting user
        /// </summary>
        [JsonPropertyName("userRole")]
        public string UserRole { get; set; } = string.Empty;
        
        /// <summary>
        /// ISO8601 timestamp of the request
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// The ACP XML template defining the task to execute
        /// </summary>
        [JsonPropertyName("taskTemplate")]
        public string TaskTemplate { get; set; } = string.Empty;
        
        /// <summary>
        /// Memory references for context retrieval
        /// </summary>
        [JsonPropertyName("memory")]
        public Dictionary<string, string> Memory { get; set; } = new();
        
        /// <summary>
        /// Security token for authentication with external services
        /// </summary>
        [JsonPropertyName("securityToken")]
        public string SecurityToken { get; set; } = string.Empty;
        
        /// <summary>
        /// Additional parameters specific to the request
        /// </summary>
        [JsonPropertyName("parameters")]
        public Dictionary<string, object> Parameters { get; set; } = new();
        
        /// <summary>
        /// Version of the MCP protocol being used
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "1.0";
    }
}