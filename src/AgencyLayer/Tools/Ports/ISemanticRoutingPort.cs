namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A route definition.
/// </summary>
public class SemanticRoute
{
    /// <summary>Route identifier.</summary>
    public required string RouteId { get; init; }

    /// <summary>Route name.</summary>
    public required string Name { get; init; }

    /// <summary>Description (used for semantic matching).</summary>
    public required string Description { get; init; }

    /// <summary>Example utterances.</summary>
    public IReadOnlyList<string> Examples { get; init; } = Array.Empty<string>();

    /// <summary>Handler/target for this route.</summary>
    public required string Handler { get; init; }

    /// <summary>Handler type (agent, workflow, tool, function).</summary>
    public required string HandlerType { get; init; }

    /// <summary>Embedding for the route.</summary>
    public float[]? Embedding { get; init; }

    /// <summary>Priority (for tie-breaking).</summary>
    public int Priority { get; init; } = 100;

    /// <summary>Whether this route is active.</summary>
    public bool IsActive { get; init; } = true;

    /// <summary>Tags for filtering.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Result of routing.
/// </summary>
public class RoutingResult
{
    /// <summary>Matched route.</summary>
    public SemanticRoute? MatchedRoute { get; init; }

    /// <summary>Match confidence (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>Alternative routes considered.</summary>
    public IReadOnlyList<(SemanticRoute Route, double Confidence)> Alternatives { get; init; }
        = Array.Empty<(SemanticRoute, double)>();

    /// <summary>Whether a route was matched.</summary>
    public bool IsMatched => MatchedRoute != null;

    /// <summary>Extracted parameters.</summary>
    public Dictionary<string, string> Parameters { get; init; } = new();
}

/// <summary>
/// Configuration for semantic routing.
/// </summary>
public class SemanticRoutingConfiguration
{
    /// <summary>Minimum confidence for a match.</summary>
    public double MinConfidence { get; init; } = 0.7;

    /// <summary>Number of alternatives to return.</summary>
    public int MaxAlternatives { get; init; } = 3;

    /// <summary>Whether to use LLM for disambiguation.</summary>
    public bool UseLLMDisambiguation { get; init; } = true;

    /// <summary>Whether to extract parameters.</summary>
    public bool ExtractParameters { get; init; } = true;

    /// <summary>Fallback route if no match.</summary>
    public string? FallbackRouteId { get; init; }
}

/// <summary>
/// Port for semantic intent routing.
/// Implements the "Semantic Intent Routing" / "Router Agents" pattern.
/// </summary>
public interface ISemanticRoutingPort
{
    /// <summary>
    /// Routes an input to the best handler.
    /// </summary>
    Task<RoutingResult> RouteAsync(
        string input,
        SemanticRoutingConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a route.
    /// </summary>
    Task RegisterRouteAsync(
        SemanticRoute route,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters a route.
    /// </summary>
    Task UnregisterRouteAsync(
        string routeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a route.
    /// </summary>
    Task UpdateRouteAsync(
        SemanticRoute route,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a route.
    /// </summary>
    Task<SemanticRoute?> GetRouteAsync(
        string routeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists routes.
    /// </summary>
    Task<IReadOnlyList<SemanticRoute>> ListRoutesAsync(
        string? handlerType = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Trains/updates embeddings for routes.
    /// </summary>
    Task TrainAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests routing with sample inputs.
    /// </summary>
    Task<IReadOnlyList<(string Input, RoutingResult Result)>> TestRoutingAsync(
        IEnumerable<string> inputs,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets routing statistics.
    /// </summary>
    Task<RoutingStatistics> GetStatisticsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Routing statistics.
/// </summary>
public class RoutingStatistics
{
    public int TotalRoutes { get; init; }
    public int TotalRoutingRequests { get; init; }
    public int SuccessfulMatches { get; init; }
    public int FallbackUsed { get; init; }
    public double AverageConfidence { get; init; }
    public Dictionary<string, int> RouteCounts { get; init; } = new();
}
