using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.ToolIntegration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class WebScrapingToolTests
{
    private readonly Mock<ILogger<WebScrapingTool>> _loggerMock;
    private readonly WebScrapingTool _webScrapingTool;

    public WebScrapingToolTests()
    {
        _loggerMock = new Mock<ILogger<WebScrapingTool>>();
        _webScrapingTool = new WebScrapingTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidUrl_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "url", "https://example.com" }
        };

        // Act
        var result = await _webScrapingTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("Scraped content from 'https://example.com'", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingUrl_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _webScrapingTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'url' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidUrlType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "url", 123 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _webScrapingTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'url' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "url", "https://example.com" }
        };

        // Act
        await _webScrapingTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Web scraping executed successfully for URL: https://example.com")),
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
            { "url", "https://example.com" }
        };

        _webScrapingTool = new WebScrapingTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _webScrapingTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing web scraping for URL: https://example.com")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
