namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Search strategy for tree exploration.
/// </summary>
public enum TreeSearchStrategy
{
    /// <summary>Breadth-first search.</summary>
    BreadthFirst,
    /// <summary>Depth-first search.</summary>
    DepthFirst,
    /// <summary>Best-first search using heuristic.</summary>
    BestFirst,
    /// <summary>Monte Carlo Tree Search.</summary>
    MCTS,
    /// <summary>Beam search with limited width.</summary>
    BeamSearch
}

/// <summary>
/// Configuration for tree search.
/// </summary>
public class TreeSearchConfiguration
{
    /// <summary>Search strategy to use.</summary>
    public TreeSearchStrategy Strategy { get; init; } = TreeSearchStrategy.BestFirst;

    /// <summary>Maximum tree depth.</summary>
    public int MaxDepth { get; init; } = 10;

    /// <summary>Maximum nodes to expand.</summary>
    public int MaxNodes { get; init; } = 100;

    /// <summary>Beam width (for beam search).</summary>
    public int BeamWidth { get; init; } = 5;

    /// <summary>MCTS exploration constant.</summary>
    public double ExplorationConstant { get; init; } = 1.414;

    /// <summary>MCTS simulation count per node.</summary>
    public int SimulationsPerNode { get; init; } = 10;

    /// <summary>Whether to enable pruning.</summary>
    public bool EnablePruning { get; init; } = true;

    /// <summary>Pruning threshold (nodes below this score are pruned).</summary>
    public double PruningThreshold { get; init; } = 0.1;

    /// <summary>Timeout for search in milliseconds.</summary>
    public int TimeoutMs { get; init; } = 60000;
}

/// <summary>
/// Represents a node in the thought/search tree.
/// </summary>
public class ThoughtNode
{
    /// <summary>Unique identifier for this node.</summary>
    public string NodeId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Parent node ID (null for root).</summary>
    public string? ParentId { get; init; }

    /// <summary>Depth in the tree (0 for root).</summary>
    public int Depth { get; init; }

    /// <summary>The thought/reasoning at this node.</summary>
    public required string Thought { get; init; }

    /// <summary>Action taken at this node (if any).</summary>
    public string? Action { get; init; }

    /// <summary>Result of the action (if taken).</summary>
    public string? ActionResult { get; init; }

    /// <summary>Heuristic score for this node.</summary>
    public double Score { get; init; }

    /// <summary>Visit count (for MCTS).</summary>
    public int VisitCount { get; init; }

    /// <summary>Total reward (for MCTS).</summary>
    public double TotalReward { get; init; }

    /// <summary>Whether this is a terminal node.</summary>
    public bool IsTerminal { get; init; }

    /// <summary>Whether this node is pruned.</summary>
    public bool IsPruned { get; init; }

    /// <summary>Child node IDs.</summary>
    public IReadOnlyList<string> ChildIds { get; init; } = Array.Empty<string>();

    /// <summary>When the node was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Metadata for the node.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Represents a complete search tree.
/// </summary>
public class SearchTree
{
    /// <summary>Unique identifier for this tree.</summary>
    public string TreeId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>The root problem or question.</summary>
    public required string RootProblem { get; init; }

    /// <summary>Root node ID.</summary>
    public required string RootNodeId { get; init; }

    /// <summary>All nodes in the tree.</summary>
    public IReadOnlyDictionary<string, ThoughtNode> Nodes { get; init; } = new Dictionary<string, ThoughtNode>();

    /// <summary>Best path found (node IDs from root to solution).</summary>
    public IReadOnlyList<string> BestPath { get; init; } = Array.Empty<string>();

    /// <summary>Score of the best path.</summary>
    public double BestPathScore { get; init; }

    /// <summary>Total nodes expanded.</summary>
    public int TotalNodesExpanded { get; init; }

    /// <summary>Maximum depth reached.</summary>
    public int MaxDepthReached { get; init; }

    /// <summary>When the search started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When the search completed.</summary>
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>Whether a solution was found.</summary>
    public bool SolutionFound { get; init; }
}

/// <summary>
/// Request to expand a node in the tree.
/// </summary>
public class NodeExpansionRequest
{
    /// <summary>Tree ID.</summary>
    public required string TreeId { get; init; }

    /// <summary>Node to expand.</summary>
    public required string NodeId { get; init; }

    /// <summary>Context from parent nodes.</summary>
    public IReadOnlyList<string> Context { get; init; } = Array.Empty<string>();

    /// <summary>Maximum children to generate.</summary>
    public int MaxChildren { get; init; } = 3;
}

/// <summary>
/// Result of search for a problem.
/// </summary>
public class SearchResult
{
    /// <summary>The search tree.</summary>
    public required SearchTree Tree { get; init; }

    /// <summary>Whether a solution was found.</summary>
    public required bool SolutionFound { get; init; }

    /// <summary>The solution (if found).</summary>
    public string? Solution { get; init; }

    /// <summary>Path of reasoning to the solution.</summary>
    public IReadOnlyList<string> ReasoningPath { get; init; } = Array.Empty<string>();

    /// <summary>Confidence in the solution (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Alternative solutions found.</summary>
    public IReadOnlyList<string> Alternatives { get; init; } = Array.Empty<string>();

    /// <summary>Search statistics.</summary>
    public SearchStatistics Statistics { get; init; } = new();
}

/// <summary>
/// Statistics from a search operation.
/// </summary>
public class SearchStatistics
{
    public int NodesExpanded { get; init; }
    public int NodesPruned { get; init; }
    public int MaxDepth { get; init; }
    public double AverageBranchingFactor { get; init; }
    public TimeSpan Duration { get; init; }
    public int LLMCalls { get; init; }
    public bool TimedOut { get; init; }
}

/// <summary>
/// Port for tree-based reasoning and search.
/// Implements "Tree-of-Thought Reasoning" and "Language Agent Tree Search (LATS)" patterns.
/// </summary>
/// <remarks>
/// This port provides tree-based exploration of solution spaces, enabling
/// branching reasoning paths, backtracking, and systematic search for
/// complex problems that benefit from exploring multiple approaches.
/// </remarks>
public interface ITreeSearchPort
{
    /// <summary>
    /// Initiates a search for a solution to a problem.
    /// </summary>
    /// <param name="problem">The problem to solve.</param>
    /// <param name="configuration">Search configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search result.</returns>
    Task<SearchResult> SearchAsync(
        string problem,
        TreeSearchConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new search tree.
    /// </summary>
    /// <param name="problem">The root problem.</param>
    /// <param name="configuration">Search configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new search tree with root node.</returns>
    Task<SearchTree> CreateTreeAsync(
        string problem,
        TreeSearchConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Expands a node in the search tree.
    /// </summary>
    /// <param name="request">The expansion request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The new child nodes.</returns>
    Task<IReadOnlyList<ThoughtNode>> ExpandNodeAsync(
        NodeExpansionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Evaluates a node's quality/score.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="nodeId">The node to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The evaluation score (0.0 - 1.0).</returns>
    Task<double> EvaluateNodeAsync(
        string treeId,
        string nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs backtracking from a failed path.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="fromNodeId">Node to backtrack from.</param>
    /// <param name="reason">Reason for backtracking.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The node to continue from.</returns>
    Task<ThoughtNode?> BacktrackAsync(
        string treeId,
        string fromNodeId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Prunes unpromising branches.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="threshold">Score threshold for pruning.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of nodes pruned.</returns>
    Task<int> PruneTreeAsync(
        string treeId,
        double threshold,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current best path in a tree.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The best path and its score.</returns>
    Task<(IReadOnlyList<ThoughtNode> Path, double Score)> GetBestPathAsync(
        string treeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific node.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="nodeId">The node ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The node.</returns>
    Task<ThoughtNode?> GetNodeAsync(
        string treeId,
        string nodeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the complete tree.
    /// </summary>
    /// <param name="treeId">The tree ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The search tree.</returns>
    Task<SearchTree?> GetTreeAsync(
        string treeId,
        CancellationToken cancellationToken = default);
}
