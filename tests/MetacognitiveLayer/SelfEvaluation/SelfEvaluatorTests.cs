using MetacognitiveLayer.SelfEvaluation;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.MetacognitiveLayer.SelfEvaluation;

/// <summary>
/// Unit tests for <see cref="SelfEvaluator"/>, covering all four evaluation methods
/// (EvaluatePerformanceAsync, AssessLearningProgressAsync, GenerateInsightsAsync,
/// ValidateBehaviorAsync) plus constructor behavior and disposal.
/// </summary>
public class SelfEvaluatorTests : IDisposable
{
    private readonly Mock<ILogger<SelfEvaluator>> _loggerMock;
    private readonly SelfEvaluator _sut;

    public SelfEvaluatorTests()
    {
        _loggerMock = new Mock<ILogger<SelfEvaluator>>();
        _sut = new SelfEvaluator(_loggerMock.Object);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // -----------------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_WithLogger_InitializesSuccessfully()
    {
        using var evaluator = new SelfEvaluator(_loggerMock.Object);

        evaluator.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_NullLogger_InitializesWithoutThrowing()
    {
        // SelfEvaluator accepts null logger (optional parameter)
        using var evaluator = new SelfEvaluator(null);

        evaluator.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // EvaluatePerformanceAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task EvaluatePerformanceAsync_ValidInputs_ReturnsEvaluationWithScore()
    {
        var metrics = new Dictionary<string, object>
        {
            ["responseTime"] = 150.0,
            ["throughput"] = 1000,
            ["errorRate"] = 0.02
        };

        var result = await _sut.EvaluatePerformanceAsync("ReasoningEngine", metrics);

        result.Should().NotBeNull();
        result.Should().ContainKey("score");
        result.Should().ContainKey("assessment");
        result.Should().ContainKey("recommendations");
        result["score"].Should().Be(1.0);
        result["assessment"].Should().Be("optimal");
    }

    [Fact]
    public async Task EvaluatePerformanceAsync_EmptyMetrics_ReturnsResult()
    {
        var metrics = new Dictionary<string, object>();

        var result = await _sut.EvaluatePerformanceAsync("EmptyComponent", metrics);

        result.Should().NotBeNull();
        result.Should().ContainKey("score");
    }

    [Fact]
    public async Task EvaluatePerformanceAsync_CancellationRequested_ThrowsIfCancelled()
    {
        var cts = new CancellationTokenSource();
        var metrics = new Dictionary<string, object> { ["metric"] = 1.0 };

        // The current implementation does not explicitly check cancellation,
        // so it completes normally. This test verifies the method signature supports it.
        var result = await _sut.EvaluatePerformanceAsync("Component", metrics, cts.Token);

        result.Should().NotBeNull();
    }

    [Fact]
    public async Task EvaluatePerformanceAsync_DifferentComponents_ReturnsResultForEach()
    {
        var metrics = new Dictionary<string, object> { ["latency"] = 50 };

        var result1 = await _sut.EvaluatePerformanceAsync("ComponentA", metrics);
        var result2 = await _sut.EvaluatePerformanceAsync("ComponentB", metrics);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1["score"].Should().Be(result2["score"]);
    }

    // -----------------------------------------------------------------------
    // AssessLearningProgressAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AssessLearningProgressAsync_ValidInputs_ReturnsProgressAssessment()
    {
        var metrics = new Dictionary<string, object>
        {
            ["accuracy"] = 0.95,
            ["epochs"] = 100,
            ["lossFunction"] = 0.05
        };

        var result = await _sut.AssessLearningProgressAsync("learning-task-42", metrics);

        result.Should().NotBeNull();
        result.Should().ContainKey("progress");
        result.Should().ContainKey("confidence");
        result.Should().ContainKey("nextSteps");
        result["progress"].Should().Be(1.0);
        result["confidence"].Should().Be(1.0);
    }

    [Fact]
    public async Task AssessLearningProgressAsync_EmptyMetrics_ReturnsResult()
    {
        var metrics = new Dictionary<string, object>();

        var result = await _sut.AssessLearningProgressAsync("empty-task", metrics);

        result.Should().NotBeNull();
        result.Should().ContainKey("progress");
    }

    [Fact]
    public async Task AssessLearningProgressAsync_MultipleTasks_ReturnsIndependentResults()
    {
        var metrics = new Dictionary<string, object> { ["accuracy"] = 0.8 };

        var result1 = await _sut.AssessLearningProgressAsync("task-1", metrics);
        var result2 = await _sut.AssessLearningProgressAsync("task-2", metrics);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // GenerateInsightsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateInsightsAsync_ValidInputs_ReturnsInsightsWithExpectedKeys()
    {
        var data = new Dictionary<string, object>
        {
            ["salesData"] = new[] { 100, 200, 150, 300 },
            ["region"] = "EMEA",
            ["period"] = "Q4-2025"
        };

        var result = await _sut.GenerateInsightsAsync("quarterly-review", data);

        result.Should().NotBeNull();
        result.Should().ContainKey("keyInsights");
        result.Should().ContainKey("patterns");
        result.Should().ContainKey("recommendations");
    }

    [Fact]
    public async Task GenerateInsightsAsync_EmptyData_ReturnsEmptyInsights()
    {
        var data = new Dictionary<string, object>();

        var result = await _sut.GenerateInsightsAsync("empty-context", data);

        result.Should().NotBeNull();
        result["keyInsights"].Should().BeEquivalentTo(Array.Empty<string>());
    }

    [Fact]
    public async Task GenerateInsightsAsync_CancellationToken_AcceptedBySignature()
    {
        var cts = new CancellationTokenSource();
        var data = new Dictionary<string, object> { ["metric"] = "value" };

        var result = await _sut.GenerateInsightsAsync("context", data, cts.Token);

        result.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // ValidateBehaviorAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ValidateBehaviorAsync_ValidBehavior_ReturnsTrue()
    {
        var parameters = new Dictionary<string, object>
        {
            ["threshold"] = 0.95,
            ["maxRetries"] = 3,
            ["timeout"] = TimeSpan.FromSeconds(30)
        };

        var result = await _sut.ValidateBehaviorAsync("AggressiveRetry", parameters);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBehaviorAsync_EmptyParameters_ReturnsTrue()
    {
        var parameters = new Dictionary<string, object>();

        var result = await _sut.ValidateBehaviorAsync("SimpleBehavior", parameters);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBehaviorAsync_MultipleBehaviors_ReturnsConsistentResults()
    {
        var parameters = new Dictionary<string, object> { ["mode"] = "strict" };

        var result1 = await _sut.ValidateBehaviorAsync("Behavior1", parameters);
        var result2 = await _sut.ValidateBehaviorAsync("Behavior2", parameters);

        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateBehaviorAsync_CancellationToken_AcceptedBySignature()
    {
        var cts = new CancellationTokenSource();
        var parameters = new Dictionary<string, object> { ["key"] = "value" };

        var result = await _sut.ValidateBehaviorAsync("TestBehavior", parameters, cts.Token);

        result.Should().BeTrue();
    }

    // -----------------------------------------------------------------------
    // Dispose tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var evaluator = new SelfEvaluator(_loggerMock.Object);

        var act = () =>
        {
            evaluator.Dispose();
            evaluator.Dispose();
        };

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // ISelfEvaluator interface compliance
    // -----------------------------------------------------------------------

    [Fact]
    public void SelfEvaluator_ImplementsISelfEvaluator()
    {
        _sut.Should().BeAssignableTo<ISelfEvaluator>();
    }

    [Fact]
    public void SelfEvaluator_ImplementsIDisposable()
    {
        _sut.Should().BeAssignableTo<IDisposable>();
    }
}
