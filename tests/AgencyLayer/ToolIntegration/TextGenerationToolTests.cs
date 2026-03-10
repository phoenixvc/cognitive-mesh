using System;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgencyLayer.ToolIntegration;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI.Chat;
using Xunit;

/// <summary>
/// Tests for <see cref="TextGenerationTool"/> using Azure.AI.OpenAI v2 stable API.
/// Mocks <see cref="ChatClient"/> (which has virtual methods in v2) instead of the
/// old beta-era OpenAIClient.
/// </summary>
public class TextGenerationToolTests
{
    private readonly Mock<ILogger<TextGenerationTool>> _loggerMock;
    private readonly Mock<ChatClient> _chatClientMock;
    private readonly TextGenerationTool _textGenerationTool;

    public TextGenerationToolTests()
    {
        _loggerMock = new Mock<ILogger<TextGenerationTool>>();
        _chatClientMock = new Mock<ChatClient>();
        _textGenerationTool = new TextGenerationTool(_chatClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidPrompt_ReturnsGeneratedText()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "prompt", "test prompt" }
        };

        SetupChatCompletionResponse("Generated text");

        // Act
        var result = await _textGenerationTool.ExecuteAsync(parameters);

        // Assert
        Assert.Equal("Generated text", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingPrompt_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _textGenerationTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'prompt' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidPromptType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "prompt", 123 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _textGenerationTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'prompt' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "prompt", "test prompt" }
        };

        SetupChatCompletionResponse("Generated text");

        // Act
        await _textGenerationTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Text generation executed successfully for prompt: test prompt")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_LogsErrorOnException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "prompt", "test prompt" }
        };

        _chatClientMock
            .Setup(client => client.CompleteChatAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _textGenerationTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error executing text generation for prompt: test prompt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    /// <summary>
    /// Helper to set up the ChatClient mock to return a ChatCompletion with the given text.
    /// Uses OpenAIChatModelFactory to construct testable response objects.
    /// </summary>
    private void SetupChatCompletionResponse(string responseText)
    {
        var chatCompletion = OpenAIChatModelFactory.ChatCompletion(
            content: new ChatMessageContent
            {
                ChatMessageContentPart.CreateTextPart(responseText)
            });

        var pipelineResponse = new Mock<PipelineResponse>();
        var clientResult = ClientResult.FromValue(chatCompletion, pipelineResponse.Object);

        _chatClientMock
            .Setup(client => client.CompleteChatAsync(
                It.IsAny<IEnumerable<ChatMessage>>(),
                It.IsAny<ChatCompletionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(clientResult);
    }
}
