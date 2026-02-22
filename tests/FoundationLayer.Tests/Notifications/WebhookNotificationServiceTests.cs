using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using FoundationLayer.Notifications;
using FoundationLayer.Notifications.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace FoundationLayer.Tests.Notifications
{
    /// <summary>
    /// Unit tests for <see cref="WebhookNotificationService"/>.
    /// </summary>
    public class WebhookNotificationServiceTests
    {
        private const string ValidEndpointUrl = "https://api.example.com/webhooks/notifications";
        private const string TestSecret = "my-super-secret-key-for-hmac";

        private readonly Mock<ILogger<WebhookNotificationService>> _loggerMock;
        private readonly WebhookNotificationOptions _defaultOptions;

        public WebhookNotificationServiceTests()
        {
            _loggerMock = new Mock<ILogger<WebhookNotificationService>>();
            _defaultOptions = new WebhookNotificationOptions
            {
                EndpointUrl = ValidEndpointUrl,
                Secret = TestSecret,
                TimeoutSeconds = 30,
                CustomHeaders = new Dictionary<string, string>()
            };
        }

        private static IOptions<WebhookNotificationOptions> WrapOptions(WebhookNotificationOptions options)
        {
            return Options.Create(options);
        }

        private static NotificationMessage CreateTestNotification(
            NotificationPriority priority = NotificationPriority.Normal)
        {
            return new NotificationMessage
            {
                NotificationId = "test-notification-001",
                NotificationType = "TestNotification",
                Priority = priority,
                Title = "Test Webhook Title",
                Body = "Test webhook notification body",
                Timestamp = DateTimeOffset.UtcNow,
                Data = new Dictionary<string, object>
                {
                    { "Key1", "Value1" },
                    { "Key2", 42 }
                }
            };
        }

        #region Constructor Null Guard Tests

        [Fact]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange & Act
            var act = () => new WebhookNotificationService(
                null!,
                WrapOptions(_defaultOptions),
                _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("httpClient");
        }

        [Fact]
        public void Constructor_NullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);

            // Act
            var act = () => new WebhookNotificationService(
                httpClient,
                null!,
                _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("options");
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);

            // Act
            var act = () => new WebhookNotificationService(
                httpClient,
                WrapOptions(_defaultOptions),
                null!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-url")]
        [InlineData("ftp://invalid-scheme.com")]
        public void Constructor_InvalidEndpointUrl_ThrowsArgumentException(string invalidUrl)
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var options = new WebhookNotificationOptions { EndpointUrl = invalidUrl };

            // Act
            var act = () => new WebhookNotificationService(
                httpClient,
                WrapOptions(options),
                _loggerMock.Object);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("options");
        }

        #endregion

        #region DeliverNotificationAsync Tests

        [Fact]
        public async Task DeliverNotificationAsync_ValidMessage_MakesHttpPostToEndpoint()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.RequestCount.Should().Be(1);
            handler.LastRequestUri.Should().NotBeNull();
            handler.LastRequestUri!.ToString().Should().Be(ValidEndpointUrl);
            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestBody.Should().NotBeNullOrEmpty();

            // Verify the body is a serialized NotificationMessage
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.TryGetProperty("NotificationId", out var idElement).Should().BeTrue();
            idElement.GetString().Should().Be("test-notification-001");
        }

        [Fact]
        public async Task DeliverNotificationAsync_ValidMessage_IncludesNotificationIdHeader()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.LastRequestHeaders.Should().NotBeNull();
            handler.LastRequestHeaders!.TryGetValues("X-Notification-Id", out var values).Should().BeTrue();
            values.Should().ContainSingle().Which.Should().Be("test-notification-001");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithSecret_IncludesValidHmacSignatureHeader()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.LastRequestHeaders.Should().NotBeNull();
            handler.LastRequestHeaders!.TryGetValues("X-Webhook-Signature", out var values).Should().BeTrue();
            var signatureHeader = values!.Single();
            signatureHeader.Should().StartWith("sha256=");

            // Independently compute the expected HMAC to verify
            var bodyBytes = Encoding.UTF8.GetBytes(handler.LastRequestBody!);
            var keyBytes = Encoding.UTF8.GetBytes(TestSecret);
            using var hmac = new HMACSHA256(keyBytes);
            var expectedHash = Convert.ToHexStringLower(hmac.ComputeHash(bodyBytes));

            signatureHeader.Should().Be($"sha256={expectedHash}");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithoutSecret_OmitsSignatureHeader()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var options = new WebhookNotificationOptions
            {
                EndpointUrl = ValidEndpointUrl,
                Secret = null,
                TimeoutSeconds = 30
            };
            var service = new WebhookNotificationService(httpClient, WrapOptions(options), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.LastRequestHeaders!.TryGetValues("X-Webhook-Signature", out _).Should().BeFalse(
                "X-Webhook-Signature should not be present when no secret is configured");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithCustomHeaders_IncludesCustomHeadersInRequest()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var options = new WebhookNotificationOptions
            {
                EndpointUrl = ValidEndpointUrl,
                Secret = TestSecret,
                TimeoutSeconds = 30,
                CustomHeaders = new Dictionary<string, string>
                {
                    { "X-Custom-Header", "custom-value" },
                    { "X-Tenant-Id", "tenant-123" }
                }
            };
            var service = new WebhookNotificationService(httpClient, WrapOptions(options), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.LastRequestHeaders!.TryGetValues("X-Custom-Header", out var customValues).Should().BeTrue();
            customValues.Should().ContainSingle().Which.Should().Be("custom-value");

            handler.LastRequestHeaders!.TryGetValues("X-Tenant-Id", out var tenantValues).Should().BeTrue();
            tenantValues.Should().ContainSingle().Which.Should().Be("tenant-123");
        }

        [Fact]
        public async Task DeliverNotificationAsync_FailedHttpResponse_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "Internal Server Error");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            var act = () => service.DeliverNotificationAsync(notification);

            // Assert
            await act.Should().ThrowAsync<HttpRequestException>();
        }

        [Fact]
        public async Task DeliverNotificationAsync_NullNotification_ThrowsArgumentNullException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);

            // Act
            var act = () => service.DeliverNotificationAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("notification");
        }

        [Fact]
        public async Task DeliverNotificationAsync_Timeout_ThrowsTimeoutException()
        {
            // Arrange: handler that delays longer than the configured timeout
            var handler = new DelayingHttpMessageHandler(
                TimeSpan.FromSeconds(10),
                HttpStatusCode.OK,
                "ok");
            using var httpClient = new HttpClient(handler);
            var options = new WebhookNotificationOptions
            {
                EndpointUrl = ValidEndpointUrl,
                Secret = TestSecret,
                TimeoutSeconds = 1 // Very short timeout
            };
            var service = new WebhookNotificationService(httpClient, WrapOptions(options), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            var act = () => service.DeliverNotificationAsync(notification);

            // Assert
            await act.Should().ThrowAsync<TimeoutException>();
        }

        [Theory]
        [InlineData(HttpStatusCode.OK)]
        [InlineData(HttpStatusCode.Created)]
        [InlineData(HttpStatusCode.Accepted)]
        [InlineData(HttpStatusCode.NoContent)]
        public async Task DeliverNotificationAsync_SuccessStatusCodes_DoNotThrow(HttpStatusCode statusCode)
        {
            // Arrange
            var handler = new MockHttpMessageHandler(statusCode, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            var act = () => service.DeliverNotificationAsync(notification);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeliverNotificationAsync_SerializesFullNotificationMessage()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new WebhookNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();
            notification.ActionButtons = new List<NotificationAction>
            {
                new NotificationAction
                {
                    ActionId = "test-action",
                    Label = "Test",
                    Type = ActionType.Default
                }
            };

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.TryGetProperty("Title", out var title).Should().BeTrue();
            title.GetString().Should().Be("Test Webhook Title");
            payload.RootElement.TryGetProperty("Body", out var body).Should().BeTrue();
            body.GetString().Should().Be("Test webhook notification body");
            payload.RootElement.TryGetProperty("Priority", out _).Should().BeTrue();
        }

        #endregion

        #region ComputeHmacSha256 Tests

        [Fact]
        public void ComputeHmacSha256_KnownInput_ProducesExpectedOutput()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes("test-payload");
            var secret = "test-secret";

            // Compute expected hash independently
            var keyBytes = Encoding.UTF8.GetBytes(secret);
            using var hmac = new HMACSHA256(keyBytes);
            var expectedHash = Convert.ToHexStringLower(hmac.ComputeHash(payload));

            // Act
            var result = WebhookNotificationService.ComputeHmacSha256(payload, secret);

            // Assert
            result.Should().Be(expectedHash);
            result.Should().NotBeNullOrEmpty();
            result.Should().MatchRegex("^[0-9a-f]+$", "HMAC should be lowercase hex");
        }

        [Fact]
        public void ComputeHmacSha256_DifferentSecrets_ProduceDifferentHashes()
        {
            // Arrange
            var payload = Encoding.UTF8.GetBytes("same-payload");

            // Act
            var hash1 = WebhookNotificationService.ComputeHmacSha256(payload, "secret-1");
            var hash2 = WebhookNotificationService.ComputeHmacSha256(payload, "secret-2");

            // Assert
            hash1.Should().NotBe(hash2);
        }

        [Fact]
        public void ComputeHmacSha256_DifferentPayloads_ProduceDifferentHashes()
        {
            // Arrange
            var payload1 = Encoding.UTF8.GetBytes("payload-1");
            var payload2 = Encoding.UTF8.GetBytes("payload-2");
            var secret = "shared-secret";

            // Act
            var hash1 = WebhookNotificationService.ComputeHmacSha256(payload1, secret);
            var hash2 = WebhookNotificationService.ComputeHmacSha256(payload2, secret);

            // Assert
            hash1.Should().NotBe(hash2);
        }

        #endregion
    }

    /// <summary>
    /// A test HTTP message handler that introduces an artificial delay before responding,
    /// used to test timeout behavior.
    /// </summary>
    public class DelayingHttpMessageHandler : DelegatingHandler
    {
        private readonly TimeSpan _delay;
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        /// <summary>
        /// Initializes a new instance of <see cref="DelayingHttpMessageHandler"/>.
        /// </summary>
        /// <param name="delay">The delay to introduce before responding.</param>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <param name="responseContent">The response body content.</param>
        public DelayingHttpMessageHandler(TimeSpan delay, HttpStatusCode statusCode, string responseContent)
        {
            _delay = delay;
            _statusCode = statusCode;
            _responseContent = responseContent;
            InnerHandler = new HttpClientHandler();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await Task.Delay(_delay, cancellationToken);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };
        }
    }
}
