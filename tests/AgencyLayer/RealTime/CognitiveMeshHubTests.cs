using CognitiveMesh.AgencyLayer.RealTime.Hubs;
using CognitiveMesh.AgencyLayer.RealTime.Ports;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.RealTime;

/// <summary>
/// Unit tests for <see cref="CognitiveMeshHub"/>, covering constructor validation,
/// connection lifecycle, and group management operations.
/// </summary>
public class CognitiveMeshHubTests
{
    private readonly Mock<ILogger<CognitiveMeshHub>> _loggerMock;
    private readonly Mock<IRealTimeNotificationPort> _notificationPortMock;

    public CognitiveMeshHubTests()
    {
        _loggerMock = new Mock<ILogger<CognitiveMeshHub>>();
        _notificationPortMock = new Mock<IRealTimeNotificationPort>();
    }

    private CognitiveMeshHub CreateHub()
    {
        return new CognitiveMeshHub(_loggerMock.Object, _notificationPortMock.Object);
    }

    private static Mock<HubCallerContext> CreateMockContext(string connectionId = "conn-1", string? userIdentifier = "user-1")
    {
        var contextMock = new Mock<HubCallerContext>();
        contextMock.Setup(c => c.ConnectionId).Returns(connectionId);
        contextMock.Setup(c => c.UserIdentifier).Returns(userIdentifier);
        return contextMock;
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CognitiveMeshHub(null!, _notificationPortMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullNotificationPort_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new CognitiveMeshHub(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("notificationPort");
    }

    // -----------------------------------------------------------------------
    // OnConnectedAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OnConnectedAsync_ValidConnection_AddsToAllUsersGroupAndLogs()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        var clientsMock = new Mock<IHubCallerClients<ICognitiveMeshHubClient>>();
        hub.Clients = clientsMock.Object;

        _notificationPortMock
            .Setup(p => p.AddToGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.OnConnectedAsync();

        // Assert
        _notificationPortMock.Verify(
            p => p.AddToGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // OnDisconnectedAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task OnDisconnectedAsync_NormalDisconnect_RemovesFromAllUsersGroup()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        var clientsMock = new Mock<IHubCallerClients<ICognitiveMeshHubClient>>();
        hub.Clients = clientsMock.Object;

        _notificationPortMock
            .Setup(p => p.RemoveFromGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.OnDisconnectedAsync(null);

        // Assert
        _notificationPortMock.Verify(
            p => p.RemoveFromGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task OnDisconnectedAsync_WithException_RemovesFromGroupAndLogs()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        var clientsMock = new Mock<IHubCallerClients<ICognitiveMeshHubClient>>();
        hub.Clients = clientsMock.Object;

        _notificationPortMock
            .Setup(p => p.RemoveFromGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var exception = new InvalidOperationException("Connection lost");

        // Act
        await hub.OnDisconnectedAsync(exception);

        // Assert
        _notificationPortMock.Verify(
            p => p.RemoveFromGroupAsync("conn-1", "all-users", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // JoinDashboardGroup tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task JoinDashboardGroup_ValidDashboardId_AddsToGroup()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        _notificationPortMock
            .Setup(p => p.AddToGroupAsync("conn-1", "dashboard-dash-42", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.JoinDashboardGroup("dash-42");

        // Assert
        _notificationPortMock.Verify(
            p => p.AddToGroupAsync("conn-1", "dashboard-dash-42", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task JoinDashboardGroup_InvalidDashboardId_ThrowsArgumentException(string? dashboardId)
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        // Act
        var act = () => hub.JoinDashboardGroup(dashboardId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("dashboardId");
    }

    // -----------------------------------------------------------------------
    // LeaveDashboardGroup tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task LeaveDashboardGroup_ValidDashboardId_RemovesFromGroup()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        _notificationPortMock
            .Setup(p => p.RemoveFromGroupAsync("conn-1", "dashboard-dash-42", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.LeaveDashboardGroup("dash-42");

        // Assert
        _notificationPortMock.Verify(
            p => p.RemoveFromGroupAsync("conn-1", "dashboard-dash-42", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task LeaveDashboardGroup_InvalidDashboardId_ThrowsArgumentException(string? dashboardId)
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        // Act
        var act = () => hub.LeaveDashboardGroup(dashboardId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("dashboardId");
    }

    // -----------------------------------------------------------------------
    // SubscribeToAgent tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubscribeToAgent_ValidAgentId_AddsToAgentGroup()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        _notificationPortMock
            .Setup(p => p.AddToGroupAsync("conn-1", "agent-agent-99", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.SubscribeToAgent("agent-99");

        // Assert
        _notificationPortMock.Verify(
            p => p.AddToGroupAsync("conn-1", "agent-agent-99", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SubscribeToAgent_InvalidAgentId_ThrowsArgumentException(string? agentId)
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        // Act
        var act = () => hub.SubscribeToAgent(agentId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("agentId");
    }

    // -----------------------------------------------------------------------
    // UnsubscribeFromAgent tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UnsubscribeFromAgent_ValidAgentId_RemovesFromAgentGroup()
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        _notificationPortMock
            .Setup(p => p.RemoveFromGroupAsync("conn-1", "agent-agent-99", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await hub.UnsubscribeFromAgent("agent-99");

        // Assert
        _notificationPortMock.Verify(
            p => p.RemoveFromGroupAsync("conn-1", "agent-agent-99", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UnsubscribeFromAgent_InvalidAgentId_ThrowsArgumentException(string? agentId)
    {
        // Arrange
        var hub = CreateHub();
        var contextMock = CreateMockContext();
        hub.Context = contextMock.Object;

        // Act
        var act = () => hub.UnsubscribeFromAgent(agentId!);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("agentId");
    }
}
