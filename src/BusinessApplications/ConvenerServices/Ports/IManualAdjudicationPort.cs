using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- DTOs for the Manual Adjudication Port ---
namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models
{
    /// <summary>
    /// Represents the status of a manual review case.
    /// </summary>
    public enum ReviewStatus
    {
        /// <summary>
        /// The review case has been submitted and is awaiting assignment.
        /// </summary>
        Pending,

        /// <summary>
        /// The review case has been assigned to a reviewer and is in progress.
        /// </summary>
        InReview,

        /// <summary>
        /// The review has been completed and approved.
        /// </summary>
        Approved,

        /// <summary>
        /// The review has been completed and rejected.
        /// </summary>
        Rejected,

        /// <summary>
        /// The review requires additional information before a decision can be made.
        /// </summary>
        NeedsMoreInfo,

        /// <summary>
        /// The review has been escalated to a higher authority.
        /// </summary>
        Escalated
    }

    /// <summary>
    /// Represents a request to submit a case for manual human review.
    /// </summary>
    public class ManualReviewRequest
    {
        /// <summary>
        /// The tenant context for this review request.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The user or system submitting the review request.
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// The type of review being requested (e.g., "EmployabilityHighRisk", "OrgBlindnessAlert").
        /// </summary>
        public string ReviewType { get; set; }

        /// <summary>
        /// The subject of the review (e.g., a user ID, organization ID, or other entity).
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// The subject type (e.g., "User", "Organization", "Team").
        /// </summary>
        public string SubjectType { get; set; }

        /// <summary>
        /// The priority of the review request.
        /// </summary>
        public string Priority { get; set; } = "Normal";

        /// <summary>
        /// A summary of the case for quick reference.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detailed information about the case.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Additional context data relevant to the review.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// An optional deadline by which the review should be completed.
        /// </summary>
        public DateTimeOffset? Deadline { get; set; }

        /// <summary>
        /// A correlation ID for tracking this request across system boundaries.
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response to a manual review request submission.
    /// </summary>
    public class ManualReviewResponse
    {
        /// <summary>
        /// The unique identifier for the review case.
        /// </summary>
        public string ReviewId { get; set; }

        /// <summary>
        /// The current status of the review.
        /// </summary>
        public ReviewStatus Status { get; set; }

        /// <summary>
        /// When the review was submitted.
        /// </summary>
        public DateTimeOffset SubmittedAt { get; set; }

        /// <summary>
        /// The estimated completion time, if available.
        /// </summary>
        public DateTimeOffset? EstimatedCompletionTime { get; set; }

        /// <summary>
        /// The correlation ID from the original request.
        /// </summary>
        public string CorrelationId { get; set; }
    }

    /// <summary>
    /// Represents a decision made by a reviewer.
    /// </summary>
    public class ReviewDecision
    {
        /// <summary>
        /// The unique identifier of the review case.
        /// </summary>
        public string ReviewId { get; set; }

        /// <summary>
        /// The user making the decision.
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// The decision (Approved or Rejected).
        /// </summary>
        public ReviewStatus Decision { get; set; }

        /// <summary>
        /// The rationale for the decision.
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// Any additional notes or instructions.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// When the decision was made.
        /// </summary>
        public DateTimeOffset DecisionTimestamp { get; set; } = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Represents a complete record of a manual review case.
    /// </summary>
    public class ReviewRecord
    {
        /// <summary>
        /// The unique identifier for the review case.
        /// </summary>
        public string ReviewId { get; set; }

        /// <summary>
        /// The tenant context for this review.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The user or system that submitted the review request.
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// The type of review.
        /// </summary>
        public string ReviewType { get; set; }

        /// <summary>
        /// The subject of the review.
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// The subject type.
        /// </summary>
        public string SubjectType { get; set; }

        /// <summary>
        /// The priority of the review.
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// A summary of the case.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Detailed information about the case.
        /// </summary>
        public string Details { get; set; }

        /// <summary>
        /// Additional context data.
        /// </summary>
        public Dictionary<string, object> Context { get; set; }

        /// <summary>
        /// When the review was submitted.
        /// </summary>
        public DateTimeOffset SubmittedAt { get; set; }

        /// <summary>
        /// When the review was assigned to a reviewer, if applicable.
        /// </summary>
        public DateTimeOffset? AssignedAt { get; set; }

        /// <summary>
        /// The user assigned to review the case, if applicable.
        /// </summary>
        public string AssignedTo { get; set; }

        /// <summary>
        /// The current status of the review.
        /// </summary>
        public ReviewStatus Status { get; set; }

        /// <summary>
        /// When the review was completed, if applicable.
        /// </summary>
        public DateTimeOffset? CompletedAt { get; set; }

        /// <summary>
        /// The user who completed the review, if applicable.
        /// </summary>
        public string ReviewedBy { get; set; }

        /// <summary>
        /// The rationale for the decision, if applicable.
        /// </summary>
        public string Rationale { get; set; }

        /// <summary>
        /// Any additional notes or instructions.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// The history of status changes and comments for this review.
        /// </summary>
        public List<ReviewHistoryEntry> History { get; set; } = new List<ReviewHistoryEntry>();

        /// <summary>
        /// The correlation ID for tracking this review across system boundaries.
        /// </summary>
        public string CorrelationId { get; set; }
    }

    /// <summary>
    /// Represents an entry in the history of a review case.
    /// </summary>
    public class ReviewHistoryEntry
    {
        /// <summary>
        /// When this entry was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// The user who performed the action.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The action that was performed.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The status of the review after this action.
        /// </summary>
        public ReviewStatus Status { get; set; }

        /// <summary>
        /// Any comments associated with this action.
        /// </summary>
        public string Comments { get; set; }
    }

    /// <summary>
    /// Well-known review types used throughout the platform.
    /// These constants should be referenced instead of hard-coding strings
    /// when submitting review requests.
    /// </summary>
    public static class ReviewTypes
    {
        /// <summary>
        /// Review required for high-risk employability assessments before results are released.
        /// </summary>
        public const string EmployabilityHighRisk = "EmployabilityHighRisk";

        /// <summary>
        /// Review required for significant organizational blindness detections.
        /// </summary>
        public const string OrgBlindnessAlert = "OrgBlindnessAlert";

        /// <summary>
        /// Review required for value diagnostic results that indicate potential issues.
        /// </summary>
        public const string ValueDiagnosticAlert = "ValueDiagnosticAlert";
    }
}

// --- Port Interface ---
namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports
{
    using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;

    /// <summary>
    /// Defines the contract for the Manual Adjudication Port in the BusinessApplications Layer.
    /// This port is responsible for managing human-in-the-loop review processes for sensitive
    /// operations that require explicit human approval before proceeding, adhering to the
    /// Hexagonal Architecture pattern.
    /// </summary>
    /// <remarks>
    /// **Priority:** Should
    /// **SLA:** Review submission must complete in &lt;1s. Human review turn-around time should be &lt;1h.
    /// **Acceptance Criteria (G/W/T):** Given flagged employability user, When case submitted, Then require explicit human review/approval before data release.
    /// </remarks>
    public interface IManualAdjudicationPort
    {
        /// <summary>
        /// Submits a case for manual human review. This is typically called when an automated process
        /// detects a situation that requires human judgment or approval.
        /// </summary>
        /// <param name="request">The request containing the details of the case.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// <see cref="ManualReviewResponse"/> with the review ID and initial status.
        /// </returns>
        Task<ManualReviewResponse> SubmitForReviewAsync(ManualReviewRequest request);

        /// <summary>
        /// Checks the current status of a review case.
        /// </summary>
        /// <param name="reviewId">The ID of the review case.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// <see cref="ReviewRecord"/> with the current state of the review.
        /// </returns>
        Task<ReviewRecord> GetReviewStatusAsync(string reviewId, string tenantId);

        /// <summary>
        /// Records a decision on a review case. This can only be called by authorized reviewers.
        /// </summary>
        /// <param name="decision">The decision details.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the decision was recorded successfully.</returns>
        Task<bool> RecordDecisionAsync(ReviewDecision decision, string tenantId);

        /// <summary>
        /// Retrieves all pending review cases for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The tenant ID.</param>
        /// <param name="reviewType">Optional filter for specific review types.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an
        /// enumerable collection of pending <see cref="ReviewRecord"/>s.
        /// </returns>
        Task<IEnumerable<ReviewRecord>> GetPendingReviewsAsync(string tenantId, string reviewType = null);

        /// <summary>
        /// Retrieves all review cases for a specific subject (e.g., user, organization).
        /// </summary>
        /// <param name="subjectId">The ID of the subject.</param>
        /// <param name="subjectType">The type of the subject.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an
        /// enumerable collection of <see cref="ReviewRecord"/>s for the subject.
        /// </returns>
        Task<IEnumerable<ReviewRecord>> GetReviewsForSubjectAsync(string subjectId, string subjectType, string tenantId);

        /// <summary>
        /// Assigns a review case to a specific reviewer.
        /// </summary>
        /// <param name="reviewId">The ID of the review case.</param>
        /// <param name="assigneeId">The ID of the user to assign the review to.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the assignment was successful.</returns>
        Task<bool> AssignReviewAsync(string reviewId, string assigneeId, string tenantId);

        /// <summary>
        /// Adds a comment to a review case without changing its status.
        /// </summary>
        /// <param name="reviewId">The ID of the review case.</param>
        /// <param name="userId">The ID of the user adding the comment.</param>
        /// <param name="comment">The comment text.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the comment was added successfully.</returns>
        Task<bool> AddCommentAsync(string reviewId, string userId, string comment, string tenantId);

        /// <summary>
        /// Requests additional information for a review case.
        /// </summary>
        /// <param name="reviewId">The ID of the review case.</param>
        /// <param name="reviewerId">The ID of the reviewer requesting information.</param>
        /// <param name="requestDetails">Details of the information being requested.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the request was recorded successfully.</returns>
        Task<bool> RequestAdditionalInfoAsync(string reviewId, string reviewerId, string requestDetails, string tenantId);

        /// <summary>
        /// Escalates a review case to a higher authority.
        /// </summary>
        /// <param name="reviewId">The ID of the review case.</param>
        /// <param name="reviewerId">The ID of the reviewer escalating the case.</param>
        /// <param name="escalationReason">The reason for escalation.</param>
        /// <param name="tenantId">The tenant ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the escalation was recorded successfully.</returns>
        Task<bool> EscalateReviewAsync(string reviewId, string reviewerId, string escalationReason, string tenantId);
    }
}
