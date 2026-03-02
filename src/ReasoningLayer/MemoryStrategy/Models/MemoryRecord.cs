using System;
using System.Collections.Generic;

namespace CognitiveMesh.ReasoningLayer.MemoryStrategy.Models;

/// <summary>
/// Represents a single record in the episodic memory system.
/// Memory records can be promoted from short-term to long-term (consolidated)
/// based on access frequency and importance scoring.
/// </summary>
public sealed class MemoryRecord
{
    /// <summary>
    /// Unique identifier for this memory record.
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// The textual content of this memory record.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// The embedding vector for semantic similarity searches.
    /// Null if no embedding has been computed.
    /// </summary>
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Tags associated with this memory record for categorical filtering.
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Importance score from 0.0 (trivial) to 1.0 (critical), influencing
    /// consolidation and pruning decisions.
    /// </summary>
    public double Importance { get; set; }

    /// <summary>
    /// Timestamp when this memory record was first created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Timestamp when this memory record was last accessed via recall.
    /// </summary>
    public DateTimeOffset LastAccessedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Number of times this memory record has been accessed via recall.
    /// </summary>
    public int AccessCount { get; set; }

    /// <summary>
    /// Whether this record has been promoted from short-term to long-term memory.
    /// </summary>
    public bool Consolidated { get; set; }
}
