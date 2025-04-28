using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OneLake.SDK;

public class OneLakeIntegrationManager
{
    private readonly OneLakeClient _oneLakeClient;
    private readonly ILogger<OneLakeIntegrationManager> _logger;

    public OneLakeIntegrationManager(string oneLakeConnectionString, ILogger<OneLakeIntegrationManager> logger)
    {
        _oneLakeClient = new OneLakeClient(oneLakeConnectionString);
        _logger = logger;
    }

    public async Task<bool> UploadFileAsync(string containerName, string fileName, Stream content)
    {
        try
        {
            _logger.LogInformation($"Uploading file: {fileName} to OneLake container: {containerName}");

            var containerClient = _oneLakeClient.GetContainerClient(containerName);
            var fileClient = containerClient.GetFileClient(fileName);

            await fileClient.UploadAsync(content, overwrite: true);

            _logger.LogInformation($"Successfully uploaded file: {fileName} to OneLake container: {containerName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to upload file: {fileName} to container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<Stream> DownloadFileAsync(string containerName, string fileName)
    {
        try
        {
            _logger.LogInformation($"Downloading file: {fileName} from OneLake container: {containerName}");

            var containerClient = _oneLakeClient.GetContainerClient(containerName);
            var fileClient = containerClient.GetFileClient(fileName);

            var response = await fileClient.DownloadAsync();

            _logger.LogInformation($"Successfully downloaded file: {fileName} from OneLake container: {containerName}");
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to download file: {fileName} from container: {containerName}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string containerName, string fileName)
    {
        try
        {
            _logger.LogInformation($"Deleting file: {fileName} from OneLake container: {containerName}");

            var containerClient = _oneLakeClient.GetContainerClient(containerName);
            var fileClient = containerClient.GetFileClient(fileName);

            await fileClient.DeleteIfExistsAsync();

            _logger.LogInformation($"Successfully deleted file: {fileName} from OneLake container: {containerName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete file: {fileName} from container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }
}
