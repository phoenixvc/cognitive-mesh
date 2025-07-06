using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.DataLake;
using Microsoft.Extensions.Logging;
using CognitiveMesh.FoundationLayer.EnterpriseConnectors;

namespace CognitiveMesh.FoundationLayer.OneLakeIntegration;

public class OneLakeIntegrationManager
{
    private readonly DataLakeServiceClient _dataLakeServiceClient;
    private readonly ILogger<OneLakeIntegrationManager> _logger;
    private readonly FeatureFlagManager _featureFlagManager;

    public OneLakeIntegrationManager(string connectionString, ILogger<OneLakeIntegrationManager> logger, FeatureFlagManager featureFlagManager)
    {
        _dataLakeServiceClient = new DataLakeServiceClient(connectionString);
        _logger = logger;
        _featureFlagManager = featureFlagManager;
    }

    public async Task<bool> UploadFileAsync(string fileSystemName, string filePath, Stream content)
    {
        try
        {
            if (_featureFlagManager.EnableADK)
            {
                _logger.LogInformation($"Uploading file: {filePath} to OneLake file system: {fileSystemName}");

                var fileSystemClient = _dataLakeServiceClient.GetFileSystemClient(fileSystemName);
                var fileClient = fileSystemClient.GetFileClient(filePath);

                await fileClient.UploadAsync(content, overwrite: true);

                _logger.LogInformation($"Successfully uploaded file: {filePath} to OneLake file system: {fileSystemName}");
                return true;
            }
            else
            {
                _logger.LogInformation("Feature not enabled.");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to upload file: {filePath} to file system: {fileSystemName}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileSystemName, string filePath)
    {
        try
        {
            if (_featureFlagManager.EnableLangGraph)
            {
                _logger.LogInformation($"Downloading file: {filePath} from OneLake file system: {fileSystemName}");

                var fileSystemClient = _dataLakeServiceClient.GetFileSystemClient(fileSystemName);
                var fileClient = fileSystemClient.GetFileClient(filePath);

                var response = await fileClient.ReadAsync();

                _logger.LogInformation($"Successfully downloaded file: {filePath} from OneLake file system: {fileSystemName}");
                return response.Value.Content;
            }
            else
            {
                _logger.LogInformation("Feature not enabled.");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to download file: {filePath} from file system: {fileSystemName}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileSystemName, string filePath)
    {
        try
        {
            if (_featureFlagManager.EnableCrewAI)
            {
                _logger.LogInformation($"Deleting file: {filePath} from OneLake file system: {fileSystemName}");

                var fileSystemClient = _dataLakeServiceClient.GetFileSystemClient(fileSystemName);
                var fileClient = fileSystemClient.GetFileClient(filePath);

                var response = await fileClient.DeleteIfExistsAsync();

                _logger.LogInformation($"Successfully deleted file: {filePath} from OneLake file system: {fileSystemName}");
                return response.Value;
            }
            else
            {
                _logger.LogInformation("Feature not enabled.");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete file: {filePath} from file system: {fileSystemName}. Error: {ex.Message}");
            return false;
        }
    }
}