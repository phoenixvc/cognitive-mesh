using CognitiveMesh.AgencyLayer.RealTime.Adapters;
using CognitiveMesh.AgencyLayer.RealTime.Hubs;
using CognitiveMesh.AgencyLayer.RealTime.Models;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.RealTime;

/// <summary>
/// Unit tests for <see cref="SignalRNotificationAdapter"/>, covering constructor validation,
/// broadcast and targeted message sending, group management, and connection tracking.
/// </summary>
public class SignalRNotificationAdapterTests
{
    private readonly Mock<IHubContext<CognitiveMeshHub, ICognitiveMeshHubClient>> _hubContextMock;
    private readonly Mock<ILogger<SignalRNotificationAdapter>> _loggerMock;
    private readonly Mock<ICognitiveMeshHubClient> _allClientsMock;
    private readonly Mock<ICognitiveMeshHubClient> _userClientMock;
    private readonly Mock<ICognitiveMeshHubClient> _groupClientMock;
    private readonly Mock<IHubClients<ICognitiveMeshHubClient>> _clientsMock;
    private readonly Mock<IGroupManager> _groupManagerMock;

    public SignalRNotificationAdapterTests()
    {
        _hubContextMock = new Mock<IHubContext<CognitiveMeshHub, ICognitiveMeshHubClient>>();
        _loggerMock = new Mock<ILogger<SignalRNotificationAdapter>>();
        _allClientsMock = new Mock<ICognitiveMeshHubClient>();
        _userClientMock = new Mock<ICognitiveMeshHubClient>();
        _groupClientMock = new Mock<ICognitiveMeshHubClient>();
        _clientsMock = new Mock<IHubClients<ICognitiveMeshHubClient>>();
        _groupManagerMock = new Mock<IGroupManager>();

        _clientsMock.Setup(c => c.All).Returns(_allClientsMock.Object);
        _clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(_userClientMock.Object);
        _clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_groupClientMock.Object);

        _hubContextMock.Setup(h => h.Clients).Returns(_clientsMock.Object);
        _hubContextMock.Setup(h => h.Groups).Returns(_groupManagerMock.Object);
    }

    private SignalRNotificationAdapter CreateAdapter()
    {
        return new SignalRNotificationAdapter(_hubContextMock.Object, _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullHubContext_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new SignalRNotificationAdapter(null!, _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("hubContext");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new SignalRNotificationAdapter(_hubContextMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // BroadcastAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task BroadcastAsync_ValidPayload_SendsNotificationToAllClients()
    {
        // Arrange
        var adapter = CreateAdapter();
        var payload = new { Message = "Hello" };

        _allClientsMock
            .Setup(c => c.ReceiveNotification(It.IsAny<RealTimeEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.BroadcastAsync("TestEvent", payload, CancellationToken.None);

        // Assert
        _allClientsMock.Verify(
            c => c.ReceiveNotification(It.Is<RealTimeEvent>(e =>
                e.EventType == "TestEvent" &&
                e.Payload == payload)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // SendToUserAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SendToUserAsync_ValidUser_SendsNotificationToSpecificUser()
    {
        // Arrange
        var adapter = CreateAdapter();
        var payload = new { Data = 42 };

        _userClientMock
            .Setup(c => c.ReceiveNotification(It.IsAny<RealTimeEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.SendToUserAsync("user-1", "UserEvent", payload, CancellationToken.None);

        // Assert
        _clientsMock.Verify(c => c.User("user-1"), Times.Once);
        _userClientMock.Verify(
            c => c.ReceiveNotification(It.Is<RealTimeEvent>(e =>
                e.EventType == "UserEvent" &&
                e.Payload == payload)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // SendToGroupAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SendToGroupAsync_ValidGroup_SendsNotificationToGroup()
    {
        // Arrange
        var adapter = CreateAdapter();
        var payload = new { Status = "Updated" };

        _groupClientMock
            .Setup(c => c.ReceiveNotification(It.IsAny<RealTimeEvent>()))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.SendToGroupAsync("dashboard-1", "GroupEvent", payload, CancellationToken.None);

        // Assert
        _clientsMock.Verify(c => c.Group("dashboard-1"), Times.Once);
        _groupClientMock.Verify(
            c => c.ReceiveNotification(It.Is<RealTimeEvent>(e =>
                e.EventType == "GroupEvent" &&
                e.Payload == payload)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // AddToGroupAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddToGroupAsync_ValidParams_DelegatesToHubContextGroups()
    {
        // Arrange
        var adapter = CreateAdapter();
        var ct = new CancellationTokenSource().Token;

        _groupManagerMock
            .Setup(g => g.AddToGroupAsync("conn-1", "my-group", ct))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.AddToGroupAsync("conn-1", "my-group", ct);

        // Assert
        _groupManagerMock.Verify(
            g => g.AddToGroupAsync("conn-1", "my-group", ct),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // RemoveFromGroupAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RemoveFromGroupAsync_ValidParams_DelegatesToHubContextGroups()
    {
        // Arrange
        var adapter = CreateAdapter();
        var ct = new CancellationTokenSource().Token;

        _groupManagerMock
            .Setup(g => g.RemoveFromGroupAsync("conn-1", "my-group", ct))
            .Returns(Task.CompletedTask);

        // Act
        await adapter.RemoveFromGroupAsync("conn-1", "my-group", ct);

        // Assert
        _groupManagerMock.Verify(
            g => g.RemoveFromGroupAsync("conn-1", "my-group", ct),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // GetConnectedUsersAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetConnectedUsersAsync_NoTrackedUsers_ReturnsEmptyList()
    {
        // Arrange
        var adapter = CreateAdapter();

        // Act
        var result = await adapter.GetConnectedUsersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConnectedUsersAsync_WithTrackedUsers_ReturnsAllTrackedUsers()
    {
        // Arrange
        var adapter = CreateAdapter();
        var user1 = new ConnectedUser("user-1", "conn-1", DateTimeOffset.UtcNow, "tenant-1");
        var user2 = new ConnectedUser("user-2", "conn-2", DateTimeOffset.UtcNow, "tenant-1");

        adapter.TrackConnection(user1);
        adapter.TrackConnection(user2);

        // Act
        var result = await adapter.GetConnectedUsersAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(u => u.UserId == "user-1" && u.ConnectionId == "conn-1");
        result.Should().Contain(u => u.UserId == "user-2" && u.ConnectionId == "conn-2");
    }

    // -----------------------------------------------------------------------
    // TrackConnection / UntrackConnection tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task TrackConnection_ThenUntrackConnection_RemovesFromConnectedUsers()
    {
        // Arrange
        var adapter = CreateAdapter();
        var user = new ConnectedUser("user-1", "conn-1", DateTimeOffset.UtcNow);

        adapter.TrackConnection(user);

        // Act
        adapter.UntrackConnection("conn-1");
        var result = await adapter.GetConnectedUsersAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task TrackConnection_DuplicateConnectionId_DoesNotOverwrite()
    {
        // Arrange
        var adapter = CreateAdapter();
        var user1 = new ConnectedUser("user-1", "conn-1", DateTimeOffset.UtcNow);
        var user2 = new ConnectedUser("user-2", "conn-1", DateTimeOffset.UtcNow);

        // Act
        adapter.TrackConnection(user1);
        adapter.TrackConnection(user2); // same connectionId, should not overwrite

        var result = await adapter.GetConnectedUsersAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].UserId.Should().Be("user-1"); // first one wins with TryAdd
    }
}
