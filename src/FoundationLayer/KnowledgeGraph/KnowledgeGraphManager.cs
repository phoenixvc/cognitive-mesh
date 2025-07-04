using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CognitiveMesh.FoundationLayer.KnowledgeGraph.Abstractions;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.FoundationLayer.KnowledgeGraph
{
    /// <summary>
    /// Manages creation and querying of the knowledge graph based on ingested data.
    /// Provides a high-level interface over the IKnowledgeGraphAdapter implementation.
    /// </summary>
    public class KnowledgeGraphManager : IKnowledgeGraphManager, IDisposable
    {
        private readonly IKnowledgeGraphAdapter _graphAdapter;
        private readonly ILogger<KnowledgeGraphManager> _logger;
        private bool _disposed = false;
        private bool _initialized = false;
        private readonly SemaphoreSlim _initLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeGraphManager"/> class.
        /// </summary>
        /// <param name="graphAdapter">The knowledge graph adapter to use.</param>
        /// <param name="logger">Optional logger instance.</param>
        /// <exception cref="ArgumentNullException">Thrown when graphAdapter is null.</exception>
        public KnowledgeGraphManager(
            IKnowledgeGraphAdapter graphAdapter,
            ILogger<KnowledgeGraphManager> logger = null)
        {
            _graphAdapter = graphAdapter ?? throw new ArgumentNullException(nameof(graphAdapter));
            _logger = logger;
            _logger?.LogInformation(0, "KnowledgeGraphManager initialized with {AdapterType}", graphAdapter.GetType().Name);
        }

        #region IKnowledgeGraphManager Implementation

        #region Helper Methods

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (!_initialized)
            {
                await InitializeAsync(cancellationToken);
            }
        }

        private IEnumerable<KnowledgeTriple> ConvertToTriples<T>(string nodeId, T obj, string label = null)
        {
            var triples = new List<KnowledgeTriple>();
            
            // Add type triple if label is provided
            if (!string.IsNullOrWhiteSpace(label))
            {
                triples.Add(new KnowledgeTriple
                {
                    Subject = nodeId,
                    Predicate = "rdf:type",
                    Object = label,
                    Timestamp = DateTimeOffset.UtcNow
                });
            }
            
            // Convert object properties to triples
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value != null)
                {
                    triples.Add(new KnowledgeTriple
                    {
                        Subject = nodeId,
                        Predicate = $"prop:{prop.Name}",
                        Object = value.ToString(),
                        Properties = 
                        {
                            ["type"] = prop.PropertyType.Name,
                            ["isCollection"] = false // TODO: Handle collections
                        },
                        Timestamp = DateTimeOffset.UtcNow
                    });
                }
            }
            
            return triples;
        }
        
        private T ConvertFromTriples<T>(string nodeId, IEnumerable<KnowledgeTriple> triples) where T : class
        {
            var instance = Activator.CreateInstance<T>();
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLowerInvariant());
            
            foreach (var triple in triples)
            {
                if (triple.Predicate.StartsWith("prop:"))
                {
                    var propName = triple.Predicate.Substring(5); // Remove 'prop:' prefix
                    if (properties.TryGetValue(propName.ToLowerInvariant(), out var prop))
                    {
                        try
                        {
                            // Simple conversion - in a real implementation, handle different types properly
                            var value = Convert.ChangeType(triple.Object, prop.PropertyType);
                            prop.SetValue(instance, value);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(0, ex, "Failed to convert property {PropName} with value {Value}", 
                                propName, triple.Object);
                        }
                    }
                }
            }
            
            return instance;
        }
        
        private IEnumerable<Dictionary<string, object>> ConvertQueryResult(object result)
        {
            // This is a simplified implementation
            // In a real implementation, this would properly convert the query result
            // based on the specific graph database being used
            
            if (result is IEnumerable<Dictionary<string, object>> dictResult)
                return dictResult;
                
            return new List<Dictionary<string, object>>
            {
                new Dictionary<string, object> { ["result"] = result }
            };
        }
        
        private string BuildFindNodesQuery(Dictionary<string, object> properties)
        {
            // This is a simplified implementation that would generate a query like:
            // MATCH (n) WHERE n.prop1 = value1 AND n.prop2 = value2 RETURN n
            
            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();
            
            int i = 0;
            foreach (var prop in properties)
            {
                var paramName = $"p{i++}";
                conditions.Add($"n.`{prop.Key}` = ${paramName}");
                parameters[paramName] = prop.Value;
            }
            
            return $"MATCH (n) WHERE {string.Join(" AND ", conditions)} RETURN n";
        }
        
        private T ConvertToObject<T>(Dictionary<string, object> properties) where T : class
        {
            try
            {
                var instance = Activator.CreateInstance<T>();
                var props = typeof(T).GetProperties();
                
                foreach (var prop in props)
                {
                    if (properties.TryGetValue(prop.Name, out var value) && value != null)
                    {
                        try
                        {
                            // Handle nullable types
                            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            var convertedValue = Convert.ChangeType(value, targetType);
                            prop.SetValue(instance, convertedValue);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(0, ex, "Failed to set property {PropName} with value {Value}", 
                                prop.Name, value);
                        }
                    }
                }
                
                return instance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(0, ex, "Failed to convert properties to object of type {TypeName}", typeof(T).Name);
                return null;
            }
        }

        #endregion

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized)
                return;

            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (_initialized) // Double-check lock
                    return;

                _logger?.LogInformation(0, "Initializing KnowledgeGraphManager...");
                
                // Initialize the underlying adapter if needed
                if (_graphAdapter is IAsyncInitialization asyncInit)
                {
                    await asyncInit.Initialization.ConfigureAwait(false);
                }

                _initialized = true;
                _logger?.LogInformation(0, "KnowledgeGraphManager initialized successfully");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize KnowledgeGraphManager: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to initialize KnowledgeGraphManager", ex);
            }
            finally
            {
                _initLock.Release();
            }
        }

        /// <inheritdoc/>
        public async Task AddNodeAsync<T>(string nodeId, T properties, string label = null, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID cannot be null or whitespace.", nameof(nodeId));

            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Adding node with ID: {NodeId}", nodeId);
                
                // Convert the node to triples
                var triples = ConvertToTriples(nodeId, properties, label);
                await _graphAdapter.UpsertTriplesAsync(triples);
                
                _logger?.LogInformation(0, "Successfully added node with ID: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to add node with ID: {NodeId}: {Message}", nodeId, ex.Message);
                throw new InvalidOperationException($"Failed to add node with ID: {nodeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task AddRelationshipAsync(
            string sourceNodeId, 
            string targetNodeId, 
            string relationshipType, 
            Dictionary<string, object> properties = null, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(sourceNodeId))
                throw new ArgumentException("Source node ID cannot be null or whitespace.", nameof(sourceNodeId));

            if (string.IsNullOrWhiteSpace(targetNodeId))
                throw new ArgumentException("Target node ID cannot be null or whitespace.", nameof(targetNodeId));

            if (string.IsNullOrWhiteSpace(relationshipType))
                throw new ArgumentException("Relationship type cannot be null or whitespace.", nameof(relationshipType));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Adding relationship of type {RelationshipType} from {SourceNodeId} to {TargetNodeId}", 
                    relationshipType, sourceNodeId, targetNodeId);

                var triple = new KnowledgeTriple
                {
                    Subject = sourceNodeId,
                    Predicate = relationshipType,
                    Object = targetNodeId,
                    Properties = properties ?? new Dictionary<string, object>()
                };

                await _graphAdapter.UpsertTriplesAsync(new[] { triple });
                _logger?.LogInformation(0, "Successfully added relationship of type {RelationshipType} from {SourceNodeId} to {TargetNodeId}", 
                    relationshipType, sourceNodeId, targetNodeId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to add relationship of type {RelationshipType} from {SourceNodeId} to {TargetNodeId}: {Message}", 
                    relationshipType, sourceNodeId, targetNodeId, ex.Message);
                throw new InvalidOperationException(
                    $"Failed to add relationship of type {relationshipType} from {sourceNodeId} to {targetNodeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Dictionary<string, object>>> QueryAsync(
            string query, 
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or whitespace.", nameof(query));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Executing query: {Query}", query);
                var result = await _graphAdapter.ExecuteQueryAsync(query);
                _logger?.LogDebug(0, "Successfully executed query: {Query}", query);
                
                // Convert the result to the expected format
                return ConvertQueryResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to execute query: {Query}: {Message}", query, ex.Message);
                throw new InvalidOperationException($"Failed to execute query: {query}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<T> GetNodeAsync<T>(string nodeId, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID cannot be null or whitespace.", nameof(nodeId));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Retrieving node with ID: {NodeId}", nodeId);
                
                // Query for all triples where the node is the subject
                var triples = await _graphAdapter.GetTriplesBySubjectAsync(nodeId);
                
                if (!triples.Any())
                {
                    _logger?.LogWarning(0, "Node with ID {NodeId} not found", nodeId);
                    return null;
                }

                // Convert triples back to object
                var result = ConvertFromTriples<T>(nodeId, triples);
                _logger?.LogDebug(0, "Successfully retrieved node with ID: {NodeId}", nodeId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to retrieve node with ID: {NodeId}", nodeId);
                throw new InvalidOperationException($"Failed to retrieve node with ID: {nodeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task UpdateNodeAsync<T>(string nodeId, T properties, CancellationToken cancellationToken = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID cannot be null or whitespace.", nameof(nodeId));

            if (properties == null)
                throw new ArgumentNullException(nameof(properties));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Updating node with ID: {NodeId}", nodeId);
                
                // First, delete existing properties for this node
                await DeleteNodeAsync(nodeId, cancellationToken);
                
                // Then add the updated properties
                await AddNodeAsync(nodeId, properties, null, cancellationToken);
                
                _logger?.LogInformation(0, "Successfully updated node with ID: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to update node with ID: {NodeId}", nodeId);
                throw new InvalidOperationException($"Failed to update node with ID: {nodeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteNodeAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(nodeId))
                throw new ArgumentException("Node ID cannot be null or whitespace.", nameof(nodeId));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Deleting node with ID: {NodeId}", nodeId);
                
                // Get all triples where this node is the subject or object
                var subjectTriples = await _graphAdapter.GetTriplesBySubjectAsync(nodeId);
                var objectTriples = await _graphAdapter.GetTriplesByObjectAsync(nodeId);
                
                // Combine and remove duplicates
                var allTriples = subjectTriples.Union(objectTriples).ToList();
                
                if (allTriples.Any())
                {
                    await _graphAdapter.DeleteTriplesAsync(allTriples);
                }
                
                _logger?.LogInformation(0, "Successfully deleted node with ID: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to delete node with ID: {NodeId}: {Message}", nodeId, ex.Message);
                throw new InvalidOperationException($"Failed to delete node with ID: {nodeId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<T>> FindNodesAsync<T>(
            Dictionary<string, object> properties, 
            CancellationToken cancellationToken = default) where T : class
        {
            if (properties == null || !properties.Any())
                throw new ArgumentException("Properties cannot be null or empty.", nameof(properties));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                _logger?.LogDebug(0, "Finding nodes with properties: {Properties}", 
                    string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")));
                
                // This is a simplified implementation that gets all nodes and filters in memory
                // In a real implementation, this would be optimized with proper indexing
                var query = BuildFindNodesQuery(properties);
                var results = await QueryAsync(query, cancellationToken);
                
                var nodes = new List<T>();
                foreach (var result in results)
                {
                    if (result.TryGetValue("n", out var nodeData) && nodeData is Dictionary<string, object> nodeProps)
                    {
                        var node = ConvertToObject<T>(nodeProps);
                        if (node != null)
                        {
                            nodes.Add(node);
                        }
                    }
                }
                
                _logger?.LogDebug(0, "Found {Count} nodes matching the criteria", nodes.Count);
                return nodes;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to find nodes with properties: {Properties}: {Message}", 
                    string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")), ex.Message);
                throw new InvalidOperationException("Failed to find nodes with the specified properties", ex);
            }
        }

        /// <summary>
        /// Builds or updates the knowledge graph from the specified source.
        /// </summary>
        /// <param name="sourceId">The ID of the source to build the graph from.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ArgumentException">Thrown when sourceId is null or whitespace.</exception>
        public async Task BuildGraphAsync(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("Source ID cannot be null or whitespace.", nameof(sourceId));

            try
            {
                var triples = await ExtractTriples(sourceId);
                await _graphAdapter.UpsertTriplesAsync(triples);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to build graph for source {sourceId}", ex);
            }
        }

        /// <summary>
        /// Executes a query against the knowledge graph.
        /// </summary>
        /// <param name="query">The query to execute.</param>
        /// <returns>The query results.</returns>
        /// <exception cref="ArgumentException">Thrown when query is null or whitespace.</exception>
        public async Task<object> QueryGraphAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException("Query cannot be null or whitespace.", nameof(query));

            return await _graphAdapter.ExecuteQueryAsync(query);
        }

        /// <summary>
        /// Extracts triples from a source document or data.
        /// </summary>
        /// <param name="sourceId">The ID of the source to extract triples from.</param>
        /// <returns>A collection of extracted triples.</returns>
        /// <exception cref="ArgumentException">Thrown when sourceId is null or whitespace.</exception>
        /// <summary>
        /// Extracts triples from a source document or data.
        /// </summary>
        /// <param name="sourceId">The ID of the source to extract triples from.</param>
        /// <returns>A collection of extracted triples.</returns>
        /// <exception cref="ArgumentException">Thrown when sourceId is null or whitespace.</exception>
        private Task<IEnumerable<KnowledgeTriple>> ExtractTriples(string sourceId)
        {
            if (string.IsNullOrWhiteSpace(sourceId))
                throw new ArgumentException("Source ID cannot be null or whitespace.", nameof(sourceId));

            // This is a placeholder implementation
            // In a real implementation, this would use NLP to extract entities and relationships
            // from the source document identified by sourceId
            IEnumerable<KnowledgeTriple> triples = new List<KnowledgeTriple>
            {
                new KnowledgeTriple
                {
                    Subject = $"resource:{sourceId}",
                    Predicate = "rdf:type",
                    Object = "Document",
                    Properties = 
                    {
                        { "extractionMethod", "placeholder" },
                        { "extractionDate", DateTime.UtcNow }
                    }
                }
            };
            
            return Task.FromResult(triples);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_graphAdapter is IDisposable disposableAdapter)
                        disposableAdapter.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// Represents a triple in the knowledge graph (subject-predicate-object).
    /// </summary>
    #endregion // Helper Methods
}
