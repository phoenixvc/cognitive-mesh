using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency;
using CognitiveMesh.Shared.Interfaces;
using System.Text.Json;

namespace CognitiveMesh.Tests.MetacognitiveLayer
{
    public class TransparencyManagerTests
    {
        private readonly Mock<IKnowledgeGraphManager> _mockGraphManager;
        private readonly Mock<ILogger<TransparencyManager>> _mockLogger;
        private readonly TransparencyManager _manager;

        public TransparencyManagerTests()
        {
            _mockGraphManager = new Mock<IKnowledgeGraphManager>();
            _mockLogger = new Mock<ILogger<TransparencyManager>>();
            _manager = new TransparencyManager(_mockLogger.Object, _mockGraphManager.Object);
        }

        [Fact]
        public async Task LogReasoningStepAsync_ShouldCreateTraceAndStep_WhenTraceDoesNotExist()
        {
            // Arrange
            var step = new ReasoningStep
            {
                Id = "step-1",
                TraceId = "trace-1",
                Name = "Test Step",
                Description = "A test step",
                Timestamp = DateTime.UtcNow,
                Inputs = new Dictionary<string, object> { { "key", "value" } },
                Confidence = 0.9f
            };

            _mockGraphManager
                .Setup(m => m.GetNodeAsync<ReasoningTraceNode>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReasoningTraceNode)null); // Trace not found

            // Act
            await _manager.LogReasoningStepAsync(step);

            // Assert
            // 1. Verify Trace Creation
            _mockGraphManager.Verify(m => m.AddNodeAsync(
                "trace:trace-1",
                It.Is<ReasoningTraceNode>(n => n.Id == "trace-1"),
                "ReasoningTrace",
                It.IsAny<CancellationToken>()), Times.Once);

            // 2. Verify Step Creation
            _mockGraphManager.Verify(m => m.AddNodeAsync(
                "step:step-1",
                It.Is<ReasoningStepNode>(n => n.Id == "step-1" && n.TraceId == "trace-1"),
                "ReasoningStep",
                It.IsAny<CancellationToken>()), Times.Once);

            // 3. Verify Relationship
            _mockGraphManager.Verify(m => m.AddRelationshipAsync(
                "trace:trace-1",
                "step:step-1",
                "HAS_STEP",
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReasoningTraceAsync_ShouldReturnTraceWithSteps()
        {
            // Arrange
            string traceId = "trace-1";
            var traceNode = new ReasoningTraceNode
            {
                Id = traceId,
                Name = "Test Trace",
                Description = "Desc"
            };

            var stepNode = new ReasoningStepNode
            {
                Id = "step-1",
                TraceId = traceId,
                Name = "Step 1",
                InputsJson = JsonSerializer.Serialize(new Dictionary<string, object> { { "in", "val" } }),
                Timestamp = DateTime.UtcNow
            };

            _mockGraphManager
                .Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(traceNode);

            _mockGraphManager
                .Setup(m => m.FindNodesAsync<ReasoningStepNode>(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReasoningStepNode> { stepNode });

            // Act
            var result = await _manager.GetReasoningTraceAsync(traceId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(traceId, result.Id);
            Assert.Single(result.Steps);
            Assert.Equal("step-1", result.Steps[0].Id);
            // Verify JSON deserialization
            // Note: System.Text.Json deserializes dictionary values as JsonElement by default unless strictly typed,
            // but our map logic handles Dictionary<string, object>.
            // However, JsonElement comparison is tricky.
            // Let's just check the key exists.
            Assert.True(result.Steps[0].Inputs.ContainsKey("in"));
        }
    }
}
