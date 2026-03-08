namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// A code context window.
/// </summary>
public class CodeContext
{
    /// <summary>Context identifier.</summary>
    public string ContextId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Files included in context.</summary>
    public IReadOnlyList<CodeFile> Files { get; init; } = Array.Empty<CodeFile>();

    /// <summary>Total tokens used.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Maximum tokens available.</summary>
    public int MaxTokens { get; init; }

    /// <summary>Relevance scores per file.</summary>
    public Dictionary<string, double> RelevanceScores { get; init; } = new();

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// A code file in context.
/// </summary>
public class CodeFile
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>File content.</summary>
    public required string Content { get; init; }

    /// <summary>Content type (e.g., csharp, typescript).</summary>
    public required string ContentType { get; init; }

    /// <summary>Whether content is truncated.</summary>
    public bool IsTruncated { get; init; }

    /// <summary>Original line count.</summary>
    public int OriginalLineCount { get; init; }

    /// <summary>Included line count.</summary>
    public int IncludedLineCount { get; init; }

    /// <summary>Token count.</summary>
    public int TokenCount { get; init; }

    /// <summary>Relevance score.</summary>
    public double Relevance { get; init; }

    /// <summary>Symbols extracted.</summary>
    public IReadOnlyList<CodeSymbol> Symbols { get; init; } = Array.Empty<CodeSymbol>();
}

/// <summary>
/// A code symbol (class, method, etc.).
/// </summary>
public class CodeSymbol
{
    /// <summary>Symbol name.</summary>
    public required string Name { get; init; }

    /// <summary>Symbol kind (class, method, property, etc.).</summary>
    public required string Kind { get; init; }

    /// <summary>Start line.</summary>
    public int StartLine { get; init; }

    /// <summary>End line.</summary>
    public int EndLine { get; init; }

    /// <summary>Signature.</summary>
    public string? Signature { get; init; }

    /// <summary>Documentation.</summary>
    public string? Documentation { get; init; }
}

/// <summary>
/// Configuration for code context curation.
/// </summary>
public class CodeContextConfiguration
{
    /// <summary>Maximum tokens for context.</summary>
    public int MaxTokens { get; init; } = 8000;

    /// <summary>Maximum files.</summary>
    public int MaxFiles { get; init; } = 20;

    /// <summary>Minimum relevance score.</summary>
    public double MinRelevance { get; init; } = 0.3;

    /// <summary>Whether to include symbols only (no full content).</summary>
    public bool SymbolsOnly { get; init; }

    /// <summary>Whether to include documentation.</summary>
    public bool IncludeDocumentation { get; init; } = true;

    /// <summary>File patterns to include.</summary>
    public IReadOnlyList<string> IncludePatterns { get; init; } = Array.Empty<string>();

    /// <summary>File patterns to exclude.</summary>
    public IReadOnlyList<string> ExcludePatterns { get; init; } = Array.Empty<string>();

    /// <summary>Priority symbols to always include.</summary>
    public IReadOnlyList<string> PrioritySymbols { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for curated code context window.
/// Implements the "Curated Code Context Window" pattern.
/// </summary>
public interface ICodeContextPort
{
    /// <summary>
    /// Builds a code context for a query.
    /// </summary>
    Task<CodeContext> BuildContextAsync(
        string query,
        string repositoryPath,
        CodeContextConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds context around specific files.
    /// </summary>
    Task<CodeContext> BuildContextForFilesAsync(
        IEnumerable<string> filePaths,
        string? query = null,
        CodeContextConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds context around specific symbols.
    /// </summary>
    Task<CodeContext> BuildContextForSymbolsAsync(
        IEnumerable<string> symbolNames,
        string repositoryPath,
        CodeContextConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands context with related code.
    /// </summary>
    Task<CodeContext> ExpandContextAsync(
        string contextId,
        int additionalTokens,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets symbols from a file.
    /// </summary>
    Task<IReadOnlyList<CodeSymbol>> GetSymbolsAsync(
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ranks files by relevance to a query.
    /// </summary>
    Task<IReadOnlyList<(string FilePath, double Relevance)>> RankFilesAsync(
        string query,
        IEnumerable<string> filePaths,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a saved context.
    /// </summary>
    Task<CodeContext?> GetContextAsync(
        string contextId,
        CancellationToken cancellationToken = default);
}
