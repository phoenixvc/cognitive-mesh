using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Engines;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Models;
using CognitiveMesh.ReasoningLayer.TemporalDecisionCore.Ports;

namespace CognitiveMesh.ReasoningLayer.Tests.TemporalDecisionCore;

public class TemporalDecisionCoreEngineTests
{
    private readonly Mock<ILogger<TemporalDecisionCoreEngine>> _loggerMock;
    private readonly Mock<ITemporalAuditPort> _auditPortMock;
    private readonly TemporalDecisionCoreEngine _engine;

    public TemporalDecisionCoreEngineTests()
    {
        _loggerMock = new Mock<ILogger<TemporalDecisionCoreEngine>>();
        _auditPortMock = new Mock<ITemporalAuditPort>();
        _engine = new TemporalDecisionCoreEngine(_loggerMock.Object, _auditPortMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TemporalDecisionCoreEngine(null!, _auditPortMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullAuditPort_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new TemporalDecisionCoreEngine(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("auditPort");
    }

    // ─── RecordEvent tests ────────────────────────────────────────────

    [Fact]
    public async Task RecordEventAsync_ValidEvent_ReturnsRecordedEvent()
    {
        // Arrange
        var temporalEvent = CreateEvent("evt-1", salience: 0.8);

        // Act
        var result = await _engine.RecordEventAsync(temporalEvent);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be("evt-1");
        result.Salience.Should().Be(0.8);
    }

    [Fact]
    public async Task RecordEventAsync_DuplicateEvent_ThrowsInvalidOperationException()
    {
        // Arrange
        var temporalEvent = CreateEvent("evt-dup", salience: 0.5);
        await _engine.RecordEventAsync(temporalEvent);

        // Act
        var act = () => _engine.RecordEventAsync(temporalEvent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*evt-dup*already exists*");
    }

    [Fact]
    public async Task RecordEventAsync_NullEvent_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.RecordEventAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── GetEvent tests ───────────────────────────────────────────────

    [Fact]
    public async Task GetEventAsync_ExistingEvent_ReturnsEvent()
    {
        // Arrange
        var temporalEvent = CreateEvent("evt-get-1", salience: 0.7);
        await _engine.RecordEventAsync(temporalEvent);

        // Act
        var result = await _engine.GetEventAsync("evt-get-1");

        // Assert
        result.Should().NotBeNull();
        result!.EventId.Should().Be("evt-get-1");
    }

    [Fact]
    public async Task GetEventAsync_NonExistentEvent_ReturnsNull()
    {
        // Act
        var result = await _engine.GetEventAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    // ─── GetEventsInRange tests ───────────────────────────────────────

    [Fact]
    public async Task GetEventsInRangeAsync_EventsInRange_ReturnsOrderedEvents()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var evt1 = CreateEvent("evt-range-1", salience: 0.5, timestamp: baseTime);
        var evt2 = CreateEvent("evt-range-2", salience: 0.6, timestamp: baseTime.AddSeconds(5));
        var evt3 = CreateEvent("evt-range-3", salience: 0.7, timestamp: baseTime.AddSeconds(30));

        await _engine.RecordEventAsync(evt1);
        await _engine.RecordEventAsync(evt2);
        await _engine.RecordEventAsync(evt3);

        // Act
        var result = await _engine.GetEventsInRangeAsync(baseTime, baseTime.AddSeconds(10));

        // Assert
        result.Should().HaveCount(2);
        result[0].EventId.Should().Be("evt-range-1");
        result[1].EventId.Should().Be("evt-range-2");
    }

    // ─── EvaluateEdge: high salience creates link ────────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_HighSalienceCloseEvents_CreatesLink()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var context = new Dictionary<string, string> { { "type", "skid" }, { "actor", "user-1" } };
        var source = CreateEvent("evt-src-1", salience: 0.9, timestamp: baseTime, context: context);
        var target = CreateEvent("evt-tgt-1", salience: 0.9, timestamp: baseTime.AddSeconds(2), context: context);
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.ShouldLink.Should().BeTrue();
        result.PromoterScore.Should().BeGreaterThanOrEqualTo(TemporalDecisionCoreEngine.PromoterThreshold);
        result.SuppressorScore.Should().BeLessThanOrEqualTo(TemporalDecisionCoreEngine.SuppressorMaxThreshold);
        result.Confidence.Should().BeGreaterThan(0);
        result.Rationale.Should().Contain("LINKED");
    }

    // ─── EvaluateEdge: low salience rejects ──────────────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_LowSalienceEvents_RejectsLink()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var contextA = new Dictionary<string, string> { { "type", "noise" } };
        var contextB = new Dictionary<string, string> { { "category", "unrelated" } };
        var source = CreateEvent("evt-low-1", salience: 0.05, timestamp: baseTime, context: contextA);
        var target = CreateEvent("evt-low-2", salience: 0.05, timestamp: baseTime.AddSeconds(8), context: contextB);
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.8 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.ShouldLink.Should().BeFalse();
        result.Rationale.Should().Contain("REJECTED");
    }

    // ─── EvaluateEdge: temporal gap too large rejects ────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_TemporalGapExceedsWindow_RejectsLink()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var source = CreateEvent("evt-gap-1", salience: 0.9, timestamp: baseTime);
        var target = CreateEvent("evt-gap-2", salience: 0.9, timestamp: baseTime.AddSeconds(25));
        var window = new TemporalWindow { CurrentMaxGapMs = 10000 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.ShouldLink.Should().BeFalse();
        result.Rationale.Should().Contain("exceeds window");
        result.Confidence.Should().Be(0.0);
    }

    // ─── Promoter/Suppressor scoring tests ───────────────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_HighContextSimilarity_PromotesLink()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var sharedContext = new Dictionary<string, string>
        {
            { "type", "incident" },
            { "severity", "high" },
            { "actor", "agent-42" }
        };
        var source = CreateEvent("evt-ctx-1", salience: 0.8, timestamp: baseTime, context: sharedContext);
        var target = CreateEvent("evt-ctx-2", salience: 0.8, timestamp: baseTime.AddSeconds(1), context: sharedContext);
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.2 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.ShouldLink.Should().BeTrue();
        result.PromoterScore.Should().BeGreaterThan(0.5);
    }

    [Fact]
    public async Task EvaluateEdgeAsync_RandomCoOccurrence_SuppressesLink()
    {
        // Arrange — low salience + different contexts + high load = high suppressor
        var baseTime = DateTimeOffset.UtcNow;
        var contextA = new Dictionary<string, string> { { "domain", "finance" } };
        var contextB = new Dictionary<string, string> { { "domain", "healthcare" } };
        var source = CreateEvent("evt-rand-1", salience: 0.1, timestamp: baseTime, context: contextA);
        var target = CreateEvent("evt-rand-2", salience: 0.15, timestamp: baseTime.AddSeconds(3), context: contextB);
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.9 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.ShouldLink.Should().BeFalse();
        result.SuppressorScore.Should().BeGreaterThan(result.PromoterScore);
    }

    // ─── AdjustWindow tests ──────────────────────────────────────────

    [Fact]
    public async Task AdjustWindowAsync_HighThreat_IncreasesGap()
    {
        // Arrange
        var window = new TemporalWindow { CurrentMaxGapMs = 10000 };

        // Act
        var result = await _engine.AdjustWindowAsync(window, threatLevel: 1.0, loadFactor: 0.0);

        // Assert
        result.CurrentMaxGapMs.Should().BeGreaterThan(10000);
        result.ThreatMultiplier.Should().BeGreaterThan(1.0);
    }

    [Fact]
    public async Task AdjustWindowAsync_HighLoad_DecreasesGap()
    {
        // Arrange
        var window = new TemporalWindow { CurrentMaxGapMs = 10000 };

        // Act
        var result = await _engine.AdjustWindowAsync(window, threatLevel: 0.0, loadFactor: 1.0);

        // Assert
        result.CurrentMaxGapMs.Should().BeLessThan(10000);
        result.LoadFactor.Should().Be(1.0);
    }

    [Fact]
    public async Task AdjustWindowAsync_BoundsCheck_ClampsToValidRange()
    {
        // Arrange
        var window = new TemporalWindow { CurrentMaxGapMs = 10000 };

        // Act — extreme threat should not exceed 20000ms
        var result = await _engine.AdjustWindowAsync(window, threatLevel: 10.0, loadFactor: 0.0);

        // Assert
        result.CurrentMaxGapMs.Should().BeLessThanOrEqualTo(TemporalDecisionCoreEngine.MaxWindowMs);
        result.CurrentMaxGapMs.Should().BeGreaterThanOrEqualTo(TemporalDecisionCoreEngine.MinWindowMs);
    }

    [Fact]
    public async Task AdjustWindowAsync_ZeroThreatZeroLoad_ResetsToBase()
    {
        // Arrange
        var window = new TemporalWindow { CurrentMaxGapMs = 5000 };

        // Act
        var result = await _engine.AdjustWindowAsync(window, threatLevel: 0.0, loadFactor: 0.0);

        // Assert
        result.CurrentMaxGapMs.Should().Be(10000); // base window
    }

    // ─── QueryTemporalGraph tests ────────────────────────────────────

    [Fact]
    public async Task QueryTemporalGraphAsync_ReturnsCorrectChain()
    {
        // Arrange — create a chain: e1 -> e2 -> e3
        var baseTime = DateTimeOffset.UtcNow;
        var context = new Dictionary<string, string> { { "type", "chain" }, { "actor", "a1" } };

        var e1 = CreateEvent("chain-1", salience: 0.9, timestamp: baseTime, context: context);
        var e2 = CreateEvent("chain-2", salience: 0.9, timestamp: baseTime.AddSeconds(1), context: context);
        var e3 = CreateEvent("chain-3", salience: 0.9, timestamp: baseTime.AddSeconds(2), context: context);

        await _engine.RecordEventAsync(e1);
        await _engine.RecordEventAsync(e2);
        await _engine.RecordEventAsync(e3);

        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };

        // Create edges
        await _engine.EvaluateEdgeAsync(e1, e2, window);
        await _engine.EvaluateEdgeAsync(e2, e3, window);

        // Act
        var query = new TemporalQuery { StartEventId = "chain-1", MaxDepth = 5 };
        var result = await _engine.QueryTemporalGraphAsync(query);

        // Assert
        result.Edges.Should().HaveCountGreaterThanOrEqualTo(1);
        result.NodeCount.Should().BeGreaterThanOrEqualTo(2);
        result.QueryDurationMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task QueryTemporalGraphAsync_FiltersByConfidence()
    {
        // Arrange — create edges with varying confidence
        var baseTime = DateTimeOffset.UtcNow;
        var highContext = new Dictionary<string, string> { { "type", "alert" }, { "severity", "critical" } };
        var lowContext = new Dictionary<string, string> { { "noise", "true" } };

        var e1 = CreateEvent("conf-1", salience: 0.95, timestamp: baseTime, context: highContext);
        var e2 = CreateEvent("conf-2", salience: 0.95, timestamp: baseTime.AddSeconds(1), context: highContext);
        var e3 = CreateEvent("conf-3", salience: 0.2, timestamp: baseTime.AddSeconds(2), context: lowContext);

        await _engine.RecordEventAsync(e1);
        await _engine.RecordEventAsync(e2);
        await _engine.RecordEventAsync(e3);

        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };
        await _engine.EvaluateEdgeAsync(e1, e2, window);
        await _engine.EvaluateEdgeAsync(e1, e3, window);

        // Act — query with high confidence filter
        var query = new TemporalQuery { StartEventId = "conf-1", MinConfidence = 0.5, MaxDepth = 3 };
        var result = await _engine.QueryTemporalGraphAsync(query);

        // Assert — only high-confidence edges should appear
        foreach (var edge in result.Edges)
        {
            edge.Confidence.Should().BeGreaterThanOrEqualTo(0.5);
        }
    }

    [Fact]
    public async Task QueryTemporalGraphAsync_RespectsDepthLimit()
    {
        // Arrange — create a long chain
        var baseTime = DateTimeOffset.UtcNow;
        var context = new Dictionary<string, string> { { "type", "deep" }, { "actor", "a1" } };
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };

        var events = new List<TemporalEvent>();
        for (int i = 0; i < 5; i++)
        {
            var evt = CreateEvent($"deep-{i}", salience: 0.9, timestamp: baseTime.AddSeconds(i), context: context);
            events.Add(evt);
            await _engine.RecordEventAsync(evt);
        }

        for (int i = 0; i < events.Count - 1; i++)
        {
            await _engine.EvaluateEdgeAsync(events[i], events[i + 1], window);
        }

        // Act — query with depth=1 (only one hop from start)
        var query = new TemporalQuery { StartEventId = "deep-0", MaxDepth = 1 };
        var result = await _engine.QueryTemporalGraphAsync(query);

        // Assert — should be limited
        result.NodeCount.Should().BeLessThanOrEqualTo(3);
    }

    // ─── Audit trail tests ───────────────────────────────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_EdgeCreated_AuditLoggedAsCreated()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var context = new Dictionary<string, string> { { "type", "audit-test" }, { "actor", "a1" } };
        var source = CreateEvent("audit-src", salience: 0.9, timestamp: baseTime, context: context);
        var target = CreateEvent("audit-tgt", salience: 0.9, timestamp: baseTime.AddSeconds(1), context: context);
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        if (result.ShouldLink)
        {
            _auditPortMock.Verify(
                a => a.LogEdgeActionAsync(
                    It.Is<TemporalEdgeLog>(log => log.Action == TemporalEdgeAction.Created),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task EvaluateEdgeAsync_EdgeRejected_AuditLoggedAsRejected()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var source = CreateEvent("rej-src", salience: 0.9, timestamp: baseTime);
        var target = CreateEvent("rej-tgt", salience: 0.9, timestamp: baseTime.AddSeconds(25));
        var window = new TemporalWindow { CurrentMaxGapMs = 10000 };

        // Act
        await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        _auditPortMock.Verify(
            a => a.LogEdgeActionAsync(
                It.Is<TemporalEdgeLog>(log => log.Action == TemporalEdgeAction.Rejected),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ─── GetEdge / GetEdgesForEvent tests ────────────────────────────

    [Fact]
    public async Task GetEdgesForEventAsync_EventWithEdges_ReturnsConnectedEdges()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var context = new Dictionary<string, string> { { "type", "connected" }, { "actor", "a1" } };
        var e1 = CreateEvent("connected-1", salience: 0.9, timestamp: baseTime, context: context);
        var e2 = CreateEvent("connected-2", salience: 0.9, timestamp: baseTime.AddSeconds(1), context: context);

        await _engine.RecordEventAsync(e1);
        await _engine.RecordEventAsync(e2);

        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.1 };
        var decision = await _engine.EvaluateEdgeAsync(e1, e2, window);

        // Act
        var edges = await _engine.GetEdgesForEventAsync("connected-1");

        // Assert
        if (decision.ShouldLink)
        {
            edges.Should().HaveCountGreaterThanOrEqualTo(1);
            edges.Should().Contain(e => e.SourceEventId == "connected-1" || e.TargetEventId == "connected-1");
        }
    }

    [Fact]
    public async Task GetEdgeAsync_NonExistentEdge_ReturnsNull()
    {
        // Act
        var result = await _engine.GetEdgeAsync("nonexistent-edge");

        // Assert
        result.Should().BeNull();
    }

    // ─── Edge evaluation with all rationale fields ───────────────────

    [Fact]
    public async Task EvaluateEdgeAsync_AlwaysIncludesRationaleAndConfidence()
    {
        // Arrange
        var baseTime = DateTimeOffset.UtcNow;
        var source = CreateEvent("rat-1", salience: 0.7, timestamp: baseTime);
        var target = CreateEvent("rat-2", salience: 0.6, timestamp: baseTime.AddSeconds(3));
        var window = new TemporalWindow { CurrentMaxGapMs = 10000, LoadFactor = 0.3 };

        // Act
        var result = await _engine.EvaluateEdgeAsync(source, target, window);

        // Assert
        result.Rationale.Should().NotBeNullOrWhiteSpace();
        result.Confidence.Should().BeGreaterThanOrEqualTo(0.0);
        result.Confidence.Should().BeLessThanOrEqualTo(1.0);
        result.EvaluationDurationMs.Should().BeGreaterThanOrEqualTo(0);
        result.PromoterScore.Should().BeInRange(0.0, 1.0);
        result.SuppressorScore.Should().BeInRange(0.0, 1.0);
    }

    [Fact]
    public async Task EvaluateEdgeAsync_NullSourceEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var target = CreateEvent("null-test-tgt", salience: 0.5);
        var window = new TemporalWindow();

        // Act
        var act = () => _engine.EvaluateEdgeAsync(null!, target, window);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateEdgeAsync_NullTargetEvent_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateEvent("null-test-src", salience: 0.5);
        var window = new TemporalWindow();

        // Act
        var act = () => _engine.EvaluateEdgeAsync(source, null!, window);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateEdgeAsync_NullWindow_ThrowsArgumentNullException()
    {
        // Arrange
        var source = CreateEvent("null-win-src", salience: 0.5);
        var target = CreateEvent("null-win-tgt", salience: 0.5);

        // Act
        var act = () => _engine.EvaluateEdgeAsync(source, target, null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static TemporalEvent CreateEvent(
        string eventId,
        double salience,
        DateTimeOffset? timestamp = null,
        Dictionary<string, string>? context = null)
    {
        return new TemporalEvent(
            EventId: eventId,
            Timestamp: timestamp ?? DateTimeOffset.UtcNow,
            Salience: salience,
            Context: context ?? new Dictionary<string, string>(),
            SourceAgentId: "test-agent",
            TenantId: "test-tenant");
    }
}
