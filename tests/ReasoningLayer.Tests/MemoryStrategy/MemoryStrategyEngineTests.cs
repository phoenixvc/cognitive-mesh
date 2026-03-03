using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Engines;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

namespace CognitiveMesh.ReasoningLayer.Tests.MemoryStrategy;

public class MemoryStrategyEngineTests
{
    private readonly Mock<ILogger<MemoryStrategyEngine>> _loggerMock;
    private readonly MemoryStrategyEngine _engine;

    public MemoryStrategyEngineTests()
    {
        _loggerMock = new Mock<ILogger<MemoryStrategyEngine>>();
        _engine = new MemoryStrategyEngine(_loggerMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new MemoryStrategyEngine(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ─── Store/Get/Update/Delete CRUD ─────────────────────────────────

    [Fact]
    public async Task StoreAsync_ValidRecord_ReturnsStoredRecord()
    {
        // Arrange
        var record = CreateRecord("rec-1", "Hello world", importance: 0.8);

        // Act
        var result = await _engine.StoreAsync(record);

        // Assert
        result.Should().NotBeNull();
        result.RecordId.Should().Be("rec-1");
        result.Content.Should().Be("Hello world");
        result.Importance.Should().Be(0.8);
    }

    [Fact]
    public async Task StoreAsync_DuplicateRecord_ThrowsInvalidOperationException()
    {
        // Arrange
        var record = CreateRecord("rec-dup", "Duplicate");
        await _engine.StoreAsync(record);

        // Act
        var act = () => _engine.StoreAsync(record);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*rec-dup*already exists*");
    }

    [Fact]
    public async Task StoreAsync_NullRecord_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.StoreAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetAsync_ExistingRecord_ReturnsRecord()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("rec-get", "Get me"));

        // Act
        var result = await _engine.GetAsync("rec-get");

        // Assert
        result.Should().NotBeNull();
        result!.RecordId.Should().Be("rec-get");
    }

    [Fact]
    public async Task GetAsync_NonExistentRecord_ReturnsNull()
    {
        // Act
        var result = await _engine.GetAsync("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingRecord_ReturnsUpdatedRecord()
    {
        // Arrange
        var record = CreateRecord("rec-upd", "Original", importance: 0.3);
        await _engine.StoreAsync(record);

        record.Content = "Updated content";
        record.Importance = 0.9;

        // Act
        var result = await _engine.UpdateAsync(record);

        // Assert
        result.Content.Should().Be("Updated content");
        result.Importance.Should().Be(0.9);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentRecord_ThrowsKeyNotFoundException()
    {
        // Arrange
        var record = CreateRecord("rec-no-exist", "Missing");

        // Act
        var act = () => _engine.UpdateAsync(record);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("rec-del", "Delete me"));

        // Act
        var result = await _engine.DeleteAsync("rec-del");

        // Assert
        result.Should().BeTrue();
        (await _engine.GetAsync("rec-del")).Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentRecord_ReturnsFalse()
    {
        // Act
        var result = await _engine.DeleteAsync("nonexistent-del");

        // Assert
        result.Should().BeFalse();
    }

    // ─── Recall with ExactMatch strategy ──────────────────────────────

    [Fact]
    public async Task RecallAsync_ExactMatch_FindsExactContent()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("em-1", "Security incident detected"));
        await _engine.StoreAsync(CreateRecord("em-2", "Performance degradation"));
        await _engine.StoreAsync(CreateRecord("em-3", "Unrelated noise"));

        var query = new RecallQuery
        {
            QueryText = "Security incident detected",
            Strategy = RecallStrategy.ExactMatch,
            MaxResults = 5
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().HaveCountGreaterThanOrEqualTo(1);
        result.StrategyUsed.Should().Be(RecallStrategy.ExactMatch);
        result.Records.First().Content.Should().Be("Security incident detected");
        result.RelevanceScores["em-1"].Should().Be(1.0);
    }

    [Fact]
    public async Task RecallAsync_ExactMatch_FindsByTag()
    {
        // Arrange
        var record = CreateRecord("em-tag", "Some content", tags: new List<string> { "security", "alert" });
        await _engine.StoreAsync(record);

        var query = new RecallQuery
        {
            QueryText = "security",
            Strategy = RecallStrategy.ExactMatch,
            MaxResults = 5
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Records.Should().Contain(r => r.RecordId == "em-tag");
    }

    // ─── Recall with SemanticSimilarity ───────────────────────────────

    [Fact]
    public async Task RecallAsync_SemanticSimilarity_FindsSimilarEmbeddings()
    {
        // Arrange — two records with similar embeddings, one with different
        var similar1 = CreateRecord("sem-1", "Machine learning model", embedding: [1.0f, 0.0f, 0.0f]);
        var similar2 = CreateRecord("sem-2", "Deep learning network", embedding: [0.9f, 0.1f, 0.0f]);
        var different = CreateRecord("sem-3", "Cooking recipe", embedding: [0.0f, 0.0f, 1.0f]);

        await _engine.StoreAsync(similar1);
        await _engine.StoreAsync(similar2);
        await _engine.StoreAsync(different);

        var query = new RecallQuery
        {
            QueryText = "ML model",
            QueryEmbedding = [1.0f, 0.0f, 0.0f],
            Strategy = RecallStrategy.SemanticSimilarity,
            MaxResults = 3
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().HaveCountGreaterThanOrEqualTo(1);
        result.StrategyUsed.Should().Be(RecallStrategy.SemanticSimilarity);

        // The most similar record should be ranked first
        if (result.Records.Count >= 2)
        {
            result.RelevanceScores["sem-1"].Should().BeGreaterThan(result.RelevanceScores["sem-3"]);
        }
    }

    [Fact]
    public async Task RecallAsync_SemanticSimilarity_NoEmbedding_ReturnsEmpty()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("no-emb", "No embedding record"));

        var query = new RecallQuery
        {
            QueryText = "test",
            QueryEmbedding = null,
            Strategy = RecallStrategy.SemanticSimilarity,
            MaxResults = 5
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().BeEmpty();
    }

    // ─── Recall with TemporalProximity ────────────────────────────────

    [Fact]
    public async Task RecallAsync_TemporalProximity_ReturnsRecentFirst()
    {
        // Arrange
        var old = CreateRecord("tp-old", "Old record");
        old.CreatedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var recent = CreateRecord("tp-recent", "Recent record");
        recent.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1);

        await _engine.StoreAsync(old);
        await _engine.StoreAsync(recent);

        var query = new RecallQuery
        {
            QueryText = "",
            Strategy = RecallStrategy.TemporalProximity,
            MaxResults = 5
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().HaveCountGreaterThanOrEqualTo(2);
        result.StrategyUsed.Should().Be(RecallStrategy.TemporalProximity);
        // Recent record should have higher score
        result.RelevanceScores["tp-recent"].Should().BeGreaterThan(result.RelevanceScores["tp-old"]);
    }

    // ─── Recall with Hybrid strategy ──────────────────────────────────

    [Fact]
    public async Task RecallAsync_Hybrid_CombinesAllStrategies()
    {
        // Arrange
        var record = CreateRecord("hyb-1", "Security alert",
            tags: new List<string> { "security" },
            embedding: [1.0f, 0.5f, 0.0f],
            importance: 0.9);
        await _engine.StoreAsync(record);

        var query = new RecallQuery
        {
            QueryText = "Security alert",
            QueryEmbedding = [1.0f, 0.5f, 0.0f],
            Strategy = RecallStrategy.Hybrid,
            MaxResults = 5
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        result.Records.Should().HaveCountGreaterThanOrEqualTo(1);
        result.StrategyUsed.Should().Be(RecallStrategy.Hybrid);
        result.RelevanceScores["hyb-1"].Should().BeGreaterThan(0);
    }

    // ─── Consolidation: promotes high-importance records ──────────────

    [Fact]
    public async Task ConsolidateAsync_HighImportanceFrequentAccess_PromotesRecord()
    {
        // Arrange
        var record = CreateRecord("cons-promote", "Important finding", importance: 0.8);
        record.AccessCount = 5; // Above threshold
        record.Consolidated = false;
        await _engine.StoreAsync(record);

        // Act
        var result = await _engine.ConsolidateAsync(
            accessCountThreshold: 3,
            importanceThreshold: 0.5);

        // Assert
        result.PromotedCount.Should().Be(1);
        var stored = await _engine.GetAsync("cons-promote");
        stored!.Consolidated.Should().BeTrue();
    }

    // ─── Consolidation: prunes old unaccessed records ─────────────────

    [Fact]
    public async Task ConsolidateAsync_OldUnaccessed_PrunesRecord()
    {
        // Arrange
        var record = CreateRecord("cons-prune", "Obsolete data", importance: 0.1);
        record.AccessCount = 0;
        record.CreatedAt = DateTimeOffset.UtcNow.AddDays(-60);
        await _engine.StoreAsync(record);

        // Act
        var result = await _engine.ConsolidateAsync(
            accessCountThreshold: 3,
            importanceThreshold: 0.5,
            pruneAge: TimeSpan.FromDays(30));

        // Assert
        result.PrunedCount.Should().Be(1);
        var stored = await _engine.GetAsync("cons-prune");
        stored.Should().BeNull();
    }

    [Fact]
    public async Task ConsolidateAsync_RecentRecordWithNoAccess_RetainsRecord()
    {
        // Arrange
        var record = CreateRecord("cons-retain", "New record", importance: 0.3);
        record.AccessCount = 0;
        record.CreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        await _engine.StoreAsync(record);

        // Act
        var result = await _engine.ConsolidateAsync(
            accessCountThreshold: 3,
            importanceThreshold: 0.5,
            pruneAge: TimeSpan.FromDays(30));

        // Assert
        result.RetainedCount.Should().BeGreaterThanOrEqualTo(1);
        var stored = await _engine.GetAsync("cons-retain");
        stored.Should().NotBeNull();
    }

    // ─── Strategy adaptation ──────────────────────────────────────────

    [Fact]
    public async Task GetBestStrategyAsync_NoData_ReturnsHybrid()
    {
        // Act
        var result = await _engine.GetBestStrategyAsync();

        // Assert
        result.Should().Be(RecallStrategy.Hybrid);
    }

    [Fact]
    public async Task GetBestStrategyAsync_WithPerformanceData_ReturnsBestPerforming()
    {
        // Arrange — record performance for different strategies
        await _engine.RecordPerformanceAsync(RecallStrategy.ExactMatch, 0.9, 1.0, true);
        await _engine.RecordPerformanceAsync(RecallStrategy.ExactMatch, 0.8, 1.5, true);
        await _engine.RecordPerformanceAsync(RecallStrategy.FuzzyMatch, 0.5, 3.0, false);
        await _engine.RecordPerformanceAsync(RecallStrategy.SemanticSimilarity, 0.7, 2.0, true);
        await _engine.RecordPerformanceAsync(RecallStrategy.SemanticSimilarity, 0.6, 2.5, false);

        // Act
        var result = await _engine.GetBestStrategyAsync();

        // Assert — ExactMatch has highest hit rate (100%)
        result.Should().Be(RecallStrategy.ExactMatch);
    }

    // ─── Performance recording and retrieval ──────────────────────────

    [Fact]
    public async Task RecordPerformanceAsync_RecordsAndUpdatesRunningAverages()
    {
        // Arrange & Act
        var perf1 = await _engine.RecordPerformanceAsync(RecallStrategy.ExactMatch, 0.8, 2.0, true);
        var perf2 = await _engine.RecordPerformanceAsync(RecallStrategy.ExactMatch, 0.6, 4.0, false);

        // Assert
        perf2.SampleCount.Should().Be(2);
        perf2.AvgRelevanceScore.Should().BeApproximately(0.7, 0.01);
        perf2.AvgLatencyMs.Should().BeApproximately(3.0, 0.01);
        perf2.HitRate.Should().BeApproximately(0.5, 0.01);
    }

    [Fact]
    public async Task RecordPerformanceAsync_SingleSample_CorrectValues()
    {
        // Act
        var perf = await _engine.RecordPerformanceAsync(RecallStrategy.TemporalProximity, 0.95, 0.5, true);

        // Assert
        perf.Strategy.Should().Be(RecallStrategy.TemporalProximity);
        perf.SampleCount.Should().Be(1);
        perf.AvgRelevanceScore.Should().Be(0.95);
        perf.AvgLatencyMs.Should().Be(0.5);
        perf.HitRate.Should().Be(1.0);
    }

    // ─── Statistics calculation ───────────────────────────────────────

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        var r1 = CreateRecord("stat-1", "Record 1", importance: 0.8);
        r1.Consolidated = true;
        var r2 = CreateRecord("stat-2", "Record 2", importance: 0.4);
        var r3 = CreateRecord("stat-3", "Record 3", importance: 0.6);

        await _engine.StoreAsync(r1);
        await _engine.StoreAsync(r2);
        await _engine.StoreAsync(r3);

        await _engine.RecordPerformanceAsync(RecallStrategy.ExactMatch, 0.9, 1.0, true);

        // Act
        var stats = await _engine.GetStatisticsAsync();

        // Assert
        stats.TotalRecords.Should().Be(3);
        stats.ConsolidatedCount.Should().Be(1);
        stats.AvgImportance.Should().BeApproximately(0.6, 0.01);
        stats.StrategyPerformanceMap.Should().ContainKey(RecallStrategy.ExactMatch);
    }

    [Fact]
    public async Task GetStatisticsAsync_EmptyStore_ReturnsZeros()
    {
        // Act
        var stats = await _engine.GetStatisticsAsync();

        // Assert
        stats.TotalRecords.Should().Be(0);
        stats.ConsolidatedCount.Should().Be(0);
        stats.AvgImportance.Should().Be(0.0);
    }

    // ─── Recall by tags ───────────────────────────────────────────────

    [Fact]
    public async Task RecallByTagsAsync_MatchingTags_ReturnsRecords()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("tag-1", "Alert record", tags: new List<string> { "security", "alert" }));
        await _engine.StoreAsync(CreateRecord("tag-2", "Perf record", tags: new List<string> { "performance" }));
        await _engine.StoreAsync(CreateRecord("tag-3", "Other alert", tags: new List<string> { "alert" }));

        // Act
        var result = await _engine.RecallByTagsAsync(new List<string> { "alert" });

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.RecordId == "tag-1");
        result.Should().Contain(r => r.RecordId == "tag-3");
    }

    // ─── Recall recent ────────────────────────────────────────────────

    [Fact]
    public async Task RecallRecentAsync_ReturnsInOrder()
    {
        // Arrange
        var old = CreateRecord("recent-1", "Old");
        old.LastAccessedAt = DateTimeOffset.UtcNow.AddHours(-5);
        old.CreatedAt = DateTimeOffset.UtcNow.AddHours(-10);

        var newer = CreateRecord("recent-2", "Newer");
        newer.LastAccessedAt = DateTimeOffset.UtcNow;
        newer.CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5);

        await _engine.StoreAsync(old);
        await _engine.StoreAsync(newer);

        // Act
        var result = await _engine.RecallRecentAsync(5);

        // Assert
        result.Should().HaveCount(2);
        result[0].RecordId.Should().Be("recent-2"); // Most recent access first
    }

    // ─── Cosine similarity helper ─────────────────────────────────────

    [Fact]
    public void CalculateCosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        // Arrange
        var vector = new float[] { 1.0f, 2.0f, 3.0f };

        // Act
        var result = MemoryStrategyEngine.CalculateCosineSimilarity(vector, vector);

        // Assert
        result.Should().BeApproximately(1.0, 0.001);
    }

    [Fact]
    public void CalculateCosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        // Arrange
        var vectorA = new float[] { 1.0f, 0.0f, 0.0f };
        var vectorB = new float[] { 0.0f, 1.0f, 0.0f };

        // Act
        var result = MemoryStrategyEngine.CalculateCosineSimilarity(vectorA, vectorB);

        // Assert
        result.Should().BeApproximately(0.0, 0.001);
    }

    [Fact]
    public void CalculateCosineSimilarity_EmptyVectors_ReturnsZero()
    {
        // Act
        var result = MemoryStrategyEngine.CalculateCosineSimilarity([], []);

        // Assert
        result.Should().Be(0.0);
    }

    // ─── MinRelevance filtering ───────────────────────────────────────

    [Fact]
    public async Task RecallAsync_MinRelevanceFilter_ExcludesLowScoreRecords()
    {
        // Arrange
        await _engine.StoreAsync(CreateRecord("rel-1", "Exact match text"));
        await _engine.StoreAsync(CreateRecord("rel-2", "Completely different content here"));

        var query = new RecallQuery
        {
            QueryText = "Exact match text",
            Strategy = RecallStrategy.ExactMatch,
            MinRelevance = 0.9,
            MaxResults = 10
        };

        // Act
        var result = await _engine.RecallAsync(query);

        // Assert
        foreach (var (_, score) in result.RelevanceScores)
        {
            score.Should().BeGreaterThanOrEqualTo(0.9);
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private static MemoryRecord CreateRecord(
        string recordId,
        string content,
        double importance = 0.5,
        float[]? embedding = null,
        List<string>? tags = null)
    {
        return new MemoryRecord
        {
            RecordId = recordId,
            Content = content,
            Importance = importance,
            Embedding = embedding,
            Tags = tags ?? [],
            CreatedAt = DateTimeOffset.UtcNow,
            LastAccessedAt = DateTimeOffset.UtcNow
        };
    }
}
