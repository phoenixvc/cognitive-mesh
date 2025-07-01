using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.ToolIntegration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class NamedEntityRecognitionToolTests
{
    private readonly Mock<ILogger<NamedEntityRecognitionTool>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly NamedEntityRecognitionTool _namedEntityRecognitionTool;

    public NamedEntityRecognitionToolTests()
    {
        _loggerMock = new Mock<ILogger<NamedEntityRecognitionTool>>();
        _httpClientMock = new Mock<HttpClient>();
        _namedEntityRecognitionTool = new NamedEntityRecognitionTool(_loggerMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidText_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "text", "test text" }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"entities\": [\"Entity1\", \"Entity2\"]}", Encoding.UTF8, "application/json")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _namedEntityRecognitionTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("Entity1", result);
        Assert.Contains("Entity2", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingText_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _namedEntityRecognitionTool.ExecuteAsync(parameters));
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
        var exception = await Assert.ThrowsAsync<Exception>(() => _namedEntityRecognitionTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'text' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "text", "test text" }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("{\"entities\": [\"Entity1\", \"Entity2\"]}", Encoding.UTF8, "application/json")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        await _namedEntityRecognitionTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Named entity recognition executed successfully for text: test text")),
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
            { "text", "test text" }
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _namedEntityRecognitionTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing named entity recognition for text: test text")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
