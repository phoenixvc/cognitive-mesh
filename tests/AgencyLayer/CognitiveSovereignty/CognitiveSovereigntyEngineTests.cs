using AgencyLayer.CognitiveSovereignty.Engines;
using AgencyLayer.CognitiveSovereignty.Models;
using AgencyLayer.CognitiveSovereignty.Ports;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CognitiveMesh.Tests.AgencyLayer.CognitiveSovereignty;

/// <summary>
/// Unit tests for <see cref="CognitiveSovereigntyEngine"/>, covering profile management,
/// mode transitions, override resolution, autonomy level calculation, and constructor guards.
/// </summary>
public class CognitiveSovereigntyEngineTests
{
    private readonly Mock<ISovereigntyOverridePort> _overridePortMock;
    private readonly Mock<ILogger<CognitiveSovereigntyEngine>> _loggerMock;
    private readonly CognitiveSovereigntyEngine _sut;

    public CognitiveSovereigntyEngineTests()
    {
        _overridePortMock = new Mock<ISovereigntyOverridePort>();
        _loggerMock = new Mock<ILogger<CognitiveSovereigntyEngine>>();

        // Default: no active overrides
        _overridePortMock
            .Setup(x => x.GetActiveOverridesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SovereigntyOverride>());

        _sut = new CognitiveSovereigntyEngine(
            _overridePortMock.Object,
            _loggerMock.Object);
    }

    // -----------------------------------------------------------------------
    // Constructor null-guard tests
    // -----------------------------------------------------------------------

    [Fact]
    public void Constructor_NullOverridePort_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSovereigntyEngine(
            null!,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("overridePort");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new CognitiveSovereigntyEngine(
            _overridePortMock.Object,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // -----------------------------------------------------------------------
    // Profile CRUD tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_NoProfile_ReturnsNull()
    {
        var profile = await _sut.GetProfileAsync("user-1");

        profile.Should().BeNull();
    }

    [Fact]
    public async Task SetModeAsync_NewUser_CreatesProfileWithMode()
    {
        var profile = await _sut.SetModeAsync("user-1", SovereigntyMode.FullAutonomy);

        profile.Should().NotBeNull();
        profile.UserId.Should().Be("user-1");
        profile.DefaultMode.Should().Be(SovereigntyMode.FullAutonomy);
    }

    [Fact]
    public async Task GetProfileAsync_AfterSetMode_ReturnsProfile()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.CoAuthorship);

        var profile = await _sut.GetProfileAsync("user-1");

        profile.Should().NotBeNull();
        profile!.UserId.Should().Be("user-1");
        profile.DefaultMode.Should().Be(SovereigntyMode.CoAuthorship);
    }

    [Fact]
    public async Task UpdateProfileAsync_ExistingProfile_UpdatesAndReturns()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);
        var profile = await _sut.GetProfileAsync("user-1");
        profile!.DefaultMode = SovereigntyMode.HumanLed;

        var updated = await _sut.UpdateProfileAsync(profile);

        updated.DefaultMode.Should().Be(SovereigntyMode.HumanLed);
        updated.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateProfileAsync_NonExistentProfile_ThrowsInvalidOperationException()
    {
        var profile = new SovereigntyProfile { UserId = "non-existent" };

        var act = () => _sut.UpdateProfileAsync(profile);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // -----------------------------------------------------------------------
    // Mode transition tests (all 5 modes)
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(SovereigntyMode.FullAutonomy)]
    [InlineData(SovereigntyMode.GuidedAutonomy)]
    [InlineData(SovereigntyMode.CoAuthorship)]
    [InlineData(SovereigntyMode.HumanLed)]
    [InlineData(SovereigntyMode.FullManual)]
    public async Task SetModeAsync_AllModes_SetsCorrectly(SovereigntyMode mode)
    {
        var profile = await _sut.SetModeAsync("user-1", mode);

        profile.DefaultMode.Should().Be(mode);

        var currentMode = await _sut.GetCurrentModeAsync("user-1");
        currentMode.Should().Be(mode);
    }

    [Fact]
    public async Task SetModeAsync_WithDomain_SetsDomainOverride()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);
        await _sut.SetModeAsync("user-1", SovereigntyMode.FullManual, domain: "financial");

        var profile = await _sut.GetProfileAsync("user-1");
        profile!.DomainOverrides.Should().ContainKey("financial");
        profile.DomainOverrides["financial"].Should().Be(SovereigntyMode.FullManual);
        profile.DefaultMode.Should().Be(SovereigntyMode.GuidedAutonomy, "default mode should not change");
    }

    [Fact]
    public async Task SetModeAsync_MultipleDomains_SetsEachIndependently()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);
        await _sut.SetModeAsync("user-1", SovereigntyMode.FullManual, domain: "financial");
        await _sut.SetModeAsync("user-1", SovereigntyMode.FullAutonomy, domain: "routine");

        var profile = await _sut.GetProfileAsync("user-1");
        profile!.DomainOverrides["financial"].Should().Be(SovereigntyMode.FullManual);
        profile.DomainOverrides["routine"].Should().Be(SovereigntyMode.FullAutonomy);
        profile.DefaultMode.Should().Be(SovereigntyMode.GuidedAutonomy);
    }

    // -----------------------------------------------------------------------
    // Mode resolution tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentModeAsync_NoProfile_ReturnsGuidedAutonomy()
    {
        var mode = await _sut.GetCurrentModeAsync("unknown-user");

        mode.Should().Be(SovereigntyMode.GuidedAutonomy);
    }

    [Fact]
    public async Task GetCurrentModeAsync_WithProfile_ReturnsDefaultMode()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.HumanLed);

        var mode = await _sut.GetCurrentModeAsync("user-1");

        mode.Should().Be(SovereigntyMode.HumanLed);
    }

    [Fact]
    public async Task GetCurrentModeAsync_WithDomainOverride_ReturnsDomainMode()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);
        await _sut.SetModeAsync("user-1", SovereigntyMode.FullManual, domain: "financial");

        var mode = await _sut.GetCurrentModeAsync("user-1", domain: "financial");

        mode.Should().Be(SovereigntyMode.FullManual);
    }

    [Fact]
    public async Task GetCurrentModeAsync_WithUnknownDomain_FallsBackToDefault()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.CoAuthorship);

        var mode = await _sut.GetCurrentModeAsync("user-1", domain: "unknown-domain");

        mode.Should().Be(SovereigntyMode.CoAuthorship);
    }

    [Fact]
    public async Task GetCurrentModeAsync_WithActiveOverride_ReturnsOverrideMode()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);

        var activeOverride = new SovereigntyOverride
        {
            OverrideId = "override-1",
            UserId = "user-1",
            PreviousMode = SovereigntyMode.GuidedAutonomy,
            NewMode = SovereigntyMode.FullManual,
            Reason = "Emergency lockdown",
            Expiry = DateTime.UtcNow.AddHours(1)
        };

        _overridePortMock
            .Setup(x => x.GetActiveOverridesAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SovereigntyOverride> { activeOverride });

        var mode = await _sut.GetCurrentModeAsync("user-1");

        mode.Should().Be(SovereigntyMode.FullManual, "active override should take priority over profile default");
    }

    [Fact]
    public async Task GetCurrentModeAsync_OverrideTakesPriorityOverDomain()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.GuidedAutonomy);
        await _sut.SetModeAsync("user-1", SovereigntyMode.FullAutonomy, domain: "routine");

        var activeOverride = new SovereigntyOverride
        {
            OverrideId = "override-1",
            UserId = "user-1",
            PreviousMode = SovereigntyMode.GuidedAutonomy,
            NewMode = SovereigntyMode.HumanLed,
            Reason = "Policy enforcement"
        };

        _overridePortMock
            .Setup(x => x.GetActiveOverridesAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SovereigntyOverride> { activeOverride });

        var mode = await _sut.GetCurrentModeAsync("user-1", domain: "routine");

        mode.Should().Be(SovereigntyMode.HumanLed, "override should take priority even over domain-specific mode");
    }

    // -----------------------------------------------------------------------
    // Autonomy level calculation tests
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(SovereigntyMode.FullAutonomy, 1.0)]
    [InlineData(SovereigntyMode.GuidedAutonomy, 0.75)]
    [InlineData(SovereigntyMode.CoAuthorship, 0.5)]
    [InlineData(SovereigntyMode.HumanLed, 0.25)]
    [InlineData(SovereigntyMode.FullManual, 0.0)]
    public void CalculateAutonomyLevel_AllModes_ReturnsCorrectLevel(SovereigntyMode mode, double expectedLevel)
    {
        var level = CognitiveSovereigntyEngine.CalculateAutonomyLevel(mode);

        level.Should().Be(expectedLevel);
    }

    [Fact]
    public void CalculateAutonomyLevel_InvalidMode_ThrowsArgumentOutOfRangeException()
    {
        var act = () => CognitiveSovereigntyEngine.CalculateAutonomyLevel((SovereigntyMode)999);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    // -----------------------------------------------------------------------
    // Expired override handling tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetCurrentModeAsync_NoActiveOverrides_FallsBackToProfile()
    {
        await _sut.SetModeAsync("user-1", SovereigntyMode.CoAuthorship);

        // Empty overrides list simulates all overrides being expired/revoked
        _overridePortMock
            .Setup(x => x.GetActiveOverridesAsync("user-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<SovereigntyOverride>());

        var mode = await _sut.GetCurrentModeAsync("user-1");

        mode.Should().Be(SovereigntyMode.CoAuthorship, "with no active overrides, should fall back to profile default");
    }

    // -----------------------------------------------------------------------
    // Argument validation tests
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetProfileAsync_NullUserId_ThrowsArgumentException()
    {
        var act = () => _sut.GetProfileAsync(null!);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task SetModeAsync_EmptyUserId_ThrowsArgumentException()
    {
        var act = () => _sut.SetModeAsync("", SovereigntyMode.FullAutonomy);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCurrentModeAsync_WhitespaceUserId_ThrowsArgumentException()
    {
        var act = () => _sut.GetCurrentModeAsync("   ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task UpdateProfileAsync_NullProfile_ThrowsArgumentNullException()
    {
        var act = () => _sut.UpdateProfileAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
