using CognitiveMesh.UILayer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitiveMesh.UILayer.Components
{
    /// <summary>
    /// Provides the business logic for managing the arrangement of widgets on a dashboard.
    /// This class handles widget placement, collision detection, and resizing, acting as the
    /// stateful manager for a single DashboardLayout instance. It is designed to be used
    /// by UI components to abstract away the complexity of layout management.
    /// </summary>
    public class DashboardLayoutManager
    {
        private readonly DashboardLayout _layout;
        private readonly ILogger<DashboardLayoutManager> _logger;
        private readonly int _gridWidth;

        /// <summary>
        /// Occurs when the dashboard layout is modified (e.g., a widget is added, moved, or resized).
        /// UI components should subscribe to this event to trigger a re-render.
        /// </summary>
        public event EventHandler LayoutChanged;

        /// <summary>
        /// Initializes a new instance of the DashboardLayoutManager class.
        /// </summary>
        /// <param name="layout">The dashboard layout object to manage.</param>
        /// <param name="logger">The logger for diagnostics.</param>
        /// <param name="gridWidth">The total number of columns in the dashboard grid. Defaults to 12.</param>
        public DashboardLayoutManager(DashboardLayout layout, ILogger<DashboardLayoutManager> logger, int gridWidth = 12)
        {
            _layout = layout ?? throw new ArgumentNullException(nameof(layout));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gridWidth = gridWidth > 0 ? gridWidth : 12;
        }

        /// <summary>
        /// Adds a new widget to the dashboard in the next available position.
        /// </summary>
        /// <param name="widgetId">The ID of the widget definition to add.</param>
        /// <param name="defaultSize">The default size of the widget to be placed.</param>
        /// <returns>The newly created WidgetInstance if successful; otherwise, null.</returns>
        public WidgetInstance AddWidget(string widgetId, WidgetSize defaultSize)
        {
            var availablePosition = GetAvailablePosition(defaultSize);
            if (availablePosition == null)
            {
                _logger.LogWarning("Could not add widget '{WidgetId}': No available position found on the dashboard.", widgetId);
                return null;
            }

            var newInstance = new WidgetInstance
            {
                WidgetId = widgetId,
                Position = availablePosition,
                Size = defaultSize
            };

            _layout.Widgets.Add(newInstance);
            _logger.LogInformation("Added new widget '{WidgetId}' at position ({X}, {Y}) with size ({Width}, {Height}).", widgetId, newInstance.Position.X, newInstance.Position.Y, newInstance.Size.Width, newInstance.Size.Height);
            OnLayoutChanged();
            return newInstance;
        }

        /// <summary>
        /// Removes a widget instance from the dashboard.
        /// </summary>
        /// <param name="instanceId">The unique ID of the widget instance to remove.</param>
        /// <returns>True if the widget was removed successfully; otherwise, false.</returns>
        public bool RemoveWidget(string instanceId)
        {
            var widgetToRemove = _layout.Widgets.FirstOrDefault(w => w.InstanceId == instanceId);
            if (widgetToRemove == null)
            {
                _logger.LogWarning("Could not remove widget: Instance with ID '{InstanceId}' not found.", instanceId);
                return false;
            }

            _layout.Widgets.Remove(widgetToRemove);
            _logger.LogInformation("Removed widget instance '{InstanceId}'.", instanceId);
            OnLayoutChanged();
            return true;
        }

        /// <summary>
        /// Moves a widget to a new position on the dashboard grid.
        /// </summary>
        /// <param name="instanceId">The ID of the widget instance to move.</param>
        /// <param name="newPosition">The target position for the widget.</param>
        /// <returns>True if the move was successful; otherwise, false.</returns>
        public bool MoveWidget(string instanceId, WidgetPosition newPosition)
        {
            var widget = GetWidgetInstance(instanceId);
            var originalPosition = widget.Position;

            // Temporarily update position for validation
            widget.Position = newPosition;
            if (!ValidatePosition(widget))
            {
                // Revert if the new position is invalid
                widget.Position = originalPosition;
                _logger.LogWarning("Invalid move for widget '{InstanceId}' to position ({X}, {Y}). Collision detected or out of bounds.", instanceId, newPosition.X, newPosition.Y);
                return false;
            }

            // The position is already updated, so we just log and fire the event
            _logger.LogInformation("Moved widget '{InstanceId}' to position ({X}, {Y}).", instanceId, newPosition.X, newPosition.Y);
            OnLayoutChanged();
            return true;
        }

        /// <summary>
        /// Resizes a widget on the dashboard grid.
        /// </summary>
        /// <param name="instanceId">The ID of the widget instance to resize.</param>
        /// <param name="newSize">The new size for the widget.</param>
        /// <returns>True if the resize was successful; otherwise, false.</returns>
        public bool ResizeWidget(string instanceId, WidgetSize newSize)
        {
            var widget = GetWidgetInstance(instanceId);
            var originalSize = widget.Size;

            // Temporarily update size for validation
            widget.Size = newSize;
            if (!ValidatePosition(widget))
            {
                // Revert if the new size causes an issue
                widget.Size = originalSize;
                _logger.LogWarning("Invalid resize for widget '{InstanceId}' to size ({Width}, {Height}). Collision detected or out of bounds.", instanceId, newSize.Width, newSize.Height);
                return false;
            }

            _logger.LogInformation("Resized widget '{InstanceId}' to size ({Width}, {Height}).", instanceId, newSize.Width, newSize.Height);
            OnLayoutChanged();
            return true;
        }

        /// <summary>
        /// Finds the next available position on the grid for a widget of a given size.
        /// </summary>
        /// <param name="widgetSize">The size of the widget to place.</param>
        /// <returns>A WidgetPosition if an available spot is found; otherwise, null.</returns>
        public WidgetPosition GetAvailablePosition(WidgetSize widgetSize)
        {
            // Simple top-to-bottom, left-to-right search
            for (int y = 0; y < 100; y++) // Assume a max of 100 rows
            {
                for (int x = 0; x <= _gridWidth - widgetSize.Width; x++)
                {
                    var prospectiveInstance = new WidgetInstance
                    {
                        InstanceId = "temp-validation-instance",
                        Position = new WidgetPosition { X = x, Y = y },
                        Size = widgetSize
                    };

                    if (ValidatePosition(prospectiveInstance, ignoreSelf: false))
                    {
                        return prospectiveInstance.Position;
                    }
                }
            }
            return null; // No position found
        }

        /// <summary>
        /// Validates if a widget's position and size are valid within the current layout (no collisions or out-of-bounds).
        /// </summary>
        /// <param name="subject">The widget instance to validate.</param>
        /// <param name="ignoreSelf">If true, the subject widget is excluded from collision checks. Used for move/resize operations.</param>
        /// <returns>True if the position is valid; otherwise, false.</returns>
        public bool ValidatePosition(WidgetInstance subject, bool ignoreSelf = true)
        {
            // Check grid boundaries
            if (subject.Position.X < 0 || subject.Position.Y < 0 || (subject.Position.X + subject.Size.Width) > _gridWidth)
            {
                return false;
            }

            // Check for collisions with other widgets
            foreach (var existingWidget in _layout.Widgets)
            {
                if (ignoreSelf && existingWidget.InstanceId == subject.InstanceId)
                {
                    continue; // Don't check against itself
                }

                if (DoWidgetsOverlap(subject, existingWidget))
                {
                    return false; // Collision detected
                }
            }

            return true; // Position is valid
        }

        /// <summary>
        /// Raises the LayoutChanged event to notify subscribers.
        /// </summary>
        protected virtual void OnLayoutChanged()
        {
            _layout.LastModified = DateTimeOffset.UtcNow;
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        private WidgetInstance GetWidgetInstance(string instanceId)
        {
            var widget = _layout.Widgets.FirstOrDefault(w => w.InstanceId == instanceId);
            return widget ?? throw new KeyNotFoundException($"Widget instance with ID '{instanceId}' not found in the layout.");
        }

        private bool DoWidgetsOverlap(WidgetInstance a, WidgetInstance b)
        {
            // AABB (Axis-Aligned Bounding Box) collision detection
            // True if the rectangles overlap, false otherwise.
            return a.Position.X < b.Position.X + b.Size.Width &&
                   a.Position.X + a.Size.Width > b.Position.X &&
                   a.Position.Y < b.Position.Y + b.Size.Height &&
                   a.Position.Y + a.Size.Height > b.Position.Y;
        }
    }
}
