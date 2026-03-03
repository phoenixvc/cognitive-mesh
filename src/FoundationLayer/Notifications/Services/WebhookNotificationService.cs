using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FoundationLayer.Notifications.Services
{
    /// <summary>
    /// Configuration options for the generic webhook notification service.
    /// </summary>
    public class WebhookNotificationOptions
    {
        /// <summary>
        /// The HTTP endpoint URL to which notification payloads are POSTed.
        /// </summary>
        public string EndpointUrl { get; set; } = string.Empty;

        /// <summary>
        /// The shared secret used to compute HMAC-SHA256 signatures for payload integrity.
        /// When provided, the signature is included in the <c>X-Webhook-Signature</c> header.
        /// </summary>
        public string? Secret { get; set; }

        /// <summary>
        /// The timeout in seconds for each webhook HTTP request. Defaults to 30 seconds.
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Custom HTTP headers to include with every webhook request.
        /// </summary>
        public Dictionary<string, string> CustomHeaders { get; set; } = new();
    }

    /// <summary>
    /// Delivers notifications via generic HTTP POST webhooks.
    /// Serializes the full <see cref="NotificationMessage"/> as JSON, signs the payload
    /// with HMAC-SHA256 when a secret is configured, and includes configurable custom headers.
    /// </summary>
    public class WebhookNotificationService : INotificationDeliveryService
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = null
        };

        private readonly HttpClient _httpClient;
        private readonly WebhookNotificationOptions _options;
        private readonly ILogger<WebhookNotificationService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebhookNotificationService"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client used to send webhook requests.</param>
        /// <param name="options">Configuration options for the webhook endpoint.</param>
        /// <param name="logger">The logger instance for structured logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the endpoint URL is invalid.</exception>
        public WebhookNotificationService(
            HttpClient httpClient,
            IOptions<WebhookNotificationOptions> options,
            ILogger<WebhookNotificationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(_options.EndpointUrl) ||
                !Uri.TryCreate(_options.EndpointUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "https" && uri.Scheme != "http"))
            {
                throw new ArgumentException(
                    "EndpointUrl must be a valid absolute HTTP or HTTPS URL.",
                    nameof(options));
            }
        }

        /// <summary>
        /// Delivers a notification by POSTing the serialized <see cref="NotificationMessage"/>
        /// to the configured webhook endpoint.
        /// </summary>
        /// <param name="notification">The notification message to deliver.</param>
        /// <returns>A task representing the asynchronous delivery operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="notification"/> is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the webhook endpoint returns a non-success status code.</exception>
        public async Task DeliverNotificationAsync(NotificationMessage notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            var json = JsonSerializer.Serialize(notification, JsonSerializerOptions);
            var bodyBytes = Encoding.UTF8.GetBytes(json);

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.EndpointUrl)
            {
                Content = new ByteArrayContent(bodyBytes)
            };

            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };

            // Add notification ID header
            request.Headers.TryAddWithoutValidation("X-Notification-Id", notification.NotificationId ?? string.Empty);

            // Compute and add HMAC-SHA256 signature if secret is configured
            if (!string.IsNullOrEmpty(_options.Secret))
            {
                var signature = ComputeHmacSha256(bodyBytes, _options.Secret);
                request.Headers.TryAddWithoutValidation("X-Webhook-Signature", $"sha256={signature}");
            }

            // Add custom headers
            if (_options.CustomHeaders != null)
            {
                foreach (var header in _options.CustomHeaders)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            _logger.LogDebug(
                "Sending webhook notification {NotificationId} to {EndpointUrl}",
                notification.NotificationId,
                _options.EndpointUrl);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.SendAsync(request, cts.Token);
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                _logger.LogError(
                    "Webhook request timed out after {TimeoutSeconds}s for notification {NotificationId}",
                    _options.TimeoutSeconds,
                    notification.NotificationId);
                throw new TimeoutException(
                    $"Webhook request to {_options.EndpointUrl} timed out after {_options.TimeoutSeconds} seconds.");
            }

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Webhook returned {StatusCode} for notification {NotificationId}: {ResponseBody}",
                    response.StatusCode,
                    notification.NotificationId,
                    responseBody);

                response.EnsureSuccessStatusCode();
            }

            _logger.LogInformation(
                "Successfully delivered webhook notification {NotificationId}",
                notification.NotificationId);
        }

        /// <summary>
        /// Computes an HMAC-SHA256 hash of the given payload bytes using the specified secret.
        /// </summary>
        /// <param name="payload">The payload bytes to sign.</param>
        /// <param name="secret">The secret key for HMAC computation.</param>
        /// <returns>The lowercase hexadecimal representation of the HMAC hash.</returns>
        internal static string ComputeHmacSha256(byte[] payload, string secret)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var hashBytes = hmac.ComputeHash(payload);
            return Convert.ToHexStringLower(hashBytes);
        }
    }

}
