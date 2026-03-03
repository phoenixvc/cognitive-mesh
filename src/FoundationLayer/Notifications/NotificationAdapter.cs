using FoundationLayer.EnterpriseConnectors;
using Microsoft.Extensions.Logging;

namespace FoundationLayer.Notifications
{
    /// <summary>
    /// Adapter for sending notifications related to agent activities and events.
    /// Implements circuit breaker pattern and retry logic for resilient notification delivery.
    /// </summary>
    public class NotificationAdapter : INotificationAdapter
    {
        private readonly ILogger<NotificationAdapter> _logger;
        private readonly INotificationDeliveryService _deliveryService;
        private readonly AgentCircuitBreakerPolicy _circuitBreaker;
        private Queue<NotificationMessage> _retryQueue = new Queue<NotificationMessage>();
        private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(1, 1);
        private readonly Timer _retryTimer;
        private bool _processingRetryQueue;

        /// <summary>
        /// Initializes a new instance of the NotificationAdapter class.
        /// </summary>
        /// <param name="deliveryService">The service for delivering notifications</param>
        /// <param name="logger">The logger</param>
        public NotificationAdapter(
            INotificationDeliveryService deliveryService,
            ILogger<NotificationAdapter> logger)
        {
            _deliveryService = deliveryService ?? throw new ArgumentNullException(nameof(deliveryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize circuit breaker with 3 failure threshold, 1s reset timeout, 3 success threshold
            _circuitBreaker = new AgentCircuitBreakerPolicy(failureThreshold: 3, resetTimeoutMs: 1000, successThreshold: 3);
            
            // Initialize retry timer to process queued notifications every 15 seconds
            _retryTimer = new Timer(ProcessRetryQueue, null, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(15));
        }

        /// <inheritdoc />
        public async Task<bool> SendNotificationAsync(NotificationMessage notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            // Set default values if not provided
            notification.NotificationId ??= Guid.NewGuid().ToString();
            notification.Timestamp = notification.Timestamp == default ? DateTimeOffset.UtcNow : notification.Timestamp;
            notification.Priority = notification.Priority == NotificationPriority.Unspecified 
                ? NotificationPriority.Normal 
                : notification.Priority;

            try
            {
                // Use circuit breaker pattern to handle potential outages
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    await _deliveryService.DeliverNotificationAsync(notification);
                    _logger.LogDebug("Successfully sent notification: {NotificationType}, ID: {NotificationId}, Priority: {Priority}", 
                        notification.NotificationType, notification.NotificationId, notification.Priority);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send notification: {NotificationType}, Priority: {Priority}. Queueing for retry.", 
                    notification.NotificationType, notification.Priority);
                
                // Queue the notification for retry
                await EnqueueForRetryAsync(notification);
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<bool> SendAgentRegistrationNotificationAsync(Guid agentId, string agentType, string registeredBy, IEnumerable<string> recipientIds)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "AgentRegistration",
                Priority = NotificationPriority.Normal,
                Title = "New Agent Registered",
                Body = $"A new agent of type '{agentType}' has been registered.",
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "AgentType", agentType },
                    { "RegisteredBy", registeredBy },
                    { "RegistrationTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string>(recipientIds),
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendAgentRetirementNotificationAsync(Guid agentId, string agentType, string retiredBy, string reason, IEnumerable<string> recipientIds)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "AgentRetirement",
                Priority = NotificationPriority.Normal,
                Title = "Agent Retired",
                Body = $"Agent of type '{agentType}' has been retired. Reason: {reason}",
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "AgentType", agentType },
                    { "RetiredBy", retiredBy },
                    { "Reason", reason },
                    { "RetirementTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string>(recipientIds),
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendAuthorityOverrideNotificationAsync(Guid agentId, string overriddenBy, string reason, IEnumerable<string> recipientIds, NotificationPriority priority = NotificationPriority.High)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "AuthorityOverride",
                Priority = priority,
                Title = "Agent Authority Override",
                Body = $"Agent authority has been overridden. Reason: {reason}",
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "OverriddenBy", overriddenBy },
                    { "Reason", reason },
                    { "OverrideTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string>(recipientIds),
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Push, NotificationChannel.Email }
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendConsentRequestNotificationAsync(Guid agentId, string agentType, string consentType, string userId, string actionDescription)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "ConsentRequest",
                Priority = NotificationPriority.High,
                Title = "Agent Consent Required",
                Body = $"Your consent is required for agent '{agentType}' to perform an action: {actionDescription}",
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "AgentType", agentType },
                    { "ConsentType", consentType },
                    { "ActionDescription", actionDescription },
                    { "RequestTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string> { userId },
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Push },
                ActionButtons = new List<NotificationAction>
                {
                    new NotificationAction { ActionId = "approve", Label = "Approve", Type = ActionType.Positive },
                    new NotificationAction { ActionId = "deny", Label = "Deny", Type = ActionType.Negative }
                },
                ExpiresAt = DateTimeOffset.UtcNow.AddHours(24)
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendAgentTaskCompletionNotificationAsync(Guid agentId, string taskId, string userId, bool success, string summary)
        {
            var priority = success ? NotificationPriority.Normal : NotificationPriority.High;
            var title = success ? "Agent Task Completed" : "Agent Task Failed";

            var notification = new NotificationMessage
            {
                NotificationType = success ? "TaskCompletion" : "TaskFailure",
                Priority = priority,
                Title = title,
                Body = summary,
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "TaskId", taskId },
                    { "Success", success },
                    { "CompletionTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string> { userId },
                Channels = new List<NotificationChannel> { NotificationChannel.InApp }
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendAgentErrorNotificationAsync(Guid agentId, string errorCode, string errorMessage, IEnumerable<string> recipientIds, NotificationPriority priority = NotificationPriority.High)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "AgentError",
                Priority = priority,
                Title = $"Agent Error: {errorCode}",
                Body = errorMessage,
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "ErrorCode", errorCode },
                    { "ErrorMessage", errorMessage },
                    { "ErrorTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string>(recipientIds),
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
            };

            return await SendNotificationAsync(notification);
        }

        /// <inheritdoc />
        public async Task<bool> SendAgentDeprecationNotificationAsync(Guid agentId, string agentType, DateTimeOffset sunsetDate, string migrationPath, IEnumerable<string> recipientIds)
        {
            var notification = new NotificationMessage
            {
                NotificationType = "AgentDeprecation",
                Priority = NotificationPriority.Normal,
                Title = "Agent Deprecation Notice",
                Body = $"Agent '{agentType}' has been deprecated and will be retired on {sunsetDate:d}. Migration path: {migrationPath}",
                Data = new Dictionary<string, object>
                {
                    { "AgentId", agentId },
                    { "AgentType", agentType },
                    { "SunsetDate", sunsetDate },
                    { "MigrationPath", migrationPath },
                    { "DeprecationTime", DateTimeOffset.UtcNow }
                },
                RecipientIds = new List<string>(recipientIds),
                Channels = new List<NotificationChannel> { NotificationChannel.InApp, NotificationChannel.Email }
            };

            return await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Enqueues a notification for retry.
        /// </summary>
        /// <param name="notification">The notification to enqueue</param>
        private async Task EnqueueForRetryAsync(NotificationMessage notification)
        {
            await _queueLock.WaitAsync();
            try
            {
                // For high priority notifications, place at the front of the queue
                if (notification.Priority == NotificationPriority.Critical || notification.Priority == NotificationPriority.High)
                {
                    var tempQueue = new Queue<NotificationMessage>();
                    tempQueue.Enqueue(notification);
                    
                    // Add all existing items after the high priority one
                    while (_retryQueue.Count > 0)
                    {
                        tempQueue.Enqueue(_retryQueue.Dequeue());
                    }
                    
                    _retryQueue = tempQueue;
                }
                else
                {
                    _retryQueue.Enqueue(notification);
                }
                
                _logger.LogInformation("Enqueued notification for retry: {NotificationType}, ID: {NotificationId}, Priority: {Priority}. Queue size: {QueueSize}", 
                    notification.NotificationType, notification.NotificationId, notification.Priority, _retryQueue.Count);
            }
            finally
            {
                _queueLock.Release();
            }
        }

        /// <summary>
        /// Processes the retry queue.
        /// </summary>
        private async void ProcessRetryQueue(object? state)
        {
            if (_processingRetryQueue)
            {
                return; // Already processing
            }

            _processingRetryQueue = true;
            try
            {
                await _queueLock.WaitAsync();
                try
                {
                    if (_retryQueue.Count == 0)
                    {
                        return; // Nothing to process
                    }

                    _logger.LogInformation("Processing notification retry queue. Items: {Count}", _retryQueue.Count);
                    
                    // Process up to 50 items at a time
                    int itemsToProcess = Math.Min(_retryQueue.Count, 50);
                    for (int i = 0; i < itemsToProcess; i++)
                    {
                        if (_retryQueue.Count == 0)
                        {
                            break;
                        }

                        var notification = _retryQueue.Dequeue();
                        
                        // Skip expired notifications
                        if (notification.ExpiresAt.HasValue && notification.ExpiresAt.Value < DateTimeOffset.UtcNow)
                        {
                            _logger.LogInformation("Skipping expired notification: {NotificationType}, ID: {NotificationId}", 
                                notification.NotificationType, notification.NotificationId);
                            continue;
                        }

                        try
                        {
                            await _deliveryService.DeliverNotificationAsync(notification);
                            _logger.LogInformation("Successfully retried notification: {NotificationType}, ID: {NotificationId}", 
                                notification.NotificationType, notification.NotificationId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to retry notification: {NotificationType}, ID: {NotificationId}. Re-enqueueing.", 
                                notification.NotificationType, notification.NotificationId);
                            
                            // Increment retry count
                            notification.RetryCount = (notification.RetryCount ?? 0) + 1;
                            
                            // If we've exceeded max retries, drop the notification or move to a dead letter queue
                            if (notification.RetryCount > 10)
                            {
                                _logger.LogError("Dropping notification after 10 failed retries: {NotificationType}, ID: {NotificationId}", 
                                    notification.NotificationType, notification.NotificationId);
                            }
                            else
                            {
                                _retryQueue.Enqueue(notification); // Put it back in the queue
                            }
                        }
                    }
                }
                finally
                {
                    _queueLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification retry queue");
            }
            finally
            {
                _processingRetryQueue = false;
            }
        }

        /// <summary>
        /// Disposes the adapter.
        /// </summary>
        public void Dispose()
        {
            _retryTimer?.Dispose();
            _queueLock?.Dispose();
        }
    }

    /// <summary>
    /// Interface for the notification adapter.
    /// </summary>
    public interface INotificationAdapter : IDisposable
    {
        /// <summary>
        /// Sends a notification.
        /// </summary>
        /// <param name="notification">The notification to send</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendNotificationAsync(NotificationMessage notification);

        /// <summary>
        /// Sends a notification about agent registration.
        /// </summary>
        /// <param name="agentId">The ID of the registered agent</param>
        /// <param name="agentType">The type of the registered agent</param>
        /// <param name="registeredBy">The ID of the user who registered the agent</param>
        /// <param name="recipientIds">The IDs of the recipients</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAgentRegistrationNotificationAsync(Guid agentId, string agentType, string registeredBy, IEnumerable<string> recipientIds);

        /// <summary>
        /// Sends a notification about agent retirement.
        /// </summary>
        /// <param name="agentId">The ID of the retired agent</param>
        /// <param name="agentType">The type of the retired agent</param>
        /// <param name="retiredBy">The ID of the user who retired the agent</param>
        /// <param name="reason">The reason for retirement</param>
        /// <param name="recipientIds">The IDs of the recipients</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAgentRetirementNotificationAsync(Guid agentId, string agentType, string retiredBy, string reason, IEnumerable<string> recipientIds);

        /// <summary>
        /// Sends a notification about an authority override.
        /// </summary>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="overriddenBy">The ID of the user who overrode the authority</param>
        /// <param name="reason">The reason for the override</param>
        /// <param name="recipientIds">The IDs of the recipients</param>
        /// <param name="priority">The priority of the notification</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAuthorityOverrideNotificationAsync(Guid agentId, string overriddenBy, string reason, IEnumerable<string> recipientIds, NotificationPriority priority = NotificationPriority.High);

        /// <summary>
        /// Sends a notification requesting consent for an agent action.
        /// </summary>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="agentType">The type of the agent</param>
        /// <param name="consentType">The type of consent requested</param>
        /// <param name="userId">The ID of the user from whom consent is requested</param>
        /// <param name="actionDescription">A description of the action requiring consent</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendConsentRequestNotificationAsync(Guid agentId, string agentType, string consentType, string userId, string actionDescription);

        /// <summary>
        /// Sends a notification about agent task completion.
        /// </summary>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="taskId">The ID of the task</param>
        /// <param name="userId">The ID of the user who initiated the task</param>
        /// <param name="success">Whether the task was successful</param>
        /// <param name="summary">A summary of the task result</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAgentTaskCompletionNotificationAsync(Guid agentId, string taskId, string userId, bool success, string summary);

        /// <summary>
        /// Sends a notification about an agent error.
        /// </summary>
        /// <param name="agentId">The ID of the agent</param>
        /// <param name="errorCode">The error code</param>
        /// <param name="errorMessage">The error message</param>
        /// <param name="recipientIds">The IDs of the recipients</param>
        /// <param name="priority">The priority of the notification</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAgentErrorNotificationAsync(Guid agentId, string errorCode, string errorMessage, IEnumerable<string> recipientIds, NotificationPriority priority = NotificationPriority.High);

        /// <summary>
        /// Sends a notification about agent deprecation.
        /// </summary>
        /// <param name="agentId">The ID of the deprecated agent</param>
        /// <param name="agentType">The type of the deprecated agent</param>
        /// <param name="sunsetDate">The date when the agent will be retired</param>
        /// <param name="migrationPath">Information about the migration path</param>
        /// <param name="recipientIds">The IDs of the recipients</param>
        /// <returns>True if the notification was successfully sent; otherwise, false</returns>
        Task<bool> SendAgentDeprecationNotificationAsync(Guid agentId, string agentType, DateTimeOffset sunsetDate, string migrationPath, IEnumerable<string> recipientIds);
    }

    /// <summary>
    /// Interface for the notification delivery service.
    /// </summary>
    public interface INotificationDeliveryService
    {
        /// <summary>
        /// Delivers a notification through the appropriate channels.
        /// </summary>
        /// <param name="notification">The notification to deliver</param>
        Task DeliverNotificationAsync(NotificationMessage notification);
    }

    /// <summary>
    /// Represents a notification message.
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// The unique identifier of the notification.
        /// </summary>
        public string NotificationId { get; set; } = string.Empty;

        /// <summary>
        /// The type of notification.
        /// </summary>
        public string NotificationType { get; set; } = string.Empty;

        /// <summary>
        /// The priority of the notification.
        /// </summary>
        public NotificationPriority Priority { get; set; }

        /// <summary>
        /// The title of the notification.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The body of the notification.
        /// </summary>
        public string Body { get; set; } = string.Empty;

        /// <summary>
        /// Additional data associated with the notification.
        /// </summary>
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The IDs of the recipients.
        /// </summary>
        public List<string> RecipientIds { get; set; } = new List<string>();

        /// <summary>
        /// The channels through which to deliver the notification.
        /// </summary>
        public List<NotificationChannel> Channels { get; set; } = new List<NotificationChannel>();

        /// <summary>
        /// Actions that can be taken on the notification.
        /// </summary>
        public List<NotificationAction> ActionButtons { get; set; } = new List<NotificationAction>();

        /// <summary>
        /// The timestamp of the notification.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The expiration time of the notification.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// The number of times delivery has been retried.
        /// </summary>
        public int? RetryCount { get; set; }

        /// <summary>
        /// The email subject line.
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Email recipients.
        /// </summary>
        public List<string> Recipients { get; set; } = new List<string>();

        /// <summary>
        /// HTML body content for email notifications.
        /// </summary>
        public string? HtmlBody { get; set; }

        /// <summary>
        /// CC recipients for email notifications.
        /// </summary>
        public List<string>? CcRecipients { get; set; }

        /// <summary>
        /// BCC recipients for email notifications.
        /// </summary>
        public List<string>? BccRecipients { get; set; }

        /// <summary>
        /// Notification category for analytics.
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Email action links.
        /// </summary>
        public List<NotificationEmailAction>? Actions { get; set; }
    }

    /// <summary>
    /// Represents an action that can be taken on a notification.
    /// </summary>
    public class NotificationAction
    {
        /// <summary>
        /// The unique identifier of the action.
        /// </summary>
        public string ActionId { get; set; } = string.Empty;

        /// <summary>
        /// The label for the action button.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The type of action.
        /// </summary>
        public ActionType Type { get; set; }

        /// <summary>
        /// The URL to navigate to when the action is taken.
        /// </summary>
        public string NavigateUrl { get; set; } = string.Empty;

        /// <summary>
        /// Additional data associated with the action.
        /// </summary>
        public Dictionary<string, object> ActionData { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// The priority of a notification.
    /// </summary>
    public enum NotificationPriority
    {
        /// <summary>
        /// Unspecified priority.
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// Low priority.
        /// </summary>
        Low = 1,

        /// <summary>
        /// Normal priority.
        /// </summary>
        Normal = 2,

        /// <summary>
        /// High priority.
        /// </summary>
        High = 3,

        /// <summary>
        /// Critical priority.
        /// </summary>
        Critical = 4
    }

    /// <summary>
    /// The channel through which a notification is delivered.
    /// </summary>
    public enum NotificationChannel
    {
        /// <summary>
        /// In-app notification.
        /// </summary>
        InApp,

        /// <summary>
        /// Email notification.
        /// </summary>
        Email,

        /// <summary>
        /// Push notification.
        /// </summary>
        Push,

        /// <summary>
        /// SMS notification.
        /// </summary>
        SMS,

        /// <summary>
        /// Webhook notification.
        /// </summary>
        Webhook
    }

    /// <summary>
    /// The type of action.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// Default action.
        /// </summary>
        Default,

        /// <summary>
        /// Positive action (e.g., approve, confirm).
        /// </summary>
        Positive,

        /// <summary>
        /// Negative action (e.g., deny, cancel).
        /// </summary>
        Negative,

        /// <summary>
        /// Neutral action (e.g., view details).
        /// </summary>
        Neutral
    }

    /// <summary>
    /// Represents an action link for email notifications.
    /// </summary>
    public class NotificationEmailAction
    {
        /// <summary>
        /// The action type identifier (e.g., "approve", "deny", "info").
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// The display label for the action button.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The URL to invoke when the action is taken.
        /// </summary>
        public string Url { get; set; } = string.Empty;
    }
}
