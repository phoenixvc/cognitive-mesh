using System.Text.Json;
using AgencyLayer.Agents.Ports;
using AgencyLayer.AgentTeamFramework.Pipeline;
using AgencyLayer.AgentTeamFramework.Serialization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgentTeamFramework.Tests;

public class AgentPipelineExecutorTests
{
    private readonly Mock<ISpecializedAgentPort> _agentPortMock;
    private readonly Mock<ILogger<AgentPipelineExecutor>> _loggerMock;
    private readonly AgentPipelineExecutor _executor;

    public AgentPipelineExecutorTests()
    {
        _agentPortMock = new Mock<ISpecializedAgentPort>();
        _loggerMock = new Mock<ILogger<AgentPipelineExecutor>>();
        _executor = new AgentPipelineExecutor(_agentPortMock.Object, _loggerMock.Object);
    }

    // ── Constructor ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NullAgentPort_ThrowsArgumentNullException()
    {
        var act = () => new AgentPipelineExecutor(null!, _loggerMock.Object);
        act.Should().Throw<ArgumentNullException>().WithParameterName("agentPort");
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        var act = () => new AgentPipelineExecutor(_agentPortMock.Object, null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    // ── ExecuteStepAsync<T> (object context) ────────────────────────────

    [Fact]
    public async Task ExecuteStepAsync_ValidResponse_DeserializesOutput()
    {
        // Arrange
        var expected = new SampleOutput { Name = "test", Value = 42 };
        SetupAgentSuccess("agent-1", expected);

        // Act
        var result = await _executor.ExecuteStepAsync<SampleOutput>(
            "agent-1", "Do something", new { input = "data" });

        // Assert
        result.Name.Should().Be("test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task ExecuteStepAsync_SerializesContextAsCamelCase()
    {
        // Arrange
        SpecializedTask? capturedTask = null;
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .Callback<string, SpecializedTask, CancellationToken>((_, task, _) => capturedTask = task)
            .ReturnsAsync(CreateSuccessResult("agent-1", new SampleOutput()));

        // Act
        await _executor.ExecuteStepAsync<SampleOutput>(
            "agent-1", "Test", new { MyProperty = "hello", NestedObject = new { InnerValue = 5 } });

        // Assert — context should be camelCase JSON
        capturedTask.Should().NotBeNull();
        capturedTask!.Context.Should().Contain("\"myProperty\"");
        capturedTask.Context.Should().Contain("\"nestedObject\"");
        capturedTask.Context.Should().Contain("\"innerValue\"");
        capturedTask.Context.Should().NotContain("\"MyProperty\"");
    }

    [Fact]
    public async Task ExecuteStepAsync_AgentFails_ThrowsInvalidOperationException()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = false,
                ErrorMessage = "Model overloaded"
            });

        // Act & Assert
        await _executor.Invoking(e => e.ExecuteStepAsync<SampleOutput>("agent-1", "Fail test", new { }))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*agent-1*Model overloaded*");
    }

    [Fact]
    public async Task ExecuteStepAsync_EmptyOutput_ReturnsDefault()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = true,
                Output = ""
            });

        // Act
        var result = await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Empty test", new { });

        // Assert — returns default instance
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
        result.Value.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteStepAsync_MalformedJson_ReturnsDefault()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = true,
                Output = "not json {{{garbage"
            });

        // Act
        var result = await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Malformed test", new { });

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
    }

    // ── ExecuteStepAsync<T> (string context) ────────────────────────────

    [Fact]
    public async Task ExecuteStepAsync_StringContext_PassesThroughWithoutReserializing()
    {
        // Arrange
        SpecializedTask? capturedTask = null;
        var rawContext = """{"already":"serialized"}""";
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .Callback<string, SpecializedTask, CancellationToken>((_, task, _) => capturedTask = task)
            .ReturnsAsync(CreateSuccessResult("agent-1", new SampleOutput()));

        // Act
        await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Test", rawContext);

        // Assert — context passed as-is
        capturedTask.Should().NotBeNull();
        capturedTask!.Context.Should().Be(rawContext);
    }

    // ── ExecuteRawAsync ─────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteRawAsync_ReturnsRawResult()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = true,
                Output = "raw output text",
                TokensUsed = 123
            });

        // Act
        var result = await _executor.ExecuteRawAsync("agent-1", "Raw test", new { });

        // Assert
        result.Output.Should().Be("raw output text");
        result.TokensUsed.Should().Be(123);
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteRawAsync_AgentFails_ThrowsInvalidOperationException()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = false,
                ErrorMessage = "Timeout"
            });

        // Act & Assert
        await _executor.Invoking(e => e.ExecuteRawAsync("agent-1", "Fail test", new { }))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Timeout*");
    }

    // ── DeserializeOrDefault ────────────────────────────────────────────

    [Fact]
    public void DeserializeOrDefault_ValidJson_ReturnsDeserialized()
    {
        var json = """{"name":"hello","value":99}""";
        var result = _executor.DeserializeOrDefault<SampleOutput>(json, "SampleOutput");
        result.Name.Should().Be("hello");
        result.Value.Should().Be(99);
    }

    [Fact]
    public void DeserializeOrDefault_NullJson_ReturnsDefault()
    {
        var result = _executor.DeserializeOrDefault<SampleOutput>(null, "SampleOutput");
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
    }

    [Fact]
    public void DeserializeOrDefault_EmptyString_ReturnsDefault()
    {
        var result = _executor.DeserializeOrDefault<SampleOutput>("", "SampleOutput");
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
    }

    [Fact]
    public void DeserializeOrDefault_WhitespaceOnly_ReturnsDefault()
    {
        var result = _executor.DeserializeOrDefault<SampleOutput>("   \n\t  ", "SampleOutput");
        result.Should().NotBeNull();
    }

    [Fact]
    public void DeserializeOrDefault_InvalidJson_ReturnsDefault()
    {
        var result = _executor.DeserializeOrDefault<SampleOutput>("{bad json!", "SampleOutput");
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
    }

    [Fact]
    public void DeserializeOrDefault_WrongSchema_ReturnsDefaultWithoutThrowing()
    {
        var json = """{"completelyDifferent": true, "count": 5}""";
        var result = _executor.DeserializeOrDefault<SampleOutput>(json, "SampleOutput");
        result.Should().NotBeNull();
        result.Name.Should().BeNull();
        result.Value.Should().Be(0);
    }

    // ── Logging ─────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteStepAsync_Success_LogsTokenUsage()
    {
        // Arrange
        SetupAgentSuccess("agent-1", new SampleOutput(), tokensUsed: 750);

        // Act
        await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Test", new { });

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("tokens used")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteStepAsync_Failure_LogsError()
    {
        // Arrange
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpecializedTaskResult
            {
                TaskId = "t1",
                AgentId = "agent-1",
                Success = false,
                ErrorMessage = "Something broke"
            });

        // Act
        try { await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Test", new { }); }
        catch (InvalidOperationException) { /* expected */ }

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains("failed")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // ── Cancellation ────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteStepAsync_CancellationToken_PassedToAgent()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        CancellationToken capturedToken = default;
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync("agent-1", It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .Callback<string, SpecializedTask, CancellationToken>((_, _, ct) => capturedToken = ct)
            .ReturnsAsync(CreateSuccessResult("agent-1", new SampleOutput()));

        // Act
        await _executor.ExecuteStepAsync<SampleOutput>("agent-1", "Test", new { }, cts.Token);

        // Assert
        capturedToken.Should().Be(cts.Token);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private void SetupAgentSuccess<T>(string agentId, T output, int tokensUsed = 500)
    {
        _agentPortMock
            .Setup(x => x.ExecuteTaskAsync(agentId, It.IsAny<SpecializedTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessResult(agentId, output, tokensUsed));
    }

    private static SpecializedTaskResult CreateSuccessResult<T>(string agentId, T output, int tokensUsed = 500) => new()
    {
        TaskId = Guid.NewGuid().ToString(),
        AgentId = agentId,
        Success = true,
        Output = JsonSerializer.Serialize(output, AgentJsonDefaults.CamelCaseIndented),
        TokensUsed = tokensUsed
    };

    public class SampleOutput
    {
        public string? Name { get; init; }
        public int Value { get; init; }
    }
}
