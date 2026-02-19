using System.Text.Json;
using FoundationLayer.ConvenerData.Entities;
using FoundationLayer.ConvenerData.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using AuthorityScope = FoundationLayer.ConvenerData.ValueObjects.AuthorityScope;

namespace FoundationLayer.Infrastructure.Repositories;
// --- Placeholder EF Core DbContext ---
// Note: In a complete application, this DbContext would reside in its own file
// within 'src/FoundationLayer/Infrastructure/Persistence/' or similar.
// This assumes the ConvenerDbContext is extended to include the AgentDefinitions DbSet.
// The configuration for the new entity is also defined here.

/// <summary>
/// EF Core configuration for the AgentDefinition aggregate root.
/// This defines how the domain entity is mapped to the database schema.
/// </summary>
public class AgentDefinitionConfiguration : IEntityTypeConfiguration<AgentDefinition>
{
    public void Configure(EntityTypeBuilder<AgentDefinition> builder)
    {
        builder.ToTable("AgentRegistry");
        builder.HasKey(a => a.AgentId);

        // Ensure that each agent type and version combination is unique.
        builder.HasIndex(a => new { a.AgentType, a.Version }).IsUnique();

        // Configure complex properties as JSON strings for simplicity.
        var jsonSerializerOptions = new JsonSerializerOptions();

        builder.Property(a => a.Capabilities)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, jsonSerializerOptions) ?? new Dictionary<string, string>()
            );

        builder.Property(a => a.DefaultAuthorityScope)
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonSerializerOptions),
                v => JsonSerializer.Deserialize<AuthorityScope>(v, jsonSerializerOptions) ?? new AuthorityScope()
            );

        // Store enums as strings for readability in the database.
        builder.Property(a => a.DefaultAutonomyLevel).HasConversion<string>();
        builder.Property(a => a.Status).HasConversion<string>();
    }
}


// --- Port Interface ---
/// <summary>
/// Defines the contract for a repository responsible for the persistence and retrieval
/// of AgentDefinition aggregate roots. This interface is part of the Core/Foundation layer
/// and is implemented in the Infrastructure layer.
/// </summary>
public interface IAgentRegistryRepository
{
    Task RegisterAsync(AgentDefinition agentDefinition);
    Task<AgentDefinition?> GetByIdAsync(Guid agentId);
    Task<AgentDefinition?> GetByTypeAndVersionAsync(string agentType, string version);
    Task<IEnumerable<AgentDefinition>> ListAllAsync(bool includeRetired = false);
    Task UpdateAsync(AgentDefinition agentDefinition);
}


// --- Repository Implementation ---
/// <summary>
/// Implements the IAgentRegistryRepository interface using Entity Framework Core for data persistence.
/// This class is responsible for all database operations related to the AgentDefinition aggregate.
/// </summary>
public class AgentRegistryRepository : IAgentRegistryRepository
{
    private readonly ConvenerDbContext _context;
    private readonly ILogger<AgentRegistryRepository> _logger;

    public AgentRegistryRepository(ConvenerDbContext context, ILogger<AgentRegistryRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task RegisterAsync(AgentDefinition agentDefinition)
    {
        try
        {
            await _context.Set<AgentDefinition>().AddAsync(agentDefinition);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully registered new agent definition: {AgentType} v{Version} (ID: {AgentId}).", agentDefinition.AgentType, agentDefinition.Version, agentDefinition.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering new agent definition for AgentType '{AgentType}'.", agentDefinition.AgentType);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AgentDefinition?> GetByIdAsync(Guid agentId)
    {
        try
        {
            return await _context.Set<AgentDefinition>().FindAsync(agentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent definition by ID '{AgentId}'.", agentId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AgentDefinition?> GetByTypeAndVersionAsync(string agentType, string version)
    {
        try
        {
            return await _context.Set<AgentDefinition>()
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AgentType == agentType && a.Version == version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving agent definition for AgentType '{AgentType}' and Version '{Version}'.", agentType, version);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AgentDefinition>> ListAllAsync(bool includeRetired = false)
    {
        try
        {
            var query = _context.Set<AgentDefinition>().AsNoTracking();

            if (!includeRetired)
            {
                query = query.Where(a => a.Status != AgentStatus.Retired);
            }

            return await query.OrderBy(a => a.AgentType).ThenByDescending(a => a.Version).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing all agent definitions.");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task UpdateAsync(AgentDefinition agentDefinition)
    {
        try
        {
            // EF Core's change tracker will automatically detect changes to the loaded entity.
            _context.Set<AgentDefinition>().Update(agentDefinition);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully updated agent definition for ID '{AgentId}'.", agentDefinition.AgentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating agent definition for ID '{AgentId}'.", agentDefinition.AgentId);
            throw;
        }
    }
}