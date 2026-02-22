using CognitiveMesh.ReasoningLayer.AnalyticalReasoning.Models;
using Microsoft.Extensions.Logging;

namespace CognitiveMesh.ReasoningLayer.AnalyticalReasoning
{

/// <summary>
/// Port interface for integrating with external data platform endpoints
/// such as Microsoft Fabric OneLake, Data Warehouses, and KQL databases.
/// </summary>
public interface IDataPlatformIntegrationPort
{
    /// <summary>
    /// Connects to data platform endpoints and retrieves contextual data for analysis enrichment.
    /// </summary>
    /// <param name="dataQuery">The data query or context describing what data to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The enrichment data retrieved from the platform endpoints.</returns>
    Task<DataPlatformResult> IntegrateWithDataEndpointsAsync(string dataQuery, CancellationToken cancellationToken = default);

    /// <summary>
    /// Orchestrates data pipelines for data ingestion, transformation, and enrichment.
    /// </summary>
    /// <param name="pipelineContext">Context describing the data transformation requirements.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The result of the pipeline orchestration.</returns>
    Task<DataPlatformResult> OrchestratePipelinesAsync(string pipelineContext, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of a data platform integration or pipeline orchestration operation.
/// </summary>
public class DataPlatformResult
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// A human-readable message describing the outcome.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional enrichment data retrieved from the platform.
    /// </summary>
    public string? EnrichmentData { get; set; }
}

/// <summary>
/// Performs data-driven analytical reasoning by integrating with external data platforms
/// and generating structured analysis results via LLM-based reasoning.
/// </summary>
public class AnalyticalReasoner
{
    private readonly ILogger<AnalyticalReasoner> _logger;
    private readonly AnalysisResultGenerator _analysisResultGenerator;
    private readonly IDataPlatformIntegrationPort? _dataPlatformPort;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticalReasoner"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for structured logging.</param>
    /// <param name="analysisResultGenerator">The generator for producing analysis results via LLM.</param>
    /// <param name="dataPlatformPort">Optional port for data platform integration. When null, platform integration steps are skipped.</param>
    public AnalyticalReasoner(
        ILogger<AnalyticalReasoner> logger,
        AnalysisResultGenerator analysisResultGenerator,
        IDataPlatformIntegrationPort? dataPlatformPort = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _analysisResultGenerator = analysisResultGenerator ?? throw new ArgumentNullException(nameof(analysisResultGenerator));
        _dataPlatformPort = dataPlatformPort;
    }

    /// <summary>
    /// Performs a data-driven analysis by integrating with data platform endpoints,
    /// orchestrating data pipelines, and generating an LLM-based analysis result.
    /// </summary>
    /// <param name="data">The data query or description to analyze.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An <see cref="AnalyticalResult"/> containing the analysis report.</returns>
    public async Task<AnalyticalResult> PerformDataDrivenAnalysisAsync(string data, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting data-driven analysis for data: {Data}", data);

            // Integrate with data platform endpoints if the port is available
            await IntegrateWithFabricDataEndpointsAsync(data, cancellationToken);

            // Orchestrate data pipelines if the port is available
            await OrchestrateDataFactoryPipelinesAsync(data, cancellationToken);

            var result = await _analysisResultGenerator.GenerateAnalysisResultAsync(data);

            _logger.LogInformation("Successfully performed data-driven analysis for data: {Data}", data);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to perform data-driven analysis for data: {Data}", data);
            throw;
        }
    }

    private async Task IntegrateWithFabricDataEndpointsAsync(string data, CancellationToken cancellationToken)
    {
        if (_dataPlatformPort is null)
        {
            _logger.LogDebug("No data platform integration port configured. Skipping Fabric data endpoint integration.");
            return;
        }

        _logger.LogInformation("Integrating with Microsoft Fabric data endpoints...");
        var result = await _dataPlatformPort.IntegrateWithDataEndpointsAsync(data, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully integrated with Microsoft Fabric data endpoints.");
        }
        else
        {
            _logger.LogWarning("Fabric data endpoint integration returned non-success: {Message}", result.Message);
        }
    }

    private async Task OrchestrateDataFactoryPipelinesAsync(string data, CancellationToken cancellationToken)
    {
        if (_dataPlatformPort is null)
        {
            _logger.LogDebug("No data platform integration port configured. Skipping Data Factory pipeline orchestration.");
            return;
        }

        _logger.LogInformation("Orchestrating Data Factory pipelines...");
        var result = await _dataPlatformPort.OrchestratePipelinesAsync(data, cancellationToken);

        if (result.IsSuccess)
        {
            _logger.LogInformation("Successfully orchestrated Data Factory pipelines.");
        }
        else
        {
            _logger.LogWarning("Data Factory pipeline orchestration returned non-success: {Message}", result.Message);
        }
    }
}

} // namespace
