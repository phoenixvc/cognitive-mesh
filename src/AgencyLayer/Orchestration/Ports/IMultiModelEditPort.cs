namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// A model in the multi-model pipeline.
/// </summary>
public class EditModel
{
    /// <summary>Model identifier.</summary>
    public required string ModelId { get; init; }

    /// <summary>Model role.</summary>
    public EditModelRole Role { get; init; }

    /// <summary>Specialization.</summary>
    public string? Specialization { get; init; }

    /// <summary>Weight in ensemble.</summary>
    public double Weight { get; init; } = 1.0;

    /// <summary>Configuration.</summary>
    public Dictionary<string, string> Configuration { get; init; } = new();
}

/// <summary>
/// Model role in editing pipeline.
/// </summary>
public enum EditModelRole
{
    /// <summary>Analyzes and understands code.</summary>
    Analyzer,
    /// <summary>Plans the edit.</summary>
    Planner,
    /// <summary>Generates code.</summary>
    Generator,
    /// <summary>Reviews and refines.</summary>
    Reviewer,
    /// <summary>Validates output.</summary>
    Validator
}

/// <summary>
/// A complex edit request.
/// </summary>
public class ComplexEditRequest
{
    /// <summary>Request identifier.</summary>
    public string RequestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Edit description.</summary>
    public required string Description { get; init; }

    /// <summary>Target files.</summary>
    public IReadOnlyList<TargetFile> TargetFiles { get; init; } = Array.Empty<TargetFile>();

    /// <summary>Context files.</summary>
    public IReadOnlyList<string> ContextFiles { get; init; } = Array.Empty<string>();

    /// <summary>Constraints.</summary>
    public IReadOnlyList<string> Constraints { get; init; } = Array.Empty<string>();

    /// <summary>Models to use.</summary>
    public IReadOnlyList<EditModel> Models { get; init; } = Array.Empty<EditModel>();

    /// <summary>Whether to run validation.</summary>
    public bool ValidateOutput { get; init; } = true;

    /// <summary>Maximum iterations.</summary>
    public int MaxIterations { get; init; } = 3;
}

/// <summary>
/// A target file for editing.
/// </summary>
public class TargetFile
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Current content.</summary>
    public required string Content { get; init; }

    /// <summary>Specific sections to edit.</summary>
    public IReadOnlyList<(int Start, int End)> Sections { get; init; } = Array.Empty<(int, int)>();
}

/// <summary>
/// Result of a complex edit.
/// </summary>
public class ComplexEditResult
{
    /// <summary>Request identifier.</summary>
    public required string RequestId { get; init; }

    /// <summary>Whether successful.</summary>
    public bool Success { get; init; }

    /// <summary>Edited files.</summary>
    public IReadOnlyList<EditedFile> EditedFiles { get; init; } = Array.Empty<EditedFile>();

    /// <summary>Pipeline stages.</summary>
    public IReadOnlyList<PipelineStage> Stages { get; init; } = Array.Empty<PipelineStage>();

    /// <summary>Validation results.</summary>
    public ValidationResults? Validation { get; init; }

    /// <summary>Iterations used.</summary>
    public int Iterations { get; init; }

    /// <summary>Total duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// An edited file.
/// </summary>
public class EditedFile
{
    /// <summary>File path.</summary>
    public required string FilePath { get; init; }

    /// <summary>Original content.</summary>
    public required string OriginalContent { get; init; }

    /// <summary>New content.</summary>
    public required string NewContent { get; init; }

    /// <summary>Diff.</summary>
    public required string Diff { get; init; }
}

/// <summary>
/// A stage in the pipeline.
/// </summary>
public class PipelineStage
{
    /// <summary>Stage name.</summary>
    public required string Name { get; init; }

    /// <summary>Model used.</summary>
    public required string ModelId { get; init; }

    /// <summary>Role.</summary>
    public EditModelRole Role { get; init; }

    /// <summary>Output.</summary>
    public required string Output { get; init; }

    /// <summary>Duration.</summary>
    public TimeSpan Duration { get; init; }
}

/// <summary>
/// Validation results.
/// </summary>
public class ValidationResults
{
    /// <summary>Syntax valid.</summary>
    public bool SyntaxValid { get; init; }

    /// <summary>Tests pass.</summary>
    public bool? TestsPass { get; init; }

    /// <summary>Lint clean.</summary>
    public bool? LintClean { get; init; }

    /// <summary>Issues found.</summary>
    public IReadOnlyList<string> Issues { get; init; } = Array.Empty<string>();
}

/// <summary>
/// Port for multi-model orchestration for complex edits.
/// Implements the "Multi-Model Orchestration for Complex Edits" pattern.
/// </summary>
public interface IMultiModelEditPort
{
    /// <summary>
    /// Performs a complex edit using multiple models.
    /// </summary>
    Task<ComplexEditResult> EditAsync(
        ComplexEditRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configures the editing pipeline.
    /// </summary>
    Task ConfigurePipelineAsync(
        IEnumerable<EditModel> models,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current pipeline configuration.
    /// </summary>
    Task<IReadOnlyList<EditModel>> GetPipelineConfigurationAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an edit without applying.
    /// </summary>
    Task<ValidationResults> ValidateEditAsync(
        EditedFile editedFile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies edits to files.
    /// </summary>
    Task ApplyEditsAsync(
        IEnumerable<EditedFile> edits,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets edit history.
    /// </summary>
    Task<IReadOnlyList<ComplexEditResult>> GetHistoryAsync(
        int limit = 20,
        CancellationToken cancellationToken = default);
}
