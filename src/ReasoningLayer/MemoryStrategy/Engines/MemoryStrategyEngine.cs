using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Ports;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Engines;

/// <summary>
/// Core engine implementing the Memory and Flexible Strategy system.
/// Provides episodic memory storage with multiple recall strategies (exact match,
/// fuzzy, semantic similarity, temporal proximity, and hybrid), memory consolidation
/// (short-term to long-term promotion), and strategy adaptation based on past performance.
/// </summary>
public sealed class MemoryStrategyEngine : IMemoryStorePort, IRecallPort, IConsolidationPort, IStrategyAdaptationPort
{
    private readonly ILogger<MemoryStrategyEngine> _logger;

    private readonly ConcurrentDictionary<string, MemoryRecord> _records = new();
    private readonly ConcurrentDictionary<RecallStrategy, StrategyPerformance> _strategyPerformance = new();

    /// <summary>
    /// Default prune age for consolidation: records older than this with no access are pruned.
    /// </summary>
    internal static readonly TimeSpan DefaultPruneAge = TimeSpan.FromDays(30);

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryStrategyEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null.</exception>
    public MemoryStrategyEngine(ILogger<MemoryStrategyEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── IMemoryStorePort ─────────────────────────────────────────────

    /// <inheritdoc />
    public Task<MemoryRecord> StoreAsync(MemoryRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.RecordId);

        if (!_records.TryAdd(record.RecordId, record))
        {
            throw new InvalidOperationException($"Record with ID '{record.RecordId}' already exists.");
        }

        _logger.LogInformation(
            "Stored memory record '{RecordId}' with importance {Importance:F2}.",
            record.RecordId, record.Importance);

        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task<MemoryRecord?> GetAsync(string recordId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordId);

        _records.TryGetValue(recordId, out var record);
        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task<MemoryRecord> UpdateAsync(MemoryRecord record, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentException.ThrowIfNullOrWhiteSpace(record.RecordId);

        if (!_records.ContainsKey(record.RecordId))
        {
            throw new KeyNotFoundException($"Record with ID '{record.RecordId}' not found.");
        }

        _records[record.RecordId] = record;

        _logger.LogInformation("Updated memory record '{RecordId}'.", record.RecordId);

        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string recordId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordId);

        var removed = _records.TryRemove(recordId, out _);

        if (removed)
        {
            _logger.LogInformation("Deleted memory record '{RecordId}'.", recordId);
        }

        return Task.FromResult(removed);
    }

    /// <inheritdoc />
    public Task<MemoryStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var records = _records.Values.ToList();

        var stats = new MemoryStatistics
        {
            TotalRecords = records.Count,
            ConsolidatedCount = records.Count(r => r.Consolidated),
            AvgImportance = records.Count > 0 ? records.Average(r => r.Importance) : 0.0,
            StrategyPerformanceMap = new Dictionary<RecallStrategy, StrategyPerformance>(
                _strategyPerformance.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
        };

        return Task.FromResult(stats);
    }

    // ─── IRecallPort ──────────────────────────────────────────────────

    /// <inheritdoc />
    public Task<RecallResult> RecallAsync(RecallQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var stopwatch = Stopwatch.StartNew();
        var allRecords = _records.Values.ToList();
        var candidates = FilterByTimeWindow(allRecords, query.TimeWindow);
        var totalCandidates = candidates.Count;

        var scoredRecords = query.Strategy switch
        {
            RecallStrategy.ExactMatch => ScoreExactMatch(candidates, query),
            RecallStrategy.FuzzyMatch => ScoreFuzzyMatch(candidates, query),
            RecallStrategy.SemanticSimilarity => ScoreSemanticSimilarity(candidates, query),
            RecallStrategy.TemporalProximity => ScoreTemporalProximity(candidates),
            RecallStrategy.Hybrid => ScoreHybrid(candidates, query),
            _ => ScoreHybrid(candidates, query)
        };

        // Filter by minimum relevance and take top N
        var filteredScores = scoredRecords
            .Where(kvp => kvp.Value >= query.MinRelevance)
            .OrderByDescending(kvp => kvp.Value)
            .Take(query.MaxResults)
            .ToList();

        var resultRecords = new List<MemoryRecord>();
        var relevanceScores = new Dictionary<string, double>();

        foreach (var (recordId, score) in filteredScores)
        {
            if (_records.TryGetValue(recordId, out var record))
            {
                // Update access tracking
                record.LastAccessedAt = DateTimeOffset.UtcNow;
                record.AccessCount++;

                resultRecords.Add(record);
                relevanceScores[recordId] = score;
            }
        }

        stopwatch.Stop();

        var result = new RecallResult
        {
            Records = resultRecords,
            StrategyUsed = query.Strategy,
            QueryDurationMs = stopwatch.Elapsed.TotalMilliseconds,
            TotalCandidates = totalCandidates,
            RelevanceScores = relevanceScores
        };

        _logger.LogInformation(
            "Recall with strategy {Strategy}: returned {Count}/{Candidates} records in {Duration:F3}ms",
            query.Strategy, resultRecords.Count, totalCandidates, result.QueryDurationMs);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryRecord>> RecallByTagsAsync(
        IReadOnlyList<string> tags,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tags);

        var tagSet = new HashSet<string>(tags, StringComparer.OrdinalIgnoreCase);

        var result = _records.Values
            .Where(r => r.Tags.Any(t => tagSet.Contains(t)))
            .OrderByDescending(r => r.Tags.Count(t => tagSet.Contains(t)))
            .ThenByDescending(r => r.Importance)
            .Take(maxResults)
            .ToList();

        // Update access tracking
        foreach (var record in result)
        {
            record.LastAccessedAt = DateTimeOffset.UtcNow;
            record.AccessCount++;
        }

        return Task.FromResult<IReadOnlyList<MemoryRecord>>(result);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<MemoryRecord>> RecallRecentAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var result = _records.Values
            .OrderByDescending(r => r.LastAccessedAt)
            .ThenByDescending(r => r.CreatedAt)
            .Take(count)
            .ToList();

        return Task.FromResult<IReadOnlyList<MemoryRecord>>(result);
    }

    // ─── IConsolidationPort ───────────────────────────────────────────

    /// <inheritdoc />
    public Task<ConsolidationResult> ConsolidateAsync(
        int accessCountThreshold = 3,
        double importanceThreshold = 0.5,
        TimeSpan? pruneAge = null,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var effectivePruneAge = pruneAge ?? DefaultPruneAge;
        var cutoffTime = DateTimeOffset.UtcNow - effectivePruneAge;

        var promoted = 0;
        var pruned = 0;
        var retained = 0;

        var recordsToRemove = new List<string>();

        foreach (var kvp in _records)
        {
            var record = kvp.Value;

            // Promotion: high access count AND high importance, not yet consolidated
            if (!record.Consolidated &&
                record.AccessCount >= accessCountThreshold &&
                record.Importance >= importanceThreshold)
            {
                record.Consolidated = true;
                promoted++;
                _logger.LogDebug("Promoted record '{RecordId}' to long-term memory.", record.RecordId);
            }
            // Pruning: old, unaccessed records
            else if (record.CreatedAt < cutoffTime && record.AccessCount == 0)
            {
                recordsToRemove.Add(record.RecordId);
                pruned++;
                _logger.LogDebug("Pruning record '{RecordId}' (old, unaccessed).", record.RecordId);
            }
            else
            {
                retained++;
            }
        }

        // Remove pruned records
        foreach (var recordId in recordsToRemove)
        {
            _records.TryRemove(recordId, out _);
        }

        stopwatch.Stop();

        var result = new ConsolidationResult(
            PromotedCount: promoted,
            PrunedCount: pruned,
            RetainedCount: retained,
            DurationMs: stopwatch.Elapsed.TotalMilliseconds);

        _logger.LogInformation(
            "Consolidation complete: promoted={Promoted}, pruned={Pruned}, retained={Retained} in {Duration:F3}ms",
            promoted, pruned, retained, result.DurationMs);

        return Task.FromResult(result);
    }

    // ─── IStrategyAdaptationPort ──────────────────────────────────────

    /// <inheritdoc />
    public Task<RecallStrategy> GetBestStrategyAsync(CancellationToken cancellationToken = default)
    {
        if (_strategyPerformance.IsEmpty)
        {
            _logger.LogDebug("No strategy performance data available; defaulting to Hybrid.");
            return Task.FromResult(RecallStrategy.Hybrid);
        }

        var best = _strategyPerformance.Values
            .Where(sp => sp.SampleCount > 0)
            .OrderByDescending(sp => sp.HitRate)
            .ThenByDescending(sp => sp.AvgRelevanceScore)
            .ThenBy(sp => sp.AvgLatencyMs)
            .FirstOrDefault();

        var result = best?.Strategy ?? RecallStrategy.Hybrid;

        _logger.LogInformation("Best strategy recommended: {Strategy} (hitRate={HitRate:F3})",
            result, best?.HitRate ?? 0.0);

        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<StrategyPerformance> RecordPerformanceAsync(
        RecallStrategy strategy,
        double relevanceScore,
        double latencyMs,
        bool wasHit,
        CancellationToken cancellationToken = default)
    {
        var perf = _strategyPerformance.GetOrAdd(strategy, _ => new StrategyPerformance
        {
            Strategy = strategy
        });

        // Update running averages
        var newSampleCount = perf.SampleCount + 1;
        perf.AvgRelevanceScore = ((perf.AvgRelevanceScore * perf.SampleCount) + relevanceScore) / newSampleCount;
        perf.AvgLatencyMs = ((perf.AvgLatencyMs * perf.SampleCount) + latencyMs) / newSampleCount;
        perf.HitRate = ((perf.HitRate * perf.SampleCount) + (wasHit ? 1.0 : 0.0)) / newSampleCount;
        perf.SampleCount = newSampleCount;

        _logger.LogDebug(
            "Recorded performance for {Strategy}: relevance={Relevance:F3}, latency={Latency:F1}ms, hit={Hit}, samples={Samples}",
            strategy, perf.AvgRelevanceScore, perf.AvgLatencyMs, wasHit, perf.SampleCount);

        return Task.FromResult(perf);
    }

    // ─── Private Scoring Methods ──────────────────────────────────────

    /// <summary>
    /// Filters records to those within the specified time window from now.
    /// </summary>
    private static List<MemoryRecord> FilterByTimeWindow(List<MemoryRecord> records, TimeSpan? timeWindow)
    {
        if (!timeWindow.HasValue)
        {
            return records;
        }

        var cutoff = DateTimeOffset.UtcNow - timeWindow.Value;
        return records.Where(r => r.CreatedAt >= cutoff).ToList();
    }

    /// <summary>
    /// Scores records using exact match strategy: matches tag names or exact content.
    /// </summary>
    private static Dictionary<string, double> ScoreExactMatch(List<MemoryRecord> candidates, RecallQuery query)
    {
        var scores = new Dictionary<string, double>();

        foreach (var record in candidates)
        {
            double score = 0.0;

            // Content exact match
            if (string.Equals(record.Content, query.QueryText, StringComparison.OrdinalIgnoreCase))
            {
                score = 1.0;
            }
            // Tag match
            else if (record.Tags.Any(t => string.Equals(t, query.QueryText, StringComparison.OrdinalIgnoreCase)))
            {
                score = 0.9;
            }
            // Content contains match
            else if (record.Content.Contains(query.QueryText, StringComparison.OrdinalIgnoreCase))
            {
                score = 0.7;
            }

            if (score > 0)
            {
                scores[record.RecordId] = score;
            }
        }

        return scores;
    }

    /// <summary>
    /// Scores records using fuzzy match strategy: Levenshtein-like similarity on content.
    /// </summary>
    private static Dictionary<string, double> ScoreFuzzyMatch(List<MemoryRecord> candidates, RecallQuery query)
    {
        var scores = new Dictionary<string, double>();

        foreach (var record in candidates)
        {
            var similarity = CalculateLevenshteinSimilarity(query.QueryText, record.Content);
            if (similarity > 0)
            {
                scores[record.RecordId] = similarity;
            }
        }

        return scores;
    }

    /// <summary>
    /// Scores records using semantic similarity strategy: cosine similarity on embeddings.
    /// </summary>
    private static Dictionary<string, double> ScoreSemanticSimilarity(List<MemoryRecord> candidates, RecallQuery query)
    {
        var scores = new Dictionary<string, double>();

        if (query.QueryEmbedding == null || query.QueryEmbedding.Length == 0)
        {
            return scores;
        }

        foreach (var record in candidates)
        {
            if (record.Embedding != null && record.Embedding.Length > 0)
            {
                var similarity = CalculateCosineSimilarity(query.QueryEmbedding, record.Embedding);
                // Normalize from [-1,1] to [0,1]
                var normalizedScore = (similarity + 1.0) / 2.0;
                if (normalizedScore > 0)
                {
                    scores[record.RecordId] = normalizedScore;
                }
            }
        }

        return scores;
    }

    /// <summary>
    /// Scores records using temporal proximity strategy: records closest in time score highest.
    /// </summary>
    private static Dictionary<string, double> ScoreTemporalProximity(List<MemoryRecord> candidates)
    {
        var scores = new Dictionary<string, double>();
        var now = DateTimeOffset.UtcNow;

        if (candidates.Count == 0)
        {
            return scores;
        }

        // Find the maximum age to normalize
        var maxAgeMs = candidates.Max(r => Math.Abs((now - r.CreatedAt).TotalMilliseconds));
        if (maxAgeMs <= 0)
        {
            maxAgeMs = 1.0;
        }

        foreach (var record in candidates)
        {
            var ageMs = Math.Abs((now - record.CreatedAt).TotalMilliseconds);
            var score = 1.0 - (ageMs / maxAgeMs);
            scores[record.RecordId] = Math.Max(0.0, score);
        }

        return scores;
    }

    /// <summary>
    /// Scores records using hybrid strategy: weighted combination of all individual strategies.
    /// </summary>
    private static Dictionary<string, double> ScoreHybrid(List<MemoryRecord> candidates, RecallQuery query)
    {
        var exactScores = ScoreExactMatch(candidates, query);
        var fuzzyScores = ScoreFuzzyMatch(candidates, query);
        var semanticScores = ScoreSemanticSimilarity(candidates, query);
        var temporalScores = ScoreTemporalProximity(candidates);

        var allRecordIds = candidates.Select(r => r.RecordId).ToHashSet();
        var hybridScores = new Dictionary<string, double>();

        // Weights for each strategy
        const double exactWeight = 0.30;
        const double fuzzyWeight = 0.25;
        const double semanticWeight = 0.30;
        const double temporalWeight = 0.15;

        foreach (var recordId in allRecordIds)
        {
            var score = 0.0;

            if (exactScores.TryGetValue(recordId, out var exactScore))
            {
                score += exactScore * exactWeight;
            }

            if (fuzzyScores.TryGetValue(recordId, out var fuzzyScore))
            {
                score += fuzzyScore * fuzzyWeight;
            }

            if (semanticScores.TryGetValue(recordId, out var semanticScore))
            {
                score += semanticScore * semanticWeight;
            }

            if (temporalScores.TryGetValue(recordId, out var temporalScore))
            {
                score += temporalScore * temporalWeight;
            }

            if (score > 0)
            {
                hybridScores[recordId] = score;
            }
        }

        return hybridScores;
    }

    // ─── Similarity Helpers ───────────────────────────────────────────

    /// <summary>
    /// Calculates the cosine similarity between two embedding vectors.
    /// Returns a value between -1.0 (opposite) and 1.0 (identical).
    /// </summary>
    /// <param name="vectorA">The first embedding vector.</param>
    /// <param name="vectorB">The second embedding vector.</param>
    /// <returns>Cosine similarity score.</returns>
    internal static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length == 0 || vectorB.Length == 0)
        {
            return 0.0;
        }

        var minLength = Math.Min(vectorA.Length, vectorB.Length);
        double dotProduct = 0.0;
        double magnitudeA = 0.0;
        double magnitudeB = 0.0;

        for (int i = 0; i < minLength; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }

        magnitudeA = Math.Sqrt(magnitudeA);
        magnitudeB = Math.Sqrt(magnitudeB);

        if (magnitudeA <= 0 || magnitudeB <= 0)
        {
            return 0.0;
        }

        return dotProduct / (magnitudeA * magnitudeB);
    }

    /// <summary>
    /// Calculates a normalized Levenshtein similarity between two strings.
    /// Returns 1.0 for identical strings and approaches 0.0 for completely different strings.
    /// </summary>
    internal static double CalculateLevenshteinSimilarity(string source, string target)
    {
        if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
        {
            return 1.0;
        }

        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
        {
            return 0.0;
        }

        // Case-insensitive comparison
        source = source.ToLowerInvariant();
        target = target.ToLowerInvariant();

        var sourceLength = source.Length;
        var targetLength = target.Length;
        var distance = new int[sourceLength + 1, targetLength + 1];

        for (int i = 0; i <= sourceLength; i++) distance[i, 0] = i;
        for (int j = 0; j <= targetLength; j++) distance[0, j] = j;

        for (int i = 1; i <= sourceLength; i++)
        {
            for (int j = 1; j <= targetLength; j++)
            {
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        var maxLength = Math.Max(sourceLength, targetLength);
        return 1.0 - ((double)distance[sourceLength, targetLength] / maxLength);
    }
}
