namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A webhook trigger configuration.
/// </summary>
public class WebhookTrigger
{
    /// <summary>Trigger identifier.</summary>
    public required string TriggerId { get; init; }

    /// <summary>Trigger name.</summary>
    public required string Name { get; init; }

    /// <summary>Platform (GitHub, Slack, etc.).</summary>
    public required string Platform { get; init; }

    /// <summary>Event type to trigger on.</summary>
    public required string EventType { get; init; }

    /// <summary>Filter conditions (JSON).</summary>
    public string? FilterConditions { get; init; }

    /// <summary>Action to execute.</summary>
    public required TriggerAction Action { get; init; }

    /// <summary>Whether active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Secret for validation.</summary>
    public string? Secret { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Last triggered.</summary>
    public DateTimeOffset? LastTriggered { get; init; }
}

/// <summary>
/// Action to execute when triggered.
/// </summary>
public class TriggerAction
{
    /// <summary>Action type.</summary>
    public TriggerActionType Type { get; init; }

    /// <summary>Agent ID to invoke.</summary>
    public string? AgentId { get; init; }

    /// <summary>Workflow ID to run.</summary>
    public string? WorkflowId { get; init; }

    /// <summary>Prompt template.</summary>
    public string? PromptTemplate { get; init; }

    /// <summary>Additional parameters.</summary>
    public Dictionary<string, string> Parameters { get; init; } = new();
}

/// <summary>
/// Trigger action type.
/// </summary>
public enum TriggerActionType
{
    /// <summary>Invoke an agent.</summary>
    InvokeAgent,
    /// <summary>Run a workflow.</summary>
    RunWorkflow,
    /// <summary>Execute a prompt.</summary>
    ExecutePrompt,
    /// <summary>Send a notification.</summary>
    Notify,
    /// <summary>Custom action.</summary>
    Custom
}

/// <summary>
/// Incoming webhook event.
/// </summary>
public class WebhookEvent
{
    /// <summary>Event identifier.</summary>
    public string EventId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Platform.</summary>
    public required string Platform { get; init; }

    /// <summary>Event type.</summary>
    public required string EventType { get; init; }

    /// <summary>Payload (JSON).</summary>
    public required string Payload { get; init; }

    /// <summary>Headers.</summary>
    public Dictionary<string, string> Headers { get; init; } = new();

    /// <summary>Source IP.</summary>
    public string? SourceIP { get; init; }

    /// <summary>Received at.</summary>
    public DateTimeOffset ReceivedAt { get; init; }
}

/// <summary>
/// Trigger execution result.
/// </summary>
public class TriggerExecutionResult
{
    /// <summary>Execution identifier.</summary>
    public required string ExecutionId { get; init; }

    /// <summary>Trigger identifier.</summary>
    public required string TriggerId { get; init; }

    /// <summary>Event identifier.</summary>
    public required string EventId { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Output.</summary>
    public string? Output { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }

    /// <summary>Executed at.</summary>
    public DateTimeOffset ExecutedAt { get; init; }
}

/// <summary>
/// Port for multi-platform webhook triggers.
/// Implements the "Multi-Platform Webhook Triggers" pattern.
/// </summary>
public interface IWebhookTriggerPort
{
    /// <summary>
    /// Registers a webhook trigger.
    /// </summary>
    Task<WebhookTrigger> RegisterTriggerAsync(
        WebhookTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a trigger.
    /// </summary>
    Task<WebhookTrigger> UpdateTriggerAsync(
        WebhookTrigger trigger,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a trigger.
    /// </summary>
    Task DeleteTriggerAsync(
        string triggerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a trigger.
    /// </summary>
    Task<WebhookTrigger?> GetTriggerAsync(
        string triggerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists triggers.
    /// </summary>
    Task<IReadOnlyList<WebhookTrigger>> ListTriggersAsync(
        string? platform = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes an incoming webhook event.
    /// </summary>
    Task<TriggerExecutionResult> ProcessEventAsync(
        WebhookEvent webhookEvent,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a webhook signature.
    /// </summary>
    Task<bool> ValidateSignatureAsync(
        string platform,
        string signature,
        string payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trigger execution history.
    /// </summary>
    Task<IReadOnlyList<TriggerExecutionResult>> GetExecutionHistoryAsync(
        string triggerId,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets webhook endpoint URL for a platform.
    /// </summary>
    Task<string> GetEndpointUrlAsync(
        string platform,
        CancellationToken cancellationToken = default);
}
