using System.Xml.Serialization;

namespace MetacognitiveLayer.Protocols.ACP.Models
{
    /// <summary>
    /// Represents fallback behavior for an ACP task in case of failure.
    /// </summary>
    public class ACPFallback
    {
        /// <summary>
        /// Type of fallback action (e.g., retry, alternate, abort)
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Maximum number of retries if Type is "retry"
        /// </summary>
        [XmlAttribute("maxRetries")]
        public int MaxRetries { get; set; }
        
        /// <summary>
        /// Name of alternate task to execute if Type is "alternate"
        /// </summary>
        [XmlElement("AlternateTask")]
        public string AlternateTask { get; set; } = string.Empty;
        
        /// <summary>
        /// Default response to return if fallback occurs
        /// </summary>
        [XmlElement("DefaultResponse")]
        public string DefaultResponse { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether to log detailed information about the failure
        /// </summary>
        [XmlAttribute("logDetailedFailure")]
        public bool LogDetailedFailure { get; set; } = true;
    }
}