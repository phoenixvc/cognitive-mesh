namespace MetacognitiveLayer.Protocols.Common.Memory.Ports;

/// <summary>
/// A disclosure level.
/// </summary>
public enum DisclosureLevel
{
    /// <summary>High-level summary only.</summary>
    Summary = 1,
    /// <summary>Key sections with context.</summary>
    KeySections = 2,
    /// <summary>Moderate detail.</summary>
    Moderate = 3,
    /// <summary>Detailed view.</summary>
    Detailed = 4,
    /// <summary>Full content.</summary>
    Full = 5
}

/// <summary>
/// A progressively disclosed document.
/// </summary>
public class DisclosedDocument
{
    /// <summary>Document identifier.</summary>
    public required string DocumentId { get; init; }

    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Current disclosure level.</summary>
    public DisclosureLevel Level { get; init; }

    /// <summary>Disclosed content at current level.</summary>
    public required string Content { get; init; }

    /// <summary>Total document size.</summary>
    public long TotalSize { get; init; }

    /// <summary>Current disclosed size.</summary>
    public long DisclosedSize { get; init; }

    /// <summary>Token count at current level.</summary>
    public int TokenCount { get; init; }

    /// <summary>Sections available.</summary>
    public IReadOnlyList<DocumentSection> Sections { get; init; } = Array.Empty<DocumentSection>();

    /// <summary>Whether more detail is available.</summary>
    public bool HasMoreDetail { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A section of a document.
/// </summary>
public class DocumentSection
{
    /// <summary>Section identifier.</summary>
    public required string SectionId { get; init; }

    /// <summary>Section title.</summary>
    public required string Title { get; init; }

    /// <summary>Start position.</summary>
    public int StartPosition { get; init; }

    /// <summary>End position.</summary>
    public int EndPosition { get; init; }

    /// <summary>Section type (heading, code, text, etc.).</summary>
    public required string SectionType { get; init; }

    /// <summary>Current disclosure level.</summary>
    public DisclosureLevel Level { get; init; }

    /// <summary>Importance score.</summary>
    public double Importance { get; init; }

    /// <summary>Summary at current level.</summary>
    public string? Summary { get; init; }
}

/// <summary>
/// Configuration for progressive disclosure.
/// </summary>
public class DisclosureConfiguration
{
    /// <summary>Initial disclosure level.</summary>
    public DisclosureLevel InitialLevel { get; init; } = DisclosureLevel.Summary;

    /// <summary>Maximum tokens per level.</summary>
    public Dictionary<DisclosureLevel, int> MaxTokensPerLevel { get; init; } = new()
    {
        { DisclosureLevel.Summary, 500 },
        { DisclosureLevel.KeySections, 2000 },
        { DisclosureLevel.Moderate, 5000 },
        { DisclosureLevel.Detailed, 10000 },
        { DisclosureLevel.Full, int.MaxValue }
    };

    /// <summary>Whether to auto-expand on query.</summary>
    public bool AutoExpand { get; init; } = true;

    /// <summary>Sections to always include.</summary>
    public IReadOnlyList<string> AlwaysIncludeSections { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for progressive disclosure of large files.
/// Implements the "Progressive Disclosure for Large Files" pattern.
/// </summary>
public interface IProgressiveDisclosurePort
{
    /// <summary>
    /// Opens a document with progressive disclosure.
    /// </summary>
    Task<DisclosedDocument> OpenDocumentAsync(
        string filePath,
        DisclosureConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands disclosure level.
    /// </summary>
    Task<DisclosedDocument> ExpandAsync(
        string documentId,
        DisclosureLevel targetLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands a specific section.
    /// </summary>
    Task<DisclosedDocument> ExpandSectionAsync(
        string documentId,
        string sectionId,
        DisclosureLevel targetLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Collapses to a lower disclosure level.
    /// </summary>
    Task<DisclosedDocument> CollapseAsync(
        string documentId,
        DisclosureLevel targetLevel,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches within document and expands relevant sections.
    /// </summary>
    Task<DisclosedDocument> SearchAndExpandAsync(
        string documentId,
        string query,
        int maxSections = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets section details.
    /// </summary>
    Task<DocumentSection> GetSectionAsync(
        string documentId,
        string sectionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets document status.
    /// </summary>
    Task<DisclosedDocument?> GetDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes a document.
    /// </summary>
    Task CloseDocumentAsync(
        string documentId,
        CancellationToken cancellationToken = default);
}
