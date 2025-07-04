using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.BusinessApplications.Common.Models;

namespace CognitiveMesh.BusinessApplications.Compliance.Ports.Models
{
    #region Core Enums and Models

    /// <summary>
    /// Defines the risk levels for AI systems as specified in the EU AI Act.
    /// </summary>
    public enum AIRiskLevel
    {
        /// <summary>
        /// AI systems that pose a clear threat to the safety, livelihoods, and rights of people (e.g., social scoring).
        /// These systems are banned under the EU AI Act.
        /// Corresponds to EU AI Act, Title II, Article 5.
        /// </summary>
        Unacceptable,

        /// <summary>
        /// AI systems identified in Annex III of the Act, subject to strict requirements including conformity assessments.
        /// Examples include systems for credit scoring, recruitment, or critical infrastructure.
        /// Corresponds to EU AI Act, Title III, Chapter 2.
        /// </summary>
        High,

        /// <summary>
        /// AI systems subject to specific transparency obligations, such as chatbots or systems generating deepfakes.
        /// Corresponds to EU AI Act, Title IV, Article 52.
        /// </summary>
        Limited,

        /// <summary>
        /// AI systems that pose minimal or no risk. The vast majority of AI systems fall into this category.
        /// These are not subject to specific obligations under the Act but are encouraged to follow voluntary codes of conduct.
        /// </summary>
        Minimal
    }

    /// <summary>
    /// Represents a request to classify an AI system according to the EU AI Act risk framework.
    /// </summary>
    public class RiskClassificationRequest
    {
        public string SystemName { get; set; }
        public string SystemVersion { get; set; }
        public string IntendedPurpose { get; set; }
        public List<string> DataSources { get; set; } = new List<string>();
        public List<string> UserDemographics { get; set; } = new List<string>();
        public string TenantId { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from a risk classification request.
    /// </summary>
    public class RiskClassificationResponse
    {
        public string SystemName { get; set; }
        public string SystemVersion { get; set; }
        public AIRiskLevel RiskLevel { get; set; }
        public string Justification { get; set; }
        public List<string> ApplicableArticles { get; set; } = new List<string>();
        public DateTimeOffset AssessedAt { get; set; }
        public string CorrelationId { get; set; }
    }

    /// <summary>
    /// Represents a request to initiate a conformity assessment for a high-risk AI system.
    /// </summary>
    public class ConformityAssessmentRequest
    {
        public Guid AgentId { get; set; }
        public string SystemVersion { get; set; }
        public string FrameworkVersion { get; set; } = "EU_AI_ACT_V1";
        public string RequestedBy { get; set; }
        public string TenantId { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the status and results of a conformity assessment.
    /// </summary>
    public class ConformityAssessment
    {
        public string AssessmentId { get; set; }
        public Guid AgentId { get; set; }
        public string Status { get; set; } // e.g., "InProgress", "Completed", "Failed"
        public string Outcome { get; set; } // e.g., "Compliant", "NonCompliant"
        public List<string> Findings { get; set; } = new List<string>();
        public string EvidenceLocation { get; set; }
        public DateTimeOffset AssessedAt { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
    }

    /// <summary>
    /// Represents a request to check if an AI system meets its transparency obligations.
    /// </summary>
    public class TransparencyCheckRequest
    {
        public Guid AgentId { get; set; }
        public string SystemType { get; set; } // e.g., "Chatbot", "DeepfakeGenerator", "EmotionRecognition"
        public string DisclosureContent { get; set; }
        public string TenantId { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from a transparency obligation check.
    /// </summary>
    public class TransparencyCheckResponse
    {
        public bool IsCompliant { get; set; }
        public List<string> Violations { get; set; } = new List<string>();
        public string ApplicableArticle { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
    }

    /// <summary>
    /// Represents a request to register a high-risk AI system in the EU database.
    /// </summary>
    public class EUDatabaseRegistrationRequest
    {
        public Guid AgentId { get; set; }
        public string SystemName { get; set; }
        public string ProviderInfo { get; set; }
        public string ConformityAssessmentId { get; set; }
        public string InstructionsForUseUrl { get; set; }
        public string TenantId { get; set; }
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Represents the response from an EU database registration request.
    /// </summary>
    public class EUDatabaseRegistrationResponse
    {
        public bool IsSuccess { get; set; }
        public string RegistrationId { get; set; }
        public string Status { get; set; } // e.g., "Submitted", "Registered", "Failed"
        public DateTimeOffset RegisteredAt { get; set; }
        public ErrorEnvelope Error { get; set; }
    }

    #endregion
}

namespace CognitiveMesh.BusinessApplications.Compliance.Ports
{
    using CognitiveMesh.BusinessApplications.Compliance.Ports.Models;

    /// <summary>
    /// Defines the contract for the EU AI Act Compliance Port.
    /// This port provides methods for classifying AI systems by risk, managing conformity assessments,
    /// ensuring transparency, and handling registration in the EU database, as required by the EU AI Act.
    /// </summary>
    public interface IEUAIActCompliancePort
    {
        /// <summary>
        /// Classifies an AI system according to the risk-based approach of the EU AI Act.
        /// </summary>
        /// <param name="request">The request containing details of the AI system to classify.</param>
        /// <returns>The risk classification and justification for the decision.</returns>
        /// <remarks>
        /// This method implements the logic defined in EU AI Act Title III and Annexes II & III.
        /// It determines if a system is Unacceptable, High-Risk, Limited-Risk, or Minimal-Risk.
        /// </remarks>
        Task<RiskClassificationResponse> ClassifySystemRiskAsync(RiskClassificationRequest request);

        /// <summary>
        /// Initiates a conformity assessment for a high-risk AI system to ensure it meets the Act's requirements.
        /// </summary>
        /// <param name="request">The request to start a conformity assessment.</param>
        /// <returns>The initial status and details of the assessment.</returns>
        /// <remarks>
        /// This method corresponds to the obligations for high-risk systems under EU AI Act, Chapter 3.
        /// The assessment covers areas like risk management, data governance, technical documentation, and human oversight.
        /// </remarks>
        Task<ConformityAssessment> RequestConformityAssessmentAsync(ConformityAssessmentRequest request);

        /// <summary>
        /// Retrieves the current status and results of a specific conformity assessment.
        /// </summary>
        /// <param name="assessmentId">The unique identifier of the assessment.</param>
        /// <param name="tenantId">The tenant ID to which the assessment belongs.</param>
        /// <returns>The full conformity assessment record if found; otherwise, null.</returns>
        Task<ConformityAssessment> GetConformityAssessmentStatusAsync(string assessmentId, string tenantId);

        /// <summary>
        /// Ensures that an AI system meets its specific transparency obligations.
        /// </summary>
        /// <param name="request">The request containing details of the system and its disclosure content.</param>
        /// <returns>A response indicating compliance and any identified violations.</returns>
        /// <remarks>
        /// This method enforces the requirements of EU AI Act, Article 52, which mandates that users
        /// must be informed when they are interacting with an AI system (like a chatbot) or being exposed
        /// to AI-generated content (like a deepfake).
        /// </remarks>
        Task<TransparencyCheckResponse> EnsureTransparencyObligationsAsync(TransparencyCheckRequest request);

        /// <summary>
        /// Registers a high-risk AI system in the public EU database.
        /// </summary>
        /// <param name="request">The request containing the necessary information for registration.</param>
        /// <returns>A response indicating the success and status of the registration.</returns>
        /// <remarks>
        /// This method implements the registration requirement for high-risk systems as specified in
        /// EU AI Act, Article 60. Registration is a prerequisite for placing a high-risk system on the market.
        /// </remarks>
        Task<EUDatabaseRegistrationResponse> RegisterHighRiskSystemAsync(EUDatabaseRegistrationRequest request);
    }
}
