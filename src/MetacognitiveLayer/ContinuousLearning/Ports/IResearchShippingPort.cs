namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// A research experiment.
/// </summary>
public class ResearchExperiment
{
    /// <summary>Experiment identifier.</summary>
    public string ExperimentId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Hypothesis.</summary>
    public required string Hypothesis { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Status.</summary>
    public ExperimentStatus Status { get; init; }

    /// <summary>Feature/implementation.</summary>
    public string? FeatureId { get; init; }

    /// <summary>Metrics to track.</summary>
    public IReadOnlyList<string> Metrics { get; init; } = Array.Empty<string>();

    /// <summary>Success criteria.</summary>
    public IReadOnlyList<string> SuccessCriteria { get; init; } = Array.Empty<string>();

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Shipped at.</summary>
    public DateTimeOffset? ShippedAt { get; init; }

    /// <summary>Completed at.</summary>
    public DateTimeOffset? CompletedAt { get; init; }
}

/// <summary>
/// Experiment status.
/// </summary>
public enum ExperimentStatus
{
    Proposed,
    Approved,
    InDevelopment,
    Shipped,
    Measuring,
    Completed,
    Failed,
    Abandoned
}

/// <summary>
/// Experiment results.
/// </summary>
public class ExperimentResults
{
    /// <summary>Experiment identifier.</summary>
    public required string ExperimentId { get; init; }

    /// <summary>Whether hypothesis validated.</summary>
    public bool HypothesisValidated { get; init; }

    /// <summary>Metric results.</summary>
    public Dictionary<string, double> MetricResults { get; init; } = new();

    /// <summary>Learnings.</summary>
    public IReadOnlyList<string> Learnings { get; init; } = Array.Empty<string>();

    /// <summary>Recommendations.</summary>
    public IReadOnlyList<string> Recommendations { get; init; } = Array.Empty<string>();

    /// <summary>Should continue.</summary>
    public bool ShouldContinue { get; init; }

    /// <summary>Should rollback.</summary>
    public bool ShouldRollback { get; init; }

    /// <summary>Analyzed at.</summary>
    public DateTimeOffset AnalyzedAt { get; init; }
}

/// <summary>
/// Port for shipping as research pattern.
/// Implements the "Shipping as Research" pattern for research-shipping loop.
/// </summary>
public interface IResearchShippingPort
{
    /// <summary>
    /// Creates a research experiment.
    /// </summary>
    Task<ResearchExperiment> CreateExperimentAsync(
        ResearchExperiment experiment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an experiment.
    /// </summary>
    Task<ResearchExperiment?> GetExperimentAsync(
        string experimentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates experiment status.
    /// </summary>
    Task<ResearchExperiment> UpdateStatusAsync(
        string experimentId,
        ExperimentStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links experiment to shipped feature.
    /// </summary>
    Task LinkToFeatureAsync(
        string experimentId,
        string featureId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records metric data.
    /// </summary>
    Task RecordMetricAsync(
        string experimentId,
        string metricName,
        double value,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyzes experiment results.
    /// </summary>
    Task<ExperimentResults> AnalyzeResultsAsync(
        string experimentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists experiments.
    /// </summary>
    Task<IReadOnlyList<ResearchExperiment>> ListExperimentsAsync(
        ExperimentStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes an experiment.
    /// </summary>
    Task<ResearchExperiment> CompleteExperimentAsync(
        string experimentId,
        ExperimentResults results,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets learnings from experiments.
    /// </summary>
    Task<IReadOnlyList<string>> GetLearningsAsync(
        DateTimeOffset? since = null,
        CancellationToken cancellationToken = default);
}
