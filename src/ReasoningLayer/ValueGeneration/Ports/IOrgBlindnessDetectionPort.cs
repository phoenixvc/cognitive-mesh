// --- DTOs and Models for the Organizational Blindness Detection Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Represents a request to detect organizational value blindness.
/// </summary>
public class OrgBlindnessDetectionRequest
{
    public ProvenanceContext Provenance { get; set; } = default!;
    public string OrganizationId { get; set; } = string.Empty;
    public string[] DepartmentFilters { get; set; } = [];
}

/// <summary>
/// The result of an organizational blindness analysis, highlighting areas where value is overlooked.
/// </summary>
public class OrgBlindnessDetectionResponse
{
    public string OrganizationId { get; set; } = string.Empty;
    public double BlindnessRiskScore { get; set; } // Score from 0.0 (no risk) to 1.0 (high risk)
    public List<string> IdentifiedBlindSpots { get; set; } = new(); // e.g., "Undervaluing maintenance work", "Overlooking quiet contributors"
    public string ModelVersion { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

// --- Port Interface ---
/// <summary>
/// Defines the contract for the Organizational Blindness Detection Port in the Reasoning Layer.
/// This port is responsible for analyzing organizational data to detect areas where value creation
/// is overlooked or undervalued, adhering to the Hexagonal Architecture pattern.
/// </summary>
public interface IOrgBlindnessDetectionPort
{
    /// <summary>
    /// Analyzes organizational data to detect "value blindness" - areas where value creation is overlooked or undervalued.
    /// </summary>
    /// <param name="request">The request specifying the organization or departments to analyze.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the blindness risk score and identified blind spots.</returns>
    /// <remarks>
    /// **Priority:** Must
    /// **SLA:** This operation must return a result in less than 250ms (P99).
    /// **Acceptance Criteria (G/W/T):** Given a valid org diagnostic payload, When submitted, Then respond 200 with a result in &lt;250ms and log the audit event.
    /// </remarks>
    Task<OrgBlindnessDetectionResponse> DetectOrganizationalBlindnessAsync(OrgBlindnessDetectionRequest request);
}