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

public class RecommendationSystemToolTests
{
    private readonly Mock<ILogger<RecommendationSystemTool>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly RecommendationSystemTool _recommendationSystemTool;

    public RecommendationSystemToolTests()
    {
        _loggerMock = new Mock<ILogger<RecommendationSystemTool>>();
        _httpClientMock = new Mock<HttpClient>();
        _recommendationSystemTool = new RecommendationSystemTool(_loggerMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("Recommendation results")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _recommendationSystemTool.ExecuteAsync(parameters);

        // Assert
        Assert.Equal("Recommendation results", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingData_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _recommendationSystemTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'data' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_InvalidDataType_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", 123 }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _recommendationSystemTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'data' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("Recommendation results")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        await _recommendationSystemTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recommendation system executed successfully for data: sample data")),
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
            { "data", "sample data" }
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        await Assert.ThrowsAsync<Exception>(() => _recommendationSystemTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing recommendation system for data: sample data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
