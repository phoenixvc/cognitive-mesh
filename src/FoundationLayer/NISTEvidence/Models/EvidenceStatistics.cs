namespace CognitiveMesh.FoundationLayer.NISTEvidence.Models;

/// <summary>
/// Aggregated statistics for NIST evidence records in the repository.
/// </summary>
/// <param name="TotalRecords">The total number of evidence records in the repository.</param>
/// <param name="PendingReviews">The number of evidence records with pending review status.</param>
/// <param name="ApprovedCount">The number of approved evidence records.</param>
/// <param name="RejectedCount">The number of rejected evidence records.</param>
/// <param name="AverageFileSizeBytes">The average file size in bytes across all evidence records.</param>
/// <param name="ByPillar">A breakdown of evidence record counts by NIST AI RMF pillar.</param>
public sealed record EvidenceStatistics(
    int TotalRecords,
    int PendingReviews,
    int ApprovedCount,
    int RejectedCount,
    long AverageFileSizeBytes,
    Dictionary<string, int> ByPillar);
