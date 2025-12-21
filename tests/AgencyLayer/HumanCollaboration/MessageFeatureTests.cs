using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.HumanCollaboration;
using CognitiveMesh.AgencyLayer.HumanCollaboration.Features.Messages;
using CognitiveMesh.Shared.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.HumanCollaboration
{
    /// <summary>
    /// Tests for the message handling features.
    /// </summary>
    public class MessageFeatureTests
    {
        private readonly Mock<IKnowledgeGraphManager> _mockKnowledgeGraph;
        private readonly Mock<ILogger<AddMessageCommandHandler>> _mockAddLogger;
        private readonly Mock<ILogger<GetSessionMessagesQueryHandler>> _mockGetLogger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageFeatureTests"/> class.
        /// </summary>
        public MessageFeatureTests()
        {
            _mockKnowledgeGraph = new Mock<IKnowledgeGraphManager>();
            _mockAddLogger = new Mock<ILogger<AddMessageCommandHandler>>();
            _mockGetLogger = new Mock<ILogger<GetSessionMessagesQueryHandler>>();
        }

        /// <summary>
        /// Verifies that adding a message persists it to the graph.
        /// </summary>
        [Fact]
        public async Task AddMessage_ShouldPersistToGraph()
        {
            // Arrange
            var handler = new AddMessageCommandHandler(_mockKnowledgeGraph.Object, _mockAddLogger.Object);
            var command = new AddMessageCommand
            {
                SessionId = "session-1",
                SenderId = "user-1",
                Content = "Hello World",
                MessageType = "text"
            };

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Id);
            Assert.Equal("session-1", result.SessionId);
            Assert.Equal("Hello World", result.Content);

            // Verify AddNodeAsync was called for the message
            _mockKnowledgeGraph.Verify(x => x.AddNodeAsync(
                It.Is<string>(id => id == result.Id),
                It.Is<CollaborationMessage>(m => m.Content == "Hello World"),
                It.Is<string>(l => l == CollaborationNodeLabels.CollaborationMessage),
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify Relationship was added
            _mockKnowledgeGraph.Verify(x => x.AddRelationshipAsync(
                "session-1",
                result.Id,
                "HAS_MESSAGE",
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that getting session messages retrieves them from the graph.
        /// </summary>
        [Fact]
        public async Task GetSessionMessages_ShouldRetrieveFromGraph()
        {
            // Arrange
            var handler = new GetSessionMessagesQueryHandler(_mockKnowledgeGraph.Object, _mockGetLogger.Object);
            var query = new GetSessionMessagesQuery
            {
                SessionId = "session-1",
                Limit = 10
            };

            var expectedMessages = new List<CollaborationMessage>
            {
                new CollaborationMessage {
                    Id = "1",
                    SessionId = "session-1",
                    Content = "M1",
                    SenderId = "user-1",
                    MessageType = "text",
                    Timestamp = DateTime.UtcNow.AddMinutes(-10)
                },
                new CollaborationMessage {
                    Id = "2",
                    SessionId = "session-1",
                    Content = "M2",
                    SenderId = "user-1",
                    MessageType = "text",
                    Timestamp = DateTime.UtcNow.AddMinutes(-5)
                }
            };

            // Mock FindNodesAsync to return the list
            _mockKnowledgeGraph.Setup(x => x.FindNodesAsync<CollaborationMessage>(
                It.Is<Dictionary<string, object>>(d => d["SessionId"].Equals("session-1")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedMessages);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Equal("M2", result.First().Content); // Should be ordered by descending timestamp
        }
    }
}
