using MetacognitiveLayer.PerformanceMonitoring;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.MetacognitiveLayer.PerformanceMonitoring;

/// <summary>
/// Unit tests for <see cref="PerformanceMonitor"/>, covering metric recording,
/// aggregated statistics, threshold checking, metric querying, and disposal.
/// </summary>
public class PerformanceMonitorTests : IDisposable
{
    private readonly Mock<IMetricsStore> _metricsStoreMock;
    private readonly PerformanceMonitor _sut;

    public PerformanceMonitorTests()
    {
        _metricsStoreMock = new Mock<IMetricsStore>();
        _sut = new PerformanceMonitor(_metricsStoreMock.Object);
    }

    public void Dispose()
    {
        _sut.Dispose();
    }

    // -----------------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullMetricsStore_ThrowsArgumentNullException()
    {
        var act = () => new PerformanceMonitor(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("metricsStore");
    }

    [Fact]
    public void Constructor_ValidMetricsStore_CreatesInstance()
    {
        using var monitor = new PerformanceMonitor(_metricsStoreMock.Object);

        monitor.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // RecordMetric tests
    // -----------------------------------------------------------------------

    [Fact]
    public void RecordMetric_ValidNameAndValue_StoresMetric()
    {
        _sut.RecordMetric("cpu.usage", 75.5);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m =>
                m.Name == "cpu.usage" &&
                m.Value == 75.5)),
            Times.Once);
    }

    [Fact]
    public void RecordMetric_WithTags_StoresMetricWithTags()
    {
        var tags = new Dictionary<string, string>
        {
            ["host"] = "server-01",
            ["region"] = "eu-west-1"
        };

        _sut.RecordMetric("memory.used", 2048.0, tags);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m =>
                m.Name == "memory.used" &&
                m.Value == 2048.0 &&
                m.Tags["host"] == "server-01" &&
                m.Tags["region"] == "eu-west-1")),
            Times.Once);
    }

    [Fact]
    public void RecordMetric_NullName_ThrowsArgumentException()
    {
        var act = () => _sut.RecordMetric(null!, 10.0);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void RecordMetric_EmptyName_ThrowsArgumentException()
    {
        var act = () => _sut.RecordMetric("", 10.0);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void RecordMetric_WhitespaceName_ThrowsArgumentException()
    {
        var act = () => _sut.RecordMetric("   ", 10.0);

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void RecordMetric_NullTags_StoresWithEmptyTags()
    {
        _sut.RecordMetric("disk.io", 500.0, null);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m =>
                m.Name == "disk.io" &&
                m.Tags != null &&
                m.Tags.Count == 0)),
            Times.Once);
    }

    [Fact]
    public void RecordMetric_NegativeValue_RecordsSuccessfully()
    {
        _sut.RecordMetric("temperature.delta", -15.3);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m => m.Value == -15.3)),
            Times.Once);
    }

    [Fact]
    public void RecordMetric_ZeroValue_RecordsSuccessfully()
    {
        _sut.RecordMetric("errors.count", 0.0);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m => m.Value == 0.0)),
            Times.Once);
    }

    [Fact]
    public void RecordMetric_MultipleMetrics_StoresAll()
    {
        _sut.RecordMetric("metric.a", 1.0);
        _sut.RecordMetric("metric.b", 2.0);
        _sut.RecordMetric("metric.c", 3.0);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.IsAny<Metric>()),
            Times.Exactly(3));
    }

    [Fact]
    public void RecordMetric_SameNameMultipleTimes_StoresEachOccurrence()
    {
        _sut.RecordMetric("cpu.usage", 50.0);
        _sut.RecordMetric("cpu.usage", 60.0);
        _sut.RecordMetric("cpu.usage", 70.0);

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m => m.Name == "cpu.usage")),
            Times.Exactly(3));
    }

    [Fact]
    public void RecordMetric_SetsTimestamp()
    {
        var before = DateTime.UtcNow;
        _sut.RecordMetric("timestamp.test", 1.0);
        var after = DateTime.UtcNow;

        _metricsStoreMock.Verify(
            x => x.StoreMetric(It.Is<Metric>(m =>
                m.Timestamp >= before && m.Timestamp <= after)),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // GetAggregatedStats tests
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAggregatedStats_NoDataRecorded_ReturnsNull()
    {
        var result = _sut.GetAggregatedStats("nonexistent.metric", TimeSpan.FromMinutes(5));

        result.Should().BeNull();
    }

    [Fact]
    public void GetAggregatedStats_NullName_ThrowsArgumentException()
    {
        var act = () => _sut.GetAggregatedStats(null!, TimeSpan.FromMinutes(5));

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void GetAggregatedStats_EmptyName_ThrowsArgumentException()
    {
        var act = () => _sut.GetAggregatedStats("", TimeSpan.FromMinutes(5));

        act.Should().Throw<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public void GetAggregatedStats_ZeroWindow_ThrowsArgumentException()
    {
        var act = () => _sut.GetAggregatedStats("cpu.usage", TimeSpan.Zero);

        act.Should().Throw<ArgumentException>().WithParameterName("window");
    }

    [Fact]
    public void GetAggregatedStats_NegativeWindow_ThrowsArgumentException()
    {
        var act = () => _sut.GetAggregatedStats("cpu.usage", TimeSpan.FromMinutes(-5));

        act.Should().Throw<ArgumentException>().WithParameterName("window");
    }

    [Fact]
    public void GetAggregatedStats_SingleValue_ReturnsCorrectStatistics()
    {
        _sut.RecordMetric("single.metric", 42.0);

        var stats = _sut.GetAggregatedStats("single.metric", TimeSpan.FromMinutes(5));

        stats.Should().NotBeNull();
        stats!.Name.Should().Be("single.metric");
        stats.Count.Should().Be(1);
        stats.Min.Should().Be(42.0);
        stats.Max.Should().Be(42.0);
        stats.Average.Should().Be(42.0);
        stats.Sum.Should().Be(42.0);
    }

    [Fact]
    public void GetAggregatedStats_MultipleValues_ReturnsCorrectAggregation()
    {
        _sut.RecordMetric("multi.metric", 10.0);
        _sut.RecordMetric("multi.metric", 20.0);
        _sut.RecordMetric("multi.metric", 30.0);

        var stats = _sut.GetAggregatedStats("multi.metric", TimeSpan.FromMinutes(5));

        stats.Should().NotBeNull();
        stats!.Count.Should().Be(3);
        stats.Min.Should().Be(10.0);
        stats.Max.Should().Be(30.0);
        stats.Average.Should().Be(20.0);
        stats.Sum.Should().Be(60.0);
    }

    [Fact]
    public void GetAggregatedStats_DifferentMetricNames_ReturnsSeparateStats()
    {
        _sut.RecordMetric("metric.a", 100.0);
        _sut.RecordMetric("metric.b", 200.0);

        var statsA = _sut.GetAggregatedStats("metric.a", TimeSpan.FromMinutes(5));
        var statsB = _sut.GetAggregatedStats("metric.b", TimeSpan.FromMinutes(5));

        statsA.Should().NotBeNull();
        statsB.Should().NotBeNull();
        statsA!.Sum.Should().Be(100.0);
        statsB!.Sum.Should().Be(200.0);
    }

    // -----------------------------------------------------------------------
    // QueryMetricsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task QueryMetricsAsync_ValidArgs_DelegatesToStore()
    {
        var since = DateTime.UtcNow.AddHours(-1);
        var expectedMetrics = new List<Metric>
        {
            new Metric { Name = "test.metric", Value = 42.0, Timestamp = DateTime.UtcNow }
        };

        _metricsStoreMock
            .Setup(x => x.QueryMetricsAsync("test.metric", since, null, null))
            .ReturnsAsync(expectedMetrics);

        var result = await _sut.QueryMetricsAsync("test.metric", since);

        result.Should().BeEquivalentTo(expectedMetrics);
        _metricsStoreMock.Verify(
            x => x.QueryMetricsAsync("test.metric", since, null, null),
            Times.Once);
    }

    [Fact]
    public async Task QueryMetricsAsync_NullName_ThrowsArgumentException()
    {
        var act = () => _sut.QueryMetricsAsync(null!, DateTime.UtcNow.AddHours(-1));

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public async Task QueryMetricsAsync_EmptyName_ThrowsArgumentException()
    {
        var act = () => _sut.QueryMetricsAsync("", DateTime.UtcNow.AddHours(-1));

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("name");
    }

    [Fact]
    public async Task QueryMetricsAsync_SinceAfterUntil_ThrowsArgumentException()
    {
        var since = DateTime.UtcNow;
        var until = DateTime.UtcNow.AddHours(-1);

        var act = () => _sut.QueryMetricsAsync("test.metric", since, until);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("since");
    }

    [Fact]
    public async Task QueryMetricsAsync_StoreThrows_WrapsInInvalidOperationException()
    {
        var since = DateTime.UtcNow.AddHours(-1);

        _metricsStoreMock
            .Setup(x => x.QueryMetricsAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime?>(), It.IsAny<IDictionary<string, string>?>()))
            .ThrowsAsync(new TimeoutException("Connection timed out"));

        var act = () => _sut.QueryMetricsAsync("test.metric", since);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*test.metric*");
    }

    [Fact]
    public async Task QueryMetricsAsync_WithTags_PassesTagsToStore()
    {
        var since = DateTime.UtcNow.AddHours(-1);
        var tags = new Dictionary<string, string> { ["host"] = "server-01" };

        _metricsStoreMock
            .Setup(x => x.QueryMetricsAsync("cpu.usage", since, null, tags))
            .ReturnsAsync(new List<Metric>());

        await _sut.QueryMetricsAsync("cpu.usage", since, tags: tags);

        _metricsStoreMock.Verify(
            x => x.QueryMetricsAsync("cpu.usage", since, null, tags),
            Times.Once);
    }

    // -----------------------------------------------------------------------
    // CheckThresholdsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CheckThresholdsAsync_DefaultImplementation_ReturnsEmptyCollection()
    {
        var violations = await _sut.CheckThresholdsAsync();

        violations.Should().NotBeNull();
        violations.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Dispose tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Dispose_DisposableMetricsStore_DisposesStore()
    {
        var disposableStoreMock = new Mock<IMetricsStore>();
        var disposable = disposableStoreMock.As<IDisposable>();

        var monitor = new PerformanceMonitor(disposableStoreMock.Object);
        monitor.Dispose();

        disposable.Verify(x => x.Dispose(), Times.Once);
    }

    [Fact]
    public void Dispose_NonDisposableMetricsStore_DoesNotThrow()
    {
        var monitor = new PerformanceMonitor(_metricsStoreMock.Object);

        var act = () => monitor.Dispose();

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        var monitor = new PerformanceMonitor(_metricsStoreMock.Object);

        var act = () =>
        {
            monitor.Dispose();
            monitor.Dispose();
        };

        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_DisposableStore_DisposedOnlyOnce()
    {
        var disposableStoreMock = new Mock<IMetricsStore>();
        var disposable = disposableStoreMock.As<IDisposable>();

        var monitor = new PerformanceMonitor(disposableStoreMock.Object);
        monitor.Dispose();
        monitor.Dispose();

        disposable.Verify(x => x.Dispose(), Times.Once);
    }

    // -----------------------------------------------------------------------
    // PerformanceMonitor implements IDisposable
    // -----------------------------------------------------------------------

    [Fact]
    public void PerformanceMonitor_ImplementsIDisposable()
    {
        _sut.Should().BeAssignableTo<IDisposable>();
    }
}
