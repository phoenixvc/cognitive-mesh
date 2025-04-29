public class KnowledgeDocument
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Content { get; set; }
    public string Source { get; set; }
    public string Category { get; set; }
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;
    public List<string> Tags { get; set; } = new List<string>();
}
