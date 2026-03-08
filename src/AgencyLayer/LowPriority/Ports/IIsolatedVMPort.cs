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
/// [LOW PRIORITY] Port for isolated VM per RL rollout.
/// Implements the "Isolated VM per RL Rollout" pattern.
///
/// This is a low-priority pattern because no VM isolation or
/// RL rollouts are in scope.
/// </summary>
public interface IIsolatedVMPort
{
    /// <summary>Creates an isolated VM.</summary>
    Task<string> CreateVMAsync(VMConfig config, CancellationToken cancellationToken = default);

    /// <summary>Executes rollout in VM.</summary>
    Task<string> ExecuteRolloutAsync(string vmId, string rolloutConfig, CancellationToken cancellationToken = default);

    /// <summary>Gets VM status.</summary>
    Task<string> GetStatusAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Destroys VM.</summary>
    Task DestroyVMAsync(string vmId, CancellationToken cancellationToken = default);
}
