namespace CognitiveMesh.FoundationLayer.Notifications
{
    /// <summary>
    /// Represents the delivery status of a notification.
    /// </summary>
    public enum DeliveryStatus
    {
        /// <summary>
        /// The delivery status is unknown.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// The notification is queued for delivery.
        /// </summary>
        Queued,
        
        /// <summary>
        /// The notification is being processed.
        /// </summary>
        Processing,
        
        /// <summary>
        /// The notification was delivered successfully.
        /// </summary>
        Delivered,
        
        /// <summary>
        /// The notification delivery failed.
        /// </summary>
        Failed,
        
        /// <summary>
        /// The notification was rejected by the provider.
        /// </summary>
        Rejected,
        
        /// <summary>
        /// The notification bounced back.
        /// </summary>
        Bounced,
        
        /// <summary>
        /// The notification was dropped.
        /// </summary>
        Dropped,
        
        /// <summary>
        /// The notification was deferred for later delivery.
        /// </summary>
        Deferred,
        
        /// <summary>
        /// The notification was delivered but marked as spam.
        /// </summary>
        Spam
    }
}
