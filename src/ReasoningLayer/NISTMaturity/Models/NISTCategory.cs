namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents the four pillars of the NIST AI Risk Management Framework.
/// Each category defines a distinct area of organizational AI governance.
/// </summary>
public enum NISTCategory
{
    /// <summary>Governance pillar: policies, accountability, and organizational culture.</summary>
    Govern,

    /// <summary>Map pillar: context and risk identification for AI systems.</summary>
    Map,

    /// <summary>Measure pillar: metrics, monitoring, and assessment of AI risks.</summary>
    Measure,

    /// <summary>Manage pillar: risk treatment, response, and recovery.</summary>
    Manage
}
