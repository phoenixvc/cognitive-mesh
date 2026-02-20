using CognitiveMesh.FoundationLayer.NISTEvidence.Models;

namespace CognitiveMesh.FoundationLayer.NISTEvidence.Ports;

/// <summary>
/// Port interface for the NIST AI RMF evidence repository.
/// Defines operations for storing, retrieving, querying, and managing
/// evidence records that support NIST AI RMF compliance.
/// </summary>
public interface INISTEvidenceRepositoryPort
{
    /// <summary>
    /// Stores a new NIST evidence record in the repository.
    /// </summary>
    /// <param name="record">The evidence record to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored evidence record with any server-assigned values populated.</returns>
    Task<NISTEvidenceRecord> StoreAsync(NISTEvidenceRecord record, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a NIST evidence record by its unique identifier.
    /// </summary>
    /// <param name="evidenceId">The unique identifier of the evidence record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The evidence record if found; otherwise, <c>null</c>.</returns>
    Task<NISTEvidenceRecord?> GetByIdAsync(Guid evidenceId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all evidence records associated with a specific NIST AI RMF statement.
    /// </summary>
    /// <param name="statementId">The NIST AI RMF statement identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of evidence records for the specified statement.</returns>
    Task<IReadOnlyList<NISTEvidenceRecord>> GetByStatementAsync(string statementId, CancellationToken cancellationToken);

    /// <summary>
    /// Queries evidence records using the specified filter criteria.
    /// </summary>
    /// <param name="filter">The filter criteria to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of evidence records matching the filter.</returns>
    Task<IReadOnlyList<NISTEvidenceRecord>> QueryAsync(EvidenceQueryFilter filter, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the review status of an evidence record.
    /// </summary>
    /// <param name="evidenceId">The unique identifier of the evidence record to update.</param>
    /// <param name="status">The new review status.</param>
    /// <param name="reviewerId">The identifier of the reviewer making the status change.</param>
    /// <param name="notes">Optional notes from the reviewer.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated evidence record if found; otherwise, <c>null</c>.</returns>
    Task<NISTEvidenceRecord?> UpdateReviewStatusAsync(
        Guid evidenceId,
        EvidenceReviewStatus status,
        string reviewerId,
        string? notes,
        CancellationToken cancellationToken);

    /// <summary>
    /// Archives an evidence record, marking it as no longer active.
    /// </summary>
    /// <param name="evidenceId">The unique identifier of the evidence record to archive.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the record was found and archived; otherwise, <c>false</c>.</returns>
    Task<bool> ArchiveAsync(Guid evidenceId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves aggregated statistics about the evidence records in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="EvidenceStatistics"/> object containing the aggregated data.</returns>
    Task<EvidenceStatistics> GetStatisticsAsync(CancellationToken cancellationToken);
}
