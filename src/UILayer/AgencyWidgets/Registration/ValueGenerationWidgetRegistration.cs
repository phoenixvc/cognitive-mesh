using CognitiveMesh.UILayer.Models;
using CognitiveMesh.UILayer.PluginAPI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.UILayer.AgencyWidgets.Registration
{
    /// <summary>
    /// Service responsible for registering all Value Generation widgets with the WidgetRegistry.
    /// This ensures that the frontend dashboard can discover and render these widgets.
    /// </summary>
    public class ValueGenerationWidgetRegistration
    {
        private readonly IWidgetRegistry _widgetRegistry;
        private readonly ILogger<ValueGenerationWidgetRegistration> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueGenerationWidgetRegistration"/> class.
        /// </summary>
        /// <param name="widgetRegistry">The widget registry to register widgets with.</param>
        /// <param name="logger">The logger for recording registration events.</param>
        public ValueGenerationWidgetRegistration(IWidgetRegistry widgetRegistry, ILogger<ValueGenerationWidgetRegistration> logger)
        {
            _widgetRegistry = widgetRegistry ?? throw new ArgumentNullException(nameof(widgetRegistry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Registers all Value Generation widgets with the widget registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task RegisterAllWidgetsAsync()
        {
            _logger.LogInformation("Starting registration of Value Generation widgets...");

            try
            {
                await RegisterTwoHundredDollarTestWidgetAsync();
                await RegisterValueDiagnosticDashboardAsync();
                await RegisterOrgBlindnessTrendsAsync();
                await RegisterEmployabilityScoreWidgetAsync();

                _logger.LogInformation("Successfully registered all Value Generation widgets.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering Value Generation widgets.");
                throw;
            }
        }

        /// <summary>
        /// Registers the $200 Test Widget with the widget registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RegisterTwoHundredDollarTestWidgetAsync()
        {
            var widget = new WidgetDefinition
            {
                Id = "two-hundred-dollar-test-widget",
                Title = "The $200 Test",
                Description = "A quick diagnostic tool to assess your personal value generation capacity.",
                Version = "1.0.0",
                Author = "Cognitive Mesh Team",
                Category = "Value Generation",
                RenderFunction = "TwoHundredDollarTestWidget",
                ConsentRequired = true,
                DataSources = new List<string>
                {
                    "ValueGenerationEngine v1.0",
                    "PersonalContributionData"
                },
                Permissions = new List<string>
                {
                    "read:personal-data",
                    "read:contribution-history"
                },
                Configuration = new Dictionary<string, object>
                {
                    { "refreshInterval", 3600 }, // Refresh every hour (in seconds)
                    { "showDetailedBreakdown", true }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "iconUrl", "/assets/icons/value-test-icon.svg" },
                    { "priority", "high" },
                    { "timeToComplete", "5-10 minutes" }
                }
            };

            bool success = await _widgetRegistry.RegisterWidgetAsync(widget);
            if (success)
            {
                _logger.LogInformation("Successfully registered $200 Test Widget.");
            }
            else
            {
                _logger.LogWarning("Failed to register $200 Test Widget.");
            }
        }

        /// <summary>
        /// Registers the Value Diagnostic Dashboard with the widget registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RegisterValueDiagnosticDashboardAsync()
        {
            var widget = new WidgetDefinition
            {
                Id = "value-diagnostic-dashboard",
                Title = "Value Diagnostic Dashboard",
                Description = "Comprehensive insights into value creation and organizational blind spots.",
                Version = "1.0.0",
                Author = "Cognitive Mesh Team",
                Category = "Value Generation",
                RenderFunction = "ValueDiagnosticDashboard",
                ConsentRequired = true,
                DataSources = new List<string>
                {
                    "ValueGenerationEngine v1.0",
                    "OrganizationalMetrics",
                    "PersonalContributionData"
                },
                Permissions = new List<string>
                {
                    "read:personal-data",
                    "read:org-metrics",
                    "read:contribution-history"
                },
                Configuration = new Dictionary<string, object>
                {
                    { "refreshInterval", 7200 }, // Refresh every 2 hours (in seconds)
                    { "defaultTimeRange", "last_3_months" },
                    { "defaultDepartment", "all" }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "iconUrl", "/assets/icons/value-dashboard-icon.svg" },
                    { "priority", "medium" },
                    { "widgetSize", "large" }
                }
            };

            bool success = await _widgetRegistry.RegisterWidgetAsync(widget);
            if (success)
            {
                _logger.LogInformation("Successfully registered Value Diagnostic Dashboard.");
            }
            else
            {
                _logger.LogWarning("Failed to register Value Diagnostic Dashboard.");
            }
        }

        /// <summary>
        /// Registers the Organizational Blindness Trends widget with the widget registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RegisterOrgBlindnessTrendsAsync()
        {
            var widget = new WidgetDefinition
            {
                Id = "org-blindness-trends-widget",
                Title = "Organizational Blindness Trends",
                Description = "Identify areas where value creation is overlooked or undervalued within your organization.",
                Version = "1.0.0",
                Author = "Cognitive Mesh Team",
                Category = "Value Generation",
                RenderFunction = "OrgBlindnessTrends",
                ConsentRequired = true,
                DataSources = new List<string>
                {
                    "OrganizationalBlindnessEngine v1.0",
                    "OrganizationalMetrics",
                    "DepartmentData"
                },
                Permissions = new List<string>
                {
                    "read:org-metrics",
                    "read:department-data"
                },
                Configuration = new Dictionary<string, object>
                {
                    { "refreshInterval", 14400 }, // Refresh every 4 hours (in seconds)
                    { "defaultTimeRange", "last_6_months" },
                    { "defaultDepartment", "all" },
                    { "defaultRiskSeverity", "all" }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "iconUrl", "/assets/icons/org-blindness-icon.svg" },
                    { "priority", "medium" },
                    { "widgetSize", "large" },
                    { "recommendedRole", "manager,executive" }
                }
            };

            bool success = await _widgetRegistry.RegisterWidgetAsync(widget);
            if (success)
            {
                _logger.LogInformation("Successfully registered Organizational Blindness Trends widget.");
            }
            else
            {
                _logger.LogWarning("Failed to register Organizational Blindness Trends widget.");
            }
        }

        /// <summary>
        /// Registers the Employability Score Widget with the widget registry.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task RegisterEmployabilityScoreWidgetAsync()
        {
            var widget = new WidgetDefinition
            {
                Id = "employability-score-widget",
                Title = "Employability Score & Development",
                Description = "Understand your current employability risk and get personalized development recommendations.",
                Version = "1.0.0",
                Author = "Cognitive Mesh Team",
                Category = "Value Generation",
                RenderFunction = "EmployabilityScoreWidget",
                ConsentRequired = true, // Explicit consent required for this sensitive widget
                DataSources = new List<string>
                {
                    "EmployabilityPredictorEngine v1.0",
                    "PersonalSkillsData",
                    "MarketTrendsData"
                },
                Permissions = new List<string>
                {
                    "read:personal-data",
                    "read:skills-data",
                    "read:market-trends"
                },
                Configuration = new Dictionary<string, object>
                {
                    { "refreshInterval", 86400 }, // Refresh every 24 hours (in seconds)
                    { "privacyEnhanced", true },
                    { "allowOptOut", true }
                },
                Metadata = new Dictionary<string, string>
                {
                    { "iconUrl", "/assets/icons/employability-icon.svg" },
                    { "priority", "high" },
                    { "widgetSize", "medium" },
                    { "privacyLevel", "enhanced" },
                    { "dataRetention", "30 days" }
                }
            };

            bool success = await _widgetRegistry.RegisterWidgetAsync(widget);
            if (success)
            {
                _logger.LogInformation("Successfully registered Employability Score Widget.");
            }
            else
            {
                _logger.LogWarning("Failed to register Employability Score Widget.");
            }
        }
    }
}
