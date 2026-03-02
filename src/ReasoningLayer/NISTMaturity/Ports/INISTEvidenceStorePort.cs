using CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Ports;

/// <summary>
/// Defines the contract for persisting and retrieving NIST AI RMF evidence artifacts.
/// Implementations may use various storage backends (CosmosDB, Blob Storage, etc.).
/// </summary>
public interface INISTEvidenceStorePort
{
    /// <summary>
    /// Stores a piece of evidence in the evidence store.
    /// </summary>
    /// <param name="evidence">The evidence to store.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous storage operation.</returns>
    Task StoreEvidenceAsync(NISTEvidence evidence, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a specific piece of evidence by its unique identifier.
    /// </summary>
    /// <param name="evidenceId">The unique identifier of the evidence.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The evidence if found; otherwise, <c>null</c>.</returns>
    Task<NISTEvidence?> GetEvidenceAsync(Guid evidenceId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all evidence associated with a specific NIST statement.
    /// </summary>
    /// <param name="statementId">The statement identifier to retrieve evidence for.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of evidence for the given statement.</returns>
    Task<IReadOnlyList<NISTEvidence>> GetEvidenceForStatementAsync(string statementId, CancellationToken cancellationToken);
}
