using CognitiveMesh.ConvenerLayer.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CognitiveMesh.ConvenerLayer.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for a repository responsible for the persistence and retrieval
    /// of Champion aggregate roots. This interface is part of the Core layer and abstracts
    /// away the specific data storage technology, which is implemented in the Infrastructure layer.
    /// </summary>
    public interface IChampionRepository
    {
        /// <summary>
        /// Retrieves a collection of potential champions based on specified criteria.
        /// This method is typically used as the first step in a discovery workflow, before
        /// scoring and ranking are applied.
        /// </summary>
        /// <param name="tenantId">The identifier for the tenant, used to ensure data isolation.</param>
        /// <param name="skillFilter">An optional filter to narrow down champions by a specific skill.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of <see cref="Champion"/> entities.</returns>
        Task<IEnumerable<Champion>> FindPotentialChampionsAsync(string tenantId, string skillFilter = null);

        /// <summary>
        /// Retrieves a single Champion entity by its unique identifier (primary key).
        /// </summary>
        /// <param name="championId">The unique ID of the Champion entity.</param>
        /// <param name="tenantId">The identifier for the tenant to scope the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Champion"/> if found; otherwise, null.</returns>
        Task<Champion> GetByIdAsync(Guid championId, string tenantId);

        /// <summary>
        /// Retrieves a single Champion entity by the user's identifier.
        /// </summary>
        /// <param name="userId">The user ID associated with the champion.</param>
        /// <param name="tenantId">The identifier for the tenant to scope the search.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="Champion"/> if found; otherwise, null.</returns>
        Task<Champion> GetByUserIdAsync(string userId, string tenantId);

        /// <summary>
        /// Persists a Champion entity to the data store. This method handles both
        /// the creation of new champions and the updating of existing ones.
        /// </summary>
        /// <param name="champion">The Champion aggregate root to save.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveChampionAsync(Champion champion);

        /// <summary>
        /// Deletes a Champion entity from the data store.
        /// </summary>
        /// <param name="championId">The unique ID of the champion to delete.</param>
        /// <param name="tenantId">The identifier for the tenant to ensure the correct champion is deleted.</param>
        /// <returns>A task that represents the asynchronous delete operation. The task result contains true if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteChampionAsync(Guid championId, string tenantId);
    }
}
