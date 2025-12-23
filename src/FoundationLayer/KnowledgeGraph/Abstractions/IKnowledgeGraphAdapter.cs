namespace FoundationLayer.KnowledgeGraph.Abstractions
{
    /// <summary>
    /// Defines the contract for knowledge graph operations.
    /// </summary>
    public interface IKnowledgeGraphAdapter : IDisposable
    {
        /// <summary>
        /// Executes a query against the knowledge graph.
        /// </summary>
        /// <param name="query">The query string in the graph query language (e.g., Cypher, SPARQL).</param>
        /// <returns>The query results.</returns>
        Task<object> ExecuteQueryAsync(string query);
        
        /// <summary>
        /// Upserts triples into the knowledge graph.
        /// </summary>
        /// <param name="triples">The triples to upsert.</param>
        Task UpsertTriplesAsync(IEnumerable<KnowledgeTriple> triples);
        
        /// <summary>
        /// Deletes triples from the knowledge graph.
        /// </summary>
        /// <param name="triples">The triples to delete.</param>
        Task DeleteTriplesAsync(IEnumerable<KnowledgeTriple> triples);
        
        /// <summary>
        /// Gets all triples where the given subject is the subject of the triple.
        /// </summary>
        /// <param name="subject">The subject to match.</param>
        /// <returns>Matching triples.</returns>
        Task<IEnumerable<KnowledgeTriple>> GetTriplesBySubjectAsync(string subject);
        
        /// <summary>
        /// Gets all triples where the given predicate is the predicate of the triple.
        /// </summary>
        /// <param name="predicate">The predicate to match.</param>
        /// <returns>Matching triples.</returns>
        Task<IEnumerable<KnowledgeTriple>> GetTriplesByPredicateAsync(string predicate);
        
        /// <summary>
        /// Gets all triples where the given object is the object of the triple.
        /// </summary>
        /// <param name="object">The object to match.</param>
        /// <returns>Matching triples.</returns>
        Task<IEnumerable<KnowledgeTriple>> GetTriplesByObjectAsync(string @object);
        
        /// <summary>
        /// Checks if a triple exists in the knowledge graph.
        /// </summary>
        /// <param name="triple">The triple to check.</param>
        /// <returns>True if the triple exists, false otherwise.</returns>
        Task<bool> TripleExistsAsync(KnowledgeTriple triple);
        
        /// <summary>
        /// Gets the count of triples in the knowledge graph.
        /// </summary>
        /// <returns>The number of triples.</returns>
        Task<long> GetTripleCountAsync();
        
        /// <summary>
        /// Clears all data from the knowledge graph.
        /// </summary>
        Task ClearAllAsync();
    }

    /// <summary>
    /// Represents a triple in the knowledge graph (subject-predicate-object).
    /// </summary>
    public class KnowledgeTriple
    {
        /// <summary>
        /// Gets or sets the subject of the triple.
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// Gets or sets the predicate of the triple.
        /// </summary>
        public string Predicate { get; set; }
        
        /// <summary>
        /// Gets or sets the object of the triple.
        /// </summary>
        public string Object { get; set; }
        
        /// <summary>
        /// Gets or sets additional properties for the triple.
        /// </summary>
        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets or sets the timestamp when the triple was created or last modified.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
