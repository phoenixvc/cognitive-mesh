namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Cost tier classification.
/// </summary>
public enum CostTier
{
    /// <summary>Free or negligible cost models.</summary>
    Free,
    /// <summary>Low-cost models (e.g., GPT-3.5-turbo).</summary>
    Low,
    /// <summary>Medium-cost models (e.g., GPT-4-turbo).</summary>
    Medium,
    /// <summary>High-cost models (e.g., GPT-4, Claude Opus).</summary>
    High,
    /// <summary>Premium models with highest capability.</summary>
    Premium
}

/// <summary>
/// Budget period types.
/// </summary>
public enum BudgetPeriod
{
    /// <summary>Per-request budget.</summary>
    PerRequest,
    /// <summary>Hourly budget cap.</summary>
    Hourly,
    /// <summary>Daily budget cap.</summary>
    Daily,
    /// <summary>Weekly budget cap.</summary>
    Weekly,
    /// <summary>Monthly budget cap.</summary>
    Monthly
}

/// <summary>
/// Budget configuration for an agent or workflow.
/// </summary>
public class BudgetConfiguration
{
    /// <summary>Unique identifier.</summary>
    public string ConfigurationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Entity this budget applies to (agent ID, workflow ID, user ID).</summary>
    public required string EntityId { get; init; }

    /// <summary>Type of entity (Agent, Workflow, User, Global).</summary>
    public required string EntityType { get; init; }

    /// <summary>Maximum budget per period.</summary>
    public required decimal MaxBudget { get; init; }

    /// <summary>Currency for the budget (USD, EUR, etc.).</summary>
    public string Currency { get; init; } = "USD";

    /// <summary>Budget period.</summary>
    public BudgetPeriod Period { get; init; } = BudgetPeriod.Daily;

    /// <summary>Maximum cost per individual request.</summary>
    public decimal? MaxCostPerRequest { get; init; }

    /// <summary>Allowed cost tiers (empty = all).</summary>
    public IReadOnlyList<CostTier> AllowedTiers { get; init; } = Array.Empty<CostTier>();

    /// <summary>Warning threshold percentage (0.0 - 1.0).</summary>
    public double WarningThreshold { get; init; } = 0.8;

    /// <summary>Whether to hard-stop at budget limit or allow overage.</summary>
    public bool HardCap { get; init; } = true;

    /// <summary>Maximum overage allowed if not hard cap (percentage).</summary>
    public double MaxOveragePercent { get; init; } = 0.1;
}

/// <summary>
/// Current budget usage.
/// </summary>
public class BudgetUsage
{
    /// <summary>The budget configuration ID.</summary>
    public required string ConfigurationId { get; init; }

    /// <summary>Entity ID.</summary>
    public required string EntityId { get; init; }

    /// <summary>Current period start.</summary>
    public DateTimeOffset PeriodStart { get; init; }

    /// <summary>Current period end.</summary>
    public DateTimeOffset PeriodEnd { get; init; }

    /// <summary>Amount spent in current period.</summary>
    public decimal AmountSpent { get; init; }

    /// <summary>Remaining budget in current period.</summary>
    public decimal RemainingBudget { get; init; }

    /// <summary>Percentage of budget used (0.0 - 1.0+).</summary>
    public double UsagePercentage { get; init; }

    /// <summary>Number of requests in current period.</summary>
    public int RequestCount { get; init; }

    /// <summary>Average cost per request.</summary>
    public decimal AverageCostPerRequest { get; init; }

    /// <summary>Whether warning threshold is exceeded.</summary>
    public bool IsWarningThresholdExceeded { get; init; }

    /// <summary>Whether budget is exhausted.</summary>
    public bool IsBudgetExhausted { get; init; }
}

/// <summary>
/// Request for budget-aware routing.
/// </summary>
public class BudgetRoutingRequest
{
    /// <summary>Entity making the request.</summary>
    public required string EntityId { get; init; }

    /// <summary>Type of entity.</summary>
    public required string EntityType { get; init; }

    /// <summary>Estimated input tokens.</summary>
    public int EstimatedInputTokens { get; init; }

    /// <summary>Estimated output tokens.</summary>
    public int EstimatedOutputTokens { get; init; }

    /// <summary>Minimum required capabilities.</summary>
    public IReadOnlyList<string> RequiredCapabilities { get; init; } = Array.Empty<string>();

    /// <summary>Preferred cost tier (null = any within budget).</summary>
    public CostTier? PreferredTier { get; init; }

    /// <summary>Whether to allow overage if needed.</summary>
    public bool AllowOverage { get; init; }
}

/// <summary>
/// Result of budget-aware routing.
/// </summary>
public class BudgetRoutingResult
{
    /// <summary>Whether routing was successful.</summary>
    public required bool IsAllowed { get; init; }

    /// <summary>Selected model ID.</summary>
    public string? SelectedModelId { get; init; }

    /// <summary>Cost tier of selected model.</summary>
    public CostTier? SelectedTier { get; init; }

    /// <summary>Estimated cost of the request.</summary>
    public decimal EstimatedCost { get; init; }

    /// <summary>Remaining budget after this request.</summary>
    public decimal RemainingBudgetAfter { get; init; }

    /// <summary>Reason if denied.</summary>
    public string? DenialReason { get; init; }

    /// <summary>Alternative models that could be used at lower cost.</summary>
    public IReadOnlyList<string> CheaperAlternatives { get; init; } = Array.Empty<string>();

    /// <summary>Estimated time until budget resets.</summary>
    public TimeSpan? TimeUntilBudgetReset { get; init; }
}

/// <summary>
/// Cost record for tracking.
/// </summary>
public class CostRecord
{
    public required string RecordId { get; init; }
    public required string EntityId { get; init; }
    public required string ModelId { get; init; }
    public int InputTokens { get; init; }
    public int OutputTokens { get; init; }
    public required decimal TotalCost { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTimeOffset Timestamp { get; init; }
    public string? RequestId { get; init; }
    public CostTier Tier { get; init; }
}

/// <summary>
/// Port for budget-aware model routing with hard cost caps.
/// Implements the "Budget-Aware Model Routing with Hard Cost Caps" pattern.
/// </summary>
/// <remarks>
/// This port enforces cost budgets by routing requests to models that fit
/// within the remaining budget and blocking requests that would exceed
/// configured limits. It supports per-entity budgets with configurable periods.
/// </remarks>
public interface IBudgetRoutingPort
{
    /// <summary>
    /// Routes a request based on budget constraints.
    /// </summary>
    /// <param name="request">The routing request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The routing result.</returns>
    Task<BudgetRoutingResult> RouteWithBudgetAsync(
        BudgetRoutingRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a cost against an entity's budget.
    /// </summary>
    /// <param name="record">The cost record.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordCostAsync(
        CostRecord record,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current budget usage for an entity.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The budget usage.</returns>
    Task<BudgetUsage?> GetBudgetUsageAsync(
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or updates a budget configuration.
    /// </summary>
    /// <param name="configuration">The budget configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpsertBudgetConfigurationAsync(
        BudgetConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a budget configuration.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The budget configuration.</returns>
    Task<BudgetConfiguration?> GetBudgetConfigurationAsync(
        string entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cost history for an entity.
    /// </summary>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="since">Start time.</param>
    /// <param name="limit">Maximum records.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cost records.</returns>
    Task<IReadOnlyList<CostRecord>> GetCostHistoryAsync(
        string entityId,
        DateTimeOffset since,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets aggregate cost statistics.
    /// </summary>
    /// <param name="entityId">Entity ID (null = all).</param>
    /// <param name="period">Period to aggregate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Cost statistics.</returns>
    Task<CostStatistics> GetCostStatisticsAsync(
        string? entityId = null,
        BudgetPeriod period = BudgetPeriod.Daily,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimates the cost for a hypothetical request.
    /// </summary>
    /// <param name="modelId">The model to estimate for.</param>
    /// <param name="inputTokens">Estimated input tokens.</param>
    /// <param name="outputTokens">Estimated output tokens.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Estimated cost.</returns>
    Task<decimal> EstimateCostAsync(
        string modelId,
        int inputTokens,
        int outputTokens,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Aggregate cost statistics.
/// </summary>
public class CostStatistics
{
    public decimal TotalCost { get; init; }
    public int TotalRequests { get; init; }
    public decimal AverageCostPerRequest { get; init; }
    public decimal MaxCostRequest { get; init; }
    public Dictionary<CostTier, decimal> CostByTier { get; init; } = new();
    public Dictionary<string, decimal> CostByModel { get; init; } = new();
    public DateTimeOffset PeriodStart { get; init; }
    public DateTimeOffset PeriodEnd { get; init; }
}
