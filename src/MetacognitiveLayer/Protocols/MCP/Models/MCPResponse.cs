using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.MCP.Models
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
        public string SessionId { get; set; }
        
        /// <summary>
        /// Identifier for the conversation
        /// </summary>
        [JsonPropertyName("conversationId")]
        public string ConversationId { get; set; }
        
        /// <summary>
        /// User identifier
        /// </summary>
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        
        /// <summary>
        /// ISO8601 timestamp of the response
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Status of the response: "success" or "error"
        /// </summary>
        [JsonPropertyName("status")]
        public string Status { get; set; }
        
        /// <summary>
        /// Result object (only present if status is "success")
        /// </summary>
        [JsonPropertyName("result", NullValueHandling = NullValueHandling.Ignore)]
        public object Result { get; set; }
        
        /// <summary>
        /// Error information (only present if status is "error")
        /// </summary>
        [JsonPropertyName("error", NullValueHandling = NullValueHandling.Ignore)]
        public MCPError Error { get; set; }
        
        /// <summary>
        /// Memory updates resulting from this operation
        /// </summary>
        [JsonPropertyName("memoryUpdates", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> MemoryUpdates { get; set; }
        
        /// <summary>
        /// Metrics about the operation execution
        /// </summary>
        [JsonPropertyName("metrics", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Metrics { get; set; }
        
        /// <summary>
        /// Version of the MCP protocol being used
        /// </summary>
        [JsonPropertyName("protocolVersion")]
        public string ProtocolVersion { get; set; } = "1.0";
    }
}