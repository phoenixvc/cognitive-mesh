using System;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.Models
{
    /// <summary>
    /// Represents the complete definition of a plugin-based widget.
    /// This model is the core contract for registering, displaying, and managing
    /// widgets within the Cognitive Mesh dashboard shell. It captures all metadata
    /// required for the UI, security, data provenance, and marketplace.
    /// </summary>
    public class WidgetDefinition
    {
        /// <summary>
        /// A unique, machine-readable identifier for the widget (e.g., "community-pulse-widget").
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The human-readable title displayed on the widget's header.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// A brief description of the widget's purpose and functionality, shown in the marketplace.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The semantic version of the widget (e.g., "1.0.0").
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// The name of the author or vendor who created the widget.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The category the widget belongs to, used for filtering in the marketplace (e.g., "Analytics", "Operations").
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// A list of data sources this widget consumes. Essential for data provenance and auditability.
        /// (e.g., ["KnowledgeGraph v1.2", "LLM-at-2024-05-16"]).
        /// </summary>
        public List<string> DataSources { get; set; } = new List<string>();

        /// <summary>
        /// A list of permissions or data access scopes the widget requires to function.
        /// Used to generate consent dialogs. (e.g., ["read:customer-data", "write:operational-plans"]).
        /// </summary>
        public List<string> Permissions { get; set; } = new List<string>();

        /// <summary>
        /// If true, the shell must obtain explicit user consent before the widget can be activated for the first time.
        /// </summary>
        public bool ConsentRequired { get; set; } = true;

        /// <summary>
        /// The name or identifier of the function or component responsible for rendering the widget's UI.
        /// The core shell uses this to dynamically load the correct UI component.
        /// </summary>
        public string RenderFunction { get; set; }

        /// <summary>
        /// A dictionary defining user-configurable options for the widget, along with their default values.
        /// This allows for instance-specific settings.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A flexible dictionary for additional marketplace metadata, such as icon URLs, tags, or links to documentation.
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
