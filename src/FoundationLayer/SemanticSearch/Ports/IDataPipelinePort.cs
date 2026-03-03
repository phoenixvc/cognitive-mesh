namespace FoundationLayer.SemanticSearch.Ports;

/// <summary>
/// Defines the contract for connecting to and managing external data pipeline services.
/// This port abstracts interactions with Microsoft Fabric data endpoints and
/// Azure Data Factory pipelines, allowing the RAG system to trigger data ingestion,
/// transformation, and indexing workflows without coupling to specific SDKs.
/// </summary>
/// <remarks>
/// <para>
/// Implementations should apply the Foundation layer's circuit breaker pattern
/// (Closed, Open, HalfOpen with 3-failure threshold) for all external service calls.
/// </para>
/// <para>
/// Two primary integration points are exposed:
/// <list type="bullet">
///   <item>
///     <description>
///       <see cref="ConnectToFabricEndpointsAsync"/> — establishes or validates connectivity
///       with Fabric data endpoints for real-time data synchronization.
///     </description>
///   </item>
///   <item>
///     <description>
///       <see cref="TriggerDataFactoryPipelineAsync"/> — initiates Data Factory pipelines
///       for batch ETL operations that feed the RAG index.
///     </description>
///   </item>
/// </list>
/// </para>
/// </remarks>
public interface IDataPipelinePort
{
    /// <summary>
    /// Establishes or validates connectivity with Microsoft Fabric data endpoints.
    /// This includes verifying authentication, endpoint availability, and data format compatibility.
    /// </summary>
    /// <param name="configuration">Configuration specifying the Fabric endpoints and authentication details.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PipelineConnectionResult"/> indicating whether the connection
    /// was successfully established, along with endpoint metadata.
    /// </returns>
    Task<PipelineConnectionResult> ConnectToFabricEndpointsAsync(
        FabricEndpointConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Triggers an Azure Data Factory pipeline run for batch data ingestion or transformation.
    /// The pipeline typically extracts data from source systems, transforms it, and loads it
    /// into the search index or vector store.
    /// </summary>
    /// <param name="request">The pipeline execution request specifying which pipeline to run and its parameters.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PipelineRunResult"/> containing the run identifier and initial status.
    /// </returns>
    Task<PipelineRunResult> TriggerDataFactoryPipelineAsync(
        DataFactoryPipelineRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks the current status of a previously triggered pipeline run.
    /// </summary>
    /// <param name="pipelineRunId">The unique identifier of the pipeline run to check.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>
    /// A <see cref="PipelineRunResult"/> with the current execution status.
    /// </returns>
    Task<PipelineRunResult> GetPipelineRunStatusAsync(
        string pipelineRunId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Configuration for connecting to Microsoft Fabric data endpoints.
/// </summary>
public class FabricEndpointConfiguration
{
    /// <summary>
    /// The workspace identifier in Microsoft Fabric.
    /// </summary>
    public string WorkspaceId { get; init; } = string.Empty;

    /// <summary>
    /// The endpoint URL for the Fabric data service.
    /// </summary>
    public string EndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// The lakehouse or warehouse name to connect to.
    /// </summary>
    public string DataStoreName { get; init; } = string.Empty;
}

/// <summary>
/// Result of a Fabric endpoint connection attempt.
/// </summary>
public class PipelineConnectionResult
{
    /// <summary>
    /// Indicates whether the connection was successfully established.
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// The resolved endpoint URL, which may differ from the configured URL after redirect resolution.
    /// </summary>
    public string? ResolvedEndpointUrl { get; init; }

    /// <summary>
    /// An error message if the connection failed; null on success.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Request to trigger an Azure Data Factory pipeline.
/// </summary>
public class DataFactoryPipelineRequest
{
    /// <summary>
    /// The name of the Data Factory pipeline to execute.
    /// </summary>
    public string PipelineName { get; init; } = string.Empty;

    /// <summary>
    /// The resource group containing the Data Factory instance.
    /// </summary>
    public string ResourceGroup { get; init; } = string.Empty;

    /// <summary>
    /// The name of the Data Factory instance.
    /// </summary>
    public string FactoryName { get; init; } = string.Empty;

    /// <summary>
    /// Optional parameters to pass to the pipeline run.
    /// </summary>
    public IDictionary<string, string>? Parameters { get; init; }
}

/// <summary>
/// Result of a Data Factory pipeline run trigger or status check.
/// </summary>
public class PipelineRunResult
{
    /// <summary>
    /// The unique identifier for the pipeline run.
    /// </summary>
    public string RunId { get; init; } = string.Empty;

    /// <summary>
    /// The current status of the pipeline run.
    /// </summary>
    public PipelineRunStatus Status { get; init; }

    /// <summary>
    /// An error message if the pipeline failed; null when not in a failed state.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Represents the possible states of a data pipeline run.
/// </summary>
public enum PipelineRunStatus
{
    /// <summary>The run status is unknown or could not be determined.</summary>
    Unknown,

    /// <summary>The pipeline run has been queued but not yet started.</summary>
    Queued,

    /// <summary>The pipeline run is currently executing.</summary>
    InProgress,

    /// <summary>The pipeline run completed successfully.</summary>
    Succeeded,

    /// <summary>The pipeline run failed.</summary>
    Failed,

    /// <summary>The pipeline run was cancelled.</summary>
    Cancelled
}
