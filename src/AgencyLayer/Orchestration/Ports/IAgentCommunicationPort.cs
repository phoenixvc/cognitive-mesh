namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Type of inter-agent message.
/// </summary>
public enum AgentMessageType
{
    /// <summary>Request for action.</summary>
    Request,
    /// <summary>Response to request.</summary>
    Response,
    /// <summary>Broadcast notification.</summary>
    Broadcast,
    /// <summary>Delegation of task.</summary>
    Delegation,
    /// <summary>Status update.</summary>
    Status,
    /// <summary>Error notification.</summary>
    Error,
    /// <summary>Acknowledgment.</summary>
    Ack
}

/// <summary>
/// A message between agents.
/// </summary>
public class AgentMessage
{
    /// <summary>Message identifier.</summary>
    public string MessageId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Message type.</summary>
    public AgentMessageType Type { get; init; }

    /// <summary>Sender agent ID.</summary>
    public required string SenderId { get; init; }

    /// <summary>Recipient agent ID (null for broadcast).</summary>
    public string? RecipientId { get; init; }

    /// <summary>Conversation/thread ID.</summary>
    public string? ConversationId { get; init; }

    /// <summary>In-reply-to message ID.</summary>
    public string? InReplyTo { get; init; }

    /// <summary>Message content.</summary>
    public required string Content { get; init; }

    /// <summary>Structured payload.</summary>
    public Dictionary<string, object> Payload { get; init; } = new();

    /// <summary>Priority.</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Time-to-live.</summary>
    public TimeSpan? TTL { get; init; }

    /// <summary>When sent.</summary>
    public DateTimeOffset SentAt { get; init; }

    /// <summary>Requires acknowledgment.</summary>
    public bool RequiresAck { get; init; }
}

/// <summary>
/// A subscription to agent messages.
/// </summary>
public class AgentSubscription
{
    /// <summary>Subscription identifier.</summary>
    public required string SubscriptionId { get; init; }

    /// <summary>Subscribing agent ID.</summary>
    public required string AgentId { get; init; }

    /// <summary>Topic pattern to subscribe to.</summary>
    public required string TopicPattern { get; init; }

    /// <summary>Message types to receive.</summary>
    public IReadOnlyList<AgentMessageType> MessageTypes { get; init; } = Array.Empty<AgentMessageType>();

    /// <summary>When created.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Result of sending a message.
/// </summary>
public class SendResult
{
    /// <summary>Message ID.</summary>
    public required string MessageId { get; init; }

    /// <summary>Whether send succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>Acknowledgment if received.</summary>
    public AgentMessage? Ack { get; init; }

    /// <summary>Error if failed.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Port for agent-to-agent communication.
/// Implements the "Agent-to-Agent Communication" pattern.
/// </summary>
public interface IAgentCommunicationPort
{
    /// <summary>
    /// Sends a message to another agent.
    /// </summary>
    Task<SendResult> SendAsync(
        AgentMessage message,
        bool waitForAck = false,
        TimeSpan? ackTimeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request and waits for response.
    /// </summary>
    Task<AgentMessage> RequestAsync(
        string recipientId,
        string content,
        Dictionary<string, object>? payload = null,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Broadcasts a message to multiple agents.
    /// </summary>
    Task BroadcastAsync(
        string topic,
        string content,
        Dictionary<string, object>? payload = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to messages.
    /// </summary>
    Task<string> SubscribeAsync(
        string agentId,
        string topicPattern,
        IEnumerable<AgentMessageType>? messageTypes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unsubscribes from messages.
    /// </summary>
    Task UnsubscribeAsync(
        string subscriptionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Receives messages for an agent.
    /// </summary>
    IAsyncEnumerable<AgentMessage> ReceiveAsync(
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending messages for an agent.
    /// </summary>
    Task<IReadOnlyList<AgentMessage>> GetPendingMessagesAsync(
        string agentId,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges a message.
    /// </summary>
    Task AcknowledgeAsync(
        string messageId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversation history.
    /// </summary>
    Task<IReadOnlyList<AgentMessage>> GetConversationAsync(
        string conversationId,
        int limit = 100,
        CancellationToken cancellationToken = default);
}
