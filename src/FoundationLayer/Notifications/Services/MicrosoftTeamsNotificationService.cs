using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationLayer.Notifications.Services
{
    /// <summary>
    /// Configuration options for the Microsoft Teams notification service.
    /// </summary>
    public class TeamsNotificationOptions
    {
        /// <summary>
        /// The Microsoft Teams incoming webhook connector URL.
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Optional default theme color for message cards (hex code without #).
        /// When not set, the color is determined by notification priority.
        /// </summary>
        public string? ThemeColor { get; set; }
    }

    /// <summary>
    /// Delivers notifications to Microsoft Teams channels via incoming webhook connectors.
    /// Uses the MessageCard format for broad connector compatibility, mapping
    /// <see cref="NotificationMessage"/> instances to cards with facts, sections, and action buttons.
    /// </summary>
    public class MicrosoftTeamsNotificationService : INotificationDeliveryService
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = null
        };

        private readonly HttpClient _httpClient;
        private readonly TeamsNotificationOptions _options;
        private readonly ILogger<MicrosoftTeamsNotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftTeamsNotificationService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to send webhook requests.</param>
        /// <param name="options">Configuration options for the Teams webhook.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the webhook URL is invalid.</exception>
        public MicrosoftTeamsNotificationService(
            HttpClient httpClient,
            IOptions<TeamsNotificationOptions> options,
            ILogger<MicrosoftTeamsNotificationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_options.WebhookUrl) ||
                !Uri.TryCreate(_options.WebhookUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "https" && uri.Scheme != "http"))
            {
                throw new ArgumentException(
                    "WebhookUrl must be a valid absolute HTTP or HTTPS URL.",
                    nameof(options));
            }
        }

        /// <summary>
        /// Delivers a notification to Microsoft Teams by posting a MessageCard payload to the configured webhook.
        /// </summary>
        /// <param name="notification">The notification message to deliver.</param>
        /// <returns>A task representing the asynchronous delivery operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the Teams webhook returns a non-success status code.</exception>
        public async Task DeliverNotificationAsync(NotificationMessage notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var themeColor = _options.ThemeColor ?? MapPriorityToThemeColor(notification.Priority);
            var payload = BuildMessageCardPayload(notification, themeColor);

            var json = JsonSerializer.Serialize(payload, JsonSerializerOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug(
                "Sending Teams notification {NotificationId} with priority {Priority}",
                notification.NotificationId,
                notification.Priority);

            var response = await _httpClient.PostAsync(_options.WebhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Teams webhook returned {StatusCode} for notification {NotificationId}: {ResponseBody}",
                    response.StatusCode,
                    notification.NotificationId,
                    responseBody);

                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation(
                "Successfully delivered Teams notification {NotificationId}",
                notification.NotificationId);
        }

        /// <summary>
        /// Maps a <see cref="NotificationPriority"/> to a Teams MessageCard theme color hex string.
        /// </summary>
        /// <param name="priority">The notification priority.</param>
        /// <returns>A hex color code string (without leading #).</returns>
        internal static string MapPriorityToThemeColor(NotificationPriority priority)
        {
            return priority switch
            {
                NotificationPriority.Critical => "FF0000",
                NotificationPriority.High => "FFA500",
                NotificationPriority.Normal => "00FF00",
                NotificationPriority.Low => "439FE0",
                _ => "00FF00"
            };
        }

        /// <summary>
        /// Builds the MessageCard payload for the Teams webhook.
        /// </summary>
        private Dictionary<string, object> BuildMessageCardPayload(NotificationMessage notification, string themeColor)
        {
            // Build facts from notification data
            var facts = new List<object>();
            if (notification.Data != null)
            {
                foreach (var kvp in notification.Data)
                {
                    facts.Add(new Dictionary<string, object>
                    {
                        ["name"] = kvp.Key,
                        ["value"] = kvp.Value?.ToString() ?? string.Empty
                    });
                }
            }

            // Always include priority and timestamp as facts
            facts.Add(new Dictionary<string, object>
            {
                ["name"] = "Priority",
                ["value"] = notification.Priority.ToString()
            });

            facts.Add(new Dictionary<string, object>
            {
                ["name"] = "Timestamp",
                ["value"] = notification.Timestamp.ToString("O")
            });

            // Build the section
            var section = new Dictionary<string, object>
            {
                ["activityTitle"] = notification.Title ?? "Notification",
                ["activitySubtitle"] = notification.NotificationType ?? string.Empty,
                ["facts"] = facts,
                ["markdown"] = true
            };

            if (!string.IsNullOrEmpty(notification.Body))
            {
                section["text"] = notification.Body;
            }

            // Build potential actions
            var potentialActions = new List<object>();
            if (notification.ActionButtons != null && notification.ActionButtons.Count > 0)
            {
                foreach (var action in notification.ActionButtons)
                {
                    if (!string.IsNullOrEmpty(action.NavigateUrl))
                    {
                        potentialActions.Add(new Dictionary<string, object>
                        {
                            ["@type"] = "OpenUri",
                            ["name"] = action.Label ?? action.ActionId,
                            ["targets"] = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["os"] = "default",
                                    ["uri"] = action.NavigateUrl
                                }
                            }
                        });
                    }
                    else
                    {
                        // For actions without URLs, use ActionCard with HttpPOST
                        potentialActions.Add(new Dictionary<string, object>
                        {
                            ["@type"] = "ActionCard",
                            ["name"] = action.Label ?? action.ActionId,
                            ["inputs"] = Array.Empty<object>(),
                            ["actions"] = new List<object>
                            {
                                new Dictionary<string, object>
                                {
                                    ["@type"] = "HttpPOST",
                                    ["name"] = action.Label ?? action.ActionId,
                                    ["target"] = $"action://{action.ActionId}"
                                }
                            }
                        });
                    }
                }
            }

            var payload = new Dictionary<string, object>
            {
                ["@type"] = "MessageCard",
                ["@context"] = "http://schema.org/extensions",
                ["themeColor"] = themeColor,
                ["summary"] = notification.Title ?? "Notification",
                ["sections"] = new List<object> { section }
            };

            if (potentialActions.Count > 0)
            {
                payload["potentialAction"] = potentialActions;
            }

            return payload;
        }
    }

}
