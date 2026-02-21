namespace CognitiveMesh.ReasoningLayer.AdaptiveBalance.Models;

/// <summary>
/// Represents a learning event captured from agent interactions or system operations.
/// Events are classified by pattern type and outcome to support continuous improvement.
/// </summary>
/// <param name="EventId">Unique identifier for this learning event.</param>
/// <param name="PatternType">Classification of the pattern observed (e.g., "reasoning_error", "timeout").</param>
/// <param name="Description">Human-readable description of the event.</param>
/// <param name="Evidence">Supporting evidence or data for the observed pattern.</param>
/// <param name="Outcome">The outcome classification of the event.</param>
/// <param name="RecordedAt">Timestamp when the event was recorded.</param>
/// <param name="SourceAgentId">Identifier of the agent that produced the event.</param>
public sealed record LearningEvent(
    Guid EventId,
    string PatternType,
    string Description,
    string Evidence,
    LearningOutcome Outcome,
    DateTimeOffset RecordedAt,
    string SourceAgentId);
