using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class PredictiveAnalyticsToolTests
{
    private readonly Mock<ILogger<PredictiveAnalyticsTool>> _loggerMock;
    private readonly PredictiveAnalyticsTool _predictiveAnalyticsTool;

    public PredictiveAnalyticsToolTests()
    {
        _loggerMock = new Mock<ILogger<PredictiveAnalyticsTool>>();
        _predictiveAnalyticsTool = new PredictiveAnalyticsTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" }
        };

        // Act
        var result = await _predictiveAnalyticsTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("Predictive analytics results", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingDataParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _predictiveAnalyticsTool.ExecuteAsync(parameters));
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

        // Act
        await _predictiveAnalyticsTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Predictive analytics executed successfully for data: sample data")),
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

        _predictiveAnalyticsTool = new PredictiveAnalyticsTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _predictiveAnalyticsTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing predictive analytics for data: sample data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
