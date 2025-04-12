public class ConversationMessage
{
    public string Role { get; set; } // "Customer" or "Agent"
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
