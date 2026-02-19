/// <summary>
/// Represents a document in the knowledge management system.
/// </summary>
public class KnowledgeDocument
{
    /// <summary>Gets or sets the unique identifier of the document.</summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    /// <summary>Gets or sets the title of the document.</summary>
    public string Title { get; set; }
    /// <summary>Gets or sets the content of the document.</summary>
    public string Content { get; set; }
    /// <summary>Gets or sets the source of the document.</summary>
    public string Source { get; set; }
    /// <summary>Gets or sets the category of the document.</summary>
    public string Category { get; set; }
    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    /// <summary>Gets or sets the tags associated with the document.</summary>
    public List<string> Tags { get; set; } = new List<string>();
}
