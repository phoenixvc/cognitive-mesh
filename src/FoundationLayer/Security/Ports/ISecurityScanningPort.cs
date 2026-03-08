namespace FoundationLayer.Security.Ports;

/// <summary>
/// Represents a security scan request for code or artifact analysis.
/// </summary>
public class SecurityScanRequest
{
    /// <summary>
    /// Unique identifier for the scan request.
    /// </summary>
    public string ScanId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The type of scan to perform (SAST, DAST, Dependency, Container, Secrets).
    /// </summary>
    public required SecurityScanType ScanType { get; init; }

    /// <summary>
    /// The target to scan (file path, URL, container image, etc.).
    /// </summary>
    public required string Target { get; init; }

    /// <summary>
    /// Optional branch or commit reference for version control targets.
    /// </summary>
    public string? GitRef { get; init; }

    /// <summary>
    /// Severity threshold for failing the scan (Critical, High, Medium, Low).
    /// </summary>
    public SecuritySeverity FailThreshold { get; init; } = SecuritySeverity.High;

    /// <summary>
    /// Additional scanner-specific configuration.
    /// </summary>
    public Dictionary<string, string> Configuration { get; init; } = new();
}

/// <summary>
/// Types of security scans supported.
/// </summary>
public enum SecurityScanType
{
    /// <summary>Static Application Security Testing (code analysis).</summary>
    SAST,
    /// <summary>Dynamic Application Security Testing (runtime analysis).</summary>
    DAST,
    /// <summary>Software Composition Analysis (dependency vulnerabilities).</summary>
    DependencyCheck,
    /// <summary>Container image vulnerability scanning.</summary>
    ContainerScan,
    /// <summary>Secrets detection (API keys, passwords, tokens).</summary>
    SecretsDetection,
    /// <summary>Infrastructure as Code scanning (Terraform, ARM, etc.).</summary>
    IaCScanning,
    /// <summary>License compliance checking.</summary>
    LicenseCompliance
}

/// <summary>
/// Severity levels for security findings.
/// </summary>
public enum SecuritySeverity
{
    Info = 0,
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Represents a security vulnerability finding.
/// </summary>
public class SecurityFinding
{
    /// <summary>Unique identifier for the finding.</summary>
    public string FindingId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Severity of the vulnerability.</summary>
    public required SecuritySeverity Severity { get; init; }

    /// <summary>Common Weakness Enumeration identifier (e.g., CWE-79).</summary>
    public string? CweId { get; init; }

    /// <summary>Common Vulnerabilities and Exposures identifier (e.g., CVE-2024-1234).</summary>
    public string? CveId { get; init; }

    /// <summary>Title of the vulnerability.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description of the vulnerability.</summary>
    public required string Description { get; init; }

    /// <summary>Location where the vulnerability was found.</summary>
    public required string Location { get; init; }

    /// <summary>Line number if applicable.</summary>
    public int? LineNumber { get; init; }

    /// <summary>Suggested remediation steps.</summary>
    public string? Remediation { get; init; }

    /// <summary>CVSS score if available (0.0 - 10.0).</summary>
    public double? CvssScore { get; init; }

    /// <summary>Whether this finding is a false positive.</summary>
    public bool IsSuppressed { get; init; }

    /// <summary>Reason for suppression if applicable.</summary>
    public string? SuppressionReason { get; init; }
}

/// <summary>
/// Result of a security scan operation.
/// </summary>
public class SecurityScanResult
{
    /// <summary>The scan request ID this result corresponds to.</summary>
    public required string ScanId { get; init; }

    /// <summary>Whether the scan passed based on the fail threshold.</summary>
    public required bool Passed { get; init; }

    /// <summary>List of security findings.</summary>
    public IReadOnlyList<SecurityFinding> Findings { get; init; } = Array.Empty<SecurityFinding>();

    /// <summary>Summary counts by severity.</summary>
    public Dictionary<SecuritySeverity, int> SeverityCounts { get; init; } = new();

    /// <summary>When the scan started.</summary>
    public DateTimeOffset StartedAt { get; init; }

    /// <summary>When the scan completed.</summary>
    public DateTimeOffset CompletedAt { get; init; }

    /// <summary>Scanner tool and version used.</summary>
    public string? ScannerVersion { get; init; }

    /// <summary>Error message if scan failed to complete.</summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Port for deterministic security scanning integrated into the build loop.
/// Implements the "Deterministic Security Scanning Build Loop" pattern.
/// </summary>
/// <remarks>
/// This port provides security scanning capabilities that can be integrated
/// into CI/CD pipelines for continuous security validation. All scans are
/// deterministic and reproducible given the same inputs.
/// </remarks>
public interface ISecurityScanningPort
{
    /// <summary>
    /// Performs a security scan on the specified target.
    /// </summary>
    /// <param name="request">The scan configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The scan result with findings.</returns>
    Task<SecurityScanResult> ScanAsync(SecurityScanRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs multiple scan types on a target in parallel.
    /// </summary>
    /// <param name="target">The target to scan.</param>
    /// <param name="scanTypes">The types of scans to perform.</param>
    /// <param name="failThreshold">Severity threshold for overall failure.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Aggregated scan results.</returns>
    Task<IReadOnlyList<SecurityScanResult>> ScanMultipleAsync(
        string target,
        IEnumerable<SecurityScanType> scanTypes,
        SecuritySeverity failThreshold = SecuritySeverity.High,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves historical scan results for a target.
    /// </summary>
    /// <param name="target">The target to query.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Historical scan results.</returns>
    Task<IReadOnlyList<SecurityScanResult>> GetScanHistoryAsync(
        string target,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suppresses a finding as a false positive or accepted risk.
    /// </summary>
    /// <param name="findingId">The finding to suppress.</param>
    /// <param name="reason">The reason for suppression.</param>
    /// <param name="approvedBy">Who approved the suppression.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SuppressFindingAsync(
        string findingId,
        string reason,
        string approvedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a build can proceed based on security gate criteria.
    /// </summary>
    /// <param name="buildId">The build identifier.</param>
    /// <param name="requiredScanTypes">Scan types that must pass.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the build can proceed.</returns>
    Task<bool> ValidateBuildSecurityGateAsync(
        string buildId,
        IEnumerable<SecurityScanType> requiredScanTypes,
        CancellationToken cancellationToken = default);
}
