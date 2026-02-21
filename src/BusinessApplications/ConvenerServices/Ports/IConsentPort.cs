using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- DTOs for the Consent Port ---
namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models
{
    /// <summary>
    /// Represents a request to record a user's consent decision for a specific action or data usage policy.
    /// </summary>
    public class ConsentRequest
    {
        /// <summary>
        /// The ID of the user giving consent.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// The tenant ID to which this consent applies.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// The specific type of consent being granted or denied (e.g., "NotifyOnProjectOpportunities", "AutoCreateCollaborationSpaces").
        /// This should be a well-known, documented string.
        /// </summary>
        public string ConsentType { get; set; } = string.Empty;

        /// <summary>
        /// An optional identifier to narrow the scope of the consent (e.g., a specific project ID or channel ID).
        /// If null or empty, the consent is considered global for the given type.
        /// </summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// True if consent is being granted; false if it is being denied or revoked.
        /// </summary>
        public bool IsGranted { get; set; }

        /// <summary>
        /// The source of the consent action, used for auditing (e.g., "Widget:ChampionFinder", "UserProfileSettings").
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Optional evidence, such as a link to the version of the privacy policy or terms the user agreed to.
        /// </summary>
        public string Evidence { get; set; } = string.Empty;

        /// <summary>
        /// The level of consent being captured (e.g., "Standard", "LegallyBinding", "ExplicitGDPRConsent").
        /// Use this to distinguish between advisory consent and those required by regulation.
        /// </summary>
        public string ConsentLevel { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) The legal framework that governs this consent (e.g., "GDPR", "EUAIAct", "HIPAA").
        /// When provided, downstream services can enforce jurisdiction-specific requirements.
        /// </summary>
        public string LegalFramework { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) A timestamp indicating when this consent expires. <c>null</c> means no expiration.
        /// </summary>
        public DateTimeOffset? ExpirationTime { get; set; }
    }

    /// <summary>
    /// Represents a persisted record of a user's consent decision.
    /// This object serves as an immutable audit record.
    /// </summary>
    public class ConsentRecord
    {
        /// <summary>Gets or sets the unique consent record identifier.</summary>
        public string ConsentId { get; set; } = string.Empty;
        /// <summary>Gets or sets the user identifier.</summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>Gets or sets the tenant identifier.</summary>
        public string TenantId { get; set; } = string.Empty;
        /// <summary>Gets or sets the type of consent.</summary>
        public string ConsentType { get; set; } = string.Empty;
        /// <summary>Gets or sets the scope of the consent.</summary>
        public string Scope { get; set; } = string.Empty;
        /// <summary>Gets or sets whether consent was granted.</summary>
        public bool IsGranted { get; set; }
        /// <summary>Gets or sets the timestamp of the consent decision.</summary>
        public DateTimeOffset Timestamp { get; set; }
        /// <summary>Gets or sets the source of the consent action.</summary>
        public string Source { get; set; } = string.Empty;
        /// <summary>Gets or sets the consent evidence reference.</summary>
        public string Evidence { get; set; } = string.Empty;

        /// <summary>
        /// The level of consent granted (mirrors <see cref="ConsentRequest.ConsentLevel"/>).
        /// </summary>
        public string ConsentLevel { get; set; } = string.Empty;

        /// <summary>
        /// The legal framework (e.g., GDPR, EUAIAct) relevant for this consent record.
        /// </summary>
        public string LegalFramework { get; set; } = string.Empty;

        /// <summary>
        /// When the consent expires (<c>null</c> if it does not expire).
        /// </summary>
        public DateTimeOffset? ExpirationTime { get; set; }

        /// <summary>
        /// Additional evidence metadata (e.g., document hashes, policy version).
        /// </summary>
        public Dictionary<string, string> EvidenceMetadata { get; set; } = new();
    }

    /// <summary>
    /// Represents a request to validate whether a user has given a specific type of consent.
    /// </summary>
    public class ValidateConsentRequest
    {
        /// <summary>Gets or sets the user identifier to validate.</summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>Gets or sets the tenant identifier.</summary>
        public string TenantId { get; set; } = string.Empty;
        /// <summary>Gets or sets the required consent type to check.</summary>
        public string RequiredConsentType { get; set; } = string.Empty;
        /// <summary>Gets or sets the optional scope to validate.</summary>
        public string Scope { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Specifies the minimum consent level that must have been granted.
        /// </summary>
        public string RequiredConsentLevel { get; set; } = string.Empty;

        /// <summary>
        /// (Optional) Specifies the regulatory framework that the consent must satisfy.
        /// </summary>
        public string RequiredLegalFramework { get; set; } = string.Empty;
    }

    /// <summary>
    /// The response from a consent validation check.
    /// </summary>
    public class ValidateConsentResponse
    {
        /// <summary>
        /// True if the required consent has been granted; otherwise, false.
        /// </summary>
        public bool HasConsent { get; set; }

        /// <summary>
        /// The timestamp of when the validation was performed.
        /// </summary>
        public DateTimeOffset ValidationTimestamp { get; set; }

        /// <summary>
        /// The ID of the relevant consent record, if one exists.
        /// </summary>
        public string ConsentRecordId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Well-known consent type identifiers used throughout the platform.
    /// These constants should be referenced instead of hard-coding strings
    /// when recording or validating consent decisions.
    /// </summary>
    public static class ConsentTypes
    {
        /// <summary>
        /// Permission for the system to perform employability-risk analysis on
        /// a user.  Required before invoking any logic in the
        /// <c>EmployabilityPredictorEngine</c> / <c>IEmployabilityPort</c>.
        /// </summary>
        public const string EmployabilityAnalysis = "EmployabilityAnalysis";

        /// <summary>
        /// Permission to collect and process the data necessary to run value
        /// diagnostics (e.g., the “$200 Test”) via the
        /// <c>ValueGenerationDiagnosticEngine</c>.
        /// </summary>
        public const string ValueDiagnosticDataCollection = "ValueDiagnosticDataCollection";

        // ------------------------------------------------------------------
        // GDPR-specific consent types
        // ------------------------------------------------------------------

        /// <summary>Consent for GDPR data processing.</summary>
        public const string GDPRDataProcessing           = "GDPRDataProcessing";
        /// <summary>Consent for GDPR data transfer outside the EU.</summary>
        public const string GDPRDataTransferOutsideEU    = "GDPRDataTransferOutsideEU";
        /// <summary>Consent for GDPR automated decision-making.</summary>
        public const string GDPRAutomatedDecisionMaking  = "GDPRAutomatedDecisionMaking";

        // ------------------------------------------------------------------
        // EU AI Act-specific consent types
        // ------------------------------------------------------------------

        /// <summary>Consent for EU AI Act high-risk system usage.</summary>
        public const string EUAIActHighRiskSystem        = "EUAIActHighRiskSystem";
        /// <summary>Consent for EU AI Act biometric identification.</summary>
        public const string EUAIActBiometricIdentification = "EUAIActBiometricIdentification";
    }
}


// --- Port Interface ---
namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports
{
    using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;

    /// <summary>
    /// Defines the contract for the Consent Port in the BusinessApplications Layer.
    /// This port is the primary entry point for managing user consent for data sharing,
    /// notifications, automated actions, and **legally-binding consent** required by
    /// regulatory frameworks (GDPR, EU AI Act, HIPAA, etc.), adhering to the Hexagonal
    /// Architecture pattern.
    /// </summary>
    public interface IConsentPort
    {
        /// <summary>
        /// Records a user's consent decision. This action must be fully audited.
        /// </summary>
        /// <param name="request">The request containing the details of the consent decision.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// created <see cref="ConsentRecord"/>.
        /// </returns>
        /// <remarks>
        /// **Acceptance Criteria:** Given a user action, when initiated, the consent flow is logged before the action proceeds.
        /// </remarks>
        Task<ConsentRecord> RecordConsentAsync(ConsentRequest request);

        /// <summary>
        /// Validates if a user has granted a specific consent. This must be called before
        /// any consent-gated action is performed.
        /// </summary>
        /// <param name="request">The request specifying the consent to validate.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the
        /// <see cref="ValidateConsentResponse"/> indicating if consent is granted.
        /// </returns>
        Task<ValidateConsentResponse> ValidateConsentAsync(ValidateConsentRequest request);

        /// <summary>
        /// Retrieves all consent records for a specific user within a tenant.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an
        /// enumerable collection of the user's <see cref="ConsentRecord"/>s.
        /// </returns>
        Task<IEnumerable<ConsentRecord>> GetUserConsentsAsync(string userId, string tenantId);

        /// <summary>
        /// Revokes a previously granted consent. This is an idempotent operation.
        /// </summary>
        /// <param name="userId">The ID of the user revoking consent.</param>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <param name="consentType">The type of consent to revoke.</param>
        /// <param name="scope">The optional scope of the consent to revoke.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is true if the revocation was successful.</returns>
        Task<bool> RevokeConsentAsync(string userId, string tenantId, string consentType, string? scope = null);
    }
}
