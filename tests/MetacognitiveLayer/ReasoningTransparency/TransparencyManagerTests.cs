using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.MetacognitiveLayer.ReasoningTransparency.Tests
{
    public class TransparencyManagerTests
    {
        private readonly Mock<IKnowledgeGraphManager> _mockKgManager;
        private readonly Mock<ILogger<TransparencyManager>> _mockLogger;
        private readonly TransparencyManager _manager;

        public TransparencyManagerTests()
        {
            _mockKgManager = new Mock<IKnowledgeGraphManager>();
            _mockLogger = new Mock<ILogger<TransparencyManager>>();
            _manager = new TransparencyManager(_mockLogger.Object, _mockKgManager.Object);
        }

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
                Confidence = 0.95f
            };

            _mockKgManager
                .Setup(m => m.AddNodeAsync(It.IsAny<string>(), It.IsAny<ReasoningStep>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await _manager.LogReasoningStepAsync(step);

            // Assert
            _mockKgManager.Verify(m => m.AddNodeAsync(
                step.Id,
                step,
                "ReasoningStep",
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task LogReasoningStepAsync_ShouldThrow_WhenStepIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _manager.LogReasoningStepAsync(null));
        }

        [Fact]
        public async Task GetDecisionRationalesAsync_ShouldReturnRationales_WhenFound()
        {
            // Arrange
            string decisionId = "decision-789";
            var mockResults = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "id", "rationale-1" },
                    { "decisionId", decisionId },
                    { "description", "Because A and B" },
                    { "confidence", "0.85" },
                    { "createdAt", DateTime.UtcNow.ToString() }
                },
                 new Dictionary<string, object>
                {
                    { "id", "rationale-2" },
                    { "decisionId", decisionId },
                    { "description", "Also C" },
                    { "confidence", "0.75" },
                    { "createdAt", DateTime.UtcNow.ToString() }
                }
            };

            _mockKgManager
                .Setup(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResults);

            // Act
            var rationales = await _manager.GetDecisionRationalesAsync(decisionId);

            // Assert
            Assert.NotNull(rationales);
            Assert.Collection(rationales,
                r1 => Assert.Equal("rationale-1", r1.Id),
                r2 => Assert.Equal("rationale-2", r2.Id)
            );

            _mockKgManager.Verify(m => m.QueryAsync(
                It.Is<string>(q => q.Contains(decisionId) && q.Contains("DecisionRationale")),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReasoningTraceAsync_ShouldReturnTraceWithSteps_WhenFound()
        {
            // Arrange
            string traceId = "trace-999";
             var mockResults = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    { "id", "step-1" },
                    { "traceId", traceId },
                    { "name", "Step 1" },
                    { "description", "Desc 1" },
                    { "confidence", "0.9" },
                    { "timestamp", DateTime.UtcNow.ToString() }
                }
            };

            _mockKgManager
                .Setup(m => m.QueryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResults);

            // Act
            var trace = await _manager.GetReasoningTraceAsync(traceId);

            // Assert
            Assert.NotNull(trace);
            Assert.Equal(traceId, trace.Id);
            Assert.Single(trace.Steps);
            Assert.Equal("step-1", trace.Steps[0].Id);
        }
    }
}
