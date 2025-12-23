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
                $"step:{step.Id}",
                It.Is<ReasoningStepNode>(s => s.Id == step.Id && s.TraceId == step.TraceId && s.Name == step.Name),
                "ReasoningStep",
                It.IsAny<CancellationToken>()), Times.Once);

            // 4. Verify Relationship Creation
            _mockKgManager.Verify(m => m.AddRelationshipAsync(
                $"step:{step.Id}",
                $"trace:{step.TraceId}",
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

        /// <summary>
        /// Verifies that GenerateTransparencyReportAsync generates a JSON report with correct content.
        /// </summary>
        [Fact]
        public async Task GenerateTransparencyReportAsync_Json_ShouldGenerateValidReport()
        {
            // Arrange
            string traceId = "trace-report-json";
            var traceNode = new ReasoningTraceNode
            {
                Id = traceId,
                Name = "Test Trace",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow
            };

            var stepNode = new ReasoningStepNode
            {
                Id = "step-1",
                TraceId = traceId,
                Name = "Test Step",
                Description = "Test Step Description",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Confidence = 0.85f,
                InputsJson = "{}",
                OutputsJson = "{}",
                MetadataJson = "{\"model\":\"gpt-4\"}"
            };

            _mockKgManager.Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(traceNode);
            _mockKgManager.Setup(m => m.FindNodesAsync<ReasoningStepNode>(
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("TraceId")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { stepNode });

            // Act
            var reports = await _manager.GenerateTransparencyReportAsync(traceId, "json");
            var report = System.Linq.Enumerable.Single(reports);

            // Assert
            Assert.NotNull(report);
            Assert.Equal(traceId, report.TraceId);
            Assert.Equal("json", report.Format);
            Assert.Contains("\"TraceId\"", report.Content);
            Assert.Contains(traceId, report.Content);
            Assert.Contains("\"Summary\"", report.Content);
            Assert.Contains("TotalSteps", report.Content);
        }

        /// <summary>
        /// Verifies that GenerateTransparencyReportAsync generates a Markdown report with correct content.
        /// </summary>
        [Fact]
        public async Task GenerateTransparencyReportAsync_Markdown_ShouldGenerateValidReport()
        {
            // Arrange
            string traceId = "trace-report-md";
            var traceNode = new ReasoningTraceNode
            {
                Id = traceId,
                Name = "Test Trace",
                Description = "Test Description",
                CreatedAt = DateTime.UtcNow.AddMinutes(-10),
                UpdatedAt = DateTime.UtcNow
            };

            var stepNode = new ReasoningStepNode
            {
                Id = "step-1",
                TraceId = traceId,
                Name = "Analysis Step",
                Description = "Analyzing data",
                Timestamp = DateTime.UtcNow.AddMinutes(-5),
                Confidence = 0.95f,
                InputsJson = "{}",
                OutputsJson = "{}",
                MetadataJson = "{}"
            };

            _mockKgManager.Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(traceNode);
            _mockKgManager.Setup(m => m.FindNodesAsync<ReasoningStepNode>(
                It.Is<Dictionary<string, object>>(d => d.ContainsKey("TraceId")),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { stepNode });

            // Act
            var reports = await _manager.GenerateTransparencyReportAsync(traceId, "markdown");
            var report = System.Linq.Enumerable.Single(reports);

            // Assert
            Assert.NotNull(report);
            Assert.Equal("markdown", report.Format);
            Assert.StartsWith("# Transparency Report", report.Content);
            Assert.Contains("**Trace ID:**", report.Content);
            Assert.Contains(traceId, report.Content);
            Assert.Contains("## Summary", report.Content);
            Assert.Contains("## Reasoning Steps", report.Content);
            Assert.Contains("### Step: Analysis Step", report.Content);
        }

        /// <summary>
        /// Verifies that GenerateTransparencyReportAsync throws when trace is not found.
        /// </summary>
        [Fact]
        public async Task GenerateTransparencyReportAsync_ShouldThrow_WhenTraceNotFound()
        {
            // Arrange
            string traceId = "nonexistent-trace";
            _mockKgManager.Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync((ReasoningTraceNode?)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _manager.GenerateTransparencyReportAsync(traceId, "json"));
        }

        /// <summary>
        /// Verifies that GenerateTransparencyReportAsync throws for unsupported format.
        /// </summary>
        [Fact]
        public async Task GenerateTransparencyReportAsync_ShouldThrow_ForUnsupportedFormat()
        {
            // Arrange
            string traceId = "trace-unsupported";
            var traceNode = new ReasoningTraceNode
            {
                Id = traceId,
                Name = "Test",
                Description = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _mockKgManager.Setup(m => m.GetNodeAsync<ReasoningTraceNode>($"trace:{traceId}", It.IsAny<CancellationToken>()))
                .ReturnsAsync(traceNode);
            _mockKgManager.Setup(m => m.FindNodesAsync<ReasoningStepNode>(
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ReasoningStepNode[0]);

            // Act & Assert
            await Assert.ThrowsAsync<NotSupportedException>(() =>
                _manager.GenerateTransparencyReportAsync(traceId, "xml"));
        }
    }
}
