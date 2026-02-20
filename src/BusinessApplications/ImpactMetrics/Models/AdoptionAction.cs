namespace CognitiveMesh.BusinessApplications.ImpactMetrics.Models;

/// <summary>
/// Enumerates the types of user actions tracked by the adoption telemetry system.
/// </summary>
public enum AdoptionAction
{
    /// <summary>
    /// User logged in to an AI-enabled tool.
    /// </summary>
    Login,

    /// <summary>
    /// User actively used a feature of the AI tool.
    /// </summary>
    FeatureUse,

    /// <summary>
    /// User was presented with a feature but chose not to use it.
    /// </summary>
    FeatureIgnore,

    /// <summary>
    /// User provided feedback on an AI feature or decision.
    /// </summary>
    Feedback,

    /// <summary>
    /// User overrode an AI-generated recommendation or decision.
    /// </summary>
    Override,

    /// <summary>
    /// User requested help or support while using an AI tool.
    /// </summary>
    HelpRequest,

    /// <summary>
    /// User completed a full workflow involving AI assistance.
    /// </summary>
    WorkflowComplete
}
