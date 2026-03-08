namespace AgencyLayer.LowPriority.Ports;

// ============================================================================
// LOW PRIORITY - VM/Infrastructure Pattern
// Reason: No VM operations in scope; also carries antipattern risk
// Reconsideration: If untrusted workloads need sandboxing
// Related Antipattern: Virtual Machine Operator Agent (Low Risk)
// ============================================================================

/// <summary>
/// VM operation.
/// </summary>
public class VMOperation
{
    /// <summary>Operation identifier.</summary>
    public required string OperationId { get; init; }

    /// <summary>Operation type.</summary>
    public required string Type { get; init; }

    /// <summary>Target VM.</summary>
    public required string VMId { get; init; }

    /// <summary>Status.</summary>
    public required string Status { get; init; }
}

/// <summary>
/// [LOW PRIORITY] Port for virtual machine operator agent.
/// Implements the "Virtual Machine Operator Agent" pattern.
///
/// This is a low-priority pattern because no VM operations are in
/// scope and it carries antipattern risk.
/// </summary>
public interface IVMOperatorPort
{
    /// <summary>Starts VM.</summary>
    Task<VMOperation> StartVMAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Stops VM.</summary>
    Task<VMOperation> StopVMAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Restarts VM.</summary>
    Task<VMOperation> RestartVMAsync(string vmId, CancellationToken cancellationToken = default);

    /// <summary>Gets operation status.</summary>
    Task<VMOperation> GetOperationAsync(string operationId, CancellationToken cancellationToken = default);
}
