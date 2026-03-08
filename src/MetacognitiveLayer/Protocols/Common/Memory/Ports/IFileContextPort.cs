namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// A file context window.
/// </summary>
public class FileContext
{
    /// <summary>Context identifier.</summary>
    public string ContextId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Files in context.</summary>
    public IReadOnlyList<ContextFile> Files { get; init; } = Array.Empty<ContextFile>();

    /// <summary>Total tokens.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Maximum tokens.</summary>
    public int MaxTokens { get; init; }

    /// <summary>Session identifier.</summary>
    public string? SessionId { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// A file in context.
/// </summary>
public class ContextFile
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>File content.</summary>
    public required string Content { get; init; }

    /// <summary>Content type.</summary>
    public required string ContentType { get; init; }

    /// <summary>File size in bytes.</summary>
    public long FileSize { get; init; }

    /// <summary>Token count.</summary>
    public int TokenCount { get; init; }

    /// <summary>Whether truncated.</summary>
    public bool IsTruncated { get; init; }

    /// <summary>Included section (if partial).</summary>
    public FileSection? IncludedSection { get; init; }

    /// <summary>Relevance score.</summary>
    public double Relevance { get; init; }

    /// <summary>Last modified.</summary>
    public DateTimeOffset LastModified { get; init; }
}

/// <summary>
/// A section of a file.
/// </summary>
public class FileSection
{
    /// <summary>Start line (1-based).</summary>
    public int StartLine { get; init; }

    /// <summary>End line (1-based).</summary>
    public int EndLine { get; init; }

    /// <summary>Start byte offset.</summary>
    public long StartOffset { get; init; }

    /// <summary>End byte offset.</summary>
    public long EndOffset { get; init; }

    /// <summary>Reason for this section.</summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Configuration for file context.
/// </summary>
public class FileContextConfiguration
{
    /// <summary>Maximum tokens.</summary>
    public int MaxTokens { get; init; } = 8000;

    /// <summary>Maximum files.</summary>
    public int MaxFiles { get; init; } = 10;

    /// <summary>Maximum file size to include fully.</summary>
    public long MaxFullFileSize { get; init; } = 50_000;

    /// <summary>Minimum relevance.</summary>
    public double MinRelevance { get; init; } = 0.2;

    /// <summary>Whether to include binary files.</summary>
    public bool IncludeBinary { get; init; }

    /// <summary>File extensions to include.</summary>
    public IReadOnlyList<string> IncludeExtensions { get; init; } = Array.Empty<string>();

    /// <summary>File extensions to exclude.</summary>
    public IReadOnlyList<string> ExcludeExtensions { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for curated file context window.
/// Implements the "Curated File Context Window" pattern.
/// </summary>
public interface IFileContextPort
{
    /// <summary>
    /// Builds file context for a query.
    /// </summary>
    Task<FileContext> BuildContextAsync(
        string query,
        string directoryPath,
        FileContextConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds context for specific files.
    /// </summary>
    Task<FileContext> BuildContextForFilesAsync(
        IEnumerable<string> filePaths,
        FileContextConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a file to existing context.
    /// </summary>
    Task<FileContext> AddFileToContextAsync(
        string contextId,
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a file from context.
    /// </summary>
    Task<FileContext> RemoveFileFromContextAsync(
        string contextId,
        string filePath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific section of a file.
    /// </summary>
    Task<ContextFile> GetFileSectionAsync(
        string filePath,
        int startLine,
        int endLine,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for relevant sections in a file.
    /// </summary>
    Task<IReadOnlyList<FileSection>> FindRelevantSectionsAsync(
        string filePath,
        string query,
        int maxSections = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets saved context.
    /// </summary>
    Task<FileContext?> GetContextAsync(
        string contextId,
        CancellationToken cancellationToken = default);
}
