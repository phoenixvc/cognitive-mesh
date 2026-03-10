using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Configuration;
using AgencyLayer.AgentTeamFramework.Engines;
using AgencyLayer.AgentTeamFramework.Pipeline;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentTeamFramework.Tests;

public class AgentTeamEngineBaseTests
{
    private readonly Mock<ISpecializedAgentPort> _agentPortMock;
    private readonly Mock<ILogger<AgentPipelineExecutor>> _pipelineLoggerMock;
    private readonly Mock<ILogger<TestTeamEngine>> _loggerMock;
    private readonly AgentPipelineExecutor _pipeline;

    public AgentTeamEngineBaseTests()
    {
        _agentPortMock = new Mock<ISpecializedAgentPort>();
        _pipelineLoggerMock = new Mock<ILogger<AgentPipelineExecutor>>();
        _loggerMock = new Mock<ILogger<TestTeamEngine>>();
        _pipeline = new AgentPipelineExecutor(_agentPortMock.Object, _pipelineLoggerMock.Object);

        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    // ── Constructor ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullAgentPort_ThrowsArgumentNullException()
    {
        var act = () => new TestTeamEngine(null!, _pipeline, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("agentPort");
    }

    [Fact]
    public void Constructor_NullPipeline_ThrowsArgumentNullException()
    {
        var act = () => new TestTeamEngine(_agentPortMock.Object, null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("pipeline");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new TestTeamEngine(_agentPortMock.Object, _pipeline, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── Properties ──────────────────────────────────────────────────────

    [Fact]
    public void TeamId_ReturnsValueFromDerived()
    {
        var engine = CreateEngine();
        engine.TeamId.Should().Be("test-team");
    }

    [Fact]
    public void AgentCount_ReturnsNumberOfDefinedAgents()
    {
        var engine = CreateEngine();
        engine.AgentCount.Should().Be(3);
    }

    [Fact]
    public void IsInitialized_BeforeInit_ReturnsFalse()
    {
        var engine = CreateEngine();
        engine.IsInitialized.Should().BeFalse();
    }

    [Fact]
    public async Task IsInitialized_AfterInit_ReturnsTrue()
    {
        var engine = CreateEngine();
        await engine.InitializeAsync();
        engine.IsInitialized.Should().BeTrue();
    }

    // ── InitializeAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_RegistersAllAgents()
    {
        // Arrange
        var engine = CreateEngine();

        // Act
        await engine.InitializeAsync();

        // Assert
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task InitializeAsync_MapsAgentDefinitionToConfiguration()
    {
        // Arrange
        var engine = CreateEngine();
        SpecializedAgentConfiguration? captured = null;
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(
                It.Is<SpecializedAgentConfiguration>(c => c.AgentId == "test-alpha"),
                It.IsAny<CancellationToken>()))
            .Callback<SpecializedAgentConfiguration, CancellationToken>((config, _) => captured = config)
            .Returns(Task.CompletedTask);

        // Act
        await engine.InitializeAsync();

        // Assert — AgentDefinitionRecord fields mapped correctly
        captured.Should().NotBeNull();
        captured!.AgentId.Should().Be("test-alpha");
        captured.Name.Should().Be("Alpha");
        captured.Type.Should().Be(SpecializedAgentType.Analyst);
        captured.SystemPrompt.Should().Contain("You are Alpha");
        captured.Temperature.Should().Be(0.3);
        captured.Domains.Should().Contain("analysis");
        captured.Goals.Should().Contain("Analyze data");
    }

    [Fact]
    public async Task InitializeAsync_CalledTwice_RegistersOnlyOnce()
    {
        var engine = CreateEngine();

        await engine.InitializeAsync();
        await engine.InitializeAsync();

        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task InitializeAsync_ConcurrentCalls_RegistersExactlyOnce()
    {
        // Arrange — slow registration to widen race window
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns(async () => await Task.Delay(10));

        var engine = CreateEngine();

        // Act — 10 concurrent init calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => engine.InitializeAsync())
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert — still only 3 registrations
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task InitializeAsync_CancellationDuringRegistration_Throws()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()))
            .Returns<SpecializedAgentConfiguration, CancellationToken>(async (_, ct) =>
            {
                await cts.CancelAsync();
                ct.ThrowIfCancellationRequested();
            });

        var engine = CreateEngine();

        // Act & Assert
        await engine.Invoking(e => e.InitializeAsync(cts.Token))
            .Should().ThrowAsync<OperationCanceledException>();
    }

    // ── EnsureInitializedAsync ──────────────────────────────────────────

    [Fact]
    public async Task EnsureInitializedAsync_NotYetInitialized_InitializesAutomatically()
    {
        var engine = CreateEngine();
        engine.IsInitialized.Should().BeFalse();

        await engine.CallEnsureInitializedAsync();

        engine.IsInitialized.Should().BeTrue();
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task EnsureInitializedAsync_AlreadyInitialized_DoesNotReinitialize()
    {
        var engine = CreateEngine();
        await engine.InitializeAsync();

        await engine.CallEnsureInitializedAsync();

        // Still only 3 registrations (not 6)
        _agentPortMock.Verify(
            x => x.RegisterAgentAsync(It.IsAny<SpecializedAgentConfiguration>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    // ── Optional fields mapping ─────────────────────────────────────────

    [Fact]
    public async Task InitializeAsync_OptionalFields_MappedCorrectly()
    {
        // Arrange
        var engine = CreateEngine();
        SpecializedAgentConfiguration? captured = null;
        _agentPortMock
            .Setup(x => x.RegisterAgentAsync(
                It.Is<SpecializedAgentConfiguration>(c => c.AgentId == "test-gamma"),
                It.IsAny<CancellationToken>()))
            .Callback<SpecializedAgentConfiguration, CancellationToken>((config, _) => captured = config)
            .Returns(Task.CompletedTask);

        // Act
        await engine.InitializeAsync();

        // Assert — optional fields set on gamma agent
        captured.Should().NotBeNull();
        captured!.Model.Should().Be("gpt-4o");
        captured.MaxTokens.Should().Be(4096);
        captured.Tools.Should().Contain("code_interpreter");
        captured.Capabilities.Should().Contain("reasoning");
        captured.Backstory.Should().Be("A seasoned analyst");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private TestTeamEngine CreateEngine() =>
        new(_agentPortMock.Object, _pipeline, _loggerMock.Object);

    /// <summary>
    /// Concrete test implementation of AgentTeamEngineBase with 3 sample agents.
    /// </summary>
    public class TestTeamEngine : AgentTeamEngineBase
    {
        public TestTeamEngine(
            ISpecializedAgentPort agentPort,
            AgentPipelineExecutor pipeline,
            ILogger<TestTeamEngine> logger)
            : base(agentPort, pipeline, logger)
        {
        }

        public override string TeamId => "test-team";

        protected override IReadOnlyList<AgentDefinitionRecord> DefineAgents() =>
        [
            new AgentDefinitionRecord
            {
                AgentId = "test-alpha",
                Name = "Alpha",
                Type = SpecializedAgentType.Analyst,
                SystemPrompt = "You are Alpha, an analysis agent.",
                Temperature = 0.3,
                Domains = ["analysis"],
                Goals = ["Analyze data"]
            },
            new AgentDefinitionRecord
            {
                AgentId = "test-beta",
                Name = "Beta",
                Type = SpecializedAgentType.Writer,
                SystemPrompt = "You are Beta, a writing agent.",
                Temperature = 0.5,
                Domains = ["writing"],
                Goals = ["Produce reports"]
            },
            new AgentDefinitionRecord
            {
                AgentId = "test-gamma",
                Name = "Gamma",
                Type = SpecializedAgentType.Planner,
                SystemPrompt = "You are Gamma, a planning agent.",
                Temperature = 0.2,
                Domains = ["planning"],
                Goals = ["Create plans"],
                Model = "gpt-4o",
                MaxTokens = 4096,
                Tools = ["code_interpreter"],
                Capabilities = ["reasoning"],
                Backstory = "A seasoned analyst"
            }
        ];

        /// <summary>
        /// Exposes protected EnsureInitializedAsync for testing.
        /// </summary>
        public Task CallEnsureInitializedAsync(CancellationToken ct = default) =>
            EnsureInitializedAsync(ct);
    }
}
