using MetacognitiveLayer.PerformanceMonitoring;
using MetacognitiveLayer.PerformanceMonitoring.Adapters;
using MetacognitiveLayer.PerformanceMonitoring.Models;
using MetacognitiveLayer.PerformanceMonitoring.Ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.MetacognitiveLayer.PerformanceMonitoring;

/// <summary>
/// Unit tests for <see cref="PerformanceMonitoringAdapter"/>, covering null guard
/// validation, metric recording, threshold checking, dashboard generation with
/// health status transitions, metric history retrieval, and alert registration.
/// </summary>
public class PerformanceMonitoringAdapterTests : IDisposable
{
    private readonly InMemoryMetricsStoreAdapter _inMemoryStore;
    private readonly PerformanceMonitor _monitor;
    private readonly ILogger<PerformanceMonitoringAdapter> _adapterLogger;
    private readonly PerformanceMonitoringAdapter _sut;

    public PerformanceMonitoringAdapterTests()
    {
        var storeLogger = NullLogger<InMemoryMetricsStoreAdapter>.Instance;
        _inMemoryStore = new InMemoryMetricsStoreAdapter(storeLogger);
        _monitor = new PerformanceMonitor(_inMemoryStore);
        _adapterLogger = NullLogger<PerformanceMonitoringAdapter>.Instance;
        _sut = new PerformanceMonitoringAdapter(_monitor, _inMemoryStore, _adapterLogger);
    }

    public void Dispose()
    {
        _monitor.Dispose();
    }

    // -----------------------------------------------------------------------
    // Constructor null guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullMonitor_ThrowsArgumentNullException()
    {
        var act = () => new PerformanceMonitoringAdapter(null!, _inMemoryStore, _adapterLogger);

        act.Should().Throw<ArgumentNullException>().WithParameterName("monitor");
    }

    [Fact]
    public void Constructor_NullStore_ThrowsArgumentNullException()
    {
        var act = () => new PerformanceMonitoringAdapter(_monitor, null!, _adapterLogger);

        act.Should().Throw<ArgumentNullException>().WithParameterName("store");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new PerformanceMonitoringAdapter(_monitor, _inMemoryStore, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // RecordMetricAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RecordMetricAsync_ValidMetric_DelegatesToPerformanceMonitor()
    {
        // Arrange
        var tags = new Dictionary<string, string> { ["env"] = "test" };

        // Act
        await _sut.RecordMetricAsync("cpu.usage", 75.5, tags, CancellationToken.None);

        // Assert
        _inMemoryStore.GetMetricCount("cpu.usage").Should().Be(1);
    }

    [Fact]
    public async Task RecordMetricAsync_NullTags_RecordsSuccessfully()
    {
        // Act
        await _sut.RecordMetricAsync("cpu.usage", 50.0, null, CancellationToken.None);

        // Assert
        _inMemoryStore.GetMetricCount("cpu.usage").Should().Be(1);
    }

    [Fact]
    public async Task RecordMetricAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => _sut.RecordMetricAsync("cpu.usage", 50.0, null, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // GetAggregatedStatsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAggregatedStatsAsync_WithRecordedMetrics_ReturnsCorrectStats()
    {
        // Arrange
        await _sut.RecordMetricAsync("latency", 10.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("latency", 20.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("latency", 30.0, null, CancellationToken.None);

        // Act
        var stats = await _sut.GetAggregatedStatsAsync("latency", CancellationToken.None);

        // Assert
        stats.Should().NotBeNull();
        stats!.Count.Should().Be(3);
        stats.Min.Should().Be(10.0);
        stats.Max.Should().Be(30.0);
        stats.Average.Should().Be(20.0);
    }

    [Fact]
    public async Task GetAggregatedStatsAsync_NoData_ReturnsNull()
    {
        // Act
        var stats = await _sut.GetAggregatedStatsAsync("nonexistent", CancellationToken.None);

        // Assert
        stats.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // CheckThresholdsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckThresholdsAsync_NoThresholds_ReturnsEmptyList()
    {
        // Act
        var violations = await _sut.CheckThresholdsAsync(CancellationToken.None);

        // Assert
        violations.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckThresholdsAsync_ThresholdViolated_ReturnsViolation()
    {
        // Arrange
        await _sut.RegisterAlertAsync("cpu.usage", new MetricThreshold
        {
            Value = 80.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RecordMetricAsync("cpu.usage", 90.0, null, CancellationToken.None);

        // Act
        var violations = await _sut.CheckThresholdsAsync(CancellationToken.None);

        // Assert
        violations.Should().HaveCount(1);
        violations[0].MetricName.Should().Be("cpu.usage");
    }

    [Fact]
    public async Task CheckThresholdsAsync_ThresholdNotViolated_ReturnsEmptyList()
    {
        // Arrange
        await _sut.RegisterAlertAsync("cpu.usage", new MetricThreshold
        {
            Value = 80.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RecordMetricAsync("cpu.usage", 50.0, null, CancellationToken.None);

        // Act
        var violations = await _sut.CheckThresholdsAsync(CancellationToken.None);

        // Assert
        violations.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GetDashboardSummaryAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDashboardSummaryAsync_ReturnsCompleteSummary()
    {
        // Arrange
        await _sut.RecordMetricAsync("cpu.usage", 50.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("memory.used", 2048.0, null, CancellationToken.None);

        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        summary.Should().NotBeNull();
        summary.GeneratedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        summary.TotalMetricsRecorded.Should().Be(2);
        summary.TopMetrics.Should().HaveCount(2);
        summary.SystemHealth.Should().Be(SystemHealthStatus.Healthy);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_NoMetrics_ReturnsEmptySummary()
    {
        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        summary.Should().NotBeNull();
        summary.TotalMetricsRecorded.Should().Be(0);
        summary.TopMetrics.Should().BeEmpty();
        summary.CurrentViolations.Should().BeEmpty();
        summary.SystemHealth.Should().Be(SystemHealthStatus.Healthy);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_HealthyStatus_WhenNoViolations()
    {
        // Arrange
        await _sut.RecordMetricAsync("cpu", 50.0, null, CancellationToken.None);

        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        summary.SystemHealth.Should().Be(SystemHealthStatus.Healthy);
        summary.CurrentViolations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_DegradedStatus_WhenOneOrTwoViolations()
    {
        // Arrange — register 2 thresholds that will be violated
        await _sut.RegisterAlertAsync("metric.a", new MetricThreshold
        {
            Value = 10.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RegisterAlertAsync("metric.b", new MetricThreshold
        {
            Value = 10.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RecordMetricAsync("metric.a", 50.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("metric.b", 50.0, null, CancellationToken.None);

        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        summary.SystemHealth.Should().Be(SystemHealthStatus.Degraded);
        summary.CurrentViolations.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDashboardSummaryAsync_CriticalStatus_WhenThreeOrMoreViolations()
    {
        // Arrange — register 3 thresholds that will be violated
        await _sut.RegisterAlertAsync("metric.a", new MetricThreshold
        {
            Value = 10.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RegisterAlertAsync("metric.b", new MetricThreshold
        {
            Value = 10.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RegisterAlertAsync("metric.c", new MetricThreshold
        {
            Value = 10.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        }, CancellationToken.None);

        await _sut.RecordMetricAsync("metric.a", 50.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("metric.b", 50.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("metric.c", 50.0, null, CancellationToken.None);

        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        summary.SystemHealth.Should().Be(SystemHealthStatus.Critical);
        summary.CurrentViolations.Should().HaveCountGreaterThanOrEqualTo(3);
    }

    // -----------------------------------------------------------------------
    // DetermineSystemHealth static method tests
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(0, SystemHealthStatus.Healthy)]
    [InlineData(1, SystemHealthStatus.Degraded)]
    [InlineData(2, SystemHealthStatus.Degraded)]
    [InlineData(3, SystemHealthStatus.Critical)]
    [InlineData(10, SystemHealthStatus.Critical)]
    public void DetermineSystemHealth_ViolationCount_ReturnsCorrectStatus(
        int violationCount, SystemHealthStatus expectedHealth)
    {
        var result = PerformanceMonitoringAdapter.DetermineSystemHealth(violationCount);

        result.Should().Be(expectedHealth);
    }

    // -----------------------------------------------------------------------
    // GetMetricHistoryAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetMetricHistoryAsync_WithData_ReturnsFilteredResults()
    {
        // Arrange
        var baseTime = new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);
        await _sut.RecordMetricAsync("latency", 10.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("latency", 20.0, null, CancellationToken.None);

        // Act — use a wide time range to capture all metrics
        var results = await _sut.GetMetricHistoryAsync(
            "latency",
            DateTimeOffset.UtcNow.AddMinutes(-1),
            DateTimeOffset.UtcNow.AddMinutes(1),
            CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMetricHistoryAsync_NoData_ReturnsEmptyList()
    {
        // Act
        var results = await _sut.GetMetricHistoryAsync(
            "nonexistent",
            DateTimeOffset.UtcNow.AddHours(-1),
            DateTimeOffset.UtcNow,
            CancellationToken.None);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMetricHistoryAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = () => _sut.GetMetricHistoryAsync("cpu", DateTimeOffset.MinValue, DateTimeOffset.MaxValue, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // RegisterAlertAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RegisterAlertAsync_ValidThreshold_RegistersSuccessfully()
    {
        // Arrange
        var threshold = new MetricThreshold
        {
            Value = 90.0,
            Condition = ThresholdCondition.GreaterThan,
            AggregationMode = ThresholdAggregation.Average,
            EvaluationWindow = TimeSpan.FromMinutes(5)
        };

        // Act — register threshold and then record a metric that violates it
        await _sut.RegisterAlertAsync("cpu.usage", threshold, CancellationToken.None);
        await _sut.RecordMetricAsync("cpu.usage", 95.0, null, CancellationToken.None);

        // Assert — the threshold should be active and report a violation
        var violations = await _sut.CheckThresholdsAsync(CancellationToken.None);
        violations.Should().HaveCount(1);
        violations[0].MetricName.Should().Be("cpu.usage");
    }

    [Fact]
    public async Task RegisterAlertAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var threshold = new MetricThreshold { Value = 90.0 };

        // Act
        var act = () => _sut.RegisterAlertAsync("cpu.usage", threshold, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // GetDashboardSummaryAsync TopMetrics content tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDashboardSummaryAsync_TopMetrics_ContainCorrectValues()
    {
        // Arrange
        await _sut.RecordMetricAsync("latency", 100.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("latency", 200.0, null, CancellationToken.None);
        await _sut.RecordMetricAsync("latency", 300.0, null, CancellationToken.None);

        // Act
        var summary = await _sut.GetDashboardSummaryAsync(CancellationToken.None);

        // Assert
        var latencyEntry = summary.TopMetrics.Should().ContainSingle(m => m.MetricName == "latency").Subject;
        latencyEntry.SampleCount.Should().Be(3);
        latencyEntry.Min.Should().Be(100.0);
        latencyEntry.Max.Should().Be(300.0);
        latencyEntry.Average.Should().Be(200.0);
        latencyEntry.LastValue.Should().Be(300.0);
    }
}
