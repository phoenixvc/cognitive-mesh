using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.Shared.Models;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Defines the contract for policy repository implementations.
    /// </summary>
    public interface IPolicyRepository
    {
        /// <summary>
        /// Gets the latest policy for a specific tenant.
        /// </summary>
        /// <param name="tenantId">The ID of the tenant.</param>
        /// <returns>The latest policy configuration for the tenant, or null if not found.</returns>
        Task<PolicyConfiguration?> GetPolicyForTenantAsync(string tenantId);

        /// <summary>
        /// Saves a policy configuration for a tenant.
        /// </summary>
        /// <param name="policy">The policy configuration to save.</param>
        /// <returns>True if the operation was successful; otherwise, false.</returns>
        Task<bool> SavePolicyForTenantAsync(PolicyConfiguration policy);
    }
}
