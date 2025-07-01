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
        public string UserId { get; set; }

        /// <summary>
        /// The tenant ID to which this consent applies.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The specific type of consent being granted or denied (e.g., "NotifyOnProjectOpportunities", "AutoCreateCollaborationSpaces").
        /// This should be a well-known, documented string.
        /// </summary>
        public string ConsentType { get; set; }

        /// <summary>
        /// An optional identifier to narrow the scope of the consent (e.g., a specific project ID or channel ID).
        /// If null or empty, the consent is considered global for the given type.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// True if consent is being granted; false if it is being denied or revoked.
        /// </summary>
        public bool IsGranted { get; set; }

        /// <summary>
        /// The source of the consent action, used for auditing (e.g., "Widget:ChampionFinder", "UserProfileSettings").
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Optional evidence, such as a link to the version of the privacy policy or terms the user agreed to.
        /// </summary>
        public string Evidence { get; set; }
    }

    /// <summary>
    /// Represents a persisted record of a user's consent decision.
    /// This object serves as an immutable audit record.
    /// </summary>
    public class ConsentRecord
    {
        public string ConsentId { get; set; }
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string ConsentType { get; set; }
        public string Scope { get; set; }
        public bool IsGranted { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string Source { get; set; }
        public string Evidence { get; set; }
    }

    /// <summary>
    /// Represents a request to validate whether a user has given a specific type of consent.
    /// </summary>
    public class ValidateConsentRequest
    {
        public string UserId { get; set; }
        public string TenantId { get; set; }
        public string RequiredConsentType { get; set; }
        public string Scope { get; set; } // Optional scope to check
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
        public string ConsentRecordId { get; set; }
    }
}


// --- Port Interface ---
namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports
{
    using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;

    /// <summary>
    /// Defines the contract for the Consent Port in the BusinessApplications Layer.
    /// This port is the primary entry point for managing user consent for data sharing,
    /// notifications, and automated actions, adhering to the Hexagonal Architecture pattern.
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
        Task<bool> RevokeConsentAsync(string userId, string tenantId, string consentType, string scope = null);
    }
}
