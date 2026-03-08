namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Types of nodes in a thought graph.
/// </summary>
public enum ThoughtNodeType
{
    /// <summary>A question or problem statement.</summary>
    Question,
    /// <summary>A hypothesis or proposed answer.</summary>
    Hypothesis,
    /// <summary>Supporting evidence or reasoning.</summary>
    Evidence,
    /// <summary>A conclusion derived from reasoning.</summary>
    Conclusion,
    /// <summary>A counterargument or objection.</summary>
    Counterargument,
    /// <summary>A refinement of a previous thought.</summary>
    Refinement,
    /// <summary>An action to take.</summary>
    Action,
    /// <summary>A result from an action.</summary>
    Result
}

/// <summary>
/// Types of edges in a thought graph.
/// </summary>
public enum ThoughtEdgeType
{
    /// <summary>Node supports the target.</summary>
    Supports,
    /// <summary>Node contradicts the target.</summary>
    Contradicts,
    /// <summary>Node refines the target.</summary>
    Refines,
    /// <summary>Node derives from the target.</summary>
    DerivesFrom,
    /// <summary>Node leads to the target action.</summary>
    LeadsTo,
    /// <summary>Node is a result of the target.</summary>
    ResultOf
}

/// <summary>
/// A node in the thought graph.
/// </summary>
public class GraphThoughtNode
{
    /// <summary>Unique identifier.</summary>
    public string NodeId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Type of node.</summary>
    public required ThoughtNodeType Type { get; init; }

    /// <summary>The thought content.</summary>
    public required string Content { get; init; }

    /// <summary>Confidence in this thought (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Source of this thought (LLM, Knowledge Graph, Tool, etc.).</summary>
    public string? Source { get; init; }

    /// <summary>Embedding for similarity comparisons.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>When the node was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// An edge in the thought graph.
/// </summary>
public class GraphThoughtEdge
{
    /// <summary>Unique identifier.</summary>
    public string EdgeId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Source node ID.</summary>
    public required string SourceId { get; init; }

    /// <summary>Target node ID.</summary>
    public required string TargetId { get; init; }

    /// <summary>Type of relationship.</summary>
    public required ThoughtEdgeType Type { get; init; }

    /// <summary>Strength of the relationship (0.0 - 1.0).</summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>Label for the edge.</summary>
    public string? Label { get; init; }
}

/// <summary>
/// A complete thought graph.
/// </summary>
public class ThoughtGraph
{
    /// <summary>Unique identifier.</summary>
    public string GraphId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The root question or problem.</summary>
    public required string RootQuestion { get; init; }

    /// <summary>Root node ID.</summary>
    public required string RootNodeId { get; init; }

    /// <summary>All nodes in the graph.</summary>
    public IReadOnlyDictionary<string, GraphThoughtNode> Nodes { get; init; } = new Dictionary<string, GraphThoughtNode>();

    /// <summary>All edges in the graph.</summary>
    public IReadOnlyList<GraphThoughtEdge> Edges { get; init; } = Array.Empty<GraphThoughtEdge>();

    /// <summary>Current best conclusion node IDs.</summary>
    public IReadOnlyList<string> ConclusionNodeIds { get; init; } = Array.Empty<string>();

    /// <summary>When the graph was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the graph was last updated.</summary>
    public DateTimeOffset LastUpdatedAt { get; init; }
}

/// <summary>
/// Request to add thoughts to the graph.
/// </summary>
public class AddThoughtsRequest
{
    /// <summary>The graph ID.</summary>
    public required string GraphId { get; init; }

    /// <summary>Parent node ID to connect to.</summary>
    public required string ParentNodeId { get; init; }

    /// <summary>The query or context for generating thoughts.</summary>
    public required string Query { get; init; }

    /// <summary>Maximum thoughts to generate.</summary>
    public int MaxThoughts { get; init; } = 3;

    /// <summary>Types of thoughts to generate.</summary>
    public IReadOnlyList<ThoughtNodeType> ThoughtTypes { get; init; } = Array.Empty<ThoughtNodeType>();
}

/// <summary>
/// Result of graph reasoning.
/// </summary>
public class GraphReasoningResult
{
    /// <summary>The graph used.</summary>
    public required ThoughtGraph Graph { get; init; }

    /// <summary>Final conclusions reached.</summary>
    public IReadOnlyList<GraphThoughtNode> Conclusions { get; init; } = Array.Empty<GraphThoughtNode>();

    /// <summary>Best reasoning path (node IDs).</summary>
    public IReadOnlyList<string> BestPath { get; init; } = Array.Empty<string>();

    /// <summary>Confidence in the conclusions (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Summary of the reasoning.</summary>
    public string? Summary { get; init; }

    /// <summary>Alternative conclusions.</summary>
    public IReadOnlyList<GraphThoughtNode> Alternatives { get; init; } = Array.Empty<GraphThoughtNode>();

    /// <summary>Statistics about the reasoning.</summary>
    public GraphReasoningStatistics Statistics { get; init; } = new();
}

/// <summary>
/// Statistics about graph reasoning.
/// </summary>
public class GraphReasoningStatistics
{
    public int TotalNodes { get; init; }
    public int TotalEdges { get; init; }
    public int MaxDepth { get; init; }
    public double AverageBranchingFactor { get; init; }
    public int LLMCalls { get; init; }
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for Graph of Thoughts reasoning.
/// Implements the "Graph of Thoughts (GoT)" pattern.
/// </summary>
/// <remarks>
/// This port enables graph-based reasoning where thoughts are nodes
/// that can have multiple connections (support, contradict, refine),
/// allowing for richer reasoning than linear chains or trees.
/// </remarks>
public interface IGraphOfThoughtsPort
{
    /// <summary>
    /// Creates a new thought graph.
    /// </summary>
    /// <param name="question">The root question or problem.</param>
    /// <param name="initialThoughts">Optional initial thoughts to seed.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new thought graph.</returns>
    Task<ThoughtGraph> CreateGraphAsync(
        string question,
        IEnumerable<string>? initialThoughts = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds thoughts to an existing graph.
    /// </summary>
    /// <param name="request">The add request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new nodes added.</returns>
    Task<IReadOnlyList<GraphThoughtNode>> AddThoughtsAsync(
        AddThoughtsRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects two nodes with an edge.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="sourceId">Source node ID.</param>
    /// <param name="targetId">Target node ID.</param>
    /// <param name="edgeType">Type of relationship.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new edge.</returns>
    Task<GraphThoughtEdge> ConnectNodesAsync(
        string graphId,
        string sourceId,
        string targetId,
        ThoughtEdgeType edgeType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregates thoughts to form conclusions.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="maxConclusions">Maximum conclusions to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The conclusions.</returns>
    Task<IReadOnlyList<GraphThoughtNode>> AggregateThoughtsAsync(
        string graphId,
        int maxConclusions = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs complete reasoning on a question.
    /// </summary>
    /// <param name="question">The question to reason about.</param>
    /// <param name="maxIterations">Maximum reasoning iterations.</param>
    /// <param name="maxNodes">Maximum nodes in the graph.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reasoning result.</returns>
    Task<GraphReasoningResult> ReasonAsync(
        string question,
        int maxIterations = 5,
        int maxNodes = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a thought graph.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The graph.</returns>
    Task<ThoughtGraph?> GetGraphAsync(
        string graphId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds related nodes in the graph.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="query">Query to match.</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Related nodes.</returns>
    Task<IReadOnlyList<GraphThoughtNode>> FindRelatedNodesAsync(
        string graphId,
        string query,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds paths between two nodes.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="startNodeId">Start node.</param>
    /// <param name="endNodeId">End node.</param>
    /// <param name="maxPaths">Maximum paths to find.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paths as lists of node IDs.</returns>
    Task<IReadOnlyList<IReadOnlyList<string>>> FindPathsAsync(
        string graphId,
        string startNodeId,
        string endNodeId,
        int maxPaths = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prunes low-value nodes from the graph.
    /// </summary>
    /// <param name="graphId">The graph ID.</param>
    /// <param name="minConfidence">Minimum confidence to keep.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of nodes pruned.</returns>
    Task<int> PruneGraphAsync(
        string graphId,
        double minConfidence = 0.3,
        CancellationToken cancellationToken = default);
}
