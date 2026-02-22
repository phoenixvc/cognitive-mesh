using System.Text.Json.Serialization;

namespace MetacognitiveLayer.Protocols.MCP.Models
{
    /// <summary>
    /// Represents a response in the Model Context Protocol (MCP) format.
    /// </summary>
    public class MCPResponse
    {
        /// <summary>
        /// Unique identifier for the session
        /// </summary>
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        /// <summary>
        /// Identifier for the conversation
        /// </summary>
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; } = string.Empty;

        /// <summary>
        /// User identifier
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// ISO8601 timestamp of the response
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Status of the response: "success" or "error"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        /// <summary>
        /// Result object (only present if status is "success")
        /// </summary>
        [JsonPropertyName("result")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object Result { get; set; } = string.Empty;
        
        /// <summary>
        /// Error information (only present if status is "error")
        /// </summary>
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MCPError Error { get; set; } = null!;
        
        /// <summary>
        /// Memory updates resulting from this operation
        /// </summary>
        [JsonPropertyName("memoryUpdates")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, string> MemoryUpdates { get; set; } = new();
        
        /// <summary>
        /// Metrics about the operation execution
        /// </summary>
        [JsonPropertyName("metrics")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, object> Metrics { get; set; } = new();
        
        /// <summary>
        /// Version of the MCP protocol being used
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "1.0";
    }
}