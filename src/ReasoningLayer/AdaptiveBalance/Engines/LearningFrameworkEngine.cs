using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;

/// <summary>
/// Engine implementing the learning framework for continuous improvement.
/// Captures learning events from agent interactions, identifies patterns,
/// and provides mistake prevention insights based on historical failures.
/// </summary>
public sealed class LearningFrameworkEngine : ILearningFrameworkPort
{
    private readonly ILogger<LearningFrameworkEngine> _logger;
    private readonly ConcurrentDictionary<string, List<LearningEvent>> _eventsByPattern = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LearningFrameworkEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public LearningFrameworkEngine(ILogger<LearningFrameworkEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task RecordEventAsync(LearningEvent learningEvent, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(learningEvent);

        if (string.IsNullOrWhiteSpace(learningEvent.PatternType))
        {
            throw new ArgumentException("PatternType is required.", nameof(learningEvent));
        }

        if (string.IsNullOrWhiteSpace(learningEvent.Description))
        {
            throw new ArgumentException("Description is required.", nameof(learningEvent));
        }

        if (string.IsNullOrWhiteSpace(learningEvent.SourceAgentId))
        {
            throw new ArgumentException("SourceAgentId is required.", nameof(learningEvent));
        }

        _logger.LogInformation(
            "Recording learning event {EventId} of type '{PatternType}' with outcome {Outcome} from agent {SourceAgentId}.",
            learningEvent.EventId, learningEvent.PatternType, learningEvent.Outcome, learningEvent.SourceAgentId);

        _eventsByPattern.AddOrUpdate(
            learningEvent.PatternType,
            _ => [learningEvent],
            (_, existing) =>
            {
                existing.Add(learningEvent);
                return existing;
            });

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LearningEvent>> GetPatternsAsync(
        string patternType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(patternType))
        {
            throw new ArgumentException("PatternType is required.", nameof(patternType));
        }

        _logger.LogInformation("Retrieving patterns for type '{PatternType}'.", patternType);

        if (_eventsByPattern.TryGetValue(patternType, out var events))
        {
            return Task.FromResult<IReadOnlyList<LearningEvent>>(events.AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<LearningEvent>>(Array.Empty<LearningEvent>());
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<LearningEvent>> GetMistakePreventionInsightsAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving mistake prevention insights from failure events.");

        var failureEvents = _eventsByPattern.Values
            .SelectMany(events => events)
            .Where(e => e.Outcome == LearningOutcome.Failure)
            .OrderByDescending(e => e.RecordedAt)
            .ToList();

        _logger.LogInformation("Found {Count} failure events for mistake prevention.", failureEvents.Count);

        return Task.FromResult<IReadOnlyList<LearningEvent>>(failureEvents.AsReadOnly());
    }
}
