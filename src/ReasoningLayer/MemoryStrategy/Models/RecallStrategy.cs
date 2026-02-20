namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Enumerates the available recall strategies for the Memory Strategy engine.
/// Each strategy uses a different matching approach to find relevant memories.
/// </summary>
public enum RecallStrategy
{
    /// <summary>
    /// Exact match on tags or content. Returns only records with identical content or matching tags.
    /// </summary>
    ExactMatch,

    /// <summary>
    /// Fuzzy matching using Levenshtein-like similarity on content strings.
    /// Tolerates minor differences such as typos or abbreviations.
    /// </summary>
    FuzzyMatch,

    /// <summary>
    /// Semantic similarity using cosine distance on embedding vectors.
    /// Requires that memory records have embeddings computed.
    /// </summary>
    SemanticSimilarity,

    /// <summary>
    /// Temporal proximity matching. Returns records closest in time to the query context.
    /// </summary>
    TemporalProximity,

    /// <summary>
    /// Hybrid strategy combining weighted results from all individual strategies
    /// to maximize recall quality.
    /// </summary>
    Hybrid
}
