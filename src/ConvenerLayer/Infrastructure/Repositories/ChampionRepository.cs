using CognitiveMesh.ConvenerLayer.Core.Entities;
using CognitiveMesh.ConvenerLayer.Core.Interfaces;
using CognitiveMesh.ConvenerLayer.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

// --- Placeholder EF Core DbContext ---
// Note: In a complete application, this DbContext would reside in its own file
// within 'src/ConvenerLayer/Infrastructure/Persistence/' or similar.
namespace CognitiveMesh.ConvenerLayer.Infrastructure.Persistence
{
    public class ConvenerDbContext : DbContext
    {
        public ConvenerDbContext(DbContextOptions<ConvenerDbContext> options) : base(options) { }

        public DbSet<Champion> Champions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // This will apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConvenerDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }

    /// <summary>
    /// EF Core configuration for the Champion aggregate root.
    /// This defines how the domain entity is mapped to the database schema.
    /// </summary>
    public class ChampionConfiguration : IEntityTypeConfiguration<Champion>
    {
        public void Configure(EntityTypeBuilder<Champion> builder)
        {
            builder.ToTable("Champions");
            builder.HasKey(c => c.ChampionId);

            // Configure value object collections as JSON strings for simplicity.
            // In a more complex scenario, these could be separate tables.
            var jsonSerializerOptions = new JsonSerializerOptions();

            builder.Property(c => c.Skills)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<Skill>>(v, jsonSerializerOptions)
                );

            builder.Property(c => c.Provenance)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                    v => JsonSerializer.Deserialize<List<ProvenanceEntry>>(v, jsonSerializerOptions)
                );

            builder.Property(c => c.TenantId).IsRequired();
            builder.HasIndex(c => new { c.UserId, c.TenantId }).IsUnique();
        }
    }
}


// --- Repository Implementation ---
namespace CognitiveMesh.ConvenerLayer.Infrastructure.Repositories
{
    using CognitiveMesh.ConvenerLayer.Infrastructure.Persistence;

    /// <summary>
    /// Implements the IChampionRepository interface using Entity Framework Core for data persistence.
    /// This class is responsible for all database operations related to the Champion aggregate.
    /// It strictly enforces tenant isolation in every query.
    /// </summary>
    public class ChampionRepository : IChampionRepository, IChampionDataRepository
    {
        private readonly ConvenerDbContext _context;
        private readonly ILogger<ChampionRepository> _logger;

        public ChampionRepository(ConvenerDbContext context, ILogger<ChampionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<Champion> GetByIdAsync(Guid championId, string tenantId)
        {
            try
            {
                return await _context.Champions
                    .FirstOrDefaultAsync(c => c.ChampionId == championId && c.TenantId == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving champion by ID '{ChampionId}' for Tenant '{TenantId}'.", championId, tenantId);
                throw; // Re-throw to allow higher layers to handle it.
            }
        }

        /// <inheritdoc />
        public async Task<Champion> GetByUserIdAsync(string userId, string tenantId)
        {
            try
            {
                return await _context.Champions
                    .FirstOrDefaultAsync(c => c.UserId == userId && c.TenantId == tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving champion by UserID '{UserId}' for Tenant '{TenantId}'.", userId, tenantId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Champion>> FindPotentialChampionsAsync(string tenantId, string skillFilter = null)
        {
            try
            {
                var query = _context.Champions
                    .AsNoTracking() // Use for read-only queries to improve performance.
                    .Where(c => c.TenantId == tenantId);

                if (!string.IsNullOrWhiteSpace(skillFilter))
                {
                    // This is a simplified search. A real-world implementation might use full-text search
                    // or a more complex query against a normalized skills table or JSON structure.
                    query = query.Where(c => c.Skills.Any(s => s.Name.Equals(skillFilter, StringComparison.OrdinalIgnoreCase)));
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding potential champions for Tenant '{TenantId}' with skill filter '{SkillFilter}'.", tenantId, skillFilter ?? "N/A");
                return Enumerable.Empty<Champion>();
            }
        }

        /// <inheritdoc />
        public async Task SaveChampionAsync(Champion champion)
        {
            try
            {
                var existing = await _context.Champions.FindAsync(champion.ChampionId);
                if (existing == null)
                {
                    _context.Champions.Add(champion);
                    _logger.LogInformation("Adding new champion with ID '{ChampionId}' for Tenant '{TenantId}'.", champion.ChampionId, champion.TenantId);
                }
                else
                {
                    _context.Entry(existing).CurrentValues.SetValues(champion);
                    _logger.LogInformation("Updating existing champion with ID '{ChampionId}' for Tenant '{TenantId}'.", champion.ChampionId, champion.TenantId);
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving champion with ID '{ChampionId}' for Tenant '{TenantId}'.", champion.ChampionId, champion.TenantId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteChampionAsync(Guid championId, string tenantId)
        {
            try
            {
                var championToDelete = await _context.Champions
                    .FirstOrDefaultAsync(c => c.ChampionId == championId && c.TenantId == tenantId);

                if (championToDelete == null)
                {
                    _logger.LogWarning("Delete failed: Champion with ID '{ChampionId}' not found for Tenant '{TenantId}'.", championId, tenantId);
                    return false;
                }

                _context.Champions.Remove(championToDelete);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted champion with ID '{ChampionId}' for Tenant '{TenantId}'.", championId, tenantId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting champion with ID '{ChampionId}' for Tenant '{TenantId}'.", championId, tenantId);
                return false;
            }
        }

        // --- IChampionDataRepository Implementation ---

        /// <inheritdoc />
        public async Task<ChampionDataSnapshot> GetChampionDataSnapshotAsync(string userId, string tenantId)
        {
            // This is a mock implementation. In a real system, this might query an analytics store
            // or an aggregated view for performance.
            var champion = await GetByUserIdAsync(userId, tenantId);
            if (champion == null) return null;

            return new ChampionDataSnapshot
            {
                UserId = champion.UserId,
                InteractionCount = champion.InteractionCount,
                EndorsementCount = 0, // Placeholder
                LastActivityDate = champion.LastActiveDate
            };
        }

        /// <inheritdoc />
        public async Task<Dictionary<string, ChampionDataSnapshot>> GetChampionDataSnapshotsAsync(IEnumerable<string> userIds, string tenantId)
        {
            // This is a mock implementation. A real system would use a more efficient batch query.
            var snapshots = new Dictionary<string, ChampionDataSnapshot>();
            foreach (var userId in userIds)
            {
                var snapshot = await GetChampionDataSnapshotAsync(userId, tenantId);
                if (snapshot != null)
                {
                    snapshots[userId] = snapshot;
                }
            }
            return snapshots;
        }
    }
}
