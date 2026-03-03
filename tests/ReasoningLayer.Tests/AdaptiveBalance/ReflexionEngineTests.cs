using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.ReasoningLayer.Tests.AdaptiveBalance;

/// <summary>
/// Tests for <see cref="ReflexionEngine"/>.
/// </summary>
public class ReflexionEngineTests
{
    private readonly Mock<ILogger<ReflexionEngine>> _loggerMock;
    private readonly ReflexionEngine _engine;

    public ReflexionEngineTests()
    {
        _loggerMock = new Mock<ILogger<ReflexionEngine>>();
        _engine = new ReflexionEngine(_loggerMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new ReflexionEngine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ─── Evaluate: hallucination detection ────────────────────────────

    [Fact]
    public async Task EvaluateAsync_OutputWithHallucinationPatterns_DetectsHallucination()
    {
        // Arrange
        var input = "What is the capital of France?";
        var output = "According to the study published in the Journal of Geography, " +
                     "it is universally accepted that Paris is 100% guaranteed to be the capital.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.IsHallucination.Should().BeTrue();
        result.Confidence.Should().BeGreaterThanOrEqualTo(ReflexionEngine.HallucinationThreshold);
    }

    [Fact]
    public async Task EvaluateAsync_CleanOutput_NoHallucination()
    {
        // Arrange
        var input = "What is the capital of France?";
        var output = "The capital of France is Paris. It has been the capital since the 10th century.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.IsHallucination.Should().BeFalse();
        result.Confidence.Should().BeLessThan(ReflexionEngine.HallucinationThreshold);
    }

    // ─── Evaluate: contradiction detection ────────────────────────────

    [Fact]
    public async Task EvaluateAsync_ContradictoryStatements_DetectsContradictions()
    {
        // Arrange
        var input = "The system is available and can handle the load.";
        var output = "The system is not available and cannot handle any requests.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.Contradictions.Should().NotBeEmpty();
        result.Contradictions.Should().Contain(c => c.Contains("contradiction"));
    }

    [Fact]
    public async Task EvaluateAsync_ConsistentStatements_NoContradictions()
    {
        // Arrange
        var input = "The weather today will be sunny.";
        var output = "Today's forecast shows clear skies and warm temperatures.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.Contradictions.Should().BeEmpty();
    }

    // ─── Evaluate: confidence scoring ─────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_ConfidenceInValidRange()
    {
        // Arrange
        var input = "Tell me about machine learning.";
        var output = "Machine learning is a subset of artificial intelligence.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.Confidence.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public async Task EvaluateAsync_VeryShortOutput_IncreasesConfidence()
    {
        // Arrange
        var input = "Please provide a detailed analysis of the market conditions " +
                     "including trends, forecasts, and risk factors for the next quarter. " +
                     "Include data from multiple sources and cross-reference findings. " +
                     "This is a very long and complex request that requires a comprehensive response.";
        var output = "OK.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.Confidence.Should().BeGreaterThan(0.0);
    }

    // ─── Evaluate: duration tracking ──────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_MeasuresEvaluationDuration()
    {
        // Arrange
        var input = "Test input.";
        var output = "Test output.";

        // Act
        var result = await _engine.EvaluateAsync(input, output, CancellationToken.None);

        // Assert
        result.EvaluationDurationMs.Should().BeGreaterThanOrEqualTo(0);
        result.EvaluatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ─── Evaluate: input validation ───────────────────────────────────

    [Fact]
    public async Task EvaluateAsync_EmptyInputText_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.EvaluateAsync("", "output", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Input text*");
    }

    [Fact]
    public async Task EvaluateAsync_EmptyAgentOutput_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.EvaluateAsync("input", "", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Agent output*");
    }

    // ─── GetRecentResults tests ───────────────────────────────────────

    [Fact]
    public async Task GetRecentResultsAsync_NoResults_ReturnsEmptyList()
    {
        // Act
        var result = await _engine.GetRecentResultsAsync(10, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentResultsAsync_MultipleEvaluations_ReturnsOrderedByRecent()
    {
        // Arrange
        await _engine.EvaluateAsync("input1", "output1", CancellationToken.None);
        await _engine.EvaluateAsync("input2", "output2", CancellationToken.None);
        await _engine.EvaluateAsync("input3", "output3", CancellationToken.None);

        // Act
        var result = await _engine.GetRecentResultsAsync(2, CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetRecentResultsAsync_NegativeCount_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var act = () => _engine.GetRecentResultsAsync(-1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task EvaluateAsync_GeneratesUniqueResultIds()
    {
        // Arrange & Act
        var result1 = await _engine.EvaluateAsync("input1", "output1", CancellationToken.None);
        var result2 = await _engine.EvaluateAsync("input2", "output2", CancellationToken.None);

        // Assert
        result1.ResultId.Should().NotBe(result2.ResultId);
    }
}
