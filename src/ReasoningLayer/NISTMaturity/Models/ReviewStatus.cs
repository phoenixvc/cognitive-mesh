namespace CognitiveMesh.ReasoningLayer.NISTMaturity.Models;

/// <summary>
/// Represents the review status of submitted evidence in the NIST maturity assessment process.
/// </summary>
public enum ReviewStatus
{
    /// <summary>Evidence has been submitted but not yet reviewed.</summary>
    Pending,

    /// <summary>Evidence has been reviewed and approved.</summary>
    Approved,

    /// <summary>Evidence has been reviewed and rejected.</summary>
    Rejected,

    /// <summary>Evidence requires revision before it can be approved.</summary>
    NeedsRevision
}
