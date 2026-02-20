using System.Collections.Concurrent;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Ports;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Services;

/// <summary>
/// In-memory implementation of the adaptive balance service, providing
/// spectrum dimension management, manual overrides, learning evidence tracking,
/// and reflexion status monitoring.
/// </summary>
public class AdaptiveBalanceService : IAdaptiveBalanceServicePort
{
    private readonly ILogger<AdaptiveBalanceService> _logger;
    private readonly ConcurrentDictionary<string, DimensionState> _dimensions = new();
    private readonly ConcurrentDictionary<string, List<SpectrumHistoryEntry>> _history = new();
    private readonly ConcurrentBag<LearningEvent> _learningEvents = new();
    private readonly ConcurrentBag<ReflexionStatusEntry> _reflexionResults = new();

    private static readonly string[] DefaultDimensions =
    [
        "Profit",
        "Risk",
        "Agreeableness",
        "IdentityGrounding",
        "LearningRate"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveBalanceService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for structured logging.</param>
    public AdaptiveBalanceService(ILogger<AdaptiveBalanceService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize all dimensions with default value of 0.5
        foreach (var dimension in DefaultDimensions)
        {
            _dimensions[dimension] = new DimensionState
            {
                Value = 0.5,
                Rationale = "Default initial position."
            };

            _history[dimension] =
            [
                new SpectrumHistoryEntry
                {
                    Value = 0.5,
                    Rationale = "Default initial position.",
                    RecordedAt = DateTimeOffset.UtcNow
                }
            ];
        }
    }

    /// <inheritdoc />
    public Task<BalanceResponse> GetBalanceAsync(BalanceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var dimensions = new List<SpectrumDimensionResult>();

        foreach (var dimensionName in DefaultDimensions)
        {
            var state = _dimensions.GetOrAdd(dimensionName, _ => new DimensionState
            {
                Value = 0.5,
                Rationale = "Default initial position."
            });

            dimensions.Add(new SpectrumDimensionResult
            {
                Dimension = dimensionName,
                Value = state.Value,
                LowerBound = Math.Max(0.0, state.Value - 0.1),
                UpperBound = Math.Min(1.0, state.Value + 0.1),
                Rationale = state.Rationale
            });
        }

        var overallConfidence = _reflexionResults.IsEmpty
            ? 0.5
            : 1.0 - _reflexionResults.Count(r => r.IsHallucination) / (double)_reflexionResults.Count;

        _logger.LogInformation(
            "Balance retrieved with {DimensionCount} dimensions, overall confidence {Confidence}",
            dimensions.Count, overallConfidence);

        return Task.FromResult(new BalanceResponse
        {
            Dimensions = dimensions,
            OverallConfidence = Math.Round(overallConfidence, 4),
            GeneratedAt = DateTimeOffset.UtcNow
        });
    }

    /// <inheritdoc />
    public Task<OverrideResponse> ApplyOverrideAsync(OverrideRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.Dimension))
        {
            throw new ArgumentException("Dimension is required.", nameof(request));
        }

        if (request.NewValue < 0.0 || request.NewValue > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "NewValue must be between 0.0 and 1.0.");
        }

        var state = _dimensions.GetOrAdd(request.Dimension, _ => new DimensionState
        {
            Value = 0.5,
            Rationale = "Default initial position."
        });

        var oldValue = state.Value;
        var now = DateTimeOffset.UtcNow;

        state.Value = request.NewValue;
        state.Rationale = request.Rationale;

        // Add history entry
        var historyEntries = _history.GetOrAdd(request.Dimension, _ => new List<SpectrumHistoryEntry>());
        lock (historyEntries)
        {
            historyEntries.Add(new SpectrumHistoryEntry
            {
                Value = request.NewValue,
                Rationale = request.Rationale,
                RecordedAt = now
            });
        }

        var overrideId = Guid.NewGuid();

        _logger.LogInformation(
            "Override {OverrideId} applied to dimension {Dimension}: {OldValue} -> {NewValue} by {OverriddenBy}",
            overrideId, request.Dimension, oldValue, request.NewValue, request.OverriddenBy);

        return Task.FromResult(new OverrideResponse
        {
            OverrideId = overrideId,
            Dimension = request.Dimension,
            OldValue = oldValue,
            NewValue = request.NewValue,
            UpdatedAt = now,
            Message = $"Override applied successfully. Dimension '{request.Dimension}' changed from {oldValue} to {request.NewValue}."
        });
    }

    /// <inheritdoc />
    public Task<SpectrumHistoryResponse> GetSpectrumHistoryAsync(string dimension, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(dimension);

        cancellationToken.ThrowIfCancellationRequested();

        var historyEntries = _history.GetOrAdd(dimension, _ => new List<SpectrumHistoryEntry>());

        List<SpectrumHistoryEntry> snapshot;
        lock (historyEntries)
        {
            snapshot = historyEntries.OrderByDescending(h => h.RecordedAt).ToList();
        }

        _logger.LogInformation(
            "History retrieved for dimension {Dimension}: {EntryCount} entries",
            dimension, snapshot.Count);

        return Task.FromResult(new SpectrumHistoryResponse
        {
            Dimension = dimension,
            History = snapshot
        });
    }

    /// <inheritdoc />
    public Task<LearningEvidenceResponse> SubmitLearningEvidenceAsync(LearningEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var eventId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        _learningEvents.Add(new LearningEvent
        {
            EventId = eventId,
            PatternType = request.PatternType,
            Description = request.Description,
            Evidence = request.Evidence,
            Outcome = request.Outcome,
            SourceAgentId = request.SourceAgentId,
            RecordedAt = now
        });

        _logger.LogInformation(
            "Learning evidence {EventId} submitted by agent {SourceAgentId}: pattern type {PatternType}",
            eventId, request.SourceAgentId, request.PatternType);

        return Task.FromResult(new LearningEvidenceResponse
        {
            EventId = eventId,
            RecordedAt = now,
            Message = "Learning evidence recorded successfully."
        });
    }

    /// <inheritdoc />
    public Task<ReflexionStatusResponse> GetReflexionStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = _reflexionResults.OrderByDescending(r => r.EvaluatedAt).ToList();

        var hallucinationRate = results.Count > 0
            ? (double)results.Count(r => r.IsHallucination) / results.Count
            : 0.0;

        var averageConfidence = results.Count > 0
            ? results.Average(r => r.Confidence)
            : 0.0;

        _logger.LogInformation(
            "Reflexion status retrieved: {ResultCount} results, hallucination rate {Rate}, average confidence {Confidence}",
            results.Count, hallucinationRate, averageConfidence);

        return Task.FromResult(new ReflexionStatusResponse
        {
            RecentResults = results,
            HallucinationRate = Math.Round(hallucinationRate, 4),
            AverageConfidence = Math.Round(averageConfidence, 4)
        });
    }

    /// <summary>
    /// Internal state for tracking the current value and rationale of a spectrum dimension.
    /// </summary>
    internal class DimensionState
    {
        /// <summary>The current value (0.0 to 1.0).</summary>
        public double Value { get; set; }

        /// <summary>The rationale for the current value.</summary>
        public string Rationale { get; set; } = string.Empty;
    }

    /// <summary>
    /// Internal record for tracking submitted learning events.
    /// </summary>
    internal class LearningEvent
    {
        /// <summary>The unique event identifier.</summary>
        public Guid EventId { get; set; }

        /// <summary>The pattern type.</summary>
        public string PatternType { get; set; } = string.Empty;

        /// <summary>The description.</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>The supporting evidence.</summary>
        public string Evidence { get; set; } = string.Empty;

        /// <summary>The observed outcome.</summary>
        public string Outcome { get; set; } = string.Empty;

        /// <summary>The source agent identifier.</summary>
        public string SourceAgentId { get; set; } = string.Empty;

        /// <summary>When the event was recorded.</summary>
        public DateTimeOffset RecordedAt { get; set; }
    }
}
