using CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Ports;

/// <summary>
/// Defines the contract for the reflexion engine that detects hallucinations
/// and contradictions in agent outputs. Implements a self-evaluation pattern
/// to improve output quality and reliability.
/// </summary>
public interface IReflexionPort
{
    /// <summary>
    /// Evaluates an agent's output against the original input for hallucinations and contradictions.
    /// Uses keyword analysis, length ratio checks, and known hallucination pattern detection.
    /// </summary>
    /// <param name="inputText">The original input or prompt text.</param>
    /// <param name="agentOutput">The agent's generated output to evaluate.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The evaluation result including hallucination classification and contradictions.</returns>
    Task<ReflexionResult> EvaluateAsync(string inputText, string agentOutput, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the most recent reflexion evaluation results.
    /// </summary>
    /// <param name="count">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A read-only list of recent reflexion results, ordered by most recent first.</returns>
    Task<IReadOnlyList<ReflexionResult>> GetRecentResultsAsync(int count, CancellationToken cancellationToken);
}
