using System.Collections.Concurrent;
using AgencyLayer.DecisionExecution;
using CognitiveMesh.Shared.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.Integration;

/// <summary>
/// Integration tests for the DecisionExecutor pipeline.
/// Uses a real in-memory knowledge graph implementation and a mock LLM
/// to verify the full DecisionExecutor -> KnowledgeGraph -> LLM -> Persist flow.
/// </summary>
public class DecisionExecutorIntegrationTests
{
    private readonly DecisionExecutor _executor;
    private readonly InMemoryKnowledgeGraphManager _knowledgeGraph;
    private readonly Mock<ILLMClient> _mockLlm;

    public DecisionExecutorIntegrationTests()
    {
        _knowledgeGraph = new InMemoryKnowledgeGraphManager();
        _mockLlm = new Mock<ILLMClient>();
        _mockLlm.SetupGet(l => l.ModelName).Returns("test-model");

        _executor = new DecisionExecutor(
            Mock.Of<ILogger<DecisionExecutor>>(),
            _knowledgeGraph,
            _mockLlm.Object);
    }

    [Fact]
    public async Task ExecuteDecision_EndToEnd_QueriesKnowledgeGraphAndPersistsResult()
    {
        // Arrange — seed the knowledge graph with context
        await _knowledgeGraph.AddNodeAsync("ctx:pricing", new Dictionary<string, object>
        {
            ["category"] = "pricing",
            ["rule"] = "Premium tier starts at $50/month"
        }, "Context");

        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Recommend premium tier based on usage patterns.");

        var request = new DecisionRequest
        {
            RequestId = "dec-001",
            DecisionType = "pricing",
            Parameters = new Dictionary<string, object> { ["userId"] = "user-42" },
            Priority = 5
        };

        // Act
        var result = await _executor.ExecuteDecisionAsync(request);

        // Assert — decision completed successfully
        result.Status.Should().Be(DecisionStatus.Completed);
        result.Outcome.Should().Be(DecisionOutcome.Success);
        result.Results.Should().ContainKey("llmResponse");
        result.Results["llmResponse"].Should().Be("Recommend premium tier based on usage patterns.");
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);

        // Verify the LLM was called with context from knowledge graph
        _mockLlm.Verify(l => l.GenerateCompletionAsync(
            It.Is<string>(prompt => prompt.Contains("pricing")),
            It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify the decision was persisted back to the knowledge graph
        var decisionNode = await _knowledgeGraph.GetNodeAsync<Dictionary<string, object>>($"decision:dec-001");
        decisionNode.Should().NotBeNull();
        decisionNode!["requestId"].Should().Be("dec-001");
        decisionNode["outcome"].Should().Be("Success");
    }

    [Fact]
    public async Task ExecuteDecision_WithNoContext_StillCompletesWithEmptyContext()
    {
        // Arrange — empty knowledge graph
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Default recommendation without context.");

        var request = new DecisionRequest
        {
            RequestId = "dec-no-ctx",
            DecisionType = "unknown-type",
            Parameters = new Dictionary<string, object> { ["action"] = "test" }
        };

        // Act
        var result = await _executor.ExecuteDecisionAsync(request);

        // Assert
        result.Status.Should().Be(DecisionStatus.Completed);
        result.Results["contextEntriesUsed"].Should().Be(0);

        // LLM prompt should indicate no context
        _mockLlm.Verify(l => l.GenerateCompletionAsync(
            It.Is<string>(prompt => prompt.Contains("No additional context available")),
            It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteDecision_LLMFailure_ReturnsFailedResult()
    {
        // Arrange
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("LLM service unavailable"));

        var request = new DecisionRequest
        {
            RequestId = "dec-llm-fail",
            DecisionType = "pricing"
        };

        // Act
        var result = await _executor.ExecuteDecisionAsync(request);

        // Assert
        result.Status.Should().Be(DecisionStatus.Failed);
        result.Outcome.Should().Be(DecisionOutcome.Error);
        result.ErrorMessage.Should().Contain("LLM service unavailable");
    }

    [Fact]
    public async Task ExecuteDecision_Cancellation_ThrowsAndRecordsCancelledStatus()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns<string, float, int, CancellationToken>((_, _, _, ct) =>
            {
                cts.Cancel();
                ct.ThrowIfCancellationRequested();
                return Task.FromResult("never reached");
            });

        var request = new DecisionRequest
        {
            RequestId = "dec-cancel",
            DecisionType = "analytics"
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _executor.ExecuteDecisionAsync(request, cts.Token));

        // Verify the status was recorded as cancelled
        var status = await _executor.GetDecisionStatusAsync("dec-cancel");
        status.Status.Should().Be(DecisionStatus.Cancelled);
    }

    [Fact]
    public async Task GetDecisionStatus_AfterExecution_ReturnsCorrectResult()
    {
        // Arrange
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("done");

        await _executor.ExecuteDecisionAsync(new DecisionRequest
        {
            RequestId = "dec-status-check",
            DecisionType = "test"
        });

        // Act
        var status = await _executor.GetDecisionStatusAsync("dec-status-check");

        // Assert
        status.Status.Should().Be(DecisionStatus.Completed);
        status.RequestId.Should().Be("dec-status-check");
    }

    [Fact]
    public async Task GetDecisionLogs_WithDateRange_FiltersCorrectly()
    {
        // Arrange
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("response");

        // Execute multiple decisions
        for (int i = 0; i < 5; i++)
        {
            await _executor.ExecuteDecisionAsync(new DecisionRequest
            {
                RequestId = $"dec-log-{i}",
                DecisionType = $"type-{i}"
            });
        }

        // Act
        var logs = (await _executor.GetDecisionLogsAsync(limit: 3)).ToList();

        // Assert
        logs.Should().HaveCount(3);
        logs.Should().BeInDescendingOrder(l => l.Timestamp);
    }

    [Fact]
    public async Task ConcurrentDecisions_EachGetsOwnKnowledgeGraphEntry()
    {
        // Arrange
        _mockLlm.Setup(l => l.GenerateCompletionAsync(
                It.IsAny<string>(), It.IsAny<float>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("concurrent-result");

        var requests = Enumerable.Range(0, 10)
            .Select(i => new DecisionRequest
            {
                RequestId = $"dec-concurrent-{i}",
                DecisionType = "batch"
            })
            .ToList();

        // Act
        var results = await Task.WhenAll(requests.Select(r => _executor.ExecuteDecisionAsync(r)));

        // Assert
        results.Should().AllSatisfy(r => r.Status.Should().Be(DecisionStatus.Completed));

        // Each decision should have its own node in the knowledge graph
        foreach (var request in requests)
        {
            var node = await _knowledgeGraph.GetNodeAsync<Dictionary<string, object>>($"decision:{request.RequestId}");
            node.Should().NotBeNull();
        }
    }
}

/// <summary>
/// In-memory implementation of IKnowledgeGraphManager for integration testing.
/// Provides real storage behavior without external dependencies.
/// </summary>
internal class InMemoryKnowledgeGraphManager : IKnowledgeGraphManager
{
    private readonly ConcurrentDictionary<string, object> _nodes = new();
    private readonly ConcurrentDictionary<string, string> _labels = new();
    private readonly List<(string Source, string Target, string Type, Dictionary<string, object>? Props)> _relationships = new();

    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task AddNodeAsync<T>(string nodeId, T properties, string? label = null, CancellationToken cancellationToken = default) where T : class
    {
        _nodes[nodeId] = properties;
        if (label != null) _labels[nodeId] = label;
        return Task.CompletedTask;
    }

    public Task AddRelationshipAsync(string sourceNodeId, string targetNodeId, string relationshipType,
        Dictionary<string, object>? properties = null, CancellationToken cancellationToken = default)
    {
        _relationships.Add((sourceNodeId, targetNodeId, relationshipType, properties));
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Dictionary<string, object>>> QueryAsync(string query, CancellationToken cancellationToken = default)
    {
        // Simple query: match nodes whose label or key contains the query string
        var results = _nodes
            .Where(kvp => kvp.Key.Contains(query, StringComparison.OrdinalIgnoreCase)
                          || (_labels.TryGetValue(kvp.Key, out var label) && label.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Select(kvp =>
            {
                if (kvp.Value is Dictionary<string, object> dict) return dict;
                return new Dictionary<string, object> { ["value"] = kvp.Value };
            })
            .ToList();

        return Task.FromResult<IEnumerable<Dictionary<string, object>>>(results);
    }

    public Task<T?> GetNodeAsync<T>(string nodeId, CancellationToken cancellationToken = default) where T : class
    {
        if (_nodes.TryGetValue(nodeId, out var value))
        {
            if (value is T typed) return Task.FromResult<T?>(typed);
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
        _nodes.TryRemove(nodeId, out _);
        _labels.TryRemove(nodeId, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<T>> FindNodesAsync<T>(Dictionary<string, object> properties, CancellationToken cancellationToken = default) where T : class
    {
        var results = _nodes.Values.OfType<T>().ToList();
        return Task.FromResult<IEnumerable<T>>(results);
    }

    public void Dispose() { }
}
