using CognitiveMesh.FoundationLayer.EvidenceArtifacts.Adapters;
using CognitiveMesh.FoundationLayer.EvidenceArtifacts.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.FoundationLayer.EvidenceArtifacts.Tests;

public class InMemoryEvidenceArtifactAdapterTests
{
    private readonly Mock<ILogger<InMemoryEvidenceArtifactAdapter>> _mockLogger;
    private readonly InMemoryEvidenceArtifactAdapter _adapter;

    public InMemoryEvidenceArtifactAdapterTests()
    {
        _mockLogger = new Mock<ILogger<InMemoryEvidenceArtifactAdapter>>();
        _adapter = new InMemoryEvidenceArtifactAdapter(_mockLogger.Object);
    }

    // ── Constructor ──────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new InMemoryEvidenceArtifactAdapter(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    // ── StoreArtifactAsync ───────────────────────────────────────────

    [Fact]
    public async Task StoreArtifactAsync_ValidArtifact_ReturnsStoredArtifact()
    {
        // Arrange
        var artifact = CreateSampleArtifact();

        // Act
        var result = await _adapter.StoreArtifactAsync(artifact, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ArtifactId.Should().NotBe(Guid.Empty);
        result.SourceType.Should().Be("AuditLog");
    }

    [Fact]
    public async Task StoreArtifactAsync_NullArtifact_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _adapter.StoreArtifactAsync(null!, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task StoreArtifactAsync_EmptyArtifactId_AssignsNewGuid()
    {
        // Arrange
        var artifact = CreateSampleArtifact();
        artifact.ArtifactId = Guid.Empty;

        // Act
        var result = await _adapter.StoreArtifactAsync(artifact, CancellationToken.None);

        // Assert
        result.ArtifactId.Should().NotBe(Guid.Empty);
    }

    // ── GetArtifactAsync ─────────────────────────────────────────────

    [Fact]
    public async Task GetArtifactAsync_ExistingId_ReturnsArtifact()
    {
        // Arrange
        var artifact = CreateSampleArtifact();
        var stored = await _adapter.StoreArtifactAsync(artifact, CancellationToken.None);

        // Act
        var result = await _adapter.GetArtifactAsync(stored.ArtifactId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.ArtifactId.Should().Be(stored.ArtifactId);
        result.SourceType.Should().Be("AuditLog");
    }

    [Fact]
    public async Task GetArtifactAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _adapter.GetArtifactAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    // ── SearchAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_FilterBySourceType_ReturnsMatchingArtifacts()
    {
        // Arrange
        var audit = CreateSampleArtifact();
        audit.SourceType = "AuditLog";
        var test = CreateSampleArtifact();
        test.SourceType = "TestResult";

        await _adapter.StoreArtifactAsync(audit, CancellationToken.None);
        await _adapter.StoreArtifactAsync(test, CancellationToken.None);

        // Act
        var results = await _adapter.SearchAsync(
            new ArtifactSearchCriteria { SourceType = "AuditLog" },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.SourceType.Should().Be("AuditLog");
    }

    [Fact]
    public async Task SearchAsync_FilterByCorrelationId_ReturnsMatchingArtifacts()
    {
        // Arrange
        var a1 = CreateSampleArtifact();
        a1.CorrelationId = "corr-001";
        var a2 = CreateSampleArtifact();
        a2.CorrelationId = "corr-002";

        await _adapter.StoreArtifactAsync(a1, CancellationToken.None);
        await _adapter.StoreArtifactAsync(a2, CancellationToken.None);

        // Act
        var results = await _adapter.SearchAsync(
            new ArtifactSearchCriteria { CorrelationId = "corr-001" },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.CorrelationId.Should().Be("corr-001");
    }

    [Fact]
    public async Task SearchAsync_FilterByTags_ReturnsArtifactsContainingAllTags()
    {
        // Arrange
        var a1 = CreateSampleArtifact();
        a1.Tags = new List<string> { "compliance", "gdpr", "audit" };
        var a2 = CreateSampleArtifact();
        a2.Tags = new List<string> { "compliance", "security" };

        await _adapter.StoreArtifactAsync(a1, CancellationToken.None);
        await _adapter.StoreArtifactAsync(a2, CancellationToken.None);

        // Act
        var results = await _adapter.SearchAsync(
            new ArtifactSearchCriteria { Tags = new List<string> { "compliance", "gdpr" } },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle()
            .Which.Tags.Should().Contain("gdpr");
    }

    [Fact]
    public async Task SearchAsync_FilterBySubmittedAfter_ReturnsMatchingArtifacts()
    {
        // Arrange
        var old = CreateSampleArtifact();
        old.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-30);
        var recent = CreateSampleArtifact();
        recent.SubmittedAt = DateTimeOffset.UtcNow.AddDays(-1);

        await _adapter.StoreArtifactAsync(old, CancellationToken.None);
        await _adapter.StoreArtifactAsync(recent, CancellationToken.None);

        // Act
        var results = await _adapter.SearchAsync(
            new ArtifactSearchCriteria { SubmittedAfter = DateTimeOffset.UtcNow.AddDays(-7) },
            CancellationToken.None);

        // Assert
        results.Should().ContainSingle();
    }

    [Fact]
    public async Task SearchAsync_MaxResults_LimitsOutput()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            await _adapter.StoreArtifactAsync(CreateSampleArtifact(), CancellationToken.None);
        }

        // Act
        var results = await _adapter.SearchAsync(
            new ArtifactSearchCriteria { MaxResults = 3 },
            CancellationToken.None);

        // Assert
        results.Should().HaveCount(3);
    }

    // ── DeleteExpiredAsync ───────────────────────────────────────────

    [Fact]
    public async Task DeleteExpiredAsync_SomeExpired_RemovesOnlyExpired()
    {
        // Arrange
        var expired1 = CreateSampleArtifact();
        expired1.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1);
        var expired2 = CreateSampleArtifact();
        expired2.ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1);
        var active = CreateSampleArtifact();
        active.ExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        var noExpiry = CreateSampleArtifact();
        noExpiry.ExpiresAt = null;

        await _adapter.StoreArtifactAsync(expired1, CancellationToken.None);
        await _adapter.StoreArtifactAsync(expired2, CancellationToken.None);
        await _adapter.StoreArtifactAsync(active, CancellationToken.None);
        await _adapter.StoreArtifactAsync(noExpiry, CancellationToken.None);

        // Act
        var deletedCount = await _adapter.DeleteExpiredAsync(CancellationToken.None);

        // Assert
        deletedCount.Should().Be(2);
        var remaining = await _adapter.GetArtifactCountAsync(CancellationToken.None);
        remaining.Should().Be(2);
    }

    [Fact]
    public async Task DeleteExpiredAsync_NoneExpired_ReturnsZero()
    {
        // Arrange
        var active = CreateSampleArtifact();
        active.ExpiresAt = DateTimeOffset.UtcNow.AddDays(30);
        await _adapter.StoreArtifactAsync(active, CancellationToken.None);

        // Act
        var deletedCount = await _adapter.DeleteExpiredAsync(CancellationToken.None);

        // Assert
        deletedCount.Should().Be(0);
    }

    // ── GetArtifactCountAsync ────────────────────────────────────────

    [Fact]
    public async Task GetArtifactCountAsync_EmptyStore_ReturnsZero()
    {
        // Act
        var count = await _adapter.GetArtifactCountAsync(CancellationToken.None);

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public async Task GetArtifactCountAsync_WithArtifacts_ReturnsCorrectCount()
    {
        // Arrange
        await _adapter.StoreArtifactAsync(CreateSampleArtifact(), CancellationToken.None);
        await _adapter.StoreArtifactAsync(CreateSampleArtifact(), CancellationToken.None);
        await _adapter.StoreArtifactAsync(CreateSampleArtifact(), CancellationToken.None);

        // Act
        var count = await _adapter.GetArtifactCountAsync(CancellationToken.None);

        // Assert
        count.Should().Be(3);
    }

    // ── Helper ───────────────────────────────────────────────────────

    private static EvidenceArtifact CreateSampleArtifact() => new()
    {
        SourceType = "AuditLog",
        Content = "Sample audit log content for compliance tracking.",
        SubmittedBy = "test-user",
        SubmittedAt = DateTimeOffset.UtcNow,
        CorrelationId = "corr-default",
        Tags = new List<string> { "compliance", "audit" },
        RetentionPolicy = RetentionPolicy.ThreeYears,
        ExpiresAt = DateTimeOffset.UtcNow.AddYears(3)
    };
}
