namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// A feature contract.
/// </summary>
public class FeatureContract
{
    /// <summary>Contract identifier.</summary>
    public required string ContractId { get; init; }

    /// <summary>Feature name.</summary>
    public required string FeatureName { get; init; }

    /// <summary>Version.</summary>
    public required string Version { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Contract type.</summary>
    public FeatureContractType Type { get; init; }

    /// <summary>Feature flags.</summary>
    public Dictionary<string, bool> Flags { get; init; } = new();

    /// <summary>Configuration values.</summary>
    public Dictionary<string, string> Configuration { get; init; } = new();

    /// <summary>Dependencies.</summary>
    public IReadOnlyList<string> Dependencies { get; init; } = Array.Empty<string>();

    /// <summary>Constraints.</summary>
    public IReadOnlyList<FeatureConstraint> Constraints { get; init; } = Array.Empty<FeatureConstraint>();

    /// <summary>Whether immutable (deployed).</summary>
    public bool IsImmutable { get; init; }

    /// <summary>Deployed at.</summary>
    public DateTimeOffset? DeployedAt { get; init; }

    /// <summary>Created at.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>Created by.</summary>
    public required string CreatedBy { get; init; }

    /// <summary>Metadata.</summary>
    public Dictionary<string, string> Metadata { get; init; } = new();
}

/// <summary>
/// Feature contract type.
/// </summary>
public enum FeatureContractType
{
    /// <summary>API contract.</summary>
    API,
    /// <summary>UI feature.</summary>
    UI,
    /// <summary>Backend feature.</summary>
    Backend,
    /// <summary>Integration.</summary>
    Integration,
    /// <summary>Configuration.</summary>
    Configuration
}

/// <summary>
/// A feature constraint.
/// </summary>
public class FeatureConstraint
{
    /// <summary>Constraint name.</summary>
    public required string Name { get; init; }

    /// <summary>Constraint type.</summary>
    public required string Type { get; init; }

    /// <summary>Value.</summary>
    public required string Value { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Whether required.</summary>
    public bool Required { get; init; } = true;
}

/// <summary>
/// Contract validation result.
/// </summary>
public class ContractValidationResult
{
    /// <summary>Contract identifier.</summary>
    public required string ContractId { get; init; }

    /// <summary>Whether valid.</summary>
    public bool IsValid { get; init; }

    /// <summary>Violations.</summary>
    public IReadOnlyList<ContractViolation> Violations { get; init; } = Array.Empty<ContractViolation>();

    /// <summary>Warnings.</summary>
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}

/// <summary>
/// A contract violation.
/// </summary>
public class ContractViolation
{
    /// <summary>Violation type.</summary>
    public required string Type { get; init; }

    /// <summary>Description.</summary>
    public required string Description { get; init; }

    /// <summary>Severity.</summary>
    public required string Severity { get; init; }

    /// <summary>Field affected.</summary>
    public string? Field { get; init; }
}

/// <summary>
/// Port for feature list as immutable contract.
/// Implements the "Feature List as Immutable Contract" pattern.
/// </summary>
public interface IFeatureContractPort
{
    /// <summary>
    /// Creates a feature contract.
    /// </summary>
    Task<FeatureContract> CreateContractAsync(
        FeatureContract contract,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a feature contract.
    /// </summary>
    Task<FeatureContract?> GetContractAsync(
        string featureName,
        string? version = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists feature contracts.
    /// </summary>
    Task<IReadOnlyList<FeatureContract>> ListContractsAsync(
        FeatureContractType? type = null,
        bool immutableOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a contract.
    /// </summary>
    Task<ContractValidationResult> ValidateContractAsync(
        FeatureContract contract,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deploys a contract (makes it immutable).
    /// </summary>
    Task<FeatureContract> DeployContractAsync(
        string contractId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if current state matches contract.
    /// </summary>
    Task<ContractValidationResult> CheckComplianceAsync(
        string contractId,
        Dictionary<string, string> currentState,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets contract history.
    /// </summary>
    Task<IReadOnlyList<FeatureContract>> GetHistoryAsync(
        string featureName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two contracts.
    /// </summary>
    Task<string> CompareContractsAsync(
        string contractId1,
        string contractId2,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new version from existing.
    /// </summary>
    Task<FeatureContract> CreateVersionAsync(
        string contractId,
        string newVersion,
        Dictionary<string, string>? changes = null,
        CancellationToken cancellationToken = default);
}
