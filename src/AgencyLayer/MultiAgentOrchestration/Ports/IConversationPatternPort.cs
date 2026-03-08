namespace AgencyLayer.MultiAgentOrchestration.Ports;

/// <summary>
/// Type of multi-agent conversation pattern.
/// </summary>
public enum ConversationPattern
{
    /// <summary>Two-agent conversation.</summary>
    TwoAgent,
    /// <summary>Sequential conversation through agents.</summary>
    Sequential,
    /// <summary>Group chat with multiple agents.</summary>
    GroupChat,
    /// <summary>Hierarchical delegation.</summary>
    Hierarchical,
    /// <summary>Hub-and-spoke pattern.</summary>
    HubAndSpoke,
    /// <summary>Nested conversation within conversation.</summary>
    Nested
}

/// <summary>
/// Role of an agent in a conversation.
/// </summary>
public enum ConversationRole
{
    /// <summary>Initiates and leads the conversation.</summary>
    Initiator,
    /// <summary>Responds to the initiator.</summary>
    Responder,
    /// <summary>Mediates between agents.</summary>
    Mediator,
    /// <summary>Observes without participating.</summary>
    Observer,
    /// <summary>Provides expertise when needed.</summary>
    Expert,
    /// <summary>Summarizes or synthesizes.</summary>
    Synthesizer
}

/// <summary>
/// A participant in a conversation.
/// </summary>
public class ConversationParticipant
{
    /// <summary>Agent identifier.</summary>
    public required string AgentId { get; init; }

    /// <summary>Role in the conversation.</summary>
    public ConversationRole Role { get; init; } = ConversationRole.Responder;

    /// <summary>System prompt for this agent.</summary>
    public string? SystemPrompt { get; init; }

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }

    /// <summary>Maximum response tokens.</summary>
    public int? MaxTokens { get; init; }

    /// <summary>Available tools.</summary>
    public IReadOnlyList<string> Tools { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A message in the conversation.
/// </summary>
public class ConversationMessage
{
    /// <summary>Message identifier.</summary>
    public string MessageId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Sender agent ID.</summary>
    public required string SenderId { get; init; }

    /// <summary>Recipient agent ID (null = broadcast).</summary>
    public string? RecipientId { get; init; }

    /// <summary>Message content.</summary>
    public required string Content { get; init; }

    /// <summary>When the message was sent.</summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>Whether this is a tool call.</summary>
    public bool IsToolCall { get; init; }

    /// <summary>Tool call details if applicable.</summary>
    public string? ToolCall { get; init; }

    /// <summary>Tool result if applicable.</summary>
    public string? ToolResult { get; init; }
}

/// <summary>
/// Configuration for a conversation.
/// </summary>
public class ConversationConfiguration
{
    /// <summary>Conversation pattern to use.</summary>
    public ConversationPattern Pattern { get; init; } = ConversationPattern.TwoAgent;

    /// <summary>Maximum turns in the conversation.</summary>
    public int MaxTurns { get; init; } = 10;

    /// <summary>Maximum tokens across all messages.</summary>
    public int? MaxTotalTokens { get; init; }

    /// <summary>Termination conditions.</summary>
    public IReadOnlyList<string> TerminationConditions { get; init; } = Array.Empty<string>();

    /// <summary>Whether to allow nested conversations.</summary>
    public bool AllowNesting { get; init; } = true;

    /// <summary>Maximum nesting depth.</summary>
    public int MaxNestingDepth { get; init; } = 3;

    /// <summary>Whether to record the conversation.</summary>
    public bool RecordConversation { get; init; } = true;
}

/// <summary>
/// State of a conversation.
/// </summary>
public class ConversationState
{
    /// <summary>Conversation identifier.</summary>
    public required string ConversationId { get; init; }

    /// <summary>Pattern being used.</summary>
    public ConversationPattern Pattern { get; init; }

    /// <summary>Participants.</summary>
    public IReadOnlyList<ConversationParticipant> Participants { get; init; } = Array.Empty<ConversationParticipant>();

    /// <summary>Messages so far.</summary>
    public IReadOnlyList<ConversationMessage> Messages { get; init; } = Array.Empty<ConversationMessage>();

    /// <summary>Current turn number.</summary>
    public int CurrentTurn { get; init; }

    /// <summary>Next speaker (if deterministic).</summary>
    public string? NextSpeaker { get; init; }

    /// <summary>Whether the conversation is complete.</summary>
    public bool IsComplete { get; init; }

    /// <summary>Completion reason.</summary>
    public string? CompletionReason { get; init; }

    /// <summary>When started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When ended (if complete).</summary>
    public DateTimeOffset? EndedAt { get; init; }
}

/// <summary>
/// Result of a conversation.
/// </summary>
public class ConversationResult
{
    /// <summary>The final conversation state.</summary>
    public required ConversationState State { get; init; }

    /// <summary>Final synthesis or conclusion.</summary>
    public string? Synthesis { get; init; }

    /// <summary>Decisions made during conversation.</summary>
    public IReadOnlyList<string> Decisions { get; init; } = Array.Empty<string>();

    /// <summary>Actions to take.</summary>
    public IReadOnlyList<string> Actions { get; init; } = Array.Empty<string>();

    /// <summary>Consensus reached (if applicable).</summary>
    public bool? ConsensusReached { get; init; }

    /// <summary>Total tokens used.</summary>
    public int TotalTokens { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for multi-agent conversation patterns.
/// Implements various conversation patterns from AutoGen/CrewAI.
/// </summary>
/// <remarks>
/// This port provides implementations of common multi-agent conversation
/// patterns including two-agent, sequential, group chat, and hierarchical
/// conversations with configurable termination and speaker selection.
/// </remarks>
public interface IConversationPatternPort
{
    /// <summary>
    /// Starts a new conversation.
    /// </summary>
    /// <param name="participants">Conversation participants.</param>
    /// <param name="initialMessage">Initial message to start.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversation state.</returns>
    Task<ConversationState> StartConversationAsync(
        IReadOnlyList<ConversationParticipant> participants,
        string initialMessage,
        ConversationConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a conversation to completion.
    /// </summary>
    /// <param name="participants">Conversation participants.</param>
    /// <param name="initialMessage">Initial message.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conversation result.</returns>
    Task<ConversationResult> RunConversationAsync(
        IReadOnlyList<ConversationParticipant> participants,
        string initialMessage,
        ConversationConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a message to a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="message">The message to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated state.</returns>
    Task<ConversationState> AddMessageAsync(
        string conversationId,
        ConversationMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next speaker in the conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The next speaker agent ID.</returns>
    Task<string> GetNextSpeakerAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a response from an agent.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="agentId">The agent to respond.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The generated message.</returns>
    Task<ConversationMessage> GenerateResponseAsync(
        string conversationId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets conversation state.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The state.</returns>
    Task<ConversationState?> GetStateAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends a conversation.
    /// </summary>
    /// <param name="conversationId">The conversation ID.</param>
    /// <param name="reason">Reason for ending.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final result.</returns>
    Task<ConversationResult> EndConversationAsync(
        string conversationId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a nested conversation.
    /// </summary>
    /// <param name="parentConversationId">Parent conversation ID.</param>
    /// <param name="participants">Nested conversation participants.</param>
    /// <param name="goal">Goal of the nested conversation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Nested conversation state.</returns>
    Task<ConversationState> CreateNestedConversationAsync(
        string parentConversationId,
        IReadOnlyList<ConversationParticipant> participants,
        string goal,
        CancellationToken cancellationToken = default);
}
