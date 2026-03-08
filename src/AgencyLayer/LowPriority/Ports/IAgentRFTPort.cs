namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - RL/Fine-Tuning Pattern
// Reason: No RL training loop; fine-tuning not planned
// Reconsideration: If organization builds RL training infrastructure
// ============================================================================

/// <summary>
/// RFT training configuration.
/// </summary>
public class RFTConfig
{
    /// <summary>Model to fine-tune.</summary>
    public required string BaseModelId { get; init; }

    /// <summary>Training data path.</summary>
    public required string TrainingDataPath { get; init; }

    /// <summary>Learning rate.</summary>
    public double LearningRate { get; init; } = 1e-5;

    /// <summary>Epochs.</summary>
    public int Epochs { get; init; } = 3;

    /// <summary>Batch size.</summary>
    public int BatchSize { get; init; } = 8;
}

/// <summary>
/// [LOW PRIORITY] Port for Agent Reinforcement Fine-Tuning.
/// Implements the "Agent Reinforcement Fine-Tuning (Agent RFT)" pattern.
///
/// This is a low-priority pattern because no RL training loop exists
/// and fine-tuning is not currently planned.
/// </summary>
public interface IAgentRFTPort
{
    /// <summary>Starts fine-tuning.</summary>
    Task<string> StartFineTuningAsync(RFTConfig config, CancellationToken cancellationToken = default);

    /// <summary>Gets training status.</summary>
    Task<(string Status, double Progress)> GetStatusAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Cancels training.</summary>
    Task CancelAsync(string jobId, CancellationToken cancellationToken = default);

    /// <summary>Gets fine-tuned model.</summary>
    Task<string?> GetModelAsync(string jobId, CancellationToken cancellationToken = default);
}
