using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.BusinessApplications.Common.Models;

namespace CognitiveMesh.BusinessApplications.Compliance.Ports.Models
{
    #region Data Subject Rights Models

    /// <summary>
    /// Base class for all Data Subject Right (DSR) requests.
    /// </summary>
    public abstract class DataSubjectRightRequest
    {
        /// <summary>
        /// The unique identifier of the data subject making the request.
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// The tenant ID in which the request is made.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// A token or evidence used to verify the identity of the data subject.
        /// </summary>
        public string IdentityVerificationToken { get; set; }

        /// <summary>
        /// The user or system that initiated the request.
        /// </summary>
        public string RequestedBy { get; set; }

        /// <summary>
        /// A correlation ID for tracing the request across the system.
        /// </summary>
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents a request for access to personal data (GDPR Article 15).
    /// </summary>
    public class DataSubjectAccessRequest : DataSubjectRightRequest { }

    /// <summary>
    /// Represents a request to rectify inaccurate personal data (GDPR Article 16).
    /// </summary>
    public class DataSubjectRectificationRequest : DataSubjectRightRequest
    {
        /// <summary>
        /// A dictionary containing the data fields to be corrected and their new values.
        /// </summary>
        public Dictionary<string, object> Rectifications { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Represents a request for the erasure of personal data (GDPR Article 17).
    /// </summary>
    public class DataSubjectErasureRequest : DataSubjectRightRequest { }

    /// <summary>
    /// Represents a request for data portability (GDPR Article 20).
    /// </summary>
    public class DataSubjectPortabilityRequest : DataSubjectRightRequest
    {
        /// <summary>
        /// The desired format for the portable data (e.g., "json", "csv").
        /// </summary>
        public string Format { get; set; } = "json";
    }

    /// <summary>
    /// Represents a request to object to a data processing activity (GDPR Article 21).
    /// </summary>
    public class DataSubjectObjectionRequest : DataSubjectRightRequest
    {
        /// <summary>
        /// The specific data processing activity being objected to.
        /// </summary>
        public string ProcessingActivity { get; set; }

        /// <summary>
        /// The grounds for the objection.
        /// </summary>
        public string GroundsForObjection { get; set; }
    }

    /// <summary>
    /// Represents the response to a Data Subject Right (DSR) request.
    /// </summary>
    public class DataSubjectRightResponse
    {
        public bool IsSuccess { get; set; }
        public string RequestType { get; set; }
        public string SubjectId { get; set; }
        public DateTimeOffset ProcessedAt { get; set; }
        public Dictionary<string, object> Data { get; set; }
        public ErrorEnvelope Error { get; set; }
    }

    #endregion

    #region Data Processing Assessment Models

    /// <summary>
    /// Represents a request to assess a data processing activity for GDPR compliance.
    /// </summary>
    public class DataProcessingAssessmentRequest
    {
        public string ActivityName { get; set; }
        public string ProcessingPurpose { get; set; }
        public List<string> DataCategories { get; set; } = new List<string>();
        public string LegalBasis { get; set; }
        public Dictionary<string, object> ProcessingDetails { get; set; } = new Dictionary<string, object>();
        public string RetentionPeriod { get; set; }
        public List<DataTransfer> DataTransfers { get; set; } = new List<DataTransfer>();
        public string TenantId { get; set; }
        public string AssessedBy { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from a data processing activity assessment.
    /// </summary>
    public class DataProcessingAssessmentResponse
    {
        public bool IsCompliant { get; set; }
        public string ActivityName { get; set; }
        public DateTimeOffset AssessedAt { get; set; }
        public List<ComplianceIssue> Issues { get; set; } = new List<ComplianceIssue>();
        public Dictionary<string, object> AssessmentDetails { get; set; }
        public ErrorEnvelope Error { get; set; }
    }

    #endregion

    #region GDPR Consent Models

    /// <summary>
    /// Represents a request to verify GDPR-compliant consent.
    /// </summary>
    public class ConsentVerificationRequest
    {
        public string SubjectId { get; set; }
        public string TenantId { get; set; }
        public string ConsentType { get; set; }
        public string ProcessingOperation { get; set; }
        public string VerifiedBy { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from a GDPR consent verification.
    /// </summary>
    public class ConsentVerificationResponse
    {
        public bool HasConsent { get; set; }
        public string SubjectId { get; set; }
        public string ConsentType { get; set; }
        public DateTimeOffset VerifiedAt { get; set; }
        public string ConsentRecordId { get; set; }
        public ErrorEnvelope Error { get; set; }
    }

    /// <summary>
    /// Represents a request to record GDPR-compliant consent.
    /// </summary>
    public class ConsentRecordRequest
    {
        public string SubjectId { get; set; }
        public string TenantId { get; set; }
        public string ConsentType { get; set; }
        public string ProcessingOperation { get; set; }
        public bool IsGranted { get; set; }
        public string Source { get; set; }
        public string Evidence { get; set; }
        public DateTimeOffset? ExpirationTime { get; set; }
        public string RecordedBy { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from a GDPR consent recording operation.
    /// </summary>
    public class ConsentRecordResponse
    {
        public bool IsSuccess { get; set; }
        public string SubjectId { get; set; }
        public string ConsentType { get; set; }
        public DateTimeOffset RecordedAt { get; set; }
        public string ConsentRecordId { get; set; }
        public bool IsGranted { get; set; }
        public ErrorEnvelope Error { get; set; }
    }

    #endregion
}

namespace CognitiveMesh.BusinessApplications.Compliance.Ports
{
    using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;

    /// <summary>
    /// Defines the contract for the GDPR Compliance Port in the BusinessApplications Layer.
    /// This port is responsible for enforcing GDPR principles, managing data subject rights,
    /// and assessing data processing activities for compliance.
    /// </summary>
    public interface IGDPRCompliancePort
    {
        #region Data Subject Rights

        /// <summary>
        /// Handles a data subject's request for access to their personal data.
        /// </summary>
        /// <param name="request">The data subject access request.</param>
        /// <returns>A response containing the subject's personal data.</returns>
        /// <remarks>Corresponds to GDPR Article 15 (Right of access by the data subject).</remarks>
        Task<DataSubjectRightResponse> HandleAccessRequestAsync(DataSubjectAccessRequest request);

        /// <summary>
        /// Handles a data subject's request to rectify inaccurate personal data.
        /// </summary>
        /// <param name="request">The data subject rectification request.</param>
        /// <returns>A response confirming the rectification.</returns>
        /// <remarks>Corresponds to GDPR Article 16 (Right to rectification).</remarks>
        Task<DataSubjectRightResponse> HandleRectificationRequestAsync(DataSubjectRectificationRequest request);

        /// <summary>
        /// Handles a data subject's request for the erasure of their personal data.
        /// </summary>
        /// <param name="request">The data subject erasure request.</param>
        /// <returns>A response confirming the erasure.</returns>
        /// <remarks>Corresponds to GDPR Article 17 (Right to erasure / 'right to be forgotten').</remarks>
        Task<DataSubjectRightResponse> HandleErasureRequestAsync(DataSubjectErasureRequest request);

        /// <summary>
        /// Handles a data subject's request for data portability.
        /// </summary>
        /// <param name="request">The data subject portability request.</param>
        /// <returns>A response containing the subject's data in a portable format.</returns>
        /// <remarks>Corresponds to GDPR Article 20 (Right to data portability).</remarks>
        Task<DataSubjectRightResponse> HandlePortabilityRequestAsync(DataSubjectPortabilityRequest request);

        /// <summary>
        /// Handles a data subject's objection to a data processing activity.
        /// </summary>
        /// <param name="request">The data subject objection request.</param>
        /// <returns>A response confirming the objection has been registered and acted upon.</returns>
        /// <remarks>Corresponds to GDPR Article 21 (Right to object).</remarks>
        Task<DataSubjectRightResponse> HandleObjectionRequestAsync(DataSubjectObjectionRequest request);

        #endregion

        #region Data Processing Assessment

        /// <summary>
        /// Assesses a data processing activity for compliance with GDPR principles.
        /// </summary>
        /// <param name="request">The request detailing the data processing activity.</param>
        /// <returns>A response containing the compliance assessment and any identified issues.</returns>
        /// <remarks>Corresponds to GDPR Article 5 (Principles relating to processing of personal data) and Article 6 (Lawfulness of processing).</remarks>
        Task<DataProcessingAssessmentResponse> AssessDataProcessingActivityAsync(DataProcessingAssessmentRequest request);

        #endregion

        #region GDPR Consent

        /// <summary>
        /// Verifies that a valid, GDPR-compliant consent has been given for a specific processing operation.
        /// </summary>
        /// <param name="request">The consent verification request.</param>
        /// <returns>A response indicating whether consent is valid.</returns>
        /// <remarks>Corresponds to GDPR Article 7 (Conditions for consent).</remarks>
        Task<ConsentVerificationResponse> VerifyGdprConsentAsync(ConsentVerificationRequest request);

        /// <summary>
        /// Records a GDPR-compliant consent decision.
        /// </summary>
        /// <param name="request">The consent record request.</param>
        /// <returns>A response confirming the consent has been recorded.</returns>
        /// <remarks>Corresponds to GDPR Article 7 (Conditions for consent).</remarks>
        Task<ConsentRecordResponse> RecordGdprConsentAsync(ConsentRecordRequest request);

        #endregion
    }
}
