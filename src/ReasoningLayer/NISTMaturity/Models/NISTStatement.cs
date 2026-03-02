namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents a single NIST AI RMF maturity statement that an organization is assessed against.
/// Each statement belongs to a specific topic and pillar within the framework.
/// </summary>
/// <param name="StatementId">Unique identifier for the statement (e.g., "GOV-1.1").</param>
/// <param name="TopicId">Identifier of the topic this statement belongs to.</param>
/// <param name="PillarId">Identifier of the pillar (e.g., "GOV", "MAP", "MEA", "MAN").</param>
/// <param name="Description">Human-readable description of the maturity requirement.</param>
/// <param name="Category">The NIST category this statement falls under.</param>
public sealed record NISTStatement(
    string StatementId,
    string TopicId,
    string PillarId,
    string Description,
    NISTCategory Category);
