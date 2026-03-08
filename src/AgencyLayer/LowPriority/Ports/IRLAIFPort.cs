namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No RLAIF pipeline infrastructure
// Reconsideration: If RL training infrastructure is built
// ============================================================================

/// <summary>
/// AI feedback for RLAIF.
/// </summary>
public class AIFeedback
{
    /// <summary>Response identifier.</summary>
    public required string ResponseId { get; init; }

    /// <summary>Response content.</summary>
    public required string Response { get; init; }

    /// <summary>AI-generated score.</summary>
    public double Score { get; init; }

    /// <summary>Feedback rationale.</summary>
    public string? Rationale { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for RLAIF (RL from AI Feedback).
/// Implements the "RLAIF (RL from AI Feedback)" pattern.
///
/// This is a low-priority pattern because no RLAIF pipeline
/// infrastructure exists.
/// </summary>
public interface IRLAIFPort
{
    /// <summary>Generates AI feedback for response.</summary>
    Task<AIFeedback> GenerateFeedbackAsync(string response, string prompt, CancellationToken cancellationToken = default);

    /// <summary>Collects feedback batch.</summary>
    Task<IReadOnlyList<AIFeedback>> CollectFeedbackBatchAsync(IEnumerable<(string Response, string Prompt)> items, CancellationToken cancellationToken = default);

    /// <summary>Trains reward model from AI feedback.</summary>
    Task TrainRewardModelAsync(IEnumerable<AIFeedback> feedbackData, CancellationToken cancellationToken = default);
}
