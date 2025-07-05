// Refactored KnowledgeGraphManager.cs
// - Full SOLID + DRY pass

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
    public class KnowledgeGraphManager : IKnowledgeGraphManager, IDisposable
    {
        private readonly IKnowledgeGraphAdapter _graphAdapter;
        private readonly ILogger<KnowledgeGraphManager>? _logger;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private bool _initialized;
        private bool _disposed;

        public KnowledgeGraphManager(
            IKnowledgeGraphAdapter graphAdapter,
            ILogger<KnowledgeGraphManager>? logger = null)
        {
            _graphAdapter = graphAdapter ?? throw new ArgumentNullException(nameof(graphAdapter));
            _logger = logger;
            LogInfo("KnowledgeGraphManager initialized with {AdapterType}", _graphAdapter.GetType().Name);
        }

        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            if (_initialized) return;
            await _initLock.WaitAsync(cancellationToken);
            try
            {
                if (_initialized) return;

                LogInfo("Initializing KnowledgeGraphManager...");
                if (_graphAdapter is IAsyncInitialization asyncInit)
                    await asyncInit.Initialization.ConfigureAwait(false);

                _initialized = true;
                LogInfo("KnowledgeGraphManager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Initialization failed: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to initialize KnowledgeGraphManager", ex);
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task AddNodeAsync<T>(string nodeId, T properties, string? label = null, CancellationToken cancellationToken = default) where T : class
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nodeId);
            ArgumentNullException.ThrowIfNull(properties);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Adding node: {NodeId}", nodeId);
                var triples = ConvertToTriples(nodeId, properties, label);
                await _graphAdapter.UpsertTriplesAsync(triples);
                LogInfo("Node added: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddNode failed for {NodeId}: {Message}", nodeId, ex.Message);
                throw;
            }
        }

        public async Task UpdateNodeAsync<T>(string nodeId, T properties, CancellationToken cancellationToken = default) where T : class
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nodeId);
            ArgumentNullException.ThrowIfNull(properties);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Updating node: {NodeId}", nodeId);
                await DeleteNodeAsync(nodeId, cancellationToken);
                await AddNodeAsync(nodeId, properties, null, cancellationToken);
                LogInfo("Node updated: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                LogError(ex, "UpdateNode failed for {NodeId}: {Message}", nodeId, ex.Message);
                throw;
            }
        }

        public async Task DeleteNodeAsync(string nodeId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nodeId);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Deleting node: {NodeId}", nodeId);
                var subjectTriples = await _graphAdapter.GetTriplesBySubjectAsync(nodeId);
                var objectTriples = await _graphAdapter.GetTriplesByObjectAsync(nodeId);
                var allTriples = subjectTriples.Union(objectTriples).ToList();
                if (allTriples.Any()) await _graphAdapter.DeleteTriplesAsync(allTriples);
                LogInfo("Node deleted: {NodeId}", nodeId);
            }
            catch (Exception ex)
            {
                LogError(ex, "DeleteNode failed for {NodeId}: {Message}", nodeId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<Dictionary<string, object>>> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(query);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Querying graph: {Query}", query);
                var result = await _graphAdapter.ExecuteQueryAsync(query);
                return ConvertQueryResult(result);
            }
            catch (Exception ex)
            {
                LogError(ex, "Query failed: {Query}: {Message}", query, ex.Message);
                throw;
            }
        }

        public async Task<T?> GetNodeAsync<T>(string nodeId, CancellationToken cancellationToken = default) where T : class
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(nodeId);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Fetching node: {NodeId}", nodeId);
                var triples = await _graphAdapter.GetTriplesBySubjectAsync(nodeId);
                return triples.Any() ? ConvertFromTriples<T>(nodeId, triples) : null;
            }
            catch (Exception ex)
            {
                LogError(ex, "GetNode failed: {NodeId}: {Message}", nodeId, ex.Message);
                throw;
            }
        }

        public async Task AddRelationshipAsync(string sourceNodeId, string targetNodeId, string relationshipType, Dictionary<string, object>? properties = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sourceNodeId);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(targetNodeId);
            ArgumentNullException.ThrowIfNullOrWhiteSpace(relationshipType);
            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Adding relationship: {Source} -[{Type}]-> {Target}", sourceNodeId, relationshipType, targetNodeId);
                var triple = new KnowledgeTriple
                {
                    Subject = sourceNodeId,
                    Predicate = relationshipType,
                    Object = targetNodeId,
                    Properties = properties ?? new Dictionary<string, object>()
                };
                await _graphAdapter.UpsertTriplesAsync(new[] { triple });
                LogInfo("Relationship added: {Source} -[{Type}]-> {Target}", sourceNodeId, relationshipType, targetNodeId);
            }
            catch (Exception ex)
            {
                LogError(ex, "AddRelationship failed: {Source}->{Target}: {Message}", sourceNodeId, targetNodeId, ex.Message);
                throw;
            }
        }

        public async Task<IEnumerable<T>> FindNodesAsync<T>(Dictionary<string, object> properties, CancellationToken cancellationToken = default) where T : class
        {
            if (properties == null || !properties.Any())
                throw new ArgumentException("Properties cannot be null or empty.", nameof(properties));

            await EnsureInitializedAsync(cancellationToken);

            try
            {
                LogDebug("Finding nodes with properties: {Props}", string.Join(", ", properties.Select(p => $"{p.Key}={p.Value}")));
                var query = BuildFindNodesQuery(properties);
                var results = await QueryAsync(query, cancellationToken);
                return results
                    .Where(r => r.TryGetValue("n", out var node) && node is Dictionary<string, object>)
                    .Select(r => ConvertToObject<T>((Dictionary<string, object>)r["n"]))
                    .Where(n => n != null)
                    .ToList();
            }
            catch (Exception ex)
            {
                LogError(ex, "FindNodes failed: {Message}", ex.Message);
                throw;
            }
        }

        public async Task BuildGraphAsync(string sourceId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sourceId);
            try
            {
                var triples = await ExtractTriples(sourceId);
                await _graphAdapter.UpsertTriplesAsync(triples);
            }
            catch (Exception ex)
            {
                LogError(ex, "BuildGraph failed: {SourceId}: {Message}", sourceId, ex.Message);
                throw;
            }
        }

        public async Task<object> QueryGraphAsync(string query)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(query);
            return await _graphAdapter.ExecuteQueryAsync(query);
        }

        private async Task<IEnumerable<KnowledgeTriple>> ExtractTriples(string sourceId)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(sourceId);
            return new List<KnowledgeTriple>
            {
                new()
                {
                    Subject = $"resource:{sourceId}",
                    Predicate = "rdf:type",
                    Object = "Document",
                    Properties =
                    {
                        ["extractionMethod"] = "placeholder",
                        ["extractionDate"] = DateTime.UtcNow
                    }
                }
            };
        }

        private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
        {
            if (!_initialized) await InitializeAsync(cancellationToken);
        }

        private void LogInfo(string msg, params object?[] args) => _logger?.LogInformation(0, msg, args);
        private void LogDebug(string msg, params object?[] args) => _logger?.LogDebug(0, msg, args);
        private void LogError(Exception ex, string msg, params object?[] args) => _logger?.LogError(0, ex, msg, args);

        public void Dispose()
        {
            if (_disposed) return;
            if (_graphAdapter is IDisposable d) d.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private T? ConvertFromTriples<T>(string nodeId, IEnumerable<KnowledgeTriple> triples) where T : class
        {
            var instance = Activator.CreateInstance<T>();
            var props = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLowerInvariant());
            foreach (var t in triples)
            {
                if (t.Predicate.StartsWith("prop:") && props.TryGetValue(t.Predicate[5..].ToLowerInvariant(), out var prop))
                {
                    try
                    {
                        var value = Convert.ChangeType(t.Object, prop.PropertyType);
                        prop.SetValue(instance, value);
                    }
                    catch (Exception ex)
                    {
                        LogWarning(ex, "Failed to map triple to property {Name}", prop.Name);
                    }
                }
            }
            return instance;
        }

        private IEnumerable<KnowledgeTriple> ConvertToTriples<T>(string nodeId, T obj, string? label)
        {
            var list = new List<KnowledgeTriple>();
            if (!string.IsNullOrWhiteSpace(label))
            {
                list.Add(new KnowledgeTriple { Subject = nodeId, Predicate = "rdf:type", Object = label, Timestamp = DateTimeOffset.UtcNow });
            }
            foreach (var prop in typeof(T).GetProperties())
            {
                var val = prop.GetValue(obj);
                if (val != null)
                {
                    list.Add(new KnowledgeTriple
                    {
                        Subject = nodeId,
                        Predicate = $"prop:{prop.Name}",
                        Object = val.ToString(),
                        Properties = { ["type"] = prop.PropertyType.Name, ["isCollection"] = false },
                        Timestamp = DateTimeOffset.UtcNow
                    });
                }
            }
            return list;
        }

        private IEnumerable<Dictionary<string, object>> ConvertQueryResult(object result)
        {
            return result is IEnumerable<Dictionary<string, object>> d ? d :
                new[] { new Dictionary<string, object> { ["result"] = result } };
        }

        private string BuildFindNodesQuery(Dictionary<string, object> props)
        {
            var conditions = new List<string>();
            var i = 0;
            foreach (var kv in props)
            {
                var param = $"p{i++}";
                conditions.Add($"n.`{kv.Key}` = ${param}");
            }
            return $"MATCH (n) WHERE {string.Join(" AND ", conditions)} RETURN n";
        }

        private T? ConvertToObject<T>(Dictionary<string, object> props) where T : class
        {
            try
            {
                var inst = Activator.CreateInstance<T>();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (props.TryGetValue(prop.Name, out var val))
                    {
                        var type = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        prop.SetValue(inst, Convert.ChangeType(val, type));
                    }
                }
                return inst;
            }
            catch (Exception ex)
            {
                LogError(ex, "ConvertToObject failed for type {Type}: {Message}", typeof(T).Name, ex.Message);
                return null;
            }
        }

        private void LogWarning(Exception ex, string msg, params object?[] args) => _logger?.LogWarning(0, ex, msg, args);
    }
}