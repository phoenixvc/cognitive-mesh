namespace AgencyLayer.Orchestration.Ports;

/// <summary>
/// Type of mock behavior for a tool.
/// </summary>
public enum MockBehavior
{
    /// <summary>Return a static response.</summary>
    StaticResponse,
    /// <summary>Return responses in sequence.</summary>
    Sequence,
    /// <summary>Return response based on input matching.</summary>
    InputMatching,
    /// <summary>Simulate latency.</summary>
    DelayedResponse,
    /// <summary>Simulate failure.</summary>
    Error,
    /// <summary>Record calls without executing.</summary>
    Record
}

/// <summary>
/// Configuration for mocking a tool.
/// </summary>
public class ToolMockConfiguration
{
    /// <summary>The tool ID to mock.</summary>
    public required string ToolId { get; init; }

    /// <summary>Mock behavior type.</summary>
    public MockBehavior Behavior { get; init; } = MockBehavior.StaticResponse;

    /// <summary>Static response to return.</summary>
    public string? StaticResponse { get; init; }

    /// <summary>Sequence of responses for Sequence behavior.</summary>
    public IReadOnlyList<string> ResponseSequence { get; init; } = Array.Empty<string>();

    /// <summary>Input patterns and their responses for InputMatching.</summary>
    public Dictionary<string, string> InputResponses { get; init; } = new();

    /// <summary>Delay in milliseconds for DelayedResponse.</summary>
    public int DelayMs { get; init; }

    /// <summary>Error message for Error behavior.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Whether the mock is enabled.</summary>
    public bool Enabled { get; init; } = true;
}

/// <summary>
/// Configuration for a workflow eval session.
/// </summary>
public class WorkflowEvalConfiguration
{
    /// <summary>Unique identifier.</summary>
    public string ConfigurationId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Name of the eval configuration.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>Tool mocks to apply.</summary>
    public IReadOnlyList<ToolMockConfiguration> ToolMocks { get; init; } = Array.Empty<ToolMockConfiguration>();

    /// <summary>Whether to record all tool calls.</summary>
    public bool RecordToolCalls { get; init; } = true;

    /// <summary>Whether to record all agent decisions.</summary>
    public bool RecordDecisions { get; init; } = true;

    /// <summary>Timeout for the eval in milliseconds.</summary>
    public int TimeoutMs { get; init; } = 60000;

    /// <summary>Whether to allow real tool calls for non-mocked tools.</summary>
    public bool AllowRealCalls { get; init; } = false;
}

/// <summary>
/// A workflow test case.
/// </summary>
public class WorkflowTestCase
{
    /// <summary>Unique identifier.</summary>
    public string TestId { get; init; } = Guid.NewGuid().ToString();

    /// <summary>Name of the test.</summary>
    public required string Name { get; init; }

    /// <summary>Description.</summary>
    public string? Description { get; init; }

    /// <summary>The workflow to test.</summary>
    public required string WorkflowId { get; init; }

    /// <summary>Input to the workflow.</summary>
    public required string Input { get; init; }

    /// <summary>Eval configuration to use.</summary>
    public required WorkflowEvalConfiguration Configuration { get; init; }

    /// <summary>Expected outcomes to verify.</summary>
    public IReadOnlyList<WorkflowExpectation> Expectations { get; init; } = Array.Empty<WorkflowExpectation>();

    /// <summary>Priority level.</summary>
    public int Priority { get; init; } = 50;

    /// <summary>Tags for categorization.</summary>
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}

/// <summary>
/// An expectation for a workflow test.
/// </summary>
public class WorkflowExpectation
{
    /// <summary>What to check.</summary>
    public required string Assertion { get; init; }

    /// <summary>Check type (OutputContains, ToolCalled, DecisionMade, etc.).</summary>
    public required string CheckType { get; init; }

    /// <summary>Expected value.</summary>
    public string? ExpectedValue { get; init; }

    /// <summary>Whether this is critical to pass.</summary>
    public bool IsCritical { get; init; } = true;
}

/// <summary>
/// Record of a tool call during eval.
/// </summary>
public class ToolCallRecord
{
    public required string ToolId { get; init; }
    public required string Input { get; init; }
    public string? Output { get; init; }
    public bool WasMocked { get; init; }
    public bool Success { get; init; }
    public double DurationMs { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

/// <summary>
/// Record of a decision during eval.
/// </summary>
public class DecisionRecord
{
    public required string DecisionType { get; init; }
    public required string Description { get; init; }
    public string? Reasoning { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

/// <summary>
/// Result of a workflow eval.
/// </summary>
public class WorkflowEvalResult
{
    /// <summary>The test ID.</summary>
    public required string TestId { get; init; }

    /// <summary>Whether the eval passed.</summary>
    public required bool Passed { get; init; }

    /// <summary>Overall score (0.0 - 1.0).</summary>
    public double Score { get; init; }

    /// <summary>Results for each expectation.</summary>
    public IReadOnlyList<ExpectationResult> ExpectationResults { get; init; } = Array.Empty<ExpectationResult>();

    /// <summary>Tool calls made during eval.</summary>
    public IReadOnlyList<ToolCallRecord> ToolCalls { get; init; } = Array.Empty<ToolCallRecord>();

    /// <summary>Decisions made during eval.</summary>
    public IReadOnlyList<DecisionRecord> Decisions { get; init; } = Array.Empty<DecisionRecord>();

    /// <summary>Final output from the workflow.</summary>
    public string? FinalOutput { get; init; }

    /// <summary>Error if the workflow failed.</summary>
    public string? ErrorMessage { get; init; }

    /// <summary>Duration in milliseconds.</summary>
    public double DurationMs { get; init; }

    /// <summary>When the eval ran.</summary>
    public DateTimeOffset RunAt { get; init; }
}

/// <summary>
/// Result of checking an expectation.
/// </summary>
public class ExpectationResult
{
    public required string Assertion { get; init; }
    public bool Passed { get; init; }
    public string? ActualValue { get; init; }
    public string? ExpectedValue { get; init; }
    public string? Explanation { get; init; }
}

/// <summary>
/// Port for workflow evaluations with mocked tools.
/// Implements the "Workflow Evals with Mocked Tools" pattern.
/// </summary>
/// <remarks>
/// This port enables testing of complete workflows by mocking tool
/// responses, allowing deterministic testing of agent decision-making
/// without side effects from real tool execution.
/// </remarks>
public interface IWorkflowEvalsPort
{
    /// <summary>
    /// Creates an eval configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The configuration ID.</returns>
    Task<string> CreateConfigurationAsync(
        WorkflowEvalConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a test case.
    /// </summary>
    /// <param name="testCase">The test case.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The test ID.</returns>
    Task<string> CreateTestCaseAsync(
        WorkflowTestCase testCase,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a single test case.
    /// </summary>
    /// <param name="testId">The test to run.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The eval result.</returns>
    Task<WorkflowEvalResult> RunTestAsync(
        string testId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs multiple test cases.
    /// </summary>
    /// <param name="testIds">Tests to run (null = all).</param>
    /// <param name="tags">Filter by tags.</param>
    /// <param name="parallel">Whether to run in parallel.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All results.</returns>
    Task<IReadOnlyList<WorkflowEvalResult>> RunTestsAsync(
        IEnumerable<string>? testIds = null,
        IEnumerable<string>? tags = null,
        bool parallel = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets test cases.
    /// </summary>
    /// <param name="workflowId">Filter by workflow (null = all).</param>
    /// <param name="tags">Filter by tags.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Test cases.</returns>
    Task<IReadOnlyList<WorkflowTestCase>> GetTestCasesAsync(
        string? workflowId = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets eval results history.
    /// </summary>
    /// <param name="testId">Filter by test (null = all).</param>
    /// <param name="workflowId">Filter by workflow (null = all).</param>
    /// <param name="limit">Maximum results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Eval results.</returns>
    Task<IReadOnlyList<WorkflowEvalResult>> GetResultsHistoryAsync(
        string? testId = null,
        string? workflowId = null,
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets eval configurations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Configurations.</returns>
    Task<IReadOnlyList<WorkflowEvalConfiguration>> GetConfigurationsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a test case.
    /// </summary>
    /// <param name="testId">The test to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteTestCaseAsync(
        string testId,
        CancellationToken cancellationToken = default);
}
