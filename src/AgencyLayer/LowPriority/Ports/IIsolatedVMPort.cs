namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - VM/Infrastructure Pattern
// Reason: No VM isolation or RL rollouts in scope
// Reconsideration: If multi-tenant isolation requirements emerge
// ============================================================================

/// <summary>
/// Isolated VM configuration.
/// </summary>
public class VMConfig
{
    /// <summary>VM identifier.</summary>
    public required string VMId { get; init; }

    /// <summary>Image.</summary>
    public required string Image { get; init; }

    /// <summary>CPU cores.</summary>
    public int CpuCores { get; init; } = 2;

    /// <summary>Memory GB.</summary>
    public int MemoryGB { get; init; } = 4;

    /// <summary>Disk GB.</summary>
    public int DiskGB { get; init; } = 20;
}

/// <summary>
/// VM instance.
/// </summary>
public class VMInstance
{
    /// <summary>VM identifier.</summary>
    public required string VMId { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }

    /// <summary>IP address.</summary>
    public string? IPAddress { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for isolated VM management and RL rollouts.
/// Implements the "Isolated VM per RL Rollout" pattern.
///
/// This is a low-priority pattern because no VM isolation or
/// RL rollouts are in scope for current architecture.
/// </summary>
public interface IIsolatedVMPort
{
    /// <summary>Creates an isolated VM.</summary>
    Task<VMInstance> CreateVMAsync(VMConfig config, CancellationToken cancellationToken = default);

    /// <summary>Executes command in VM.</summary>
    Task<string> ExecuteAsync(string vmId, string command, CancellationToken cancellationToken = default);

    /// <summary>Executes rollout in VM.</summary>
    Task<string> ExecuteRolloutAsync(string vmId, string rolloutConfig, CancellationToken cancellationToken = default);

    /// <summary>Gets VM status.</summary>
    Task<string> GetStatusAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Lists VMs.</summary>
    Task<IReadOnlyList<VMInstance>> ListVMsAsync(CancellationToken cancellationToken = default);

    /// <summary>Destroys VM.</summary>
    Task DestroyVMAsync(string vmId, CancellationToken cancellationToken = default);
}
