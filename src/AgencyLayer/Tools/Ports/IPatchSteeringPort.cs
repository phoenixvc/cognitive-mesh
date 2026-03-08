namespace AgencyLayer.Tools.Ports;

/// <summary>
/// A patch candidate.
/// </summary>
public class PatchCandidate
{
    /// <summary>Candidate identifier.</summary>
    public string CandidateId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Original content.</summary>
    public required string OriginalContent { get; init; }

    /// <summary>Patched content.</summary>
    public required string PatchedContent { get; init; }

    /// <summary>Diff.</summary>
    public required string Diff { get; init; }

    /// <summary>Tool used to generate.</summary>
    public required string GeneratingTool { get; init; }

    /// <summary>Confidence.</summary>
    public double Confidence { get; init; }

    /// <summary>Reasoning.</summary>
    public string? Reasoning { get; init; }

    /// <summary>Lines added.</summary>
    public int LinesAdded { get; init; }

    /// <summary>Lines removed.</summary>
    public int LinesRemoved { get; init; }

    /// <summary>Generated at.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>
/// Patch steering configuration.
/// </summary>
public class PatchSteeringConfiguration
{
    /// <summary>Number of candidates to generate.</summary>
    public int CandidateCount { get; init; } = 3;

    /// <summary>Tools to use for generation.</summary>
    public IReadOnlyList<string> PreferredTools { get; init; } = Array.Empty<string>();

    /// <summary>Minimum confidence threshold.</summary>
    public double MinConfidence { get; init; } = 0.7;

    /// <summary>Maximum lines changed.</summary>
    public int? MaxLinesChanged { get; init; }

    /// <summary>Style constraints.</summary>
    public IReadOnlyList<string> StyleConstraints { get; init; } = Array.Empty<string>();

    /// <summary>Whether to validate patches.</summary>
    public bool ValidatePatches { get; init; } = true;
}

/// <summary>
/// Patch selection result.
/// </summary>
public class PatchSelectionResult
{
    /// <summary>Selected patch.</summary>
    public PatchCandidate? SelectedPatch { get; init; }

    /// <summary>All candidates.</summary>
    public IReadOnlyList<PatchCandidate> AllCandidates { get; init; } = Array.Empty<PatchCandidate>();

    /// <summary>Selection reasoning.</summary>
    public string? SelectionReasoning { get; init; }

    /// <summary>Confidence in selection.</summary>
    public double Confidence { get; init; }

    /// <summary>Validation result.</summary>
    public PatchValidation? Validation { get; init; }
}

/// <summary>
/// Patch validation result.
/// </summary>
public class PatchValidation
{
    /// <summary>Whether patch is valid.</summary>
    public bool IsValid { get; init; }

    /// <summary>Syntax errors.</summary>
    public IReadOnlyList<string> SyntaxErrors { get; init; } = Array.Empty<string>();

    /// <summary>Style violations.</summary>
    public IReadOnlyList<string> StyleViolations { get; init; } = Array.Empty<string>();

    /// <summary>Test results if run.</summary>
    public bool? TestsPassed { get; init; }

    /// <summary>Semantic issues.</summary>
    public IReadOnlyList<string> SemanticIssues { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for patch steering via prompted tool selection.
/// Implements the "Patch Steering via Prompted Tool Selection" pattern.
/// </summary>
public interface IPatchSteeringPort
{
    /// <summary>
    /// Generates patch candidates for an issue.
    /// </summary>
    Task<IReadOnlyList<PatchCandidate>> GenerateCandidatesAsync(
        string issueDescription,
        string filePath,
        string currentContent,
        PatchSteeringConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Selects the best patch.
    /// </summary>
    Task<PatchSelectionResult> SelectBestPatchAsync(
        IEnumerable<PatchCandidate> candidates,
        string issueDescription,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates and selects in one operation.
    /// </summary>
    Task<PatchSelectionResult> GenerateAndSelectAsync(
        string issueDescription,
        string filePath,
        string currentContent,
        PatchSteeringConfiguration? configuration = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a patch.
    /// </summary>
    Task<PatchValidation> ValidatePatchAsync(
        PatchCandidate patch,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a patch.
    /// </summary>
    Task<bool> ApplyPatchAsync(
        PatchCandidate patch,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reverts a patch.
    /// </summary>
    Task<bool> RevertPatchAsync(
        string candidateId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records patch outcome for learning.
    /// </summary>
    Task RecordOutcomeAsync(
        string candidateId,
        bool successful,
        string? feedback = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recommended tools for a patch type.
    /// </summary>
    Task<IReadOnlyList<string>> GetRecommendedToolsAsync(
        string issueType,
        string language,
        CancellationToken cancellationToken = default);
}
