using System.Xml.Serialization;

namespace MetacognitiveLayer.Protocols.ACP.Models
{
    /// <summary>
    /// Represents metadata associated with an ACP task.
    /// </summary>
    public class ACPMetadata
    {
        /// <summary>
        /// Author of the task template
        /// </summary>
        [XmlElement("Author")]
        public string Author { get; set; }
        
        /// <summary>
        /// Creation date of the task template
        /// </summary>
        [XmlElement("CreatedDate")]
        public DateTime CreatedDate { get; set; }
        
        /// <summary>
        /// Last modified date of the task template
        /// </summary>
        [XmlElement("LastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }
        
        /// <summary>
        /// Tags associated with the task template
        /// </summary>
        [XmlArray("Tags")]
        [XmlArrayItem("Tag")]
        public List<string> Tags { get; set; } = new List<string>();
        
        /// <summary>
        /// Parent template ID if this is derived from another template
        /// </summary>
        [XmlElement("ParentTemplate")]
        public string ParentTemplate { get; set; }
        
        /// <summary>
        /// Additional custom properties
        /// </summary>
        [XmlArray("CustomProperties")]
        [XmlArrayItem("Property")]
        public List<KeyValuePair> CustomProperties { get; set; } = new List<KeyValuePair>();
    }

    /// <summary>
    /// Represents a key-value pair for custom metadata properties.
    /// </summary>
    public class KeyValuePair
    {
        /// <summary>
        /// Key of the property
        /// </summary>
        [XmlAttribute("key")]
        public string Key { get; set; }
        
        /// <summary>
        /// Value of the property
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}