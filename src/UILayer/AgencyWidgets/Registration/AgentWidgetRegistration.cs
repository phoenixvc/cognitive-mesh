using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.UILayer.AgencyWidgets.Adapters;
using CognitiveMesh.UILayer.Core;
using CognitiveMesh.UILayer.Models;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.AgencyWidgets.Registration
{
    /// <summary>
    /// Service responsible for registering all Agentic AI System widgets with the WidgetRegistry.
    /// This ensures that the frontend dashboard can discover and render these widgets.
    /// </summary>
    public class AgentWidgetRegistration : IWidgetRegistrationService
    {
        private readonly IWidgetRegistry _widgetRegistry;
        private readonly ITelemetryAdapter _telemetryAdapter;
        private readonly ILogger<AgentWidgetRegistration> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentWidgetRegistration"/> class.
        /// </summary>
        /// <param name="widgetRegistry">The widget registry service.</param>
        /// <param name="telemetryAdapter">The telemetry adapter for logging registration events.</param>
        /// <param name="logger">The logger instance.</param>
        public AgentWidgetRegistration(
            IWidgetRegistry widgetRegistry,
            ITelemetryAdapter telemetryAdapter,
            ILogger<AgentWidgetRegistration> logger)
        {
            _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
            _telemetryAdapter = telemetryAdapter ?? throw new ArgumentNullException(nameof(telemetryAdapter));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers all Agentic AI System widgets with the widget registry.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task RegisterWidgetsAsync()
        {
            _logger.LogInformation("Starting registration of Agentic AI System widgets");

            try
            {
                await RegisterAgentControlCenterAsync();
                await RegisterAgentStatusBannerAsync();
                await RegisterAuthorityConsentModalAsync();
                await RegisterRegistryViewerAsync();
                await RegisterAuditEventLogOverlayAsync();

                await _telemetryAdapter.LogEventAsync(new TelemetryEvent
                {
                    timestamp = DateTime.UtcNow,
                    widgetId = "agent-widget-registration",
                    panelId = "AgentWidgetRegistration",
                    userId = "system",
                    action = "AgentWidgetsRegistrationCompleted",
                    metadata = new Dictionary<string, object>
                    {
                        { "registeredWidgets", 5 },
                        { "registrationTime", DateTime.UtcNow.ToString("o") }
                    }
                });

                _logger.LogInformation("Successfully registered all Agentic AI System widgets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Agentic AI System widgets");
                
                await _telemetryAdapter.LogEventAsync(new TelemetryEvent
                {
                    timestamp = DateTime.UtcNow,
                    widgetId = "agent-widget-registration",
                    panelId = "AgentWidgetRegistration",
                    userId = "system",
                    action = "AgentWidgetsRegistrationFailed",
                    errorCode = "REGISTRATION_FAILED",
                    metadata = new Dictionary<string, object>
                    {
                        { "errorMessage", ex.Message },
                        { "stackTrace", ex.StackTrace }
                    }
                });

                throw; // Re-throw to allow higher-level error handling
            }
        }

        /// <summary>
        /// Registers the Agent Control Center widget.
        /// </summary>
        private async Task RegisterAgentControlCenterAsync()
        {
            try
            {
                var widgetDefinition = new WidgetDefinition
                {
                    Id = "agent-control-center",
                    Name = "Agent Control Center",
                    Description = "Centralized dashboard for managing and monitoring agents",
                    Category = WidgetCategory.Administration,
                    Priority = WidgetPriority.High,
                    Path = "AgencyWidgets/Panels/AgentControlCenter",
                    RequiredPermissions = new[] { "Agent:View", "Agent:Manage" },
                    DefaultSize = new WidgetSize { Width = 12, Height = 8 }, // Full width, 8 rows high
                    MinSize = new WidgetSize { Width = 6, Height = 4 },
                    MaxSize = new WidgetSize { Width = 12, Height = 12 },
                    RequiredAdapters = new[] 
                    { 
                        "IDataAPIAdapterPort", 
                        "ITelemetryAdapter", 
                        "IThemeAdapter" 
                    },
                    Tags = new[] { "agent", "control", "administration", "monitoring" },
                    Version = "1.0.0"
                };

                await _widgetRegistry.RegisterWidgetAsync(widgetDefinition);
                _logger.LogInformation("Registered Agent Control Center widget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Agent Control Center widget");
                throw;
            }
        }

        /// <summary>
        /// Registers the Agent Status Banner widget.
        /// </summary>
        private async Task RegisterAgentStatusBannerAsync()
        {
            try
            {
                var widgetDefinition = new WidgetDefinition
                {
                    Id = "agent-status-banner",
                    Name = "Agent Status Banner",
                    Description = "Real-time status indicator for active agents",
                    Category = WidgetCategory.Monitoring,
                    Priority = WidgetPriority.Critical, // Critical as it shows real-time status
                    Path = "AgencyWidgets/Panels/AgentStatusBanner",
                    RequiredPermissions = new[] { "Agent:View" },
                    DefaultSize = new WidgetSize { Width = 12, Height = 1 }, // Full width, 1 row high (banner)
                    MinSize = new WidgetSize { Width = 6, Height = 1 },
                    MaxSize = new WidgetSize { Width = 12, Height = 2 },
                    RequiredAdapters = new[] 
                    { 
                        "IDataAPIAdapterPort", 
                        "INotificationAdapter", 
                        "ITelemetryAdapter", 
                        "IThemeAdapter" 
                    },
                    Tags = new[] { "agent", "status", "monitoring", "real-time" },
                    Version = "1.0.0",
                    IsFloating = true, // Banner should float at the top
                    IsAlwaysVisible = true // Always visible when agents are active
                };

                await _widgetRegistry.RegisterWidgetAsync(widgetDefinition);
                _logger.LogInformation("Registered Agent Status Banner widget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Agent Status Banner widget");
                throw;
            }
        }

        /// <summary>
        /// Registers the Authority/Consent Modal widget.
        /// </summary>
        private async Task RegisterAuthorityConsentModalAsync()
        {
            try
            {
                var widgetDefinition = new WidgetDefinition
                {
                    Id = "authority-consent-modal",
                    Name = "Authority/Consent Modal",
                    Description = "Modal dialog for handling agent authority escalations and consent requests",
                    Category = WidgetCategory.Security,
                    Priority = WidgetPriority.Critical, // Critical as it handles security-related actions
                    Path = "AgencyWidgets/Panels/AuthorityConsentModal",
                    RequiredPermissions = new[] { "Agent:View", "Agent:Consent" },
                    DefaultSize = new WidgetSize { Width = 6, Height = 6 }, // Modal size
                    MinSize = new WidgetSize { Width = 4, Height = 4 },
                    MaxSize = new WidgetSize { Width = 8, Height = 8 },
                    RequiredAdapters = new[] 
                    { 
                        "IConsentAdapter", 
                        "ITelemetryAdapter", 
                        "IThemeAdapter" 
                    },
                    Tags = new[] { "agent", "authority", "consent", "security", "modal" },
                    Version = "1.0.0",
                    IsModal = true, // This is a modal dialog
                    IsSystemWidget = true // System-level widget that can be triggered by the system
                };

                await _widgetRegistry.RegisterWidgetAsync(widgetDefinition);
                _logger.LogInformation("Registered Authority/Consent Modal widget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Authority/Consent Modal widget");
                throw;
            }
        }

        /// <summary>
        /// Registers the Registry Viewer widget.
        /// </summary>
        private async Task RegisterRegistryViewerAsync()
        {
            try
            {
                var widgetDefinition = new WidgetDefinition
                {
                    Id = "registry-viewer",
                    Name = "Agent Registry Viewer",
                    Description = "Detailed view of all registered agents with filtering and export capabilities",
                    Category = WidgetCategory.Administration,
                    Priority = WidgetPriority.Medium,
                    Path = "AgencyWidgets/Panels/RegistryViewer",
                    RequiredPermissions = new[] { "Agent:View", "Agent:Export" },
                    DefaultSize = new WidgetSize { Width = 12, Height = 10 }, // Full width, 10 rows high
                    MinSize = new WidgetSize { Width = 8, Height = 6 },
                    MaxSize = new WidgetSize { Width = 12, Height = 12 },
                    RequiredAdapters = new[] 
                    { 
                        "IDataAPIAdapterPort", 
                        "ITelemetryAdapter", 
                        "IThemeAdapter" 
                    },
                    Tags = new[] { "agent", "registry", "administration", "export" },
                    Version = "1.0.0"
                };

                await _widgetRegistry.RegisterWidgetAsync(widgetDefinition);
                _logger.LogInformation("Registered Registry Viewer widget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Registry Viewer widget");
                throw;
            }
        }

        /// <summary>
        /// Registers the Audit Event Log Overlay widget.
        /// </summary>
        private async Task RegisterAuditEventLogOverlayAsync()
        {
            try
            {
                var widgetDefinition = new WidgetDefinition
                {
                    Id = "audit-event-log-overlay",
                    Name = "Audit Event Log Overlay",
                    Description = "Comprehensive audit trail of all agent activities and system events",
                    Category = WidgetCategory.Compliance,
                    Priority = WidgetPriority.High,
                    Path = "AgencyWidgets/Panels/AuditEventLogOverlay",
                    RequiredPermissions = new[] { "Agent:View", "Agent:Audit" },
                    DefaultSize = new WidgetSize { Width = 10, Height = 10 }, // Nearly full width, 10 rows high
                    MinSize = new WidgetSize { Width = 6, Height = 6 },
                    MaxSize = new WidgetSize { Width = 12, Height = 12 },
                    RequiredAdapters = new[] 
                    { 
                        "IDataAPIAdapterPort", 
                        "ITelemetryAdapter", 
                        "IThemeAdapter" 
                    },
                    Tags = new[] { "agent", "audit", "compliance", "logs", "events" },
                    Version = "1.0.0",
                    IsOverlay = true // This is an overlay widget
                };

                await _widgetRegistry.RegisterWidgetAsync(widgetDefinition);
                _logger.LogInformation("Registered Audit Event Log Overlay widget");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register Audit Event Log Overlay widget");
                throw;
            }
        }
    }
}
