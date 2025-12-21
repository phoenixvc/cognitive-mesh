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

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency.Tests
{
    /// <summary>
    /// Tests for the <see cref="TransparencyManager"/> class.
    /// </summary>
    public class TransparencyManagerTests
    {
        private readonly Mock<IKnowledgeGraphManager> _mockKgManager;
        private readonly Mock<ILogger<TransparencyManager>> _mockLogger;
        private readonly TransparencyManager _manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransparencyManagerTests"/> class.
        /// </summary>
        public TransparencyManagerTests()
        {
            _mockKgManager = new Mock<IKnowledgeGraphManager>();
            _mockLogger = new Mock<ILogger<TransparencyManager>>();
            _manager = new TransparencyManager(_mockLogger.Object, _mockKgManager.Object);
        }

        /// <summary>
        /// Verifies that LogReasoningStepAsync correctly persists the step and trace logic.
        /// </summary>
        [Fact]
        public async Task LogReasoningStepAsync_ShouldPersistStep_WhenCalled()
        {
            // Arrange
            var step = new ReasoningStep
            {
                Id = "step-123",
                TraceId = "trace-456",
                Name = "Test Step",
                Description = "A test reasoning step",
                Confidence = 0.95f,
                Inputs = new Dictionary<string, object> { { "key", "value" } },
                Outputs = new Dictionary<string, object>(),
                Metadata = new Dictionary<string, object>()
            };

            // Mock Trace Node Check (Return null to trigger creation)
            _mockKgManager
                .Setup(m => m.GetNodeAsync<ReasoningTraceNode>(It.Is<string>(id => id == "trace:trace-456"), It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReasoningTraceNode?)null);

            // Mock Trace Node Creation
            _mockKgManager
                .Setup(m => m.AddNodeAsync(It.IsAny<string>(), It.IsAny<ReasoningTraceNode>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Mock Step Node Creation
            _mockKgManager
                .Setup(m => m.AddNodeAsync(It.IsAny<string>(), It.IsAny<ReasoningStepNode>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

             // Mock Relationship Creation
            _mockKgManager
                .Setup(m => m.AddRelationshipAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>?>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _manager.LogReasoningStepAsync(step);

            // Assert

            // 1. Verify Trace Node Check
            _mockKgManager.Verify(m => m.GetNodeAsync<ReasoningTraceNode>("trace:trace-456", It.IsAny<CancellationToken>()), Times.Once);

            // 2. Verify Trace Node Creation
            _mockKgManager.Verify(m => m.AddNodeAsync(
                "trace:trace-456",
                It.Is<ReasoningTraceNode>(t => t.Id == "trace-456"),
                "ReasoningTrace",
                It.IsAny<CancellationToken>()), Times.Once);

            // 3. Verify Step Node Creation
            // Note: The implementation maps ReasoningStep (domain) to ReasoningStepNode (storage)
            _mockKgManager.Verify(m => m.AddNodeAsync(
                step.Id,
                It.Is<ReasoningStepNode>(s => s.Id == step.Id && s.TraceId == step.TraceId && s.Name == step.Name),
                "ReasoningStep",
                It.IsAny<CancellationToken>()), Times.Once);

            // 4. Verify Relationship Creation
            _mockKgManager.Verify(m => m.AddRelationshipAsync(
                step.Id,
                step.TraceId,
                "BELONGS_TO",
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Verifies that LogReasoningStepAsync throws ArgumentNullException when step is null.
        /// </summary>
        [Fact]
        public async Task LogReasoningStepAsync_ShouldThrow_WhenStepIsNull()
        {
            // Act & Assert
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.LogReasoningStepAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Verifies that GetDecisionRationalesAsync returns rationales.
        /// </summary>
        [Fact]
        public async Task GetDecisionRationalesAsync_ShouldReturnRationales_WhenFound()
        {
            // Arrange
            string decisionId = "decision-789";

            // Note: The current implementation of GetDecisionRationalesAsync in TransparencyManager
            // is a placeholder that returns hardcoded data and does NOT query the KG manager.
            // So we don't mock the KG manager calls for this specific test until the implementation is updated.

            // Act
            var rationales = await _manager.GetDecisionRationalesAsync(decisionId);

            // Assert
            Assert.NotNull(rationales);
            Assert.NotEmpty(rationales);
            var firstRationale = System.Linq.Enumerable.First(rationales);
            Assert.Equal(decisionId, firstRationale.DecisionId);
        }

        /// <summary>
        /// Verifies that GetReasoningTraceAsync returns a trace with its steps.
        /// </summary>
        [Fact]
        public async Task GetReasoningTraceAsync_ShouldReturnTraceWithSteps_WhenFound()
        {
            // Arrange
            string traceId = "trace-999";

            var traceNode = new ReasoningTraceNode
            {
                Id = traceId,
                Name = "Test Trace",
                Description = "Description",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var stepNode = new ReasoningStepNode
            {
                Id = "step-1",
                TraceId = traceId,
                Name = "Step 1",
                Description = "Desc 1",
                Confidence = 0.9f,
                Timestamp = DateTime.UtcNow,
                InputsJson = JsonSerializer.Serialize(new Dictionary<string, object> { { "key", "val" } }),
                OutputsJson = JsonSerializer.Serialize(new Dictionary<string, object>()),
                MetadataJson = JsonSerializer.Serialize(new Dictionary<string, object>())
            };

            // Mock Trace Retrieval
            _mockKgManager
                .Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(traceNode);

            // Mock Steps Retrieval
            _mockKgManager
                .Setup(m => m.FindNodesAsync<ReasoningStepNode>(It.IsAny<Dictionary<string, object>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ReasoningStepNode> { stepNode });

            // Act
            var trace = await _manager.GetReasoningTraceAsync(traceId);

            // Assert
            Assert.NotNull(trace);
            Assert.Equal(traceId, trace!.Id);
            Assert.Single(trace.Steps);
            Assert.Equal("step-1", trace.Steps[0].Id);
            Assert.True(trace.Steps[0].Inputs.ContainsKey("key"));

            // Verify calls
            _mockKgManager.Verify(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()), Times.Once);
            _mockKgManager.Verify(m => m.FindNodesAsync<ReasoningStepNode>(
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("TraceId") && d["TraceId"].ToString() == traceId),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
