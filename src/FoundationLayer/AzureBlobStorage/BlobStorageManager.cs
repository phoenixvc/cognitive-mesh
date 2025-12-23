namespace FoundationLayer.AzureBlobStorage
{
    /// <summary>
    /// Adapter for Azure Blob Storage operations.
    /// </summary>
    public class BlobStorageManager : IDisposable
    {
        private readonly IBlobStorageAdapter _adapter;
        private bool _disposed = false;

        public BlobStorageManager(IBlobStorageAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        /// <summary>
        /// Uploads binary data to a blob.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to create or overwrite.</param>
        /// <param name="data">The binary data to upload.</param>
        /// <param name="contentType">Optional content type of the blob.</param>
        /// <param name="metadata">Optional metadata for the blob.</param>
        public async Task UploadBlobAsync(
            string containerName, 
            string blobName, 
            byte[] data, 
            string contentType = null,
            System.Collections.Generic.IDictionary<string, string> metadata = null)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or whitespace.", nameof(containerName));
                
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name cannot be null or whitespace.", nameof(blobName));
                
            if (data == null || data.Length == 0)
                throw new ArgumentException("Data cannot be null or empty.", nameof(data));

            try
            {
                var containerClient = _adapter.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                using var stream = new MemoryStream(data);
                var blobHttpHeaders = new BlobHttpHeaders 
                { 
                    ContentType = contentType ?? "application/octet-stream" 
                };
                
                var options = new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders
                };
                
                if (metadata != null)
                {
                    options.Metadata = metadata;
                }
                
                await blobClient.UploadAsync(stream, options);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to upload blob '{blobName}' to container '{containerName}'", ex);
            }
        }

        /// <summary>
        /// Downloads a blob as a byte array.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to download.</param>
        /// <returns>The blob content as a byte array.</returns>
        public async Task<byte[]> DownloadBlobAsync(string containerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or whitespace.", nameof(containerName));
                
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name cannot be null or whitespace.", nameof(blobName));

            try
            {
                var containerClient = _adapter.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                
                using var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                return stream.ToArray();
            }
            catch (Azure.RequestFailedException ex) when (ex.Status == 404)
            {
                return null; // Return null if blob not found
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to download blob '{blobName}' from container '{containerName}'", ex);
            }
        }

        /// <summary>
        /// Deletes a blob from the container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <returns>True if the blob was deleted; false if it didn't exist.</returns>
        public async Task<bool> DeleteBlobAsync(string containerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or whitespace.", nameof(containerName));
                
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name cannot be null or whitespace.", nameof(blobName));

            try
            {
                var containerClient = _adapter.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete blob '{blobName}' from container '{containerName}'", ex);
            }
        }

        /// <summary>
        /// Checks if a blob exists in the container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to check.</param>
        /// <returns>True if the blob exists; otherwise, false.</returns>
        public async Task<bool> BlobExistsAsync(string containerName, string blobName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentException("Container name cannot be null or whitespace.", nameof(containerName));
                
            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name cannot be null or whitespace.", nameof(blobName));

            try
            {
                var containerClient = _adapter.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);
                var response = await blobClient.ExistsAsync();
                return response.Value;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to check if blob '{blobName}' exists in container '{containerName}'", ex);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                (_adapter as IDisposable)?.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
