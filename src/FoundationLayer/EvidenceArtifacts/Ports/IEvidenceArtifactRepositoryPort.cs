using CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;

namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Ports;

/// <summary>
/// Port interface for the evidence artifact repository used in the
/// Adaptive Balance (PRD-002) framework. Defines operations for storing,
/// retrieving, searching, and managing evidence artifacts.
/// </summary>
public interface IEvidenceArtifactRepositoryPort
{
    /// <summary>
    /// Stores a new evidence artifact in the repository.
    /// </summary>
    /// <param name="artifact">The evidence artifact to store.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The stored artifact with any server-assigned values populated.</returns>
    Task<EvidenceArtifact> StoreArtifactAsync(EvidenceArtifact artifact, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves an evidence artifact by its unique identifier.
    /// </summary>
    /// <param name="artifactId">The unique identifier of the artifact.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The artifact if found; otherwise, <c>null</c>.</returns>
    Task<EvidenceArtifact?> GetArtifactAsync(Guid artifactId, CancellationToken cancellationToken);

    /// <summary>
    /// Searches for evidence artifacts matching the specified criteria.
    /// </summary>
    /// <param name="criteria">The search criteria to apply.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A read-only list of artifacts matching the criteria.</returns>
    Task<IReadOnlyList<EvidenceArtifact>> SearchAsync(ArtifactSearchCriteria criteria, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes all expired artifacts from the repository based on their <see cref="EvidenceArtifact.ExpiresAt"/> value.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of artifacts that were deleted.</returns>
    Task<int> DeleteExpiredAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the total count of artifacts currently in the repository.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The total number of artifacts.</returns>
    Task<int> GetArtifactCountAsync(CancellationToken cancellationToken);
}
