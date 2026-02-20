using CognitiveMesh.FoundationLayer.NISTEvidence.Adapters;
using CognitiveMesh.FoundationLayer.NISTEvidence.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.FoundationLayer.NISTEvidence.Tests;

public class InMemoryNISTEvidenceAdapterTests
{
    private readonly Mock<ILogger<InMemoryNISTEvidenceAdapter>> _mockLogger;
    private readonly InMemoryNISTEvidenceAdapter _adapter;

    public InMemoryNISTEvidenceAdapterTests()
    {
        _mockLogger = new Mock<ILogger<InMemoryNISTEvidenceAdapter>>();
        _adapter = new InMemoryNISTEvidenceAdapter(_mockLogger.Object);
    }

    // ── Constructor ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new InMemoryNISTEvidenceAdapter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ── StoreAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task StoreAsync_ValidRecord_ReturnsStoredRecord()
    {
        // Arrange
        var record = CreateSampleRecord();

        // Act
        var result = await _adapter.StoreAsync(record, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.EvidenceId.Should().NotBe(Guid.Empty);
        result.StatementId.Should().Be("GV-1.1-001");
        result.AuditTrail.Should().ContainSingle()
            .Which.Action.Should().Be("Created");
    }

    [Fact]
    public async Task StoreAsync_NullRecord_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _adapter.StoreAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StoreAsync_EmptyEvidenceId_AssignsNewGuid()
    {
        // Arrange
        var record = CreateSampleRecord();
        record.EvidenceId = Guid.Empty;

        // Act
        var result = await _adapter.StoreAsync(record, CancellationToken.None);

        // Assert
        result.EvidenceId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task StoreAsync_ProvidedEvidenceId_PreservesId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var record = CreateSampleRecord();
        record.EvidenceId = expectedId;

        // Act
        var result = await _adapter.StoreAsync(record, CancellationToken.None);

        // Assert
        result.EvidenceId.Should().Be(expectedId);
    }

    [Fact]
    public async Task StoreAsync_DuplicateId_OverwritesPreviousRecord()
    {
        // Arrange
        var id = Guid.NewGuid();
        var first = CreateSampleRecord();
        first.EvidenceId = id;
        first.Content = "First version";

        var second = CreateSampleRecord();
        second.EvidenceId = id;
        second.Content = "Second version";

        // Act
        await _adapter.StoreAsync(first, CancellationToken.None);
        await _adapter.StoreAsync(second, CancellationToken.None);
        var result = await _adapter.GetByIdAsync(id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Content.Should().Be("Second version");
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsRecord()
    {
        // Arrange
        var record = CreateSampleRecord();
        var stored = await _adapter.StoreAsync(record, CancellationToken.None);

        // Act
        var result = await _adapter.GetByIdAsync(stored.EvidenceId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.EvidenceId.Should().Be(stored.EvidenceId);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _adapter.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ── GetByStatementAsync ──────────────────────────────────────────

    [Fact]
    public async Task GetByStatementAsync_MatchingStatement_ReturnsRecords()
    {
        // Arrange
        var record1 = CreateSampleRecord();
        record1.StatementId = "GV-1.1-001";
        var record2 = CreateSampleRecord();
        record2.StatementId = "GV-1.1-001";
        var record3 = CreateSampleRecord();
        record3.StatementId = "MAP-2.1-003";

        await _adapter.StoreAsync(record1, CancellationToken.None);
        await _adapter.StoreAsync(record2, CancellationToken.None);
        await _adapter.StoreAsync(record3, CancellationToken.None);

        // Act
        var results = await _adapter.GetByStatementAsync("GV-1.1-001", CancellationToken.None);

        // Assert
        results.Should().HaveCount(2);
        results.Should().OnlyContain(r => r.StatementId == "GV-1.1-001");
    }

    [Fact]
    public async Task GetByStatementAsync_NoMatch_ReturnsEmptyList()
    {
        // Act
        var results = await _adapter.GetByStatementAsync("NONEXISTENT", CancellationToken.None);

        // Assert
        results.Should().BeEmpty();
    }

    // ── QueryAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task QueryAsync_FilterByPillarId_ReturnsMatchingRecords()
    {
        // Arrange
        var record1 = CreateSampleRecord();
        record1.PillarId = "GOVERN";
        var record2 = CreateSampleRecord();
        record2.PillarId = "MAP";

        await _adapter.StoreAsync(record1, CancellationToken.None);
        await _adapter.StoreAsync(record2, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { PillarId = "GOVERN" },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.PillarId.Should().Be("GOVERN");
    }

    [Fact]
    public async Task QueryAsync_FilterByTopicId_ReturnsMatchingRecords()
    {
        // Arrange
        var record1 = CreateSampleRecord();
        record1.TopicId = "GV-1";
        var record2 = CreateSampleRecord();
        record2.TopicId = "MAP-2";

        await _adapter.StoreAsync(record1, CancellationToken.None);
        await _adapter.StoreAsync(record2, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { TopicId = "GV-1" },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.TopicId.Should().Be("GV-1");
    }

    [Fact]
    public async Task QueryAsync_FilterByReviewStatus_ReturnsMatchingRecords()
    {
        // Arrange
        var pending = CreateSampleRecord();
        pending.ReviewStatus = EvidenceReviewStatus.Pending;
        var approved = CreateSampleRecord();
        approved.ReviewStatus = EvidenceReviewStatus.Approved;

        await _adapter.StoreAsync(pending, CancellationToken.None);
        await _adapter.StoreAsync(approved, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { ReviewStatus = EvidenceReviewStatus.Approved },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.ReviewStatus.Should().Be(EvidenceReviewStatus.Approved);
    }

    [Fact]
    public async Task QueryAsync_FilterBySubmittedAfter_ReturnsMatchingRecords()
    {
        // Arrange
        var old = CreateSampleRecord();
        old.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-30);
        var recent = CreateSampleRecord();
        recent.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1);

        await _adapter.StoreAsync(old, CancellationToken.None);
        await _adapter.StoreAsync(recent, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { SubmittedAfter = DateTimeOffset.UtcNow.AddDays(-7) },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle();
    }

    [Fact]
    public async Task QueryAsync_FilterBySubmittedBefore_ReturnsMatchingRecords()
    {
        // Arrange
        var old = CreateSampleRecord();
        old.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-30);
        var recent = CreateSampleRecord();
        recent.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1);

        await _adapter.StoreAsync(old, CancellationToken.None);
        await _adapter.StoreAsync(recent, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { SubmittedBefore = DateTimeOffset.UtcNow.AddDays(-7) },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle();
    }

    [Fact]
    public async Task QueryAsync_MaxResults_LimitsOutput()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _adapter.StoreAsync(CreateSampleRecord(), CancellationToken.None);
        }

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter { MaxResults = 3 },
            CancellationToken.None);

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task QueryAsync_CombinedFilters_ReturnsMatchingRecords()
    {
        // Arrange
        var match = CreateSampleRecord();
        match.PillarId = "GOVERN";
        match.ReviewStatus = EvidenceReviewStatus.Approved;

        var noMatch1 = CreateSampleRecord();
        noMatch1.PillarId = "GOVERN";
        noMatch1.ReviewStatus = EvidenceReviewStatus.Pending;

        var noMatch2 = CreateSampleRecord();
        noMatch2.PillarId = "MAP";
        noMatch2.ReviewStatus = EvidenceReviewStatus.Approved;

        await _adapter.StoreAsync(match, CancellationToken.None);
        await _adapter.StoreAsync(noMatch1, CancellationToken.None);
        await _adapter.StoreAsync(noMatch2, CancellationToken.None);

        // Act
        var results = await _adapter.QueryAsync(
            new EvidenceQueryFilter
            {
                PillarId = "GOVERN",
                ReviewStatus = EvidenceReviewStatus.Approved
            },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.PillarId.Should().Be("GOVERN");
    }

    // ── UpdateReviewStatusAsync ──────────────────────────────────────

    [Fact]
    public async Task UpdateReviewStatusAsync_ExistingRecord_UpdatesStatus()
    {
        // Arrange
        var record = CreateSampleRecord();
        var stored = await _adapter.StoreAsync(record, CancellationToken.None);

        // Act
        var result = await _adapter.UpdateReviewStatusAsync(
            stored.EvidenceId,
            EvidenceReviewStatus.Approved,
            "reviewer-1",
            "Looks good.",
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ReviewStatus.Should().Be(EvidenceReviewStatus.Approved);
        result.ReviewerId.Should().Be("reviewer-1");
        result.ReviewerNotes.Should().Be("Looks good.");
        result.AuditTrail.Should().HaveCount(2);
        result.AuditTrail.Last().Action.Should().Be("StatusChanged");
    }

    [Fact]
    public async Task UpdateReviewStatusAsync_NonExistingRecord_ReturnsNull()
    {
        // Act
        var result = await _adapter.UpdateReviewStatusAsync(
            Guid.NewGuid(),
            EvidenceReviewStatus.Approved,
            "reviewer-1",
            null,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ── ArchiveAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ArchiveAsync_ExistingRecord_SetsIsArchivedAndReturnsTrue()
    {
        // Arrange
        var record = CreateSampleRecord();
        var stored = await _adapter.StoreAsync(record, CancellationToken.None);

        // Act
        var result = await _adapter.ArchiveAsync(stored.EvidenceId, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var archived = await _adapter.GetByIdAsync(stored.EvidenceId, CancellationToken.None);
        archived.Should().NotBeNull();
        archived!.IsArchived.Should().BeTrue();
        archived.AuditTrail.Should().HaveCount(2);
        archived.AuditTrail.Last().Action.Should().Be("Archived");
    }

    [Fact]
    public async Task ArchiveAsync_NonExistingRecord_ReturnsFalse()
    {
        // Act
        var result = await _adapter.ArchiveAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    // ── GetStatisticsAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetStatisticsAsync_EmptyRepository_ReturnsZeroStatistics()
    {
        // Act
        var stats = await _adapter.GetStatisticsAsync(CancellationToken.None);

        // Assert
        stats.TotalRecords.Should().Be(0);
        stats.PendingReviews.Should().Be(0);
        stats.ApprovedCount.Should().Be(0);
        stats.RejectedCount.Should().Be(0);
        stats.AverageFileSizeBytes.Should().Be(0);
        stats.ByPillar.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStatisticsAsync_PopulatedRepository_ReturnsCorrectAggregates()
    {
        // Arrange
        var r1 = CreateSampleRecord();
        r1.PillarId = "GOVERN";
        r1.ReviewStatus = EvidenceReviewStatus.Pending;
        r1.FileSizeBytes = 1000;

        var r2 = CreateSampleRecord();
        r2.PillarId = "GOVERN";
        r2.ReviewStatus = EvidenceReviewStatus.Approved;
        r2.FileSizeBytes = 2000;

        var r3 = CreateSampleRecord();
        r3.PillarId = "MAP";
        r3.ReviewStatus = EvidenceReviewStatus.Rejected;
        r3.FileSizeBytes = 3000;

        await _adapter.StoreAsync(r1, CancellationToken.None);
        await _adapter.StoreAsync(r2, CancellationToken.None);
        await _adapter.StoreAsync(r3, CancellationToken.None);

        // Act
        var stats = await _adapter.GetStatisticsAsync(CancellationToken.None);

        // Assert
        stats.TotalRecords.Should().Be(3);
        stats.PendingReviews.Should().Be(1);
        stats.ApprovedCount.Should().Be(1);
        stats.RejectedCount.Should().Be(1);
        stats.AverageFileSizeBytes.Should().Be(2000);
        stats.ByPillar.Should().HaveCount(2);
        stats.ByPillar["GOVERN"].Should().Be(2);
        stats.ByPillar["MAP"].Should().Be(1);
    }

    // ── Helper ───────────────────────────────────────────────────────

    private static NISTEvidenceRecord CreateSampleRecord() => new()
    {
        StatementId = "GV-1.1-001",
        TopicId = "GV-1",
        PillarId = "GOVERN",
        ArtifactType = "Policy",
        Content = "Sample evidence content for NIST AI RMF compliance.",
        SubmittedBy = "test-user",
        SubmittedAt = DateTimeOffset.UtcNow,
        Tags = new List<string> { "governance", "policy" },
        FileSizeBytes = 1024
    };
}
