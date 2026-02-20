// --- DTOs and Models for the Value Diagnostic Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Contains provenance and consent metadata that must accompany every request,
/// ensuring compliance with the Global NFR Appendix.
/// </summary>
public class ProvenanceContext
{
    public string TenantId { get; set; } = string.Empty;
    public string ActorId { get; set; } = string.Empty;
    public string ConsentId { get; set; } = string.Empty; // ID of the consent record for this action
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Represents a request to run a value diagnostic for a user or team.
/// </summary>
public class ValueDiagnosticRequest
{
    public ProvenanceContext Provenance { get; set; } = default!;
    public string TargetId { get; set; } = string.Empty; // Can be a UserId or TeamId
    public string TargetType { get; set; } = string.Empty; // "User" or "Team"
}

/// <summary>
/// The result of a value diagnostic, including scores and actionable insights.
/// </summary>
public class ValueDiagnosticResponse
{
    public string TargetId { get; set; } = string.Empty;
    public double ValueScore { get; set; } // e.g., the "$200 Test" score
    public string ValueProfile { get; set; } = string.Empty; // e.g., "Innovator", "Stabilizer", "Connector"
    public List<string> Strengths { get; set; } = new();
    public List<string> DevelopmentOpportunities { get; set; } = new();
    public string ModelVersion { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
}

// --- Port Interface ---
/// <summary>
/// Defines the contract for the Value Diagnostic Port in the Reasoning Layer.
/// This port is the entry point for value diagnostics that assess the value generation
/// profile of users or teams, adhering to the Hexagonal Architecture pattern.
/// </summary>
public interface IValueDiagnosticPort
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
}