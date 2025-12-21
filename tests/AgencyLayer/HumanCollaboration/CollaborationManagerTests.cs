using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.HumanCollaboration;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.HumanCollaboration
{
    public class CollaborationManagerTests
    {
        private readonly Mock<ILogger<CollaborationManager>> _loggerMock;
        private readonly Mock<IKnowledgeGraphManager> _knowledgeGraphManagerMock;
        private readonly Mock<ILLMClient> _llmClientMock;
        private readonly CollaborationManager _manager;

        public CollaborationManagerTests()
        {
            _loggerMock = new Mock<ILogger<CollaborationManager>>();
            _knowledgeGraphManagerMock = new Mock<IKnowledgeGraphManager>();
            _llmClientMock = new Mock<ILLMClient>();
            _manager = new CollaborationManager(_loggerMock.Object, _knowledgeGraphManagerMock.Object, _llmClientMock.Object);
        }

        [Fact]
        public async Task CreateSessionAsync_ShouldCreateSessionAndStoreInGraph()
        {
            // Arrange
            var sessionName = "Test Session";
            var description = "Test Description";
            var participantIds = new List<string> { "user-1", "agent-1" };

            // Act
            var session = await _manager.CreateSessionAsync(sessionName, description, participantIds);

            // Assert
            Assert.NotNull(session);
            Assert.Equal(sessionName, session.Name);
            Assert.Equal(description, session.Description);
            Assert.Equal(CollaborationStatus.Active, session.Status);

            // Verify session was stored in graph
            _knowledgeGraphManagerMock.Verify(kg => kg.AddNodeAsync(
                It.Is<string>(id => id == session.Id),
                It.Is<object>(obj => GetPropertyValue(obj, "Name") as string == sessionName),
                It.Is<string>(label => label == "CollaborationSession"),
                It.IsAny<CancellationToken>()), Times.Once);

            // Verify relationships were created for participants
            foreach (var participantId in participantIds)
            {
                _knowledgeGraphManagerMock.Verify(kg => kg.AddRelationshipAsync(
                    It.Is<string>(id => id == session.Id),
                    participantId,
                    "HAS_PARTICIPANT",
                    null,
                    It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        private object GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }
    }
}
