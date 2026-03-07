namespace AgencyLayer.Tools.Ports;

/// <summary>
/// Communication platform types.
/// </summary>
public enum CommunicationPlatform
{
    /// <summary>Email (SMTP).</summary>
    Email,
    /// <summary>Slack.</summary>
    Slack,
    /// <summary>Microsoft Teams.</summary>
    MsTeams,
    /// <summary>Discord.</summary>
    Discord,
    /// <summary>SMS.</summary>
    Sms,
    /// <summary>Webhook.</summary>
    Webhook,
    /// <summary>Push notification.</summary>
    PushNotification
}

/// <summary>
/// Priority of a message.
/// </summary>
public enum MessagePriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// A message to send.
/// </summary>
public class OutgoingMessage
{
    /// <summary>Unique identifier.</summary>
    public string MessageId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Platform to send to.</summary>
    public required CommunicationPlatform Platform { get; init; }

    /// <summary>Recipients (email addresses, channel IDs, user IDs).</summary>
    public required IReadOnlyList<string> Recipients { get; init; }

    /// <summary>Subject or title (for email, cards).</summary>
    public string? Subject { get; init; }

    /// <summary>Message body.</summary>
    public required string Body { get; init; }

    /// <summary>Body format (text, html, markdown).</summary>
    public string Format { get; init; } = "text";

    /// <summary>Priority.</summary>
    public MessagePriority Priority { get; init; } = MessagePriority.Normal;

    /// <summary>Attachments.</summary>
    public IReadOnlyList<MessageAttachment> Attachments { get; init; } = Array.Empty<MessageAttachment>();

    /// <summary>Reply-to address (for email).</summary>
    public string? ReplyTo { get; init; }

    /// <summary>Thread or conversation ID (for replies).</summary>
    public string? ThreadId { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// A message attachment.
/// </summary>
public class MessageAttachment
{
    /// <summary>File name.</summary>
    public required string FileName { get; init; }

    /// <summary>Content type.</summary>
    public required string ContentType { get; init; }

    /// <summary>Content as base64.</summary>
    public string? ContentBase64 { get; init; }

    /// <summary>URL to the content.</summary>
    public string? ContentUrl { get; init; }
}

/// <summary>
/// Result of sending a message.
/// </summary>
public class SendMessageResult
{
    /// <summary>The message ID.</summary>
    public required string MessageId { get; init; }

    /// <summary>Whether send was successful.</summary>
    public required bool Success { get; init; }

    /// <summary>Platform-specific message ID.</summary>
    public string? PlatformMessageId { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>When the message was sent.</summary>
    public DateTimeOffset? SentAt { get; init; }
}

/// <summary>
/// An incoming message from a platform.
/// </summary>
public class IncomingMessage
{
    /// <summary>Platform the message came from.</summary>
    public required CommunicationPlatform Platform { get; init; }

    /// <summary>Platform-specific message ID.</summary>
    public required string PlatformMessageId { get; init; }

    /// <summary>Sender identifier.</summary>
    public required string SenderId { get; init; }

    /// <summary>Sender display name.</summary>
    public string? SenderName { get; init; }

    /// <summary>Channel or conversation ID.</summary>
    public string? ChannelId { get; init; }

    /// <summary>Thread ID.</summary>
    public string? ThreadId { get; init; }

    /// <summary>Message content.</summary>
    public required string Content { get; init; }

    /// <summary>When the message was received.</summary>
    public DateTimeOffset ReceivedAt { get; init; }

    /// <summary>Attachments.</summary>
    public IReadOnlyList<MessageAttachment> Attachments { get; init; } = Array.Empty<MessageAttachment>();

    /// <summary>Metadata from the platform.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Configuration for a communication platform.
/// </summary>
public class PlatformConfiguration
{
    /// <summary>The platform.</summary>
    public required CommunicationPlatform Platform { get; init; }

    /// <summary>Whether the platform is enabled.</summary>
    public bool Enabled { get; init; } = true;

    /// <summary>Credential reference (secret name).</summary>
    public string? CredentialRef { get; init; }

    /// <summary>Platform-specific settings.</summary>
    public Dictionary<string, string> Settings { get; init; } = new();

    /// <summary>Rate limit (messages per minute).</summary>
    public int? RateLimitPerMinute { get; init; }
}

/// <summary>
/// Port for multi-platform communication aggregation.
/// Implements the "Multi-Platform Communication Aggregation" pattern.
/// </summary>
/// <remarks>
/// This port provides a unified interface for sending and receiving
/// messages across multiple communication platforms (Slack, Teams,
/// email, webhooks) with consistent handling.
/// </remarks>
public interface IMultiPlatformCommunicationPort
{
    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The send result.</returns>
    Task<SendMessageResult> SendAsync(
        OutgoingMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a message to multiple platforms.
    /// </summary>
    /// <param name="platforms">Platforms to send to.</param>
    /// <param name="subject">Subject.</param>
    /// <param name="body">Body.</param>
    /// <param name="recipients">Recipients per platform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Results for each platform.</returns>
    Task<IReadOnlyDictionary<CommunicationPlatform, SendMessageResult>> BroadcastAsync(
        IEnumerable<CommunicationPlatform> platforms,
        string subject,
        string body,
        IReadOnlyDictionary<CommunicationPlatform, IReadOnlyList<string>> recipients,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent messages from a platform.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="channelId">Channel or conversation ID.</param>
    /// <param name="since">Start time.</param>
    /// <param name="limit">Maximum messages.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Messages.</returns>
    Task<IReadOnlyList<IncomingMessage>> GetMessagesAsync(
        CommunicationPlatform platform,
        string channelId,
        DateTimeOffset? since = null,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Replies to a message.
    /// </summary>
    /// <param name="originalMessage">The message to reply to.</param>
    /// <param name="replyBody">Reply content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The send result.</returns>
    Task<SendMessageResult> ReplyAsync(
        IncomingMessage originalMessage,
        string replyBody,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets or sets platform configuration.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="configuration">Configuration to set (null = get).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Current configuration.</returns>
    Task<PlatformConfiguration?> ConfigurePlatformAsync(
        CommunicationPlatform platform,
        PlatformConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets available platforms.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Configured platforms.</returns>
    Task<IReadOnlyList<PlatformConfiguration>> GetConfiguredPlatformsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connectivity to a platform.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether the platform is reachable.</returns>
    Task<bool> TestConnectivityAsync(
        CommunicationPlatform platform,
        CancellationToken cancellationToken = default);
}
