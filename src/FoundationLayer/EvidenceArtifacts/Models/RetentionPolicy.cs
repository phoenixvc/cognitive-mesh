namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;

/// <summary>
/// Defines the data retention policy for an evidence artifact,
/// controlling how long the artifact is retained before expiration.
/// </summary>
public enum RetentionPolicy
{
    /// <summary>
    /// The artifact is retained indefinitely with no expiration.
    /// </summary>
    Indefinite,

    /// <summary>
    /// The artifact is retained for one year from submission.
    /// </summary>
    OneYear,

    /// <summary>
    /// The artifact is retained for three years from submission.
    /// </summary>
    ThreeYears,

    /// <summary>
    /// The artifact is retained for five years from submission.
    /// </summary>
    FiveYears,

    /// <summary>
    /// The artifact follows GDPR-compliant retention rules,
    /// typically requiring deletion when no longer necessary for processing purposes.
    /// </summary>
    GDPR_Compliant
}
