namespace AgencyLayer.Agents.Ports;

/// <summary>
/// Type of codebase query.
/// </summary>
public enum CodebaseQueryType
{
    /// <summary>Find where something is defined.</summary>
    FindDefinition,
    /// <summary>Find usages of something.</summary>
    FindUsages,
    /// <summary>Explain how something works.</summary>
    Explain,
    /// <summary>Answer a question about the code.</summary>
    Question,
    /// <summary>Summarize a component or module.</summary>
    Summarize,
    /// <summary>Find related code.</summary>
    FindRelated,
    /// <summary>Trace a call path.</summary>
    TraceCallPath,
    /// <summary>Find patterns in the code.</summary>
    FindPatterns
}

/// <summary>
/// A query about the codebase.
/// </summary>
public class CodebaseQuery
{
    /// <summary>Unique identifier.</summary>
    public string QueryId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The natural language query.</summary>
    public required string Query { get; init; }

    /// <summary>Type of query (if specified).</summary>
    public CodebaseQueryType? QueryType { get; init; }

    /// <summary>Scope to search (file paths, namespaces, etc.).</summary>
    public IReadOnlyList<string> Scope { get; init; } = Array.Empty<string>();

    /// <summary>Maximum results to return.</summary>
    public int MaxResults { get; init; } = 10;

    /// <summary>Whether to include code snippets.</summary>
    public bool IncludeCodeSnippets { get; init; } = true;

    /// <summary>Whether to explain the results.</summary>
    public bool IncludeExplanations { get; init; } = true;
}

/// <summary>
/// A code location found by the query.
/// </summary>
public class CodeLocation
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Line number.</summary>
    public int LineNumber { get; init; }

    /// <summary>Column number.</summary>
    public int? ColumnNumber { get; init; }

    /// <summary>End line number.</summary>
    public int? EndLineNumber { get; init; }

    /// <summary>Symbol name.</summary>
    public string? SymbolName { get; init; }

    /// <summary>Symbol type (class, method, property, etc.).</summary>
    public string? SymbolType { get; init; }
}

/// <summary>
/// A result from a codebase query.
/// </summary>
public class CodebaseQueryResult
{
    /// <summary>The query ID.</summary>
    public required string QueryId { get; init; }

    /// <summary>Primary answer to the query.</summary>
    public required string Answer { get; init; }

    /// <summary>Confidence in the answer (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Relevant code locations.</summary>
    public IReadOnlyList<CodeLocation> Locations { get; init; } = Array.Empty<CodeLocation>();

    /// <summary>Code snippets.</summary>
    public IReadOnlyList<CodeSnippet> Snippets { get; init; } = Array.Empty<CodeSnippet>();

    /// <summary>Related queries to explore.</summary>
    public IReadOnlyList<string> RelatedQueries { get; init; } = Array.Empty<string>();

    /// <summary>Sources used to answer.</summary>
    public IReadOnlyList<string> Sources { get; init; } = Array.Empty<string>();

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// A code snippet.
/// </summary>
public class CodeSnippet
{
    /// <summary>Location of the snippet.</summary>
    public required CodeLocation Location { get; init; }

    /// <summary>The code content.</summary>
    public required string Code { get; init; }

    /// <summary>Language of the code.</summary>
    public string? Language { get; init; }

    /// <summary>Explanation of the snippet.</summary>
    public string? Explanation { get; init; }

    /// <summary>Relevance score (0.0 - 1.0).</summary>
    public double Relevance { get; init; }
}

/// <summary>
/// Onboarding content for new developers.
/// </summary>
public class OnboardingContent
{
    /// <summary>High-level overview of the codebase.</summary>
    public required string Overview { get; init; }

    /// <summary>Key components and their purposes.</summary>
    public IReadOnlyList<ComponentSummary> KeyComponents { get; init; } = Array.Empty<ComponentSummary>();

    /// <summary>Getting started steps.</summary>
    public IReadOnlyList<string> GettingStarted { get; init; } = Array.Empty<string>();

    /// <summary>Important conventions to follow.</summary>
    public IReadOnlyList<string> Conventions { get; init; } = Array.Empty<string>();

    /// <summary>Key files to understand.</summary>
    public IReadOnlyList<string> KeyFiles { get; init; } = Array.Empty<string>();

    /// <summary>Common tasks and how to do them.</summary>
    public Dictionary<string, string> CommonTasks { get; init; } = new();
}

/// <summary>
/// Summary of a component.
/// </summary>
public class ComponentSummary
{
    /// <summary>Name of the component.</summary>
    public required string Name { get; init; }

    /// <summary>Purpose/description.</summary>
    public required string Description { get; init; }

    /// <summary>Key files.</summary>
    public IReadOnlyList<string> KeyFiles { get; init; } = Array.Empty<string>();

    /// <summary>Dependencies.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for agent-powered codebase QA and onboarding.
/// Implements the "Agent-Powered Codebase QA / Onboarding" pattern.
/// </summary>
/// <remarks>
/// This port provides intelligent codebase exploration and question
/// answering capabilities, helping developers understand large codebases
/// quickly and find relevant code for their tasks.
/// </remarks>
public interface ICodebaseQAPort
{
    /// <summary>
    /// Asks a question about the codebase.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result.</returns>
    Task<CodebaseQueryResult> QueryAsync(
        CodebaseQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates onboarding content for the codebase.
    /// </summary>
    /// <param name="scope">Scope to generate for (null = entire codebase).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Onboarding content.</returns>
    Task<OnboardingContent> GenerateOnboardingAsync(
        IEnumerable<string>? scope = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes the codebase for faster queries.
    /// </summary>
    /// <param name="paths">Paths to index (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of files indexed.</returns>
    Task<int> IndexCodebaseAsync(
        IEnumerable<string>? paths = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds the definition of a symbol.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Location of the definition.</returns>
    Task<CodeLocation?> FindDefinitionAsync(
        string symbol,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds usages of a symbol.
    /// </summary>
    /// <param name="symbol">The symbol name.</param>
    /// <param name="limit">Maximum usages to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Locations of usages.</returns>
    Task<IReadOnlyList<CodeLocation>> FindUsagesAsync(
        string symbol,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Explains a code file or symbol.
    /// </summary>
    /// <param name="target">File path or symbol name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Explanation.</returns>
    Task<string> ExplainAsync(
        string target,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets suggested queries based on context.
    /// </summary>
    /// <param name="context">Current context or file.</param>
    /// <param name="limit">Maximum suggestions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Suggested queries.</returns>
    Task<IReadOnlyList<string>> GetSuggestedQueriesAsync(
        string? context = null,
        int limit = 5,
        CancellationToken cancellationToken = default);
}
