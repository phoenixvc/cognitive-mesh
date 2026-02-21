using System.Xml.Serialization;

namespace MetacognitiveLayer.Protocols.ACP.Models
{
    /// <summary>
    /// Represents a task in the AI Communication Protocol (ACP).
    /// </summary>
    [XmlRoot("Task")]
    public class ACPTask
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Domain this task belongs to
        /// </summary>
        [XmlAttribute("domain")]
        public string Domain { get; set; } = string.Empty;
        
        /// <summary>
        /// Version of the task template
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Description of the task
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Primary tool to execute for this task
        /// </summary>
        [XmlElement("PrimaryTool")]
        public string PrimaryTool { get; set; } = string.Empty;
        
        /// <summary>
        /// List of parameters for the task
        /// </summary>
        [XmlArray("Parameters")]
        [XmlArrayItem("Parameter")]
        public List<ACPParameter> Parameters { get; set; } = new List<ACPParameter>();
        
        /// <summary>
        /// List of constraints for the task
        /// </summary>
        [XmlArray("Constraints")]
        [XmlArrayItem("Constraint")]
        public List<ACPConstraint> Constraints { get; set; } = new List<ACPConstraint>();
        
        /// <summary>
        /// List of tools required for this task
        /// </summary>
        [XmlArray("RequiredTools")]
        [XmlArrayItem("Tool")]
        public List<string> RequiredTools { get; set; } = new List<string>();
        
        /// <summary>
        /// Fallback behavior in case of failure
        /// </summary>
        [XmlElement("Fallback")]
        public ACPFallback Fallback { get; set; } = null!;
        
        /// <summary>
        /// Metadata associated with this task
        /// </summary>
        [XmlElement("Metadata")]
        public ACPMetadata Metadata { get; set; } = null!;
    }
}