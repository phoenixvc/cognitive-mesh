using CognitiveMesh.ReasoningLayer.AgencyRouter.Adapters;
using CognitiveMesh.ReasoningLayer.AgencyRouter.Ports.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// --- EF Core DbContext and Entity Configuration ---
// Note: In a complete application, the DbContext would be in its own file.
// The entity configuration is placed here to show how the PolicyConfiguration entity is mapped to the database.
namespace CognitiveMesh.FoundationLayer.Infrastructure.Persistence
{
    /// <summary>
    /// EF Core configuration for the PolicyConfiguration entity.
    /// This defines how the policy object is mapped to the database schema.
    /// </summary>
    public class PolicyConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<PolicyConfiguration>
    {
        public void Configure(EntityTypeBuilder<PolicyConfiguration> builder)
        {
            builder.ToTable("Policies");
            builder.HasKey(p => p.PolicyId);

            // Ensure that each tenant can only have one policy of a specific version.
            builder.HasIndex(p => new { p.TenantId, p.PolicyVersion }).IsUnique();

            builder.Property(p => p.TenantId).IsRequired();
            builder.Property(p => p.PolicyVersion).IsRequired();

            // Store the list of rules as a JSON string in a single column.
            // This is a practical approach for complex but encapsulated data.
            var jsonSerializerOptions = new JsonSerializerOptions();
            builder.Property(p => p.Rules)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<RoutingRule>>(v, jsonSerializerOptions)
                );
        }
    }
}


// --- Repository Implementation ---
namespace CognitiveMesh.FoundationLayer.Infrastructure.Repositories
{
    using CognitiveMesh.FoundationLayer.Infrastructure.Persistence;

    /// <summary>
    /// Implements the IPolicyRepository interface using Entity Framework Core.
    /// This class is responsible for all database operations related to routing policies,
    /// strictly enforcing tenant isolation and providing robust error handling.
    /// </summary>
    public class PolicyRepository : IPolicyRepository
    {
        private readonly ConvenerDbContext _context;
        private readonly ILogger<PolicyRepository> _logger;

        public PolicyRepository(ConvenerDbContext context, ILogger<PolicyRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<PolicyConfiguration> GetPolicyForTenantAsync(string tenantId)
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

        /// <inheritdoc />
        public async Task<bool> SavePolicyForTenantAsync(PolicyConfiguration policy)
        {
            if (policy == null || string.IsNullOrWhiteSpace(policy.TenantId) || string.IsNullOrWhiteSpace(policy.PolicyVersion))
            {
                _logger.LogWarning("SavePolicyForTenantAsync called with an invalid policy object.");
                return false;
            }

            try
            {
                // Check if a policy with the same tenant and version already exists.
                var existingPolicy = await _context.Set<PolicyConfiguration>()
                    .FirstOrDefaultAsync(p => p.TenantId == policy.TenantId && p.PolicyVersion == policy.PolicyVersion);

                if (existingPolicy == null)
                {
                    // If it doesn't exist, add it as a new policy.
                    policy.PolicyId = Guid.NewGuid().ToString(); // Ensure a new GUID for the primary key.
                    _context.Set<PolicyConfiguration>().Add(policy);
                    _logger.LogInformation("Creating new policy version '{PolicyVersion}' for Tenant '{TenantId}'.", policy.PolicyVersion, policy.TenantId);
                }
                else
                {
                    // If it exists, update it. This makes the save operation idempotent.
                    existingPolicy.Rules = policy.Rules; // Update the rules.
                    _context.Set<PolicyConfiguration>().Update(existingPolicy);
                    _logger.LogInformation("Updating existing policy version '{PolicyVersion}' for Tenant '{TenantId}'.", policy.PolicyVersion, policy.TenantId);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error saving policy version '{PolicyVersion}' for Tenant '{TenantId}'. A unique constraint might have been violated.", policy.PolicyVersion, policy.TenantId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while saving policy version '{PolicyVersion}' for Tenant '{TenantId}'.", policy.PolicyVersion, policy.TenantId);
                return false;
            }
        }
    }
}
