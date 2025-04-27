using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using OneLake.SDK;

public class BlobStorageManager
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly OneLakeClient _oneLakeClient;
    private readonly ILogger<BlobStorageManager> _logger;

    public BlobStorageManager(string azureConnectionString, string oneLakeConnectionString, ILogger<BlobStorageManager> logger)
    {
        _blobServiceClient = new BlobServiceClient(azureConnectionString);
        _oneLakeClient = new OneLakeClient(oneLakeConnectionString);
        _logger = logger;
    }

    public async Task<bool> UploadBlobAsync(string containerName, string blobName, Stream content, bool useOneLake = false)
    {
        try
        {
            if (useOneLake)
            {
                _logger.LogInformation($"Uploading blob: {blobName} to OneLake container: {containerName}");

                var containerClient = _oneLakeClient.GetContainerClient(containerName);
                var blobClient = containerClient.GetFileClient(blobName);

                await blobClient.UploadAsync(content, overwrite: true);

                _logger.LogInformation($"Successfully uploaded blob: {blobName} to OneLake container: {containerName}");
            }
            else
            {
                _logger.LogInformation($"Uploading blob: {blobName} to Azure container: {containerName}");

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(content, overwrite: true);

                _logger.LogInformation($"Successfully uploaded blob: {blobName} to Azure container: {containerName}");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to upload blob: {blobName} to container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, bool useOneLake = false)
    {
        try
        {
            if (useOneLake)
            {
                _logger.LogInformation($"Downloading blob: {blobName} from OneLake container: {containerName}");

                var containerClient = _oneLakeClient.GetContainerClient(containerName);
                var blobClient = containerClient.GetFileClient(blobName);

                var response = await blobClient.DownloadAsync();

                _logger.LogInformation($"Successfully downloaded blob: {blobName} from OneLake container: {containerName}");
                return response.Value.Content;
            }
            else
            {
                _logger.LogInformation($"Downloading blob: {blobName} from Azure container: {containerName}");

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                var response = await blobClient.DownloadAsync();

                _logger.LogInformation($"Successfully downloaded blob: {blobName} from Azure container: {containerName}");
                return response.Value.Content;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to download blob: {blobName} from container: {containerName}. Error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteBlobAsync(string containerName, string blobName, bool useOneLake = false)
    {
        try
        {
            if (useOneLake)
            {
                _logger.LogInformation($"Deleting blob: {blobName} from OneLake container: {containerName}");

                var containerClient = _oneLakeClient.GetContainerClient(containerName);
                var blobClient = containerClient.GetFileClient(blobName);

                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation($"Successfully deleted blob: {blobName} from OneLake container: {containerName}");
            }
            else
            {
                _logger.LogInformation($"Deleting blob: {blobName} from Azure container: {containerName}");

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation($"Successfully deleted blob: {blobName} from Azure container: {containerName}");
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete blob: {blobName} from container: {containerName}. Error: {ex.Message}");
            return false;
        }
    }
}
