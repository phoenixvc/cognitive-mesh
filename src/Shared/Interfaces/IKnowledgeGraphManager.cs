using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CognitiveMesh.Shared.Interfaces
{
    /// <summary>
    /// Defines the interface for knowledge graph operations
    /// </summary>
    public interface IKnowledgeGraphManager : IDisposable
    {
        /// <summary>
        /// Initializes the knowledge graph
        /// </summary>
        Task InitializeAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a node to the knowledge graph
        /// </summary>
        Task AddNodeAsync<T>(string nodeId, T properties, string label = null, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Adds a relationship between two nodes
        /// </summary>
        Task AddRelationshipAsync(string sourceNodeId, string targetNodeId, string relationshipType, 
            Dictionary<string, object> properties = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Queries the knowledge graph
        /// </summary>
        Task<IEnumerable<Dictionary<string, object>>> QueryAsync(string query, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a node by its ID
        /// </summary>
        Task<T> GetNodeAsync<T>(string nodeId, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Updates a node's properties
        /// </summary>
        Task UpdateNodeAsync<T>(string nodeId, T properties, CancellationToken cancellationToken = default) where T : class;

        /// <summary>
        /// Deletes a node and its relationships
        /// </summary>
        Task DeleteNodeAsync(string nodeId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds nodes by their properties
        /// </summary>
        Task<IEnumerable<T>> FindNodesAsync<T>(Dictionary<string, object> properties, 
            CancellationToken cancellationToken = default) where T : class;
    }
}
