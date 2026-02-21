// --- Port Interface for the Value Generation Port ---
// DTOs are defined in their respective port files:
//   IValueDiagnosticPort.cs — ProvenanceContext, ValueDiagnosticRequest, ValueDiagnosticResponse
//   IOrgBlindnessDetectionPort.cs — OrgBlindnessDetectionRequest, OrgBlindnessDetectionResponse
//   IEmployabilityPort.cs — EmployabilityCheckRequest, EmployabilityCheckResponse

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

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
    /// **Acceptance Criteria (G/W/T):** Given a valid payload, When submitted, Then respond 200 with a result in &lt;300ms and log the audit event.
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
    /// **Acceptance Criteria (G/W/T):** Given a valid org diagnostic payload, When submitted, Then respond 200 with a result in &lt;250ms and log the audit event.
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