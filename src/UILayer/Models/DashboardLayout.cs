using System;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.Models
{
    /// <summary>
    /// Represents a user's complete dashboard configuration, including the arrangement
    /// and state of all widget instances. This allows for saving, loading, and managing
    /// multiple personalized dashboard profiles per user.
    /// </summary>
    public class DashboardLayout
    {
        /// <summary>
        /// The unique identifier for this specific layout profile.
        /// </summary>
        public string LayoutId { get; set; }

        /// <summary>
        /// The identifier of the user who owns this dashboard layout.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// A human-readable name for the dashboard layout (e.g., "Daily Operations View").
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// An optional description of the layout's purpose or focus.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The collection of widget instances that make up this dashboard layout.
        /// This defines which widgets are present and their specific configuration and position.
        /// </summary>
        public List<WidgetInstance> Widgets { get; set; } = new List<WidgetInstance>();

        /// <summary>
        /// The date and time when this layout was first created.
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// The date and time when this layout was last modified.
        /// </summary>
        public DateTimeOffset LastModified { get; set; }

        /// <summary>
        /// Indicates whether this is the default layout to be loaded for the user upon login.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// The version of this layout, allowing for schema migrations or updates in the future.
        /// </summary>
        public int Version { get; set; } = 1;
    }
}
