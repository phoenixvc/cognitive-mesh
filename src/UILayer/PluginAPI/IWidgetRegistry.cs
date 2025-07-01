using CognitiveMesh.UILayer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.UILayer.PluginAPI
{
    /// <summary>
    /// Defines the core contract for managing the lifecycle of widgets within the Cognitive Mesh dashboard.
    /// This interface serves as the stable, versioned API for the plugin ecosystem, handling registration,
    /// retrieval, and validation of all dashboard widgets.
    /// </summary>
    public interface IWidgetRegistry
    {
        /// <summary>
        /// Registers a new widget with the system or updates an existing one.
        /// The implementation should perform validation, check for version conflicts,
        /// and persist the widget definition.
        /// </summary>
        /// <param name="widgetDefinition">The complete definition of the widget to register.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the widget was successfully registered, and false otherwise.</returns>
        Task<bool> RegisterWidgetAsync(WidgetDefinition widgetDefinition);

        /// <summary>
        /// Retrieves a single widget definition by its unique identifier.
        /// </summary>
        /// <param name="widgetId">The unique ID of the widget to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="WidgetDefinition"/> if found; otherwise, null.</returns>
        Task<WidgetDefinition> GetWidgetAsync(string widgetId);

        /// <summary>
        /// Retrieves all currently registered and approved widgets available to the user.
        /// This is used to populate the plugin/widget library in the UI.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of all available widget definitions.</returns>
        Task<IEnumerable<WidgetDefinition>> GetAllWidgetsAsync();

        /// <summary>
        /// Removes a widget from the registry, effectively uninstalling it from the system.
        /// The implementation should handle cleanup and notify relevant systems if necessary.
        /// </summary>
        /// <param name="widgetId">The unique ID of the widget to unregister.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the widget was successfully unregistered, and false otherwise.</returns>
        Task<bool> UnregisterWidgetAsync(string widgetId);

        /// <summary>
        /// Validates a widget definition against the system's rules and contracts without registering it.
        /// This can be used to check for compliance, required fields, and signature validity before attempting registration.
        /// </summary>
        /// <param name="widgetDefinition">The widget definition to validate.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains true if the definition is valid, and false otherwise.</returns>
        Task<bool> ValidateWidgetAsync(WidgetDefinition widgetDefinition);
    }
}
