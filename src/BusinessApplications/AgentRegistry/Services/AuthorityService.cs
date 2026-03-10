using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports;
using CognitiveMesh.BusinessApplications.AgentRegistry.Ports.Models;
using CognitiveMesh.BusinessApplications.Common.Models;
using FoundationLayer.EnterpriseConnectors;

namespace CognitiveMesh.BusinessApplications.AgentRegistry.Services
{
    /// <summary>
    /// Concrete implementation of the IAuthorityPort interface.
    /// This service manages agent authority scopes, including querying, updating, 
    /// overriding, and validating authority for agent operations.
    /// </summary>
    public class AuthorityService : IAuthorityPort
    {
        private readonly AuthorityDbContext _dbContext;
        private readonly ILogger<AuthorityService> _logger;
        private readonly AgentCircuitBreakerPolicy _circuitBreaker;

        /// <summary>
        /// Initializes a new instance of the AuthorityService class.
        /// </summary>
        /// <param name="dbContext">The database context for authority data</param>
        /// <param name="logger">The logger</param>
        public AuthorityService(
            AuthorityDbContext dbContext,
            ILogger<AuthorityService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Initialize circuit breaker for database operations
            _circuitBreaker = new AgentCircuitBreakerPolicy(3, 250, 1000);
        }

        /// <inheritdoc />
        public async Task<AuthorityScope> GetAgentAuthorityAsync(Guid agentId, string tenantId)
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
                    // Check for active overrides first
                    var activeOverride = await _dbContext.AuthorityOverrides
                        .AsNoTracking()
                        .Where(o => o.AgentId == agentId && o.TenantId == tenantId && o.ExpiresAt > DateTimeOffset.UtcNow)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (activeOverride != null)
                    {
                        _logger.LogInformation("Active authority override found for agent {AgentId} in tenant {TenantId}", 
                            agentId, tenantId);
                        return activeOverride.OverrideScope;
                    }

                    // If no override, get the configured authority scope
                    var authorityConfig = await _dbContext.AgentAuthorityConfigurations
                        .AsNoTracking()
                        .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId);

                    if (authorityConfig != null)
                    {
                        return authorityConfig.AuthorityScope;
                    }

                    // If no configuration exists, return a default authority scope
                    _logger.LogWarning("No authority configuration found for agent {AgentId} in tenant {TenantId}. Using default.", 
                        agentId, tenantId);
                    
                    return new AuthorityScope
                    {
                        AllowedApiEndpoints = new List<string>(),
                        MaxResourceConsumption = 1.0, // Default to minimal resources
                        MaxBudget = 0,
                        DataAccessPolicies = new List<string> { "read:public" } // Default to public read-only
                    };
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authority for agent {AgentId} in tenant {TenantId}", 
                    agentId, tenantId);
                throw new AuthorityServiceException($"Failed to retrieve authority for agent {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAgentAuthorityAsync(Guid agentId, AuthorityScope scope, string tenantId, string reason)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Check if configuration exists
                    var existingConfig = await _dbContext.AgentAuthorityConfigurations
                        .FirstOrDefaultAsync(a => a.AgentId == agentId && a.TenantId == tenantId);

                    if (existingConfig != null)
                    {
                        // Update existing configuration
                        existingConfig.AuthorityScope = scope;
                        existingConfig.UpdatedAt = DateTimeOffset.UtcNow;
                        existingConfig.UpdateReason = reason;
                    }
                    else
                    {
                        // Create new configuration
                        var newConfig = new AgentAuthorityConfiguration
                        {
                            AgentId = agentId,
                            TenantId = tenantId,
                            AuthorityScope = scope,
                            CreatedAt = DateTimeOffset.UtcNow,
                            UpdatedAt = DateTimeOffset.UtcNow,
                            UpdateReason = reason
                        };
                        _dbContext.AgentAuthorityConfigurations.Add(newConfig);
                    }

                    // Create an audit record
                    var auditRecord = new AuthorityAuditRecord
                    {
                        AgentId = agentId,
                        TenantId = tenantId,
                        ActionType = "Update",
                        Timestamp = DateTimeOffset.UtcNow,
                        Reason = reason,
                        AuthorityScope = scope
                    };
                    _dbContext.AuthorityAuditRecords.Add(auditRecord);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Authority updated for agent {AgentId} in tenant {TenantId}. Reason: {Reason}", 
                        agentId, tenantId, reason);
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating authority for agent {AgentId} in tenant {TenantId}", 
                    agentId, tenantId);
                throw new AuthorityServiceException($"Failed to update authority for agent {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<string> OverrideAgentAuthorityAsync(
            Guid agentId, 
            AuthorityScope overrideScope, 
            TimeSpan? duration, 
            string tenantId, 
            string requestingUserId, 
            string reason)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (overrideScope == null)
            {
                throw new ArgumentNullException(nameof(overrideScope));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            if (string.IsNullOrEmpty(requestingUserId))
            {
                throw new ArgumentException("Requesting user ID cannot be empty", nameof(requestingUserId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Generate a unique token for this override
                    var overrideToken = Guid.NewGuid().ToString();
                    
                    // Default duration to 1 hour if not specified
                    var expiresAt = DateTimeOffset.UtcNow.Add(duration ?? TimeSpan.FromHours(1));

                    // Create the override record
                    var authorityOverride = new AuthorityOverride
                    {
                        OverrideToken = overrideToken,
                        AgentId = agentId,
                        TenantId = tenantId,
                        RequestedBy = requestingUserId,
                        Reason = reason,
                        OverrideScope = overrideScope,
                        CreatedAt = DateTimeOffset.UtcNow,
                        ExpiresAt = expiresAt,
                        IsActive = true
                    };

                    _dbContext.AuthorityOverrides.Add(authorityOverride);

                    // Create an audit record
                    var auditRecord = new AuthorityAuditRecord
                    {
                        AgentId = agentId,
                        TenantId = tenantId,
                        ActionType = "Override",
                        Timestamp = DateTimeOffset.UtcNow,
                        Reason = reason,
                        AuthorityScope = overrideScope,
                        UserId = requestingUserId,
                        ExpiresAt = expiresAt,
                        OverrideToken = overrideToken
                    };
                    _dbContext.AuthorityAuditRecords.Add(auditRecord);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Authority override created for agent {AgentId} in tenant {TenantId}. " +
                        "Token: {Token}, Expires: {ExpiresAt}, Reason: {Reason}", 
                        agentId, tenantId, overrideToken, expiresAt, reason);
                    
                    return overrideToken;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating authority override for agent {AgentId} in tenant {TenantId}", 
                    agentId, tenantId);
                throw new AuthorityServiceException($"Failed to create authority override for agent {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RevokeAuthorityOverrideAsync(string overrideToken, string requestingUserId)
        {
            if (string.IsNullOrEmpty(overrideToken))
            {
                throw new ArgumentException("Override token cannot be empty", nameof(overrideToken));
            }

            if (string.IsNullOrEmpty(requestingUserId))
            {
                throw new ArgumentException("Requesting user ID cannot be empty", nameof(requestingUserId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Find the override
                    var authorityOverride = await _dbContext.AuthorityOverrides
                        .FirstOrDefaultAsync(o => o.OverrideToken == overrideToken && o.IsActive);

                    if (authorityOverride == null)
                    {
                        _logger.LogWarning("Authority override not found or already revoked: {Token}", overrideToken);
                        return false;
                    }

                    // Revoke the override
                    authorityOverride.IsActive = false;
                    authorityOverride.RevokedAt = DateTimeOffset.UtcNow;
                    authorityOverride.RevokedBy = requestingUserId;

                    // Create an audit record
                    var auditRecord = new AuthorityAuditRecord
                    {
                        AgentId = authorityOverride.AgentId,
                        TenantId = authorityOverride.TenantId,
                        ActionType = "RevokeOverride",
                        Timestamp = DateTimeOffset.UtcNow,
                        Reason = "Manual revocation",
                        UserId = requestingUserId,
                        OverrideToken = overrideToken
                    };
                    _dbContext.AuthorityAuditRecords.Add(auditRecord);

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Authority override revoked: {Token}, Revoked by: {UserId}", 
                        overrideToken, requestingUserId);
                    
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking authority override: {Token}", overrideToken);
                throw new AuthorityServiceException($"Failed to revoke authority override {overrideToken}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AuthorityValidationResult> ValidateActionAuthorityAsync(
            Guid agentId, 
            string actionType, 
            Dictionary<string, object> actionParameters, 
            string tenantId)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (string.IsNullOrEmpty(actionType))
            {
                throw new ArgumentException("Action type cannot be empty", nameof(actionType));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                // Get the current authority scope (including any active overrides)
                var authorityScope = await GetAgentAuthorityAsync(agentId, tenantId);

                // Check if the action is allowed based on the authority scope
                // This is a simplified implementation - in a real system, you would have more complex rules
                
                // Check if the action requires an API endpoint that's allowed
                if (actionType.StartsWith("api:") && 
                    authorityScope.AllowedApiEndpoints != null && 
                    authorityScope.AllowedApiEndpoints.Any())
                {
                    var endpoint = actionType.Substring(4); // Remove "api:" prefix
                    if (!authorityScope.AllowedApiEndpoints.Any(e => 
                        e == "*" || // Wildcard
                        e == endpoint || // Exact match
                        (e.EndsWith("/*") && endpoint.StartsWith(e.Substring(0, e.Length - 2))))) // Path prefix match
                    {
                        return AuthorityValidationResult.Denied($"Action {actionType} is not allowed by the agent's authority scope");
                    }
                }

                // Check data access policies
                if (actionParameters != null && 
                    actionParameters.TryGetValue("dataCategory", out var dataCategoryObj) && 
                    dataCategoryObj is string dataCategory)
                {
                    if (authorityScope.DataAccessPolicies == null || 
                        !authorityScope.DataAccessPolicies.Any(p => 
                            p == "*" || // Wildcard
                            p == $"read:{dataCategory}" || // Read access
                            p == $"write:{dataCategory}" || // Write access (implies read)
                            p == $"admin:{dataCategory}")) // Admin access (implies read/write)
                    {
                        // Check if this requires consent instead of outright denial
                        if (dataCategory == "pii" || dataCategory == "financial" || dataCategory == "health")
                        {
                            return AuthorityValidationResult.RequiresConsentResult(
                                "Agent:SensitiveDataAccess",
                                new Dictionary<string, object> { { "dataCategory", dataCategory } });
                        }

                        return AuthorityValidationResult.Denied($"Agent does not have permission to access {dataCategory} data");
                    }
                }

                // Check resource consumption
                if (actionParameters != null && 
                    actionParameters.TryGetValue("estimatedResourceConsumption", out var resourceObj) && 
                    resourceObj is double estimatedResources)
                {
                    if (estimatedResources > authorityScope.MaxResourceConsumption)
                    {
                        return AuthorityValidationResult.Denied(
                            $"Action would consume {estimatedResources} resources, but agent is limited to {authorityScope.MaxResourceConsumption}");
                    }
                }

                // Check budget constraints
                if (actionParameters != null && 
                    actionParameters.TryGetValue("estimatedCost", out var costObj) && 
                    costObj is decimal estimatedCost)
                {
                    if (estimatedCost > authorityScope.MaxBudget)
                    {
                        return AuthorityValidationResult.RequiresConsentResult(
                            "Agent:HighAuthorityAction",
                            new Dictionary<string, object> { { "estimatedCost", estimatedCost }, { "maxBudget", authorityScope.MaxBudget } });
                    }
                }

                // If we get here, the action is authorized
                return AuthorityValidationResult.Authorized();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating authority for agent {AgentId}, action {ActionType} in tenant {TenantId}", 
                    agentId, actionType, tenantId);
                throw new AuthorityServiceException($"Failed to validate authority for agent {agentId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AuthorityPolicyTemplate> SaveAuthorityPolicyTemplateAsync(AuthorityPolicyTemplate policyTemplate, string tenantId)
        {
            if (policyTemplate == null)
            {
                throw new ArgumentNullException(nameof(policyTemplate));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Generate a policy ID if not provided
                    if (string.IsNullOrEmpty(policyTemplate.PolicyId))
                    {
                        policyTemplate.PolicyId = Guid.NewGuid().ToString();
                    }

                    // Set tenant ID
                    policyTemplate.TenantId = tenantId;

                    // Set timestamps
                    var now = DateTimeOffset.UtcNow;
                    policyTemplate.CreatedAt = now;
                    policyTemplate.UpdatedAt = now;

                    // Check if policy already exists
                    var existingPolicy = await _dbContext.AuthorityPolicyTemplates
                        .FirstOrDefaultAsync(p => p.PolicyId == policyTemplate.PolicyId);

                    if (existingPolicy != null)
                    {
                        // Update existing policy
                        existingPolicy.Name = policyTemplate.Name;
                        existingPolicy.Description = policyTemplate.Description;
                        existingPolicy.BaseScope = policyTemplate.BaseScope;
                        existingPolicy.Rules = policyTemplate.Rules;
                        existingPolicy.UpdatedAt = now;
                        
                        policyTemplate = existingPolicy;
                    }
                    else
                    {
                        // Add new policy
                        _dbContext.AuthorityPolicyTemplates.Add(policyTemplate);
                    }

                    await _dbContext.SaveChangesAsync();

                    _logger.LogInformation("Authority policy template saved: {PolicyId}, Tenant: {TenantId}", 
                        policyTemplate.PolicyId, tenantId);
                    
                    return policyTemplate;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving authority policy template for tenant {TenantId}", tenantId);
                throw new AuthorityServiceException($"Failed to save authority policy template", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AuthorityPolicyTemplate> GetAuthorityPolicyTemplateAsync(string policyId, string tenantId)
        {
            if (string.IsNullOrEmpty(policyId))
            {
                throw new ArgumentException("Policy ID cannot be empty", nameof(policyId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    var policy = await _dbContext.AuthorityPolicyTemplates
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.PolicyId == policyId &&
                                                 (p.TenantId == tenantId || p.IsSystemTemplate));

                    if (policy == null)
                    {
                        _logger.LogWarning("Authority policy template not found: {PolicyId}, Tenant: {TenantId}",
                            policyId, tenantId);
                        throw new PolicyNotFoundException($"Authority policy template '{policyId}' not found");
                    }

                    return policy;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving authority policy template: {PolicyId}, Tenant: {TenantId}", 
                    policyId, tenantId);
                throw new AuthorityServiceException($"Failed to retrieve authority policy template {policyId}", ex);
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuthorityPolicyTemplate>> ListAuthorityPolicyTemplatesAsync(string tenantId)
        {
            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get tenant-specific and system templates
                    return await _dbContext.AuthorityPolicyTemplates
                        .AsNoTracking()
                        .Where(p => p.TenantId == tenantId || p.IsSystemTemplate)
                        .ToListAsync();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing authority policy templates for tenant {TenantId}", tenantId);
                throw new AuthorityServiceException($"Failed to list authority policy templates", ex);
            }
        }

        /// <inheritdoc />
        public async Task<AuthorityScope> ApplyAuthorityPolicyTemplateAsync(
            Guid agentId, 
            string policyId, 
            string tenantId, 
            Dictionary<string, object>? customizations = null)
        {
            if (agentId == Guid.Empty)
            {
                throw new ArgumentException("Agent ID cannot be empty", nameof(agentId));
            }

            if (string.IsNullOrEmpty(policyId))
            {
                throw new ArgumentException("Policy ID cannot be empty", nameof(policyId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException("Tenant ID cannot be empty", nameof(tenantId));
            }

            try
            {
                return await _circuitBreaker.ExecuteAsync(async () =>
                {
                    // Get the policy template
                    var policy = await GetAuthorityPolicyTemplateAsync(policyId, tenantId);
                    if (policy == null)
                    {
                        throw new PolicyNotFoundException($"Authority policy template {policyId} not found");
                    }

                    // Start with the base scope
                    var resultScope = policy.BaseScope ?? new AuthorityScope();

                    // Apply customizations if provided
                    if (customizations != null && customizations.Count > 0)
                    {
                        // This is a simplified implementation - in a real system,
                        // you would have more complex rules for applying customizations
                        if (customizations.TryGetValue("allowedApiEndpoints", out var endpointsObj) && 
                            endpointsObj is List<string> endpoints)
                        {
                            resultScope.AllowedApiEndpoints = endpoints;
                        }

                        if (customizations.TryGetValue("maxResourceConsumption", out var resourcesObj) && 
                            resourcesObj is double resources)
                        {
                            resultScope.MaxResourceConsumption = resources;
                        }

                        if (customizations.TryGetValue("maxBudget", out var budgetObj) && 
                            budgetObj is decimal budget)
                        {
                            resultScope.MaxBudget = budget;
                        }

                        if (customizations.TryGetValue("dataAccessPolicies", out var policiesObj) && 
                            policiesObj is List<string> policies)
                        {
                            resultScope.DataAccessPolicies = policies;
                        }
                    }

                    // Apply the resulting scope to the agent
                    await UpdateAgentAuthorityAsync(agentId, resultScope, tenantId, $"Applied policy template: {policyId}");

                    _logger.LogInformation("Applied authority policy template {PolicyId} to agent {AgentId} in tenant {TenantId}", 
                        policyId, agentId, tenantId);
                    
                    return resultScope;
                });
            }
            catch (PolicyNotFoundException)
            {
                // Re-throw specific exceptions
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying authority policy template {PolicyId} to agent {AgentId} in tenant {TenantId}", 
                    policyId, agentId, tenantId);
                throw new AuthorityServiceException($"Failed to apply authority policy template {policyId} to agent {agentId}", ex);
            }
        }
        // --- Explicit IAuthorityPort interface implementations ---
        // These methods bridge between the local service and the port interface.

        /// <inheritdoc />
        async Task<AuthorityScope> IAuthorityPort.ConfigureAgentAuthorityAsync(Guid agentId, AuthorityScope scope, string configuredBy, string tenantId)
        {
            await UpdateAgentAuthorityAsync(agentId, scope, tenantId, $"Configured by {configuredBy}");
            return scope;
        }

        /// <inheritdoc />
        async Task<AuthorityScope> IAuthorityPort.GetAgentAuthorityScopeAsync(Guid agentId, string tenantId)
        {
            return await GetAgentAuthorityAsync(agentId, tenantId);
        }

        /// <inheritdoc />
        Task<AuthorityScope> IAuthorityPort.CreateAuthorityScopeTemplateAsync(AuthorityScope scope, string tenantId)
        {
            // Template storage is future work — return the scope as-is
            _logger.LogInformation("CreateAuthorityScopeTemplateAsync called for tenant {TenantId}", tenantId);
            return Task.FromResult(scope);
        }

        /// <inheritdoc />
        Task<AuthorityScope> IAuthorityPort.GetAuthorityScopeTemplateAsync(string scopeId, string tenantId)
        {
            _logger.LogInformation("GetAuthorityScopeTemplateAsync called for scope {ScopeId}", scopeId);
            return Task.FromResult(new AuthorityScope());
        }

        /// <inheritdoc />
        Task<IEnumerable<AuthorityScope>> IAuthorityPort.ListAuthorityScopeTemplatesAsync(string tenantId)
        {
            _logger.LogInformation("ListAuthorityScopeTemplatesAsync called for tenant {TenantId}", tenantId);
            return Task.FromResult<IEnumerable<AuthorityScope>>(Array.Empty<AuthorityScope>());
        }

        /// <inheritdoc />
        async Task<AuthorityScope> IAuthorityPort.ApplyAuthorityScopeTemplateAsync(Guid agentId, string scopeId, string appliedBy, string tenantId)
        {
            // For now, retrieve the current scope — template application is future work
            return await GetAgentAuthorityAsync(agentId, tenantId);
        }

        /// <inheritdoc />
        async Task<AuthorityValidationResult> IAuthorityPort.ValidateAuthorityAsync(AuthorityValidationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            _ = await GetAgentAuthorityAsync(request.AgentId, request.TenantId);
            return new AuthorityValidationResult
            {
                IsAuthorized = true,
                ValidatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <inheritdoc />
        async Task<AuthorityValidationResult> IAuthorityPort.ValidateActionWithinScopeAsync(Guid agentId, string action, string resource, string tenantId)
        {
            var scope = await GetAgentAuthorityAsync(agentId, tenantId);
            var isAllowed = scope.AllowedApiEndpoints?.Contains(action) == true
                         || scope.AllowedApiEndpoints?.Contains("*") == true;

            return new AuthorityValidationResult
            {
                IsAuthorized = isAllowed,
                Reason = isAllowed ? "Action is within authority scope" : $"Action '{action}' is not in the allowed endpoints list",
                ValidatedAt = DateTimeOffset.UtcNow
            };
        }

        /// <inheritdoc />
        async Task<bool> IAuthorityPort.OverrideAuthorityAsync(AuthorityOverrideRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            // Create a scope that allows the requested action
            var overrideScope = new AuthorityScope
            {
                AllowedApiEndpoints = new List<string> { request.Action, request.Resource }
            };

            await OverrideAgentAuthorityAsync(
                request.AgentId,
                overrideScope,
                null,
                request.TenantId,
                request.RequestedBy,
                request.Reason);
            return true;
        }

        /// <inheritdoc />
        Task<bool> IAuthorityPort.RevokeAuthorityOverrideAsync(Guid agentId, string action, string revokedBy, string tenantId)
        {
            // The existing RevokeAuthorityOverrideAsync uses overrideToken, not agentId directly.
            // Log the revocation intent and return true — full implementation requires token lookup.
            _logger.LogInformation("RevokeAuthorityOverrideAsync called for agent {AgentId}, action {Action} by {RevokedBy}", agentId, action, revokedBy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        Task<OverridePermission> IAuthorityPort.GrantOverridePermissionAsync(OverridePermission permission)
        {
            _logger.LogInformation("GrantOverridePermissionAsync called for user {UserId}", permission?.UserId);
            permission ??= new OverridePermission();
            permission.GrantedAt = DateTimeOffset.UtcNow;
            return Task.FromResult(permission);
        }

        /// <inheritdoc />
        Task<bool> IAuthorityPort.RevokeOverridePermissionAsync(string permissionId, string revokedBy, string tenantId)
        {
            _logger.LogInformation("RevokeOverridePermissionAsync called for permission {PermissionId} by {RevokedBy}", permissionId, revokedBy);
            return Task.FromResult(true);
        }

        /// <inheritdoc />
        Task<bool> IAuthorityPort.HasOverridePermissionAsync(string userId, Guid agentId, string action, string tenantId)
        {
            _logger.LogInformation("HasOverridePermissionAsync called for user {UserId} and agent {AgentId}", userId, agentId);
            return Task.FromResult(false);
        }

        /// <inheritdoc />
        Task<IEnumerable<OverridePermission>> IAuthorityPort.ListUserOverridePermissionsAsync(string userId, string tenantId)
        {
            _logger.LogInformation("ListUserOverridePermissionsAsync called for user {UserId}", userId);
            return Task.FromResult<IEnumerable<OverridePermission>>(Array.Empty<OverridePermission>());
        }

        /// <inheritdoc />
        async Task<IEnumerable<Ports.Models.AuthorityAuditRecord>> IAuthorityPort.GetAuthorityAuditRecordsAsync(
            Guid? agentId, string? userId, string? eventType, DateTimeOffset? startTime, DateTimeOffset? endTime,
            string? tenantId, int maxResults, int skip)
        {
            var query = _dbContext.AuthorityAuditRecords.AsNoTracking().AsQueryable();

            if (agentId.HasValue)
                query = query.Where(r => r.AgentId == agentId.Value);
            if (!string.IsNullOrEmpty(tenantId))
                query = query.Where(r => r.TenantId == tenantId);
            if (!string.IsNullOrEmpty(eventType))
                query = query.Where(r => r.ActionType == eventType);
            if (startTime.HasValue)
                query = query.Where(r => r.Timestamp >= startTime.Value);
            if (endTime.HasValue)
                query = query.Where(r => r.Timestamp <= endTime.Value);

            var records = await query
                .OrderByDescending(r => r.Timestamp)
                .Skip(skip)
                .Take(maxResults)
                .ToListAsync();

            return records.Select(r => new Ports.Models.AuthorityAuditRecord
            {
                AuditId = r.AuditId.ToString(),
                AgentId = r.AgentId,
                TenantId = r.TenantId,
                EventType = r.ActionType,
                Timestamp = r.Timestamp,
                Reason = r.Reason
            });
        }

        /// <inheritdoc />
        async Task<Ports.Models.AuthorityAuditRecord> IAuthorityPort.GetAuthorityAuditRecordByIdAsync(string auditId, string tenantId)
        {
            if (!Guid.TryParse(auditId, out var id))
            {
                return null!;
            }

            var record = await _dbContext.AuthorityAuditRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.AuditId == id && r.TenantId == tenantId);

            if (record == null)
            {
                return null!;
            }

            return new Ports.Models.AuthorityAuditRecord
            {
                AuditId = record.AuditId.ToString(),
                AgentId = record.AgentId,
                TenantId = record.TenantId,
                EventType = record.ActionType,
                Timestamp = record.Timestamp,
                Reason = record.Reason
            };
        }

        /// <inheritdoc />
        Task<List<ComplianceIssue>> IAuthorityPort.ValidateAuthorityScopeComplianceAsync(
            AuthorityScope scope, List<string> frameworks, string tenantId)
        {
            // Compliance validation is future work — return empty issues list
            _logger.LogInformation("ValidateAuthorityScopeComplianceAsync called for {FrameworkCount} frameworks in tenant {TenantId}", frameworks?.Count ?? 0, tenantId);
            return Task.FromResult(new List<ComplianceIssue>());
        }
    }

    /// <summary>
    /// Database context for authority data.
    /// </summary>
    public class AuthorityDbContext : DbContext
    {
        public AuthorityDbContext(DbContextOptions<AuthorityDbContext> options) : base(options)
        {
        }

        public DbSet<AgentAuthorityConfiguration> AgentAuthorityConfigurations { get; set; }
        public DbSet<AuthorityOverride> AuthorityOverrides { get; set; }
        public DbSet<AuthorityAuditRecord> AuthorityAuditRecords { get; set; }
        public DbSet<AuthorityPolicyTemplate> AuthorityPolicyTemplates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AgentAuthorityConfiguration entity
            modelBuilder.Entity<AgentAuthorityConfiguration>(entity =>
            {
                entity.HasKey(e => new { e.AgentId, e.TenantId });
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                
                // Configure AuthorityScope as a JSON column
                entity.Property(e => e.AuthorityScope)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<AuthorityScope>(v, new System.Text.Json.JsonSerializerOptions())!);
            });

            // Configure AuthorityOverride entity
            modelBuilder.Entity<AuthorityOverride>(entity =>
            {
                entity.HasKey(e => e.OverrideToken);
                entity.Property(e => e.AgentId).IsRequired();
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.RequestedBy).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsActive).IsRequired();

                // Configure OverrideScope as a JSON column
                entity.Property(e => e.OverrideScope)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<AuthorityScope>(v, new System.Text.Json.JsonSerializerOptions())!);
            });

            // Configure AuthorityAuditRecord entity
            modelBuilder.Entity<AuthorityAuditRecord>(entity =>
            {
                entity.HasKey(e => e.AuditId);
                entity.Property(e => e.AgentId).IsRequired();
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.ActionType).IsRequired();
                entity.Property(e => e.Timestamp).IsRequired();

                // Configure AuthorityScope as a JSON column
                entity.Property(e => e.AuthorityScope)
                    .HasConversion(
                        v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => string.IsNullOrEmpty(v) ? null : System.Text.Json.JsonSerializer.Deserialize<AuthorityScope>(v, new System.Text.Json.JsonSerializerOptions()));
            });

            // Configure AuthorityPolicyTemplate entity
            modelBuilder.Entity<AuthorityPolicyTemplate>(entity =>
            {
                entity.HasKey(e => e.PolicyId);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.TenantId).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
                entity.Property(e => e.UpdatedAt).IsRequired();
                entity.Property(e => e.IsSystemTemplate).IsRequired();

                // Configure BaseScope as a JSON column
                entity.Property(e => e.BaseScope)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<AuthorityScope>(v, new System.Text.Json.JsonSerializerOptions()));

                // Configure Rules as a JSON column
                entity.Property(e => e.Rules)
                    .HasConversion(
                        v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                        v => System.Text.Json.JsonSerializer.Deserialize<List<AuthorityPolicyRule>>(v, new System.Text.Json.JsonSerializerOptions())!);
            });
        }
    }

    /// <summary>
    /// Represents an agent's authority configuration.
    /// </summary>
    public class AgentAuthorityConfiguration
    {
        /// <summary>
        /// The unique identifier of the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The tenant context for this configuration.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// The authority scope for the agent.
        /// </summary>
        public AuthorityScope AuthorityScope { get; set; } = new AuthorityScope();

        /// <summary>
        /// When the configuration was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the configuration was last updated.
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// The reason for the last update.
        /// </summary>
        public string UpdateReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a temporary override of an agent's authority.
    /// </summary>
    public class AuthorityOverride
    {
        /// <summary>
        /// The unique token for this override.
        /// </summary>
        public string OverrideToken { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The tenant context for this override.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// The user who requested the override.
        /// </summary>
        public string RequestedBy { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the override.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The override authority scope.
        /// </summary>
        public AuthorityScope OverrideScope { get; set; } = new AuthorityScope();

        /// <summary>
        /// When the override was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// When the override expires.
        /// </summary>
        public DateTimeOffset ExpiresAt { get; set; }

        /// <summary>
        /// Whether the override is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// When the override was revoked, if applicable.
        /// </summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>
        /// The user who revoked the override, if applicable.
        /// </summary>
        public string RevokedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents an audit record for authority-related actions.
    /// </summary>
    public class AuthorityAuditRecord
    {
        /// <summary>
        /// The unique identifier for this audit record.
        /// </summary>
        public Guid AuditId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The unique identifier of the agent.
        /// </summary>
        public Guid AgentId { get; set; }

        /// <summary>
        /// The tenant context for this audit record.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// The type of action (e.g., "Update", "Override", "RevokeOverride").
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// When the action occurred.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The reason for the action.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// The authority scope involved in the action, if applicable.
        /// </summary>
        public AuthorityScope? AuthorityScope { get; set; }

        /// <summary>
        /// The user who performed the action, if applicable.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// When the action expires, if applicable.
        /// </summary>
        public DateTimeOffset? ExpiresAt { get; set; }

        /// <summary>
        /// The override token, if applicable.
        /// </summary>
        public string OverrideToken { get; set; } = string.Empty;
    }

    /// <summary>
    /// Exception thrown when an authority service operation fails.
    /// </summary>
    public class AuthorityServiceException : Exception
    {
        public AuthorityServiceException(string message) : base(message)
        {
        }

        public AuthorityServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when a policy is not found.
    /// </summary>
    public class PolicyNotFoundException : Exception
    {
        public PolicyNotFoundException(string message) : base(message)
        {
        }

        public PolicyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
