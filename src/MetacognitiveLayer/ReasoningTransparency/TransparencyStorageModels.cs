namespace MetacognitiveLayer.ReasoningTransparency
{
    /// <summary>
    /// Represents the storage model for a reasoning trace node in the knowledge graph.
    /// </summary>
    public class ReasoningTraceNode
    {
        /// <summary>
        /// Unique identifier for the trace.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Name of the trace.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the trace.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Creation timestamp.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Last update timestamp.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the storage model for a reasoning step node in the knowledge graph.
    /// </summary>
    public class ReasoningStepNode
    {
        /// <summary>
        /// Unique identifier for the step.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Identifier of the trace this step belongs to.
        /// </summary>
        public string TraceId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the step.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of the step.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the step.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Confidence score of the step.
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// JSON serialized inputs.
        /// </summary>
        public string InputsJson { get; set; } = "{}";

        /// <summary>
        /// JSON serialized outputs.
        /// </summary>
        public string OutputsJson { get; set; } = "{}";

        /// <summary>
        /// JSON serialized metadata.
        /// </summary>
        public string MetadataJson { get; set; } = "{}";
    }
}
