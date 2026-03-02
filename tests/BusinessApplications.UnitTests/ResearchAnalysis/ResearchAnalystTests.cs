using CognitiveMesh.BusinessApplications.ResearchAnalysis;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.BusinessApplications.ResearchAnalysis;

/// <summary>
/// Unit tests for <see cref="ResearchAnalyst"/>, covering constructor guards,
/// research topic analysis, result retrieval, search, and update operations.
/// </summary>
public class ResearchAnalystTests
{
    private readonly Mock<ILogger<ResearchAnalyst>> _loggerMock;
    private readonly Mock<IKnowledgeGraphManager> _knowledgeGraphMock;
    private readonly Mock<ILLMClient> _llmClientMock;
    private readonly Mock<IVectorDatabaseAdapter> _vectorDatabaseMock;
    private readonly ResearchAnalyst _sut;

    public ResearchAnalystTests()
    {
        _loggerMock = new Mock<ILogger<ResearchAnalyst>>();
        _knowledgeGraphMock = new Mock<IKnowledgeGraphManager>();
        _llmClientMock = new Mock<ILLMClient>();
        _vectorDatabaseMock = new Mock<IVectorDatabaseAdapter>();

        _sut = new ResearchAnalyst(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new ResearchAnalyst(
            null!,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullKnowledgeGraphManager_ThrowsArgumentNullException()
    {
        var act = () => new ResearchAnalyst(
            _loggerMock.Object,
            null!,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("knowledgeGraphManager");
    }

    [Fact]
    public void Constructor_NullLLMClient_ThrowsArgumentNullException()
    {
        var act = () => new ResearchAnalyst(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            null!,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("llmClient");
    }

    [Fact]
    public void Constructor_NullVectorDatabase_ThrowsArgumentNullException()
    {
        var act = () => new ResearchAnalyst(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("vectorDatabase");
    }

    [Fact]
    public void Constructor_AllValidDependencies_DoesNotThrow()
    {
        var act = () => new ResearchAnalyst(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // AnalyzeResearchTopicAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AnalyzeResearchTopicAsync_ValidTopic_ReturnsCompletedResult()
    {
        var result = await _sut.AnalyzeResearchTopicAsync(
            "AI Ethics", null, CancellationToken.None);

        result.Should().NotBeNull();
        result.Topic.Should().Be("AI Ethics");
        result.Status.Should().Be(ResearchStatus.Completed);
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_ValidTopic_ReturnsResultWithId()
    {
        var result = await _sut.AnalyzeResearchTopicAsync(
            "Machine Learning", null, CancellationToken.None);

        result.Id.Should().NotBeNullOrEmpty();
        result.Id.Should().StartWith("research-");
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_ValidTopic_ReturnsKeyFindings()
    {
        var result = await _sut.AnalyzeResearchTopicAsync(
            "Neural Networks", null, CancellationToken.None);

        result.KeyFindings.Should().NotBeNull();
        result.KeyFindings.Should().HaveCount(3);
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_ValidTopic_ReturnsSummary()
    {
        var result = await _sut.AnalyzeResearchTopicAsync(
            "Quantum Computing", null, CancellationToken.None);

        result.Summary.Should().NotBeNullOrEmpty();
        result.Summary.Should().Contain("Quantum Computing");
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_WithParameters_StoresParametersInResult()
    {
        var parameters = new ResearchParameters
        {
            MaxSources = 20,
            IncludeExternalSources = false,
            MinConfidence = 0.9f,
            TimeoutSeconds = 600
        };

        var result = await _sut.AnalyzeResearchTopicAsync(
            "Deep Learning", parameters, CancellationToken.None);

        result.Parameters.Should().NotBeNull();
        result.Parameters.Should().BeSameAs(parameters);
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_NullParameters_UsesDefaultParameters()
    {
        var result = await _sut.AnalyzeResearchTopicAsync(
            "Data Science", null, CancellationToken.None);

        result.Parameters.Should().NotBeNull();
        result.Parameters.MaxSources.Should().Be(10);
        result.Parameters.IncludeExternalSources.Should().BeTrue();
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_ValidTopic_SetsTimestamps()
    {
        var before = DateTime.UtcNow;

        var result = await _sut.AnalyzeResearchTopicAsync(
            "Timestamps Test", null, CancellationToken.None);

        var after = DateTime.UtcNow;
        result.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
        result.CompletedAt.Should().NotBeNull();
        result.CompletedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AnalyzeResearchTopicAsync_NullOrEmptyTopic_ThrowsArgumentException(string? topic)
    {
        var act = () => _sut.AnalyzeResearchTopicAsync(topic!, null, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("topic");
    }

    [Fact]
    public async Task AnalyzeResearchTopicAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.AnalyzeResearchTopicAsync("Cancelled Topic", null, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // GetResearchResultAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetResearchResultAsync_ValidResearchId_ReturnsResult()
    {
        var result = await _sut.GetResearchResultAsync(
            "research-42", CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("research-42");
        result.Status.Should().Be(ResearchStatus.Completed);
    }

    [Fact]
    public async Task GetResearchResultAsync_ValidResearchId_ReturnsResultWithSummary()
    {
        var result = await _sut.GetResearchResultAsync(
            "research-99", CancellationToken.None);

        result.Summary.Should().NotBeNullOrEmpty();
        result.Topic.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetResearchResultAsync_ValidResearchId_ReturnsResultWithKeyFindings()
    {
        var result = await _sut.GetResearchResultAsync(
            "research-findings", CancellationToken.None);

        result.KeyFindings.Should().NotBeNull();
        result.KeyFindings.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetResearchResultAsync_NullOrEmptyResearchId_ThrowsArgumentException(string? researchId)
    {
        var act = () => _sut.GetResearchResultAsync(researchId!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("researchId");
    }

    [Fact]
    public async Task GetResearchResultAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.GetResearchResultAsync("research-42", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // SearchResearchResultsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SearchResearchResultsAsync_ValidQuery_ReturnsResults()
    {
        var results = await _sut.SearchResearchResultsAsync(
            "machine learning", 10, CancellationToken.None);

        results.Should().NotBeNull();
        results.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SearchResearchResultsAsync_ValidQuery_ReturnsResultsWithMatchingSummary()
    {
        var results = await _sut.SearchResearchResultsAsync(
            "neural networks", 10, CancellationToken.None);

        var resultList = results.ToList();
        resultList.Should().HaveCountGreaterThan(0);
        resultList[0].Summary.Should().Contain("neural networks");
    }

    [Fact]
    public async Task SearchResearchResultsAsync_ValidQuery_ReturnsCompletedResults()
    {
        var results = await _sut.SearchResearchResultsAsync(
            "search test", 10, CancellationToken.None);

        var resultList = results.ToList();
        resultList.Should().AllSatisfy(r =>
        {
            r.Status.Should().Be(ResearchStatus.Completed);
            r.Id.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchResearchResultsAsync_NullOrEmptyQuery_ThrowsArgumentException(string? query)
    {
        var act = () => _sut.SearchResearchResultsAsync(query!, 10, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("query");
    }

    [Fact]
    public async Task SearchResearchResultsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.SearchResearchResultsAsync("test query", 10, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // UpdateResearchAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateResearchAsync_ValidInputs_ReturnsUpdatedResult()
    {
        var update = new ResearchUpdate
        {
            Status = ResearchStatus.InProgress,
            Summary = "Updated summary"
        };

        var result = await _sut.UpdateResearchAsync(
            "research-42", update, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("research-42");
        result.Status.Should().Be(ResearchStatus.InProgress);
    }

    [Fact]
    public async Task UpdateResearchAsync_ValidInputs_SetsUpdatedAtTimestamp()
    {
        var update = new ResearchUpdate { Summary = "Timestamp test" };
        var before = DateTime.UtcNow;

        var result = await _sut.UpdateResearchAsync(
            "research-ts", update, CancellationToken.None);

        var after = DateTime.UtcNow;
        result.UpdatedAt.Should().NotBeNull();
        result.UpdatedAt!.Value.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateResearchAsync_NullOrEmptyResearchId_ThrowsArgumentException(string? researchId)
    {
        var update = new ResearchUpdate { Summary = "Test" };

        var act = () => _sut.UpdateResearchAsync(researchId!, update, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("researchId");
    }

    [Fact]
    public async Task UpdateResearchAsync_NullUpdate_ThrowsArgumentNullException()
    {
        var act = () => _sut.UpdateResearchAsync("research-42", null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("update");
    }

    [Fact]
    public async Task UpdateResearchAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var update = new ResearchUpdate { Summary = "Cancelled" };

        var act = () => _sut.UpdateResearchAsync("research-42", update, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // Interface compliance
    // -----------------------------------------------------------------------

    [Fact]
    public void ResearchAnalyst_ImplementsIResearchAnalyst()
    {
        _sut.Should().BeAssignableTo<IResearchAnalyst>();
    }
}
