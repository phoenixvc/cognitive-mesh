public class ContentCreationRequest
{
    public string Topic { get; set; }
    public string ContentType { get; set; } // Blog post, white paper, email, etc.
    public string TargetAudience { get; set; }
    public string Style { get; set; }
    public string Length { get; set; } // Short, medium, long
}
