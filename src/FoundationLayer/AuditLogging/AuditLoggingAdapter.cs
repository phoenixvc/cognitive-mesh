using System.Text.Json;
using FoundationLayer.AuditLogging.Models;

namespace FoundationLayer.AuditLogging;
// <-- new models (ethical/legal events)

/// <summary>
/// Adapter for logging agent-related audit events to a persistent store.
/// Implements circuit breaker pattern and retry logic for resilient event logging.
/// </summary>
public class AuditLoggingAdapter : IAuditLoggingAdapter
{
    private readonly ILogger<AuditLoggingAdapter> _logger;
    private readonly IAuditEventRepository _repository;
    private readonly AgentCircuitBreakerPolicy _circuitBreaker;
    private readonly Queue<AuditEvent> _retryQueue = new Queue<AuditEvent>();
    private readonly SemaphoreSlim _queueLock = new SemaphoreSlim(1, 1);
    private readonly Timer _retryTimer;
    private bool _processingRetryQueue;

    /// <summary>
    /// Initializes a new instance of the AuditLoggingAdapter class.
    /// </summary>
    /// <param name="repository">The repository for storing audit events</param>
    /// <param name="logger">The logger</param>
    public AuditLoggingAdapter(
        IAuditEventRepository repository,
        ILogger<AuditLoggingAdapter> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
        // Initialize circuit breaker with 3 retries, 250ms initial delay, 1s max delay, 50ms jitter
        _circuitBreaker = new AgentCircuitBreakerPolicy(3, 250, 1000, 50);
            
        // Initialize retry timer to process queued events every 30 seconds
        _retryTimer = new Timer(ProcessRetryQueue, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
    }

    /// <inheritdoc />
    public async Task<bool> LogAgentEventAsync(AgentAuditEventType eventType, object eventData, string? correlationId = null)
    {
        if (eventData == null)
        {
            throw new ArgumentNullException(nameof(eventData));
        }
            
        // Ensure correlationId is not null by providing a default value if needed
        correlationId ??= Guid.NewGuid().ToString();

        var auditEvent = new AuditEvent
        {
            EventId = Guid.NewGuid().ToString(),
            EventType = eventType.ToString(),
            EventCategory = "Agent",
            Timestamp = DateTimeOffset.UtcNow,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            EventData = SerializeEventData(eventData)
        };

        try
        {
            // Use circuit breaker pattern to handle potential outages
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                await _repository.SaveEventAsync(auditEvent);
                _logger.LogDebug("Successfully logged agent audit event: {EventType}, ID: {EventId}", 
                    auditEvent.EventType, auditEvent.EventId);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log agent audit event: {EventType}. Queueing for retry.", 
                auditEvent.EventType);
                
            // Queue the event for retry
            await EnqueueForRetryAsync(auditEvent);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> LogAgentRegisteredAsync(Guid agentId, string agentType, string registeredBy, string correlationId = null)
    {
        var eventData = new AgentRegisteredEvent
        {
            AgentId = agentId,
            AgentType = agentType,
            RegisteredBy = registeredBy,
            RegisteredAt = DateTimeOffset.UtcNow
        };

        return await LogAgentEventAsync(AgentAuditEventType.AgentRegistered, eventData, correlationId);
    }

    /// <inheritdoc />
    public async Task<bool> LogAgentRetiredAsync(Guid agentId, string retiredBy, string reason, string correlationId = null)
    {
        var eventData = new AgentRetiredEvent
        {
            AgentId = agentId,
            RetiredBy = retiredBy,
            RetiredAt = DateTimeOffset.UtcNow,
            Reason = reason
        };

        return await LogAgentEventAsync(AgentAuditEventType.AgentRetired, eventData, correlationId);
    }

    /// <inheritdoc />
    public async Task<bool> LogAgentActionExecutedAsync(
        Guid agentId, 
        string actionType, 
        string userId, 
        Dictionary<string, object> parameters, 
        bool success,
        string correlationId = null)
    {
        var eventData = new AgentActionExecutedEvent
        {
            AgentId = agentId,
            ActionType = actionType,
            UserId = userId,
            Parameters = parameters,
            ExecutedAt = DateTimeOffset.UtcNow,
            Success = success
        };

        return await LogAgentEventAsync(AgentAuditEventType.AgentActionExecuted, eventData, correlationId);
    }

    /// <inheritdoc />
    public async Task<bool> LogAuthorityOverriddenAsync(
        Guid agentId, 
        string overriddenBy, 
        string reason, 
        Dictionary<string, object> overrideDetails, 
        string correlationId = null)
    {
        var eventData = new AuthorityOverriddenEvent
        {
            AgentId = agentId,
            OverriddenBy = overriddenBy,
            OverriddenAt = DateTimeOffset.UtcNow,
            Reason = reason,
            OverrideDetails = overrideDetails
        };

        return await LogAgentEventAsync(AgentAuditEventType.AuthorityOverridden, eventData, correlationId);
    }

    /// <inheritdoc />
    public async Task<bool> LogConsentRequestedAsync(
        Guid agentId, 
        string userId, 
        string consentType, 
        string correlationId = null)
    {
        var eventData = new ConsentRequestedEvent
        {
            AgentId = agentId,
            UserId = userId,
            ConsentType = consentType,
            RequestedAt = DateTimeOffset.UtcNow
        };

        return await LogAgentEventAsync(AgentAuditEventType.ConsentRequested, eventData, correlationId);
    }

    /// <inheritdoc />
    public async Task<bool> LogConsentDecisionAsync(
        Guid agentId, 
        string userId, 
        string consentType, 
        bool granted, 
        string correlationId = null)
    {
        var eventData = new ConsentDecisionEvent
        {
            AgentId = agentId,
            UserId = userId,
            ConsentType = consentType,
            Granted = granted,
            DecisionAt = DateTimeOffset.UtcNow
        };

        return await LogAgentEventAsync(
            granted ? AgentAuditEventType.ConsentGranted : AgentAuditEventType.ConsentDenied, 
            eventData, 
            correlationId);
    }

    // -----------------------------------------------------------------
    //             Value-Generation Backend helper wrappers
    // -----------------------------------------------------------------

    /// <summary>
    /// Logs a value diagnostic run event (ValueGeneration module).
    /// </summary>
    public Task<bool> LogValueDiagnosticRunAsync(
        string targetId,
        string targetType,
        double valueScore,
        string valueProfile,
        string correlationId = null)
    {
        var ev = new ValueDiagnosticRunEvent
        {
            TargetId = targetId,
            TargetType = targetType,
            ValueScore = valueScore,
            ValueProfile = valueProfile,
            ExecutedAt = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.ValueDiagnosticRun, ev, correlationId);
    }

    /// <summary>
    /// Logs an organizational blindness detection event.
    /// </summary>
    public Task<bool> LogOrgBlindnessDetectedAsync(
        string organizationId,
        double blindnessRiskScore,
        int blindSpotCount,
        string correlationId = null)
    {
        var ev = new OrgBlindnessDetectedEvent
        {
            OrganizationId = organizationId,
            BlindnessRiskScore = blindnessRiskScore,
            BlindSpotCount = blindSpotCount,
            DetectedAt = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.OrgBlindnessDetected, ev, correlationId);
    }

    /// <summary>
    /// Logs an employability risk flagged event.
    /// </summary>
    public Task<bool> LogEmployabilityRiskFlaggedAsync(
        string userId,
        double riskScore,
        string riskLevel,
        string correlationId = null)
    {
        var ev = new EmployabilityRiskFlaggedEvent
        {
            UserId = userId,
            RiskScore = riskScore,
            RiskLevel = riskLevel,
            FlaggedAt = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.EmployabilityRiskFlagged, ev, correlationId);
    }

    /// <summary>
    /// Logs that a manual adjudication request has been created.
    /// </summary>
    public Task<bool> LogManualAdjudicationRequestedAsync(
        string reviewId,
        string reviewType,
        string subjectId,
        string subjectType,
        string priority,
        string correlationId = null)
    {
        var ev = new ManualAdjudicationRequestedEvent
        {
            ReviewId = reviewId,
            ReviewType = reviewType,
            SubjectId = subjectId,
            SubjectType = subjectType,
            Priority = priority,
            RequestedAt = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.ManualAdjudicationRequested, ev, correlationId);
    }

    // -----------------------------------------------------------------
    //      Ethical & Legal Compliance Framework helper wrappers
    // -----------------------------------------------------------------

    /// <summary>
    /// Logs that a new governance / ethical / legal policy has been approved.
    /// </summary>
    public Task<bool> LogPolicyApprovedAsync(
        string policyId,
        string policyType,
        string policyVersion,
        string approvedBy,
        List<string> stakeholders,
        string description,
        string tenantId,
        string correlationId = null)
    {
        var ev = new PolicyApprovedEvent
        {
            PolicyId     = policyId,
            PolicyType   = policyType,
            PolicyVersion = policyVersion,
            ApprovedBy   = approvedBy,
            Stakeholders = stakeholders ?? new List<string>(),
            Description  = description,
            ApprovedAt   = DateTimeOffset.UtcNow,
            TenantId     = tenantId
        };

        return LogAgentEventAsync(AgentAuditEventType.PolicyApproved, ev, correlationId);
    }

    /// <summary>
    /// Logs when an existing policy has been rolled back to a previous version.
    /// </summary>
    public Task<bool> LogPolicyRolledBackAsync(
        string policyId,
        string policyType,
        string previousVersion,
        string newVersion,
        string rolledBackBy,
        string reason,
        string tenantId,
        string correlationId = null)
    {
        var ev = new PolicyRolledBackEvent
        {
            PolicyId        = policyId,
            PolicyType      = policyType,
            PreviousVersion = previousVersion,
            NewVersion      = newVersion,
            RolledBackBy    = rolledBackBy,
            Reason          = reason,
            RolledBackAt    = DateTimeOffset.UtcNow,
            TenantId        = tenantId
        };

        return LogAgentEventAsync(AgentAuditEventType.PolicyRolledBack, ev, correlationId);
    }

    /// <summary>
    /// Logs a governance violation detected by the monitoring engine.
    /// </summary>
    public Task<bool> LogGovernanceViolationAsync(
        string violationId,
        string policyId,
        string violationType,
        string severity,
        Dictionary<string, object> context,
        string description,
        string tenantId,
        string detectedBy,
        string correlationId = null)
    {
        var ev = new GovernanceViolationEvent
        {
            ViolationId = violationId,
            PolicyId    = policyId,
            ViolationType = violationType,
            Severity    = severity,
            Context     = context ?? new Dictionary<string, object>(),
            Description = description,
            TenantId    = tenantId,
            DetectedBy  = detectedBy,
            DetectedAt  = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.GovernanceViolation, ev, correlationId);
    }

    /// <summary>
    /// Logs the result of an ethical assessment performed on an agent action or reasoning chain.
    /// </summary>
    public Task<bool> LogEthicalAssessmentPerformedAsync(
        string assessmentId,
        string assessmentType,
        Guid agentId,
        string actionType,
        bool isValid,
        List<string> violations,
        string reasoningTrace,
        string tenantId,
        string correlationId = null)
    {
        var ev = new EthicalAssessmentPerformedEvent
        {
            AssessmentId      = assessmentId,
            AssessmentType    = assessmentType,
            AgentId           = agentId,
            ActionType        = actionType,
            IsValid           = isValid,
            ViolationsDetected = violations ?? new List<string>(),
            ReasoningTrace    = reasoningTrace,
            TenantId          = tenantId,
            AssessmentTimestamp = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.EthicalAssessmentPerformed, ev, correlationId);
    }

    /// <summary>
    /// Logs the outcome of a legal compliance check (e.g., GDPR, EU AI Act).
    /// </summary>
    public Task<bool> LogLegalComplianceCheckedAsync(
        string complianceCheckId,
        string regulationType,
        string dataSubjectId,
        bool isCompliant,
        List<string> complianceIssues,
        List<string> regulationSections,
        string checkedBy,
        string tenantId,
        string correlationId = null)
    {
        var ev = new LegalComplianceCheckedEvent
        {
            ComplianceCheckId = complianceCheckId,
            RegulationType    = regulationType,
            DataSubjectId     = dataSubjectId,
            IsCompliant       = isCompliant,
            ComplianceIssues  = complianceIssues ?? new List<string>(),
            RegulationSections = regulationSections ?? new List<string>(),
            CheckedBy         = checkedBy,
            TenantId          = tenantId,
            CheckedAt         = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.LegalComplianceChecked, ev, correlationId);
    }

    /// <summary>
    /// Logs an informational dignity violation detected by the Information Ethics engine.
    /// </summary>
    public Task<bool> LogInformationalDignityViolationAsync(
        string violationId,
        string subjectId,
        string dataType,
        string action,
        string violationType,
        string severity,
        string description,
        string tenantId,
        string correlationId = null)
    {
        var ev = new InformationalDignityViolationEvent
        {
            ViolationId  = violationId,
            SubjectId    = subjectId,
            DataType     = dataType,
            Action       = action,
            ViolationType = violationType,
            Severity     = severity,
            Description  = description,
            TenantId     = tenantId,
            DetectedAt   = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.InformationalDignityViolation, ev, correlationId);
    }

    /// <summary>
    /// Logs a cross-cultural adaptation action executed for a user.
    /// </summary>
    public Task<bool> LogCrossCulturalAdaptationAsync(
        string adaptationId,
        string userId,
        Dictionary<string, object> culturalDimensions,
        string adaptationType,
        string adaptationContext,
        string originalContent,
        string adaptedContent,
        string tenantId,
        string correlationId = null)
    {
        var ev = new CrossCulturalAdaptationEvent
        {
            AdaptationId      = adaptationId,
            UserId            = userId,
            CulturalDimensions = culturalDimensions ?? new Dictionary<string, object>(),
            AdaptationType    = adaptationType,
            AdaptationContext = adaptationContext,
            OriginalContent   = originalContent,
            AdaptedContent    = adaptedContent,
            TenantId          = tenantId,
            AdaptedAt         = DateTimeOffset.UtcNow
        };

        return LogAgentEventAsync(AgentAuditEventType.CrossCulturalAdaptation, ev, correlationId);
    }
    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _repository.SearchEventsAsync(criteria);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search audit events with criteria: {Criteria}", 
                JsonSerializer.Serialize(criteria));
            throw new AuditSearchException("Failed to search audit events", ex);
        }
    }

    /// <inheritdoc />
    public async Task<AuditEvent> GetEventByIdAsync(string eventId)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _repository.GetEventByIdAsync(eventId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit event with ID: {EventId}", eventId);
            throw new AuditSearchException($"Failed to get audit event with ID: {eventId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> GetEventsByCorrelationIdAsync(string correlationId)
    {
        try
        {
            return await _circuitBreaker.ExecuteAsync(async () =>
            {
                return await _repository.GetEventsByCorrelationIdAsync(correlationId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get audit events with correlation ID: {CorrelationId}", correlationId);
            throw new AuditSearchException($"Failed to get audit events with correlation ID: {correlationId}", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuditEvent>> GetAgentEventsAsync(Guid agentId, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null)
    {
        var criteria = new AuditSearchCriteria
        {
            EventDataContains = new Dictionary<string, object> { { "AgentId", agentId } },
            StartTime = startTime,
            EndTime = endTime,
            MaxResults = 1000
        };

        return await SearchEventsAsync(criteria);
    }

    /// <summary>
    /// Enqueues an audit event for retry.
    /// </summary>
    /// <param name="auditEvent">The audit event to enqueue</param>
    private async Task EnqueueForRetryAsync(AuditEvent auditEvent)
    {
        await _queueLock.WaitAsync();
        try
        {
            _retryQueue.Enqueue(auditEvent);
            _logger.LogInformation("Enqueued audit event for retry: {EventType}, ID: {EventId}. Queue size: {QueueSize}", 
                auditEvent.EventType, auditEvent.EventId, _retryQueue.Count);
        }
        finally
        {
            _queueLock.Release();
        }
    }

    /// <summary>
    /// Processes the retry queue.
    /// </summary>
    private async void ProcessRetryQueue(object state)
    {
        if (_processingRetryQueue)
        {
            return; // Already processing
        }

        _processingRetryQueue = true;
        try
        {
            await _queueLock.WaitAsync();
            try
            {
                if (_retryQueue.Count == 0)
                {
                    return; // Nothing to process
                }

                _logger.LogInformation("Processing retry queue. Items: {Count}", _retryQueue.Count);
                    
                // Process up to 100 items at a time
                int itemsToProcess = Math.Min(_retryQueue.Count, 100);
                for (int i = 0; i < itemsToProcess; i++)
                {
                    if (_retryQueue.Count == 0)
                    {
                        break;
                    }

                    var auditEvent = _retryQueue.Dequeue();
                    try
                    {
                        await _repository.SaveEventAsync(auditEvent);
                        _logger.LogInformation("Successfully retried audit event: {EventType}, ID: {EventId}", 
                            auditEvent.EventType, auditEvent.EventId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to retry audit event: {EventType}, ID: {EventId}. Re-enqueueing.", 
                            auditEvent.EventType, auditEvent.EventId);
                        _retryQueue.Enqueue(auditEvent); // Put it back in the queue
                    }
                }
            }
            finally
            {
                _queueLock.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing retry queue");
        }
        finally
        {
            _processingRetryQueue = false;
        }
    }

    /// <summary>
    /// Serializes event data to JSON.
    /// </summary>
    /// <param name="eventData">The event data to serialize</param>
    /// <returns>The serialized event data</returns>
    private string SerializeEventData(object eventData)
    {
        try
        {
            return JsonSerializer.Serialize(eventData, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize event data. Using ToString() instead.");
            return eventData.ToString();
        }
    }

    /// <summary>
    /// Disposes the adapter.
    /// </summary>
    public void Dispose()
    {
        _retryTimer?.Dispose();
        _queueLock?.Dispose();
    }
}

/// <summary>
/// Defines the contract for the audit logging adapter.
/// </summary>
public interface IAuditLoggingAdapter : IDisposable
{
    /// <summary>
    /// Logs an agent-related audit event.
    /// </summary>
    /// <param name="eventType">The type of event</param>
    /// <param name="eventData">The event data</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogAgentEventAsync(AgentAuditEventType eventType, object eventData, string? correlationId = null);

    /// <summary>
    /// Logs an agent registration event.
    /// </summary>
    /// <param name="agentId">The ID of the registered agent</param>
    /// <param name="agentType">The type of the registered agent</param>
    /// <param name="registeredBy">The ID of the user who registered the agent</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogAgentRegisteredAsync(Guid agentId, string agentType, string registeredBy, string? correlationId = null);

    /// <summary>
    /// Logs an agent retirement event.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="retiredBy">The ID of the user who retired the agent</param>
    /// <param name="reason">The reason for retirement</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogAgentRetiredAsync(Guid agentId, string retiredBy, string reason, string? correlationId = null);

    /// <summary>
    /// Logs an agent action execution event.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="actionType">The type of action executed</param>
    /// <param name="userId">The ID of the user who initiated the action</param>
    /// <param name="parameters">The parameters of the action</param>
    /// <param name="success">Whether the action was successful</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogAgentActionExecutedAsync(
        Guid agentId, 
        string actionType, 
        string userId, 
        Dictionary<string, object> parameters, 
        bool success,
        string? correlationId = null);

    /// <summary>
    /// Logs an authority override event.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="overriddenBy">The ID of the user who overrode the authority</param>
    /// <param name="reason">The reason for the override</param>
    /// <param name="overrideDetails">Details about the override</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogAuthorityOverriddenAsync(
        Guid agentId, 
        string overriddenBy, 
        string reason, 
        Dictionary<string, object> overrideDetails, 
        string? correlationId = null);

    /// <summary>
    /// Logs a consent request event.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="userId">The ID of the user from whom consent is requested</param>
    /// <param name="consentType">The type of consent requested</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogConsentRequestedAsync(
        Guid agentId, 
        string userId, 
        string consentType, 
        string? correlationId = null);

    /// <summary>
    /// Logs a consent decision event.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="userId">The ID of the user who made the decision</param>
    /// <param name="consentType">The type of consent</param>
    /// <param name="granted">Whether consent was granted</param>
    /// <param name="correlationId">Optional correlation ID for tracing related events</param>
    /// <returns>True if the event was successfully logged; otherwise, false</returns>
    Task<bool> LogConsentDecisionAsync(
        Guid agentId, 
        string userId, 
        string consentType, 
        bool granted, 
        string? correlationId = null);

    /// <summary>
    /// Searches for audit events based on criteria.
    /// </summary>
    /// <param name="criteria">The search criteria</param>
    /// <returns>A collection of matching audit events</returns>
    Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria);

    /// <summary>
    /// Gets an audit event by its ID.
    /// </summary>
    /// <param name="eventId">The ID of the event</param>
    /// <returns>The audit event if found; otherwise, null</returns>
    Task<AuditEvent> GetEventByIdAsync(string eventId);

    /// <summary>
    /// Gets all audit events with a specific correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID</param>
    /// <returns>A collection of audit events with the specified correlation ID</returns>
    Task<IEnumerable<AuditEvent>> GetEventsByCorrelationIdAsync(string correlationId);

    /// <summary>
    /// Gets all audit events for a specific agent.
    /// </summary>
    /// <param name="agentId">The ID of the agent</param>
    /// <param name="startTime">Optional start time for filtering</param>
    /// <param name="endTime">Optional end time for filtering</param>
    /// <returns>A collection of audit events for the specified agent</returns>
    Task<IEnumerable<AuditEvent>> GetAgentEventsAsync(Guid agentId, DateTimeOffset? startTime = null, DateTimeOffset? endTime = null);
}

/// <summary>
/// Repository interface for storing and retrieving audit events.
/// </summary>
public interface IAuditEventRepository
{
    /// <summary>
    /// Saves an audit event to the repository.
    /// </summary>
    /// <param name="auditEvent">The audit event to save</param>
    Task SaveEventAsync(AuditEvent auditEvent);

    /// <summary>
    /// Gets an audit event by its ID.
    /// </summary>
    /// <param name="eventId">The ID of the event</param>
    /// <returns>The audit event if found; otherwise, null</returns>
    Task<AuditEvent> GetEventByIdAsync(string eventId);

    /// <summary>
    /// Gets all audit events with a specific correlation ID.
    /// </summary>
    /// <param name="correlationId">The correlation ID</param>
    /// <returns>A collection of audit events with the specified correlation ID</returns>
    Task<IEnumerable<AuditEvent>> GetEventsByCorrelationIdAsync(string correlationId);

    /// <summary>
    /// Searches for audit events based on criteria.
    /// </summary>
    /// <param name="criteria">The search criteria</param>
    /// <returns>A collection of matching audit events</returns>
    Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria);
}

/// <summary>
/// Represents an audit event in the system.
/// </summary>
public class AuditEvent
{
    /// <summary>
    /// The unique identifier of the event.
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// The type of the event.
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// The category of the event.
    /// </summary>
    public string EventCategory { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// The correlation ID for tracing related events.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// The ID of the user who triggered the event.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The tenant ID associated with the event.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// The source of the event.
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// The serialized event data.
    /// </summary>
    public string? EventData { get; set; }

    /// <summary>
    /// Additional metadata associated with the event.
    /// </summary>
    public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}

/// <summary>
/// Criteria for searching audit events.
/// </summary>
public class AuditSearchCriteria
{
    /// <summary>
    /// The start time for the search range.
    /// </summary>
    public DateTimeOffset? StartTime { get; set; }

    /// <summary>
    /// The end time for the search range.
    /// </summary>
    public DateTimeOffset? EndTime { get; set; }

    /// <summary>
    /// The event types to include.
    /// </summary>
    public List<string> EventTypes { get; set; } = new List<string>();

    /// <summary>
    /// The event categories to include.
    /// </summary>
    public List<string> EventCategories { get; set; } = new List<string>();

    /// <summary>
    /// The user IDs to include.
    /// </summary>
    public List<string> UserIds { get; set; } = new List<string>();

    /// <summary>
    /// The agent IDs to include.
    /// </summary>
    public List<Guid> AgentIds { get; set; } = new List<Guid>();

    /// <summary>
    /// The tenant IDs to include.
    /// </summary>
    public List<string> TenantIds { get; set; } = new List<string>();

    /// <summary>
    /// The correlation IDs to include.
    /// </summary>
    public List<string> CorrelationIds { get; set; } = new List<string>();

    /// <summary>
    /// The maximum number of results to return.
    /// </summary>
    public int? Limit { get; set; }

    /// <summary>
    /// The number of results to skip.
    /// </summary>
    public int? Offset { get; set; }
}

/// <summary>
/// Types of agent audit events.
/// </summary>
public enum AgentAuditEventType
{
    /// <summary>
    /// An agent was registered.
    /// </summary>
    AgentRegistered,

    /// <summary>
    /// An agent was retired.
    /// </summary>
    AgentRetired,

    /// <summary>
    /// An agent was deprecated.
    /// </summary>
    AgentDeprecated,

    /// <summary>
    /// An agent was updated.
    /// </summary>
    AgentUpdated,

    /// <summary>
    /// An agent executed an action.
    /// </summary>
    AgentActionExecuted,

    /// <summary>
    /// An agent's authority was overridden.
    /// </summary>
    AuthorityOverridden,

    /// <summary>
    /// Consent was requested for an agent action.
    /// </summary>
    ConsentRequested,

    /// <summary>
    /// Consent was granted for an agent action.
    /// </summary>
    ConsentGranted,

    /// <summary>
    /// A value-generation diagnostic (the "$200 Test") was executed for a
    /// user or team.
    /// </summary>
    ValueDiagnosticRun,

    /// <summary>
    /// An organizational blindness analysis completed and detected
    /// significant blind spots.
    /// </summary>
    OrgBlindnessDetected,

    /// <summary>
    /// An employability-risk assessment flagged a user as medium or high
    /// risk.
    /// </summary>
    EmployabilityRiskFlagged,

    /// <summary>
    /// A manual adjudication case was created (human-in-the-loop review
    /// requested).
    /// </summary>
    ManualAdjudicationRequested,

    // -----------------------------------------------------------------
    //            Ethical & Legal Compliance Framework event types
    // -----------------------------------------------------------------

    /// <summary>
    /// A new governance/ethical/legal policy was approved.
    /// </summary>
    PolicyApproved,

    /// <summary>
    /// An existing policy was rolled back to a previous version.
    /// </summary>
    PolicyRolledBack,

    /// <summary>
    /// A governance violation was detected by the monitoring engine.
    /// </summary>
    GovernanceViolation,

    /// <summary>
    /// An ethical assessment was performed on an action or reasoning chain.
    /// </summary>
    EthicalAssessmentPerformed,

    /// <summary>
    /// A legal compliance check (e.g., GDPR, EU AI Act) was performed.
    /// </summary>
    LegalComplianceChecked,

    /// <summary>
    /// An informational dignity violation (Floridi) was detected.
    /// </summary>
    InformationalDignityViolation,

    /// <summary>
    /// A cross-cultural adaptation action was executed (Hofstede framework).
    /// </summary>
    CrossCulturalAdaptation
}

/// <summary>
/// Event data for agent registration events.
/// </summary>
public class AgentRegisteredEvent
{
    /// <summary>
    /// The ID of the registered agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The type of the registered agent.
    /// </summary>
    public string AgentType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who registered the agent.
    /// </summary>
    public string RegisteredBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of the registration.
    /// </summary>
    public DateTimeOffset RegisteredAt { get; set; }
}

/// <summary>
/// Event data for agent retirement events.
/// </summary>
public class AgentRetiredEvent
{
    /// <summary>
    /// The ID of the retired agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The ID of the user who retired the agent.
    /// </summary>
    public string RetiredBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of the retirement.
    /// </summary>
    public DateTimeOffset RetiredAt { get; set; }

    /// <summary>
    /// The reason for retirement.
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Event data for agent action execution events.
/// </summary>
public class AgentActionExecutedEvent
{
    /// <summary>
    /// The ID of the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The type of action executed.
    /// </summary>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the user who initiated the action.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The parameters of the action.
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// The timestamp of the execution.
    /// </summary>
    public DateTimeOffset ExecutedAt { get; set; }

    /// <summary>
    /// Whether the action was successful.
    /// </summary>
    public bool Success { get; set; }
}

/// <summary>
/// Event data for authority override events.
/// </summary>
public class AuthorityOverriddenEvent
{
    /// <summary>
    /// The ID of the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The ID of the user who overrode the authority.
    /// </summary>
    public string OverriddenBy { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of the override.
    /// </summary>
    public DateTimeOffset OverriddenAt { get; set; }

    /// <summary>
    /// The reason for the override.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Details about the override.
    /// </summary>
    public Dictionary<string, object> OverrideDetails { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Event data for consent request events.
/// </summary>
public class ConsentRequestedEvent
{
    /// <summary>
    /// The ID of the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The ID of the user from whom consent is requested.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The type of consent requested.
    /// </summary>
    public string ConsentType { get; set; } = string.Empty;

    /// <summary>
    /// The timestamp of the request.
    /// </summary>
    public DateTimeOffset RequestedAt { get; set; }
}

/// <summary>
/// Event data for consent decision events.
/// </summary>
public class ConsentDecisionEvent
{
    /// <summary>
    /// The ID of the agent.
    /// </summary>
    public Guid AgentId { get; set; }

    /// <summary>
    /// The ID of the user who made the decision.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// The type of consent.
    /// </summary>
    public string ConsentType { get; set; } = string.Empty;

    /// <summary>
    /// Whether consent was granted.
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// The timestamp of the decision.
    /// </summary>
    public DateTimeOffset DecisionAt { get; set; }
}

/// <summary>
/// Exception thrown when an audit search operation fails.
/// </summary>
public class AuditSearchException : Exception
{
    /// <summary>
    /// Initializes a new instance of the AuditSearchException class.
    /// </summary>
    /// <param name="message">The exception message</param>
    /// <param name="innerException">The inner exception</param>
    public AuditSearchException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
}