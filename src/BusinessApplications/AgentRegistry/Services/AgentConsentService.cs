using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CognitiveMesh.BusinessApplications.AgentRegistry.Models;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.Common.Models;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports;
using CognitiveMesh.BusinessApplications.ConvenerServices.Ports.Models;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Services
{
    /// <summary>
    /// Concrete implementation of the IAgentConsentPort interface.
    /// This service manages agent-specific consent operations, including recording,
    /// validating, and revoking consent for agent actions.
    /// </summary>
    public class AgentConsentService : IAgentConsentPort
    {
        private readonly ConsentDbContext _dbContext;
        private readonly ILogger<AgentConsentService> _logger;
        private readonly AgentCircuitBreakerPolicy _circuitBreaker;

        /// <summary>
        /// Initializes a new instance of the AgentConsentService class.
        /// </summary>
        /// <param name="dbContext">The database context for consent data</param>
        /// <param name="logger">The logger</param>
        public AgentConsentService(
            ConsentDbContext dbContext,
            ILogger<AgentConsentService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize circuit breaker for database operations
            _circuitBreaker = new AgentCircuitBreakerPolicy(3, 250, 1000, 50);
        }

        #region IConsentPort Implementation

        /// <inheritdoc />
        public async Task<ConsentRecord> RecordConsentAsync(ConsentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Create a new consent record
                    var record = new ConsentRecord
                    {
                        ConsentId = Guid.NewGuid().ToString(),
                        UserId = request.UserId,
                        TenantId = request.TenantId,
                        ConsentType = request.ConsentType,
                        Scope = request.Scope,
                        IsGranted = request.IsGranted,
                        Timestamp = DateTimeOffset.UtcNow,
                        Source = request.Source,
                        Evidence = request.Evidence
                    };

                    // Add to database
                    _dbContext.ConsentRecords.Add(record);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Consent recorded: {ConsentType}, User: {UserId}, Granted: {IsGranted}", 
                        request.ConsentType, request.UserId, request.IsGranted);
                    
                    return record;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording consent: {ConsentType}, User: {UserId}", 
                    request.ConsentType, request.UserId);
                throw new ConsentServiceException("Failed to record consent", ex);
            }
        }

        /// <inheritdoc />
        public async Task<ValidateConsentResponse> ValidateConsentAsync(ValidateConsentRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get the latest consent record for this type and scope
                    var record = await _dbContext.ConsentRecords
                        .AsNoTracking()
                        .Where(c => c.UserId == request.UserId && 
                                   c.TenantId == request.TenantId && 
                                   c.ConsentType == request.RequiredConsentType &&
                                   (string.IsNullOrEmpty(request.Scope) || c.Scope == request.Scope))
                        .OrderByDescending(c => c.Timestamp)
                        .FirstOrDefaultAsync();

                    // Check if we have a valid consent record
                    var response = new ValidateConsentResponse
                    {
                        HasConsent = record != null && record.IsGranted,
                        ValidationTimestamp = DateTimeOffset.UtcNow,
                        ConsentRecordId = record?.ConsentId
                    };

                    _logger.LogInformation("Consent validated: {ConsentType}, User: {UserId}, HasConsent: {HasConsent}", 
                        request.RequiredConsentType, request.UserId, response.HasConsent);
                    
                    return response;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating consent: {ConsentType}, User: {UserId}", 
                    request.RequiredConsentType, request.UserId);
                throw new ConsentServiceException("Failed to validate consent", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ConsentRecord>> GetUserConsentsAsync(string userId, string tenantId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    return await _dbContext.ConsentRecords
                        .AsNoTracking()
                        .Where(c => c.UserId == userId && c.TenantId == tenantId)
                        .OrderByDescending(c => c.Timestamp)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consents for user: {UserId}, Tenant: {TenantId}", 
                    userId, tenantId);
                throw new ConsentServiceException("Failed to get user consents", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RevokeConsentAsync(string userId, string tenantId, string consentType, string scope = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (string.IsNullOrEmpty(consentType))
            {
                throw new ArgumentException("Consent type cannot be empty", nameof(consentType));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Create a revocation record
                    var record = new ConsentRecord
                    {
                        ConsentId = Guid.NewGuid().ToString(),
                        UserId = userId,
                        TenantId = tenantId,
                        ConsentType = consentType,
                        Scope = scope,
                        IsGranted = false, // Revoked = not granted
                        Timestamp = DateTimeOffset.UtcNow,
                        Source = "Revocation API",
                        Evidence = "Explicit revocation"
                    };

                    // Add to database
                    _dbContext.ConsentRecords.Add(record);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Consent revoked: {ConsentType}, User: {UserId}, Scope: {Scope}", 
                        consentType, userId, scope ?? "global");
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking consent: {ConsentType}, User: {UserId}, Scope: {Scope}", 
                    consentType, userId, scope ?? "global");
                throw new ConsentServiceException("Failed to revoke consent", ex);
            }
        }

        #endregion

        #region IAgentConsentPort Implementation

        /// <inheritdoc />
        public async Task<ConsentRecord> RecordAgentConsentAsync(ConsentRequest request, Guid agentId, string agentAction)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Create a new agent consent record
                    var record = new AgentConsentRecord
                    {
                        ConsentId = Guid.NewGuid().ToString(),
                        UserId = request.UserId,
                        TenantId = request.TenantId,
                        ConsentType = request.ConsentType,
                        Scope = request.Scope,
                        IsGranted = request.IsGranted,
                        Timestamp = DateTimeOffset.UtcNow,
                        Source = request.Source,
                        Evidence = request.Evidence,
                        AgentId = agentId,
                        AgentAction = agentAction,
                        OperationContext = new Dictionary<string, object>()
                    };

                    // Add to database
                    _dbContext.AgentConsentRecords.Add(record);
                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent consent recorded: {ConsentType}, User: {UserId}, Agent: {AgentId}, Granted: {IsGranted}", 
                        request.ConsentType, request.UserId, agentId, request.IsGranted);
                    
                    return record;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording agent consent: {ConsentType}, User: {UserId}, Agent: {AgentId}", 
                    request.ConsentType, request.UserId, agentId);
                throw new ConsentServiceException("Failed to record agent consent", ex);
            }
        }

        /// <inheritdoc />
        public async Task<ValidateConsentResponse> ValidateAgentConsentAsync(string userId, string tenantId, Guid agentId, string consentType)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (string.IsNullOrEmpty(consentType))
            {
                throw new ArgumentException("Consent type cannot be empty", nameof(consentType));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Check for emergency override
                    var emergencyOverride = await IsEmergencyOverrideActiveAsync(tenantId, agentId);
                    if (emergencyOverride)
                    {
                        _logger.LogInformation("Emergency override active for agent {AgentId} in tenant {TenantId}", 
                            agentId, tenantId);
                        
                        return new ValidateConsentResponse
                        {
                            HasConsent = true,
                            ValidationTimestamp = DateTimeOffset.UtcNow,
                            ConsentRecordId = "emergency-override"
                        };
                    }

                    // Check user preferences
                    var preferences = await GetAgentConsentPreferencesAsync(userId, tenantId);
                    if (preferences != null)
                    {
                        // Check agent-specific preferences
                        if (preferences.AgentPreferences.TryGetValue(agentId, out var agentPrefs))
                        {
                            if (agentPrefs.BlockAgent)
                            {
                                _logger.LogInformation("Agent {AgentId} is blocked for user {UserId}", agentId, userId);
                                return new ValidateConsentResponse
                                {
                                    HasConsent = false,
                                    ValidationTimestamp = DateTimeOffset.UtcNow,
                                    ConsentRecordId = "agent-blocked"
                                };
                            }

                            if (agentPrefs.TrustAgent || agentPrefs.PreApprovedConsentTypes.Contains(consentType))
                            {
                                _logger.LogInformation("Agent {AgentId} has pre-approved consent {ConsentType} for user {UserId}", 
                                    agentId, consentType, userId);
                                return new ValidateConsentResponse
                                {
                                    HasConsent = true,
                                    ValidationTimestamp = DateTimeOffset.UtcNow,
                                    ConsentRecordId = "pre-approved"
                                };
                            }
                        }

                        // Check consent type preferences
                        if (preferences.ConsentTypePreferences.TryGetValue(consentType, out var isApproved) && isApproved)
                        {
                            _logger.LogInformation("Consent type {ConsentType} is pre-approved for user {UserId}", 
                                consentType, userId);
                            return new ValidateConsentResponse
                            {
                                HasConsent = true,
                                ValidationTimestamp = DateTimeOffset.UtcNow,
                                ConsentRecordId = "type-pre-approved"
                            };
                        }

                        // Check auto-consent for low risk
                        if (preferences.AutoConsentLowRisk && IsLowRiskConsentType(consentType))
                        {
                            _logger.LogInformation("Low-risk consent type {ConsentType} auto-approved for user {UserId}", 
                                consentType, userId);
                            return new ValidateConsentResponse
                            {
                                HasConsent = true,
                                ValidationTimestamp = DateTimeOffset.UtcNow,
                                ConsentRecordId = "auto-approved-low-risk"
                            };
                        }
                    }

                    // Get the latest agent consent record for this type
                    var record = await _dbContext.AgentConsentRecords
                        .AsNoTracking()
                        .Where(c => c.UserId == userId && 
                                   c.TenantId == tenantId && 
                                   c.AgentId == agentId &&
                                   c.ConsentType == consentType)
                        .OrderByDescending(c => c.Timestamp)
                        .FirstOrDefaultAsync();

                    // If no agent-specific record, check for general consent
                    if (record == null)
                    {
                        var generalRecord = await _dbContext.ConsentRecords
                            .AsNoTracking()
                            .Where(c => c.UserId == userId && 
                                       c.TenantId == tenantId && 
                                       c.ConsentType == consentType)
                            .OrderByDescending(c => c.Timestamp)
                            .FirstOrDefaultAsync();

                        if (generalRecord != null)
                        {
                            _logger.LogInformation("Using general consent for agent {AgentId}, consent type {ConsentType}, user {UserId}", 
                                agentId, consentType, userId);
                            
                            return new ValidateConsentResponse
                            {
                                HasConsent = generalRecord.IsGranted,
                                ValidationTimestamp = DateTimeOffset.UtcNow,
                                ConsentRecordId = generalRecord.ConsentId
                            };
                        }
                    }

                    // Check if we have a valid consent record
                    var response = new ValidateConsentResponse
                    {
                        HasConsent = record != null && record.IsGranted,
                        ValidationTimestamp = DateTimeOffset.UtcNow,
                        ConsentRecordId = record?.ConsentId
                    };

                    _logger.LogInformation("Agent consent validated: {ConsentType}, User: {UserId}, Agent: {AgentId}, HasConsent: {HasConsent}", 
                        consentType, userId, agentId, response.HasConsent);
                    
                    return response;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating agent consent: {ConsentType}, User: {UserId}, Agent: {AgentId}", 
                    consentType, userId, agentId);
                throw new ConsentServiceException("Failed to validate agent consent", ex);
            }
        }

        /// <inheritdoc />
        public async Task<Dictionary<ValidateConsentRequest, ValidateConsentResponse>> ValidateConsentBatchAsync(IEnumerable<ValidateConsentRequest> requests)
        {
            if (requests == null)
            {
                throw new ArgumentNullException(nameof(requests));
            }

            var results = new Dictionary<ValidateConsentRequest, ValidateConsentResponse>();

            foreach (var request in requests)
            {
                try
                {
                    var response = await ValidateConsentAsync(request);
                    results[request] = response;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating consent in batch: {ConsentType}, User: {UserId}", 
                        request.RequiredConsentType, request.UserId);
                    
                    // Add a failed response
                    results[request] = new ValidateConsentResponse
                    {
                        HasConsent = false,
                        ValidationTimestamp = DateTimeOffset.UtcNow
                    };
                }
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AgentConsentRecord>> GetAgentConsentHistoryAsync(
            Guid agentId, 
            string tenantId, 
            DateTimeOffset? startDate = null, 
            DateTimeOffset? endDate = null)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var query = _dbContext.AgentConsentRecords
                        .AsNoTracking()
                        .Where(c => c.AgentId == agentId && c.TenantId == tenantId);

                    if (startDate.HasValue)
                    {
                        query = query.Where(c => c.Timestamp >= startDate.Value);
                    }

                    if (endDate.HasValue)
                    {
                        query = query.Where(c => c.Timestamp <= endDate.Value);
                    }

                    return await query.OrderByDescending(c => c.Timestamp).ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting consent history for agent: {AgentId}, Tenant: {TenantId}", 
                    agentId, tenantId);
                throw new ConsentServiceException("Failed to get agent consent history", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> SetAgentConsentPreferencesAsync(string userId, string tenantId, AgentConsentPreferences preferences)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (preferences == null)
            {
                throw new ArgumentNullException(nameof(preferences));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Set the user ID and tenant ID
                    preferences.UserId = userId;
                    preferences.TenantId = tenantId;

                    // Check if preferences already exist
                    var existingPreferences = await _dbContext.AgentConsentPreferences
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);

                    if (existingPreferences != null)
                    {
                        // Update existing preferences
                        existingPreferences.AutoConsentLowRisk = preferences.AutoConsentLowRisk;
                        existingPreferences.RememberDecisions = preferences.RememberDecisions;
                        existingPreferences.ConsentExpiration = preferences.ConsentExpiration;
                        existingPreferences.AgentPreferences = preferences.AgentPreferences;
                        existingPreferences.ConsentTypePreferences = preferences.ConsentTypePreferences;
                    }
                    else
                    {
                        // Add new preferences
                        _dbContext.AgentConsentPreferences.Add(preferences);
                    }

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Agent consent preferences set for user {UserId}, tenant {TenantId}", 
                        userId, tenantId);
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting agent consent preferences for user {UserId}, tenant {TenantId}", 
                    userId, tenantId);
                throw new ConsentServiceException("Failed to set agent consent preferences", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AgentConsentPreferences> GetAgentConsentPreferencesAsync(string userId, string tenantId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var preferences = await _dbContext.AgentConsentPreferences
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);

                    if (preferences == null)
                    {
                        // Return default preferences
                        return new AgentConsentPreferences
                        {
                            UserId = userId,
                            TenantId = tenantId,
                            AutoConsentLowRisk = false,
                            RememberDecisions = true,
                            ConsentExpiration = TimeSpan.FromDays(90),
                            AgentPreferences = new Dictionary<Guid, AgentSpecificPreferences>(),
                            ConsentTypePreferences = new Dictionary<string, bool>()
                        };
                    }

                    return preferences;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting agent consent preferences for user {UserId}, tenant {TenantId}", 
                    userId, tenantId);
                throw new ConsentServiceException("Failed to get agent consent preferences", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RevokeAllAgentConsentsAsync(string userId, string tenantId, Guid agentId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get all agent consent types for this agent
                    var consentTypes = await _dbContext.AgentConsentRecords
                        .AsNoTracking()
                        .Where(c => c.UserId == userId && c.TenantId == tenantId && c.AgentId == agentId)
                        .Select(c => c.ConsentType)
                        .Distinct()
                        .ToListAsync();

                    // Revoke each consent type
                    foreach (var consentType in consentTypes)
                    {
                        // Create a revocation record
                        var record = new AgentConsentRecord
                        {
                            ConsentId = Guid.NewGuid().ToString(),
                            UserId = userId,
                            TenantId = tenantId,
                            ConsentType = consentType,
                            IsGranted = false, // Revoked = not granted
                            Timestamp = DateTimeOffset.UtcNow,
                            Source = "Bulk Revocation API",
                            Evidence = "Explicit bulk revocation",
                            AgentId = agentId,
                            AgentAction = "All"
                        };

                        // Add to database
                        _dbContext.AgentConsentRecords.Add(record);
                    }

                    // Update agent preferences if they exist
                    var preferences = await _dbContext.AgentConsentPreferences
                        .FirstOrDefaultAsync(p => p.UserId == userId && p.TenantId == tenantId);

                    if (preferences != null && preferences.AgentPreferences.ContainsKey(agentId))
                    {
                        // Block the agent
                        preferences.AgentPreferences[agentId].BlockAgent = true;
                        preferences.AgentPreferences[agentId].TrustAgent = false;
                        preferences.AgentPreferences[agentId].PreApprovedConsentTypes.Clear();
                    }

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("All consents revoked for agent {AgentId}, user {UserId}, tenant {TenantId}", 
                        agentId, userId, tenantId);
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all consents for agent {AgentId}, user {UserId}, tenant {TenantId}", 
                    agentId, userId, tenantId);
                throw new ConsentServiceException("Failed to revoke all agent consents", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> IsEmergencyOverrideActiveAsync(string tenantId, Guid agentId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var override_ = await _dbContext.EmergencyOverrides
                        .AsNoTracking()
                        .FirstOrDefaultAsync(o => o.TenantId == tenantId && 
                                                 o.AgentId == agentId && 
                                                 o.ExpiresAt > DateTimeOffset.UtcNow);

                    return override_ != null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking emergency override for agent {AgentId}, tenant {TenantId}", 
                    agentId, tenantId);
                throw new ConsentServiceException("Failed to check emergency override", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ActivateEmergencyOverrideAsync(
            string tenantId, 
            Guid agentId, 
            string requestingUserId, 
            string reason, 
            TimeSpan duration)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (string.IsNullOrEmpty(requestingUserId))
            {
                throw new ArgumentException("Requesting user ID cannot be empty", nameof(requestingUserId));
            }

            if (string.IsNullOrEmpty(reason))
            {
                throw new ArgumentException("Reason cannot be empty", nameof(reason));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Check if an override already exists
                    var existingOverride = await _dbContext.EmergencyOverrides
                        .FirstOrDefaultAsync(o => o.TenantId == tenantId && 
                                                 o.AgentId == agentId && 
                                                 o.ExpiresAt > DateTimeOffset.UtcNow);

                    if (existingOverride != null)
                    {
                        // Update the existing override
                        existingOverride.ExpiresAt = DateTimeOffset.UtcNow.Add(duration);
                        existingOverride.Reason = reason;
                        existingOverride.RequestingUserId = requestingUserId;
                    }
                    else
                    {
                        // Create a new override
                        var override_ = new EmergencyOverride
                        {
                            TenantId = tenantId,
                            AgentId = agentId,
                            ActivatedAt = DateTimeOffset.UtcNow,
                            ExpiresAt = DateTimeOffset.UtcNow.Add(duration),
                            RequestingUserId = requestingUserId,
                            Reason = reason
                        };

                        _dbContext.EmergencyOverrides.Add(override_);
                    }

                    // Create an audit record
                    var auditRecord = new EmergencyOverrideAuditRecord
                    {
                        TenantId = tenantId,
                        AgentId = agentId,
                        Timestamp = DateTimeOffset.UtcNow,
                        ActionType = "Activate",
                        UserId = requestingUserId,
                        Reason = reason,
                        Duration = duration
                    };

                    _dbContext.EmergencyOverrideAuditRecords.Add(auditRecord);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Emergency override activated for agent {AgentId}, tenant {TenantId}, duration {Duration}", 
                        agentId, tenantId, duration);
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating emergency override for agent {AgentId}, tenant {TenantId}", 
                    agentId, tenantId);
                throw new ConsentServiceException("Failed to activate emergency override", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Determines if a consent type is considered low risk.
        /// </summary>
        /// <param name="consentType">The consent type to check</param>
        /// <returns>True if the consent type is low risk; otherwise, false</returns>
        private bool IsLowRiskConsentType(string consentType)
        {
            // Define which agent consent types are considered low risk
            // This is a simplified implementation - in a real system, you would have more complex rules
            return consentType != AgentConsentTypes.HighAuthorityAction &&
                   consentType != AgentConsentTypes.SensitiveDataAccess &&
                   consentType != AgentConsentTypes.AuthorityEscalation;
        }

        #endregion
    }

    /// <summary>
    /// Database context for consent data.
    /// </summary>
    public class ConsentDbContext : DbContext
    {
        public ConsentDbContext(DbContextOptions<ConsentDbContext> options) : base(options)
        {
        }

        public DbSet<ConsentRecord> ConsentRecords { get; set; }
        public DbSet<AgentConsentRecord> AgentConsentRecords { get; set; }
        public DbSet<AgentConsentPreferences> AgentConsentPreferences { get; set; }
        public DbSet<EmergencyOverride> EmergencyOverrides { get; set; }
        public DbSet<EmergencyOverrideAuditRecord> EmergencyOverrideAuditRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure ConsentRecord entity
            modelBuilder.Entity<ConsentRecord>(entity =>
            {
                entity.HasKey(e => e.ConsentId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.ConsentType).IsRequired();
                entity.Property(e => e.IsGranted).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Source).IsRequired();
            });

            // Configure AgentConsentRecord entity
            modelBuilder.Entity<AgentConsentRecord>(entity =>
            {
                entity.HasKey(e => e.ConsentId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.ConsentType).IsRequired();
                entity.Property(e => e.IsGranted).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.Source).IsRequired();
                entity.Property(e => e.AgentId).IsRequired();
                
                // Configure OperationContext as a JSON column
                entity.Property(e => e.OperationContext)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, new System.Text.Json.JsonSerializerOptions()));
            });

            // Configure AgentConsentPreferences entity
            modelBuilder.Entity<AgentConsentPreferences>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TenantId });
                entity.Property(e => e.AutoConsentLowRisk).IsRequired();
                entity.Property(e => e.RememberDecisions).IsRequired();
                
                // Configure AgentPreferences as a JSON column
                entity.Property(e => e.AgentPreferences)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<Guid, AgentSpecificPreferences>>(v, new System.Text.Json.JsonSerializerOptions()));
                
                // Configure ConsentTypePreferences as a JSON column
                entity.Property(e => e.ConsentTypePreferences)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, bool>>(v, new System.Text.Json.JsonSerializerOptions()));
            });

            // Configure EmergencyOverride entity
            modelBuilder.Entity<EmergencyOverride>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.AgentId });
                entity.Property(e => e.ActivatedAt).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.RequestingUserId).IsRequired();
                entity.Property(e => e.Reason).IsRequired();
            });

            // Configure EmergencyOverrideAuditRecord entity
            modelBuilder.Entity<EmergencyOverrideAuditRecord>(entity =>
            {
                entity.HasKey(e => e.AuditId);
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.AgentId).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();
                entity.Property(e => e.ActionType).IsRequired();
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Reason).IsRequired();
            });
        }
    }

    /// <summary>
    /// Represents an emergency override for agent consent.
    /// </summary>
    public class EmergencyOverride
    {
        /// <summary>
        /// The tenant context for this override.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The agent for which consent is overridden.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// When the override was activated.
        /// </summary>
        public DateTimeOffset ActivatedAt { get; set; }

        /// <summary>
        /// When the override expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// The user who requested the override.
        /// </summary>
        public string RequestingUserId { get; set; }

        /// <summary>
        /// The reason for the override.
        /// </summary>
        public string Reason { get; set; }
    }

    /// <summary>
    /// Represents an audit record for emergency override actions.
    /// </summary>
    public class EmergencyOverrideAuditRecord
    {
        /// <summary>
        /// The unique identifier for this audit record.
        /// </summary>
        public Guid AuditId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The tenant context for this audit record.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// The agent for which consent was overridden.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// When the action occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The type of action (e.g., "Activate", "Deactivate").
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// The user who performed the action.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The reason for the action.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// The duration of the override, if applicable.
        /// </summary>
        public TimeSpan? Duration { get; set; }
    }

    /// <summary>
    /// Exception thrown when a consent service operation fails.
    /// </summary>
    public class ConsentServiceException : Exception
    {
        public ConsentServiceException(string message) : base(message)
        {
        }

        public ConsentServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
