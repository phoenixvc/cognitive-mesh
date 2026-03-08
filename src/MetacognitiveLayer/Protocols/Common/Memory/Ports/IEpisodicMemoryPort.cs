namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// Type of episode.
/// </summary>
public enum EpisodeType
{
    /// <summary>Task execution episode.</summary>
    TaskExecution,
    /// <summary>Conversation episode.</summary>
    Conversation,
    /// <summary>Learning episode.</summary>
    Learning,
    /// <summary>Problem-solving episode.</summary>
    ProblemSolving,
    /// <summary>Error/recovery episode.</summary>
    ErrorRecovery
}

/// <summary>
/// An episode in memory.
/// </summary>
public class Episode
{
    /// <summary>Episode identifier.</summary>
    public required string EpisodeId { get; init; }

    /// <summary>Episode type.</summary>
    public EpisodeType Type { get; init; }

    /// <summary>Episode summary.</summary>
    public required string Summary { get; init; }

    /// <summary>Detailed content.</summary>
    public required string Content { get; init; }

    /// <summary>Key events in the episode.</summary>
    public IReadOnlyList<EpisodeEvent> Events { get; init; } = Array.Empty<EpisodeEvent>();

    /// <summary>Outcome.</summary>
    public required string Outcome { get; init; }

    /// <summary>Whether the episode was successful.</summary>
    public bool Success { get; init; }

    /// <summary>Lessons learned.</summary>
    public IReadOnlyList<string> LessonsLearned { get; init; } = Array.Empty<string>();

    /// <summary>Entities involved.</summary>
    public IReadOnlyList<string> Entities { get; init; } = Array.Empty<string>();

    /// <summary>Embedding for retrieval.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Importance score (0.0 - 1.0).</summary>
    public double Importance { get; init; }

    /// <summary>Access count.</summary>
    public int AccessCount { get; init; }

    /// <summary>Last accessed.</summary>
    public DateTimeOffset LastAccessedAt { get; init; }

    /// <summary>When the episode occurred.</summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>Tags.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An event within an episode.
/// </summary>
public class EpisodeEvent
{
    /// <summary>Event description.</summary>
    public required string Description { get; init; }

    /// <summary>Event type (action, observation, decision, etc.).</summary>
    public required string EventType { get; init; }

    /// <summary>When it occurred.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Associated data.</summary>
    public Dictionary<string, string> Data { get; init; } = new();
}

/// <summary>
/// Query for episodic memory.
/// </summary>
public class EpisodicQuery
{
    /// <summary>Query text.</summary>
    public string? Query { get; init; }

    /// <summary>Query embedding.</summary>
    public float[]? QueryEmbedding { get; init; }

    /// <summary>Filter by type.</summary>
    public EpisodeType? Type { get; init; }

    /// <summary>Filter by time range.</summary>
    public DateTimeOffset? Since { get; init; }
    public DateTimeOffset? Until { get; init; }

    /// <summary>Filter by success.</summary>
    public bool? SuccessfulOnly { get; init; }

    /// <summary>Filter by entities.</summary>
    public IReadOnlyList<string>? Entities { get; init; }

    /// <summary>Filter by tags.</summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>Minimum importance.</summary>
    public double? MinImportance { get; init; }

    /// <summary>Maximum results.</summary>
    public int TopK { get; init; } = 10;

    /// <summary>Whether to boost by recency.</summary>
    public bool BoostByRecency { get; init; } = true;

    /// <summary>Whether to boost by access frequency.</summary>
    public bool BoostByFrequency { get; init; } = false;
}

/// <summary>
/// Port for episodic memory.
/// Implements the "Episodic Memory" pattern.
/// </summary>
public interface IEpisodicMemoryPort
{
    /// <summary>
    /// Stores an episode.
    /// </summary>
    Task StoreEpisodeAsync(
        Episode episode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves episodes matching a query.
    /// </summary>
    Task<IReadOnlyList<Episode>> RetrieveAsync(
        EpisodicQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an episode by ID.
    /// </summary>
    Task<Episode?> GetEpisodeAsync(
        string episodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an episode's importance.
    /// </summary>
    Task UpdateImportanceAsync(
        string episodeId,
        double importance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consolidates memories (moves to long-term, summarizes).
    /// </summary>
    Task ConsolidateAsync(
        TimeSpan olderThan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forgets low-importance episodes.
    /// </summary>
    Task ForgetAsync(
        double belowImportance,
        TimeSpan olderThan,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets related episodes.
    /// </summary>
    Task<IReadOnlyList<Episode>> GetRelatedAsync(
        string episodeId,
        int topK = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts lessons from episodes.
    /// </summary>
    Task<IReadOnlyList<string>> ExtractLessonsAsync(
        IEnumerable<string> episodeIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets memory statistics.
    /// </summary>
    Task<EpisodicMemoryStatistics> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Episodic memory statistics.
/// </summary>
public class EpisodicMemoryStatistics
{
    public int TotalEpisodes { get; init; }
    public int RecentEpisodes { get; init; }
    public double AverageImportance { get; init; }
    public Dictionary<EpisodeType, int> ByType { get; init; } = new();
    public int ConsolidatedEpisodes { get; init; }
    public DateTimeOffset OldestEpisode { get; init; }
    public DateTimeOffset NewestEpisode { get; init; }
}
