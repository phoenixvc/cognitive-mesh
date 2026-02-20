using CognitiveMesh.BusinessApplications.NISTCompliance.Models;
using CognitiveMesh.BusinessApplications.NISTCompliance.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.BusinessApplications.NISTCompliance;

/// <summary>
/// Unit tests for <see cref="NISTComplianceService"/> covering evidence management,
/// checklist retrieval, scoring, review workflows, roadmap generation, and audit logging.
/// </summary>
public class NISTComplianceServiceTests
{
    private readonly Mock<ILogger<NISTComplianceService>> _loggerMock;
    private readonly NISTComplianceService _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="NISTComplianceServiceTests"/> class.
    /// </summary>
    public NISTComplianceServiceTests()
    {
        _loggerMock = new Mock<ILogger<NISTComplianceService>>();
        _sut = new NISTComplianceService(_loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new NISTComplianceService(null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // SubmitEvidenceAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitEvidenceAsync_ValidRequest_ReturnsResponseWithPendingStatus()
    {
        // Arrange
        var request = new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Policy document content",
            SubmittedBy = "user-1",
            Tags = new List<string> { "governance" },
            FileSizeBytes = 1024
        };

        // Act
        var result = await _sut.SubmitEvidenceAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.EvidenceId.Should().NotBe(Guid.Empty);
        result.StatementId.Should().Be("GOV-1");
        result.ReviewStatus.Should().Be("Pending");
        result.SubmittedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task SubmitEvidenceAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitEvidenceAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SubmitEvidenceAsync_MultipleSubmissions_CreatesDistinctIds()
    {
        var request1 = new NISTEvidenceRequest { StatementId = "GOV-1", ArtifactType = "Document", Content = "Content1", SubmittedBy = "user-1" };
        var request2 = new NISTEvidenceRequest { StatementId = "GOV-2", ArtifactType = "Screenshot", Content = "Content2", SubmittedBy = "user-2" };

        var result1 = await _sut.SubmitEvidenceAsync(request1);
        var result2 = await _sut.SubmitEvidenceAsync(request2);

        result1.EvidenceId.Should().NotBe(result2.EvidenceId);
    }

    // -----------------------------------------------------------------------
    // GetChecklistAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetChecklistAsync_ValidOrganization_Returns4PillarsWith12Statements()
    {
        var result = await _sut.GetChecklistAsync("org-1");

        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.Pillars.Should().HaveCount(4);
        result.TotalStatements.Should().Be(12);
        result.Pillars.Select(p => p.PillarId).Should().Contain(new[] { "GOVERN", "MAP", "MEASURE", "MANAGE" });
    }

    [Fact]
    public async Task GetChecklistAsync_NoEvidenceSubmitted_AllStatementsIncomplete()
    {
        var result = await _sut.GetChecklistAsync("org-1");

        result.CompletedStatements.Should().Be(0);
        result.Pillars.SelectMany(p => p.Statements).Should().AllSatisfy(s =>
        {
            s.IsComplete.Should().BeFalse();
            s.EvidenceCount.Should().Be(0);
        });
    }

    [Fact]
    public async Task GetChecklistAsync_WithApprovedEvidence_StatementsShowComplete()
    {
        // Submit and approve evidence
        var submitResult = await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Content",
            SubmittedBy = "user-1"
        });

        await _sut.SubmitReviewAsync(new NISTReviewRequest
        {
            EvidenceId = submitResult.EvidenceId,
            ReviewerId = "reviewer-1",
            Decision = "Approved"
        });

        // Act
        var result = await _sut.GetChecklistAsync("org-1");

        // Assert
        result.CompletedStatements.Should().Be(1);
        var gov1 = result.Pillars.SelectMany(p => p.Statements).First(s => s.StatementId == "GOV-1");
        gov1.IsComplete.Should().BeTrue();
        gov1.EvidenceCount.Should().Be(1);
    }

    [Fact]
    public async Task GetChecklistAsync_NullOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetChecklistAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // GetScoreAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetScoreAsync_NoEvidence_ReturnsZeroScores()
    {
        var result = await _sut.GetScoreAsync("org-1");

        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.OverallScore.Should().Be(0.0);
        result.PillarScores.Should().HaveCount(4);
        result.PillarScores.Should().AllSatisfy(p => p.AverageScore.Should().Be(0.0));
    }

    [Fact]
    public async Task GetScoreAsync_WithApprovedEvidence_ReturnsPositiveScore()
    {
        // Submit and approve evidence
        var submitResult = await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Content",
            SubmittedBy = "user-1"
        });

        await _sut.SubmitReviewAsync(new NISTReviewRequest
        {
            EvidenceId = submitResult.EvidenceId,
            ReviewerId = "reviewer-1",
            Decision = "Approved"
        });

        // Act
        var result = await _sut.GetScoreAsync("org-1");

        // Assert
        result.OverallScore.Should().BeGreaterThan(0.0);
        var governScore = result.PillarScores.First(p => p.PillarId == "GOVERN");
        governScore.AverageScore.Should().BeGreaterThan(0.0);
    }

    [Fact]
    public async Task GetScoreAsync_EmptyOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetScoreAsync("");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // -----------------------------------------------------------------------
    // SubmitReviewAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task SubmitReviewAsync_Approved_UpdatesStatus()
    {
        var submitResult = await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
        {
            StatementId = "MAP-1",
            ArtifactType = "AuditReport",
            Content = "Audit content",
            SubmittedBy = "user-1"
        });

        var reviewResult = await _sut.SubmitReviewAsync(new NISTReviewRequest
        {
            EvidenceId = submitResult.EvidenceId,
            ReviewerId = "reviewer-1",
            Decision = "Approved",
            Notes = "Well documented."
        });

        reviewResult.Should().NotBeNull();
        reviewResult.EvidenceId.Should().Be(submitResult.EvidenceId);
        reviewResult.NewStatus.Should().Be("Approved");
        reviewResult.ReviewedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task SubmitReviewAsync_Rejected_UpdatesStatus()
    {
        var submitResult = await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
        {
            StatementId = "MAP-2",
            ArtifactType = "Document",
            Content = "Insufficient content",
            SubmittedBy = "user-1"
        });

        var reviewResult = await _sut.SubmitReviewAsync(new NISTReviewRequest
        {
            EvidenceId = submitResult.EvidenceId,
            ReviewerId = "reviewer-1",
            Decision = "Rejected",
            Notes = "Needs more detail."
        });

        reviewResult.NewStatus.Should().Be("Rejected");
    }

    [Fact]
    public async Task SubmitReviewAsync_NonExistentEvidence_ThrowsKeyNotFoundException()
    {
        var request = new NISTReviewRequest
        {
            EvidenceId = Guid.NewGuid(),
            ReviewerId = "reviewer-1",
            Decision = "Approved"
        };

        var act = () => _sut.SubmitReviewAsync(request);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SubmitReviewAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _sut.SubmitReviewAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    // -----------------------------------------------------------------------
    // GetRoadmapAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetRoadmapAsync_NoEvidence_Returns12Gaps()
    {
        var result = await _sut.GetRoadmapAsync("org-1");

        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.Gaps.Should().HaveCount(12);
        result.Gaps.Should().AllSatisfy(g => g.CurrentScore.Should().Be(0));
        result.Gaps.Should().AllSatisfy(g => g.TargetScore.Should().Be(4));
        result.Gaps.Should().AllSatisfy(g => g.RecommendedActions.Should().NotBeEmpty());
    }

    [Fact]
    public async Task GetRoadmapAsync_WithApprovedEvidence_ReducesGapCount()
    {
        // Submit evidence and approve enough to score >= 4 for GOV-1
        for (var i = 0; i < 3; i++)
        {
            var submitResult = await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
            {
                StatementId = "GOV-1",
                ArtifactType = "Document",
                Content = $"Content {i}",
                SubmittedBy = "user-1"
            });

            await _sut.SubmitReviewAsync(new NISTReviewRequest
            {
                EvidenceId = submitResult.EvidenceId,
                ReviewerId = "reviewer-1",
                Decision = "Approved"
            });
        }

        var result = await _sut.GetRoadmapAsync("org-1");

        // GOV-1 should have score 4 (min(3+1, 5) = 4), so it should not be a gap
        result.Gaps.Should().HaveCountLessThan(12);
        result.Gaps.Should().NotContain(g => g.StatementId == "GOV-1");
    }

    [Fact]
    public async Task GetRoadmapAsync_GapPrioritiesAreCorrect()
    {
        var result = await _sut.GetRoadmapAsync("org-1");

        // All gaps should be "Critical" because all scores are 0
        result.Gaps.Should().AllSatisfy(g => g.Priority.Should().Be("Critical"));
    }

    // -----------------------------------------------------------------------
    // GetAuditLogAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAuditLogAsync_NoActivity_ReturnsEmptyLog()
    {
        var result = await _sut.GetAuditLogAsync("org-1", 100);

        result.Should().NotBeNull();
        result.OrganizationId.Should().Be("org-1");
        result.Entries.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetAuditLogAsync_AfterSubmission_ContainsEntry()
    {
        await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
        {
            StatementId = "GOV-1",
            ArtifactType = "Document",
            Content = "Content",
            SubmittedBy = "user-1"
        });

        // Audit entries are stored under "default-org" in the service
        var result = await _sut.GetAuditLogAsync("default-org", 100);

        result.Entries.Should().NotBeEmpty();
        result.Entries.Should().Contain(e => e.Action == "EvidenceSubmitted");
    }

    [Fact]
    public async Task GetAuditLogAsync_MaxResultsLimitsOutput()
    {
        // Submit multiple evidence items
        for (var i = 0; i < 5; i++)
        {
            await _sut.SubmitEvidenceAsync(new NISTEvidenceRequest
            {
                StatementId = $"GOV-{i + 1}",
                ArtifactType = "Document",
                Content = $"Content {i}",
                SubmittedBy = "user-1"
            });
        }

        var result = await _sut.GetAuditLogAsync("default-org", 2);

        result.Entries.Should().HaveCountLessOrEqualTo(2);
        result.TotalCount.Should().Be(5);
    }

    [Fact]
    public async Task GetAuditLogAsync_NullOrganizationId_ThrowsArgumentException()
    {
        var act = () => _sut.GetAuditLogAsync(null!, 50);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
