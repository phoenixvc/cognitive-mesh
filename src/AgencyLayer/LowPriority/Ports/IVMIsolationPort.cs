namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - VM/Infrastructure Pattern
// Reason: No VM isolation or RL rollouts in scope
// Reconsideration: If multi-tenant isolation requirements emerge
// ============================================================================

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
/// [LOW PRIORITY] Port for isolated VM per RL rollout.
/// Implements the "Isolated VM per RL Rollout" pattern.
///
/// This is a low-priority pattern because no VM isolation or RL
/// rollouts are in scope for current architecture.
/// </summary>
public interface IVMIsolationPort
{
    /// <summary>Creates isolated VM.</summary>
    Task<VMInstance> CreateVMAsync(string imageId, CancellationToken cancellationToken = default);

    /// <summary>Executes in VM.</summary>
    Task<string> ExecuteAsync(string vmId, string command, CancellationToken cancellationToken = default);

    /// <summary>Destroys VM.</summary>
    Task DestroyVMAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Lists VMs.</summary>
    Task<IReadOnlyList<VMInstance>> ListVMsAsync(CancellationToken cancellationToken = default);
}
