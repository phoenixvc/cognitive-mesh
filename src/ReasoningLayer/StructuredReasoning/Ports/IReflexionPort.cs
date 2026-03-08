namespace ReasoningLayer.StructuredReasoning.Ports;

/// <summary>
/// Type of reflection.
/// </summary>
public enum ReflectionType
{
    /// <summary>Self-evaluation of performance.</summary>
    SelfEvaluation,
    /// <summary>Feedback incorporation.</summary>
    FeedbackIncorporation,
    /// <summary>Strategy refinement.</summary>
    StrategyRefinement,
    /// <summary>Error analysis.</summary>
    ErrorAnalysis,
    /// <summary>Memory update.</summary>
    MemoryUpdate
}

/// <summary>
/// A reflection entry.
/// </summary>
public class ReflectionEntry
{
    /// <summary>Entry identifier.</summary>
    public string EntryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Type of reflection.</summary>
    public required ReflectionType Type { get; init; }

    /// <summary>The reflection content.</summary>
    public required string Content { get; init; }

    /// <summary>Episode or task this reflects on.</summary>
    public required string EpisodeId { get; init; }

    /// <summary>Key insights extracted.</summary>
    public IReadOnlyList<string> Insights { get; init; } = Array.Empty<string>();

    /// <summary>Suggested improvements.</summary>
    public IReadOnlyList<string> Improvements { get; init; } = Array.Empty<string>();

    /// <summary>Self-assessed performance score (0.0 - 1.0).</summary>
    public double? PerformanceScore { get; init; }

    /// <summary>When the reflection was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// An episode to reflect on.
/// </summary>
public class ReflexionEpisode
{
    /// <summary>Episode identifier.</summary>
    public required string EpisodeId { get; init; }

    /// <summary>The task attempted.</summary>
    public required string Task { get; init; }

    /// <summary>Actions taken.</summary>
    public IReadOnlyList<string> Actions { get; init; } = Array.Empty<string>();

    /// <summary>Outcome achieved.</summary>
    public required string Outcome { get; init; }

    /// <summary>Whether the task succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>External feedback if any.</summary>
    public string? ExternalFeedback { get; init; }

    /// <summary>Ground truth if available.</summary>
    public string? GroundTruth { get; init; }
}

/// <summary>
/// Configuration for Reflexion.
/// </summary>
public class ReflexionConfiguration
{
    /// <summary>Maximum reflection iterations.</summary>
    public int MaxIterations { get; init; } = 3;

    /// <summary>Whether to use long-term memory.</summary>
    public bool UseLongTermMemory { get; init; } = true;

    /// <summary>Maximum memories to retrieve.</summary>
    public int MaxMemories { get; init; } = 5;

    /// <summary>Reflection types to perform.</summary>
    public IReadOnlyList<ReflectionType> ReflectionTypes { get; init; } = new[]
    {
        ReflectionType.SelfEvaluation,
        ReflectionType.StrategyRefinement
    };

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }
}

/// <summary>
/// Result of Reflexion reasoning.
/// </summary>
public class ReflexionResult
{
    /// <summary>Whether the task succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Final answer or output.</summary>
    public string? Answer { get; init; }

    /// <summary>Episodes (attempts) made.</summary>
    public IReadOnlyList<ReflexionEpisode> Episodes { get; init; } = Array.Empty<ReflexionEpisode>();

    /// <summary>Reflections generated.</summary>
    public IReadOnlyList<ReflectionEntry> Reflections { get; init; } = Array.Empty<ReflectionEntry>();

    /// <summary>Iterations used.</summary>
    public int IterationsUsed { get; init; }

    /// <summary>Final performance score.</summary>
    public double FinalScore { get; init; }

    /// <summary>Score improvement from first to last attempt.</summary>
    public double ScoreImprovement { get; init; }

    /// <summary>Memories stored for future use.</summary>
    public IReadOnlyList<string> MemoriesStored { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for Reflexion (self-reflection and improvement).
/// Implements the "Reflexion" pattern.
/// </summary>
/// <remarks>
/// This port implements the Reflexion paradigm where agents improve
/// through self-reflection on past episodes, storing insights in
/// long-term memory for future task attempts.
/// </remarks>
public interface IReflexionPort
{
    /// <summary>
    /// Runs a task with Reflexion.
    /// </summary>
    /// <param name="task">The task to complete.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<ReflexionResult> RunWithReflexionAsync(
        string task,
        ReflexionConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a reflection on an episode.
    /// </summary>
    /// <param name="episode">The episode to reflect on.</param>
    /// <param name="reflectionTypes">Types of reflection to perform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Reflection entries.</returns>
    Task<IReadOnlyList<ReflectionEntry>> ReflectAsync(
        ReflexionEpisode episode,
        IEnumerable<ReflectionType> reflectionTypes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves relevant memories for a task.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="maxMemories">Maximum memories to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Relevant reflection entries.</returns>
    Task<IReadOnlyList<ReflectionEntry>> RetrieveMemoriesAsync(
        string task,
        int maxMemories = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a reflection in long-term memory.
    /// </summary>
    /// <param name="reflection">The reflection to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task StoreMemoryAsync(
        ReflectionEntry reflection,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates an episode's success.
    /// </summary>
    /// <param name="episode">The episode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Score (0.0 - 1.0) and evaluation.</returns>
    Task<(double Score, string Evaluation)> EvaluateEpisodeAsync(
        ReflexionEpisode episode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an improved strategy from reflections.
    /// </summary>
    /// <param name="task">The task.</param>
    /// <param name="previousReflections">Previous reflections.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Improved strategy.</returns>
    Task<string> GenerateImprovedStrategyAsync(
        string task,
        IReadOnlyList<ReflectionEntry> previousReflections,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears memories for a task type.
    /// </summary>
    /// <param name="taskPattern">Task pattern to match.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of memories cleared.</returns>
    Task<int> ClearMemoriesAsync(
        string taskPattern,
        CancellationToken cancellationToken = default);
}
