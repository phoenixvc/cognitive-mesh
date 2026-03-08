namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Configuration for best-of-N sampling.
/// </summary>
public class BestOfNConfiguration
{
    /// <summary>Number of candidates to generate.</summary>
    public int N { get; init; } = 3;

    /// <summary>Selection strategy.</summary>
    public BestOfNStrategy Strategy { get; init; } = BestOfNStrategy.LLMJudge;

    /// <summary>Scoring prompt for LLM judge.</summary>
    public string? ScoringPrompt { get; init; }

    /// <summary>Criteria for evaluation.</summary>
    public IReadOnlyList<string> Criteria { get; init; } = Array.Empty<string>();

    /// <summary>Whether to run candidates in parallel.</summary>
    public bool Parallel { get; init; } = true;

    /// <summary>Temperature for generation.</summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>Model for generation.</summary>
    public string? GenerationModel { get; init; }

    /// <summary>Model for judging.</summary>
    public string? JudgeModel { get; init; }

    /// <summary>Whether to include reasoning.</summary>
    public bool IncludeReasoning { get; init; } = true;
}

/// <summary>
/// Strategy for selecting best candidate.
/// </summary>
public enum BestOfNStrategy
{
    /// <summary>Use LLM as judge.</summary>
    LLMJudge,
    /// <summary>Use embedding similarity.</summary>
    EmbeddingSimilarity,
    /// <summary>Use consensus voting.</summary>
    ConsensusVoting,
    /// <summary>Use length-normalized scoring.</summary>
    LengthNormalized,
    /// <summary>Use custom scorer.</summary>
    CustomScorer
}

/// <summary>
/// A candidate response.
/// </summary>
public class Candidate
{
    /// <summary>Candidate identifier.</summary>
    public string CandidateId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The response content.</summary>
    public required string Content { get; init; }

    /// <summary>Score assigned.</summary>
    public double Score { get; init; }

    /// <summary>Ranking (1 = best).</summary>
    public int Rank { get; init; }

    /// <summary>Reasoning for score.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Per-criteria scores.</summary>
    public Dictionary<string, double> CriteriaScores { get; init; } = new();

    /// <summary>Generation metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Result of best-of-N selection.
/// </summary>
public class BestOfNResult
{
    /// <summary>Selected best candidate.</summary>
    public required Candidate Best { get; init; }

    /// <summary>All candidates with scores.</summary>
    public IReadOnlyList<Candidate> AllCandidates { get; init; } = Array.Empty<Candidate>();

    /// <summary>Selection reasoning.</summary>
    public string? SelectionReasoning { get; init; }

    /// <summary>Confidence in selection.</summary>
    public double Confidence { get; init; }

    /// <summary>Total generation time.</summary>
    public TimeSpan GenerationTime { get; init; }

    /// <summary>Total judging time.</summary>
    public TimeSpan JudgingTime { get; init; }
}

/// <summary>
/// Port for recursive best-of-N delegation.
/// Implements the "Recursive Best-of-N Delegation" pattern for quality improvement.
/// </summary>
public interface IBestOfNPort
{
    /// <summary>
    /// Generates N candidates and selects the best.
    /// </summary>
    Task<BestOfNResult> SelectBestAsync(
        string prompt,
        BestOfNConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Recursively improves a response through multiple rounds.
    /// </summary>
    Task<BestOfNResult> RecursiveImproveAsync(
        string initialPrompt,
        int rounds = 3,
        BestOfNConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scores a set of candidates.
    /// </summary>
    Task<IReadOnlyList<Candidate>> ScoreCandidatesAsync(
        IEnumerable<string> candidates,
        string prompt,
        BestOfNConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a custom scorer.
    /// </summary>
    void RegisterScorer(
        string scorerId,
        Func<string, string, CancellationToken, Task<double>> scorer);
}
