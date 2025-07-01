using System;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.Models
{
    /// <summary>
    /// Represents the position of a widget on the dashboard grid.
    /// </summary>
    public class WidgetPosition
    {
        /// <summary>
        /// The X-coordinate (column) of the widget on the dashboard grid.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The Y-coordinate (row) of the widget on the dashboard grid.
        /// </summary>
        public int Y { get; set; }
    }

    /// <summary>
    /// Represents the size of a widget on the dashboard grid.
    /// </summary>
    public class WidgetSize
    {
        /// <summary>
        /// The width of the widget, typically in grid units.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the widget, typically in grid units.
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// Records a user's consent for a specific permission required by a widget instance.
    /// </summary>
    public class UserConsentRecord
    {
        /// <summary>
        /// The permission string for which consent was granted (e.g., "read:customer-data").
        /// </summary>
        public string Permission { get; set; }

        /// <summary>
        /// The date and time when the consent was granted by the user.
        /// </summary>
        public DateTimeOffset ConsentDate { get; set; }

        /// <summary>
        /// An optional identifier for the version of the policy or terms that the user consented to.
        /// </summary>
        public string ConsentVersion { get; set; }
    }

    /// <summary>
    /// Represents a specific instance of a widget placed on a user's dashboard.
    /// This class holds the state, position, and user-specific configuration for a single
    /// widget on the canvas, linking back to its core `WidgetDefinition`.
    /// </summary>
    public class WidgetInstance
    {
        /// <summary>
        /// A unique identifier for this specific instance of the widget on the dashboard.
        /// This allows for multiple instances of the same widget type.
        /// </summary>
        public string InstanceId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The identifier of the core widget definition, linking this instance to its
        /// underlying type and metadata in the `IWidgetRegistry`.
        /// </summary>
        public string WidgetId { get; set; }

        /// <summary>
        /// The position (X, Y coordinates) of the widget on the dashboard canvas.
        /// </summary>
        public WidgetPosition Position { get; set; } = new WidgetPosition();

        /// <summary>
        /// The size (width, height) of the widget on the dashboard canvas.
        /// </summary>
        public WidgetSize Size { get; set; } = new WidgetSize();

        /// <summary>
        /// A dictionary of instance-specific configuration values that override the defaults
        /// from the `WidgetDefinition`. For example, a stock widget's ticker symbol.
        /// </summary>
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A collection of consent records granted by the user for this specific widget instance.
        /// This is essential for auditing and enforcing data sovereignty.
        /// </summary>
        public List<UserConsentRecord> UserConsents { get; set; } = new List<UserConsentRecord>();

        /// <summary>
        /// The timestamp of the last time the user interacted with this widget instance.
        /// Useful for telemetry and identifying stale widgets.
        /// </summary>
        public DateTimeOffset LastInteraction { get; set; }

        /// <summary>
        /// A flag indicating whether the widget is currently visible on the dashboard.
        /// Allows users to temporarily hide widgets without removing them from the layout.
        /// </summary>
        public bool IsVisible { get; set; } = true;
    }
}
