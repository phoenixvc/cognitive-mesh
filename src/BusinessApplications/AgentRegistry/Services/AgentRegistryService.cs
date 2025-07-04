using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CognitiveMesh.AgencyLayer.MultiAgentOrchestration.Ports.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Data;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Services
{
    /// <summary>
    /// Concrete implementation of the IAgentRegistryPort interface.
    /// This service manages the lifecycle of agent definitions, including registration,
    /// querying, updating, deprecation, and retirement.
    /// </summary>
    public class AgentRegistryService : IAgentRegistryPort
    {
        private readonly AgentDbContext _dbContext;
        private readonly ILogger<AgentRegistryService> _logger;
        private readonly AgentCircuitBreakerPolicy _circuitBreaker;

        /// <summary>
        /// Initializes a new instance of the AgentRegistryService class.
        /// </summary>
        /// <param name="dbContext">The database context for agent data</param>
        /// <param name="logger">The logger</param>
        public AgentRegistryService(
            AgentDbContext dbContext,
            ILogger<AgentRegistryService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize circuit breaker for database operations
            _circuitBreaker = new AgentCircuitBreakerPolicy(3, 250, 1000, 50);
        }

        /// <inheritdoc />
        public async Task<AgentDefinition> RegisterAgentAsync(AgentDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            // Validate the agent definition
            var validationResult = await ValidateAgentDefinitionAsync(definition);
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(", ", validationResult.Errors);
                _logger.LogWarning("Agent definition validation failed: {ErrorMessage}", errorMessage);
                throw new AgentValidationException(errorMessage);
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Set default values if not provided
                    if (definition.AgentId == Guid.Empty)
                    {
                        definition.AgentId = Guid.NewGuid();
                    }

                    // Set default status if not specified
                    if (definition.Status == 0) // Default enum value
                    {
                        definition.Status = AgentStatus.Active;
                    }

                    // Create a version record for the initial registration
                    var versionRecord = new AgentVersionRecord
                    {
                        AgentId = definition.AgentId,
                        Version = "1.0.0", // Initial version
                        CreatedAt = DateTimeOffset.UtcNow,
                        Status = definition.Status,
                        CreatedBy = "System", // This should be replaced with actual user info
                        ChangeDescription = "Initial registration"
                    };

                    // Add the agent definition and version record to the database
                    _dbContext.AgentDefinitions.Add(definition);
                    _dbContext.AgentVersionRecords.Add(versionRecord);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent registered successfully: {AgentId}, Type: {AgentType}", 
                        definition.AgentId, definition.AgentType);

                    return definition;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering agent: {AgentType}", definition.AgentType);
                throw new AgentRegistrationException("Failed to register agent", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AgentDefinition> GetAgentByIdAsync(Guid agentId)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var agent = await _dbContext.AgentDefinitions
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AgentId == agentId);

                    if (agent == null)
                    {
                        _logger.LogWarning("Agent not found: {AgentId}", agentId);
                    }

                    return agent;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving agent: {AgentId}", agentId);
                throw new AgentNotFoundException($"Agent with ID {agentId} could not be retrieved", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AgentDefinition>> ListAgentsAsync(bool includeRetired = false, bool includeDeprecated = true)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var query = _dbContext.AgentDefinitions.AsNoTracking();

                    // Filter based on parameters
                    if (!includeRetired)
                    {
                        query = query.Where(a => a.Status != AgentStatus.Retired);
                    }

                    if (!includeDeprecated)
                    {
                        query = query.Where(a => a.Status != AgentStatus.Deprecated);
                    }

                    return await query.ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing agents");
                throw new AgentRegistryException("Failed to list agents", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AgentDefinition> UpdateAgentAsync(AgentDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get the existing agent
                    var existingAgent = await _dbContext.AgentDefinitions
                        .FirstOrDefaultAsync(a => a.AgentId == definition.AgentId);

                    if (existingAgent == null)
                    {
                        _logger.LogWarning("Agent not found for update: {AgentId}", definition.AgentId);
                        throw new AgentNotFoundException($"Agent with ID {definition.AgentId} not found");
                    }

                    // Check if the agent is retired
                    if (existingAgent.Status == AgentStatus.Retired)
                    {
                        _logger.LogWarning("Cannot update retired agent: {AgentId}", definition.AgentId);
                        throw new AgentRetiredException($"Agent with ID {definition.AgentId} is retired and cannot be updated");
                    }

                    // Create a version record for the update
                    var newVersion = IncrementVersion(existingAgent);
                    var versionRecord = new AgentVersionRecord
                    {
                        AgentId = definition.AgentId,
                        Version = newVersion,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Status = definition.Status,
                        CreatedBy = "System", // This should be replaced with actual user info
                        ChangeDescription = "Agent updated"
                    };

                    // Update the agent properties
                    existingAgent.Description = definition.Description;
                    existingAgent.Capabilities = definition.Capabilities;
                    existingAgent.DefaultAutonomyLevel = definition.DefaultAutonomyLevel;
                    existingAgent.DefaultAuthorityScope = definition.DefaultAuthorityScope;
                    existingAgent.Status = definition.Status;

                    // Add the version record
                    _dbContext.AgentVersionRecords.Add(versionRecord);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent updated successfully: {AgentId}, New Version: {Version}", 
                        definition.AgentId, newVersion);

                    return existingAgent;
                });
            }
            catch (AgentNotFoundException)
            {
                // Re-throw specific exceptions
                throw;
            }
            catch (AgentRetiredException)
            {
                // Re-throw specific exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating agent: {AgentId}", definition.AgentId);
                throw new AgentRegistryException($"Failed to update agent with ID {definition.AgentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeprecateAgentAsync(Guid agentId, DeprecationNotice deprecationNotice)
        {
            if (deprecationNotice == null)
            {
                throw new ArgumentNullException(nameof(deprecationNotice));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get the existing agent
                    var existingAgent = await _dbContext.AgentDefinitions
                        .FirstOrDefaultAsync(a => a.AgentId == agentId);

                    if (existingAgent == null)
                    {
                        _logger.LogWarning("Agent not found for deprecation: {AgentId}", agentId);
                        return false;
                    }

                    // Check if the agent is already retired
                    if (existingAgent.Status == AgentStatus.Retired)
                    {
                        _logger.LogWarning("Cannot deprecate retired agent: {AgentId}", agentId);
                        return false;
                    }

                    // Update the agent status to deprecated
                    existingAgent.Status = AgentStatus.Deprecated;

                    // Store the deprecation notice
                    // In a real implementation, you would store this in a dedicated table
                    // For now, we'll assume there's a JSON column or similar in the agent definition
                    // existingAgent.DeprecationInfo = JsonSerializer.Serialize(deprecationNotice);

                    // Create a version record for the deprecation
                    var newVersion = IncrementVersion(existingAgent);
                    var versionRecord = new AgentVersionRecord
                    {
                        AgentId = agentId,
                        Version = newVersion,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Status = AgentStatus.Deprecated,
                        CreatedBy = "System", // This should be replaced with actual user info
                        ChangeDescription = $"Deprecated: {deprecationNotice.Reason}. Sunset date: {deprecationNotice.SunsetDate}"
                    };

                    // Add the version record
                    _dbContext.AgentVersionRecords.Add(versionRecord);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent deprecated successfully: {AgentId}, Sunset Date: {SunsetDate}", 
                        agentId, deprecationNotice.SunsetDate);

                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deprecating agent: {AgentId}", agentId);
                throw new AgentRegistryException($"Failed to deprecate agent with ID {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RetireAgentAsync(Guid agentId)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get the existing agent
                    var existingAgent = await _dbContext.AgentDefinitions
                        .FirstOrDefaultAsync(a => a.AgentId == agentId);

                    if (existingAgent == null)
                    {
                        _logger.LogWarning("Agent not found for retirement: {AgentId}", agentId);
                        return false;
                    }

                    // Check if the agent is already retired
                    if (existingAgent.Status == AgentStatus.Retired)
                    {
                        _logger.LogInformation("Agent already retired: {AgentId}", agentId);
                        return true;
                    }

                    // Update the agent status to retired
                    existingAgent.Status = AgentStatus.Retired;

                    // Create a version record for the retirement
                    var newVersion = IncrementVersion(existingAgent);
                    var versionRecord = new AgentVersionRecord
                    {
                        AgentId = agentId,
                        Version = newVersion,
                        CreatedAt = DateTimeOffset.UtcNow,
                        Status = AgentStatus.Retired,
                        CreatedBy = "System", // This should be replaced with actual user info
                        ChangeDescription = "Agent retired"
                    };

                    // Add the version record
                    _dbContext.AgentVersionRecords.Add(versionRecord);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent retired successfully: {AgentId}", agentId);

                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retiring agent: {AgentId}", agentId);
                throw new AgentRegistryException($"Failed to retire agent with ID {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AgentDefinition>> FindAgentsAsync(AgentSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Start with a base query
                    var query = _dbContext.AgentDefinitions.AsNoTracking();

                    // Apply active only filter
                    if (criteria.ActiveOnly)
                    {
                        query = query.Where(a => a.Status == AgentStatus.Active);
                    }

                    // Apply capabilities filter
                    if (criteria.RequiredCapabilities != null && criteria.RequiredCapabilities.Count > 0)
                    {
                        foreach (var capability in criteria.RequiredCapabilities)
                        {
                            query = query.Where(a => a.Capabilities.Contains(capability));
                        }
                    }

                    // Apply agent types filter
                    if (criteria.AgentTypes != null && criteria.AgentTypes.Count > 0)
                    {
                        query = query.Where(a => criteria.AgentTypes.Contains(a.AgentType));
                    }

                    // Apply minimum autonomy level filter
                    if (criteria.MinimumAutonomyLevel.HasValue)
                    {
                        query = query.Where(a => a.DefaultAutonomyLevel >= criteria.MinimumAutonomyLevel.Value);
                    }

                    // Apply free-text search
                    if (!string.IsNullOrEmpty(criteria.SearchText))
                    {
                        query = query.Where(a => 
                            a.AgentType.Contains(criteria.SearchText) || 
                            a.Description.Contains(criteria.SearchText) ||
                            a.Capabilities.Any(c => c.Contains(criteria.SearchText)));
                    }

                    // Apply max results limit
                    query = query.Take(criteria.MaxResults);

                    return await query.ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding agents with criteria: {Criteria}", 
                    criteria.ToString());
                throw new AgentRegistryException("Failed to find agents", ex);
            }
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateAgentDefinitionAsync(AgentDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var result = new ValidationResult { IsValid = true };

            // Validate required fields
            if (string.IsNullOrEmpty(definition.AgentType))
            {
                result.IsValid = false;
                result.Errors.Add("AgentType is required");
            }

            if (string.IsNullOrEmpty(definition.Description))
            {
                result.Warnings.Add("Description is empty");
            }

            // Check for duplicate agent type
            try
            {
                var existingAgent = await _dbContext.AgentDefinitions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.AgentType == definition.AgentType && a.AgentId != definition.AgentId);

                if (existingAgent != null)
                {
                    result.IsValid = false;
                    result.Errors.Add($"An agent with type '{definition.AgentType}' already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating agent definition");
                result.IsValid = false;
                result.Errors.Add("Validation failed due to a system error");
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AgentVersionRecord>> GetAgentVersionHistoryAsync(Guid agentId)
        {
            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    return await _dbContext.AgentVersionRecords
                        .AsNoTracking()
                        .Where(v => v.AgentId == agentId)
                        .OrderByDescending(v => v.CreatedAt)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving version history for agent: {AgentId}", agentId);
                throw new AgentRegistryException($"Failed to retrieve version history for agent with ID {agentId}", ex);
            }
        }

        /// <summary>
        /// Increments the version number of an agent definition.
        /// Uses semantic versioning (MAJOR.MINOR.PATCH).
        /// </summary>
        /// <param name="agent">The agent definition</param>
        /// <returns>The new version number</returns>
        private string IncrementVersion(AgentDefinition agent)
        {
            // In a real implementation, you would store the version in the agent definition
            // For now, we'll get the latest version from the version history
            var latestVersion = _dbContext.AgentVersionRecords
                .Where(v => v.AgentId == agent.AgentId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => v.Version)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(latestVersion))
            {
                return "1.0.0";
            }

            // Parse the version
            var parts = latestVersion.Split('.');
            if (parts.Length != 3)
            {
                return "1.0.0";
            }

            // Increment the patch version
            if (int.TryParse(parts[2], out int patch))
            {
                parts[2] = (patch + 1).ToString();
            }
            else
            {
                parts[2] = "0";
            }

            return string.Join(".", parts);
        }
    }

    /// <summary>
    /// Database context for agent data.
    /// </summary>
    public class AgentDbContext : DbContext
    {
        public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
        {
        }

        public DbSet<AgentDefinition> AgentDefinitions { get; set; }
        public DbSet<AgentVersionRecord> AgentVersionRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AgentDefinition entity
            modelBuilder.Entity<AgentDefinition>(entity =>
            {
                entity.HasKey(e => e.AgentId);
                entity.Property(e => e.AgentType).IsRequired();
                entity.Property(e => e.Description).IsRequired(false);
                entity.Property(e => e.Status).IsRequired();
                
                // Configure capabilities as a JSON column
                entity.Property(e => e.Capabilities)
                    .HasConversion(
                        v => string.Join(",", v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
                
                // Configure DefaultAuthorityScope as a JSON column
                entity.Property(e => e.DefaultAuthorityScope)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<AuthorityScope>(v, new System.Text.Json.JsonSerializerOptions()));
            });

            // Configure AgentVersionRecord entity
            modelBuilder.Entity<AgentVersionRecord>(entity =>
            {
                entity.HasKey(e => new { e.AgentId, e.Version });
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.CreatedBy).IsRequired();
                entity.Property(e => e.ChangeDescription).IsRequired(false);
            });
        }
    }

    /// <summary>
    /// Exception thrown when agent registration fails.
    /// </summary>
    public class AgentRegistrationException : Exception
    {
        public AgentRegistrationException(string message) : base(message)
        {
        }

        public AgentRegistrationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an agent is not found.
    /// </summary>
    public class AgentNotFoundException : Exception
    {
        public AgentNotFoundException(string message) : base(message)
        {
        }

        public AgentNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an agent is retired.
    /// </summary>
    public class AgentRetiredException : Exception
    {
        public AgentRetiredException(string message) : base(message)
        {
        }

        public AgentRetiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when agent validation fails.
    /// </summary>
    public class AgentValidationException : Exception
    {
        public AgentValidationException(string message) : base(message)
        {
        }

        public AgentValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when an agent registry operation fails.
    /// </summary>
    public class AgentRegistryException : Exception
    {
        public AgentRegistryException(string message) : base(message)
        {
        }

        public AgentRegistryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
