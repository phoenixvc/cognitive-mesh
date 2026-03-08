namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// A sandbox instance.
/// </summary>
public class SandboxInstance
{
    /// <summary>Sandbox identifier.</summary>
    public required string SandboxId { get; init; }

    /// <summary>Sandbox type.</summary>
    public SandboxType Type { get; init; }

    /// <summary>Status.</summary>
    public SandboxStatus Status { get; init; }

    /// <summary>Resource limits.</summary>
    public SandboxLimits Limits { get; init; } = new();

    /// <summary>Environment variables.</summary>
    public Dictionary<string, string> Environment { get; init; } = new();

    /// <summary>Working directory.</summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Expires at.</summary>
    public DateTimeOffset ExpiresAt { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Sandbox type.
/// </summary>
public enum SandboxType
{
    /// <summary>Container sandbox.</summary>
    Container,
    /// <summary>VM sandbox.</summary>
    VM,
    /// <summary>Process sandbox.</summary>
    Process,
    /// <summary>WebAssembly sandbox.</summary>
    Wasm
}

/// <summary>
/// Sandbox status.
/// </summary>
public enum SandboxStatus
{
    Creating,
    Ready,
    Running,
    Completed,
    Failed,
    Terminated,
    Expired
}

/// <summary>
/// Sandbox resource limits.
/// </summary>
public class SandboxLimits
{
    /// <summary>CPU cores.</summary>
    public double CpuCores { get; init; } = 1.0;

    /// <summary>Memory in MB.</summary>
    public int MemoryMB { get; init; } = 512;

    /// <summary>Disk in MB.</summary>
    public int DiskMB { get; init; } = 1024;

    /// <summary>Network enabled.</summary>
    public bool NetworkEnabled { get; init; }

    /// <summary>Execution timeout.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromMinutes(5);

    /// <summary>Maximum processes.</summary>
    public int MaxProcesses { get; init; } = 50;
}

/// <summary>
/// Fan-out configuration.
/// </summary>
public class FanOutConfiguration
{
    /// <summary>Number of sandboxes.</summary>
    public int SandboxCount { get; init; } = 5;

    /// <summary>Sandbox type.</summary>
    public SandboxType Type { get; init; } = SandboxType.Container;

    /// <summary>Limits per sandbox.</summary>
    public SandboxLimits Limits { get; init; } = new();

    /// <summary>Adaptive scaling.</summary>
    public bool AdaptiveScaling { get; init; } = true;

    /// <summary>Maximum sandboxes (for adaptive).</summary>
    public int MaxSandboxes { get; init; } = 20;

    /// <summary>Minimum sandboxes (for adaptive).</summary>
    public int MinSandboxes { get; init; } = 1;
}

/// <summary>
/// Execution request for sandboxes.
/// </summary>
public class SandboxExecutionRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Tasks to execute.</summary>
    public IReadOnlyList<SandboxTask> Tasks { get; init; } = Array.Empty<SandboxTask>();

    /// <summary>Configuration.</summary>
    public FanOutConfiguration? Configuration { get; init; }

    /// <summary>Whether to wait for all.</summary>
    public bool WaitForAll { get; init; } = true;

    /// <summary>Strategy for distribution.</summary>
    public DistributionStrategy Strategy { get; init; } = DistributionStrategy.RoundRobin;
}

/// <summary>
/// A task for sandbox execution.
/// </summary>
public class SandboxTask
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Command to execute.</summary>
    public required string Command { get; init; }

    /// <summary>Arguments.</summary>
    public IReadOnlyList<string> Arguments { get; init; } = Array.Empty<string>();

    /// <summary>Input data.</summary>
    public string? Input { get; init; }

    /// <summary>Timeout.</summary>
    public TimeSpan? Timeout { get; init; }
}

/// <summary>
/// Distribution strategy.
/// </summary>
public enum DistributionStrategy
{
    RoundRobin,
    LeastLoaded,
    Random,
    Sticky
}

/// <summary>
/// Sandbox execution result.
/// </summary>
public class SandboxExecutionResult
{
    /// <summary>Request identifier.</summary>
    public required string RequestId { get; init; }

    /// <summary>Task results.</summary>
    public IReadOnlyList<SandboxTaskResult> TaskResults { get; init; } = Array.Empty<SandboxTaskResult>();

    /// <summary>Sandboxes used.</summary>
    public int SandboxesUsed { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan TotalDuration { get; init; }
}

/// <summary>
/// Task result from sandbox.
/// </summary>
public class SandboxTaskResult
{
    /// <summary>Task identifier.</summary>
    public required string TaskId { get; init; }

    /// <summary>Sandbox identifier.</summary>
    public required string SandboxId { get; init; }

    /// <summary>Exit code.</summary>
    public int ExitCode { get; init; }

    /// <summary>Standard output.</summary>
    public string? Stdout { get; init; }

    /// <summary>Standard error.</summary>
    public string? Stderr { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success => ExitCode == 0;

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Port for adaptive sandbox fan-out controller.
/// Implements the "Adaptive Sandbox Fan-Out Controller" pattern.
/// </summary>
public interface ISandboxFanOutPort
{
    /// <summary>
    /// Executes tasks across sandboxes.
    /// </summary>
    Task<SandboxExecutionResult> ExecuteAsync(
        SandboxExecutionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a sandbox.
    /// </summary>
    Task<SandboxInstance> CreateSandboxAsync(
        SandboxType type,
        SandboxLimits? limits = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminates a sandbox.
    /// </summary>
    Task TerminateSandboxAsync(
        string sandboxId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists active sandboxes.
    /// </summary>
    Task<IReadOnlyList<SandboxInstance>> ListSandboxesAsync(
        SandboxStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes in a specific sandbox.
    /// </summary>
    Task<SandboxTaskResult> ExecuteInSandboxAsync(
        string sandboxId,
        SandboxTask task,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Scales sandbox pool.
    /// </summary>
    Task ScalePoolAsync(
        int targetCount,
        SandboxType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pool metrics.
    /// </summary>
    Task<SandboxPoolMetrics> GetMetricsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Sandbox pool metrics.
/// </summary>
public class SandboxPoolMetrics
{
    public int TotalSandboxes { get; init; }
    public int ActiveSandboxes { get; init; }
    public int IdleSandboxes { get; init; }
    public double AverageUtilization { get; init; }
    public int TasksExecuted { get; init; }
    public int TasksFailed { get; init; }
}
