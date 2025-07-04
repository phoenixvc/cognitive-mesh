using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.FoundationLayer.Notifications;

namespace CognitiveMesh.FoundationLayer.Notifications.Services
{
    /// <summary>
    /// Concrete implementation of INotificationDeliveryService using SendGrid for email delivery.
    /// This service handles sending notifications via email, with support for priority queuing,
    /// action buttons, and resilient delivery with automatic retries.
    /// </summary>
    public class SendGridNotificationService : INotificationDeliveryService, IDisposable
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridNotificationService> _logger;
        private readonly AgentCircuitBreakerPolicy _circuitBreaker;
        
        // Priority queues for notifications
        private readonly ConcurrentQueue<NotificationMessage> _criticalQueue = new ConcurrentQueue<NotificationMessage>();
        private readonly ConcurrentQueue<NotificationMessage> _highQueue = new ConcurrentQueue<NotificationMessage>();
        private readonly ConcurrentQueue<NotificationMessage> _normalQueue = new ConcurrentQueue<NotificationMessage>();
        private readonly ConcurrentQueue<NotificationMessage> _lowQueue = new ConcurrentQueue<NotificationMessage>();
        
        // Dead letter queue for failed notifications
        private readonly ConcurrentQueue<NotificationMessage> _deadLetterQueue = new ConcurrentQueue<NotificationMessage>();
        
        // Processing flags and cancellation
        private readonly SemaphoreSlim _processingSemaphore = new SemaphoreSlim(1, 1);
        private readonly Timer _processingTimer;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _isProcessing = false;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the SendGridNotificationService class.
        /// </summary>
        /// <param name="sendGridClient">The SendGrid client</param>
        /// <param name="options">SendGrid configuration options</param>
        /// <param name="logger">The logger</param>
        public SendGridNotificationService(
            ISendGridClient sendGridClient,
            IOptions<SendGridOptions> options,
            ILogger<SendGridNotificationService> logger)
        {
            _sendGridClient = sendGridClient ?? throw new ArgumentNullException(nameof(sendGridClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize circuit breaker for SendGrid API calls
            _circuitBreaker = new AgentCircuitBreakerPolicy(3, 250, 1000, 50);
            
            // Start the processing timer to process queues periodically
            _processingTimer = new Timer(ProcessQueuesCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds));
            
            _logger.LogInformation("SendGridNotificationService initialized with {QueueCapacity} queue capacity", 
                _options.MaxQueueCapacity);
        }

        /// <inheritdoc />
        public async Task<bool> SendNotificationAsync(NotificationMessage notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            // Validate notification
            if (string.IsNullOrEmpty(notification.Subject) || 
                (notification.Recipients == null || !notification.Recipients.Any()))
            {
                _logger.LogWarning("Invalid notification: Missing subject or recipients");
                return false;
            }

            // Check if we're over capacity
            int totalQueueSize = _criticalQueue.Count + _highQueue.Count + _normalQueue.Count + _lowQueue.Count;
            if (totalQueueSize >= _options.MaxQueueCapacity)
            {
                _logger.LogError("Notification queue at capacity ({Capacity}). Sending to dead letter queue.", 
                    _options.MaxQueueCapacity);
                _deadLetterQueue.Enqueue(notification);
                return false;
            }

            // Set default expiration if not provided
            if (!notification.ExpiresAt.HasValue)
            {
                notification.ExpiresAt = DateTimeOffset.UtcNow.AddDays(_options.DefaultExpirationDays);
            }

            // Add to the appropriate queue based on priority
            switch (notification.Priority)
            {
                case NotificationPriority.Critical:
                    _criticalQueue.Enqueue(notification);
                    _logger.LogDebug("Enqueued critical notification: {Subject}", notification.Subject);
                    break;
                case NotificationPriority.High:
                    _highQueue.Enqueue(notification);
                    _logger.LogDebug("Enqueued high priority notification: {Subject}", notification.Subject);
                    break;
                case NotificationPriority.Normal:
                    _normalQueue.Enqueue(notification);
                    _logger.LogDebug("Enqueued normal priority notification: {Subject}", notification.Subject);
                    break;
                case NotificationPriority.Low:
                    _lowQueue.Enqueue(notification);
                    _logger.LogDebug("Enqueued low priority notification: {Subject}", notification.Subject);
                    break;
                default:
                    _normalQueue.Enqueue(notification);
                    _logger.LogDebug("Enqueued notification with unspecified priority as normal: {Subject}", notification.Subject);
                    break;
            }

            // Trigger immediate processing for critical notifications
            if (notification.Priority == NotificationPriority.Critical)
            {
                await ProcessQueuesAsync();
            }

            return true;
        }

        /// <inheritdoc />
        public async Task<DeliveryStatus> CheckDeliveryStatusAsync(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                throw new ArgumentException("Notification ID cannot be empty", nameof(notificationId));
            }

            try
            {
                // In a real implementation, you would query SendGrid's API for delivery status
                // This is a simplified implementation
                
                // Check if the notification is in any of the queues
                if (IsInQueue(_criticalQueue, notificationId) || 
                    IsInQueue(_highQueue, notificationId) || 
                    IsInQueue(_normalQueue, notificationId) || 
                    IsInQueue(_lowQueue, notificationId))
                {
                    return new DeliveryStatus
                    {
                        NotificationId = notificationId,
                        Status = DeliveryStatusType.Queued,
                        LastUpdated = DateTimeOffset.UtcNow
                    };
                }

                // Check if the notification is in the dead letter queue
                if (IsInQueue(_deadLetterQueue, notificationId))
                {
                    return new DeliveryStatus
                    {
                        NotificationId = notificationId,
                        Status = DeliveryStatusType.Failed,
                        LastUpdated = DateTimeOffset.UtcNow,
                        ErrorMessage = "Notification failed to send and was moved to dead letter queue"
                    };
                }

                // In a real implementation, you would check SendGrid's API for delivery status
                // For now, we'll assume it was delivered if it's not in any queue
                return new DeliveryStatus
                {
                    NotificationId = notificationId,
                    Status = DeliveryStatusType.Delivered,
                    LastUpdated = DateTimeOffset.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking delivery status for notification {NotificationId}", notificationId);
                return new DeliveryStatus
                {
                    NotificationId = notificationId,
                    Status = DeliveryStatusType.Unknown,
                    LastUpdated = DateTimeOffset.UtcNow,
                    ErrorMessage = "Error checking delivery status"
                };
            }
        }

        /// <inheritdoc />
        public async Task<bool> CancelNotificationAsync(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                throw new ArgumentException("Notification ID cannot be empty", nameof(notificationId));
            }

            bool removed = false;
            
            // Try to remove from each queue
            removed |= RemoveFromQueue(_criticalQueue, notificationId);
            removed |= RemoveFromQueue(_highQueue, notificationId);
            removed |= RemoveFromQueue(_normalQueue, notificationId);
            removed |= RemoveFromQueue(_lowQueue, notificationId);
            removed |= RemoveFromQueue(_deadLetterQueue, notificationId);

            if (removed)
            {
                _logger.LogInformation("Notification {NotificationId} cancelled successfully", notificationId);
            }
            else
            {
                _logger.LogWarning("Notification {NotificationId} not found in any queue", notificationId);
            }

            return removed;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DeliveryStatus>> GetFailedNotificationsAsync()
        {
            var failedNotifications = new List<DeliveryStatus>();
            
            // Get all notifications from the dead letter queue
            foreach (var notification in _deadLetterQueue)
            {
                failedNotifications.Add(new DeliveryStatus
                {
                    NotificationId = notification.NotificationId,
                    Status = DeliveryStatusType.Failed,
                    LastUpdated = DateTimeOffset.UtcNow,
                    ErrorMessage = "Notification failed to send and was moved to dead letter queue",
                    Metadata = new Dictionary<string, string>
                    {
                        { "Subject", notification.Subject },
                        { "Priority", notification.Priority.ToString() },
                        { "RecipientCount", notification.Recipients.Count.ToString() }
                    }
                });
            }

            return failedNotifications;
        }

        /// <inheritdoc />
        public async Task<bool> RetryFailedNotificationAsync(string notificationId)
        {
            if (string.IsNullOrEmpty(notificationId))
            {
                throw new ArgumentException("Notification ID cannot be empty", nameof(notificationId));
            }

            // Find the notification in the dead letter queue
            var notification = _deadLetterQueue.FirstOrDefault(n => n.NotificationId == notificationId);
            if (notification == null)
            {
                _logger.LogWarning("Failed notification {NotificationId} not found for retry", notificationId);
                return false;
            }

            // Remove from dead letter queue
            RemoveFromQueue(_deadLetterQueue, notificationId);

            // Reset retry count and re-enqueue
            notification.RetryCount = 0;
            return await SendNotificationAsync(notification);
        }

        /// <summary>
        /// Processes the notification queues in priority order.
        /// </summary>
        private async Task ProcessQueuesAsync()
        {
            // Ensure only one processing operation at a time
            if (!await _processingSemaphore.WaitAsync(0))
            {
                return;
            }

            try
            {
                _isProcessing = true;
                
                // Process queues in priority order
                await ProcessQueue(_criticalQueue, _options.MaxCriticalRetries);
                await ProcessQueue(_highQueue, _options.MaxHighPriorityRetries);
                await ProcessQueue(_normalQueue, _options.MaxNormalPriorityRetries);
                await ProcessQueue(_lowQueue, _options.MaxLowPriorityRetries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification queues");
            }
            finally
            {
                _isProcessing = false;
                _processingSemaphore.Release();
            }
        }

        /// <summary>
        /// Processes a specific notification queue.
        /// </summary>
        /// <param name="queue">The queue to process</param>
        /// <param name="maxRetries">Maximum number of retries for notifications in this queue</param>
        private async Task ProcessQueue(ConcurrentQueue<NotificationMessage> queue, int maxRetries)
        {
            int processedCount = 0;
            int maxToProcess = Math.Min(queue.Count, _options.BatchSize);
            
            while (processedCount < maxToProcess && queue.TryDequeue(out var notification))
            {
                processedCount++;
                
                // Skip expired notifications
                if (notification.ExpiresAt.HasValue && notification.ExpiresAt.Value < DateTimeOffset.UtcNow)
                {
                    _logger.LogInformation("Skipping expired notification {NotificationId}: {Subject}", 
                        notification.NotificationId, notification.Subject);
                    continue;
                }

                try
                {
                    // Send the notification
                    bool success = await SendEmailViaGridAsync(notification);
                    
                    if (!success)
                    {
                        // If failed and under retry limit, re-enqueue
                        if (notification.RetryCount < maxRetries)
                        {
                            notification.RetryCount++;
                            queue.Enqueue(notification);
                            _logger.LogWarning("Re-enqueued notification {NotificationId} for retry {RetryCount}/{MaxRetries}", 
                                notification.NotificationId, notification.RetryCount, maxRetries);
                        }
                        else
                        {
                            // Move to dead letter queue if exceeded retry limit
                            _deadLetterQueue.Enqueue(notification);
                            _logger.LogError("Notification {NotificationId} exceeded retry limit. Moved to dead letter queue.", 
                                notification.NotificationId);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notification {NotificationId}", notification.NotificationId);
                    
                    // Move to dead letter queue on unexpected errors
                    _deadLetterQueue.Enqueue(notification);
                }
                
                // Avoid overwhelming the API with too many requests at once
                if (processedCount % _options.ThrottleAfter == 0)
                {
                    await Task.Delay(_options.ThrottleDelayMs);
                }
            }
        }

        /// <summary>
        /// Sends an email notification via SendGrid.
        /// </summary>
        /// <param name="notification">The notification to send</param>
        /// <returns>True if successful; otherwise, false</returns>
        private async Task<bool> SendEmailViaGridAsync(NotificationMessage notification)
        {
            try
            {
                // Create the email message
                var msg = new SendGridMessage();
                
                // Set sender
                msg.SetFrom(new EmailAddress(_options.DefaultFromEmail, _options.DefaultFromName));
                
                // Set subject
                msg.SetSubject(notification.Subject);
                
                // Add recipients
                foreach (var recipient in notification.Recipients)
                {
                    if (IsValidEmail(recipient))
                    {
                        msg.AddTo(new EmailAddress(recipient));
                    }
                    else
                    {
                        _logger.LogWarning("Invalid email address skipped: {Recipient}", recipient);
                    }
                }
                
                // Add CC recipients if any
                if (notification.CcRecipients != null)
                {
                    foreach (var cc in notification.CcRecipients)
                    {
                        if (IsValidEmail(cc))
                        {
                            msg.AddCc(new EmailAddress(cc));
                        }
                    }
                }
                
                // Add BCC recipients if any
                if (notification.BccRecipients != null)
                {
                    foreach (var bcc in notification.BccRecipients)
                    {
                        if (IsValidEmail(bcc))
                        {
                            msg.AddBcc(new EmailAddress(bcc));
                        }
                    }
                }
                
                // Create content based on notification type
                string htmlContent = await CreateHtmlContentAsync(notification);
                string plainTextContent = CreatePlainTextContent(notification);
                
                msg.AddContent(MimeType.Text, plainTextContent);
                msg.AddContent(MimeType.Html, htmlContent);
                
                // Add tracking settings
                msg.SetClickTracking(true, true);
                msg.SetOpenTracking(true);
                
                // Set category for analytics
                msg.AddCategory(notification.Category ?? "Default");
                
                // Add custom arguments for tracking
                msg.AddCustomArg("notification_id", notification.NotificationId);
                msg.AddCustomArg("priority", notification.Priority.ToString());
                
                // Execute the API call with circuit breaker
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var response = await _sendGridClient.SendEmailAsync(msg);
                    
                    if (response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.OK)
                    {
                        _logger.LogInformation("Notification {NotificationId} sent successfully", notification.NotificationId);
                        return true;
                    }
                    else
                    {
                        string responseBody = await response.Body.ReadAsStringAsync();
                        _logger.LogWarning("Failed to send notification {NotificationId}. Status: {StatusCode}, Response: {Response}", 
                            notification.NotificationId, response.StatusCode, responseBody);
                        return false;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification {NotificationId} via SendGrid", notification.NotificationId);
                return false;
            }
        }

        /// <summary>
        /// Creates HTML content for the notification, including action buttons if specified.
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <returns>HTML content</returns>
        private async Task<string> CreateHtmlContentAsync(NotificationMessage notification)
        {
            // Start with basic HTML template
            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.AppendLine("  <meta charset=\"UTF-8\">");
            htmlBuilder.AppendLine("  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            htmlBuilder.AppendLine("  <style>");
            htmlBuilder.AppendLine("    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }");
            htmlBuilder.AppendLine("    .container { max-width: 600px; margin: 0 auto; padding: 20px; }");
            htmlBuilder.AppendLine("    .header { background-color: #f8f9fa; padding: 15px; border-bottom: 1px solid #dee2e6; }");
            htmlBuilder.AppendLine("    .content { padding: 20px 0; }");
            htmlBuilder.AppendLine("    .footer { font-size: 12px; color: #6c757d; border-top: 1px solid #dee2e6; padding-top: 15px; margin-top: 20px; }");
            htmlBuilder.AppendLine("    .btn { display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; user-select: none; border: 1px solid transparent; padding: .375rem .75rem; font-size: 1rem; line-height: 1.5; border-radius: .25rem; text-decoration: none; margin: 5px 10px 5px 0; }");
            htmlBuilder.AppendLine("    .btn-primary { color: #fff; background-color: #007bff; border-color: #007bff; }");
            htmlBuilder.AppendLine("    .btn-secondary { color: #fff; background-color: #6c757d; border-color: #6c757d; }");
            htmlBuilder.AppendLine("    .btn-success { color: #fff; background-color: #28a745; border-color: #28a745; }");
            htmlBuilder.AppendLine("    .btn-danger { color: #fff; background-color: #dc3545; border-color: #dc3545; }");
            htmlBuilder.AppendLine("    .btn-info { color: #fff; background-color: #17a2b8; border-color: #17a2b8; }");
            htmlBuilder.AppendLine("  </style>");
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.AppendLine("  <div class=\"container\">");
            htmlBuilder.AppendLine("    <div class=\"header\">");
            htmlBuilder.AppendLine($"      <h2>{HttpUtility.HtmlEncode(notification.Subject)}</h2>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("    <div class=\"content\">");
            
            // Add the message body
            if (!string.IsNullOrEmpty(notification.HtmlBody))
            {
                htmlBuilder.AppendLine(notification.HtmlBody);
            }
            else if (!string.IsNullOrEmpty(notification.Body))
            {
                // Convert plain text to HTML
                htmlBuilder.AppendLine($"<p>{HttpUtility.HtmlEncode(notification.Body).Replace(Environment.NewLine, "<br />")}</p>");
            }
            
            // Add action buttons if specified
            if (notification.Actions != null && notification.Actions.Any())
            {
                htmlBuilder.AppendLine("    <div class=\"actions\" style=\"margin-top: 20px;\">");
                
                foreach (var action in notification.Actions)
                {
                    // Determine button style based on action type
                    string buttonClass = "btn-primary";
                    switch (action.Type.ToLowerInvariant())
                    {
                        case "approve":
                        case "confirm":
                        case "accept":
                            buttonClass = "btn-success";
                            break;
                        case "reject":
                        case "deny":
                        case "decline":
                            buttonClass = "btn-danger";
                            break;
                        case "info":
                        case "details":
                            buttonClass = "btn-info";
                            break;
                        case "cancel":
                            buttonClass = "btn-secondary";
                            break;
                    }
                    
                    // Create the action URL with tracking parameters
                    string actionUrl = action.Url;
                    if (!actionUrl.Contains("?"))
                    {
                        actionUrl += "?";
                    }
                    else
                    {
                        actionUrl += "&";
                    }
                    
                    actionUrl += $"notification_id={HttpUtility.UrlEncode(notification.NotificationId)}&action={HttpUtility.UrlEncode(action.Type)}";
                    
                    // Add the button
                    htmlBuilder.AppendLine($"      <a href=\"{actionUrl}\" class=\"btn {buttonClass}\">{HttpUtility.HtmlEncode(action.Label)}</a>");
                }
                
                htmlBuilder.AppendLine("    </div>");
            }
            
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("    <div class=\"footer\">");
            
            // Add expiration notice if applicable
            if (notification.ExpiresAt.HasValue)
            {
                htmlBuilder.AppendLine($"      <p>This notification will expire on {notification.ExpiresAt.Value:g}.</p>");
            }
            
            // Add unsubscribe link if required
            if (_options.IncludeUnsubscribeLink)
            {
                string unsubscribeUrl = $"{_options.UnsubscribeBaseUrl}?email={{{{recipient.email}}}}";
                htmlBuilder.AppendLine($"      <p><a href=\"{unsubscribeUrl}\">Unsubscribe</a> from these notifications.</p>");
            }
            
            htmlBuilder.AppendLine("      <p>&copy; " + DateTime.UtcNow.Year + " Cognitive Mesh. All rights reserved.</p>");
            htmlBuilder.AppendLine("    </div>");
            htmlBuilder.AppendLine("  </div>");
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");
            
            return htmlBuilder.ToString();
        }

        /// <summary>
        /// Creates plain text content for the notification.
        /// </summary>
        /// <param name="notification">The notification</param>
        /// <returns>Plain text content</returns>
        private string CreatePlainTextContent(NotificationMessage notification)
        {
            var textBuilder = new StringBuilder();
            
            // Add subject as header
            textBuilder.AppendLine(notification.Subject);
            textBuilder.AppendLine(new string('-', notification.Subject.Length));
            textBuilder.AppendLine();
            
            // Add the message body
            if (!string.IsNullOrEmpty(notification.Body))
            {
                textBuilder.AppendLine(notification.Body);
                textBuilder.AppendLine();
            }
            
            // Add action links if specified
            if (notification.Actions != null && notification.Actions.Any())
            {
                textBuilder.AppendLine("Actions:");
                
                foreach (var action in notification.Actions)
                {
                    textBuilder.AppendLine($"- {action.Label}: {action.Url}");
                }
                
                textBuilder.AppendLine();
            }
            
            // Add expiration notice if applicable
            if (notification.ExpiresAt.HasValue)
            {
                textBuilder.AppendLine($"This notification will expire on {notification.ExpiresAt.Value:g}.");
                textBuilder.AppendLine();
            }
            
            // Add footer
            textBuilder.AppendLine("© " + DateTime.UtcNow.Year + " Cognitive Mesh. All rights reserved.");
            
            return textBuilder.ToString();
        }

        /// <summary>
        /// Timer callback to process the notification queues.
        /// </summary>
        private async void ProcessQueuesCallback(object state)
        {
            if (_disposed)
            {
                return;
            }
            
            await ProcessQueuesAsync();
        }

        /// <summary>
        /// Checks if a notification is in a specific queue.
        /// </summary>
        /// <param name="queue">The queue to check</param>
        /// <param name="notificationId">The notification ID to find</param>
        /// <returns>True if the notification is in the queue; otherwise, false</returns>
        private bool IsInQueue(ConcurrentQueue<NotificationMessage> queue, string notificationId)
        {
            return queue.Any(n => n.NotificationId == notificationId);
        }

        /// <summary>
        /// Removes a notification from a specific queue.
        /// </summary>
        /// <param name="queue">The queue to modify</param>
        /// <param name="notificationId">The notification ID to remove</param>
        /// <returns>True if the notification was removed; otherwise, false</returns>
        private bool RemoveFromQueue(ConcurrentQueue<NotificationMessage> queue, string notificationId)
        {
            var tempList = new List<NotificationMessage>();
            bool found = false;
            
            // Dequeue all items
            while (queue.TryDequeue(out var notification))
            {
                if (notification.NotificationId == notificationId)
                {
                    found = true;
                }
                else
                {
                    tempList.Add(notification);
                }
            }
            
            // Re-enqueue items except the one we want to remove
            foreach (var notification in tempList)
            {
                queue.Enqueue(notification);
            }
            
            return found;
        }

        /// <summary>
        /// Validates if a string is a valid email address.
        /// </summary>
        /// <param name="email">The email to validate</param>
        /// <returns>True if valid; otherwise, false</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Disposes resources used by the service.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            _disposed = true;
            _cancellationTokenSource.Cancel();
            _processingTimer?.Dispose();
            _processingSemaphore?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }

    /// <summary>
    /// Configuration options for the SendGrid notification service.
    /// </summary>
    public class SendGridOptions
    {
        /// <summary>
        /// Default email address to send from.
        /// </summary>
        public string DefaultFromEmail { get; set; } = "notifications@cognitivemesh.com";
        
        /// <summary>
        /// Default display name for the sender.
        /// </summary>
        public string DefaultFromName { get; set; } = "Cognitive Mesh Notifications";
        
        /// <summary>
        /// Maximum number of notifications to process in a batch.
        /// </summary>
        public int BatchSize { get; set; } = 50;
        
        /// <summary>
        /// Maximum capacity of the notification queues.
        /// </summary>
        public int MaxQueueCapacity { get; set; } = 10000;
        
        /// <summary>
        /// Number of seconds between queue processing runs.
        /// </summary>
        public int ProcessingIntervalSeconds { get; set; } = 5;
        
        /// <summary>
        /// Maximum number of retries for critical notifications.
        /// </summary>
        public int MaxCriticalRetries { get; set; } = 10;
        
        /// <summary>
        /// Maximum number of retries for high priority notifications.
        /// </summary>
        public int MaxHighPriorityRetries { get; set; } = 5;
        
        /// <summary>
        /// Maximum number of retries for normal priority notifications.
        /// </summary>
        public int MaxNormalPriorityRetries { get; set; } = 3;
        
        /// <summary>
        /// Maximum number of retries for low priority notifications.
        /// </summary>
        public int MaxLowPriorityRetries { get; set; } = 1;
        
        /// <summary>
        /// Default expiration in days for notifications.
        /// </summary>
        public int DefaultExpirationDays { get; set; } = 7;
        
        /// <summary>
        /// Number of API calls after which to add a delay to avoid rate limiting.
        /// </summary>
        public int ThrottleAfter { get; set; } = 10;
        
        /// <summary>
        /// Delay in milliseconds to add after throttle threshold is reached.
        /// </summary>
        public int ThrottleDelayMs { get; set; } = 100;
        
        /// <summary>
        /// Whether to include an unsubscribe link in emails.
        /// </summary>
        public bool IncludeUnsubscribeLink { get; set; } = true;
        
        /// <summary>
        /// Base URL for unsubscribe links.
        /// </summary>
        public string UnsubscribeBaseUrl { get; set; } = "https://cognitivemesh.com/unsubscribe";
    }
}
