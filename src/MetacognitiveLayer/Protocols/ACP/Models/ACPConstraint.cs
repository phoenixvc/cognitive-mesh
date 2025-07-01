using System.Xml.Serialization;

namespace CognitiveMesh.MetacognitiveLayer.Protocols.ACP.Models
{
    /// <summary>
    /// Represents a constraint on a parameter in an ACP task.
    /// </summary>
    public class ACPConstraint
    {
        /// <summary>
        /// Type of constraint (e.g., range, regex, enum)
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }
        
        /// <summary>
        /// Name of the parameter this constraint applies to
        /// </summary>
        [XmlAttribute("parameterName")]
        public string ParameterName { get; set; }
        
        /// <summary>
        /// Value or expression for the constraint
        /// </summary>
        [XmlElement("Value")]
        public string Value { get; set; }
        
        /// <summary>
        /// Error message to display when constraint is violated
        /// </summary>
        [XmlElement("ErrorMessage")]
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Severity of the constraint (e.g., error, warning)
        /// </summary>
        [XmlAttribute("severity")]
        public string Severity { get; set; } = "error";
    }
}