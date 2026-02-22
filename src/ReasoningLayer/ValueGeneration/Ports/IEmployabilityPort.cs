// --- DTOs and Models for the Employability Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Represents a request to check the employability risk of a user.
/// This is a sensitive operation that requires explicit consent.
/// </summary>
public class EmployabilityCheckRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;

    /// <summary>Gets or sets the unique identifier of the user to assess.</summary>
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// The result of an employability check, including a risk score and contributing factors.
/// </summary>
public class EmployabilityCheckResponse
{
    /// <summary>Gets or sets the unique identifier of the assessed user.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Gets or sets the employability risk score, from 0.0 (low risk) to 1.0 (high risk).</summary>
    public double EmployabilityRiskScore { get; set; }

    /// <summary>Gets or sets the risk level classification: "Low", "Medium", or "High".</summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of identified risk factors contributing to the score.</summary>
    public List<string> RiskFactors { get; set; } = new();

    /// <summary>Gets or sets the list of recommended actions to mitigate employability risk.</summary>
    public List<string> RecommendedActions { get; set; } = new();

    /// <summary>Gets or sets the version of the model used to produce this assessment.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier for tracing this operation.</summary>
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