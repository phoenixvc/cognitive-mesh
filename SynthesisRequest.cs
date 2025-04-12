public class SynthesisRequest
{
    public List<DocumentInput> Documents { get; set; } = new List<DocumentInput>();
    public string FocusArea { get; set; }
    public List<string> Elements { get; set; } = new List<string>();
}
