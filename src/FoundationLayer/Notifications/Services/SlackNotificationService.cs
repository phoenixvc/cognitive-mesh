using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationLayer.Notifications.Services
{
    /// <summary>
    /// Configuration options for the Slack notification service.
    /// </summary>
    public class SlackNotificationOptions
    {
        /// <summary>
        /// The Slack incoming webhook URL used to post messages.
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// The default Slack channel to post notifications to (e.g., "#general").
        /// </summary>
        public string? DefaultChannel { get; set; }

        /// <summary>
        /// The bot username displayed in Slack for posted messages.
        /// </summary>
        public string BotUsername { get; set; } = "Cognitive Mesh";

        /// <summary>
        /// The emoji icon displayed next to the bot username (e.g., ":robot_face:").
        /// </summary>
        public string IconEmoji { get; set; } = ":robot_face:";
    }

    /// <summary>
    /// Delivers notifications to Slack channels via incoming webhook integration.
    /// Maps <see cref="NotificationMessage"/> instances to Slack Block Kit payloads
    /// with rich formatting including headers, sections, context blocks, and action buttons.
    /// </summary>
    public class SlackNotificationService : INotificationDeliveryService
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = null
        };

        private readonly HttpClient _httpClient;
        private readonly SlackNotificationOptions _options;
        private readonly ILogger<SlackNotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to send webhook requests.</param>
        /// <param name="options">Configuration options for the Slack webhook.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the webhook URL is invalid.</exception>
        public SlackNotificationService(
            HttpClient httpClient,
            IOptions<SlackNotificationOptions> options,
            ILogger<SlackNotificationService> logger)
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
        /// Delivers a notification to Slack by posting a Block Kit payload to the configured webhook.
        /// </summary>
        /// <param name="notification">The notification message to deliver.</param>
        /// <returns>A task representing the asynchronous delivery operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the Slack API returns a non-success status code.</exception>
        public async Task DeliverNotificationAsync(NotificationMessage notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var color = MapPriorityToColor(notification.Priority);
            var payload = BuildSlackPayload(notification, color);

            var json = JsonSerializer.Serialize(payload, JsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogDebug(
                "Sending Slack notification {NotificationId} with priority {Priority}",
                notification.NotificationId,
                notification.Priority);

            var response = await _httpClient.PostAsync(_options.WebhookUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Slack webhook returned {StatusCode} for notification {NotificationId}: {ResponseBody}",
                    response.StatusCode,
                    notification.NotificationId,
                    responseBody);

                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation(
                "Successfully delivered Slack notification {NotificationId}",
                notification.NotificationId);
        }

        /// <summary>
        /// Maps a <see cref="NotificationPriority"/> to a Slack attachment color string.
        /// </summary>
        /// <param name="priority">The notification priority.</param>
        /// <returns>A color string compatible with Slack attachments.</returns>
        internal static string MapPriorityToColor(NotificationPriority priority)
        {
            return priority switch
            {
                NotificationPriority.Critical => "danger",
                NotificationPriority.High => "warning",
                NotificationPriority.Normal => "good",
                NotificationPriority.Low => "#439FE0",
                _ => "good"
            };
        }

        /// <summary>
        /// Builds the full Slack webhook payload including blocks and attachments.
        /// </summary>
        private Dictionary<string, object> BuildSlackPayload(NotificationMessage notification, string color)
        {
            var blocks = new List<object>();

            // Header block with title
            blocks.Add(new Dictionary<string, object>
            {
                ["type"] = "header",
                ["text"] = new Dictionary<string, object>
                {
                    ["type"] = "plain_text",
                    ["text"] = notification.Title ?? "Notification",
                    ["emoji"] = true
                }
            });

            // Section block with body
            if (!string.IsNullOrEmpty(notification.Body))
            {
                blocks.Add(new Dictionary<string, object>
                {
                    ["type"] = "section",
                    ["text"] = new Dictionary<string, object>
                    {
                        ["type"] = "mrkdwn",
                        ["text"] = notification.Body
                    }
                });
            }

            // Context block with priority and timestamp
            var contextElements = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["type"] = "mrkdwn",
                    ["text"] = $"*Priority:* {notification.Priority}"
                },
                new Dictionary<string, object>
                {
                    ["type"] = "mrkdwn",
                    ["text"] = $"*Timestamp:* {notification.Timestamp:O}"
                }
            };

            blocks.Add(new Dictionary<string, object>
            {
                ["type"] = "context",
                ["elements"] = contextElements
            });

            // Actions block for action buttons
            if (notification.ActionButtons != null && notification.ActionButtons.Count > 0)
            {
                var actionElements = new List<object>();
                foreach (var action in notification.ActionButtons)
                {
                    var buttonStyle = action.Type switch
                    {
                        ActionType.Positive => "primary",
                        ActionType.Negative => "danger",
                        _ => (string?)null
                    };

                    var button = new Dictionary<string, object>
                    {
                        ["type"] = "button",
                        ["text"] = new Dictionary<string, object>
                        {
                            ["type"] = "plain_text",
                            ["text"] = (object)(action.Label ?? action.ActionId ?? string.Empty)
                        },
                        ["action_id"] = action.ActionId ?? Guid.NewGuid().ToString()
                    };

                    if (buttonStyle != null)
                    {
                        button["style"] = buttonStyle;
                    }

                    if (!string.IsNullOrEmpty(action.NavigateUrl))
                    {
                        button["url"] = action.NavigateUrl;
                    }

                    actionElements.Add(button);
                }

                blocks.Add(new Dictionary<string, object>
                {
                    ["type"] = "actions",
                    ["elements"] = actionElements
                });
            }

            // Build attachment fields from notification data
            var fields = new List<object>();
            if (notification.Data != null)
            {
                foreach (var kvp in notification.Data)
                {
                    fields.Add(new Dictionary<string, object>
                    {
                        ["title"] = kvp.Key,
                        ["value"] = kvp.Value?.ToString() ?? string.Empty,
                        ["short"] = true
                    });
                }
            }

            // Build the full payload
            var payload = new Dictionary<string, object>
            {
                ["blocks"] = blocks,
                ["attachments"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["color"] = color,
                        ["fields"] = fields
                    }
                }
            };

            if (!string.IsNullOrEmpty(_options.DefaultChannel))
            {
                payload["channel"] = _options.DefaultChannel;
            }

            if (!string.IsNullOrEmpty(_options.BotUsername))
            {
                payload["username"] = _options.BotUsername;
            }

            if (!string.IsNullOrEmpty(_options.IconEmoji))
            {
                payload["icon_emoji"] = _options.IconEmoji;
            }

            return payload;
        }
    }

}
