public class ConversationRequest
{
    public string CustomerId { get; set; }
    public List<ConversationMessage> ConversationHistory { get; set; } = new List<ConversationMessage>();
    public string CurrentMessage { get; set; }
}
