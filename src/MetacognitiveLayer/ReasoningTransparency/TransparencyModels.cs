using System;
using System.Collections.Generic;

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency
{
    /// <summary>
    /// Represents a step in a reasoning process
    /// </summary>
    public class ReasoningStep
    {
        /// <summary>
        /// Unique identifier for the step
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the trace this step belongs to
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Name of the step
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the step
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// When the step occurred
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Inputs to the step
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; } = new();

        /// <summary>
        /// Outputs from the step
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; } = new();

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a trace of reasoning steps
    /// </summary>
    public class ReasoningTrace
    {
        /// <summary>
        /// Unique identifier for the trace
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the trace
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the trace
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The steps in this reasoning trace
        /// </summary>
        public List<ReasoningStep> Steps { get; set; } = new();

        /// <summary>
        /// When the trace was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the trace was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Represents the rationale behind a decision
    /// </summary>
    public class DecisionRationale
    {
        /// <summary>
        /// Unique identifier for the rationale
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the decision this rationale is for
        /// </summary>
        public string DecisionId { get; set; }

        /// <summary>
        /// Description of the rationale
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Confidence score (0-1)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Factors that contributed to the decision
        /// </summary>
        public Dictionary<string, float> Factors { get; set; } = new();

        /// <summary>
        /// When the rationale was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Represents a transparency report
    /// </summary>
    public class TransparencyReport
    {
        /// <summary>
        /// Unique identifier for the report
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Identifier of the trace this report is for
        /// </summary>
        public string TraceId { get; set; }

        /// <summary>
        /// Format of the report (e.g., json, html, markdown)
        /// </summary>
        public string Format { get; set; }

        /// <summary>
        /// The report content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// When the report was generated
        /// </summary>
        public DateTime GeneratedAt { get; set; }
    }
}
