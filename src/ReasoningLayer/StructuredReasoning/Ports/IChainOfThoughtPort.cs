namespace ReasoningLayer.StructuredReasoning.Ports;

/// <summary>
/// Type of chain-of-thought prompting.
/// </summary>
public enum ChainOfThoughtType
{
    /// <summary>Zero-shot CoT (Let's think step by step).</summary>
    ZeroShot,
    /// <summary>Few-shot CoT with examples.</summary>
    FewShot,
    /// <summary>Self-consistency (multiple paths, vote).</summary>
    SelfConsistency,
    /// <summary>Complex CoT with subquestions.</summary>
    Complex,
    /// <summary>Automatic CoT example generation.</summary>
    AutoCoT
}

/// <summary>
/// A step in the chain of thought.
/// </summary>
public class ThoughtStep
{
    /// <summary>Step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>The thought or reasoning.</summary>
    public required string Thought { get; init; }

    /// <summary>Intermediate conclusion if any.</summary>
    public string? IntermediateConclusion { get; init; }

    /// <summary>Confidence in this step (0.0 - 1.0).</summary>
    public double Confidence { get; init; }
}

/// <summary>
/// An example for few-shot CoT.
/// </summary>
public class ChainOfThoughtExample
{
    /// <summary>The question.</summary>
    public required string Question { get; init; }

    /// <summary>The reasoning steps.</summary>
    public required string Reasoning { get; init; }

    /// <summary>The answer.</summary>
    public required string Answer { get; init; }
}

/// <summary>
/// Configuration for chain-of-thought reasoning.
/// </summary>
public class ChainOfThoughtConfiguration
{
    /// <summary>Type of CoT to use.</summary>
    public ChainOfThoughtType Type { get; init; } = ChainOfThoughtType.ZeroShot;

    /// <summary>Examples for few-shot (if applicable).</summary>
    public IReadOnlyList<ChainOfThoughtExample> Examples { get; init; } = Array.Empty<ChainOfThoughtExample>();

    /// <summary>Number of paths for self-consistency.</summary>
    public int SelfConsistencyPaths { get; init; } = 5;

    /// <summary>Temperature for self-consistency sampling.</summary>
    public double SamplingTemperature { get; init; } = 0.7;

    /// <summary>Prompt prefix for CoT.</summary>
    public string? PromptPrefix { get; init; }

    /// <summary>Model to use.</summary>
    public string? Model { get; init; }

    /// <summary>Whether to extract structured steps.</summary>
    public bool ExtractStructuredSteps { get; init; } = true;
}

/// <summary>
/// Result of chain-of-thought reasoning.
/// </summary>
public class ChainOfThoughtResult
{
    /// <summary>Whether reasoning succeeded.</summary>
    public required bool Success { get; init; }

    /// <summary>The final answer.</summary>
    public string? Answer { get; init; }

    /// <summary>The full reasoning text.</summary>
    public required string FullReasoning { get; init; }

    /// <summary>Extracted steps (if structured).</summary>
    public IReadOnlyList<ThoughtStep> Steps { get; init; } = Array.Empty<ThoughtStep>();

    /// <summary>Confidence in the answer (0.0 - 1.0).</summary>
    public double Confidence { get; init; }

    /// <summary>For self-consistency: all paths tried.</summary>
    public IReadOnlyList<string> AlternativePaths { get; init; } = Array.Empty<string>();

    /// <summary>For self-consistency: votes per answer.</summary>
    public Dictionary<string, int> AnswerVotes { get; init; } = new();

    /// <summary>Reasoning duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for Chain-of-Thought reasoning.
/// Implements the "Chain-of-Thought (CoT) Prompting" pattern.
/// </summary>
/// <remarks>
/// This port implements various chain-of-thought prompting strategies
/// including zero-shot, few-shot, and self-consistency approaches
/// to improve reasoning quality on complex tasks.
/// </remarks>
public interface IChainOfThoughtPort
{
    /// <summary>
    /// Reasons through a question using chain-of-thought.
    /// </summary>
    /// <param name="question">The question to answer.</param>
    /// <param name="configuration">Configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reasoning result.</returns>
    Task<ChainOfThoughtResult> ReasonAsync(
        string question,
        ChainOfThoughtConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a zero-shot chain-of-thought.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="promptPrefix">Custom prompt prefix.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reasoning and answer.</returns>
    Task<ChainOfThoughtResult> ReasonZeroShotAsync(
        string question,
        string? promptPrefix = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a few-shot chain-of-thought.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="examples">Examples to use.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reasoning and answer.</returns>
    Task<ChainOfThoughtResult> ReasonFewShotAsync(
        string question,
        IReadOnlyList<ChainOfThoughtExample> examples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uses self-consistency to improve answer reliability.
    /// </summary>
    /// <param name="question">The question.</param>
    /// <param name="numPaths">Number of reasoning paths.</param>
    /// <param name="temperature">Sampling temperature.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result with voting information.</returns>
    Task<ChainOfThoughtResult> ReasonWithSelfConsistencyAsync(
        string question,
        int numPaths = 5,
        double temperature = 0.7,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates CoT examples automatically for a domain.
    /// </summary>
    /// <param name="domain">Domain description.</param>
    /// <param name="sampleQuestions">Sample questions from the domain.</param>
    /// <param name="numExamples">Number of examples to generate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated examples.</returns>
    Task<IReadOnlyList<ChainOfThoughtExample>> GenerateExamplesAsync(
        string domain,
        IReadOnlyList<string> sampleQuestions,
        int numExamples = 5,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extracts structured steps from reasoning text.
    /// </summary>
    /// <param name="reasoning">The full reasoning text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Extracted steps.</returns>
    Task<IReadOnlyList<ThoughtStep>> ExtractStepsAsync(
        string reasoning,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a chain-of-thought for logical consistency.
    /// </summary>
    /// <param name="steps">The reasoning steps.</param>
    /// <param name="answer">The final answer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Whether consistent and any issues.</returns>
    Task<(bool IsConsistent, IReadOnlyList<string> Issues)> VerifyReasoningAsync(
        IReadOnlyList<ThoughtStep> steps,
        string answer,
        CancellationToken cancellationToken = default);
}
