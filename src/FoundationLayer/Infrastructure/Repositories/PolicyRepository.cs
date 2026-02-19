using CognitiveMesh.Shared.Interfaces;
using CognitiveMesh.Shared.Models;
using FoundationLayer.ConvenerData.Persistence;
using FoundationLayer.Infrastructure.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

// --- Repository Implementation ---
namespace FoundationLayer.Infrastructure.Repositories
{
    /// <summary>
    /// Implements the IPolicyRepository interface using Entity Framework Core.
    /// This class is responsible for all database operations related to routing policies,
    /// strictly enforcing tenant isolation and providing robust error handling.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ConvenerDbContext _context;
        private readonly ILogger<PolicyRepository> _logger;
        private readonly string _currentTenantId;

        public PolicyRepository(ConvenerDbContext context, ILogger<PolicyRepository> logger, string? tenantId = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentTenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        }

        /// <inheritdoc />
        public async Task<PolicyConfiguration?> GetPolicyForTenantAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                _logger.LogWarning("GetPolicyForTenantAsync called with a null or empty tenantId.");
                return null;
            }

            try
            {
                // Retrieve the latest policy version for the given tenant.
                return await _context.Set<PolicyConfiguration>()
                    .AsNoTracking()
                    .Where(p => p.TenantId == tenantId)
                    .OrderByDescending(p => p.PolicyVersion)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy for Tenant '{TenantId}'.", tenantId);
                throw; // Re-throw to allow the service layer to handle the exception.
            }
        }

        public async Task<PolicyConfiguration?> GetActivePolicyAsync()
        {
            try
            {
                return await _context.Set<PolicyConfiguration>()
                    .Where(p => p.TenantId == _currentTenantId && p.IsActive)
                    .OrderByDescending(p => p.PolicyVersion)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active policy for tenant {TenantId}", _currentTenantId);
                throw new PolicyRepositoryException("Failed to retrieve active policy", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> SavePolicyForTenantAsync(PolicyConfiguration policy)
        {
            if (policy == null || string.IsNullOrWhiteSpace(policy.TenantId) || string.IsNullOrWhiteSpace(policy.PolicyVersion))
            {
                throw new ArgumentException("Policy and its TenantId and PolicyVersion must be provided");
            }

            try
            {
                // Check if a policy with the same version already exists for this tenant
                var existingPolicy = await _context.Set<PolicyConfiguration>()
                    .FirstOrDefaultAsync(p => p.TenantId == policy.TenantId && p.PolicyVersion == policy.PolicyVersion);

                if (existingPolicy != null)
                {
                    // Update existing policy
                    _context.Entry(existingPolicy).CurrentValues.SetValues(policy);
                    existingPolicy.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new policy version
                    policy.CreatedAt = DateTime.UtcNow;
                    await _context.Set<PolicyConfiguration>().AddAsync(policy);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving policy for tenant {TenantId} with version {PolicyVersion}", 
                    policy?.TenantId, policy?.PolicyVersion);
                throw new PolicyRepositoryException("Failed to save policy", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeletePolicyForTenantAsync(string policyId)
        {
            if (string.IsNullOrWhiteSpace(policyId))
            {
                throw new ArgumentException("Policy ID must be provided");
            }

            try
            {
                var policy = await _context.Set<PolicyConfiguration>()
                    .FirstOrDefaultAsync(p => p.PolicyId == policyId && p.TenantId == _currentTenantId);

                if (policy != null)
                {
                    _context.Entry(policy).State = EntityState.Deleted;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting policy for tenant {TenantId} with ID {PolicyId}", 
                    _currentTenantId, policyId);
                throw new PolicyRepositoryException("Failed to delete policy", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PolicyConfiguration>> GetPolicyHistoryForTenantAsync(string tenantId)
        {
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                throw new ArgumentException("Tenant ID must be provided", nameof(tenantId));
            }

            try
            {
                return await _context.Set<PolicyConfiguration>()
                    .Where(p => p.TenantId == tenantId)
                    .OrderByDescending(p => p.PolicyVersion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving policy history for tenant {TenantId}", tenantId);
                throw new PolicyRepositoryException("Failed to retrieve policy history", ex);
            }
        }
    }
}
