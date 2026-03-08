namespace BusinessApplications.CustomerIntelligence.Ports;

/// <summary>
/// A latent demand signal.
/// </summary>
public class LatentDemandSignal
{
    /// <summary>Signal identifier.</summary>
    public string SignalId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Signal type.</summary>
    public DemandSignalType Type { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Strength (0.0 - 1.0).</summary>
    public double Strength { get; init; }

    /// <summary>Source (feedback, usage, search, etc.).</summary>
    public required string Source { get; init; }

    /// <summary>Related product/feature.</summary>
    public string? RelatedProduct { get; init; }

    /// <summary>Customer segment.</summary>
    public string? Segment { get; init; }

    /// <summary>Evidence.</summary>
    public IReadOnlyList<string> Evidence { get; init; } = Array.Empty<string>();

    /// <summary>Frequency of signal.</summary>
    public int Frequency { get; init; }

    /// <summary>Trend direction.</summary>
    public TrendDirection Trend { get; init; }

    /// <summary>First observed.</summary>
    public DateTimeOffset FirstObserved { get; init; }

    /// <summary>Last observed.</summary>
    public DateTimeOffset LastObserved { get; init; }
}

/// <summary>
/// Demand signal type.
/// </summary>
public enum DemandSignalType
{
    /// <summary>Feature request.</summary>
    FeatureRequest,
    /// <summary>Pain point.</summary>
    PainPoint,
    /// <summary>Usage pattern.</summary>
    UsagePattern,
    /// <summary>Search behavior.</summary>
    SearchBehavior,
    /// <summary>Support inquiry.</summary>
    SupportInquiry,
    /// <summary>Competitive gap.</summary>
    CompetitiveGap,
    /// <summary>Integration need.</summary>
    IntegrationNeed
}

/// <summary>
/// Trend direction.
/// </summary>
public enum TrendDirection
{
    Rising,
    Stable,
    Declining,
    Emerging
}

/// <summary>
/// Product opportunity.
/// </summary>
public class ProductOpportunity
{
    /// <summary>Opportunity identifier.</summary>
    public string OpportunityId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Title.</summary>
    public required string Title { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Opportunity type.</summary>
    public required string Type { get; init; }

    /// <summary>Confidence score.</summary>
    public double Confidence { get; init; }

    /// <summary>Potential impact.</summary>
    public ImpactLevel Impact { get; init; }

    /// <summary>Supporting signals.</summary>
    public IReadOnlyList<LatentDemandSignal> Signals { get; init; } = Array.Empty<LatentDemandSignal>();

    /// <summary>Target segments.</summary>
    public IReadOnlyList<string> TargetSegments { get; init; } = Array.Empty<string>();

    /// <summary>Estimated market size.</summary>
    public string? EstimatedMarketSize { get; init; }

    /// <summary>Recommendations.</summary>
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();

    /// <summary>Discovered at.</summary>
    public DateTimeOffset DiscoveredAt { get; init; }
}

/// <summary>
/// Impact level.
/// </summary>
public enum ImpactLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Port for latent demand product discovery.
/// Implements the "Latent Demand Product Discovery" pattern.
/// </summary>
public interface ILatentDemandPort
{
    /// <summary>
    /// Records a demand signal.
    /// </summary>
    Task RecordSignalAsync(
        LatentDemandSignal signal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts signals from feedback.
    /// </summary>
    Task<IReadOnlyList<LatentDemandSignal>> ExtractSignalsFromFeedbackAsync(
        string feedback,
        string source,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes usage patterns for signals.
    /// </summary>
    Task<IReadOnlyList<LatentDemandSignal>> AnalyzeUsagePatternsAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets demand signals.
    /// </summary>
    Task<IReadOnlyList<LatentDemandSignal>> GetSignalsAsync(
        DemandSignalType? type = null,
        string? segment = null,
        double minStrength = 0.0,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Discovers product opportunities.
    /// </summary>
    Task<IReadOnlyList<ProductOpportunity>> DiscoverOpportunitiesAsync(
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top opportunities.
    /// </summary>
    Task<IReadOnlyList<ProductOpportunity>> GetTopOpportunitiesAsync(
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an opportunity.
    /// </summary>
    Task<(bool Valid, IReadOnlyList<string> Concerns)> ValidateOpportunityAsync(
        string opportunityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates opportunity report.
    /// </summary>
    Task<string> GenerateReportAsync(
        DateTimeOffset since,
        string format = "markdown",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets signal trends.
    /// </summary>
    Task<IReadOnlyList<(string Category, TrendDirection Trend, int Count)>> GetTrendsAsync(
        DateTimeOffset since,
        CancellationToken cancellationToken = default);
}
