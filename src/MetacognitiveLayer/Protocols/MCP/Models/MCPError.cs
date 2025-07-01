using System.Text.Json.Serialization;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.MCP.Models
{
    /// <summary>
    /// Represents an error in the Model Context Protocol (MCP) response.
    /// </summary>
    public class MCPError
    {
        /// <summary>
        /// Error message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; }
        
        /// <summary>
        /// Type of error
        /// </summary>
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// Detailed error information for debugging
        /// </summary>
        [JsonPropertyName("details")]
        public string Details { get; set; }
        
        /// <summary>
        /// Error code for categorization
        /// </summary>
        [JsonPropertyName("code", NullValueHandling = NullValueHandling.Ignore)]
        public string Code { get; set; }
        
        /// <summary>
        /// Recommended action to resolve the error
        /// </summary>
        [JsonPropertyName("recommendation", NullValueHandling = NullValueHandling.Ignore)]
        public string Recommendation { get; set; }
    }
}