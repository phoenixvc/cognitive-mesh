using System;
using System.Collections.Generic;

namespace CognitiveMesh.FoundationLayer.AuditLogging.Models
{
    /// <summary>
    /// Criteria for searching audit events.
    /// </summary>
    public class AuditSearchCriteria
    {
        /// <summary>
        /// Gets or sets the start time for filtering events.
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time for filtering events.
        /// </summary>
        public DateTimeOffset? EndTime { get; set; }

        /// <summary>
        /// Gets or sets the tenant ID for filtering events.
        /// </summary>
        public string? TenantId { get; set; }

        /// <summary>
        /// Gets or sets the user ID for filtering events.
        /// </summary>
        public string? UserId { get; set; }

        /// <summary>
        /// Gets or sets the event type for filtering events.
        /// </summary>
        public string? EventType { get; set; }

        /// <summary>
        /// Gets or sets the correlation ID for filtering events.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// The user IDs to include.
        /// </summary>
        public List<string> UserIds { get; set; } = new List<string>();

        /// <summary>
        /// The event types to include.
        /// </summary>
        public List<string> EventTypes { get; set; } = new List<string>();

        /// <summary>
        /// The event categories to include.
        /// </summary>
        public List<string> EventCategories { get; set; } = new List<string>();

        /// <summary>
        /// The agent IDs to include.
        /// </summary>
        public List<Guid> AgentIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Gets or sets the maximum number of results to return.
        /// </summary>
        public int? MaxResults { get; set; }

        /// <summary>
        /// Gets or sets the number of results to skip.
        /// </summary>
        public int? Skip { get; set; }
        
        /// <summary>
        /// Gets or sets the page number for pagination (1-based).
        /// </summary>
        public int? PageNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the page size for pagination.
        /// </summary>
        public int? PageSize { get; set; }

        /// <summary>
        /// Gets or sets the search text to look for in event data or type.
        /// </summary>
        public string? SearchText { get; set; }

        /// <summary>
        /// Gets or sets the key-value pairs to search for within the event data.
        /// </summary>
        public Dictionary<string, object> EventDataContains { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a value indicating whether to sort results in descending order.
        /// If false, results will be sorted in ascending order.
        /// Default is true (descending).
        /// </summary>
        public bool? SortDescending { get; set; } = true;
    }
}
