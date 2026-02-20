using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Ports;

/// <summary>
/// Defines the contract for CRUD operations on the episodic memory store.
/// Provides basic storage, retrieval, update, and deletion of memory records
/// along with aggregate statistics.
/// </summary>
public interface IMemoryStorePort
{
    /// <summary>
    /// Stores a new memory record in the episodic memory store.
    /// </summary>
    /// <param name="record">The memory record to store.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The stored memory record.</returns>
    Task<MemoryRecord> StoreAsync(MemoryRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a memory record by its unique identifier.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record to retrieve.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The memory record if found; otherwise, null.</returns>
    Task<MemoryRecord?> GetAsync(string recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing memory record in the store.
    /// </summary>
    /// <param name="record">The memory record with updated fields.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>The updated memory record.</returns>
    Task<MemoryRecord> UpdateAsync(MemoryRecord record, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a memory record by its unique identifier.
    /// </summary>
    /// <param name="recordId">The unique identifier of the record to delete.</param>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>True if the record was deleted; false if it was not found.</returns>
    Task<bool> DeleteAsync(string recordId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves aggregate statistics about the memory store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the async operation.</param>
    /// <returns>Statistics including total records, consolidation counts, and strategy performance.</returns>
    Task<MemoryStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}
