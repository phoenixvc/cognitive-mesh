namespace FoundationLayer.AuditLogging.Models
{
    /// <summary>
    /// Event data for when a value diagnostic run was executed.
    /// </summary>
    public class ValueDiagnosticRunEvent
    {
        /// <summary>
        /// The ID of the target being diagnosed.
        /// </summary>
        public string TargetId { get; set; } = string.Empty;

        /// <summary>
        /// The type of the target (e.g., "User", "Team").
        /// </summary>
        public string TargetType { get; set; } = string.Empty;

        /// <summary>
        /// The computed value score.
        /// </summary>
        public double ValueScore { get; set; }

        /// <summary>
        /// The value profile description.
        /// </summary>
        public string ValueProfile { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the diagnostic was executed.
        /// </summary>
        public DateTimeOffset ExecutedAt { get; set; }
    }

    /// <summary>
    /// Event data for when organizational blindness was detected.
    /// </summary>
    public class OrgBlindnessDetectedEvent
    {
        /// <summary>
        /// The ID of the organization.
        /// </summary>
        public string OrganizationId { get; set; } = string.Empty;

        /// <summary>
        /// The blindness risk score.
        /// </summary>
        public double BlindnessRiskScore { get; set; }

        /// <summary>
        /// The number of blind spots detected.
        /// </summary>
        public int BlindSpotCount { get; set; }

        /// <summary>
        /// The timestamp when the blindness was detected.
        /// </summary>
        public DateTimeOffset DetectedAt { get; set; }
    }

    /// <summary>
    /// Event data for when an employability risk was flagged.
    /// </summary>
    public class EmployabilityRiskFlaggedEvent
    {
        /// <summary>
        /// The ID of the user flagged.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The risk score.
        /// </summary>
        public double RiskScore { get; set; }

        /// <summary>
        /// The risk level (e.g., "Medium", "High").
        /// </summary>
        public string RiskLevel { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the risk was flagged.
        /// </summary>
        public DateTimeOffset FlaggedAt { get; set; }
    }

    /// <summary>
    /// Event data for when a manual adjudication was requested.
    /// </summary>
    public class ManualAdjudicationRequestedEvent
    {
        /// <summary>
        /// The ID of the review.
        /// </summary>
        public string ReviewId { get; set; } = string.Empty;

        /// <summary>
        /// The type of review.
        /// </summary>
        public string ReviewType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the subject being reviewed.
        /// </summary>
        public string SubjectId { get; set; } = string.Empty;

        /// <summary>
        /// The type of the subject being reviewed.
        /// </summary>
        public string SubjectType { get; set; } = string.Empty;

        /// <summary>
        /// The priority of the review.
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the adjudication was requested.
        /// </summary>
        public DateTimeOffset RequestedAt { get; set; }
    }
}
