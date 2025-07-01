using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.ToolIntegration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class WebSearchToolTests
{
    private readonly Mock<ILogger<WebSearchTool>> _loggerMock;
    private readonly WebSearchTool _webSearchTool;

    public WebSearchToolTests()
    {
        _loggerMock = new Mock<ILogger<WebSearchTool>>();
        _webSearchTool = new WebSearchTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidQuery_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "query", "test query" }
        };

        // Act
        var result = await _webSearchTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("Search results for 'test query'", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingQuery_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _webSearchTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'query' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidQueryType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "query", 123 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _webSearchTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'query' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "query", "test query" }
        };

        // Act
        await _webSearchTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web search executed successfully for query: test query")),
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
            { "query", "test query" }
        };

        _webSearchTool = new WebSearchTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _webSearchTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing web search for query: test query")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
