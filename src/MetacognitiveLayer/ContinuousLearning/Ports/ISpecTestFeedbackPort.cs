namespace MetacognitiveLayer.ContinuousLearning.Ports;

/// <summary>
/// Type of specification.
/// </summary>
public enum SpecificationType
{
    /// <summary>Functional requirement.</summary>
    Functional,
    /// <summary>Non-functional requirement (performance, security).</summary>
    NonFunctional,
    /// <summary>User story or acceptance criteria.</summary>
    UserStory,
    /// <summary>API contract.</summary>
    ApiContract,
    /// <summary>Business rule.</summary>
    BusinessRule,
    /// <summary>Agent behavior specification.</summary>
    AgentBehavior
}

/// <summary>
/// A specification that can be converted to tests.
/// </summary>
public class Specification
{
    /// <summary>Unique identifier.</summary>
    public string SpecId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Title of the specification.</summary>
    public required string Title { get; init; }

    /// <summary>Detailed description.</summary>
    public required string Description { get; init; }

    /// <summary>Type of specification.</summary>
    public SpecificationType Type { get; init; }

    /// <summary>Given conditions (preconditions).</summary>
    public IReadOnlyList<string> Given { get; init; } = Array.Empty<string>();

    /// <summary>When actions (triggers).</summary>
    public IReadOnlyList<string> When { get; init; } = Array.Empty<string>();

    /// <summary>Then outcomes (expected results).</summary>
    public IReadOnlyList<string> Then { get; init; } = Array.Empty<string>();

    /// <summary>Examples for the specification.</summary>
    public IReadOnlyList<SpecificationExample> Examples { get; init; } = Array.Empty<SpecificationExample>();

    /// <summary>Agent or component this spec applies to.</summary>
    public string? AppliesTo { get; init; }

    /// <summary>Priority level.</summary>
    public int Priority { get; init; } = 50;

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();

    /// <summary>Version of the specification.</summary>
    public int Version { get; init; } = 1;

    /// <summary>When the spec was created.</summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>When the spec was last updated.</summary>
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// An example for a specification.
/// </summary>
public class SpecificationExample
{
    /// <summary>Name of the example.</summary>
    public required string Name { get; init; }

    /// <summary>Input values.</summary>
    public Dictionary<string, string> Inputs { get; init; } = new();

    /// <summary>Expected output values.</summary>
    public Dictionary<string, string> ExpectedOutputs { get; init; } = new();

    /// <summary>Notes about this example.</summary>
    public string? Notes { get; init; }
}

/// <summary>
/// A test case generated from a specification.
/// </summary>
public class SpecTest
{
    /// <summary>Unique identifier.</summary>
    public string TestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Source specification ID.</summary>
    public required string SpecId { get; init; }

    /// <summary>Test name.</summary>
    public required string Name { get; init; }

    /// <summary>Test description.</summary>
    public required string Description { get; init; }

    /// <summary>Setup steps.</summary>
    public IReadOnlyList<string> SetupSteps { get; init; } = Array.Empty<string>();

    /// <summary>Test steps.</summary>
    public IReadOnlyList<TestStep> Steps { get; init; } = Array.Empty<TestStep>();

    /// <summary>Teardown steps.</summary>
    public IReadOnlyList<string> TeardownSteps { get; init; } = Array.Empty<string>();

    /// <summary>Input data.</summary>
    public Dictionary<string, string> Inputs { get; init; } = new();

    /// <summary>Expected outcomes.</summary>
    public IReadOnlyList<ExpectedOutcome> ExpectedOutcomes { get; init; } = Array.Empty<ExpectedOutcome>();

    /// <summary>Priority.</summary>
    public int Priority { get; init; }

    /// <summary>When the test was generated.</summary>
    public DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>
/// A step in a test case.
/// </summary>
public class TestStep
{
    /// <summary>Step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Action to perform.</summary>
    public required string Action { get; init; }

    /// <summary>Expected result of this step.</summary>
    public string? ExpectedResult { get; init; }

    /// <summary>Tool or method to invoke.</summary>
    public string? ToolInvocation { get; init; }

    /// <summary>Parameters for the tool.</summary>
    public Dictionary<string, string> Parameters { get; init; } = new();
}

/// <summary>
/// An expected outcome for a test.
/// </summary>
public class ExpectedOutcome
{
    /// <summary>What to check.</summary>
    public required string Assertion { get; init; }

    /// <summary>Type of check (Equals, Contains, Matches, etc.).</summary>
    public required string CheckType { get; init; }

    /// <summary>Expected value.</summary>
    public string? ExpectedValue { get; init; }

    /// <summary>Criticality (must pass vs. nice to have).</summary>
    public bool IsCritical { get; init; } = true;
}

/// <summary>
/// Result of running a spec test.
/// </summary>
public class SpecTestResult
{
    /// <summary>The test ID.</summary>
    public required string TestId { get; init; }

    /// <summary>The spec ID.</summary>
    public required string SpecId { get; init; }

    /// <summary>Whether the test passed.</summary>
    public required bool Passed { get; init; }

    /// <summary>Step results.</summary>
    public IReadOnlyList<StepResult> StepResults { get; init; } = Array.Empty<StepResult>();

    /// <summary>Outcome results.</summary>
    public IReadOnlyList<OutcomeResult> OutcomeResults { get; init; } = Array.Empty<OutcomeResult>();

    /// <summary>Error if test failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>When the test ran.</summary>
    public DateTimeOffset RunAt { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// Result of a single test step.
/// </summary>
public class StepResult
{
    /// <summary>Step number.</summary>
    public int StepNumber { get; init; }

    /// <summary>Whether step succeeded.</summary>
    public bool Success { get; init; }

    /// <summary>Actual result.</summary>
    public string? ActualResult { get; init; }

    /// <summary>Error message if failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }
}

/// <summary>
/// Result of an outcome check.
/// </summary>
public class OutcomeResult
{
    /// <summary>The assertion.</summary>
    public required string Assertion { get; init; }

    /// <summary>Whether assertion passed.</summary>
    public bool Passed { get; init; }

    /// <summary>Actual value.</summary>
    public string? ActualValue { get; init; }

    /// <summary>Expected value.</summary>
    public string? ExpectedValue { get; init; }
}

/// <summary>
/// Port for spec-as-test feedback loop.
/// Implements the "Spec-As-Test Feedback Loop" pattern.
/// </summary>
/// <remarks>
/// This port converts specifications (user stories, requirements, API contracts)
/// into executable tests, creating a feedback loop that validates
/// agent behavior against documented specifications.
/// </remarks>
public interface ISpecTestFeedbackPort
{
    /// <summary>
    /// Creates or updates a specification.
    /// </summary>
    /// <param name="spec">The specification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The spec ID.</returns>
    Task<string> UpsertSpecificationAsync(
        Specification spec,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates tests from a specification.
    /// </summary>
    /// <param name="specId">The specification ID.</param>
    /// <param name="regenerate">Whether to regenerate existing tests.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generated tests.</returns>
    Task<IReadOnlyList<SpecTest>> GenerateTestsAsync(
        string specId,
        bool regenerate = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs tests for a specification.
    /// </summary>
    /// <param name="specId">The specification ID.</param>
    /// <param name="agentId">The agent to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test results.</returns>
    Task<IReadOnlyList<SpecTestResult>> RunSpecTestsAsync(
        string specId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a specific test.
    /// </summary>
    /// <param name="testId">The test ID.</param>
    /// <param name="agentId">The agent to test.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test result.</returns>
    Task<SpecTestResult> RunTestAsync(
        string testId,
        string agentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets specifications.
    /// </summary>
    /// <param name="type">Filter by type (null = all).</param>
    /// <param name="appliesTo">Filter by target (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Specifications.</returns>
    Task<IReadOnlyList<Specification>> GetSpecificationsAsync(
        SpecificationType? type = null,
        string? appliesTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tests for a specification.
    /// </summary>
    /// <param name="specId">The specification ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tests.</returns>
    Task<IReadOnlyList<SpecTest>> GetTestsAsync(
        string specId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets test history.
    /// </summary>
    /// <param name="specId">Filter by spec (null = all).</param>
    /// <param name="agentId">Filter by agent (null = all).</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test results.</returns>
    Task<IReadOnlyList<SpecTestResult>> GetTestHistoryAsync(
        string? specId = null,
        string? agentId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets coverage report showing which specs have tests and pass rates.
    /// </summary>
    /// <param name="appliesTo">Filter by target (null = all).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Coverage report.</returns>
    Task<SpecCoverageReport> GetCoverageReportAsync(
        string? appliesTo = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Coverage report for specifications.
/// </summary>
public class SpecCoverageReport
{
    /// <summary>Total specs.</summary>
    public int TotalSpecs { get; init; }

    /// <summary>Specs with tests.</summary>
    public int SpecsWithTests { get; init; }

    /// <summary>Total tests.</summary>
    public int TotalTests { get; init; }

    /// <summary>Overall pass rate.</summary>
    public double OverallPassRate { get; init; }

    /// <summary>Specs by type.</summary>
    public Dictionary<SpecificationType, int> SpecsByType { get; init; } = new();

    /// <summary>Pass rate by type.</summary>
    public Dictionary<SpecificationType, double> PassRateByType { get; init; } = new();

    /// <summary>Specs without tests.</summary>
    public IReadOnlyList<string> SpecsWithoutTests { get; init; } = Array.Empty<string>();

    /// <summary>Failing specs.</summary>
    public IReadOnlyList<string> FailingSpecs { get; init; } = Array.Empty<string>();
}
