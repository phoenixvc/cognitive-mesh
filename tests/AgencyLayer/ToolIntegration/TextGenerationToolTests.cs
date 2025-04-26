using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Azure.AI.OpenAI;

public class TextGenerationToolTests
{
    private readonly Mock<ILogger<TextGenerationTool>> _loggerMock;
    private readonly Mock<OpenAIClient> _openAIClientMock;
    private readonly TextGenerationTool _textGenerationTool;

    public TextGenerationToolTests()
    {
        _loggerMock = new Mock<ILogger<TextGenerationTool>>();
        _openAIClientMock = new Mock<OpenAIClient>();
        _textGenerationTool = new TextGenerationTool(_openAIClientMock.Object, "testDeployment", _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidPrompt_ReturnsGeneratedText()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "prompt", "test prompt" }
        };

        var chatCompletions = new ChatCompletions
        {
            Choices = new List<ChatChoice>
            {
                new ChatChoice
                {
                    Message = new ChatMessage
                    {
                        Content = "Generated text"
                    }
                }
            }
        };

        _openAIClientMock
            .Setup(client => client.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>()))
            .ReturnsAsync(Response.FromValue(chatCompletions, new MockResponse(200)));

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

        var chatCompletions = new ChatCompletions
        {
            Choices = new List<ChatChoice>
            {
                new ChatChoice
                {
                    Message = new ChatMessage
                    {
                        Content = "Generated text"
                    }
                }
            }
        };

        _openAIClientMock
            .Setup(client => client.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>()))
            .ReturnsAsync(Response.FromValue(chatCompletions, new MockResponse(200)));

        // Act
        await _textGenerationTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Text generation executed successfully for prompt: test prompt")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
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

        _openAIClientMock
            .Setup(client => client.GetChatCompletionsAsync(It.IsAny<ChatCompletionsOptions>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _textGenerationTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing text generation for prompt: test prompt")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
