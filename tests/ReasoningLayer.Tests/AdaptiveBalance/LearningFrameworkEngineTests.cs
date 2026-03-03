using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.ReasoningLayer.Tests.AdaptiveBalance;

/// <summary>
/// Tests for <see cref="LearningFrameworkEngine"/>.
/// </summary>
public class LearningFrameworkEngineTests
{
    private readonly Mock<ILogger<LearningFrameworkEngine>> _loggerMock;
    private readonly LearningFrameworkEngine _engine;

    public LearningFrameworkEngineTests()
    {
        _loggerMock = new Mock<ILogger<LearningFrameworkEngine>>();
        _engine = new LearningFrameworkEngine(_loggerMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LearningFrameworkEngine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ─── RecordEvent tests ────────────────────────────────────────────

    [Fact]
    public async Task RecordEventAsync_ValidEvent_StoresSuccessfully()
    {
        // Arrange
        var learningEvent = CreateEvent("reasoning_error", LearningOutcome.Failure);

        // Act
        await _engine.RecordEventAsync(learningEvent, CancellationToken.None);

        // Assert
        var patterns = await _engine.GetPatternsAsync("reasoning_error", CancellationToken.None);
        patterns.Should().HaveCount(1);
        patterns[0].EventId.Should().Be(learningEvent.EventId);
    }

    [Fact]
    public async Task RecordEventAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.RecordEventAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RecordEventAsync_EmptyPatternType_ThrowsArgumentException()
    {
        // Arrange
        var learningEvent = new LearningEvent(
            EventId: Guid.NewGuid(),
            PatternType: "",
            Description: "Some description",
            Evidence: "Some evidence",
            Outcome: LearningOutcome.Success,
            RecordedAt: DateTimeOffset.UtcNow,
            SourceAgentId: "agent-1");

        // Act
        var act = () => _engine.RecordEventAsync(learningEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*PatternType*");
    }

    [Fact]
    public async Task RecordEventAsync_EmptyDescription_ThrowsArgumentException()
    {
        // Arrange
        var learningEvent = new LearningEvent(
            EventId: Guid.NewGuid(),
            PatternType: "timeout",
            Description: "",
            Evidence: "Some evidence",
            Outcome: LearningOutcome.Success,
            RecordedAt: DateTimeOffset.UtcNow,
            SourceAgentId: "agent-1");

        // Act
        var act = () => _engine.RecordEventAsync(learningEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Description*");
    }

    [Fact]
    public async Task RecordEventAsync_EmptySourceAgentId_ThrowsArgumentException()
    {
        // Arrange
        var learningEvent = new LearningEvent(
            EventId: Guid.NewGuid(),
            PatternType: "timeout",
            Description: "Some description",
            Evidence: "Some evidence",
            Outcome: LearningOutcome.Success,
            RecordedAt: DateTimeOffset.UtcNow,
            SourceAgentId: "");

        // Act
        var act = () => _engine.RecordEventAsync(learningEvent, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SourceAgentId*");
    }

    [Fact]
    public async Task RecordEventAsync_MultipleEventsOfSameType_GroupedCorrectly()
    {
        // Arrange
        var event1 = CreateEvent("timeout", LearningOutcome.Failure);
        var event2 = CreateEvent("timeout", LearningOutcome.Success);
        var event3 = CreateEvent("other_type", LearningOutcome.Failure);

        // Act
        await _engine.RecordEventAsync(event1, CancellationToken.None);
        await _engine.RecordEventAsync(event2, CancellationToken.None);
        await _engine.RecordEventAsync(event3, CancellationToken.None);

        // Assert
        var timeoutPatterns = await _engine.GetPatternsAsync("timeout", CancellationToken.None);
        timeoutPatterns.Should().HaveCount(2);

        var otherPatterns = await _engine.GetPatternsAsync("other_type", CancellationToken.None);
        otherPatterns.Should().HaveCount(1);
    }

    // ─── GetPatterns tests ────────────────────────────────────────────

    [Fact]
    public async Task GetPatternsAsync_NonExistentType_ReturnsEmptyList()
    {
        // Act
        var result = await _engine.GetPatternsAsync("nonexistent", CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPatternsAsync_EmptyPatternType_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.GetPatternsAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*PatternType*");
    }

    // ─── GetMistakePreventionInsights tests ───────────────────────────

    [Fact]
    public async Task GetMistakePreventionInsightsAsync_OnlyFailureEvents_ReturnsAll()
    {
        // Arrange
        var failureEvent1 = CreateEvent("reasoning_error", LearningOutcome.Failure,
            recordedAt: DateTimeOffset.UtcNow.AddMinutes(-10));
        var failureEvent2 = CreateEvent("timeout", LearningOutcome.Failure,
            recordedAt: DateTimeOffset.UtcNow.AddMinutes(-5));
        var successEvent = CreateEvent("reasoning_error", LearningOutcome.Success);

        await _engine.RecordEventAsync(failureEvent1, CancellationToken.None);
        await _engine.RecordEventAsync(failureEvent2, CancellationToken.None);
        await _engine.RecordEventAsync(successEvent, CancellationToken.None);

        // Act
        var result = await _engine.GetMistakePreventionInsightsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(e => e.Outcome == LearningOutcome.Failure);
    }

    [Fact]
    public async Task GetMistakePreventionInsightsAsync_SortedByRecency()
    {
        // Arrange
        var olderEvent = CreateEvent("error", LearningOutcome.Failure,
            recordedAt: DateTimeOffset.UtcNow.AddHours(-2));
        var newerEvent = CreateEvent("error", LearningOutcome.Failure,
            recordedAt: DateTimeOffset.UtcNow.AddMinutes(-5));

        await _engine.RecordEventAsync(olderEvent, CancellationToken.None);
        await _engine.RecordEventAsync(newerEvent, CancellationToken.None);

        // Act
        var result = await _engine.GetMistakePreventionInsightsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result[0].RecordedAt.Should().BeAfter(result[1].RecordedAt);
    }

    [Fact]
    public async Task GetMistakePreventionInsightsAsync_NoFailures_ReturnsEmptyList()
    {
        // Arrange
        var successEvent = CreateEvent("pattern", LearningOutcome.Success);
        await _engine.RecordEventAsync(successEvent, CancellationToken.None);

        // Act
        var result = await _engine.GetMistakePreventionInsightsAsync(CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static LearningEvent CreateEvent(
        string patternType,
        LearningOutcome outcome,
        DateTimeOffset? recordedAt = null)
    {
        return new LearningEvent(
            EventId: Guid.NewGuid(),
            PatternType: patternType,
            Description: $"Test event for pattern '{patternType}'",
            Evidence: "Test evidence data",
            Outcome: outcome,
            RecordedAt: recordedAt ?? DateTimeOffset.UtcNow,
            SourceAgentId: "test-agent");
    }
}
