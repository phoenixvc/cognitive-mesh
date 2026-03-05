using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Models
{
    /// <summary>
    /// Represents an audit event in the system.
    /// </summary>
    public class AuditEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditEvent"/> class.
        /// </summary>
        public AuditEvent()
        {
            EventId = Guid.NewGuid().ToString();
            Timestamp = DateTimeOffset.UtcNow;
            Metadata = new Dictionary<string, string>();
        }

        /// <summary>
        /// Gets or sets the unique identifier of the event.
        /// </summary>
        [Key]
        [Required]
        [StringLength(36)]
        public string EventId { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the event.
        /// </summary>
        [StringLength(50)]
        public string? EventCategory { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the event occurred.
        /// </summary>
        [Required]
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID for tracing related events.
        /// </summary>
        [StringLength(100)]
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who triggered the event.
        /// </summary>
        [StringLength(100)]
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID associated with the event.
        /// </summary>
        [StringLength(100)]
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the source of the event.
        /// </summary>
        [StringLength(200)]
        public string? Source { get; set; }

        /// <summary>
        /// Gets or sets the serialized event data.
        /// </summary>
        public string? EventData { get; set; }

        /// <summary>
        /// Gets or sets additional metadata associated with the event.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the serialized metadata for database storage.
        /// </summary>
        [StringLength(4000)]
        public string? SerializedMetadata
        {
            get => Metadata != null && Metadata.Count > 0 
                ? System.Text.Json.JsonSerializer.Serialize(Metadata) 
                : null;
            set => Metadata = !string.IsNullOrEmpty(value) 
                ? System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(value) ?? new Dictionary<string, string>()
                : new Dictionary<string, string>();
        }
    }
}
