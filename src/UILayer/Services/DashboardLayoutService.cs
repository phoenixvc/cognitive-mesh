using CognitiveMesh.UILayer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CognitiveMesh.UILayer.Services
{
    /// <summary>
    /// Manages the persistence and lifecycle of user dashboard layouts.
    /// For the MVP, this service uses a local file system for storage, with each
    /// layout serialized as a JSON file. This can be replaced with a database
    /// implementation in the future without changing the public contract.
    /// </summary>
    public class DashboardLayoutService
    {
        private readonly ILogger<DashboardLayoutService> _logger;
        private readonly string _storagePath;

        // Options for pretty-printing the JSON to make the files human-readable.
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { WriteIndented = true };

        public DashboardLayoutService(ILogger<DashboardLayoutService> logger)
        {
            _logger = logger;
            // Define a base storage path. In a real application, this would come from configuration.
            _storagePath = Path.Combine(Path.GetTempPath(), "CognitiveMesh", "DashboardLayouts");
            Directory.CreateDirectory(_storagePath); // Ensure the base directory exists.
            _logger.LogInformation("DashboardLayoutService initialized. Storage path: {StoragePath}", _storagePath);
        }

        /// <summary>
        /// Saves a dashboard layout to the persistence store.
        /// If the layout already exists, it will be overwritten.
        /// </summary>
        /// <param name="layout">The dashboard layout to save.</param>
        /// <returns>True if the layout was saved successfully; otherwise, false.</returns>
        public async Task<bool> SaveLayoutAsync(DashboardLayout layout)
        {
            if (!IsValidLayout(layout))
            {
                _logger.LogWarning("Save failed: Layout validation failed for LayoutId '{LayoutId}'.", layout?.LayoutId ?? "null");
                return false;
            }

            try
            {
                layout.LastModified = DateTimeOffset.UtcNow;
                var userDirectory = Path.Combine(_storagePath, layout.UserId);
                Directory.CreateDirectory(userDirectory);

                var filePath = Path.Combine(userDirectory, $"{layout.LayoutId}.json");
                var jsonContent = JsonSerializer.Serialize(layout, _jsonSerializerOptions);

                await File.WriteAllTextAsync(filePath, jsonContent);
                _logger.LogInformation("Successfully saved layout '{LayoutName}' (ID: {LayoutId}) for User '{UserId}'.", layout.Name, layout.LayoutId, layout.UserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save layout with ID '{LayoutId}' for User '{UserId}'.", layout.LayoutId, layout.UserId);
                return false;
            }
        }

        /// <summary>
        /// Loads a specific dashboard layout for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="layoutId">The ID of the layout to load.</param>
        /// <returns>The loaded DashboardLayout, or null if not found.</returns>
        public async Task<DashboardLayout> LoadLayoutAsync(string userId, string layoutId)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(layoutId))
            {
                _logger.LogWarning("Load failed: UserId and LayoutId cannot be null or empty.");
                return null;
            }

            try
            {
                var filePath = Path.Combine(_storagePath, userId, $"{layoutId}.json");
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("Layout file not found for User '{UserId}', LayoutId '{LayoutId}'.", userId, layoutId);
                    return null;
                }

                var jsonContent = await File.ReadAllTextAsync(filePath);
                var layout = JsonSerializer.Deserialize<DashboardLayout>(jsonContent);
                _logger.LogInformation("Successfully loaded layout '{LayoutName}' for User '{UserId}'.", layout.Name, userId);
                return layout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load layout with ID '{LayoutId}' for User '{UserId}'.", layoutId, userId);
                return null;
            }
        }

        /// <summary>
        /// Retrieves all dashboard layouts for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A collection of the user's dashboard layouts.</returns>
        public async Task<IEnumerable<DashboardLayout>> GetUserLayoutsAsync(string userId)
        {
            var userDirectory = Path.Combine(_storagePath, userId);
            if (!Directory.Exists(userDirectory))
            {
                _logger.LogInformation("No layouts found for User '{UserId}' as the directory does not exist.", userId);
                return Enumerable.Empty<DashboardLayout>();
            }

            var layouts = new List<DashboardLayout>();
            var layoutFiles = Directory.GetFiles(userDirectory, "*.json");

            foreach (var filePath in layoutFiles)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(filePath);
                    var layout = JsonSerializer.Deserialize<DashboardLayout>(jsonContent);
                    if (layout != null)
                    {
                        layouts.Add(layout);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to load and deserialize layout file at path: {FilePath}", filePath);
                }
            }
            _logger.LogInformation("Found {Count} layouts for User '{UserId}'.", layouts.Count, userId);
            return layouts;
        }

        /// <summary>
        /// Deletes a specific dashboard layout.
        /// </summary>
        /// <param name="userId">The ID of the user who owns the layout.</param>
        /// <param name="layoutId">The ID of the layout to delete.</param>
        /// <returns>True if the layout was deleted successfully; otherwise, false.</returns>
        public Task<bool> DeleteLayoutAsync(string userId, string layoutId)
        {
            try
            {
                var filePath = Path.Combine(_storagePath, userId, $"{layoutId}.json");
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Successfully deleted layout with ID '{LayoutId}' for User '{UserId}'.", layoutId, userId);
                    return Task.FromResult(true);
                }
                _logger.LogWarning("Delete failed: Layout with ID '{LayoutId}' for User '{UserId}' not found.", layoutId, userId);
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete layout with ID '{LayoutId}' for User '{UserId}'.", layoutId, userId);
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Sets a specific layout as the default for a user, ensuring all others are not default.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="layoutId">The ID of the layout to set as default.</param>
        /// <returns>True if the default was set successfully; otherwise, false.</returns>
        public async Task<bool> SetDefaultLayoutAsync(string userId, string layoutId)
        {
            var userLayouts = (await GetUserLayoutsAsync(userId)).ToList();
            if (!userLayouts.Any(l => l.LayoutId == layoutId))
            {
                _logger.LogWarning("Cannot set default: LayoutId '{LayoutId}' not found for User '{UserId}'.", layoutId, userId);
                return false;
            }

            bool success = true;
            foreach (var layout in userLayouts)
            {
                bool shouldBeDefault = layout.LayoutId == layoutId;
                if (layout.IsDefault != shouldBeDefault)
                {
                    layout.IsDefault = shouldBeDefault;
                    bool saved = await SaveLayoutAsync(layout);
                    if (!saved)
                    {
                        success = false; // Track if any save operation fails.
                        _logger.LogError("Failed to update IsDefault flag for LayoutId '{LayoutId}' for User '{UserId}'.", layout.LayoutId, userId);
                    }
                }
            }
            
            if(success)
                _logger.LogInformation("Successfully set LayoutId '{LayoutId}' as default for User '{UserId}'.", layoutId, userId);

            return success;
        }

        /// <summary>
        /// Creates a default dashboard layout for a new user.
        /// </summary>
        /// <param name="userId">The ID of the new user.</param>
        /// <returns>The newly created default DashboardLayout.</returns>
        public async Task<DashboardLayout> CreateDefaultLayoutAsync(string userId)
        {
            var defaultLayout = new DashboardLayout
            {
                LayoutId = $"default-{Guid.NewGuid():N}",
                UserId = userId,
                Name = "My First Dashboard",
                Description = "A default layout to get you started.",
                CreatedDate = DateTimeOffset.UtcNow,
                LastModified = DateTimeOffset.UtcNow,
                IsDefault = true,
                Widgets = new List<WidgetInstance>
                {
                    new WidgetInstance
                    {
                        WidgetId = "welcome-widget", // A predefined widget for new users
                        Position = new WidgetPosition { X = 0, Y = 0 },
                        Size = new WidgetSize { Width = 4, Height = 2 }
                    }
                }
            };

            bool saved = await SaveLayoutAsync(defaultLayout);
            if (saved)
            {
                _logger.LogInformation("Successfully created default layout for User '{UserId}'.", userId);
                return defaultLayout;
            }

            _logger.LogError("Failed to save the new default layout for User '{UserId}'.", userId);
            return null;
        }

        private bool IsValidLayout(DashboardLayout layout)
        {
            return layout != null &&
                   !string.IsNullOrWhiteSpace(layout.UserId) &&
                   !string.IsNullOrWhiteSpace(layout.LayoutId) &&
                   !string.IsNullOrWhiteSpace(layout.Name);
        }
    }
}
