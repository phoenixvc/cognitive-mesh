namespace CognitiveMesh.BusinessApplications.ConvenerServices.Ports;

/// <summary>
/// Port interface for tracking how innovations spread through an organization.
/// Implements the Innovation Spread Engine concept from the Convener PRD:
/// detect, log, and propagate innovations; track adoption lineage.
/// </summary>
public interface IInnovationSpreadPort
{
    /// <summary>
    /// Gets the spread metrics and adoption lineage for a specific idea.
    /// </summary>
    /// <param name="ideaId">The unique identifier of the idea to track.</param>
    /// <param name="tenantId">Tenant scope for data isolation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The innovation spread analysis for the idea.</returns>
    Task<InnovationSpreadResult> GetInnovationSpreadAsync(
        string ideaId, string tenantId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an innovation spread analysis.
/// </summary>
public class InnovationSpreadResult
{
    /// <summary>The idea being tracked.</summary>
    public string IdeaId { get; set; } = string.Empty;

    /// <summary>Original author/proposer of the idea.</summary>
    public string OriginatorUserId { get; set; } = string.Empty;

    /// <summary>When the idea was first proposed.</summary>
    public DateTimeOffset ProposedAt { get; set; }

    /// <summary>Number of teams/individuals who have adopted this idea.</summary>
    public int AdoptionCount { get; set; }

    /// <summary>Adoption rate as a percentage of the organization.</summary>
    public double AdoptionRatePercent { get; set; }

    /// <summary>Virality score indicating how quickly the idea is spreading (0.0–1.0).</summary>
    public double ViralityScore { get; set; }

    /// <summary>Adoption lineage — ordered list of adoption events.</summary>
    public List<AdoptionEvent> AdoptionLineage { get; set; } = new();

    /// <summary>Current spread phase (Seed, Growth, Maturity, Saturation).</summary>
    public SpreadPhase Phase { get; set; }
}

/// <summary>
/// A single adoption event in the innovation spread lineage.
/// </summary>
public class AdoptionEvent
{
    /// <summary>User or team ID that adopted the idea.</summary>
    public string AdopterUserId { get; set; } = string.Empty;

    /// <summary>When the adoption occurred.</summary>
    public DateTimeOffset AdoptedAt { get; set; }

    /// <summary>Who introduced/referred the idea to this adopter.</summary>
    public string? ReferredByUserId { get; set; }

    /// <summary>Context of adoption (e.g., "sprint planning", "tech talk").</summary>
    public string? AdoptionContext { get; set; }
}

/// <summary>
/// Innovation diffusion phases based on Rogers' Diffusion of Innovations.
/// </summary>
public enum SpreadPhase
{
    /// <summary>Initial proposal, few adopters.</summary>
    Seed,
    /// <summary>Accelerating adoption.</summary>
    Growth,
    /// <summary>Broad adoption, rate slowing.</summary>
    Maturity,
    /// <summary>Most eligible parties have adopted.</summary>
    Saturation
}
