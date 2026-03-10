namespace CognitiveMesh.FoundationLayer.AuditLogging.Enums
{
    /// <summary>
    /// Defines the types of audit events that can be logged for agents.
    /// </summary>
    public enum AgentAuditEventType
    {
        /// <summary>
        /// Indicates a policy was approved.
        /// </summary>
        PolicyApproved,
        
        /// <summary>
        /// Indicates a policy was rolled back.
        /// </summary>
        PolicyRolledBack,
        
        /// <summary>
        /// Indicates a governance violation occurred.
        /// </summary>
        GovernanceViolation,
        
        /// <summary>
        /// Indicates an ethical assessment was performed.
        /// </summary>
        EthicalAssessmentPerformed,
        
        /// <summary>
        /// Indicates a legal compliance check was performed.
        /// </summary>
        LegalComplianceChecked,
        
        /// <summary>
        /// Indicates an informational dignity violation occurred.
        /// </summary>
        InformationalDignityViolation,
        
        /// <summary>
        /// Indicates a cross-cultural adaptation occurred.
        /// </summary>
        CrossCulturalAdaptation
    }
}
