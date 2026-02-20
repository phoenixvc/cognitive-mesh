using CognitiveMesh.ReasoningLayer.NISTMaturity.Engines;
using CognitiveMesh.ReasoningLayer.NISTMaturity.Models;
using CognitiveMesh.ReasoningLayer.NISTMaturity.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.ReasoningLayer.Tests.NISTMaturity;

/// <summary>
/// Tests for <see cref="NISTMaturityAssessmentEngine"/>.
/// </summary>
public class NISTMaturityAssessmentEngineTests
{
    private readonly Mock<ILogger<NISTMaturityAssessmentEngine>> _loggerMock;
    private readonly Mock<INISTEvidenceStorePort> _evidenceStoreMock;
    private readonly NISTMaturityAssessmentEngine _engine;

    public NISTMaturityAssessmentEngineTests()
    {
        _loggerMock = new Mock<ILogger<NISTMaturityAssessmentEngine>>();
        _evidenceStoreMock = new Mock<INISTEvidenceStorePort>();
        _engine = new NISTMaturityAssessmentEngine(_loggerMock.Object, _evidenceStoreMock.Object);
    }

    // ─── Constructor null guard tests ─────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new NISTMaturityAssessmentEngine(null!, _evidenceStoreMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_NullEvidenceStore_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new NISTMaturityAssessmentEngine(_loggerMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("evidenceStore");
    }

    // ─── SubmitEvidence tests ─────────────────────────────────────────

    [Fact]
    public async Task SubmitEvidenceAsync_ValidEvidence_StoresAndReturns()
    {
        // Arrange
        var evidence = CreateEvidence("GOV-1.1", fileSizeBytes: 1024);

        // Act
        var result = await _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StatementId.Should().Be("GOV-1.1");
        _evidenceStoreMock.Verify(
            s => s.StoreEvidenceAsync(evidence, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SubmitEvidenceAsync_NullEvidence_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _engine.SubmitEvidenceAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SubmitEvidenceAsync_OversizedFile_ThrowsArgumentException()
    {
        // Arrange
        var evidence = CreateEvidence("GOV-1.1", fileSizeBytes: 10_485_761);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*exceeds maximum*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_ExactMaxFileSize_Succeeds()
    {
        // Arrange
        var evidence = CreateEvidence("GOV-1.1", fileSizeBytes: 10_485_760);

        // Act
        var result = await _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task SubmitEvidenceAsync_NegativeFileSize_ThrowsArgumentException()
    {
        // Arrange
        var evidence = CreateEvidence("GOV-1.1", fileSizeBytes: -1);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be negative*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_EmptyStatementId_ThrowsArgumentException()
    {
        // Arrange
        var evidence = CreateEvidence("", fileSizeBytes: 1024);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*StatementId*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_EmptyArtifactType_ThrowsArgumentException()
    {
        // Arrange
        var evidence = new NISTEvidence(
            EvidenceId: Guid.NewGuid(),
            StatementId: "GOV-1.1",
            ArtifactType: "",
            Content: "Some content",
            SubmittedBy: "user-1",
            SubmittedAt: DateTimeOffset.UtcNow,
            Tags: ["governance"],
            FileSizeBytes: 1024,
            ReviewerId: null,
            ReviewStatus: ReviewStatus.Pending);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*ArtifactType*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_EmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var evidence = new NISTEvidence(
            EvidenceId: Guid.NewGuid(),
            StatementId: "GOV-1.1",
            ArtifactType: "Document",
            Content: "",
            SubmittedBy: "user-1",
            SubmittedAt: DateTimeOffset.UtcNow,
            Tags: ["governance"],
            FileSizeBytes: 1024,
            ReviewerId: null,
            ReviewStatus: ReviewStatus.Pending);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Content*");
    }

    [Fact]
    public async Task SubmitEvidenceAsync_EmptySubmittedBy_ThrowsArgumentException()
    {
        // Arrange
        var evidence = new NISTEvidence(
            EvidenceId: Guid.NewGuid(),
            StatementId: "GOV-1.1",
            ArtifactType: "Document",
            Content: "Some content",
            SubmittedBy: "",
            SubmittedAt: DateTimeOffset.UtcNow,
            Tags: ["governance"],
            FileSizeBytes: 1024,
            ReviewerId: null,
            ReviewStatus: ReviewStatus.Pending);

        // Act
        var act = () => _engine.SubmitEvidenceAsync(evidence, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*SubmittedBy*");
    }

    // ─── ScoreStatement tests ─────────────────────────────────────────

    [Fact]
    public async Task ScoreStatementAsync_ZeroEvidence_ReturnsScore1()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 0);

        // Act
        var result = await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);

        // Assert
        result.Score.Should().Be(1);
        result.StatementId.Should().Be("GOV-1.1");
        result.Rationale.Should().Contain("No evidence");
    }

    [Fact]
    public async Task ScoreStatementAsync_OneEvidence_ReturnsScore2()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.2", 1);

        // Act
        var result = await _engine.ScoreStatementAsync("GOV-1.2", CancellationToken.None);

        // Assert
        result.Score.Should().Be(2);
        result.Rationale.Should().Contain("Limited evidence");
    }

    [Fact]
    public async Task ScoreStatementAsync_TwoEvidence_ReturnsScore2()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.3", 2);

        // Act
        var result = await _engine.ScoreStatementAsync("GOV-1.3", CancellationToken.None);

        // Assert
        result.Score.Should().Be(2);
    }

    [Fact]
    public async Task ScoreStatementAsync_ThreeEvidence_ReturnsScore3()
    {
        // Arrange
        SetupEvidenceCount("MAP-1.1", 3);

        // Act
        var result = await _engine.ScoreStatementAsync("MAP-1.1", CancellationToken.None);

        // Assert
        result.Score.Should().Be(3);
        result.Rationale.Should().Contain("Moderate evidence");
    }

    [Fact]
    public async Task ScoreStatementAsync_FourEvidence_ReturnsScore3()
    {
        // Arrange
        SetupEvidenceCount("MAP-1.2", 4);

        // Act
        var result = await _engine.ScoreStatementAsync("MAP-1.2", CancellationToken.None);

        // Assert
        result.Score.Should().Be(3);
    }

    [Fact]
    public async Task ScoreStatementAsync_FiveEvidence_ReturnsScore4()
    {
        // Arrange
        SetupEvidenceCount("MEA-1.1", 5);

        // Act
        var result = await _engine.ScoreStatementAsync("MEA-1.1", CancellationToken.None);

        // Assert
        result.Score.Should().Be(4);
        result.Rationale.Should().Contain("Strong evidence");
    }

    [Fact]
    public async Task ScoreStatementAsync_SevenEvidence_ReturnsScore4()
    {
        // Arrange
        SetupEvidenceCount("MEA-1.2", 7);

        // Act
        var result = await _engine.ScoreStatementAsync("MEA-1.2", CancellationToken.None);

        // Assert
        result.Score.Should().Be(4);
    }

    [Fact]
    public async Task ScoreStatementAsync_EightEvidence_ReturnsScore5()
    {
        // Arrange
        SetupEvidenceCount("MAN-1.1", 8);

        // Act
        var result = await _engine.ScoreStatementAsync("MAN-1.1", CancellationToken.None);

        // Assert
        result.Score.Should().Be(5);
        result.Rationale.Should().Contain("Comprehensive evidence");
    }

    [Fact]
    public async Task ScoreStatementAsync_TenEvidence_ReturnsScore5()
    {
        // Arrange
        SetupEvidenceCount("MAN-1.2", 10);

        // Act
        var result = await _engine.ScoreStatementAsync("MAN-1.2", CancellationToken.None);

        // Assert
        result.Score.Should().Be(5);
    }

    [Fact]
    public async Task ScoreStatementAsync_EmptyStatementId_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.ScoreStatementAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*StatementId*");
    }

    [Fact]
    public async Task ScoreStatementAsync_SetsCorrectScoredBy()
    {
        // Arrange
        SetupEvidenceCount("GOV-2.1", 3);

        // Act
        var result = await _engine.ScoreStatementAsync("GOV-2.1", CancellationToken.None);

        // Assert
        result.ScoredBy.Should().Be("NISTMaturityAssessmentEngine");
        result.ScoredAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ─── GetPillarScores tests ────────────────────────────────────────

    [Fact]
    public async Task GetPillarScoresAsync_NoScores_ReturnsFourPillarsWithZero()
    {
        // Act
        var result = await _engine.GetPillarScoresAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().HaveCount(4);
        result.Should().Contain(p => p.PillarId == "GOV" && p.PillarName == "Govern");
        result.Should().Contain(p => p.PillarId == "MAP" && p.PillarName == "Map");
        result.Should().Contain(p => p.PillarId == "MEA" && p.PillarName == "Measure");
        result.Should().Contain(p => p.PillarId == "MAN" && p.PillarName == "Manage");
        result.Should().OnlyContain(p => p.AverageScore == 0.0);
    }

    [Fact]
    public async Task GetPillarScoresAsync_WithScores_ReturnsCorrectAverages()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 3);
        SetupEvidenceCount("GOV-1.2", 8);
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("GOV-1.2", CancellationToken.None);

        // Act
        var result = await _engine.GetPillarScoresAsync("org-1", CancellationToken.None);

        // Assert
        var govPillar = result.First(p => p.PillarId == "GOV");
        govPillar.StatementCount.Should().Be(2);
        govPillar.AverageScore.Should().Be(4.0); // (3 + 5) / 2 = 4.0
    }

    [Fact]
    public async Task GetPillarScoresAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.GetPillarScoresAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*OrganizationId*");
    }

    // ─── GenerateRoadmap tests ────────────────────────────────────────

    [Fact]
    public async Task GenerateRoadmapAsync_WithGaps_ReturnsGapsSortedByPriority()
    {
        // Arrange - Score 1 is critical, Score 2 is high, Score 3 is medium
        SetupEvidenceCount("GOV-1.1", 0); // score 1 -> Critical
        SetupEvidenceCount("MAP-1.1", 1); // score 2 -> High
        SetupEvidenceCount("MEA-1.1", 3); // score 3 -> Medium
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("MAP-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("MEA-1.1", CancellationToken.None);

        // Act
        var result = await _engine.GenerateRoadmapAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.Gaps.Should().HaveCount(3);
        result.Gaps[0].Priority.Should().Be(GapPriority.Critical);
        result.Gaps[1].Priority.Should().Be(GapPriority.High);
        result.Gaps[2].Priority.Should().Be(GapPriority.Medium);
    }

    [Fact]
    public async Task GenerateRoadmapAsync_NoGaps_ReturnsEmptyGapList()
    {
        // Arrange - All scores >= 4
        SetupEvidenceCount("GOV-1.1", 5); // score 4
        SetupEvidenceCount("MAP-1.1", 8); // score 5
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("MAP-1.1", CancellationToken.None);

        // Act
        var result = await _engine.GenerateRoadmapAsync("org-1", CancellationToken.None);

        // Assert
        result.Gaps.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateRoadmapAsync_CriticalGap_HasRecommendedActions()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 0); // score 1 -> Critical
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);

        // Act
        var result = await _engine.GenerateRoadmapAsync("org-1", CancellationToken.None);

        // Assert
        var criticalGap = result.Gaps.First(g => g.StatementId == "GOV-1.1");
        criticalGap.CurrentScore.Should().Be(1);
        criticalGap.TargetScore.Should().Be(4);
        criticalGap.RecommendedActions.Should().NotBeEmpty();
        criticalGap.RecommendedActions.Should().Contain(a => a.Contains("GOV-1.1"));
    }

    [Fact]
    public async Task GenerateRoadmapAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.GenerateRoadmapAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*OrganizationId*");
    }

    [Fact]
    public async Task GenerateRoadmapAsync_GeneratesUniqueRoadmapId()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 0);
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);

        // Act
        var result1 = await _engine.GenerateRoadmapAsync("org-1", CancellationToken.None);
        var result2 = await _engine.GenerateRoadmapAsync("org-1", CancellationToken.None);

        // Assert
        result1.RoadmapId.Should().NotBe(result2.RoadmapId);
    }

    // ─── GetAssessment tests ──────────────────────────────────────────

    [Fact]
    public async Task GetAssessmentAsync_WithScores_CompilesCorrectAssessment()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 3); // score 3
        SetupEvidenceCount("MAP-1.1", 5); // score 4
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("MAP-1.1", CancellationToken.None);

        // Act
        var result = await _engine.GetAssessmentAsync("org-1", CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.OverallScore.Should().Be(3.5); // (3 + 4) / 2
        result.StatementScores.Should().HaveCount(2);
        result.PillarScores.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetAssessmentAsync_NoScores_ReturnsZeroOverallScore()
    {
        // Act
        var result = await _engine.GetAssessmentAsync("org-1", CancellationToken.None);

        // Assert
        result.OverallScore.Should().Be(0.0);
        result.StatementScores.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAssessmentAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        // Act
        var act = () => _engine.GetAssessmentAsync("", CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*OrganizationId*");
    }

    [Fact]
    public async Task GetAssessmentAsync_GeneratesUniqueAssessmentId()
    {
        // Act
        var result1 = await _engine.GetAssessmentAsync("org-1", CancellationToken.None);
        var result2 = await _engine.GetAssessmentAsync("org-1", CancellationToken.None);

        // Assert
        result1.AssessmentId.Should().NotBe(result2.AssessmentId);
    }

    [Fact]
    public async Task GetAssessmentAsync_PillarScoresMatchStatements()
    {
        // Arrange
        SetupEvidenceCount("GOV-1.1", 3); // score 3
        SetupEvidenceCount("GOV-2.1", 8); // score 5
        SetupEvidenceCount("MAP-1.1", 1); // score 2
        await _engine.ScoreStatementAsync("GOV-1.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("GOV-2.1", CancellationToken.None);
        await _engine.ScoreStatementAsync("MAP-1.1", CancellationToken.None);

        // Act
        var result = await _engine.GetAssessmentAsync("org-1", CancellationToken.None);

        // Assert
        var govPillar = result.PillarScores.First(p => p.PillarId == "GOV");
        govPillar.StatementCount.Should().Be(2);
        govPillar.AverageScore.Should().Be(4.0); // (3 + 5) / 2

        var mapPillar = result.PillarScores.First(p => p.PillarId == "MAP");
        mapPillar.StatementCount.Should().Be(1);
        mapPillar.AverageScore.Should().Be(2.0);
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    private NISTEvidence CreateEvidence(string statementId, long fileSizeBytes)
    {
        return new NISTEvidence(
            EvidenceId: Guid.NewGuid(),
            StatementId: statementId,
            ArtifactType: "Document",
            Content: "Test evidence content",
            SubmittedBy: "test-user",
            SubmittedAt: DateTimeOffset.UtcNow,
            Tags: ["test"],
            FileSizeBytes: fileSizeBytes,
            ReviewerId: null,
            ReviewStatus: ReviewStatus.Pending);
    }

    private void SetupEvidenceCount(string statementId, int count)
    {
        var evidenceList = new List<NISTEvidence>();
        for (var i = 0; i < count; i++)
        {
            evidenceList.Add(CreateEvidence(statementId, 1024));
        }

        _evidenceStoreMock
            .Setup(s => s.GetEvidenceForStatementAsync(statementId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(evidenceList.AsReadOnly());
    }
}
