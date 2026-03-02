using CognitiveMesh.BusinessApplications.AdaptiveBalance.Controllers;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Models;
using CognitiveMesh.BusinessApplications.AdaptiveBalance.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.AdaptiveBalance;

/// <summary>
/// Unit tests for <see cref="AdaptiveBalanceController"/> covering all five
/// endpoints, constructor null guards, validation, and delegation.
/// </summary>
public class AdaptiveBalanceControllerTests
{
    private readonly Mock<ILogger<AdaptiveBalanceController>> _loggerMock;
    private readonly Mock<IAdaptiveBalanceServicePort> _servicePortMock;
    private readonly AdaptiveBalanceController _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdaptiveBalanceControllerTests"/> class.
    /// </summary>
    public AdaptiveBalanceControllerTests()
    {
        _loggerMock = new Mock<ILogger<AdaptiveBalanceController>>();
        _servicePortMock = new Mock<IAdaptiveBalanceServicePort>();

        _sut = new AdaptiveBalanceController(
            _loggerMock.Object,
            _servicePortMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AdaptiveBalanceController(null!, _servicePortMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullServicePort_ThrowsArgumentNullException()
    {
        var act = () => new AdaptiveBalanceController(_loggerMock.Object, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("servicePort");
    }

    // -----------------------------------------------------------------------
    // GetBalanceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetBalanceAsync_ValidRequest_ReturnsDelegatedResponse()
    {
        // Arrange
        var request = new BalanceRequest
        {
            Context = new Dictionary<string, string> { { "scenario", "test" } }
        };

        var expected = new BalanceResponse
        {
            Dimensions = new List<SpectrumDimensionResult>
            {
                new() { Dimension = "Profit", Value = 0.5, LowerBound = 0.4, UpperBound = 0.6, Rationale = "Default" }
            },
            OverallConfidence = 0.85,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        _servicePortMock
            .Setup(s => s.GetBalanceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetBalanceAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _servicePortMock.Verify(s => s.GetBalanceAsync(request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBalanceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.GetBalanceAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetBalanceAsync_EmptyContext_StillDelegatesToService()
    {
        var request = new BalanceRequest { Context = new Dictionary<string, string>() };

        var expected = new BalanceResponse
        {
            Dimensions = new List<SpectrumDimensionResult>(),
            OverallConfidence = 0.5,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        _servicePortMock
            .Setup(s => s.GetBalanceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.GetBalanceAsync(request, CancellationToken.None);

        result.Should().Be(expected);
    }

    // -----------------------------------------------------------------------
    // ApplyOverrideAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ApplyOverrideAsync_ValidRequest_ReturnsDelegatedResponse()
    {
        // Arrange
        var request = new OverrideRequest
        {
            Dimension = "Profit",
            NewValue = 0.7,
            Rationale = "Market conditions",
            OverriddenBy = "admin-1"
        };

        var expected = new OverrideResponse
        {
            OverrideId = Guid.NewGuid(),
            Dimension = "Profit",
            OldValue = 0.5,
            NewValue = 0.7,
            UpdatedAt = DateTimeOffset.UtcNow,
            Message = "Override applied."
        };

        _servicePortMock
            .Setup(s => s.ApplyOverrideAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.ApplyOverrideAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task ApplyOverrideAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.ApplyOverrideAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ApplyOverrideAsync_EmptyDimension_ThrowsArgumentException()
    {
        var request = new OverrideRequest
        {
            Dimension = "",
            NewValue = 0.5,
            Rationale = "Test",
            OverriddenBy = "admin-1"
        };

        var act = () => _sut.ApplyOverrideAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Dimension*");
    }

    [Fact]
    public async Task ApplyOverrideAsync_EmptyOverriddenBy_ThrowsArgumentException()
    {
        var request = new OverrideRequest
        {
            Dimension = "Profit",
            NewValue = 0.5,
            Rationale = "Test",
            OverriddenBy = ""
        };

        var act = () => _sut.ApplyOverrideAsync(request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*OverriddenBy*");
    }

    // -----------------------------------------------------------------------
    // GetSpectrumHistoryAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetSpectrumHistoryAsync_ValidDimension_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new SpectrumHistoryResponse
        {
            Dimension = "Profit",
            History = new List<SpectrumHistoryEntry>
            {
                new() { Value = 0.5, Rationale = "Default", RecordedAt = DateTimeOffset.UtcNow }
            }
        };

        _servicePortMock
            .Setup(s => s.GetSpectrumHistoryAsync("Profit", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetSpectrumHistoryAsync("Profit", CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_NullDimension_ThrowsArgumentException()
    {
        var act = () => _sut.GetSpectrumHistoryAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetSpectrumHistoryAsync_EmptyDimension_ThrowsArgumentException()
    {
        var act = () => _sut.GetSpectrumHistoryAsync("", CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // SubmitLearningEvidenceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitLearningEvidenceAsync_ValidRequest_ReturnsDelegatedResponse()
    {
        // Arrange
        var request = new LearningEvidenceRequest
        {
            PatternType = "Bias",
            Description = "Detected bias in output",
            Evidence = "Comparison data",
            Outcome = "Mitigated",
            SourceAgentId = "agent-1"
        };

        var expected = new LearningEvidenceResponse
        {
            EventId = Guid.NewGuid(),
            RecordedAt = DateTimeOffset.UtcNow,
            Message = "Learning evidence recorded."
        };

        _servicePortMock
            .Setup(s => s.SubmitLearningEvidenceAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.SubmitLearningEvidenceAsync(request, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task SubmitLearningEvidenceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitLearningEvidenceAsync(null!, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -----------------------------------------------------------------------
    // GetReflexionStatusAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetReflexionStatusAsync_ReturnsDelegatedResponse()
    {
        // Arrange
        var expected = new ReflexionStatusResponse
        {
            RecentResults = new List<ReflexionStatusEntry>
            {
                new() { ResultId = Guid.NewGuid(), IsHallucination = false, Confidence = 0.9, EvaluatedAt = DateTimeOffset.UtcNow }
            },
            HallucinationRate = 0.0,
            AverageConfidence = 0.9
        };

        _servicePortMock
            .Setup(s => s.GetReflexionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetReflexionStatusAsync(CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        _servicePortMock.Verify(s => s.GetReflexionStatusAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
