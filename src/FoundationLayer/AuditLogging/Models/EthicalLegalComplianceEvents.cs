namespace FoundationLayer.AuditLogging.Models
{
    /// <summary>
    /// Event data for when a new governance/ethical/legal policy was approved.
    /// </summary>
    public class PolicyApprovedEvent
    {
        /// <summary>
        /// The unique identifier of the approved policy.
        /// </summary>
        public string PolicyId { get; set; } = string.Empty;

        /// <summary>
        /// The type of policy (e.g., "Legal", "Ethical", "Cultural").
        /// </summary>
        public string PolicyType { get; set; } = string.Empty;

        /// <summary>
        /// The version of the policy that was approved.
        /// </summary>
        public string PolicyVersion { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user who approved the policy.
        /// </summary>
        public string ApprovedBy { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the policy was approved.
        /// </summary>
        public DateTimeOffset ApprovedAt { get; set; }

        /// <summary>
        /// A description of the policy.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The list of stakeholders who participated in the approval process.
        /// </summary>
        public List<string> Stakeholders { get; set; } = new List<string>();

        /// <summary>
        /// The tenant ID to which this policy applies.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event data for when an existing policy was rolled back to a previous version.
    /// </summary>
    public class PolicyRolledBackEvent
    {
        /// <summary>
        /// The unique identifier of the policy.
        /// </summary>
        public string PolicyId { get; set; } = string.Empty;

        /// <summary>
        /// The type of policy (e.g., "Legal", "Ethical", "Cultural").
        /// </summary>
        public string PolicyType { get; set; } = string.Empty;

        /// <summary>
        /// The version of the policy before the rollback.
        /// </summary>
        public string PreviousVersion { get; set; } = string.Empty;

        /// <summary>
        /// The version of the policy after the rollback (the version being rolled back to).
        /// </summary>
        public string NewVersion { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user who initiated the rollback.
        /// </summary>
        public string RolledBackBy { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the rollback occurred.
        /// </summary>
        public DateTimeOffset RolledBackAt { get; set; }

        /// <summary>
        /// The reason for the rollback.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID to which this policy applies.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event data for when a governance violation was detected by the monitoring engine.
    /// </summary>
    public class GovernanceViolationEvent
    {
        /// <summary>
        /// The unique identifier of the violation.
        /// </summary>
        public string ViolationId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the policy that was violated.
        /// </summary>
        public string PolicyId { get; set; } = string.Empty;

        /// <summary>
        /// The type of violation (e.g., "AccessControl", "DataResidency", "ConsentViolation").
        /// </summary>
        public string ViolationType { get; set; } = string.Empty;

        /// <summary>
        /// The severity of the violation (e.g., "Low", "Medium", "High", "Critical").
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the violation was detected.
        /// </summary>
        public DateTimeOffset DetectedAt { get; set; }

        /// <summary>
        /// The ID of the system or user that detected the violation.
        /// </summary>
        public string DetectedBy { get; set; } = string.Empty;

        /// <summary>
        /// The context in which the violation occurred.
        /// </summary>
        public Dictionary<string, object> Context { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// A description of the violation.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID in which the violation occurred.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event data for when an ethical assessment was performed on an action or reasoning chain.
    /// </summary>
    public class EthicalAssessmentPerformedEvent
    {
        /// <summary>
        /// The unique identifier of the assessment.
        /// </summary>
        public string AssessmentId { get; set; } = string.Empty;

        /// <summary>
        /// The type of assessment (e.g., "NormativeAgency", "InformationEthics").
        /// </summary>
        public string AssessmentType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the agent being assessed.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The type of action being assessed.
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the action or reasoning chain is ethically valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// The list of violations detected during the assessment.
        /// </summary>
        public List<string> ViolationsDetected { get; set; } = new List<string>();

        /// <summary>
        /// The timestamp when the assessment was performed.
        /// </summary>
        public DateTimeOffset AssessmentTimestamp { get; set; }

        /// <summary>
        /// The reasoning trace that led to the assessment result.
        /// </summary>
        public string ReasoningTrace { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID in which the assessment was performed.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event data for when a legal compliance check (e.g., GDPR, EU AI Act) was performed.
    /// </summary>
    public class LegalComplianceCheckedEvent
    {
        /// <summary>
        /// The unique identifier of the compliance check.
        /// </summary>
        public string ComplianceCheckId { get; set; } = string.Empty;

        /// <summary>
        /// The type of regulation being checked (e.g., "GDPR", "EUAIAct", "SectoralRegulation").
        /// </summary>
        public string RegulationType { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the data subject (if applicable).
        /// </summary>
        public string DataSubjectId { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the check determined compliance.
        /// </summary>
        public bool IsCompliant { get; set; }

        /// <summary>
        /// The list of compliance issues detected during the check.
        /// </summary>
        public List<string> ComplianceIssues { get; set; } = new List<string>();

        /// <summary>
        /// The timestamp when the check was performed.
        /// </summary>
        public DateTimeOffset CheckedAt { get; set; }

        /// <summary>
        /// The ID of the system or user that performed the check.
        /// </summary>
        public string CheckedBy { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID in which the check was performed.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// The specific articles or sections of the regulation that were checked.
        /// </summary>
        public List<string> RegulationSections { get; set; } = new List<string>();
    }

    /// <summary>
    /// Event data for when an informational dignity violation (Floridi) was detected.
    /// </summary>
    public class InformationalDignityViolationEvent
    {
        /// <summary>
        /// The unique identifier of the violation.
        /// </summary>
        public string ViolationId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the subject whose informational dignity was violated.
        /// </summary>
        public string SubjectId { get; set; } = string.Empty;

        /// <summary>
        /// The type of data involved in the violation (e.g., "PII", "Behavioral", "Inferred").
        /// </summary>
        public string DataType { get; set; } = string.Empty;

        /// <summary>
        /// The action that led to the violation (e.g., "Store", "Analyze", "Share").
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// The type of violation (e.g., "Misrepresentation", "LossOfControl", "InformationalHarm").
        /// </summary>
        public string ViolationType { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the violation was detected.
        /// </summary>
        public DateTimeOffset DetectedAt { get; set; }

        /// <summary>
        /// A description of the violation.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The severity of the violation (e.g., "Low", "Medium", "High", "Critical").
        /// </summary>
        public string Severity { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID in which the violation occurred.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Event data for when a cross-cultural adaptation action was executed (Hofstede framework).
    /// </summary>
    public class CrossCulturalAdaptationEvent
    {
        /// <summary>
        /// The unique identifier of the adaptation action.
        /// </summary>
        public string AdaptationId { get; set; } = string.Empty;

        /// <summary>
        /// The ID of the user for whom the adaptation was performed.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The Hofstede cultural dimensions used for the adaptation.
        /// </summary>
        public Dictionary<string, object> CulturalDimensions { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// The type of adaptation performed (e.g., "UIPrompt", "ConsentFlow", "AuthorityDisplay").
        /// </summary>
        public string AdaptationType { get; set; } = string.Empty;

        /// <summary>
        /// The context in which the adaptation was performed.
        /// </summary>
        public string AdaptationContext { get; set; } = string.Empty;

        /// <summary>
        /// The timestamp when the adaptation was performed.
        /// </summary>
        public DateTimeOffset AdaptedAt { get; set; }

        /// <summary>
        /// The original content before adaptation.
        /// </summary>
        public string OriginalContent { get; set; } = string.Empty;

        /// <summary>
        /// The adapted content after cultural adaptation.
        /// </summary>
        public string AdaptedContent { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID in which the adaptation was performed.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;
    }
}
