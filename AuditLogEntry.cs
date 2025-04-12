public class AuditLogEntry
{
    public string UserId { get; set; }
    public string Action { get; set; }
    public DateTime Timestamp { get; set; }
    public string Details { get; set; }
}
