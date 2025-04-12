public class ConversationResponse
{
    public string Response { get; set; }
    public string SentimentAnalysis { get; set; }
    public List<string> SuggestedActions { get; set; } = new List<string>();
}
