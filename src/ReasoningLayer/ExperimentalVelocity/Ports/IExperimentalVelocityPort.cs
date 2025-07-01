using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// --- DTOs for the Experimental Velocity Port ---
namespace CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports.Models
{
    /// <summary>
    /// Contains provenance and consent metadata that must accompany every request.
    /// This ensures compliance with the Global NFR Appendix.
    /// </summary>
    public class ProvenanceContext
    {
        public string TenantId { get; set; }
        public string ActorId { get; set; }
        public string ConsentId { get; set; } // ID of the consent record for this action
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }

    // --- Recalibration DTOs ---

    /// <summary>
    /// Represents a request to recalibrate a project's estimated velocity.
    /// </summary>
    public class VelocityRecalibrationRequest
    {
        public ProvenanceContext Provenance { get; set; }
        public string ProjectId { get; set; }
        public string ProjectDescription { get; set; }
        public double OriginalEffortHours { get; set; }
        public double OriginalCost { get; set; }
    }

    /// <summary>
    /// Represents the result of a velocity recalibration, applying the "divide by 100" logic.
    /// </summary>
    public class VelocityRecalibrationResponse
    {
        public string ProjectId { get; set; }
        public double OriginalEffortHours { get; set; }
        public double RecalibratedEffortHours { get; set; }
        public double OriginalCost { get; set; }
        public double RecalibratedCost { get; set; }
        public string Explanation { get; set; }
        public string ModelVersion { get; set; }
        public string CorrelationId { get; set; }
    }

    // --- Innovation Theater DTOs ---

    /// <summary>
    /// Represents a request to analyze a project for signs of "innovation theater".
    /// </summary>
    public class TheaterDetectionRequest
    {
        public ProvenanceContext Provenance { get; set; }
        public string ProjectId { get; set; }
        public Dictionary<string, object> ProjectMetadata { get; set; } // e.g., meeting_count, decision_velocity, stakeholder_alignment_score
    }

    /// <summary>
    /// Represents the result of an innovation theater analysis.
    /// </summary>
    public class TheaterDetectionResponse
    {
        public string ProjectId { get; set; }
        public double TheaterRiskScore { get; set; } // A score from 0.0 (no risk) to 1.0 (high risk)
        public string RiskLevel { get; set; } // "Low", "Medium", "High"
        public List<string> ContributingFactors { get; set; } = new List<string>();
        public string ModelVersion { get; set; }
        public string CorrelationId { get; set; }
    }

    // --- Competitive Reality Check DTOs ---

    /// <summary>
    /// Represents a request to benchmark a project against competitive realities.
    /// </summary>
    public class CompetitiveRealityCheckRequest
    {
        public ProvenanceContext Provenance { get; set; }
        public string ProjectId { get; set; }
        public string IndustrySector { get; set; }
        public Dictionary<string, double> CurrentProjectMetrics { get; set; } // e.g., time_to_market, feature_velocity
    }

    /// <summary>
    /// Represents the result of a competitive reality check.
    /// </summary>
    public class CompetitiveRealityCheckResponse
    {
        public string ProjectId { get; set; }
        public bool IsAtCompetitiveRisk { get; set; }
        public string RiskSummary { get; set; }
        public Dictionary<string, double> GapAnalysis { get; set; } = new Dictionary<string, double>(); // Key: metric, Value: gap percentage
        public string ModelVersion { get; set; }
        public string CorrelationId { get; set; }
    }
}


// --- Port Interface ---
namespace CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports
{
    using CognitiveMesh.ReasoningLayer.ExperimentalVelocity.Ports.Models;

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
}
