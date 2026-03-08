namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A web search query.
/// </summary>
public class WebSearchQuery
{
    /// <summary>Query identifier.</summary>
    public string QueryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Search query.</summary>
    public required string Query { get; init; }

    /// <summary>Maximum results.</summary>
    public int MaxResults { get; init; } = 10;

    /// <summary>Search filters.</summary>
    public SearchFilters? Filters { get; init; }

    /// <summary>Whether to include snippets.</summary>
    public bool IncludeSnippets { get; init; } = true;

    /// <summary>Whether to fetch full content.</summary>
    public bool FetchContent { get; init; }

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }
}

/// <summary>
/// Search filters.
/// </summary>
public class SearchFilters
{
    /// <summary>Time range.</summary>
    public SearchTimeRange TimeRange { get; init; } = SearchTimeRange.Any;

    /// <summary>Allowed domains.</summary>
    public IReadOnlyList<string> AllowedDomains { get; init; } = Array.Empty<string>();

    /// <summary>Blocked domains.</summary>
    public IReadOnlyList<string> BlockedDomains { get; init; } = Array.Empty<string>();

    /// <summary>Content type filter.</summary>
    public string? ContentType { get; init; }

    /// <summary>Language.</summary>
    public string? Language { get; init; }

    /// <summary>Region.</summary>
    public string? Region { get; init; }
}

/// <summary>
/// Search time range.
/// </summary>
public enum SearchTimeRange
{
    /// <summary>Any.</summary>
    Any,
    /// <summary>PastDay.</summary>
    PastDay,
    /// <summary>PastWeek.</summary>
    PastWeek,
    /// <summary>PastMonth.</summary>
    PastMonth,
    /// <summary>PastYear.</summary>
    PastYear
}

/// <summary>
/// A search result.
/// </summary>
public class WebSearchResult
{
    /// <summary>Result URL.</summary>
    public required string Url { get; init; }

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Snippet.</summary>
    public string? Snippet { get; init; }

    /// <summary>Full content (if fetched).</summary>
    public string? Content { get; init; }

    /// <summary>Domain.</summary>
    public required string Domain { get; init; }

    /// <summary>Publish date.</summary>
    public DateTimeOffset? PublishDate { get; init; }

    /// <summary>Relevance score.</summary>
    public double Relevance { get; init; }

    /// <summary>Position in results.</summary>
    public int Position { get; init; }
}

/// <summary>
/// Search session results.
/// </summary>
public class SearchSession
{
    /// <summary>Session identifier.</summary>
    public required string SessionId { get; init; }

    /// <summary>Queries in session.</summary>
    public IReadOnlyList<WebSearchQuery> Queries { get; init; } = Array.Empty<WebSearchQuery>();

    /// <summary>All results.</summary>
    public IReadOnlyList<WebSearchResult> Results { get; init; } = Array.Empty<WebSearchResult>();

    /// <summary>Synthesized answer.</summary>
    public string? SynthesizedAnswer { get; init; }

    /// <summary>Sources used.</summary>
    public IReadOnlyList<string> Sources { get; init; } = Array.Empty<string>();

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Research task.
/// </summary>
public class ResearchTask
{
    /// <summary>Task identifier.</summary>
    public string TaskId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Research question.</summary>
    public required string Question { get; init; }

    /// <summary>Depth of research.</summary>
    public ResearchDepth Depth { get; init; } = ResearchDepth.Standard;

    /// <summary>Required sources.</summary>
    public int RequiredSources { get; init; } = 5;

    /// <summary>Whether to verify facts.</summary>
    public bool VerifyFacts { get; init; } = true;
}

/// <summary>
/// Research depth.
/// </summary>
public enum ResearchDepth
{
    /// <summary>Quick.</summary>
    Quick,
    /// <summary>Standard.</summary>
    Standard,
    /// <summary>Deep.</summary>
    Deep,
    /// <summary>Comprehensive.</summary>
    Comprehensive
}

/// <summary>
/// Research result.
/// </summary>
public class ResearchResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Answer.</summary>
    public required string Answer { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Sources.</summary>
    public IReadOnlyList<WebSearchResult> Sources { get; init; } = Array.Empty<WebSearchResult>();

    /// <summary>Key findings.</summary>
    public IReadOnlyList<string> KeyFindings { get; init; } = Array.Empty<string>();

    /// <summary>Queries performed.</summary>
    public int QueriesPerformed { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for AI web search agent loop.
/// Implements the "AI Web Search Agent Loop" pattern.
/// </summary>
public interface IWebSearchAgentPort
{
    /// <summary>
    /// Performs a web search.
    /// </summary>
    Task<IReadOnlyList<WebSearchResult>> SearchAsync(
        WebSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs research on a question.
    /// </summary>
    Task<ResearchResult> ResearchAsync(
        ResearchTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches and processes a web page.
    /// </summary>
    Task<string> FetchPageContentAsync(
        string url,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Synthesizes an answer from search results.
    /// </summary>
    Task<string> SynthesizeAnswerAsync(
        string question,
        IEnumerable<WebSearchResult> results,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a search session for iterative research.
    /// </summary>
    Task<SearchSession> CreateSessionAsync(
        string initialQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continues a search session.
    /// </summary>
    Task<SearchSession> ContinueSessionAsync(
        string sessionId,
        string followUpQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets session results.
    /// </summary>
    Task<SearchSession?> GetSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a claim against web sources.
    /// </summary>
    Task<(bool Verified, double Confidence, IReadOnlyList<string> Sources)> VerifyClaimAsync(
        string claim,
        CancellationToken cancellationToken = default);
}
