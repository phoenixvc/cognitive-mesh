using System.Xml.Serialization;

namespace MetacognitiveLayer.Protocols.ACP.Models
{
    /// <summary>
    /// Represents a parameter in an ACP task.
    /// </summary>
    public class ACPParameter
    {
        /// <summary>
        /// Name of the parameter
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Data type of the parameter
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; } = string.Empty;
        
        /// <summary>
        /// Whether this parameter is required
        /// </summary>
        [XmlAttribute("required")]
        public bool Required { get; set; }
        
        /// <summary>
        /// Whether this parameter's value comes from context
        /// </summary>
        [XmlAttribute("isContextual")]
        public bool IsContextual { get; set; }
        
        /// <summary>
        /// Default or provided value of the parameter
        /// </summary>
        [XmlElement("Value")]
        public object Value { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the parameter
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;
    }
}