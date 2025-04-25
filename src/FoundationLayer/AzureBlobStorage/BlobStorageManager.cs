using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

public class BlobStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageManager> _logger;

    public BlobStorageManager(string connectionString, ILogger<BlobStorageManager> logger)
    {
        _blobServiceClient = new BlobServiceClient(connectionString);
        _logger = logger;
    }

    public async Task<bool> UploadBlobAsync(string containerName, string blobName, Stream content)
    {
        try
        {
            _logger.LogInformation($"Uploading blob: {blobName} to container: {containerName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(content, overwrite: true);

            _logger.LogInformation($"Successfully uploaded blob: {blobName} to container: {containerName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to upload blob: {blobName} to container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        try
        {
            _logger.LogInformation($"Downloading blob: {blobName} from container: {containerName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadAsync();

            _logger.LogInformation($"Successfully downloaded blob: {blobName} from container: {containerName}");
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to download blob: {blobName} from container: {containerName}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
    {
        try
        {
            _logger.LogInformation($"Deleting blob: {blobName} from container: {containerName}");

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            await blobClient.DeleteIfExistsAsync();

            _logger.LogInformation($"Successfully deleted blob: {blobName} from container: {containerName}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete blob: {blobName} from container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }
}
