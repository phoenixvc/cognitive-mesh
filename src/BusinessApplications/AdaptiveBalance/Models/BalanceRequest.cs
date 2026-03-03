namespace CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;

/// <summary>
/// Represents a request to retrieve the current adaptive balance positions
/// for all spectrum dimensions.
/// </summary>
public class BalanceRequest
{
    /// <summary>
    /// Contextual information that may influence the balance calculation,
    /// provided as key-value pairs.
    /// </summary>
    public Dictionary<string, string> Context { get; set; } = new();
}
