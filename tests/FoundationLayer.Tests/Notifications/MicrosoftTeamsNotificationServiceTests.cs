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
    /// Unit tests for <see cref="MicrosoftTeamsNotificationService"/>.
    /// </summary>
    public class MicrosoftTeamsNotificationServiceTests
    {
        private const string ValidWebhookUrl = "https://outlook.office.com/webhook/test-webhook-id";

        private readonly Mock<ILogger<MicrosoftTeamsNotificationService>> _loggerMock;
        private readonly TeamsNotificationOptions _defaultOptions;

        public MicrosoftTeamsNotificationServiceTests()
        {
            _loggerMock = new Mock<ILogger<MicrosoftTeamsNotificationService>>();
            _defaultOptions = new TeamsNotificationOptions
            {
                WebhookUrl = ValidWebhookUrl
            };
        }

        private static IOptions<TeamsNotificationOptions> WrapOptions(TeamsNotificationOptions options)
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
                    { "AgentId", "agent-123" },
                    { "TaskId", "task-456" }
                },
                ActionButtons = actionButtons ?? new List<NotificationAction>()
            };
        }

        #region Constructor Null Guard Tests

        [Fact]
        public void Constructor_NullHttpClient_ThrowsArgumentNullException()
        {
            // Arrange & Act
            var act = () => new MicrosoftTeamsNotificationService(
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
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);

            // Act
            var act = () => new MicrosoftTeamsNotificationService(
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
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);

            // Act
            var act = () => new MicrosoftTeamsNotificationService(
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
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var options = new TeamsNotificationOptions { WebhookUrl = invalidUrl };

            // Act
            var act = () => new MicrosoftTeamsNotificationService(
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
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            handler.RequestCount.Should().Be(1);
            handler.LastRequestUri.Should().NotBeNull();
            handler.LastRequestUri!.ToString().Should().Be(ValidWebhookUrl);
            handler.LastRequestMethod.Should().Be(HttpMethod.Post);
            handler.LastRequestBody.Should().NotBeNullOrEmpty();

            // Verify the body contains expected MessageCard structure
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.GetProperty("@type").GetString().Should().Be("MessageCard");
            payload.RootElement.GetProperty("@context").GetString().Should().Be("http://schema.org/extensions");
            payload.RootElement.GetProperty("summary").GetString().Should().Be("Test Title");
            payload.RootElement.TryGetProperty("sections", out _).Should().BeTrue();
        }

        [Theory]
        [InlineData(NotificationPriority.Critical, "FF0000")]
        [InlineData(NotificationPriority.High, "FFA500")]
        [InlineData(NotificationPriority.Normal, "00FF00")]
        [InlineData(NotificationPriority.Low, "439FE0")]
        public async Task DeliverNotificationAsync_PriorityMapping_UsesCorrectThemeColor(
            NotificationPriority priority, string expectedColor)
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification(priority: priority);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.GetProperty("themeColor").GetString().Should().Be(expectedColor);
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithCustomThemeColor_UsesCustomColorOverPriority()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var options = new TeamsNotificationOptions
            {
                WebhookUrl = ValidWebhookUrl,
                ThemeColor = "AABBCC"
            };
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(options), _loggerMock.Object);
            var notification = CreateTestNotification(priority: NotificationPriority.Critical);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.GetProperty("themeColor").GetString().Should().Be("AABBCC");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithActionButtons_IncludesPotentialActions()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);

            var actionButtons = new List<NotificationAction>
            {
                new NotificationAction
                {
                    ActionId = "view-details",
                    Label = "View Details",
                    Type = ActionType.Neutral,
                    NavigateUrl = "https://example.com/details"
                },
                new NotificationAction
                {
                    ActionId = "approve",
                    Label = "Approve",
                    Type = ActionType.Positive
                }
            };

            var notification = CreateTestNotification(actionButtons: actionButtons);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.TryGetProperty("potentialAction", out var potentialActions).Should().BeTrue();
            potentialActions.GetArrayLength().Should().Be(2);

            // First action has URL => OpenUri type
            potentialActions[0].GetProperty("@type").GetString().Should().Be("OpenUri");
            potentialActions[0].GetProperty("name").GetString().Should().Be("View Details");

            // Second action has no URL => ActionCard type
            potentialActions[1].GetProperty("@type").GetString().Should().Be("ActionCard");
            potentialActions[1].GetProperty("name").GetString().Should().Be("Approve");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithDataDictionary_IncludesFactsInSection()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var sections = payload.RootElement.GetProperty("sections");
            sections.GetArrayLength().Should().BeGreaterThan(0);
            var facts = sections[0].GetProperty("facts");

            // Should have Data entries (2) + Priority + Timestamp = 4
            facts.GetArrayLength().Should().Be(4);

            // First two facts should be from Data dictionary
            facts[0].GetProperty("name").GetString().Should().Be("AgentId");
            facts[0].GetProperty("value").GetString().Should().Be("agent-123");
            facts[1].GetProperty("name").GetString().Should().Be("TaskId");
            facts[1].GetProperty("value").GetString().Should().Be("task-456");
        }

        [Fact]
        public async Task DeliverNotificationAsync_FailedHttpResponse_ThrowsHttpRequestException()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, "Bad Request");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
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
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);

            // Act
            var act = () => service.DeliverNotificationAsync(null!);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithParameterName("notification");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithEmptyBody_SectionOmitsText()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification(body: null);

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            var sections = payload.RootElement.GetProperty("sections");
            sections[0].TryGetProperty("text", out _).Should().BeFalse(
                "the section should not contain a 'text' property when body is null");
        }

        [Fact]
        public async Task DeliverNotificationAsync_WithNoActionButtons_OmitsPotentialAction()
        {
            // Arrange
            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "1");
            using var httpClient = new HttpClient(handler);
            var service = new MicrosoftTeamsNotificationService(httpClient, WrapOptions(_defaultOptions), _loggerMock.Object);
            var notification = CreateTestNotification();

            // Act
            await service.DeliverNotificationAsync(notification);

            // Assert
            var payload = JsonDocument.Parse(handler.LastRequestBody!);
            payload.RootElement.TryGetProperty("potentialAction", out _).Should().BeFalse(
                "potentialAction should not be present when there are no action buttons");
        }

        #endregion

        #region MapPriorityToThemeColor Tests

        [Fact]
        public void MapPriorityToThemeColor_UnspecifiedPriority_ReturnsGreenHex()
        {
            // Act
            var result = MicrosoftTeamsNotificationService.MapPriorityToThemeColor(NotificationPriority.Unspecified);

            // Assert
            result.Should().Be("00FF00");
        }

        #endregion
    }
}
