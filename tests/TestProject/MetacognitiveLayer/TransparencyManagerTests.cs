using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;
using CognitiveMesh.MetacognitiveLayer.ReasoningTransparency;
using CognitiveMesh.Shared.Interfaces;

namespace CognitiveMesh.Tests.MetacognitiveLayer
{
    public class TransparencyManagerTests
    {
        private readonly MockLogger<TransparencyManager> _logger;
        private readonly MockKnowledgeGraphManager _graphManager;
        private readonly TransparencyManager _manager;

        public TransparencyManagerTests()
        {
            _logger = new MockLogger<TransparencyManager>();
            _graphManager = new MockKnowledgeGraphManager();
            _manager = new TransparencyManager(_logger, _graphManager);
        }

        [Fact]
        public async Task LogReasoningStepAsync_ShouldCreateTraceAndStep()
        {
            // Arrange
            var step = new ReasoningStep
            {
                Id = "step-1",
                TraceId = "trace-1",
                Name = "Test Step",
                Description = "Testing",
                Timestamp = DateTime.UtcNow,
                Confidence = 0.9f
            };

            // Act
            await _manager.LogReasoningStepAsync(step);

            // Assert
            var trace = await _graphManager.GetNodeAsync<ReasoningTrace>("trace-1");
            Assert.NotNull(trace);
            Assert.Equal("trace-1", trace.Id);

            var storedStep = await _graphManager.GetNodeAsync<ReasoningStep>("step-1");
            Assert.NotNull(storedStep);
            Assert.Equal("step-1", storedStep.Id);

            // Check relationship
            Assert.Contains(_graphManager.Relationships, r =>
                r.SourceId == "trace-1" &&
                r.TargetId == "step-1" &&
                r.Type == "HAS_STEP");
        }

        [Fact]
        public async Task GenerateTransparencyReportAsync_Json_ShouldReturnValidReport()
        {
            // Arrange
            var traceId = "trace-report-json";
            var step1 = new ReasoningStep
            {
                Id = "s1",
                TraceId = traceId,
                Name = "Step 1",
                Timestamp = DateTime.UtcNow.AddMinutes(-10),
                Confidence = 0.8f,
                Metadata = new Dictionary<string, object> { ["model"] = "gpt-4" }
            };
            var step2 = new ReasoningStep
            {
                Id = "s2",
                TraceId = traceId,
                Name = "Step 2",
                Timestamp = DateTime.UtcNow,
                Confidence = 0.9f,
                Metadata = new Dictionary<string, object> { ["model"] = "gpt-3.5" }
            };

            await _manager.LogReasoningStepAsync(step1);
            await _manager.LogReasoningStepAsync(step2);

            // Act
            var reports = await _manager.GenerateTransparencyReportAsync(traceId, "json");
            var report = reports.Single();

            // Assert
            Assert.Equal(traceId, report.TraceId);
            Assert.Equal("json", report.Format);
            Assert.Contains("gpt-4", report.Content!);
            Assert.Contains("gpt-3.5", report.Content!);
            Assert.Contains("AverageConfidence", report.Content!);
            Assert.Contains("0.85", report.Content!); // Average of 0.8 and 0.9
        }

        [Fact]
        public async Task GenerateTransparencyReportAsync_Markdown_ShouldReturnValidReport()
        {
            // Arrange
            var traceId = "trace-report-md";
            var step1 = new ReasoningStep
            {
                Id = "s1",
                TraceId = traceId,
                Name = "Analysis",
                Timestamp = DateTime.UtcNow,
                Confidence = 0.95f
            };

            await _manager.LogReasoningStepAsync(step1);

            // Act
            var reports = await _manager.GenerateTransparencyReportAsync(traceId, "markdown");
            var report = reports.Single();

            // Assert
            Assert.Equal("markdown", report.Format);
            Assert.StartsWith("# Transparency Report", report.Content);
            Assert.Contains("**Trace ID:** trace-report-md", report.Content!);
            Assert.Contains("### Step: Analysis", report.Content!);
        }
    }

    // --- Mocks ---

    public class MockLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }

    public class MockKnowledgeGraphManager : IKnowledgeGraphManager
    {
        private readonly Dictionary<string, object> _nodes = new Dictionary<string, object>();
        public readonly List<(string SourceId, string TargetId, string Type)> Relationships = new List<(string, string, string)>();

        public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task AddNodeAsync<T>(string nodeId, T properties, string? label = null, CancellationToken cancellationToken = default) where T : class
        {
            _nodes[nodeId] = properties;
            return Task.CompletedTask;
        }

        public Task AddRelationshipAsync(string sourceNodeId, string targetNodeId, string relationshipType, Dictionary<string, object>? properties = null, CancellationToken cancellationToken = default)
        {
            Relationships.Add((sourceNodeId, targetNodeId, relationshipType));
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Dictionary<string, object>>> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Enumerable.Empty<Dictionary<string, object>>());
        }

        public Task<T?> GetNodeAsync<T>(string nodeId, CancellationToken cancellationToken = default) where T : class
        {
            if (_nodes.TryGetValue(nodeId, out var node) && node is T typedNode)
            {
                return Task.FromResult<T?>(typedNode);
            }
            return Task.FromResult<T?>(null);
        }

        public Task UpdateNodeAsync<T>(string nodeId, T properties, CancellationToken cancellationToken = default) where T : class
        {
            _nodes[nodeId] = properties;
            return Task.CompletedTask;
        }

        public Task DeleteNodeAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            _nodes.Remove(nodeId);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> FindNodesAsync<T>(Dictionary<string, object> properties, CancellationToken cancellationToken = default) where T : class
        {
            var result = new List<T>();
            foreach (var node in _nodes.Values)
            {
                if (node is T typedNode)
                {
                    bool match = true;
                    var type = typeof(T);
                    foreach (var kvp in properties)
                    {
                        var prop = type.GetProperty(kvp.Key);
                        if (prop == null)
                        {
                            match = false;
                            break;
                        }

                        var val = prop.GetValue(typedNode);
                        if (!object.Equals(val, kvp.Value))
                        {
                            match = false;
                            break;
                        }
                    }
                    if (match)
                    {
                        result.Add(typedNode);
                    }
                }
            }
            return Task.FromResult<IEnumerable<T>>(result);
        }

        public void Dispose() { }
    }
}
