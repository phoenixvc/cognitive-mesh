using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class DataVisualizationToolTests
{
    private readonly Mock<ILogger<DataVisualizationTool>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly DataVisualizationTool _dataVisualizationTool;

    public DataVisualizationToolTests()
    {
        _loggerMock = new Mock<ILogger<DataVisualizationTool>>();
        _httpClientMock = new Mock<HttpClient>();
        _dataVisualizationTool = new DataVisualizationTool(_loggerMock.Object, _httpClientMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidParameters_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" }
        };

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("Visualization result")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _dataVisualizationTool.ExecuteAsync(parameters);

        // Assert
        Assert.Equal("Visualization result", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingDataParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _dataVisualizationTool.ExecuteAsync(parameters));
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
            Content = new StringContent("Visualization result")
        };

        _httpClientMock
            .Setup(client => client.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>()))
            .ReturnsAsync(responseMessage);

        // Act
        await _dataVisualizationTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Data visualization executed successfully for data: sample data")),
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
        await Assert.ThrowsAsync<Exception>(() => _dataVisualizationTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing data visualization for data: sample data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
