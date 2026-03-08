namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// A map-reduce job definition.
/// </summary>
public class MapReduceJob
{
    /// <summary>Job identifier.</summary>
    public string JobId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Job name.</summary>
    public required string Name { get; init; }

    /// <summary>Input items to process.</summary>
    public required IReadOnlyList<string> Inputs { get; init; }

    /// <summary>Map prompt template.</summary>
    public required string MapPrompt { get; init; }

    /// <summary>Reduce prompt template.</summary>
    public required string ReducePrompt { get; init; }

    /// <summary>Maximum parallel map operations.</summary>
    public int MaxParallelism { get; init; } = 10;

    /// <summary>Chunk size for batching.</summary>
    public int ChunkSize { get; init; } = 5;

    /// <summary>Timeout per map operation.</summary>
    public TimeSpan MapTimeout { get; init; } = TimeSpan.FromMinutes(2);

    /// <summary>Model to use.</summary>
    public string? ModelId { get; init; }

    /// <summary>Additional context.</summary>
    public string? Context { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Result of a map operation.
/// </summary>
public class MapResult
{
    /// <summary>Input that was processed.</summary>
    public required string Input { get; init; }

    /// <summary>Output from map.</summary>
    public required string Output { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Error if failed.</summary>
    public string? Error { get; init; }

    /// <summary>Execution time.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Result of a map-reduce job.
/// </summary>
public class MapReduceResult
{
    /// <summary>Job ID.</summary>
    public required string JobId { get; init; }

    /// <summary>Final reduced output.</summary>
    public required string Output { get; init; }

    /// <summary>Individual map results.</summary>
    public IReadOnlyList<MapResult> MapResults { get; init; } = Array.Empty<MapResult>();

    /// <summary>Total inputs processed.</summary>
    public int TotalInputs { get; init; }

    /// <summary>Successful maps.</summary>
    public int SuccessfulMaps { get; init; }

    /// <summary>Failed maps.</summary>
    public int FailedMaps { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan TotalDuration { get; init; }

    /// <summary>Whether overall job succeeded.</summary>
    public bool Success { get; init; }
}

/// <summary>
/// Port for LLM Map-Reduce pattern.
/// Implements the "LLM Map-Reduce" pattern for large-scale parallel processing.
/// </summary>
public interface IMapReducePort
{
    /// <summary>
    /// Executes a map-reduce job.
    /// </summary>
    Task<MapReduceResult> ExecuteAsync(
        MapReduceJob job,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes only the map phase.
    /// </summary>
    Task<IReadOnlyList<MapResult>> MapAsync(
        IEnumerable<string> inputs,
        string mapPrompt,
        int maxParallelism = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes only the reduce phase.
    /// </summary>
    Task<string> ReduceAsync(
        IEnumerable<string> mappedOutputs,
        string reducePrompt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets job status.
    /// </summary>
    Task<MapReduceJobStatus> GetJobStatusAsync(
        string jobId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a running job.
    /// </summary>
    Task CancelJobAsync(
        string jobId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Status of a map-reduce job.
/// </summary>
public class MapReduceJobStatus
{
    public required string JobId { get; init; }
    public required string Status { get; init; }
    public int TotalInputs { get; init; }
    public int ProcessedInputs { get; init; }
    public int FailedInputs { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
}
