using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class DataAnalysisToolTests
{
    private readonly Mock<ILogger<DataAnalysisTool>> _loggerMock;
    private readonly DataAnalysisTool _dataAnalysisTool;

    public DataAnalysisToolTests()
    {
        _loggerMock = new Mock<ILogger<DataAnalysisTool>>();
        _dataAnalysisTool = new DataAnalysisTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidParameters_ReturnsResults()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" },
            { "analysisType", "sample analysis" }
        };

        // Act
        var result = await _dataAnalysisTool.ExecuteAsync(parameters);

        // Assert
        Assert.Contains("Analysis results (sample analysis)", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingDataParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "analysisType", "sample analysis" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _dataAnalysisTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'data' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_MissingAnalysisTypeParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" }
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _dataAnalysisTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'analysisType' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "sample data" },
            { "analysisType", "sample analysis" }
        };

        // Act
        await _dataAnalysisTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Data analysis executed successfully for analysis type: sample analysis")),
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
            { "data", "sample data" },
            { "analysisType", "sample analysis" }
        };

        _dataAnalysisTool = new DataAnalysisTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _dataAnalysisTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing data analysis for analysis type: sample analysis")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
