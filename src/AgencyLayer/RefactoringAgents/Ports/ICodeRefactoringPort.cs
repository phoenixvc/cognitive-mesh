using System.Text.Json.Serialization;

namespace AgencyLayer.RefactoringAgents.Ports;

/// <summary>
/// Identifies which SOLID principle is violated.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SolidPrinciple
{
    /// <summary>Single Responsibility Principle: a class should have one reason to change.</summary>
    SRP,
    /// <summary>Open/Closed Principle: open for extension, closed for modification.</summary>
    OCP,
    /// <summary>Liskov Substitution Principle: subtypes must be substitutable for their base types.</summary>
    LSP,
    /// <summary>Interface Segregation Principle: clients should not depend on methods they do not use.</summary>
    ISP,
    /// <summary>Dependency Inversion Principle: depend on abstractions, not concretions.</summary>
    DIP
}

/// <summary>
/// Severity level for a detected code issue.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IssueSeverity
{
    /// <summary>Informational suggestion for improvement.</summary>
    Info,
    /// <summary>Potential issue that may cause maintenance burden.</summary>
    Warning,
    /// <summary>Clear violation that should be addressed.</summary>
    Error
}

/// <summary>
/// Specifies which categories of analysis to perform.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AnalysisScope
{
    /// <summary>Analyze only SOLID principle adherence.</summary>
    Solid,
    /// <summary>Analyze only DRY (Don't Repeat Yourself) adherence.</summary>
    Dry,
    /// <summary>Analyze both SOLID and DRY.</summary>
    Both
}

/// <summary>
/// A location within source code where an issue was detected.
/// </summary>
public class CodeLocation
{
    /// <summary>The file path or class name where the issue was found.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>The approximate line number of the issue.</summary>
    public int Line { get; set; }

    /// <summary>The name of the class, method, or member involved.</summary>
    public string MemberName { get; set; } = string.Empty;
}

/// <summary>
/// A detected violation of a SOLID principle.
/// </summary>
public class SolidViolation
{
    /// <summary>Which SOLID principle was violated.</summary>
    public SolidPrinciple Principle { get; set; }

    /// <summary>Severity of the violation.</summary>
    public IssueSeverity Severity { get; set; }

    /// <summary>Where in the code the violation occurs.</summary>
    public CodeLocation Location { get; set; } = new();

    /// <summary>Human-readable description of the violation.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Suggested fix or refactoring approach.</summary>
    public string SuggestedFix { get; set; } = string.Empty;
}

/// <summary>
/// A detected DRY violation (duplicated or repeated code).
/// </summary>
public class DryViolation
{
    /// <summary>Severity of the duplication.</summary>
    public IssueSeverity Severity { get; set; }

    /// <summary>Locations where the duplicated code appears.</summary>
    public List<CodeLocation> Locations { get; set; } = new();

    /// <summary>The duplicated code fragment or pattern description.</summary>
    public string DuplicatedPattern { get; set; } = string.Empty;

    /// <summary>Human-readable description of the duplication.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Suggested abstraction or refactoring to eliminate the duplication.</summary>
    public string SuggestedAbstraction { get; set; } = string.Empty;
}

/// <summary>
/// A concrete refactoring suggestion with before/after code.
/// </summary>
public class RefactoringSuggestion
{
    /// <summary>The type of refactoring (e.g., ExtractInterface, ExtractMethod, InjectDependency).</summary>
    public string RefactoringType { get; set; } = string.Empty;

    /// <summary>Human-readable description of what the refactoring does.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>The original code before refactoring.</summary>
    public string Before { get; set; } = string.Empty;

    /// <summary>The suggested code after refactoring.</summary>
    public string After { get; set; } = string.Empty;

    /// <summary>Estimated impact: how much this refactoring improves the code (0.0 to 1.0).</summary>
    public double Impact { get; set; }
}

/// <summary>
/// Request to analyze source code for SOLID/DRY violations.
/// </summary>
public class CodeAnalysisRequest
{
    /// <summary>The source code to analyze.</summary>
    public string SourceCode { get; set; } = string.Empty;

    /// <summary>The programming language of the source code.</summary>
    public string Language { get; set; } = "csharp";

    /// <summary>Which analysis categories to include.</summary>
    public AnalysisScope Scope { get; set; } = AnalysisScope.Both;

    /// <summary>Optional file path for context in reported locations.</summary>
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Result of analyzing source code for SOLID/DRY violations.
/// </summary>
public class CodeAnalysisResponse
{
    /// <summary>Detected SOLID principle violations.</summary>
    public List<SolidViolation> SolidViolations { get; set; } = new();

    /// <summary>Detected DRY violations.</summary>
    public List<DryViolation> DryViolations { get; set; } = new();

    /// <summary>Concrete refactoring suggestions.</summary>
    public List<RefactoringSuggestion> Suggestions { get; set; } = new();

    /// <summary>Overall code quality score (0.0 worst to 1.0 best).</summary>
    public double OverallScore { get; set; }

    /// <summary>Human-readable summary of the analysis.</summary>
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Defines the contract for code refactoring analysis operations.
/// This port follows the Hexagonal Architecture pattern for the Agency Layer.
/// </summary>
public interface ICodeRefactoringPort
{
    /// <summary>
    /// Analyzes source code for SOLID and/or DRY principle violations.
    /// </summary>
    /// <param name="request">The analysis request containing source code and options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Analysis results including violations, suggestions, and an overall score.</returns>
    Task<CodeAnalysisResponse> AnalyzeCodeAsync(
        CodeAnalysisRequest request,
        CancellationToken cancellationToken = default);
}
