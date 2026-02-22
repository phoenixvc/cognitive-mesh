using System.Collections.Concurrent;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;
using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Engines;

/// <summary>
/// Engine implementing adaptive balance logic across spectrum dimensions and milestone workflows.
/// Generates context-aware balance recommendations, supports manual overrides,
/// and manages phased milestone workflows with rollback capabilities.
/// </summary>
public sealed class AdaptiveBalanceEngine : IAdaptiveBalancePort, IMilestoneWorkflowPort
{
    private readonly ILogger<AdaptiveBalanceEngine> _logger;
    private readonly ConcurrentDictionary<SpectrumDimension, List<SpectrumPosition>> _spectrumHistory = new();
    private readonly ConcurrentDictionary<Guid, MilestoneWorkflow> _workflows = new();
    private readonly ConcurrentDictionary<SpectrumDimension, double> _currentPositions = new();

    /// <summary>
    /// Default position value for spectrum dimensions when no context is available.
    /// </summary>
    public const double DefaultPosition = 0.5;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveBalanceEngine"/> class.
    /// </summary>
    /// <param name="logger">Logger for structured logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public AdaptiveBalanceEngine(ILogger<AdaptiveBalanceEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<BalanceRecommendation> GetBalanceAsync(
        Dictionary<string, string> context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        _logger.LogInformation("Generating balance recommendation with {ContextCount} context entries.", context.Count);

        var dimensions = new List<SpectrumPosition>();

        foreach (var dimension in Enum.GetValues<SpectrumDimension>())
        {
            var (value, rationale) = CalculatePosition(dimension, context);
            var position = new SpectrumPosition(
                Dimension: dimension,
                Value: value,
                LowerBound: Math.Max(0.0, value - 0.2),
                UpperBound: Math.Min(1.0, value + 0.2),
                Rationale: rationale,
                RecommendedBy: "AdaptiveBalanceEngine");

            dimensions.Add(position);
            _currentPositions[dimension] = value;

            _spectrumHistory.AddOrUpdate(
                dimension,
                _ => [position],
                (_, existing) =>
                {
                    existing.Add(position);
                    return existing;
                });
        }

        var confidence = CalculateOverallConfidence(context);

        var recommendation = new BalanceRecommendation(
            RecommendationId: Guid.NewGuid(),
            Dimensions: dimensions,
            Context: new Dictionary<string, string>(context),
            OverallConfidence: confidence,
            GeneratedAt: DateTimeOffset.UtcNow);

        _logger.LogInformation(
            "Generated recommendation {RecommendationId} with confidence {Confidence}.",
            recommendation.RecommendationId, confidence);

        return Task.FromResult(recommendation);
    }

    /// <inheritdoc />
    public Task<BalanceRecommendation> ApplyOverrideAsync(
        BalanceOverride balanceOverride, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(balanceOverride);

        if (balanceOverride.NewValue < 0.0 || balanceOverride.NewValue > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(balanceOverride),
                $"NewValue must be between 0.0 and 1.0, but was {balanceOverride.NewValue}.");
        }

        _logger.LogInformation(
            "Applying override to dimension {Dimension}: {OriginalValue} -> {NewValue} by {OverriddenBy}.",
            balanceOverride.Dimension, balanceOverride.OriginalValue,
            balanceOverride.NewValue, balanceOverride.OverriddenBy);

        _currentPositions[balanceOverride.Dimension] = balanceOverride.NewValue;

        var overridePosition = new SpectrumPosition(
            Dimension: balanceOverride.Dimension,
            Value: balanceOverride.NewValue,
            LowerBound: Math.Max(0.0, balanceOverride.NewValue - 0.1),
            UpperBound: Math.Min(1.0, balanceOverride.NewValue + 0.1),
            Rationale: $"Manual override: {balanceOverride.Rationale}",
            RecommendedBy: balanceOverride.OverriddenBy);

        _spectrumHistory.AddOrUpdate(
            balanceOverride.Dimension,
            _ => [overridePosition],
            (_, existing) =>
            {
                existing.Add(overridePosition);
                return existing;
            });

        var dimensions = new List<SpectrumPosition>();
        foreach (var dimension in Enum.GetValues<SpectrumDimension>())
        {
            if (dimension == balanceOverride.Dimension)
            {
                dimensions.Add(overridePosition);
            }
            else
            {
                var currentValue = _currentPositions.GetOrAdd(dimension, DefaultPosition);
                dimensions.Add(new SpectrumPosition(
                    Dimension: dimension,
                    Value: currentValue,
                    LowerBound: Math.Max(0.0, currentValue - 0.2),
                    UpperBound: Math.Min(1.0, currentValue + 0.2),
                    Rationale: "Carried forward from previous recommendation.",
                    RecommendedBy: "AdaptiveBalanceEngine"));
            }
        }

        var recommendation = new BalanceRecommendation(
            RecommendationId: Guid.NewGuid(),
            Dimensions: dimensions,
            Context: new Dictionary<string, string> { ["override_applied"] = "true" },
            OverallConfidence: 0.9,
            GeneratedAt: DateTimeOffset.UtcNow);

        return Task.FromResult(recommendation);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<SpectrumPosition>> GetSpectrumHistoryAsync(
        SpectrumDimension dimension, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving spectrum history for dimension {Dimension}.", dimension);

        if (_spectrumHistory.TryGetValue(dimension, out var history))
        {
            return Task.FromResult<IReadOnlyList<SpectrumPosition>>(history.AsReadOnly());
        }

        return Task.FromResult<IReadOnlyList<SpectrumPosition>>(Array.Empty<SpectrumPosition>());
    }

    /// <inheritdoc />
    public Task<MilestoneWorkflow> CreateWorkflowAsync(
        List<MilestonePhase> phases, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(phases);

        if (phases.Count == 0)
        {
            throw new ArgumentException("At least one phase is required to create a workflow.", nameof(phases));
        }

        _logger.LogInformation("Creating workflow with {PhaseCount} phases.", phases.Count);

        var workflow = new MilestoneWorkflow
        {
            WorkflowId = Guid.NewGuid(),
            Phases = new List<MilestonePhase>(phases),
            CurrentPhaseIndex = 0,
            Status = WorkflowStatus.NotStarted,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = null
        };

        _workflows[workflow.WorkflowId] = workflow;

        _logger.LogInformation("Created workflow {WorkflowId} with status {Status}.", workflow.WorkflowId, workflow.Status);

        return Task.FromResult(workflow);
    }

    /// <inheritdoc />
    public Task<MilestoneWorkflow> AdvancePhaseAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
        {
            throw new InvalidOperationException($"Workflow {workflowId} not found.");
        }

        if (workflow.Status == WorkflowStatus.Completed)
        {
            throw new InvalidOperationException($"Workflow {workflowId} is already completed.");
        }

        if (workflow.Status == WorkflowStatus.Failed)
        {
            throw new InvalidOperationException($"Workflow {workflowId} has failed and cannot advance.");
        }

        _logger.LogInformation(
            "Advancing workflow {WorkflowId} from phase {CurrentPhase}.",
            workflowId, workflow.CurrentPhaseIndex);

        if (workflow.Status == WorkflowStatus.NotStarted)
        {
            workflow.Status = WorkflowStatus.InProgress;
        }

        if (workflow.CurrentPhaseIndex >= workflow.Phases.Count - 1)
        {
            workflow.Status = WorkflowStatus.Completed;
            workflow.CompletedAt = DateTimeOffset.UtcNow;
            _logger.LogInformation("Workflow {WorkflowId} completed.", workflowId);
        }
        else
        {
            workflow.CurrentPhaseIndex++;
            _logger.LogInformation(
                "Workflow {WorkflowId} advanced to phase {CurrentPhase} ({PhaseName}).",
                workflowId, workflow.CurrentPhaseIndex, workflow.Phases[workflow.CurrentPhaseIndex].Name);
        }

        return Task.FromResult(workflow);
    }

    /// <inheritdoc />
    public Task<MilestoneWorkflow> RollbackPhaseAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        if (!_workflows.TryGetValue(workflowId, out var workflow))
        {
            throw new InvalidOperationException($"Workflow {workflowId} not found.");
        }

        if (workflow.Status == WorkflowStatus.NotStarted)
        {
            throw new InvalidOperationException($"Workflow {workflowId} has not started and cannot be rolled back.");
        }

        var currentPhase = workflow.Phases[workflow.CurrentPhaseIndex];
        var rollbackToPhaseId = currentPhase.RollbackToPhaseId;

        if (string.IsNullOrWhiteSpace(rollbackToPhaseId))
        {
            throw new InvalidOperationException(
                $"Phase '{currentPhase.PhaseId}' does not support rollback (no RollbackToPhaseId defined).");
        }

        var targetIndex = workflow.Phases.FindIndex(p => p.PhaseId == rollbackToPhaseId);

        if (targetIndex < 0)
        {
            throw new InvalidOperationException(
                $"Rollback target phase '{rollbackToPhaseId}' not found in workflow {workflowId}.");
        }

        _logger.LogInformation(
            "Rolling back workflow {WorkflowId} from phase {FromPhase} to phase {ToPhase} ({ToPhaseId}).",
            workflowId, workflow.CurrentPhaseIndex, targetIndex, rollbackToPhaseId);

        workflow.CurrentPhaseIndex = targetIndex;
        workflow.Status = WorkflowStatus.RolledBack;

        return Task.FromResult(workflow);
    }

    /// <inheritdoc />
    public Task<MilestoneWorkflow?> GetWorkflowAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving workflow {WorkflowId}.", workflowId);

        _workflows.TryGetValue(workflowId, out var workflow);
        return Task.FromResult(workflow);
    }

    /// <summary>
    /// Calculates the optimal position for a spectrum dimension based on context.
    /// </summary>
    private static (double Value, string Rationale) CalculatePosition(
        SpectrumDimension dimension, Dictionary<string, string> context)
    {
        var value = DefaultPosition;
        var rationale = $"Default position for {dimension}.";

        switch (dimension)
        {
            case SpectrumDimension.Profit:
                if (context.TryGetValue("revenue_target", out var revenueTarget))
                {
                    if (double.TryParse(revenueTarget, out var target) && target > 0)
                    {
                        value = Math.Min(1.0, 0.5 + (target / 1_000_000.0) * 0.3);
                        rationale = $"Adjusted profit position based on revenue target of {revenueTarget}.";
                    }
                }
                break;

            case SpectrumDimension.Risk:
                if (context.TryGetValue("threat_level", out var threatLevel))
                {
                    value = threatLevel.ToLowerInvariant() switch
                    {
                        "high" or "critical" => 0.2,
                        "medium" => 0.4,
                        "low" => 0.7,
                        _ => DefaultPosition
                    };
                    rationale = $"Risk tolerance adjusted to {value} based on threat level '{threatLevel}'.";
                }
                break;

            case SpectrumDimension.Agreeableness:
                if (context.TryGetValue("customer_sentiment", out var sentiment))
                {
                    value = sentiment.ToLowerInvariant() switch
                    {
                        "positive" => 0.7,
                        "neutral" => 0.5,
                        "negative" => 0.3,
                        _ => DefaultPosition
                    };
                    rationale = $"Agreeableness adjusted based on customer sentiment '{sentiment}'.";
                }
                break;

            case SpectrumDimension.IdentityGrounding:
                if (context.TryGetValue("compliance_mode", out var complianceMode))
                {
                    value = complianceMode.ToLowerInvariant() switch
                    {
                        "strict" => 0.9,
                        "standard" => 0.6,
                        "relaxed" => 0.3,
                        _ => DefaultPosition
                    };
                    rationale = $"Identity grounding set to {value} based on compliance mode '{complianceMode}'.";
                }
                break;

            case SpectrumDimension.LearningRate:
                if (context.TryGetValue("data_volume", out var dataVolume))
                {
                    value = dataVolume.ToLowerInvariant() switch
                    {
                        "high" => 0.8,
                        "medium" => 0.5,
                        "low" => 0.3,
                        _ => DefaultPosition
                    };
                    rationale = $"Learning rate adjusted to {value} based on data volume '{dataVolume}'.";
                }
                break;
        }

        return (value, rationale);
    }

    /// <summary>
    /// Calculates overall confidence based on how much context is available.
    /// </summary>
    private static double CalculateOverallConfidence(Dictionary<string, string> context)
    {
        var knownKeys = new[] { "threat_level", "revenue_target", "customer_sentiment", "compliance_mode", "data_volume" };
        var matchCount = context.Keys.Count(k => knownKeys.Contains(k, StringComparer.OrdinalIgnoreCase));

        return matchCount switch
        {
            0 => 0.3,
            1 => 0.5,
            2 => 0.65,
            3 => 0.8,
            4 => 0.9,
            _ => 0.95
        };
    }
}
