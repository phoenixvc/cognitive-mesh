// --- DTOs for the Experimental Velocity Port ---

namespace CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports;

/// <summary>
/// Contains provenance and consent metadata that must accompany every request.
/// This ensures compliance with the Global NFR Appendix.
/// </summary>
public class ProvenanceContext
{
    /// <summary>Gets or sets the tenant identifier for multi-tenancy isolation.</summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>Gets or sets the actor identifier representing the user or system performing the action.</summary>
    public string ActorId { get; set; } = string.Empty;

    /// <summary>Gets or sets the identifier of the consent record authorizing this action.</summary>
    public string ConsentId { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier used to trace requests across services.</summary>
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
}

// --- Recalibration DTOs ---

/// <summary>
/// Represents a request to recalibrate a project's estimated velocity.
/// </summary>
public class VelocityRecalibrationRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;

    /// <summary>Gets or sets the unique identifier of the project to recalibrate.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the textual description of the project.</summary>
    public string ProjectDescription { get; set; } = string.Empty;

    /// <summary>Gets or sets the original estimated effort in hours before recalibration.</summary>
    public double OriginalEffortHours { get; set; }

    /// <summary>Gets or sets the original estimated cost before recalibration.</summary>
    public double OriginalCost { get; set; }
}

/// <summary>
/// Represents the result of a velocity recalibration, applying the "divide by 100" logic.
/// </summary>
public class VelocityRecalibrationResponse
{
    /// <summary>Gets or sets the unique identifier of the recalibrated project.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the original estimated effort in hours before recalibration.</summary>
    public double OriginalEffortHours { get; set; }

    /// <summary>Gets or sets the recalibrated effort in hours after applying the velocity model.</summary>
    public double RecalibratedEffortHours { get; set; }

    /// <summary>Gets or sets the original estimated cost before recalibration.</summary>
    public double OriginalCost { get; set; }

    /// <summary>Gets or sets the recalibrated cost after applying the velocity model.</summary>
    public double RecalibratedCost { get; set; }

    /// <summary>Gets or sets the human-readable explanation of how the recalibration was performed.</summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>Gets or sets the version of the model used for recalibration, for provenance tracking.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier linking this response to the originating request.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}

// --- Innovation Theater DTOs ---

/// <summary>
/// Represents a request to analyze a project for signs of "innovation theater".
/// </summary>
public class TheaterDetectionRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;

    /// <summary>Gets or sets the unique identifier of the project to analyze.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the project metadata used for theater detection (e.g., meeting_count, decision_velocity, stakeholder_alignment_score).</summary>
    public Dictionary<string, object> ProjectMetadata { get; set; } = new();
}

/// <summary>
/// Represents the result of an innovation theater analysis.
/// </summary>
public class TheaterDetectionResponse
{
    /// <summary>Gets or sets the unique identifier of the analyzed project.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the theater risk score, ranging from 0.0 (no risk) to 1.0 (high risk).</summary>
    public double TheaterRiskScore { get; set; }

    /// <summary>Gets or sets the categorical risk level: "Low", "Medium", or "High".</summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>Gets or sets the list of factors contributing to the theater risk score, ordered by impact.</summary>
    public List<string> ContributingFactors { get; set; } = new List<string>();

    /// <summary>Gets or sets the version of the model used for detection, for provenance tracking.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier linking this response to the originating request.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}

// --- Competitive Reality Check DTOs ---

/// <summary>
/// Represents a request to benchmark a project against competitive realities.
/// </summary>
public class CompetitiveRealityCheckRequest
{
    /// <summary>Gets or sets the provenance context containing tenant, actor, and consent metadata.</summary>
    public ProvenanceContext Provenance { get; set; } = default!;

    /// <summary>Gets or sets the unique identifier of the project to benchmark.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets the industry sector used to select competitive benchmarks.</summary>
    public string IndustrySector { get; set; } = string.Empty;

    /// <summary>Gets or sets the current project metrics to compare against industry benchmarks (e.g., time_to_market, feature_velocity).</summary>
    public Dictionary<string, double> CurrentProjectMetrics { get; set; } = new();
}

/// <summary>
/// Represents the result of a competitive reality check.
/// </summary>
public class CompetitiveRealityCheckResponse
{
    /// <summary>Gets or sets the unique identifier of the benchmarked project.</summary>
    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the project is at competitive risk relative to industry benchmarks.</summary>
    public bool IsAtCompetitiveRisk { get; set; }

    /// <summary>Gets or sets a human-readable summary of the competitive risk assessment.</summary>
    public string RiskSummary { get; set; } = string.Empty;

    /// <summary>Gets or sets the gap analysis mapping each metric name to its gap percentage relative to the benchmark.</summary>
    public Dictionary<string, double> GapAnalysis { get; set; } = new Dictionary<string, double>();

    /// <summary>Gets or sets the version of the model used for the reality check, for provenance tracking.</summary>
    public string ModelVersion { get; set; } = string.Empty;

    /// <summary>Gets or sets the correlation identifier linking this response to the originating request.</summary>
    public string CorrelationId { get; set; } = string.Empty;
}


// --- Port Interface ---
/// <summary>
/// Defines the contract for the Experimental Velocity Port in the Reasoning Layer.
/// This port is the primary entry point for all velocity recalibration, innovation theater detection,
/// and competitive reality check logic, adhering to the Hexagonal Architecture pattern.
/// </summary>
public interface IExperimentalVelocityPort
{
    /// <summary>
    /// Recalibrates a project's estimated velocity using the "divide by 100" logic.
    /// This is a high-priority (Must) operation with a strict performance SLA.
    /// </summary>
    /// <param name="request">The request containing the project briefing to be recalibrated.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the recalibrated estimates.</returns>
    /// <remarks>
    /// **SLA:** This operation must return a result in less than 200ms (P99).
    /// **Acceptance Criteria:** Given a project briefing, the engine must return a recalibrated estimate.
    /// </remarks>
    Task<VelocityRecalibrationResponse> RecalibrateAsync(VelocityRecalibrationRequest request);

    /// <summary>
    /// Analyzes project metadata to detect signs of "innovation theater".
    /// This is a high-priority (Must) operation with a very strict performance SLA.
    /// </summary>
    /// <param name="request">The request containing project metadata for analysis.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the theater risk score and contributing factors.</returns>
    /// <remarks>
    /// **SLA:** This operation must return a result in less than 100ms (P99).
    /// **Acceptance Criteria:** Given process metadata, the engine returns a score and cause analysis.
    /// </remarks>
    Task<TheaterDetectionResponse> DetectInnovationTheaterAsync(TheaterDetectionRequest request);

    /// <summary>
    /// Benchmarks a project's current state against AI-enabled competitors to identify competitive risks.
    /// This is a secondary-priority (Should) operation.
    /// </summary>
    /// <param name="request">The request containing the project's current metrics and industry sector.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the competitive gap analysis.</returns>
    /// <remarks>
    /// **SLA:** This operation must return a result in less than 300ms (P95).
    /// **Acceptance Criteria:** Given metrics, the engine returns the time/capacity/velocity gap.
    /// </remarks>
    Task<CompetitiveRealityCheckResponse> PerformRealityCheckAsync(CompetitiveRealityCheckRequest request);
}