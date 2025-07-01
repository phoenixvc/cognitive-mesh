using CognitiveMesh.UILayer.Models;
using CognitiveMesh.UILayer.PluginAPI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CognitiveMesh.UILayer.Core
{
    /// <summary>
    /// A concrete, in-memory implementation of the IWidgetRegistry interface.
    /// This class manages the lifecycle of dashboard widgets in a thread-safe manner.
    /// For the MVP, storage is in-memory; this can be extended with a persistent
    /// backing store in the future.
    /// </summary>
    public class WidgetRegistry : IWidgetRegistry
    {
        private readonly ILogger<WidgetRegistry> _logger;
        private readonly ConcurrentDictionary<string, WidgetDefinition> _widgets = new();

        public WidgetRegistry(ILogger<WidgetRegistry> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<bool> RegisterWidgetAsync(WidgetDefinition widgetDefinition)
        {
            if (!await ValidateWidgetAsync(widgetDefinition))
            {
                _logger.LogWarning("Widget registration failed due to invalid definition for widget ID '{WidgetId}'.", widgetDefinition?.Id ?? "unknown");
                return false;
            }

            try
            {
                _widgets.AddOrUpdate(
                    widgetDefinition.Id,
                    // Add factory: executed if the key does not exist
                    (addKey) =>
                    {
                        _logger.LogInformation("Successfully registered new widget: '{WidgetTitle}' (ID: {WidgetId}, Version: {WidgetVersion}).", widgetDefinition.Title, widgetDefinition.Id, widgetDefinition.Version);
                        return widgetDefinition;
                    },
                    // Update factory: executed if the key already exists
                    (updateKey, existingWidget) =>
                    {
                        var newVersion = new Version(widgetDefinition.Version);
                        var existingVersion = new Version(existingWidget.Version);

                        if (newVersion > existingVersion)
                        {
                            _logger.LogInformation("Successfully updated widget '{WidgetTitle}' (ID: {WidgetId}) from version {OldVersion} to {NewVersion}.", existingWidget.Title, existingWidget.Id, existingWidget.Version, widgetDefinition.Version);
                            return widgetDefinition; // Replace with the new definition
                        }
                        else
                        {
                            _logger.LogWarning("Skipped widget update for '{WidgetTitle}' (ID: {WidgetId}). Incoming version {NewVersion} is not newer than existing version {OldVersion}.", existingWidget.Title, existingWidget.Id, widgetDefinition.Version, existingWidget.Version);
                            return existingWidget; // Keep the existing definition
                        }
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while registering widget with ID '{WidgetId}'.", widgetDefinition.Id);
                return false;
            }
        }

        /// <inheritdoc />
        public Task<WidgetDefinition> GetWidgetAsync(string widgetId)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
                return Task.FromResult<WidgetDefinition>(null);

            _widgets.TryGetValue(widgetId, out var widget);
            return Task.FromResult(widget);
        }

        /// <inheritdoc />
        public Task<IEnumerable<WidgetDefinition>> GetAllWidgetsAsync()
        {
            // Return a snapshot of the current values to avoid issues with collection modification during enumeration.
            return Task.FromResult(_widgets.Values.ToList().AsEnumerable());
        }

        /// <inheritdoc />
        public Task<bool> UnregisterWidgetAsync(string widgetId)
        {
            if (string.IsNullOrWhiteSpace(widgetId))
            {
                _logger.LogWarning("Unregister widget failed: Widget ID was null or empty.");
                return Task.FromResult(false);
            }

            if (_widgets.TryRemove(widgetId, out var removedWidget))
            {
                _logger.LogInformation("Successfully unregistered widget: '{WidgetTitle}' (ID: {WidgetId}).", removedWidget.Title, removedWidget.Id);
                return Task.FromResult(true);
            }

            _logger.LogWarning("Unregister widget failed: Widget with ID '{WidgetId}' was not found.", widgetId);
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        public Task<bool> ValidateWidgetAsync(WidgetDefinition widgetDefinition)
        {
            if (widgetDefinition == null)
            {
                _logger.LogError("Validation failed: Widget definition is null.");
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(widgetDefinition.Id))
            {
                _logger.LogWarning("Validation failed for widget '{WidgetTitle}': ID is required.", widgetDefinition.Title ?? "N/A");
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(widgetDefinition.Title))
            {
                _logger.LogWarning("Validation failed for widget ID '{WidgetId}': Title is required.", widgetDefinition.Id);
                return Task.FromResult(false);
            }

            if (string.IsNullOrWhiteSpace(widgetDefinition.RenderFunction))
            {
                _logger.LogWarning("Validation failed for widget ID '{WidgetId}': RenderFunction is required.", widgetDefinition.Id);
                return Task.FromResult(false);
            }

            if (!Version.TryParse(widgetDefinition.Version, out _))
            {
                _logger.LogWarning("Validation failed for widget ID '{WidgetId}': Version string '{Version}' is not a valid version.", widgetDefinition.Id, widgetDefinition.Version);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
