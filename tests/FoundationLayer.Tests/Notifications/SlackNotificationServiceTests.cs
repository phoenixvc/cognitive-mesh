using System.Net;
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
    /// Unit tests for <see cref="SlackNotificationService"/>.
    /// </summary>
    public class SlackNotificationServiceTests
    {
        private const string ValidWebhookUrl = "https://hooks.example.com/services/T00000000/B00000000/XXXXXXXXXXXXXXXXXXXXXXXX";

        private readonly Mock<ILogger<SlackNotificationService>> _loggerMock;
        private readonly SlackNotificationOptions _defaultOptions;

        public SlackNotificationServiceTests()
        {
            _loggerMock = new Mock<ILogger<SlackNotificationService>>();
            _defaultOptions = new SlackNotificationOptions
            {
                WebhookUrl = ValidWebhookUrl,
                DefaultChannel = "#test-notifications",
                BotUsername = "TestBot",
                IconEmoji = ":test:"
            };
        }

        private static IOptions<SlackNotificationOptions> WrapOptions(SlackNotificationOptions options)
        {
            return Options.Create(options);
        }

        private static NotificationMessage CreateTestNotification(
            NotificationPriority priority = NotificationPriority.Normal,
            string? body = "Test notification body",
            List<NotificationAction>? actionButtons = null)
        {
            return new NotificationMessage
            {
                NotificationId = Guid.NewGuid().ToString(),
                NotificationType = "TestNotification",
                Priority = priority,
                Title = "Test Title",
                Body = body,
                Timestamp = DateTimeOffset.UtcNow,
                Data = new Dictionary<string, object>
                {
                    { "Key1", "Value1" },
                    { "Key2", 42 }
                },
                ActionButtons = actionButtons ?? new List<NotificationAction>()
            };
        }

        #region Constructor Null Guard Tests

        [Fact]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange & Act
            var act = () => new SlackNotificationService(
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
            var act = () => new SlackNotificationService(
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
            var act = () => new SlackNotificationService(
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
        public void Constructor_InvalidWebhookUrl_ThrowsArgumentException(string invalidUrl)
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var options = new SlackNotificationOptions { WebhookUrl = invalidUrl };

            // Act
            var act = () => new SlackNotificationService(
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
        public async Task DeliverNotificationAsync_ValidMessage_MakesHttpCallToWebhookUrl()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.RequestCount.Should().Be(1);
            handler.LastRequestUri.Should().NotBeNull();
            handler.LastRequestUri!.ToString().Should().Be(ValidWebhookUrl);
            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestBody.Should().NotBeNullOrEmpty();

            // Verify the body contains expected Slack payload structure
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.TryGetProperty("blocks", out _).Should().BeTrue();
            payload.RootElement.TryGetProperty("attachments", out _).Should().BeTrue();
            payload.RootElement.GetProperty("channel").GetString().Should().Be("#test-notifications");
            payload.RootElement.GetProperty("username").GetString().Should().Be("TestBot");
            payload.RootElement.GetProperty("icon_emoji").GetString().Should().Be(":test:");
        }

        [Theory]
        [InlineData(NotificationPriority.Critical, "danger")]
        [InlineData(NotificationPriority.High, "warning")]
        [InlineData(NotificationPriority.Normal, "good")]
        [InlineData(NotificationPriority.Low, "#439FE0")]
        public async Task DeliverNotificationAsync_PriorityMapping_UsesCorrectColor(
            NotificationPriority priority, string expectedColor)
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification(priority: priority);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var attachments = payload.RootElement.GetProperty("attachments");
            attachments.GetArrayLength().Should().BeGreaterThan(0);
            attachments[0].GetProperty("color").GetString().Should().Be(expectedColor);
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithActionButtons_IncludesActionsBlock()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);

            var actionButtons = new List<NotificationAction>
            {
                new NotificationAction
                {
                    ActionId = "approve",
                    Label = "Approve",
                    Type = ActionType.Positive,
                    NavigateUrl = "https://example.com/approve"
                },
                new NotificationAction
                {
                    ActionId = "deny",
                    Label = "Deny",
                    Type = ActionType.Negative
                }
            };

            var notification = CreateTestNotification(actionButtons: actionButtons);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var blocks = payload.RootElement.GetProperty("blocks");

            // Find the actions block
            bool foundActionsBlock = false;
            for (int i = 0; i < blocks.GetArrayLength(); i++)
            {
                var block = blocks[i];
                if (block.GetProperty("type").GetString() == "actions")
                {
                    foundActionsBlock = true;
                    var elements = block.GetProperty("elements");
                    elements.GetArrayLength().Should().Be(2);

                    // Verify first button (Positive -> primary style)
                    elements[0].GetProperty("text").GetProperty("text").GetString().Should().Be("Approve");
                    elements[0].GetProperty("style").GetString().Should().Be("primary");
                    elements[0].GetProperty("url").GetString().Should().Be("https://example.com/approve");

                    // Verify second button (Negative -> danger style)
                    elements[1].GetProperty("text").GetProperty("text").GetString().Should().Be("Deny");
                    elements[1].GetProperty("style").GetString().Should().Be("danger");
                }
            }

            foundActionsBlock.Should().BeTrue("an 'actions' block should be present when ActionButtons are provided");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithEmptyBody_OmitsSectionBlock()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification(body: null);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.RequestCount.Should().Be(1);

            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var blocks = payload.RootElement.GetProperty("blocks");

            // Should have header and context, but no section block with body
            bool hasSectionBlock = false;
            for (int i = 0; i < blocks.GetArrayLength(); i++)
            {
                if (blocks[i].GetProperty("type").GetString() == "section")
                {
                    hasSectionBlock = true;
                }
            }

            hasSectionBlock.Should().BeFalse("no section block should be present when body is null/empty");
        }

        [Fact]
        public async Task DeliverNotificationAsync_FailedHttpResponse_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.InternalServerError, "Server Error");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
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
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);

            // Act
            var act = () => service.DeliverNotificationAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("notification");
        }

        [Fact]
        public async Task DeliverNotificationAsync_ValidMessage_IncludesDataFieldsInAttachment()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "ok");
            using var httpClient = new HttpClient(handler);
            var service = new SlackNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var attachments = payload.RootElement.GetProperty("attachments");
            var fields = attachments[0].GetProperty("fields");
            fields.GetArrayLength().Should().Be(2);
            fields[0].GetProperty("title").GetString().Should().Be("Key1");
            fields[0].GetProperty("value").GetString().Should().Be("Value1");
        }

        #endregion

        #region MapPriorityToColor Tests

        [Fact]
        public void MapPriorityToColor_UnspecifiedPriority_ReturnsGood()
        {
            // Act
            var result = SlackNotificationService.MapPriorityToColor(NotificationPriority.Unspecified);

            // Assert
            result.Should().Be("good");
        }

        #endregion
    }

    /// <summary>
    /// A test HTTP message handler that captures request details and returns a configurable response.
    /// </summary>
    public class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseContent;

        /// <summary>
        /// Gets the number of requests that have been sent through this handler.
        /// </summary>
        public int RequestCount { get; private set; }

        /// <summary>
        /// Gets the URI of the last request.
        /// </summary>
        public Uri? LastRequestUri { get; private set; }

        /// <summary>
        /// Gets the HTTP method of the last request.
        /// </summary>
        public HttpMethod? LastRequestMethod { get; private set; }

        /// <summary>
        /// Gets the body of the last request as a string.
        /// </summary>
        public string? LastRequestBody { get; private set; }

        /// <summary>
        /// Gets the headers of the last request.
        /// </summary>
        public System.Net.Http.Headers.HttpRequestHeaders? LastRequestHeaders { get; private set; }

        /// <summary>
        /// Gets the content headers of the last request.
        /// </summary>
        public System.Net.Http.Headers.HttpContentHeaders? LastContentHeaders { get; private set; }

        /// <summary>
        /// Initializes a new instance of <see cref="MockHttpMessageHandler"/>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to return.</param>
        /// <param name="responseContent">The response body content.</param>
        public MockHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
        {
            _statusCode = statusCode;
            _responseContent = responseContent;
            InnerHandler = new HttpClientHandler();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri;
            LastRequestMethod = request.Method;
            LastRequestHeaders = request.Headers;

            if (request.Content != null)
            {
                LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                LastContentHeaders = request.Content.Headers;
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent)
            };
        }
    }
}
