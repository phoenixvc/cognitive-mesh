using CognitiveMesh.BusinessApplications.CustomerIntelligence;
using CognitiveMesh.Shared.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace CognitiveMesh.Tests.BusinessApplications.CustomerIntelligence;

/// <summary>
/// Unit tests for <see cref="CustomerIntelligenceManager"/>, covering constructor guards,
/// profile retrieval, segment queries, insight generation, and behavior prediction.
/// </summary>
public class CustomerIntelligenceManagerTests
{
    private readonly Mock<ILogger<CustomerIntelligenceManager>> _loggerMock;
    private readonly Mock<IKnowledgeGraphManager> _knowledgeGraphMock;
    private readonly Mock<ILLMClient> _llmClientMock;
    private readonly Mock<IVectorDatabaseAdapter> _vectorDatabaseMock;
    private readonly CustomerIntelligenceManager _sut;

    public CustomerIntelligenceManagerTests()
    {
        _loggerMock = new Mock<ILogger<CustomerIntelligenceManager>>();
        _knowledgeGraphMock = new Mock<IKnowledgeGraphManager>();
        _llmClientMock = new Mock<ILLMClient>();
        _vectorDatabaseMock = new Mock<IVectorDatabaseAdapter>();

        _sut = new CustomerIntelligenceManager(
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
        var act = () => new CustomerIntelligenceManager(
            null!,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullKnowledgeGraphManager_ThrowsArgumentNullException()
    {
        var act = () => new CustomerIntelligenceManager(
            _loggerMock.Object,
            null!,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("knowledgeGraphManager");
    }

    [Fact]
    public void Constructor_NullLLMClient_ThrowsArgumentNullException()
    {
        var act = () => new CustomerIntelligenceManager(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            null!,
            _vectorDatabaseMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("llmClient");
    }

    [Fact]
    public void Constructor_NullVectorDatabase_ThrowsArgumentNullException()
    {
        var act = () => new CustomerIntelligenceManager(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("vectorDatabase");
    }

    [Fact]
    public void Constructor_AllValidDependencies_DoesNotThrow()
    {
        var act = () => new CustomerIntelligenceManager(
            _loggerMock.Object,
            _knowledgeGraphMock.Object,
            _llmClientMock.Object,
            _vectorDatabaseMock.Object);

        act.Should().NotThrow();
    }

    // -----------------------------------------------------------------------
    // GetCustomerProfileAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCustomerProfileAsync_ValidCustomerId_ReturnsProfile()
    {
        var result = await _sut.GetCustomerProfileAsync("customer-123", CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be("customer-123");
        result.Name.Should().NotBeNullOrEmpty();
        result.Email.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetCustomerProfileAsync_ValidCustomerId_ReturnsProfileWithSegments()
    {
        var result = await _sut.GetCustomerProfileAsync("customer-456", CancellationToken.None);

        result.Segments.Should().NotBeNull();
        result.Segments.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomerProfileAsync_ValidCustomerId_ReturnsProfileWithMetadata()
    {
        var result = await _sut.GetCustomerProfileAsync("customer-789", CancellationToken.None);

        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomerProfileAsync_ValidCustomerId_ReturnsPositiveLifetimeValue()
    {
        var result = await _sut.GetCustomerProfileAsync("customer-001", CancellationToken.None);

        result.LifetimeValue.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetCustomerProfileAsync_NullOrEmptyCustomerId_ThrowsArgumentException(string? customerId)
    {
        var act = () => _sut.GetCustomerProfileAsync(customerId!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("customerId");
    }

    [Fact]
    public async Task GetCustomerProfileAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.GetCustomerProfileAsync("customer-123", cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // GetCustomerSegmentsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCustomerSegmentsAsync_ValidQuery_ReturnsSegments()
    {
        var query = new CustomerSegmentQuery();

        var result = await _sut.GetCustomerSegmentsAsync(query, CancellationToken.None);

        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCustomerSegmentsAsync_ValidQuery_ReturnsSegmentsWithProperties()
    {
        var query = new CustomerSegmentQuery { NameContains = "High" };

        var result = await _sut.GetCustomerSegmentsAsync(query, CancellationToken.None);

        var segments = result.ToList();
        segments.Should().HaveCountGreaterThan(0);
        segments.Should().AllSatisfy(segment =>
        {
            segment.Id.Should().NotBeNullOrEmpty();
            segment.Name.Should().NotBeNullOrEmpty();
            segment.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Fact]
    public async Task GetCustomerSegmentsAsync_ValidQuery_ReturnsSegmentsWithRules()
    {
        var query = new CustomerSegmentQuery();

        var result = await _sut.GetCustomerSegmentsAsync(query, CancellationToken.None);

        var segments = result.ToList();
        segments.Should().AllSatisfy(segment =>
        {
            segment.Rules.Should().NotBeNull();
            segment.Rules.Should().NotBeEmpty();
        });
    }

    [Fact]
    public async Task GetCustomerSegmentsAsync_NullQuery_ThrowsArgumentNullException()
    {
        var act = () => _sut.GetCustomerSegmentsAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>().WithParameterName("query");
    }

    [Fact]
    public async Task GetCustomerSegmentsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var query = new CustomerSegmentQuery();

        var act = () => _sut.GetCustomerSegmentsAsync(query, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // GenerateCustomerInsightsAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GenerateCustomerInsightsAsync_ValidCustomerIdAllTypes_ReturnsBothInsightTypes()
    {
        var result = await _sut.GenerateCustomerInsightsAsync(
            "customer-123", InsightType.All, CancellationToken.None);

        var insights = result.ToList();
        insights.Should().HaveCount(2);
        insights.Should().Contain(i => i.Type == InsightType.PurchasePatterns);
        insights.Should().Contain(i => i.Type == InsightType.BehavioralPatterns);
    }

    [Fact]
    public async Task GenerateCustomerInsightsAsync_PurchasePatternsOnly_ReturnsPurchaseInsight()
    {
        var result = await _sut.GenerateCustomerInsightsAsync(
            "customer-123", InsightType.PurchasePatterns, CancellationToken.None);

        var insights = result.ToList();
        insights.Should().HaveCount(1);
        insights[0].Type.Should().Be(InsightType.PurchasePatterns);
        insights[0].Confidence.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GenerateCustomerInsightsAsync_BehavioralPatternsOnly_ReturnsBehavioralInsight()
    {
        var result = await _sut.GenerateCustomerInsightsAsync(
            "customer-123", InsightType.BehavioralPatterns, CancellationToken.None);

        var insights = result.ToList();
        insights.Should().HaveCount(1);
        insights[0].Type.Should().Be(InsightType.BehavioralPatterns);
    }

    [Fact]
    public async Task GenerateCustomerInsightsAsync_DefaultInsightType_ReturnsAllInsights()
    {
        var result = await _sut.GenerateCustomerInsightsAsync("customer-123");

        var insights = result.ToList();
        insights.Should().HaveCount(2);
    }

    [Fact]
    public async Task GenerateCustomerInsightsAsync_InsightsContainMetadata_MetadataNotEmpty()
    {
        var result = await _sut.GenerateCustomerInsightsAsync(
            "customer-123", InsightType.All, CancellationToken.None);

        var insights = result.ToList();
        insights.Should().AllSatisfy(insight =>
        {
            insight.Metadata.Should().NotBeNull();
            insight.Metadata.Should().NotBeEmpty();
            insight.Title.Should().NotBeNullOrEmpty();
            insight.Description.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GenerateCustomerInsightsAsync_NullOrEmptyCustomerId_ThrowsArgumentException(string? customerId)
    {
        var act = () => _sut.GenerateCustomerInsightsAsync(customerId!, InsightType.All, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("customerId");
    }

    [Fact]
    public async Task GenerateCustomerInsightsAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.GenerateCustomerInsightsAsync("customer-123", InsightType.All, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // PredictCustomerBehaviorAsync tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PredictCustomerBehaviorAsync_ChurnPrediction_ReturnsChurnResult()
    {
        var result = await _sut.PredictCustomerBehaviorAsync(
            "customer-123", PredictionType.Churn, CancellationToken.None);

        result.Should().NotBeNull();
        result.CustomerId.Should().Be("customer-123");
        result.Type.Should().Be(PredictionType.Churn);
        result.PredictedValue.Should().Be(0.65f);
        result.Explanation.Should().Contain("churn");
    }

    [Fact]
    public async Task PredictCustomerBehaviorAsync_PurchasePrediction_ReturnsPurchaseResult()
    {
        var result = await _sut.PredictCustomerBehaviorAsync(
            "customer-123", PredictionType.Purchase, CancellationToken.None);

        result.Should().NotBeNull();
        result.Type.Should().Be(PredictionType.Purchase);
        result.PredictedValue.Should().Be(0.78f);
        result.Explanation.Should().Contain("purchase");
    }

    [Fact]
    public async Task PredictCustomerBehaviorAsync_ValidPrediction_ReturnsConfidenceScore()
    {
        var result = await _sut.PredictCustomerBehaviorAsync(
            "customer-123", PredictionType.Churn, CancellationToken.None);

        result.Confidence.Should().BeInRange(0f, 1f);
    }

    [Fact]
    public async Task PredictCustomerBehaviorAsync_ValidPrediction_ReturnsMetadata()
    {
        var result = await _sut.PredictCustomerBehaviorAsync(
            "customer-123", PredictionType.Churn, CancellationToken.None);

        result.Metadata.Should().NotBeNull();
        result.Metadata.Should().ContainKey("timeframe");
        result.Metadata.Should().ContainKey("modelVersion");
    }

    [Fact]
    public async Task PredictCustomerBehaviorAsync_ValidPrediction_SetsGeneratedAtTimestamp()
    {
        var before = DateTime.UtcNow;

        var result = await _sut.PredictCustomerBehaviorAsync(
            "customer-123", PredictionType.Churn, CancellationToken.None);

        var after = DateTime.UtcNow;
        result.GeneratedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task PredictCustomerBehaviorAsync_NullOrEmptyCustomerId_ThrowsArgumentException(string? customerId)
    {
        var act = () => _sut.PredictCustomerBehaviorAsync(customerId!, PredictionType.Churn, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithParameterName("customerId");
    }

    [Fact]
    public async Task PredictCustomerBehaviorAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => _sut.PredictCustomerBehaviorAsync("customer-123", PredictionType.Churn, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // -----------------------------------------------------------------------
    // Interface compliance
    // -----------------------------------------------------------------------

    [Fact]
    public void CustomerIntelligenceManager_ImplementsICustomerIntelligenceManager()
    {
        _sut.Should().BeAssignableTo<ICustomerIntelligenceManager>();
    }
}
