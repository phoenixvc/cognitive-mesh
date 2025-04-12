public class FeedbackRecord
{
    public string Id { get; set; }
    public string QueryId { get; set; }
    public string Type { get; set; }
    public int Rating { get; set; }
    public string Comments { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}
