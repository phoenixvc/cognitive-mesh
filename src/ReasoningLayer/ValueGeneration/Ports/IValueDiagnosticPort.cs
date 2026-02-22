// --- DTOs and Models for the Value Diagnostic Port ---

namespace CognitiveMesh.ReasoningLayer.ValueGeneration.Ports;

/// <summary>
/// Contains provenance and consent metadata that must accompany every request,
/// ensuring compliance with the Global NFR Appendix.
/// </summary>
public class ProvenanceContext
{
    /// <summary>Gets or sets the tenant identifier for multi-tenant isolation.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the actor initiating the request.</summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the consent record authorizing this action.</summary>
    public string ConsentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier for distributed tracing.</summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Represents a request to run a value diagnostic for a user or team.
/// </summary>
public class ValueDiagnosticRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;

    /// <summary>Gets or sets the identifier of the target to diagnose (a UserId or TeamId).</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of the target: "User" or "Team".</summary>
    public string TargetType { get; set; } = string.Empty;
}

/// <summary>
/// The result of a value diagnostic, including scores and actionable insights.
/// </summary>
public class ValueDiagnosticResponse
{
    /// <summary>Gets or sets the identifier of the diagnosed target.</summary>
    public string TargetId { get; set; } = string.Empty;

    /// <summary>Gets or sets the computed value score (e.g., the "$200 Test" score).</summary>
    public double ValueScore { get; set; }

    /// <summary>Gets or sets the value profile classification (e.g., "Innovator", "Stabilizer", "Connector").</summary>
    public string ValueProfile { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of identified strengths.</summary>
    public List<string> Strengths { get; set; } = new();

    /// <summary>Gets or sets the list of development opportunities for improvement.</summary>
    public List<string> DevelopmentOpportunities { get; set; } = new();

    /// <summary>Gets or sets the version of the model used to produce this diagnostic.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier for tracing this operation.</summary>
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