using MetacognitiveLayer.PerformanceMonitoring;
using MetacognitiveLayer.PerformanceMonitoring.Adapters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.MetacognitiveLayer.PerformanceMonitoring;

/// <summary>
/// Unit tests for <see cref="InMemoryMetricsStoreAdapter"/>, covering metric storage,
/// date range filtering, metric name enumeration, thread safety, and entry trimming.
/// </summary>
public class InMemoryMetricsStoreAdapterTests
{
    private readonly InMemoryMetricsStoreAdapter _sut;

    public InMemoryMetricsStoreAdapterTests()
    {
        var logger = NullLogger<InMemoryMetricsStoreAdapter>.Instance;
        _sut = new InMemoryMetricsStoreAdapter(logger);
    }

    // -----------------------------------------------------------------------
    // Constructor tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new InMemoryMetricsStoreAdapter(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // StoreMetric tests
    // -----------------------------------------------------------------------

    [Fact]
    public void StoreMetric_SingleMetric_AddsMetricToStore()
    {
        // Arrange
        var metric = CreateMetric("cpu.usage", 75.5);

        // Act
        _sut.StoreMetric(metric);

        // Assert
        _sut.GetMetricCount("cpu.usage").Should().Be(1);
        _sut.GetAllMetricNames().Should().Contain("cpu.usage");
    }

    [Fact]
    public void StoreMetric_MultipleMetricsForSameName_StoresAll()
    {
        // Arrange & Act
        _sut.StoreMetric(CreateMetric("cpu.usage", 50.0));
        _sut.StoreMetric(CreateMetric("cpu.usage", 60.0));
        _sut.StoreMetric(CreateMetric("cpu.usage", 70.0));

        // Assert
        _sut.GetMetricCount("cpu.usage").Should().Be(3);
    }

    [Fact]
    public void StoreMetric_NullMetric_ThrowsArgumentNullException()
    {
        var act = () => _sut.StoreMetric(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void StoreMetric_DifferentMetricNames_StoresSeparately()
    {
        // Arrange & Act
        _sut.StoreMetric(CreateMetric("cpu.usage", 50.0));
        _sut.StoreMetric(CreateMetric("memory.used", 2048.0));

        // Assert
        _sut.GetMetricCount("cpu.usage").Should().Be(1);
        _sut.GetMetricCount("memory.used").Should().Be(1);
        _sut.GetAllMetricNames().Should().HaveCount(2);
    }

    // -----------------------------------------------------------------------
    // GetMetrics date range filter tests
    // -----------------------------------------------------------------------

    [Fact]
    public void GetMetrics_WithDateRangeFilter_ReturnsOnlyMatchingMetrics()
    {
        // Arrange
        var baseTime = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        _sut.StoreMetric(CreateMetric("temp", 20.0, baseTime.AddMinutes(-10)));
        _sut.StoreMetric(CreateMetric("temp", 25.0, baseTime));
        _sut.StoreMetric(CreateMetric("temp", 30.0, baseTime.AddMinutes(10)));
        _sut.StoreMetric(CreateMetric("temp", 35.0, baseTime.AddMinutes(20)));

        // Act
        var results = _sut.GetMetrics("temp", baseTime, baseTime.AddMinutes(10));

        // Assert
        results.Should().HaveCount(2);
        results.Select(m => m.Value).Should().BeEquivalentTo(new[] { 25.0, 30.0 });
    }

    [Fact]
    public void GetMetrics_NonExistentMetric_ReturnsEmpty()
    {
        // Act
        var results = _sut.GetMetrics("nonexistent", DateTime.MinValue, DateTime.MaxValue);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetMetrics_NoMetricsInRange_ReturnsEmpty()
    {
        // Arrange
        var baseTime = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        _sut.StoreMetric(CreateMetric("temp", 20.0, baseTime));

        // Act — query a range that doesn't include the stored metric
        var results = _sut.GetMetrics("temp", baseTime.AddHours(1), baseTime.AddHours(2));

        // Assert
        results.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // QueryMetricsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task QueryMetricsAsync_ReturnsFilteredResults()
    {
        // Arrange
        var baseTime = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        _sut.StoreMetric(CreateMetric("cpu", 10.0, baseTime));
        _sut.StoreMetric(CreateMetric("cpu", 20.0, baseTime.AddMinutes(5)));
        _sut.StoreMetric(CreateMetric("cpu", 30.0, baseTime.AddMinutes(10)));

        // Act
        var results = (await _sut.QueryMetricsAsync("cpu", baseTime, baseTime.AddMinutes(5))).ToList();

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryMetricsAsync_NonExistentMetric_ReturnsEmpty()
    {
        // Act
        var results = (await _sut.QueryMetricsAsync("does.not.exist", DateTime.MinValue, DateTime.MaxValue)).ToList();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task QueryMetricsAsync_WithTags_FiltersCorrectly()
    {
        // Arrange
        var baseTime = new DateTime(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var tags1 = new Dictionary<string, string> { ["host"] = "server-01" };
        var tags2 = new Dictionary<string, string> { ["host"] = "server-02" };

        _sut.StoreMetric(new Metric
        {
            Name = "cpu",
            Value = 50.0,
            Timestamp = baseTime,
            Tags = tags1
        });
        _sut.StoreMetric(new Metric
        {
            Name = "cpu",
            Value = 60.0,
            Timestamp = baseTime.AddMinutes(1),
            Tags = tags2
        });

        // Act
        var results = (await _sut.QueryMetricsAsync("cpu", baseTime, baseTime.AddMinutes(5), tags1)).ToList();

        // Assert
        results.Should().HaveCount(1);
        results[0].Value.Should().Be(50.0);
    }

    // -----------------------------------------------------------------------
    // GetAllMetricNames tests
    // -----------------------------------------------------------------------

    [Fact]
    public void GetAllMetricNames_NoMetrics_ReturnsEmpty()
    {
        _sut.GetAllMetricNames().Should().BeEmpty();
    }

    [Fact]
    public void GetAllMetricNames_MultipleMetrics_ReturnsAllNames()
    {
        // Arrange
        _sut.StoreMetric(CreateMetric("cpu.usage", 50.0));
        _sut.StoreMetric(CreateMetric("memory.used", 2048.0));
        _sut.StoreMetric(CreateMetric("disk.io", 100.0));

        // Act
        var names = _sut.GetAllMetricNames();

        // Assert
        names.Should().HaveCount(3);
        names.Should().Contain(new[] { "cpu.usage", "memory.used", "disk.io" });
    }

    [Fact]
    public void GetAllMetricNames_DuplicateMetricNames_ReturnsDistinctNames()
    {
        // Arrange
        _sut.StoreMetric(CreateMetric("cpu.usage", 50.0));
        _sut.StoreMetric(CreateMetric("cpu.usage", 60.0));
        _sut.StoreMetric(CreateMetric("cpu.usage", 70.0));

        // Act
        var names = _sut.GetAllMetricNames();

        // Assert
        names.Should().HaveCount(1);
        names.Should().Contain("cpu.usage");
    }

    // -----------------------------------------------------------------------
    // Thread safety tests
    // -----------------------------------------------------------------------

    [Fact]
    public void StoreMetric_ConcurrentWrites_MaintainsConsistency()
    {
        // Arrange
        const int threadCount = 10;
        const int metricsPerThread = 100;

        // Act
        Parallel.For(0, threadCount, threadIndex =>
        {
            for (var i = 0; i < metricsPerThread; i++)
            {
                _sut.StoreMetric(CreateMetric($"concurrent.metric.{threadIndex}", i * 1.0));
            }
        });

        // Assert
        var names = _sut.GetAllMetricNames();
        names.Should().HaveCount(threadCount);

        for (var t = 0; t < threadCount; t++)
        {
            _sut.GetMetricCount($"concurrent.metric.{t}").Should().Be(metricsPerThread);
        }
    }

    [Fact]
    public void StoreMetric_ConcurrentWritesToSameMetric_AllRecorded()
    {
        // Arrange
        const int threadCount = 8;
        const int metricsPerThread = 50;

        // Act
        Parallel.For(0, threadCount, _ =>
        {
            for (var i = 0; i < metricsPerThread; i++)
            {
                _sut.StoreMetric(CreateMetric("shared.metric", i * 1.0));
            }
        });

        // Assert
        _sut.GetMetricCount("shared.metric").Should().Be(threadCount * metricsPerThread);
    }

    // -----------------------------------------------------------------------
    // Trim to 10,000 entries per metric
    // -----------------------------------------------------------------------

    [Fact]
    public void StoreMetric_ExceedsMaxEntries_TrimsToLimit()
    {
        // Arrange
        var maxEntries = InMemoryMetricsStoreAdapter.MaxEntriesPerMetric;

        // Act — store more than the maximum
        for (var i = 0; i < maxEntries + 500; i++)
        {
            _sut.StoreMetric(CreateMetric("large.metric", i * 1.0));
        }

        // Assert — should be trimmed to the max
        _sut.GetMetricCount("large.metric").Should().BeLessThanOrEqualTo(maxEntries);
    }

    [Fact]
    public void StoreMetric_ExactlyMaxEntries_DoesNotTrim()
    {
        // Arrange
        var maxEntries = InMemoryMetricsStoreAdapter.MaxEntriesPerMetric;

        // Act — store exactly the maximum
        for (var i = 0; i < maxEntries; i++)
        {
            _sut.StoreMetric(CreateMetric("exact.metric", i * 1.0));
        }

        // Assert — all should be retained
        _sut.GetMetricCount("exact.metric").Should().Be(maxEntries);
    }

    // -----------------------------------------------------------------------
    // GetTotalMetricCount tests
    // -----------------------------------------------------------------------

    [Fact]
    public void GetTotalMetricCount_ReturnsCorrectTotal()
    {
        // Arrange
        _sut.StoreMetric(CreateMetric("a", 1.0));
        _sut.StoreMetric(CreateMetric("a", 2.0));
        _sut.StoreMetric(CreateMetric("b", 3.0));

        // Act & Assert
        _sut.GetTotalMetricCount().Should().Be(3);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static Metric CreateMetric(string name, double value, DateTime? timestamp = null)
    {
        return new Metric
        {
            Name = name,
            Value = value,
            Timestamp = timestamp ?? DateTime.UtcNow,
            Tags = new Dictionary<string, string>()
        };
    }
}
