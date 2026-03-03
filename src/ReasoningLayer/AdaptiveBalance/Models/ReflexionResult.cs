namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents the result of a reflexion evaluation that checks for hallucinations
/// and contradictions between an input prompt and an agent's output.
/// </summary>
/// <param name="ResultId">Unique identifier for this evaluation result.</param>
/// <param name="InputText">The original input text that was evaluated.</param>
/// <param name="IsHallucination">Whether the output was classified as a hallucination.</param>
/// <param name="Confidence">Confidence level of the evaluation (0.0 to 1.0).</param>
/// <param name="Contradictions">List of detected contradictions between input and output.</param>
/// <param name="EvaluationDurationMs">Duration of the evaluation in milliseconds.</param>
/// <param name="EvaluatedAt">Timestamp when the evaluation was performed.</param>
public sealed record ReflexionResult(
    Guid ResultId,
    string InputText,
    bool IsHallucination,
    double Confidence,
    List<string> Contradictions,
    long EvaluationDurationMs,
    DateTimeOffset EvaluatedAt);
