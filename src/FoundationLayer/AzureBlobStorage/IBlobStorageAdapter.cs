namespace FoundationLayer.AzureBlobStorage
{
    /// <summary>
    /// Defines the contract for blob storage operations.
    /// </summary>
    public interface IBlobStorageAdapter : IDisposable
    {
        /// <summary>
        /// Gets a BlobContainerClient for the specified container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <returns>A BlobContainerClient instance.</returns>
        BlobContainerClient GetBlobContainerClient(string containerName);

        /// <summary>
        /// Creates a new container if it doesn't exist.
        /// </summary>
        /// <param name="containerName">The name of the container to create.</param>
        /// <param name="publicAccessType">The level of public access for the container.</param>
        /// <returns>True if the container was created, false if it already exists.</returns>
        Task<bool> CreateContainerIfNotExistsAsync(
            string containerName, 
            Azure.Storage.Blobs.Models.PublicAccessType publicAccessType = default);

        /// <summary>
        /// Uploads a stream to a blob.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to create or overwrite.</param>
        /// <param name="content">The stream containing the content to upload.</param>
        /// <param name="contentType">The content type of the blob.</param>
        /// <param name="metadata">Optional metadata for the blob.</param>
        /// <returns>The BlobContentInfo for the uploaded blob.</returns>
        Task<Azure.Storage.Blobs.Models.BlobContentInfo> UploadBlobAsync(
            string containerName,
            string blobName,
            Stream content,
            string contentType = null,
            IDictionary<string, string> metadata = null);

        /// <summary>
        /// Downloads a blob to a stream.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to download.</param>
        /// <param name="destination">The stream to write the downloaded content to.</param>
        /// <returns>A Response containing the blob properties and content.</returns>
        Task<Azure.Response> DownloadBlobToAsync(
            string containerName,
            string blobName,
            Stream destination);

        /// <summary>
        /// Deletes a blob if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to delete.</param>
        /// <returns>True if the blob was deleted, false if it didn't exist.</returns>
        Task<bool> DeleteBlobIfExistsAsync(
            string containerName,
            string blobName);

        /// <summary>
        /// Checks if a blob exists.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="blobName">The name of the blob to check.</param>
        /// <returns>True if the blob exists, false otherwise.</returns>
        Task<bool> BlobExistsAsync(
            string containerName,
            string blobName);

        /// <summary>
        /// Lists all blobs in a container.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="prefix">Optional prefix to filter blobs.</param>
        /// <returns>An async enumerable of blob items.</returns>
        IAsyncEnumerable<Azure.Storage.Blobs.Models.BlobItem> ListBlobsAsync(
            string containerName,
            string prefix = null);
    }
}
