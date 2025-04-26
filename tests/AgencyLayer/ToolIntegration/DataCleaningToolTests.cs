using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class DataCleaningToolTests
{
    private readonly Mock<ILogger<DataCleaningTool>> _loggerMock;
    private readonly DataCleaningTool _dataCleaningTool;

    public DataCleaningToolTests()
    {
        _loggerMock = new Mock<ILogger<DataCleaningTool>>();
        _dataCleaningTool = new DataCleaningTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_ReturnsCleanedData()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "  sample data  " }
        };

        // Act
        var result = await _dataCleaningTool.ExecuteAsync(parameters);

        // Assert
        Assert.Equal("sample data", result);
    }

    [Fact]
    public async Task ExecuteAsync_MissingDataParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _dataCleaningTool.ExecuteAsync(parameters));
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
        var exception = await Assert.ThrowsAsync<Exception>(() => _dataCleaningTool.ExecuteAsync(parameters));
        Assert.Equal("Missing or invalid 'data' parameter", exception.Message);
    }

    [Fact]
    public async Task ExecuteAsync_LogsInformation()
    {
        // Arrange
        var parameters = new Dictionary<string, object>
        {
            { "data", "  sample data  " }
        };

        // Act
        await _dataCleaningTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Data cleaning executed successfully for data:   sample data  ")),
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
            { "data", "  sample data  " }
        };

        _dataCleaningTool = new DataCleaningTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _dataCleaningTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing data cleaning for data:   sample data  ")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
