using System;
using System.Collections.Generic;

namespace CognitiveMesh.UILayer.Models.Marketplace
{
    /// <summary>
    /// Represents the status of a plugin submission in the review workflow.
    /// </summary>
    public enum SubmissionStatus
    {
        /// <summary>
        /// The plugin has been submitted and is awaiting review.
        /// </summary>
        Pending,

        /// <summary>
        /// The plugin is actively being reviewed by an administrator.
        /// </summary>
        UnderReview,

        /// <summary>
        /// The plugin has been approved and is available in the marketplace.
        /// </summary>
        Approved,

        /// <summary>
        /// The plugin has been rejected and will not be available in the marketplace.
        /// </summary>
        Rejected,

        /// <summary>
        /// The plugin requires changes from the developer before it can be reconsidered.
        /// </summary>
        RequiresChanges
    }

    /// <summary>
    /// Contains information about the developer or vendor who submitted the plugin.
    /// </summary>
    public class SubmitterInfo
    {
        /// <summary>
        /// The name of the individual author or developer team.
        /// </summary>
        public string AuthorName { get; set; }

        /// <summary>
        /// The name of the company or organization, if applicable.
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// The contact email address for the submitter.
        /// </summary>
        public string ContactEmail { get; set; }
    }

    /// <summary>
    /// Represents a single event in the audit trail of a plugin submission.
    /// </summary>
    public class AuditTrailEntry
    {
        /// <summary>
        /// The timestamp when the event occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// A description of the action taken (e.g., "Submitted", "Status Changed to Approved").
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The ID of the user or system that performed the action.
        /// </summary>
        public string ActorId { get; set; }

        /// <summary>
        /// Optional comments or notes related to this specific event.
        /// </summary>
        public string Comments { get; set; }
    }

    /// <summary>
    /// Represents a plugin submission to the marketplace, tracking its entire lifecycle
    /// from initial submission through review, approval, and publication.
    /// </summary>
    public class PluginSubmission
    {
        /// <summary>
        /// A unique identifier for the submission request.
        /// </summary>
        public string SubmissionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The full definition of the widget being submitted. This is the core payload of the submission.
        /// </summary>
        public WidgetDefinition WidgetDefinition { get; set; }

        /// <summary>
        /// Information about the person or entity who submitted the plugin.
        /// </summary>
        public SubmitterInfo SubmitterInfo { get; set; }

        /// <summary>
        /// The date and time when the plugin was originally submitted.
        /// </summary>
        public DateTimeOffset SubmissionDate { get; set; }

        /// <summary>
        /// The current status of the submission in the review workflow.
        /// </summary>
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;

        /// <summary>
        /// Feedback or comments from the marketplace administrator who reviewed the submission.
        /// </summary>
        public string ReviewerComments { get; set; }

        /// <summary>
        /// The results of any automated security scans performed on the plugin package.
        /// </summary>
        public string SecurityScanResults { get; set; }

        /// <summary>
        /// The cryptographic signature of the submitted plugin package, used to verify its integrity and origin.
        /// </summary>
        public string CodeSignature { get; set; }

        /// <summary>
        /// A chronological log of all actions and status changes related to this submission,
        /// providing a complete audit trail for compliance and tracking.
        /// </summary>
        public List<AuditTrailEntry> AuditTrail { get; set; } = new List<AuditTrailEntry>();
    }
}
