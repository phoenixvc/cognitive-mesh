// --- DTOs and Models for the Value Generation Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Contains provenance and consent metadata that must accompany every request,
/// ensuring compliance with the Global NFR Appendix.
/// </summary>
public class ProvenanceContext
{
    public string TenantId { get; set; }
    public string ActorId { get; set; }
    public string ConsentId { get; set; } // ID of the consent record for this action
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

// --- Value Diagnostic DTOs ---

/// <summary>
/// Represents a request to run a value diagnostic for a user or team.
/// </summary>
public class ValueDiagnosticRequest
{
    public ProvenanceContext Provenance { get; set; }
    public string TargetId { get; set; } // Can be a UserId or TeamId
    public string TargetType { get; set; } // "User" or "Team"
}

/// <summary>
/// The result of a value diagnostic, including scores and actionable insights.
/// </summary>
public class ValueDiagnosticResponse
{
    public string TargetId { get; set; }
    public double ValueScore { get; set; } // e.g., the "$200 Test" score
    public string ValueProfile { get; set; } // e.g., "Innovator", "Stabilizer", "Connector"
    public List<string> Strengths { get; set; } = new();
    public List<string> DevelopmentOpportunities { get; set; } = new();
    public string ModelVersion { get; set; }
    public string CorrelationId { get; set; }
}

// --- Organizational Blindness DTOs ---

/// <summary>
/// Represents a request to detect organizational value blindness.
/// </summary>
public class OrgBlindnessDetectionRequest
{
    public ProvenanceContext Provenance { get; set; }
    public string OrganizationId { get; set; }
    public string[] DepartmentFilters { get; set; }
}

/// <summary>
/// The result of an organizational blindness analysis, highlighting areas where value is overlooked.
/// </summary>
public class OrgBlindnessDetectionResponse
{
    public string OrganizationId { get; set; }
    public double BlindnessRiskScore { get; set; } // Score from 0.0 (no risk) to 1.0 (high risk)
    public List<string> IdentifiedBlindSpots { get; set; } = new(); // e.g., "Undervaluing maintenance work", "Overlooking quiet contributors"
    public string ModelVersion { get; set; }
    public string CorrelationId { get; set; }
}

// --- Employability DTOs ---

/// <summary>
/// Represents a request to check the employability risk of a user.
/// This is a sensitive operation that requires explicit consent.
/// </summary>
public class EmployabilityCheckRequest
{
    public ProvenanceContext Provenance { get; set; }
    public string UserId { get; set; }
}

/// <summary>
/// The result of an employability check, including a risk score and contributing factors.
/// </summary>
public class EmployabilityCheckResponse
{
    public string UserId { get; set; }
    public double EmployabilityRiskScore { get; set; } // Score from 0.0 (low risk) to 1.0 (high risk)
    public string RiskLevel { get; set; } // "Low", "Medium", "High"
    public List<string> RiskFactors { get; set; } = new(); // e.g., "Skills mismatch with market trends", "Low creative output"
    public List<string> RecommendedActions { get; set; } = new(); // e.g., "Explore training for 'AI Prompt Engineering'", "Engage in more cross-functional projects"
    public string ModelVersion { get; set; }
    public string CorrelationId { get; set; }
}


// --- Port Interface ---
/// <summary>
/// Defines the contract for the Value Generation Port in the Reasoning Layer.
/// This port is the primary entry point for all value diagnostics, organizational blindness detection,
/// and employability risk analysis, adhering to the Hexagonal Architecture pattern.
/// </summary>
public interface IValueGenerationPort
{
    /// <summary>
    /// Runs a diagnostic to assess the value generation profile of a user or team.
    /// </summary>
    /// <param name="request">The request containing the target for the diagnostic.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the value diagnostic scores and insights.</returns>
    /// <remarks>
    /// **Priority:** Must
    /// **SLA:** This operation must return a result in less than 300ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given a valid payload, When submitted, Then respond 200 with a result in <300ms and log the audit event.
    /// </remarks>
    Task<ValueDiagnosticResponse> RunValueDiagnosticAsync(ValueDiagnosticRequest request);

    /// <summary>
    /// Analyzes organizational data to detect "value blindness" - areas where value creation is overlooked or undervalued.
    /// </summary>
    /// <param name="request">The request specifying the organization or departments to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the blindness risk score and identified blind spots.</returns>
    /// <remarks>
    /// **Priority:** Must
    /// **SLA:** This operation must return a result in less than 250ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given a valid org diagnostic payload, When submitted, Then respond 200 with a result in <250ms and log the audit event.
    /// </remarks>
    Task<OrgBlindnessDetectionResponse> DetectOrganizationalBlindnessAsync(OrgBlindnessDetectionRequest request);

    /// <summary>
    /// Assesses an individual's employability risk based on their skills, creativity, and market trends.
    /// This is a sensitive HR operation and must be gated by the ConsentPort.
    /// </summary>
    /// <param name="request">The request containing the user to be assessed.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the employability risk analysis.</returns>
    /// <remarks>
    /// **Priority:** Must
    /// **SLA:** This operation must return a result in less than 350ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given an employability check request with missing consent, When submitted, Then the operation must be rejected with a 403/CONSENT_MISSING error.
    /// </remarks>
    Task<EmployabilityCheckResponse> CheckEmployabilityAsync(EmployabilityCheckRequest request);
}