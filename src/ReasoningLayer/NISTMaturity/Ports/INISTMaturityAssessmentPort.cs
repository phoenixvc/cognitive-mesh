using CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Ports;

/// <summary>
/// Defines the contract for the NIST AI RMF maturity assessment engine.
/// This port provides capabilities for evidence submission, statement scoring,
/// pillar-level aggregation, roadmap generation, and full assessment compilation.
/// </summary>
public interface INISTMaturityAssessmentPort
{
    /// <summary>
    /// Submits evidence for a NIST AI RMF maturity statement.
    /// Validates the evidence and persists it via the evidence store.
    /// </summary>
    /// <param name="evidence">The evidence to submit.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The submitted evidence with any server-side enrichments.</returns>
    Task<NISTEvidence> SubmitEvidenceAsync(NISTEvidence evidence, CancellationToken cancellationToken);

    /// <summary>
    /// Scores a specific NIST AI RMF statement based on available evidence.
    /// The score is derived from the quantity and quality of supporting evidence.
    /// </summary>
    /// <param name="statementId">The identifier of the statement to score.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The calculated maturity score for the statement.</returns>
    Task<MaturityScore> ScoreStatementAsync(string statementId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves aggregated pillar-level scores for an organization.
    /// Pillars are: Govern, Map, Measure, and Manage.
    /// </summary>
    /// <param name="organizationId">The organization to retrieve scores for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of pillar scores.</returns>
    Task<IReadOnlyList<PillarScore>> GetPillarScoresAsync(string organizationId, CancellationToken cancellationToken);

    /// <summary>
    /// Generates an improvement roadmap identifying gaps and recommended actions.
    /// Gaps are prioritized based on the difference between current and target scores.
    /// </summary>
    /// <param name="organizationId">The organization to generate the roadmap for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The generated improvement roadmap.</returns>
    Task<ImprovementRoadmap> GenerateRoadmapAsync(string organizationId, CancellationToken cancellationToken);

    /// <summary>
    /// Compiles a full maturity assessment from all stored scores for an organization.
    /// Includes pillar scores, statement scores, and an overall maturity level.
    /// </summary>
    /// <param name="organizationId">The organization to assess.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The compiled maturity assessment.</returns>
    Task<MaturityAssessment> GetAssessmentAsync(string organizationId, CancellationToken cancellationToken);
}
