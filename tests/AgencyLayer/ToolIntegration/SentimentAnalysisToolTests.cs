using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SentimentAnalysisToolTests
{
    private readonly Mock<ILogger<SentimentAnalysisTool>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly SentimentAnalysisTool _sentimentAnalysisTool;

    public SentimentAnalysisToolTests()
    {
        _loggerMock = new Mock<ILogger<SentimentAnalysisTool>>();
        _httpClientMock = new Mock<HttpClient>();
        _sentimentAnalysisTool = new SentimentAnalysisTool(_loggerMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidText_ReturnsSentiment()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "text", "This is a test text." }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"sentiment\":\"positive\"}")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _sentimentAnalysisTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("positive", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingText_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sentimentAnalysisTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'text' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidTextType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "text", 123 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _sentimentAnalysisTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'text' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "text", "This is a test text." }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"sentiment\":\"positive\"}")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        await _sentimentAnalysisTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Sentiment analysis executed successfully for text: This is a test text.")),
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
            { "text", "This is a test text." }
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _sentimentAnalysisTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing sentiment analysis for text: This is a test text.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
