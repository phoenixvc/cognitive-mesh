using System;

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency
{
    /// <summary>
    /// Represents the storage model for a reasoning trace node in the knowledge graph.
    /// </summary>
    public class ReasoningTraceNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the storage model for a reasoning step node in the knowledge graph.
    /// </summary>
    public class ReasoningStepNode
    {
        public string Id { get; set; }
        public string TraceId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public float Confidence { get; set; }

        // Serialized JSON strings for complex dictionary properties
        public string InputsJson { get; set; }
        public string OutputsJson { get; set; }
        public string MetadataJson { get; set; }
    }
}
