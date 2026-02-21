using AgencyLayer.DecisionExecution;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.AgencyLayer.DecisionExecution;

/// <summary>
/// Comprehensive unit tests for <see cref="DecisionExecutor"/>, covering ExecuteDecisionAsync,
/// GetDecisionStatusAsync, GetDecisionLogsAsync, constructor null guards, cancellation,
/// and error handling paths.
/// </summary>
public class DecisionExecutorComprehensiveTests
{
    private readonly Mock<ILogger<DecisionExecutor>> _loggerMock;
    private readonly Mock<IKnowledgeGraphManager> _knowledgeGraphMock;
    private readonly Mock<ILLMClient> _llmClientMock;
    private readonly DecisionExecutor _sut;

    public DecisionExecutorComprehensiveTests()
    {
        _loggerMock = new Mock<ILogger<DecisionExecutor>>();
        _knowledgeGraphMock = new Mock<IKnowledgeGraphManager>();
        _llmClientMock = new Mock<ILLMClient>();

        _sut = new DecisionExecutor(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new DecisionExecutor(
            null!,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullKnowledgeGraphManager_ThrowsArgumentNullException()
    {
        var act = () => new DecisionExecutor(
            _loggerMock.Object,
            null!,
            _llmClientMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("knowledgeGraphManager");
    }

    [Fact]
    public void Constructor_NullLLMClient_ThrowsArgumentNullException()
    {
        var act = () => new DecisionExecutor(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("llmClient");
    }

    [Fact]
    public void Constructor_ValidDependencies_CreatesInstance()
    {
        var executor = new DecisionExecutor(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object);

        executor.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // ExecuteDecisionAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ExecuteDecisionAsync_ValidRequest_ReturnsCompletedResult()
    {
        var request = CreateDecisionRequest("Deploy API v2.0");

        var result = await _sut.ExecuteDecisionAsync(request);

        result.Should().NotBeNull();
        result.RequestId.Should().Be(request.RequestId);
        result.Status.Should().Be(DecisionStatus.Completed);
        result.Outcome.Should().Be(DecisionOutcome.Success);
        result.ExecutionTime.Should().BeGreaterThan(TimeSpan.Zero);
        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteDecisionAsync_ValidRequest_IncludesMetadata()
    {
        var request = CreateDecisionRequest("Scale infrastructure");

        var result = await _sut.ExecuteDecisionAsync(request);

        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().ContainKey("executionNode");
        result.Metadata.Should().ContainKey("version");
    }

    [Fact]
    public async Task ExecuteDecisionAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.ExecuteDecisionAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("request");
    }

    [Fact]
    public async Task ExecuteDecisionAsync_CancellationRequested_ReturnsFailedResult()
    {
        var request = CreateDecisionRequest("Cancelled decision");
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // The implementation catches all exceptions (including OperationCanceledException)
        // and returns a failed result instead of propagating
        var result = await _sut.ExecuteDecisionAsync(request, cts.Token);

        result.Status.Should().Be(DecisionStatus.Failed);
        result.Outcome.Should().Be(DecisionOutcome.Error);
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExecuteDecisionAsync_PreservesRequestId()
    {
        var request = new DecisionRequest
        {
            RequestId = "custom-req-12345",
            DecisionType = "TestDecision",
            Priority = 5
        };

        var result = await _sut.ExecuteDecisionAsync(request);

        result.RequestId.Should().Be("custom-req-12345");
    }

    [Fact]
    public async Task ExecuteDecisionAsync_WithParameters_Succeeds()
    {
        var request = new DecisionRequest
        {
            DecisionType = "ResourceAllocation",
            Parameters = new Dictionary<string, object>
            {
                ["region"] = "eu-west-1",
                ["maxInstances"] = 10,
                ["budget"] = 5000.0
            },
            Priority = 3
        };

        var result = await _sut.ExecuteDecisionAsync(request);

        result.Status.Should().Be(DecisionStatus.Completed);
        result.Outcome.Should().Be(DecisionOutcome.Success);
    }

    [Fact]
    public async Task ExecuteDecisionAsync_MultipleRequests_HandledIndependently()
    {
        var request1 = CreateDecisionRequest("Decision Alpha");
        var request2 = CreateDecisionRequest("Decision Beta");

        var result1 = await _sut.ExecuteDecisionAsync(request1);
        var result2 = await _sut.ExecuteDecisionAsync(request2);

        result1.RequestId.Should().NotBe(result2.RequestId);
        result1.Status.Should().Be(DecisionStatus.Completed);
        result2.Status.Should().Be(DecisionStatus.Completed);
    }

    // -----------------------------------------------------------------------
    // GetDecisionStatusAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDecisionStatusAsync_ValidRequestId_ReturnsCompletedStatus()
    {
        var requestId = "req-status-check-123";

        var result = await _sut.GetDecisionStatusAsync(requestId);

        result.Should().NotBeNull();
        result.RequestId.Should().Be(requestId);
        result.Status.Should().Be(DecisionStatus.Completed);
        result.Outcome.Should().Be(DecisionOutcome.Success);
    }

    [Fact]
    public async Task GetDecisionStatusAsync_NullRequestId_ThrowsArgumentException()
    {
        var act = () => _sut.GetDecisionStatusAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("requestId");
    }

    [Fact]
    public async Task GetDecisionStatusAsync_EmptyRequestId_ThrowsArgumentException()
    {
        var act = () => _sut.GetDecisionStatusAsync("");

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("requestId");
    }

    [Fact]
    public async Task GetDecisionStatusAsync_WhitespaceRequestId_ThrowsArgumentException()
    {
        var act = () => _sut.GetDecisionStatusAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("requestId");
    }

    [Fact]
    public async Task GetDecisionStatusAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.GetDecisionStatusAsync("req-123", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetDecisionStatusAsync_ReturnsTimestamp()
    {
        var result = await _sut.GetDecisionStatusAsync("req-timestamp-test");

        result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // -----------------------------------------------------------------------
    // GetDecisionLogsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetDecisionLogsAsync_DefaultParameters_ReturnsLogs()
    {
        var logs = await _sut.GetDecisionLogsAsync();

        logs.Should().NotBeNull();
        logs.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetDecisionLogsAsync_ReturnsLogsWithExpectedProperties()
    {
        var logs = (await _sut.GetDecisionLogsAsync()).ToList();

        logs.Should().HaveCountGreaterThan(0);
        var firstLog = logs.First();
        firstLog.RequestId.Should().NotBeNullOrEmpty();
        firstLog.DecisionType.Should().NotBeNullOrEmpty();
        firstLog.Status.Should().Be(DecisionStatus.Completed);
        firstLog.Outcome.Should().Be(DecisionOutcome.Success);
        firstLog.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDecisionLogsAsync_WithDateRange_ReturnsLogs()
    {
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        var logs = await _sut.GetDecisionLogsAsync(startDate, endDate);

        logs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDecisionLogsAsync_WithLimit_ReturnsLogs()
    {
        var logs = await _sut.GetDecisionLogsAsync(limit: 10);

        logs.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDecisionLogsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.GetDecisionLogsAsync(cancellationToken: cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task GetDecisionLogsAsync_LogsContainMetadata()
    {
        var logs = (await _sut.GetDecisionLogsAsync()).ToList();

        logs.Should().AllSatisfy(log =>
        {
            log.Metadata.Should().NotBeNull();
        });
    }

    // -----------------------------------------------------------------------
    // IDecisionExecutor interface compliance
    // -----------------------------------------------------------------------

    [Fact]
    public void DecisionExecutor_ImplementsIDecisionExecutor()
    {
        _sut.Should().BeAssignableTo<IDecisionExecutor>();
    }

    // -----------------------------------------------------------------------
    // DecisionRequest model tests
    // -----------------------------------------------------------------------

    [Fact]
    public void DecisionRequest_DefaultValues_ArePopulated()
    {
        var request = new DecisionRequest();

        request.RequestId.Should().NotBeNullOrEmpty();
        request.RequestId.Should().StartWith("req-");
        request.Parameters.Should().NotBeNull();
        request.Priority.Should().Be(1);
        request.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        request.Metadata.Should().NotBeNull();
    }

    [Fact]
    public void DecisionResult_DefaultValues_ArePopulated()
    {
        var result = new DecisionResult { RequestId = "test-default-values" };

        result.Results.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // Helper methods
    // -----------------------------------------------------------------------

    private static DecisionRequest CreateDecisionRequest(string decisionType)
    {
        return new DecisionRequest
        {
            DecisionType = decisionType,
            Parameters = new Dictionary<string, object>
            {
                ["param1"] = "value1",
                ["param2"] = 42
            },
            Priority = 2,
            Metadata = new Dictionary<string, object>
            {
                ["source"] = "unit-test",
                ["correlationId"] = Guid.NewGuid().ToString()
            }
        };
    }
}
