using System.Text.Json;

namespace FoundationLayer.AuditLogging.Services
{
    /// <summary>
    /// Concrete implementation of the IAuditEventRepository interface using Entity Framework Core.
    /// This repository handles the persistence and retrieval of audit events, ensuring data integrity
    /// and providing resilient operations through a circuit breaker pattern.
    /// </summary>
    public class AuditEventRepository : IAuditEventRepository
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<AuditEventRepository> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the AuditEventRepository class.
        /// </summary>
        /// <param name="cosmosOptions">Configuration options for Cosmos DB</param>
        /// <param name="logger">Logger instance</param>
        public AuditEventRepository(
            IOptions<CosmosDbOptions> cosmosOptions,
            ILogger<AuditEventRepository> logger)
        {
            if (cosmosOptions == null || cosmosOptions.Value == null)
            {
                throw new ArgumentNullException(nameof(cosmosOptions));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var options = cosmosOptions.Value;
            _cosmosClient = new CosmosClient(options.ConnectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });

            _container = _cosmosClient.GetContainer(options.DatabaseName, options.AuditContainerName);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };

            _logger.LogInformation("AuditEventRepository initialized with database {DatabaseName} and container {ContainerName}",
                options.DatabaseName, options.AuditContainerName);
        }

        /// <inheritdoc />
        public async Task SaveEventAsync(AuditEvent auditEvent)
        {
            if (auditEvent == null)
            {
                throw new ArgumentNullException(nameof(auditEvent));
            }

            try
            {
                // Add partition key for efficient querying
                var partitionKey = DeterminePartitionKey(auditEvent);
                
                // Create a document with the partition key included
                var document = new
                {
                    id = auditEvent.EventId,
                    partitionKey,
                    eventType = auditEvent.EventType,
                    eventCategory = auditEvent.EventCategory,
                    timestamp = auditEvent.Timestamp,
                    correlationId = auditEvent.CorrelationId,
                    eventData = auditEvent.EventData,
                    // Add TTL if configured (for data retention policies)
                    ttl = GetTimeToLiveForEventType(auditEvent.EventType)
                };

                // Save to Cosmos DB
                await _container.CreateItemAsync(document, new PartitionKey(partitionKey));
                _logger.LogDebug("Successfully saved audit event: {EventType}, ID: {EventId}", 
                    auditEvent.EventType, auditEvent.EventId);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                // Document with this ID already exists
                _logger.LogWarning("Audit event with ID {EventId} already exists. Skipping save operation.", 
                    auditEvent.EventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save audit event: {EventType}, ID: {EventId}", 
                    auditEvent.EventType, auditEvent.EventId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<AuditEvent> GetEventByIdAsync(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                throw new ArgumentException("Event ID cannot be null or empty.", nameof(eventId));
            }

            try
            {
                // We need to query across all partition keys since we don't know the partition key
                var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @eventId")
                    .WithParameter("@eventId", eventId);

                var results = await ExecuteQueryAsync(query);
                return results.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get audit event with ID: {EventId}", eventId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditEvent>> GetEventsByCorrelationIdAsync(string correlationId)
        {
            if (string.IsNullOrWhiteSpace(correlationId))
            {
                throw new ArgumentException("Correlation ID cannot be null or empty.", nameof(correlationId));
            }

            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.correlationId = @correlationId ORDER BY c.timestamp DESC")
                    .WithParameter("@correlationId", correlationId);

                return await ExecuteQueryAsync(query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get audit events with correlation ID: {CorrelationId}", correlationId);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<AuditEvent>> SearchEventsAsync(AuditSearchCriteria criteria)
        {
            if (criteria == null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            try
            {
                var queryBuilder = new System.Text.StringBuilder("SELECT * FROM c WHERE 1=1");
                var queryDefinition = new QueryDefinition(queryBuilder.ToString());

                // Add time range filters
                if (criteria.StartTime.HasValue)
                {
                    queryBuilder.Append(" AND c.timestamp >= @startTime");
                    queryDefinition = queryDefinition.WithParameter("@startTime", criteria.StartTime.Value.ToString("o"));
                }

                if (criteria.EndTime.HasValue)
                {
                    queryBuilder.Append(" AND c.timestamp <= @endTime");
                    queryDefinition = queryDefinition.WithParameter("@endTime", criteria.EndTime.Value.ToString("o"));
                }

                // Add event type filters
                if (criteria.EventTypes != null && criteria.EventTypes.Count > 0)
                {
                    queryBuilder.Append(" AND c.eventType IN (");
                    for (int i = 0; i < criteria.EventTypes.Count; i++)
                    {
                        var paramName = $"@eventType{i}";
                        queryBuilder.Append(i > 0 ? ", " : "").Append(paramName);
                        queryDefinition = queryDefinition.WithParameter(paramName, criteria.EventTypes[i]);
                    }
                    queryBuilder.Append(")");
                }

                // Add event category filters
                if (criteria.EventCategories != null && criteria.EventCategories.Count > 0)
                {
                    queryBuilder.Append(" AND c.eventCategory IN (");
                    for (int i = 0; i < criteria.EventCategories.Count; i++)
                    {
                        var paramName = $"@eventCategory{i}";
                        queryBuilder.Append(i > 0 ? ", " : "").Append(paramName);
                        queryDefinition = queryDefinition.WithParameter(paramName, criteria.EventCategories[i]);
                    }
                    queryBuilder.Append(")");
                }

                // Add free text search
                if (!string.IsNullOrWhiteSpace(criteria.SearchText))
                {
                    queryBuilder.Append(" AND (CONTAINS(c.eventData, @searchText, true) OR CONTAINS(c.eventType, @searchText, true))");
                    queryDefinition = queryDefinition.WithParameter("@searchText", criteria.SearchText);
                }

                // Add event data contains filters
                if (criteria.EventDataContains != null && criteria.EventDataContains.Count > 0)
                {
                    foreach (var kvp in criteria.EventDataContains)
                    {
                        // Note: This is a simplified approach. In a real system, you might need
                        // a more sophisticated way to search within JSON data
                        var paramName = $"@{kvp.Key}";
                        queryBuilder.Append($" AND CONTAINS(c.eventData, {paramName}, true)");
                        queryDefinition = queryDefinition.WithParameter(paramName, JsonSerializer.Serialize(kvp.Value, _jsonOptions));
                    }
                }

                // Add sorting, pagination
                queryBuilder.Append(" ORDER BY c.timestamp DESC OFFSET @skip LIMIT @limit");
                queryDefinition = queryDefinition
                    .WithParameter("@skip", criteria.Skip)
                    .WithParameter("@limit", criteria.MaxResults);

                // Execute the final query
                queryDefinition = new QueryDefinition(queryBuilder.ToString());
                return await ExecuteQueryAsync(queryDefinition);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to search audit events with criteria: {Criteria}", 
                    JsonSerializer.Serialize(criteria, _jsonOptions));
                throw;
            }
        }

        /// <summary>
        /// Executes a query against the Cosmos DB container and returns the results as AuditEvent objects.
        /// </summary>
        /// <param name="queryDefinition">The query to execute</param>
        /// <returns>A collection of audit events matching the query</returns>
        private async Task<IEnumerable<AuditEvent>> ExecuteQueryAsync(QueryDefinition queryDefinition)
        {
            var results = new List<AuditEvent>();
            var queryOptions = new QueryRequestOptions { MaxItemCount = 100 };

            var iterator = _container.GetItemQueryIterator<dynamic>(
                queryDefinition,
                requestOptions: queryOptions);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
                    results.Add(new AuditEvent
                    {
                        EventId = item.id,
                        EventType = item.eventType,
                        EventCategory = item.eventCategory,
                        Timestamp = item.timestamp,
                        CorrelationId = item.correlationId,
                        EventData = item.eventData
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// Determines the appropriate partition key for an audit event.
        /// </summary>
        /// <param name="auditEvent">The audit event</param>
        /// <returns>The partition key value</returns>
        private string DeterminePartitionKey(AuditEvent auditEvent)
        {
            // Use event type as partition key for efficient querying by event type
            // For ethical/legal events, we might want to use a special partition strategy
            if (IsEthicalOrLegalEvent(auditEvent.EventType))
            {
                return $"ethical_{auditEvent.EventType}";
            }
            
            return auditEvent.EventType;
        }

        /// <summary>
        /// Determines if an event is an ethical or legal compliance event.
        /// </summary>
        /// <param name="eventType">The event type string</param>
        /// <returns>True if the event is an ethical or legal compliance event; otherwise, false</returns>
        private bool IsEthicalOrLegalEvent(string eventType)
        {
            // Check if the event type is one of the ethical/legal compliance event types
            return eventType == AgentAuditEventType.PolicyApproved.ToString() ||
                   eventType == AgentAuditEventType.PolicyRolledBack.ToString() ||
                   eventType == AgentAuditEventType.GovernanceViolation.ToString() ||
                   eventType == AgentAuditEventType.EthicalAssessmentPerformed.ToString() ||
                   eventType == AgentAuditEventType.LegalComplianceChecked.ToString() ||
                   eventType == AgentAuditEventType.InformationalDignityViolation.ToString() ||
                   eventType == AgentAuditEventType.CrossCulturalAdaptation.ToString();
        }

        /// <summary>
        /// Gets the time-to-live (TTL) value for an event type.
        /// </summary>
        /// <param name="eventType">The event type</param>
        /// <returns>The TTL value in seconds, or null for no expiration</returns>
        private int? GetTimeToLiveForEventType(string eventType)
        {
            // Ethical and legal events might need longer retention periods for compliance
            if (IsEthicalOrLegalEvent(eventType))
            {
                // 7 years retention for compliance events (in seconds)
                return 7 * 365 * 24 * 60 * 60;
            }

            // Default retention period (1 year in seconds)
            return 365 * 24 * 60 * 60;
        }
    }

    /// <summary>
    /// Configuration options for Cosmos DB.
    /// </summary>
    public class CosmosDbOptions
    {
        /// <summary>
        /// The Cosmos DB connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The name of the Cosmos DB database.
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// The name of the container for audit events.
        /// </summary>
        public string AuditContainerName { get; set; }
    }
}
