using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CognitiveMesh.AgencyLayer.ToolIntegration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class ClassificationToolTests
{
    private readonly Mock<ILogger<ClassificationTool>> _loggerMock;
    private readonly ClassificationTool _classificationTool;

    public ClassificationToolTests()
    {
        _loggerMock = new Mock<ILogger<ClassificationTool>>();
        _classificationTool = new ClassificationTool(_loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidData_ReturnsResults()
    {
        try
        {
            Console.WriteLine("Starting test: ExecuteAsync_ValidData_ReturnsResults");
            // Arrange
            var parameters = new Dictionary<string, object>
            {
                { "data", "sample data" }
            };

            Console.WriteLine("Calling ExecuteAsync with parameters");
            // Act
            var result = await _classificationTool.ExecuteAsync(parameters);
            Console.WriteLine($"ExecuteAsync completed with result: {result}");

            // Assert
            Assert.Contains("Classification results", result);
            Console.WriteLine("Test completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Test failed with exception: {ex}");
            throw;
        }
    }

    [Fact]
    public async Task ExecuteAsync_MissingDataParameter_ThrowsException()
    {
        // Arrange
        var parameters = new Dictionary<string, object>();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _classificationTool.ExecuteAsync(parameters));
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
        var exception = await Assert.ThrowsAsync<Exception>(() => _classificationTool.ExecuteAsync(parameters));
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
        await _classificationTool.ExecuteAsync(parameters);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Classification executed successfully for data: sample data")),
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

        _classificationTool = new ClassificationTool(_loggerMock.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => _classificationTool.ExecuteAsync(parameters));

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error executing classification for data: sample data")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
