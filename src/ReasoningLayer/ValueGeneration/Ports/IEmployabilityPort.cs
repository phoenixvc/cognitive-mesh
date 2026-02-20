// --- DTOs and Models for the Employability Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Represents a request to check the employability risk of a user.
/// This is a sensitive operation that requires explicit consent.
/// </summary>
public class EmployabilityCheckRequest
{
    public ProvenanceContext Provenance { get; set; } = default!;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// The result of an employability check, including a risk score and contributing factors.
/// </summary>
public class EmployabilityCheckResponse
{
    public string UserId { get; set; } = string.Empty;
    public double EmployabilityRiskScore { get; set; } // Score from 0.0 (low risk) to 1.0 (high risk)
    public string RiskLevel { get; set; } = string.Empty; // "Low", "Medium", "High"
    public List<string> RiskFactors { get; set; } = new(); // e.g., "Skills mismatch with market trends", "Low creative output"
    public List<string> RecommendedActions { get; set; } = new(); // e.g., "Explore training for 'AI Prompt Engineering'", "Engage in more cross-functional projects"
    public string ModelVersion { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

// --- Port Interface ---
/// <summary>
/// Defines the contract for the Employability Port in the Reasoning Layer.
/// This port is responsible for assessing an individual's employability risk based on their skills,
/// creativity, and market trends. As a sensitive HR operation, it requires explicit consent and
/// may be subject to manual review before results are released.
/// </summary>
public interface IEmployabilityPort
{
    /// <summary>
    /// Assesses an individual's employability risk based on their skills, creativity, and market trends.
    /// This is a sensitive HR operation and must be gated by the ConsentPort with the "EmployabilityAnalysis" consent type.
    /// For high-risk assessments, manual review via the ManualAdjudicationPort may be required before results are released.
    /// </summary>
    /// <param name="request">The request containing the user to be assessed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the employability risk analysis.</returns>
    /// <remarks>
    /// **Priority:** Must
    /// **SLA:** This operation must return a result in less than 350ms (P99).
    /// **Security:** Requires "EmployabilityAnalysis" consent and appropriate data access policies.
    /// **Compliance:** All assessments must be logged for audit purposes with user consent evidence.
    /// **Acceptance Criteria (G/W/T):** 
    /// - Given an employability check with missing consent, When submitted, Then respond 403/error_code=CONSENT_MISSING, log attempt.
    /// - Given a flagged employability user, When case submitted, Then require explicit human review/approval before data release.
    /// </remarks>
    Task<EmployabilityCheckResponse> CheckEmployabilityAsync(EmployabilityCheckRequest request);
}